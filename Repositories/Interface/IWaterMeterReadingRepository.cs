using KejaHUnt_PropertiesAPI.Models.Domain;

namespace KejaHUnt_PropertiesAPI.Repositories.Interface;

public interface IWaterMeterReadingRepository
{
    Task<WaterMeterReading> CreateAsync(WaterMeterReading reading);
    Task<List<WaterMeterReading>> GetHistoryByUnitIdAsync(long unitId);

    /// <summary>
    /// True if a reading already exists for this unit+period — used to enforce
    /// idempotency (a billing run can't be accidentally executed twice for the same month).
    /// </summary>
    Task<HashSet<long>> GetUnitIdsWithReadingForPeriodAsync(IEnumerable<long> unitIds, int month, int year);

    /// <summary>
    /// Bulk-fetches the most recent reading per unit in a single query, keyed by UnitId.
    /// Avoids N+1 queries when generating bills for an entire property at once.
    /// </summary>
    Task<Dictionary<long, WaterMeterReading>> GetLastReadingsBulkAsync(IEnumerable<long> unitIds);
}