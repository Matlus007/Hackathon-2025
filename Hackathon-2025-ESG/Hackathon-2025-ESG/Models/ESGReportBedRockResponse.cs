using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hackathon_2025_ESG.Models
{
    public class ESGReportBedRockResponse
    {

    }

    public class EsgReportResponse
    {
        public string Company { get; set; }
        public string ReportingPeriod { get; set; }
        public int FinalScore { get; set; }
        public List<SectionScore> Sections { get; set; }
        public List<ModelSuggestion> ModelSuggestions { get; set; }
    }

    public class SectionScore
    {
        public string Section { get; set; } // Environmental, Social, Governance
        public double Score { get; set; }
        public List<Indicator> Indicators { get; set; }
    }

    public class Indicator
    {
        public string Name { get; set; }
        public bool Fulfilled { get; set; }
        public string MissingEvidenceReason { get; set; }
        public List<Evidence> Evidence { get; set; }
    }

    public class Evidence
    {
        public string File { get; set; }
        public string Excerpt { get; set; }
        public string EvidenceConfidence { get; set; }
    }

    public class ModelSuggestion
    {
        public string Suggestion { get; set; }
        public string Justification { get; set; }
    }
}