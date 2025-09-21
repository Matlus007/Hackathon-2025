namespace Hackathon_2025_ESG.Services.Interface
{
    public interface IBedrockService
    {
        Task<string> InvokeAsync(string payloadJson);
    }
}