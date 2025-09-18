using Hackathon_2025_ESG.Areas.Identity.Data;

namespace Hackathon_2025_ESG.Models
{
    public class EsgReport
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public Hackathon_2025_ESGUser User { get; set; }

        public string FileName { get; set; }

        public string S3FilePath { get; set; }

        public string S3RawDocsDirectoryPath { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
