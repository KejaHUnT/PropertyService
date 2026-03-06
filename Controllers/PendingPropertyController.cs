using KejaHUnt_PropertiesAPI.Models.Dto;
using KejaHUnt_PropertiesAPI.Repositories.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KejaHUnt_PropertiesAPI.Controllers
{
    [Route("api/properties/pending")]
    [ApiController]
    public class PendingPropertyController : ControllerBase
    {
        private readonly IPendingPropertyService _pendingService;
        private readonly IImageRepository _fileService; // Assuming this uploads files and returns Guid

        public PendingPropertyController(IPendingPropertyService pendingService, IImageRepository fileService)
        {
            _pendingService = pendingService;
            _fileService = fileService;
        }

        [HttpPost("submit")]
        public async Task<IActionResult> Submit([FromForm] PendingPropertyRequestDto dto)
        {
            var userId = dto.Email; // Replace with your method of getting UserId from token
            var documentId = await _fileService.Upload(dto.ImageFile);
            var property = await _pendingService.SubmitAsync(dto, userId, documentId);
            return Ok(property);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPending()
        {
            var pendingList = await _pendingService.GetAllPendingAsync();
            return Ok(pendingList);
        }

        [HttpPost("approve/{id}")]
        public async Task<IActionResult> Approve(long id)
        {
            await _pendingService.ApproveAsync(id);
            return Ok(new { message = "Property approved successfully." });
        }

    }
}
