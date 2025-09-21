using System.Text.Json;

namespace Hackathon_2025_ESG.Services
{
    public class BedrockPayloadBuilder
    {
        public static string BuildReportPayload(
            string companyName,
            string frameworkJson,
            string erpJson,
            List<(string file, string excerpt)> docs,
            string reportingPeriod
        )
        {
            const string instructions = @"
You are an ESG reporting assistant. Produce ONLY valid JSON (no explanatory text outside JSON). The JSON must match the schema described below exactly.

Goal:
- Create an ESG report for the given company and reportingPeriod using the provided CRI framework, ERP metrics, and document excerpts.
- The final numeric score MUST be deterministic and between 0 and 100 (inclusive).
- Use a deterministic baseline scoring algorithm described below and include any model-suggested adjustments only as a separate object with justification (they must not change the baseline numeric score).

Deterministic Scoring Rules (baseline):
1. The final score is a weighted sum of three equally weighted sections:
   - Environmental weight = 33.33
   - Social weight = 33.33
   - Governance weight = 33.34
   (These weights sum to 100.)
2. For each section, identify the relevant indicators from the provided framework JSON (count them). Let:
   - fulfilled_i = number of indicators the company fulfills for section i
   - total_i = total indicators defined in the framework for section i
   - section_score_i = (fulfilled_i / total_i) * section_weight
   If total_i is 0, treat section_score_i as 0 and set an 'insufficientIndicators' flag for that section.
3. Final numeric score = round(sum(section_score_i)) between 0 and 100.
4. For every indicator counted as 'fulfilled', attach at least one piece of evidence: { file, excerpt }.
5. If uncertain, include evidenceConfidence but do not count low confidence as fulfilled.

Return JSON only. No extra text.
";

            // Build structured user data
            var userPrompt = new
            {
                company = companyName,
                framework = JsonDocument.Parse(frameworkJson).RootElement,
                erp = JsonDocument.Parse(erpJson).RootElement,
                documents = docs.Select(d => new { d.file, d.excerpt }).ToArray(),
                reportingPeriod = reportingPeriod
            };

            // Merge instructions + data
            var combinedPrompt = $"{instructions}\n\nUser Data:\n{JsonSerializer.Serialize(userPrompt)}";

            // Bedrock messages format
            var payload = new
            {
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = new[]
                        {
                            new { text = combinedPrompt }
                        }
                    }
                }
            };

            return JsonSerializer.Serialize(payload);
        }

        public static string BuildSummaryTextractPayload(string textractString)
        {
            const string instructions = @"
You are an ESG reporting assistant. Summarize the following text into a concise summary of key points relevant to ESG reporting. Focus on extracting actionable insights, metrics, and any mentions of ESG-related initiatives or outcomes. The summary should be clear and structured, highlighting the most important information for ESG assessment.";

            var combinedPrompt = $"{instructions}\n\nText:\n{textractString}";

            var payload = new
            {
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = new[]
                        {
                            new { text = combinedPrompt }
                        }
                    }
                }
            };

            return JsonSerializer.Serialize(payload);
        }
    }
}