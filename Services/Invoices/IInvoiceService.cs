using KejaHUnt_PropertiesAPI.Models.Dto;

namespace KejaHUnt_PropertiesAPI.Services.Invoices
{
    public interface IInvoiceService
    {
        Task<InvoiceDto> GenerateInvoiceAsync(CreateInvoiceDto dto);
        Task<InvoiceDto?> GetByIdAsync(long id);
        Task<List<InvoiceDto>> GetAllAsync();
        Task<List<InvoiceDto>> GetByPropertyIdAsync(long propertyId);
        Task<List<InvoiceDto>> GetByUnitIdAsync(long unitId);
        Task<List<InvoiceDto>> GetByTenantIdAsync(long tenantId);
    }
}