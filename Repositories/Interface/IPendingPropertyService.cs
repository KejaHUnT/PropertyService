using KejaHUnt_PropertiesAPI.Models.Dto;

namespace KejaHUnt_PropertiesAPI.Repositories.Interface
{
    public interface IPendingPropertyService
    {
        Task<PendingPropertyDto> SubmitAsync(PendingPropertyRequestDto dto, string userId, string imageUrl);
        Task<IEnumerable<PendingPropertyRequestDto>> GetAllPendingAsync();
        Task ApproveAsync(long id);
    }
}
