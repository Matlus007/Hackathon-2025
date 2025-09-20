using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hackathon_2025_ESG.Models.ERPSample
{
    public class Employee
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string? EmployeeId { get; set; }            // PK
        public string? DepartmentId { get; set; }          // FK to Department
        public string Name { get; set; } = string.Empty;
        public string? Position { get; set; }
        public DateTime HireDate { get; set; }
        public decimal Salary { get; set; }

        // Navigation Properties
        // public Department? Department { get; set; }
    }
}
