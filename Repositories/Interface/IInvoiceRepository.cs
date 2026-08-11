using KejaHUnt_PropertiesAPI.Models.Domain;

namespace KejaHUnt_PropertiesAPI.Repositories.Interface
{
    public interface IInvoiceRepository
    {
        Task<Invoice> CreateAsync(Invoice invoice);
        Task<Invoice> UpdateFromUnitPaymentsAsync(Invoice invoice);
        Task<Invoice?> GetByIdAsync(long id);
        Task<List<Invoice>> GetAllAsync();
        Task<List<Invoice>> GetByPropertyIdAsync(long propertyId);
        Task<List<Invoice>> GetByUnitIdAsync(long unitId);
        Task<List<Invoice>> GetByTenantIdAsync(long tenantId);
        Task<Invoice?> GetByUnitAndPeriodAsync(long unitId, int month, int year);
        Task<Invoice?> GetByUnitPaymentsIdAsync(long unitPaymentsId);
    }
}