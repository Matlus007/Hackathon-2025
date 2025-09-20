namespace Hackathon_2025_ESG.Areas.Client.Models
{
    public class HomePageViewModel
    {
        public List<ReportCardViewModel> RecentReports { get; set; } = new List<ReportCardViewModel>();
        public int TotalReportCount { get; set; }
    }

    // This class represents a single report card in the UI
    public class ReportCardViewModel
    {
        public string Id { get; set; }
        public string FileName { get; set; }
        public string FormattedSize { get; set; } // e.g., "2.4 MB"
        //public int PageCount { get; set; }
        public string FormattedCreationDate { get; set; } // e.g., "Created 1/15/2024"
        public string DownloadUrl { get; set; } // This will hold the temporary S3 link
    }
}
