using Hackathon_2025_ESG.Areas.Client.Models;
using Hackathon_2025_ESG.Controllers;
using Hackathon_2025_ESG.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Hackathon_2025_ESG.Areas.Client.Controllers
{
    [Area("Client")]
    [Route("GenerateReport/Upload/[action]")]
    public class UploadDocsController : Controller
    {
        private readonly ILogger<UploadDocsController> _logger;
        private readonly IAwsS3UploaderService _s3Uploader;
        private readonly IConfiguration _configuration;

        public UploadDocsController(
            ILogger<UploadDocsController> logger,
            IAwsS3UploaderService s3Uploader,
            IConfiguration configuration)
        {
            _logger = logger;
            _s3Uploader = s3Uploader;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var model = new EsgAnalysisViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateAnalysis(EsgAnalysisViewModel model)
        {
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

            if (!ModelState.IsValid)
            {
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

            // Create a single, unique timestamp for this entire report submission
            var reportTimestamp = DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss");

            // 3. --- Upload Files by Category ---
            int totalSuccessCount = 0;

            // Use a helper method to upload files for each category to its specific folder
            totalSuccessCount += await UploadCategoryFilesAsync(model.EnvironmentalFiles, bucketName, userId, reportTimestamp, "Environmental-Docs");
            totalSuccessCount += await UploadCategoryFilesAsync(model.SocialFiles, bucketName, userId, reportTimestamp, "Social-Docs");
            totalSuccessCount += await UploadCategoryFilesAsync(model.GovernanceFiles, bucketName, userId, reportTimestamp, "Governance-Docs");

            _logger.LogInformation("User '{UserId}' successfully uploaded {SuccessCount} of {TotalFiles} files for report '{ReportTimestamp}'.", userId, totalSuccessCount, allFiles.Count(), reportTimestamp);

            if (totalSuccessCount == 0 && allFiles.Any())
            {
                TempData["Error"] = "An error occurred, and none of your files could be uploaded. Please try again.";
                return View("Index", model);
            }

            // 4. --- Redirect to a results or processing page ---
            //TempData["SuccessMessage"] = $"Successfully uploaded {totalSuccessCount} of {allFiles.Count()} files. Your analysis will begin shortly.";
            return RedirectToAction("Index");
        }

        private async Task<int> UploadCategoryFilesAsync(List<IFormFile> files, string bucketName, string userId, string timestamp, string categoryFolder)
        {
            if (files == null || !files.Any())
            {
                return 0; // No files in this category, so 0 were uploaded.
            }

            int successCount = 0;
            _logger.LogInformation("Starting upload of {FileCount} files to '{CategoryFolder}' for user '{UserId}'.", files.Count, categoryFolder, userId);

            foreach (var file in files)
            {
                if (file == null || file.Length == 0) continue;

                try
                {
                    // Construct the full S3 key according to the required structure
                    // Example: user-id-123/2025-09-20-11-22-33/Raw-Docs/Environmental-Docs/guid-original-filename.pdf
                    string s3Key = $"{userId}/{timestamp}/Raw-Docs/{categoryFolder}/{Guid.NewGuid()}-{file.FileName}";
                    //string s3Key = $"{Guid.NewGuid()}-{file.FileName}";

                    using (var stream = file.OpenReadStream())
                    {
                        var uploadSuccess = await _s3Uploader.UploadFileAsync(bucketName, s3Key, stream, file.ContentType);
                        if (uploadSuccess)
                        {
                            successCount++;
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
    }
}
