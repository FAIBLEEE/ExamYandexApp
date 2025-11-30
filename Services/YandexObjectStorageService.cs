using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace ExamYandexApp.Services
{
    public class YandexObjectStorageService : IObjectStorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;
        private readonly string _serviceUrl;

        public YandexObjectStorageService(IConfiguration configuration)
        {
            var accessKey = configuration["ObjectStorage:AccessKey"];
            var secretKey = configuration["ObjectStorage:SecretKey"];
            _bucketName = configuration["ObjectStorage:BucketName"];
            _serviceUrl = configuration["ObjectStorage:ServiceUrl"] ?? "https://storage.yandexcloud.net";

            Console.WriteLine($"Object Storage Config - Bucket: {_bucketName}, ServiceUrl: {_serviceUrl}");

            var config = new AmazonS3Config
            {
                ServiceURL = _serviceUrl,
                ForcePathStyle = true
            };

            _s3Client = new AmazonS3Client(accessKey, secretKey, config);
        }

        public async Task<string> UploadFileAsync(IFormFile file, string fileName)
        {
            if (string.IsNullOrEmpty(_bucketName))
            {
                throw new InvalidOperationException("BucketName is not configured");
            }

            using var stream = file.OpenReadStream();
            
            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = stream,
                Key = fileName,
                BucketName = _bucketName,
                ContentType = file.ContentType,
                CannedACL = S3CannedACL.PublicRead
            };

            var transferUtility = new TransferUtility(_s3Client);
            await transferUtility.UploadAsync(uploadRequest);

            return fileName;
        }

        public async Task<bool> DeleteFileAsync(string fileName)
        {
            try
            {
                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = fileName
                };

                await _s3Client.DeleteObjectAsync(deleteRequest);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string GetFileUrl(string fileName)
        {
            return $"{_serviceUrl}/{_bucketName}/{fileName}";
        }
    }
}