using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hackathon_2025_ESG.Areas.Client.Controllers
{

    [Area("Client")]
    [Route("GenerateReport/[action]")]
    //[Authorize] // Ensure the user is logged in
    public class SelectStandardController : Controller
    {
        private readonly ILogger<UploadDocsController> _logger;
        public SelectStandardController(ILogger<UploadDocsController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubmitStandard(string SelectedStandard)
        {
            // You can now pass this value to the next page or use it in your logic.
            _logger.LogInformation("User selected the {Standard} standard.", SelectedStandard);

            // Example: Redirect to the UploadDocs page, passing the standard as a query parameter
            return RedirectToAction("Index", "UploadDocs", new { area = "Client", standard = SelectedStandard });
        }
    }
}
