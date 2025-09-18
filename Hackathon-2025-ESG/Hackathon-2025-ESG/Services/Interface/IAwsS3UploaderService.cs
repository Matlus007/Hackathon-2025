namespace Hackathon_2025_ESG.Services.Interface
{
    public interface IAwsS3UploaderService
    {
        Task<bool> UploadFileAsync(string bucketName, string key, Stream fileStream, string contentType);
    }
}
