using System.Text;
using System.Text.Json;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;

namespace Hackathon_2025_ESG.Services
{
    public class BedrockClientWrapper
    {
        private readonly ILogger<BedrockClientWrapper> _logger;
        private readonly IConfiguration _configuration;

        private readonly IAmazonBedrockRuntime _bedrock;

        public BedrockClientWrapper(ILogger<BedrockClientWrapper> logger, IConfiguration configuration, IAmazonBedrockRuntime bedrock)
        {
            _logger = logger;
            _configuration = configuration;
            _bedrock = bedrock;
        }

        public async Task<string> InvokeBedrockModelAsync(string prompt, CancellationToken ct = default)
        {
            var modelId = "amazon.nova-pro-v1:0";
            var request = new InvokeModelRequest
            {
                ModelId = modelId,
                ContentType = "application/json",
                Accept = "application/json",
                Body = new MemoryStream(Encoding.UTF8.GetBytes(prompt)),
            };

            try
            {
                var response = await _bedrock.InvokeModelAsync(request, ct);
                using var reader = new StreamReader(response.Body);
                return await reader.ReadToEndAsync();
            }
            catch (ValidationException ve)
            {
                _logger.LogError(ve, "Validation error invoking Bedrock model: {Message}", ve.Message);
                throw;
            }
            catch (AmazonBedrockRuntimeException abre)
            {
                _logger.LogError(abre, "AWS Bedrock service error: {Message}", abre.Message);
                throw;
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                _logger.LogWarning("Bedrock model invocation was canceled.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invoking Bedrock model.");
                throw;
            }
        }

        public string BuildBedRockPayload(string companyName, string frameworkJson, string erpJson, List<(string file, string excerpt)> docs, string reportingPeriod)
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
   - fulfilled_i = number of indicators the company fulfills for section i (based on ERP data + document evidence)
   - total_i = total indicators defined in the framework for section i
   - section_score_i = (fulfilled_i / total_i) * section_weight
   If total_i is 0, treat section_score_i as 0 and set an 'insufficientIndicators' flag for that section.
3. Final numeric score = round(sum(section_score_i)) to the nearest integer between 0 and 100.
4. For every indicator counted as 'fulfilled', attach at least one piece of evidence: an object containing { file: '<s3/path-or-filename>', excerpt: '<short excerpt or sentence>' }.
5. If the model is uncertain about a fulfillment decision, mark the indicator with 'evidenceConfidence': 'low|medium|high'. Low confidence does NOT count as fulfilled in the deterministic baseline (but may be recommended as provisional in model_suggestions).

Model Output Requirements:
- Return JSON object conforming exactly to the schema below.
- Text fields should be concise: overview max 600 words, each criterion explanation max 120 words.
- Use arrays of objects (not nested strings) so the PDF generator can render reliably.
- Include both numeric baseline scores and model_proposed_adjustments (if any). The app will use baseline numeric score for the PDF numeric result.

Evidence and Auditability:
- For each fulfilled indicator include 'evidence': array of { file, excerpt, evidenceConfidence }.
- For each unfulfilled indicator optionally include 'missingEvidenceReason' (short string).
- If the model suggests changing a baseline score, place that suggestion in 'model_suggestions' with a structured justification and list the evidence supporting the suggestion.

Return JSON only. Do NOT include any additional commentary, explanation, or text outside the JSON object.
";

            var promptObj = new
            {
                system = "You are an ESG reporting assistant. Return JSON.",
                company = companyName,
                framework = JsonDocument.Parse(frameworkJson).RootElement,
                erp = JsonDocument.Parse(erpJson).RootElement,
                documents = docs.Select(d => new { d.file, excerpt = d.excerpt }).ToArray(),
                reportingPeriod = reportingPeriod,
                instructions = instructions
            };

            return JsonSerializer.Serialize(promptObj);
        }
    }
}