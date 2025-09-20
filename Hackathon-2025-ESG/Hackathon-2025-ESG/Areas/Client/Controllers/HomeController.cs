using Hackathon_2025_ESG.Areas.Client.Models;
using Microsoft.AspNetCore.Mvc;

namespace Hackathon_2025_ESG.Areas.Client.Controllers
{
    [Area("Client")]
    [Route("Home/[action]")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var mockReports = new List<ReportCardViewModel>
            {
                new ReportCardViewModel
                {
                    Id = "1",
                    FileName = "Q4_2023_ESG_Assessment.pdf",
                    FormattedSize = "2.4 MB",
                    FormattedCreationDate = "Created 1/15/2024",
                    DownloadUrl = "#" // Placeholder for the S3 pre-signed URL
                },
                new ReportCardViewModel
                {
                    Id = "2",
                    FileName = "Annual_Sustainability_Report_2023.pdf",
                    FormattedSize = "5.8 MB",
                    FormattedCreationDate = "Created 1/12/2024",
                    DownloadUrl = "#"
                }
            };

            var viewModel = new HomePageViewModel
            {
                RecentReports = mockReports,
                TotalReportCount = 12 // Placeholder count
            };

            return View(viewModel);
        }
    }
}
