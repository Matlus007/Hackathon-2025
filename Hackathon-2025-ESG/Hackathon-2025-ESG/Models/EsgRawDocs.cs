using Hackathon_2025_ESG.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;

namespace Hackathon_2025_ESG.Models
{
    public class EsgRawDocs
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public Hackathon_2025_ESGUser User { get; set; }

        public string FileName { get; set; }

        public string S3DirectoryPath { get; set; }

        public string S3FilePath { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
