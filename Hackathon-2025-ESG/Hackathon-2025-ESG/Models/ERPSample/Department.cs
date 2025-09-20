using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hackathon_2025_ESG.Models.ERPSample
{
    public class Department
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string? DepartmentId { get; set; }          // PK
        public string? CompanyId { get; set; }             // FK to Company
        public string Name { get; set; } = string.Empty;

        // Navigation Properties
        // public Company? Company { get; set; }
        // public ICollection<Employee>? Employees { get; set; }
    }
}
