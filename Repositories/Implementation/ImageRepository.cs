using System.Text.Json;
using KejaHUnt_PropertiesAPI.Data;
using KejaHUnt_PropertiesAPI.Models.Dto;
using KejaHUnt_PropertiesAPI.Repositories.Interface;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace KejaHUnt_PropertiesAPI.Repositories.Implementation
{
    public class ImageRepository : IImageRepository
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ImageRepository(HttpClient httpClient, IConfiguration configuration, ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        

        public async Task<Guid> Upload(IFormFile formFile)
        {
            if (formFile == null || formFile.Length == 0)
            {
                throw new ApplicationException("No file uploaded.");
            }
            var fileHandlerBaseUrl = _configuration["FileHandlerApi:BaseUrl"];
            var endpoint = $"{fileHandlerBaseUrl}/upload";                      

            using var httpClient = new HttpClient();
            using var formData = new MultipartFormDataContent();

            // Read the form file stream
            using var stream = formFile.OpenReadStream();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(formFile.ContentType);

            // Add the file to the form data. The name "file" must match the parameter name in [FromForm] IFormFile file
            formData.Add(fileContent, "file", formFile.FileName);

            // Send HTTP POST to external upload endpoint
            try
            {
                // Send HTTP POST to external upload endpoint
                var response = await httpClient.PostAsync(endpoint, formData);
                if (response.IsSuccessStatusCode)
                {
                    // Deserialize the response body to get the documentId as a string
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var responseObject = JsonConvert.DeserializeObject<JObject>(responseBody);

                    // Extract the documentId as a string and then convert it to a Guid
                    var documentIdString = responseObject["documentId"].ToString();
                    Guid documentId = Guid.Parse(documentIdString);

                    return documentId;
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    throw new ApplicationException($"Failed to upload file: {errorMessage}");
                }

            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                throw new ApplicationException($"Error occurred during file upload: {ex.Message}", ex);
            }
        }

        // Helper class for deserialization
        private class UploadResult
        {
            public Guid DocumentId { get; set; }
        }

        public async Task<FileHandlerResponse> GetByDocumentIdAsync(Guid documentId)
        {
            var fileHandlerUrl = _configuration["FileHandlerApi:FetchUrl"];
            var endpoint = $"{fileHandlerUrl}/{documentId}";

            using var httpClient = new HttpClient();
            
            var response = await httpClient.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new ApplicationException($"Failed to fetch file: {error}");
            }

            var resultJson = await response.Content.ReadAsStringAsync();

            // Deserialize the response to FileHandlerResponse DTO
            var fileResponse = JsonSerializer.Deserialize<FileHandlerResponse>(resultJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (fileResponse == null || string.IsNullOrEmpty(fileResponse.Base64))
                throw new ApplicationException("Invalid file response from file handler.");

            return fileResponse;
        }

        // Edit an existing file (update it)
        public async Task<Guid> Edit(Guid? documentId, IFormFile formFile)
        {
            if (formFile == null || formFile.Length == 0)
            {
                throw new ApplicationException("No file uploaded.");
            }

            var fileHandlerBaseUrl = _configuration["FileHandlerApi:BaseUrl"];
            var endpoint = $"{fileHandlerBaseUrl}/{documentId}";  // Assuming the endpoint to edit is '/edit/{documentId}'

            using var formData = new MultipartFormDataContent();
            using var stream = formFile.OpenReadStream();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(formFile.ContentType);

            formData.Add(fileContent, "newFile", formFile.FileName);

            try
            {
                var response = await _httpClient.PutAsync(endpoint, formData);
                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var responseObject = JsonConvert.DeserializeObject<JObject>(responseBody);
                    var documentIdString = responseObject["documentId"].ToString();
                    Guid updatedDocumentId = Guid.Parse(documentIdString);

                    return updatedDocumentId;
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    throw new ApplicationException($"Failed to update file: {errorMessage}");
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error occurred during file update: {ex.Message}", ex);
            }
        }


        //private string GetContentType(string extension)
        //{
        //    return extension.ToLower() switch
        //    {
        //        ".jpg" or ".jpeg" => "image/jpeg",
        //        ".png" => "image/png",
        //        ".gif" => "image/gif",
        //        ".pdf" => "application/pdf",
        //        ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        //        ".doc" => "application/msword",
        //        _ => "application/octet-stream"
        //    };
        //}


    }
}
