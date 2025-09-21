using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Hackathon_2025_ESG.Services.Interface;
using System.Text;

namespace Hackathon_2025_ESG.Services
{
    public class BedrockService: IBedrockService
    {
        private readonly IAmazonBedrockRuntime _bedrockClient;
        private readonly ILogger<BedrockService> _logger;

        private const string MODEL_ID = "amazon.nova-pro-v1:0";

        public BedrockService(IAmazonBedrockRuntime bedrockClient, ILogger<BedrockService> logger)
        {
            _bedrockClient = bedrockClient;
            _logger = logger;
        }

        public async Task<string> InvokeAsync(string payloadJson)
        {
            try
            {
                _logger.LogInformation("Invoking Bedrock model: {ModelId}", MODEL_ID);

                var request = new InvokeModelRequest
                {
                    ModelId = MODEL_ID,
                    ContentType = "application/json",
                    Accept = "application/json",
                    Body = new MemoryStream(Encoding.UTF8.GetBytes(payloadJson))
                };
                
                var response = await _bedrockClient.InvokeModelAsync(request);

                using var reader = new StreamReader(response.Body);
                var result = await reader.ReadToEndAsync();

                _logger.LogInformation("Bedrock Response Received");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Bedrock model.");
                throw;
            }
        }
    }
}