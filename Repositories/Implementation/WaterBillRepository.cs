using KejaHUnt_PropertiesAPI.Data;
using KejaHUnt_PropertiesAPI.Models.Domain;
using KejaHUnt_PropertiesAPI.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace KejaHUnt_PropertiesAPI.Repositories.Implementation;

public class WaterBillRepository : IWaterBillRepository
{
    private readonly ApplicationDbContext _db;

    public WaterBillRepository(ApplicationDbContext db) => _db = db;

    public async Task<WaterBill> CreateAsync(WaterBill bill)
    {
        _db.WaterBills.Add(bill);
        await _db.SaveChangesAsync();
        return bill;
    }

    public async Task<WaterBill?> GetByIdAsync(long id) =>
        await _db.WaterBills
            .AsNoTracking()
            .Include(b => b.Unit)
            .Include(b => b.Reading)
            .FirstOrDefaultAsync(b => b.Id == id);

    public async Task<List<WaterBill>> GetByUnitIdAsync(long unitId) =>
        await _db.WaterBills
            .AsNoTracking()
            .Where(b => b.UnitId == unitId)
            .OrderByDescending(b => b.BillingYear)
            .ThenByDescending(b => b.BillingMonth)
            .ToListAsync();

    public async Task<List<WaterBill>> GetByPropertyAndPeriodAsync(long propertyId, int month, int year) =>
        await _db.WaterBills
            .AsNoTracking()
            .Include(b => b.Unit)
            .Where(b => b.Unit.PropertyId == propertyId && b.BillingMonth == month && b.BillingYear == year)
            .ToListAsync();
}