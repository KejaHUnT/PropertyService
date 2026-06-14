using KejaHUnt_PropertiesAPI.Repositories.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KejaHUnt_PropertiesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly IImageRepository _imageRepository;

        public ImageController(IImageRepository imageRepository)
        {
            _imageRepository = imageRepository;
        }

        [HttpPost]
        [Route("{folder}")]
        public async Task<IActionResult> Upload(string folder, [FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            try
            {
                var imageUrl = await _imageRepository.Upload(file, folder);
                return Ok(new { imageUrl });
            }
            catch (ApplicationException ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPut]
        [Route("{folder}")]
        public async Task<IActionResult> EditFile(string folder, [FromForm] IFormFile file, [FromQuery] string? existingImageUrl)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            try
            {
                var newImageUrl = await _imageRepository.Edit(existingImageUrl, file, folder);
                return Ok(new { imageUrl = newImageUrl, message = "File updated successfully." });
            }
            catch (ApplicationException ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}