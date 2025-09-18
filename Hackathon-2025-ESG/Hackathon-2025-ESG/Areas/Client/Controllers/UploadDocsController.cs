using Hackathon_2025_ESG.Areas.Client.Models;
using Hackathon_2025_ESG.Controllers;
using Hackathon_2025_ESG.Services.Interface;
using Microsoft.AspNetCore.Mvc;

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

        //[HttpPost]
        //public async Task<IActionResult> GenerateAnalysis(EsgAnalysisViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View("Index", model);
        //    }

        //    var allFiles = model.EnvironmentalFiles
        //                        .Concat(model.SocialFiles)
        //                        .Concat(model.GovernanceFiles);

        //    if (!allFiles.Any())
        //    {
        //        ModelState.AddModelError("", "Please upload at least one document.");
        //        return View("Index", model);
        //    }

        //    // Get the bucket name from appsettings.json
        //    var bucketName = _configuration["S3Buckets:EsgDocuments"];
        //    if (string.IsNullOrEmpty(bucketName))
        //    {
        //        _logger.LogError("S3 bucket name is not configured in appsettings.json");
        //        // Handle error appropriately, maybe return an error view
        //        return Content("Error: S3 bucket is not configured.");
        //    }

        //    int successCount = 0;
        //    foreach (var file in allFiles)
        //    {
        //        if (file.Length > 0)
        //        {
        //            // Define the S3 key (folder path + unique file name)
        //            // Example: "esg-reports/2025-09-19/guid-original-filename.pdf"
        //            string s3Key = $"esg-reports/{DateTime.UtcNow:yyyy-MM-dd}/{Guid.NewGuid()}-{file.FileName}";

        //            using (var stream = file.OpenReadStream())
        //            {
        //                var uploadSuccess = await _s3Uploader.UploadFileAsync(bucketName, s3Key, stream, file.ContentType);
        //                if (uploadSuccess)
        //                {
        //                    successCount++;
        //                }
        //            }
        //        }
        //    }

        //    _logger.LogInformation("{SuccessCount} out of {TotalFiles} files were uploaded successfully.", successCount, allFiles.Count());

        //    // You can now redirect to a success page or display a summary
        //    return Content($"Successfully uploaded {successCount} of {allFiles.Count()} files for analysis.");
        //}
    }
}
