using KejaHUnt_PropertiesAPI.Data;
using KejaHUnt_PropertiesAPI.Models.Domain;
using KejaHUnt_PropertiesAPI.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace KejaHUnt_PropertiesAPI.Repositories.Implementation;

public class WaterRateRepository : IWaterRateRepository
{
    private readonly ApplicationDbContext _db;

    public WaterRateRepository(ApplicationDbContext db) => _db = db;

    public async Task<WaterRate?> GetActiveByPropertyIdAsync(long propertyId) =>
        await _db.WaterRates
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.PropertyId == propertyId && r.IsActive);

    public async Task<List<WaterRate>> GetHistoryByPropertyIdAsync(long propertyId) =>
        await _db.WaterRates
            .AsNoTracking()
            .Where(r => r.PropertyId == propertyId)
            .OrderByDescending(r => r.EffectiveFrom)
            .ToListAsync();

    public async Task<WaterRate> SetActiveRateAsync(long propertyId, decimal pricePerUnit, DateTime effectiveFrom)
    {
        var activeRates = await _db.WaterRates
            .Where(r => r.PropertyId == propertyId && r.IsActive)
            .ToListAsync();

        foreach (var rate in activeRates)
        {
            rate.IsActive = false;
            rate.EffectiveTo = effectiveFrom;
        }

        var newRate = new WaterRate
        {
            PropertyId = propertyId,
            PricePerUnit = pricePerUnit,
            EffectiveFrom = effectiveFrom,
            IsActive = true
        };

        _db.WaterRates.Add(newRate);
        await _db.SaveChangesAsync();
        return newRate;
    }
}