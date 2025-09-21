using System.ComponentModel.DataAnnotations;

namespace Hackathon_2025_ESG.Areas.Client.Models
{
    public class EsgAnalysisViewModel
    {
        public string? SelectedStandard { get; set; }

        [Display(Name = "Environmental Documents")]
        public List<IFormFile> EnvironmentalFiles { get; set; } = new List<IFormFile>();

        [Display(Name = "Social Documents")]
        public List<IFormFile> SocialFiles { get; set; } = new List<IFormFile>();

        [Display(Name = "Governance Documents")]
        public List<IFormFile> GovernanceFiles { get; set; } = new List<IFormFile>();
    }
}
