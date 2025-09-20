using Amazon.S3;
using Amazon.S3.Model;
using Hackathon_2025_ESG.Services.Interface;
using System.Net;

namespace Hackathon_2025_ESG.Services
{
    public class AwsS3UploaderService : IAwsS3UploaderService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly ILogger<AwsS3UploaderService> _logger;

        public AwsS3UploaderService(IAmazonS3 s3Client, ILogger<AwsS3UploaderService> logger)
        {
            _s3Client = s3Client;
            _logger = logger;
        }

        public async Task<bool> UploadFileAsync(string bucketName, string key, Stream fileStream, string contentType)
        {
            try
            {
                var request = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = key,
                    InputStream = fileStream,
                    ContentType = contentType,
                    //CannedACL = S3CannedACL.PublicRead
                };

                var response = await _s3Client.PutObjectAsync(request);

                if (response.HttpStatusCode == HttpStatusCode.OK)
                {
                    _logger.LogInformation("Successfully uploaded file {Key} to bucket {BucketName}.", key, bucketName);
                    return true;
                }
                else
                {
                    _logger.LogError("Failed to upload file {Key} to bucket {BucketName}. Status: {StatusCode}", key, bucketName, response.HttpStatusCode);
                    return false;
                }
            }
            catch (AmazonS3Exception e)
            {
                _logger.LogError(e, "Error encountered on server when writing an object. Key: {Key}, Bucket: {BucketName}", key, bucketName);
                return false;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unknown error encountered when writing an object. Key: {Key}, Bucket: {BucketName}", key, bucketName);
                return false;
            }
        }

        public Task<string> GetTemporaryLinkAsync(string bucketName, string key)
        {
            try
            {
                var request = new GetPreSignedUrlRequest
                {
                    BucketName = bucketName,
                    Key = key,
                    Expires = DateTime.UtcNow.AddHours(1) // Link is valid for 1 hour
                };

                string url = _s3Client.GetPreSignedURL(request);
                return Task.FromResult(url);
            }
            catch (AmazonS3Exception e)
            {
                _logger.LogError(e, "Error generating pre-signed URL for key: {Key}", key);
                return Task.FromResult("#"); // Return a dead link on error
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unknown error generating pre-signed URL for key: {Key}", key);
                return Task.FromResult("#");
            }
        }

        public async Task<long> GetObjectSizeAsync(string bucketName, string key)
        {
            try
            {
                var request = new GetObjectMetadataRequest
                {
                    BucketName = bucketName,
                    Key = key
                };
                var response = await _s3Client.GetObjectMetadataAsync(request);
                return response.Headers.ContentLength; // Returns the size in bytes
            }
            catch (AmazonS3Exception e)
            {
                _logger.LogError(e, "Error getting metadata for key: {Key}. File may not exist.", key);
                return 0; // Return 0 if file not found or on error
            }
        }
    }
}
