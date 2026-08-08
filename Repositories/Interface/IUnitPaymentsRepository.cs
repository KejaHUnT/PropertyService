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

        /// <summary>
        /// Applies a water charge to the UnitPayments row for the given unit+period,
        /// keeping ExpectedAmount in sync. Returns null if no payment record exists yet
        /// for that period — this method never creates one, since tenant assignment is
        /// owned by a different service. Caller decides how to handle the null case.
        /// </summary>
        Task<UnitPayments?> ApplyWaterChargeAsync(long unitId, int month, int year, decimal waterAmount, long waterBillId);
    }
}
