using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hackathon_2025_ESG.Models.ERPSample
{
    public class Company
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string? CompanyId { get; set; }             // PK
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Country { get; set; }
        public string? Industry { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        // public ICollection<Department>? Departments { get; set; }
        // public ICollection<Order>? Orders { get; set; }
        // public ICollection<EnvironmentalMetric>? EnvironmentalMetrics { get; set; }
        // public ICollection<Compliance>? Compliances { get; set; }
    }
}
