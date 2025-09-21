using Amazon;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Hackathon_2025_ESG.Areas.Client.Models;
using Hackathon_2025_ESG.Controllers;
using Hackathon_2025_ESG.Data;
using Hackathon_2025_ESG.Models;
using Hackathon_2025_ESG.Services;
using Hackathon_2025_ESG.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using NuGet.Packaging.Signing;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Hackathon_2025_ESG.Areas.Client.Controllers
{
    [Area("Client")]
    [Route("GenerateReport/Upload/[action]")]
    public class UploadDocsController : Controller
    {
        private readonly ILogger<UploadDocsController> _logger;
        private readonly IAwsS3UploaderService _s3Uploader;
        private readonly IConfiguration _configuration;
        private readonly Hackathon_2025_ESGContext _context;
        private readonly IBedrockService _bedrock;

        public UploadDocsController(
            ILogger<UploadDocsController> logger,
            IAwsS3UploaderService s3Uploader,
            IConfiguration configuration,
            Hackathon_2025_ESGContext context,
            IBedrockService bedrock)
        {
            _logger = logger;
            _s3Uploader = s3Uploader;
            _configuration = configuration;
            _context = context;
            _bedrock = bedrock;

        }

        [HttpGet]
        public IActionResult Index(string standard)
        {
            var model = new EsgAnalysisViewModel();

            model.SelectedStandard = standard;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateAnalysis(EsgAnalysisViewModel model)
        {
            Console.WriteLine("GenerateAnalysis called.");

            // 1. --- Initial Validation ---
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "User session expired. Please log in again.";
                return Redirect("~/Identity/Account/Login");
            }

            var allFiles = (model.EnvironmentalFiles ?? new List<IFormFile>())
                           .Concat(model.SocialFiles ?? new List<IFormFile>())
                           .Concat(model.GovernanceFiles ?? new List<IFormFile>());

            if (!allFiles.Any())
            {
                ModelState.AddModelError("", "Please upload at least one document to generate an analysis.");
                return View("Index", model); // Return to the upload view with an error
            }
            Console.WriteLine("Got Files.");

            if (!ModelState.IsValid)
            {
                // Find all validation errors in the ModelState
                var validationErrors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                // Log the collected errors for debugging
                _logger.LogWarning("Model validation failed. Errors: {@ValidationErrors}", validationErrors);

                return View("Index", model);
            }

            // 2. --- S3 Configuration and Path Setup ---
            var bucketName = _configuration["S3Buckets:EsgDocuments"];
            if (string.IsNullOrEmpty(bucketName))
            {
                _logger.LogCritical("S3 bucket name 'EsgDocuments' is not configured in appsettings.json.");
                // In a real app, you'd show a user-friendly error page.
                return StatusCode(500, "Server configuration error: The storage destination is not set up.");
            }
            Console.WriteLine("Retrieved Bucket Name.");

            // Create a single, unique timestamp for this entire report submission
            var reportTimestamp = DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss");

            // 3. --- Upload Files by Category ---
            int totalSuccessCount = 0;

            // Use a helper method to upload files for each category to its specific folder
            totalSuccessCount += await UploadCategoryFilesAsync(model.EnvironmentalFiles, bucketName, userId, reportTimestamp, "Environmental-Docs");
            totalSuccessCount += await UploadCategoryFilesAsync(model.SocialFiles, bucketName, userId, reportTimestamp, "Social-Docs");
            totalSuccessCount += await UploadCategoryFilesAsync(model.GovernanceFiles, bucketName, userId, reportTimestamp, "Governance-Docs");

            _logger.LogInformation("User '{UserId}' successfully uploaded {SuccessCount} of {TotalFiles} files for report '{ReportTimestamp}'.", userId, totalSuccessCount, allFiles.Count(), reportTimestamp);

            string AiReportFileName = reportTimestamp + "-AI-ESG-Report.pdf";
            string AiReportS3Directory = $"{userId}/{reportTimestamp}/AI-Report";
            string AiReportS3Key = $"{AiReportS3Directory}/{AiReportFileName}";

            await SaveAiReportRecordAsync(userId, AiReportFileName, AiReportS3Key, AiReportS3Directory);

            _logger.LogInformation("User '{UserId}' successfully uploaded {AiReportFileName} files for AI Report '{ReportTimestamp}'.", userId, AiReportFileName, reportTimestamp);

            if (totalSuccessCount == 0 && allFiles.Any())
            {
                TempData["Error"] = "An error occurred, and none of your files could be uploaded. Please try again.";
                return View("Index", model);
            }



            // 4. --- Redirect to a results or processing page ---
            //TempData["SuccessMessage"] = $"Successfully uploaded {totalSuccessCount} of {allFiles.Count()} files. Your analysis will begin shortly.";
            return RedirectToAction("Index", "Home", new { area = "Client" });
        }

        private async Task<int> UploadCategoryFilesAsync(List<IFormFile> files, string bucketName, string userId, string timestamp, string categoryFolder)
        {
            if (files == null || !files.Any())
            {
                return 0; // No files in this category, so 0 were uploaded.
            }

            int successCount = 0;
            _logger.LogInformation("Starting upload of {FileCount} files to '{CategoryFolder}' for user '{UserId}'.", files.Count, categoryFolder, userId);

            string s3Directory = $"{userId}/{timestamp}/Raw-Docs/{categoryFolder}";

            foreach (var file in files)
            {
                if (file == null || file.Length == 0) continue;

                try
                {
                    // Construct the full S3 key according to the required structure
                    // Example: user-id-123/2025-09-20-11-22-33/Raw-Docs/Environmental-Docs/guid-original-filename.pdf
                    string s3Key = $"{s3Directory}/{Guid.NewGuid()}-{file.FileName}";
                    //string s3Key = $"{Guid.NewGuid()}-{file.FileName}";

                    using (var stream = file.OpenReadStream())
                    {
                        var uploadSuccess = await _s3Uploader.UploadFileAsync(bucketName, s3Key, stream, file.ContentType);
                        if (uploadSuccess)
                        {
                            successCount++;

                            await SaveRawDocRecordAsync(userId, file, s3Key, s3Directory);
                        }
                        else
                        {
                            _logger.LogWarning("Failed to upload file '{FileName}' to S3 key '{S3Key}'.", file.FileName, s3Key);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An exception occurred while uploading file '{FileName}' for user '{UserId}'.", file.FileName, userId);
                }
            }



            return successCount;
        }

        private async Task SaveRawDocRecordAsync(string userId, IFormFile file, string s3Key, string s3Directory)
        {
            try
            {
                var newDocRecord = new EsgRawDocs
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    FileName = file.FileName,
                    S3DirectoryPath = s3Directory,
                    S3FilePath = s3Key,
                    CreatedAt = DateTime.UtcNow
                };

                _context.EsgRawDoc.Add(newDocRecord);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully saved database record for file: {FileName}", file.FileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save database record for uploaded file: {FileName}", file.FileName);
            }
        }

        private async Task SaveAiReportRecordAsync(string userId, string fileName, string s3Key, string s3Directory)
        {
            try
            {
                var newDocRecord = new EsgReport
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    FileName = fileName,
                    S3FilePath = s3Key,
                    S3RawDocsDirectoryPath = s3Directory,
                    CreatedAt = DateTime.UtcNow
                };

                _context.EsgReport.Add(newDocRecord);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully saved database record for file: {FileName}", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save database record for uploaded file: {FileName}", fileName);
            }
        }

        private async Task AccessBedrock()
        {
            try
            {
                var prompt = BedrockPayloadBuilder.BuildReportPayload(
                    "Sample Company",
                    "{}",
                    "{}",
                    new List<(string file, string excerpt)> { ("doc1.pdf", "Excerpt from document 1"), ("doc2.pdf", "Excerpt from document 2") },
                    "2023"
                );
                _logger.LogError("Prompt: {Prompt}", prompt);


                var response = await _bedrock.InvokeAsync(prompt);
                _logger.LogInformation("Bedrock Response: {response}", response);

                // process the response to become pdf report
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accessing Bedrock service.");
            }
        }

        // private async Task GenerateAndUploadPdfAsync(EsgReportResponse reportData, string bucketName, string s3Key)
        // {
        //     try
        //     {
        //         // 1. Generate PDF
        //         var document = new EsgReportPdfDocument(reportData);

        //         using var pdfStream = new MemoryStream();
        //         document.GeneratePdf(pdfStream);

        //         // Reset stream position before uploading
        //         pdfStream.Position = 0;

        //         // 2. Upload to S3
        //         var uploadSuccess = await _s3Uploader.UploadFileAsync(bucketName, s3Key, pdfStream, "application/pdf");
        //         if (!uploadSuccess)
        //         {
        //             _logger.LogError("Failed to upload generated ESG PDF to S3. Key: {S3Key}", s3Key);
        //             throw new Exception("PDF upload failed.");
        //         }

        //         _logger.LogInformation("Successfully uploaded ESG report PDF to S3. Key: {S3Key}", s3Key);
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Error generating or uploading ESG report PDF.");
        //         throw;
        //     }
        // }
    }
}
