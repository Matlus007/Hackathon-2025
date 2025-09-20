namespace Hackathon_2025_ESG.Services.Interface
{
    public interface IAwsS3UploaderService
    {
        Task<bool> UploadFileAsync(string bucketName, string key, Stream fileStream, string contentType);

        Task<string> GetTemporaryLinkAsync(string bucketName, string key);

        Task<long> GetObjectSizeAsync(string bucketName, string key);
    }
}
