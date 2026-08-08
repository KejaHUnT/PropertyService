using KejaHUnt_PropertiesAPI.Data;
using KejaHUnt_PropertiesAPI.Models.Domain;
using KejaHUnt_PropertiesAPI.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace KejaHUnt_PropertiesAPI.Repositories.Implementation;

 public class WaterMeterReadingRepository : IWaterMeterReadingRepository
    {
        private readonly ApplicationDbContext _db;

        public WaterMeterReadingRepository(ApplicationDbContext db) => _db = db;

        public async Task<WaterMeterReading> CreateAsync(WaterMeterReading reading)
        {
            _db.WaterMeterReadings.Add(reading);
            await _db.SaveChangesAsync();
            return reading;
        }

        public async Task<List<WaterMeterReading>> GetHistoryByUnitIdAsync(long unitId) =>
            await _db.WaterMeterReadings
                .AsNoTracking()
                .Include(r => r.Bill)
                .Where(r => r.UnitId == unitId)
                .OrderByDescending(r => r.BillingYear)
                .ThenByDescending(r => r.BillingMonth)
                .ToListAsync();

        public async Task<HashSet<long>> GetUnitIdsWithReadingForPeriodAsync(IEnumerable<long> unitIds, int month, int year)
        {
            var ids = unitIds.ToList();
            var existing = await _db.WaterMeterReadings
                .AsNoTracking()
                .Where(r => ids.Contains(r.UnitId) && r.BillingMonth == month && r.BillingYear == year)
                .Select(r => r.UnitId)
                .ToListAsync();

            return existing.ToHashSet();
        }

        public async Task<Dictionary<long, WaterMeterReading>> GetLastReadingsBulkAsync(IEnumerable<long> unitIds)
        {
            var ids = unitIds.ToList();
            if (ids.Count == 0) return new Dictionary<long, WaterMeterReading>();

            // Single grouped query — one round trip regardless of unit count.
            var latest = await _db.WaterMeterReadings
                .AsNoTracking()
                .Where(r => ids.Contains(r.UnitId))
                .GroupBy(r => r.UnitId)
                .Select(g => g
                    .OrderByDescending(r => r.BillingYear)
                    .ThenByDescending(r => r.BillingMonth)
                    .First())
                .ToListAsync();

            return latest.ToDictionary(r => r.UnitId);
        }
    }