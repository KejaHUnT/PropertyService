using KejaHUnt_PropertiesAPI.Models.Domain;
using KejaHUnt_PropertiesAPI.Models.Dto;

namespace KejaHUnt_PropertiesAPI.Services.Payments
{
    public interface IPaymentService
    {

        Task<InitializePaymentResponse> InitializePaymentAsync(CreateUnitPaymentsDto dto);

        Task<List<UnitPaymentsDto>> GetAllAsync();

        Task<UnitPaymentsDto?> GetByIdAsync(long id);

        Task<List<UnitPaymentsDto>> GetByTenantIdAsync(long tenantId);

        Task<List<UnitPaymentsDto>> GetByUnitIdAsync(long unitId);

        Task<UnitPaymentsDto?> UpdateAsync(long id, UpdateUnitPaymentsDto dto);

        Task<UnitPaymentsDto?> DeleteAsync(long id);

        Task HandleWebhookAsync(string reference, int status);
    }
}
