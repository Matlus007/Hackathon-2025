using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hackathon_2025_ESG.Models.ERPSample
{
    public class OrderDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string? OrderDetailId { get; set; }         // PK
        public string? OrderId { get; set; }               // FK to Order
        public string? ProductId { get; set; }             // FK to Product
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        // Navigation Properties
        // public Order? Order { get; set; }
        // public Product? Product { get; set; }
    }
}
