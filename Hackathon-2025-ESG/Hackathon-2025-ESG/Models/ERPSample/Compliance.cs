using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hackathon_2025_ESG.Models.ERPSample
{
    public class Compliance
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string? ComplianceId { get; set; }          // PK
        public string? CompanyId { get; set; }             // FK to Company

        public string Name { get; set; } = string.Empty;
        public DateTime CertificationDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string Status { get; set; } = "Active"; // Active, Expired, Pending

        // Navigation Properties
        // public Company? Company { get; set; }
    }
}
