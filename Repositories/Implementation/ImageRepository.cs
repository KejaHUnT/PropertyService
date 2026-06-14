using KejaHUnt_PropertiesAPI.Repositories.Interface;
using Minio;
using Minio.DataModel.Args;

namespace KejaHUnt_PropertiesAPI.Repositories.Implementation
{
    public class ImageRepository : IImageRepository
    {
        private readonly IMinioClient _minioClient;
        private readonly IConfiguration _configuration;

        public ImageRepository(IMinioClient minioClient, IConfiguration configuration)
        {
            _minioClient = minioClient;
            _configuration = configuration;
        }

        private string GetBucket() => _configuration["ObjectStorage:Bucket"] ?? "kejahunt-images";
        private string GetPublicBaseUrl() => _configuration["ObjectStorage:PublicBaseUrl"] ?? "";

        public async Task<string> Upload(IFormFile formFile, string folder)
        {
            if (formFile == null || formFile.Length == 0)
                throw new ApplicationException("No file uploaded.");

            var extension = Path.GetExtension(formFile.FileName);
            var objectName = $"{folder}/{Guid.NewGuid()}{extension}";
            var bucket = GetBucket();

            using var stream = formFile.OpenReadStream();

            var putArgs = new PutObjectArgs()
                .WithBucket(bucket)
                .WithObject(objectName)
                .WithStreamData(stream)
                .WithObjectSize(formFile.Length)
                .WithContentType(formFile.ContentType);

            await _minioClient.PutObjectAsync(putArgs);

            return $"{GetPublicBaseUrl()}/{objectName}";
        }

        public async Task<string> Edit(string? existingImageUrl, IFormFile formFile, string folder)
        {
            if (!string.IsNullOrEmpty(existingImageUrl))
                await Delete(existingImageUrl);

            return await Upload(formFile, folder);
        }

        public async Task Delete(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;

            var baseUrl = GetPublicBaseUrl();
            var objectName = imageUrl.Replace($"{baseUrl}/", "");
            var bucket = GetBucket();

            var removeArgs = new RemoveObjectArgs()
                .WithBucket(bucket)
                .WithObject(objectName);

            await _minioClient.RemoveObjectAsync(removeArgs);
        }
    }
}