using KejaHUnt_PropertiesAPI.Models.Domain;

namespace KejaHUnt_PropertiesAPI.Repositories.Interface;

public interface IWaterRateRepository
{
    Task<WaterRate?> GetActiveByPropertyIdAsync(long propertyId);
    Task<List<WaterRate>> GetHistoryByPropertyIdAsync(long propertyId);

    /// <summary>
    /// Deactivates any currently active rate(s) for the property and inserts the
    /// new one as active, in a single unit of work.
    /// </summary>
    Task<WaterRate> SetActiveRateAsync(long propertyId, decimal pricePerUnit, DateTime effectiveFrom);
}