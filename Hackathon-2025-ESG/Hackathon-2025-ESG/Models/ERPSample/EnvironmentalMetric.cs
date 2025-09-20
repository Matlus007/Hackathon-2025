using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hackathon_2025_ESG.Models.ERPSample
{
    public class EnvironmentalMetric
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string? MetricId { get; set; }              // PK
        public string? CompanyId { get; set; }             // FK to Company
        public DateTime MetricDate { get; set; }

        public decimal EnergyUsage { get; set; }       // kWh
        public decimal WaterUsage { get; set; }        // liters
        public decimal WasteGenerated { get; set; }    // kg
        public decimal Emissions { get; set; }         // CO2 kg

        // Navigation Properties
        // public Company? Company { get; set; }
    }
}
