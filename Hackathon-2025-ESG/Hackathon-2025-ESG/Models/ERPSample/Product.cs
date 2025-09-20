using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hackathon_2025_ESG.Models.ERPSample
{
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string? ProductId { get; set; }             // PK
        public string Name { get; set; } = string.Empty;
        public string? Category { get; set; }
        public decimal CarbonFootprint { get; set; }   // kg CO2 per unit
        public decimal WaterUsage { get; set; }        // liters per unit

        public string? VendorId { get; set; }             // FK to Vendor

        // Navigation Properties
        // public Vendor? Vendor { get; set; }
        // public ICollection<OrderDetail>? OrderDetails { get; set; }
    }
}
