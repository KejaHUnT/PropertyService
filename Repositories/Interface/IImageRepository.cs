namespace KejaHUnt_PropertiesAPI.Repositories.Interface
{
    public interface IImageRepository
    {
        Task<string> Upload(IFormFile formFile, string folder);
        Task<string> Edit(string? existingImageUrl, IFormFile formFile, string folder);
        Task Delete(string imageUrl);
    }       
}