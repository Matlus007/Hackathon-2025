using Hackathon_2025_ESG.Areas.Client.Models;
using Hackathon_2025_ESG.Data;
using Hackathon_2025_ESG.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Hackathon_2025_ESG.Areas.Client.Controllers
{
    [Area("Client")]
    [Route("Home/[action]")]
    public class HomeController : Controller
    {
        private readonly Hackathon_2025_ESGContext _context;
        private readonly IAwsS3UploaderService _s3Service;
        private readonly IConfiguration _configuration;

        public HomeController(
            Hackathon_2025_ESGContext context,
            IAwsS3UploaderService s3Service,
            IConfiguration configuration)
        {
            _context = context;
            _s3Service = s3Service;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "User session expired. Please log in again.";
                return Redirect("~/Identity/Account/Login");
            }

            // Get the bucket name from appsettings.json
            var bucketName = _configuration["S3Buckets:EsgDocuments"];

            // 1. Fetch report records from the database for the current user
            var userReports = await _context.EsgReport
                                      .Where(r => r.UserId == userId)
                                      .OrderByDescending(r => r.CreatedAt) // Show newest first
                                      .ToListAsync();

            var reportCards = new List<ReportCardViewModel>();

            // 2. Create a ViewModel for each report and generate the S3 link
            foreach (var report in userReports)
            {
                long fileSizeInBytes = await _s3Service.GetObjectSizeAsync(bucketName, report.S3FilePath);
                string downloadUrl = await _s3Service.GetTemporaryLinkAsync(bucketName, report.S3FilePath);

                var card = new ReportCardViewModel
                {
                    Id = report.Id,
                    FileName = report.FileName,
                    FormattedSize = FormatFileSize(fileSizeInBytes),
                    FormattedCreationDate = $"Created {report.CreatedAt:M/d/yyyy}",
                    DownloadUrl = downloadUrl
                };
                reportCards.Add(card);
            }

            // 3. Populate the final view model
            var viewModel = new HomePageViewModel
            {
                RecentReports = reportCards,
                TotalReportCount = userReports.Count()
            };

            return View(viewModel);
        }

        private static string FormatFileSize(long bytes)
        {
            if (bytes == 0) return "0 Bytes";
            string[] suffixes = { "Bytes", "KB", "MB", "GB", "TB" };
            int i = 0;
            double dblSByte = bytes;
            while (dblSByte >= 1024 && i < suffixes.Length - 1)
            {
                dblSByte /= 1024;
                i++;
            }
            return $"{dblSByte:0.##} {suffixes[i]}";
        }
    }
}
