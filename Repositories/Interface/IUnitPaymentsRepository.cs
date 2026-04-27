using KejaHUnt_PropertiesAPI.Models.Domain;

namespace KejaHUnt_PropertiesAPI.Repositories.Interface
{
    public interface IUnitPaymentsRepository
    {
        Task<UnitPayments> CreateAsync(UnitPayments unitPayments);
        Task<List<UnitPayments>> GetAllAsync();
        Task<UnitPayments?> GetByIdAsync(long id);
        Task<UnitPayments?> UpdateAsync(UnitPayments unitPayments);
        Task<UnitPayments?> DeleteAsync(long id);

        Task<List<UnitPayments>> GetByPropertyIdAsync(long propertyId);
        Task<List<UnitPayments>> GetByTenantIdAsync(long tenantId);
        Task<List<UnitPayments>> GetByUnitIdAsync(long unitId);
        Task<UnitPayments?> GetByUnitAndPeriodAsync(long unitId, int month, int year);
    }
}
