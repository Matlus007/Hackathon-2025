using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hackathon_2025_ESG.Models.ERPSample
{
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string? OrderId { get; set; }               // PK
        public string? CompanyId { get; set; }             // FK to Company
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }

        // Navigation Properties
        // public Company? Company { get; set; }
        // public ICollection<OrderDetail>? OrderDetails { get; set; }
    }
}
