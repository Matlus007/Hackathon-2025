using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hackathon_2025_ESG.Models.ERPSample
{
    public class Vendor
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string? VendorId { get; set; }              // PK
        public string Name { get; set; } = string.Empty;
        public string? Country { get; set; }
        public string? ESGRating { get; set; }         // e.g., "A", "B", "C"
        public bool Active { get; set; } = true;

        // Navigation Properties
        // public ICollection<Product>? Products { get; set; }
    }
}
