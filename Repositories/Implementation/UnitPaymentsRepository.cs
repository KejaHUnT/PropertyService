using KejaHUnt_PropertiesAPI.Data;
using KejaHUnt_PropertiesAPI.Models.Domain;
using KejaHUnt_PropertiesAPI.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace KejaHUnt_PropertiesAPI.Repositories.Implementation
{
    public class UnitPaymentsRepository : IUnitPaymentsRepository
    {
        private readonly ApplicationDbContext _db;

        public UnitPaymentsRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<UnitPayments> CreateAsync(UnitPayments unitPayments)
        {
            await _db.UnitPayments.AddAsync(unitPayments);
            await _db.SaveChangesAsync();
            return unitPayments;
        }

        public async Task<List<UnitPayments>> GetAllAsync()
        {
            return await _db.UnitPayments
                .Include(x => x.Unit)
                .Include(x => x.Property)
                .ToListAsync();
        }

        public async Task<UnitPayments?> GetByIdAsync(long id)
        {
            return await _db.UnitPayments
                .Include(x => x.Unit)
                .Include(x => x.Property)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<UnitPayments?> UpdateAsync(UnitPayments unitPayments)
        {
            var existing = await _db.UnitPayments
                .FirstOrDefaultAsync(x => x.Id == unitPayments.Id);

            if (existing == null)
                return null;

            existing.UnitId = unitPayments.UnitId;
            existing.PropertyId = unitPayments.PropertyId;
            existing.TenantId = unitPayments.TenantId;
            existing.PeriodMonth = unitPayments.PeriodMonth;
            existing.PeriodYear = unitPayments.PeriodYear;

            // IMPORTANT
            existing.ExpectedAmount = unitPayments.ExpectedAmount;
            existing.PaidAmount = unitPayments.PaidAmount;
            existing.Status = unitPayments.Status;

            await _db.SaveChangesAsync();
            return existing;
        }

        public async Task<UnitPayments?> DeleteAsync(long id)
        {
            var existing = await _db.UnitPayments
                .FirstOrDefaultAsync(x => x.Id == id);

            if (existing == null)
                return null;

            _db.UnitPayments.Remove(existing);
            await _db.SaveChangesAsync();

            return existing;
        }

        public async Task<List<UnitPayments>> GetByPropertyIdAsync(long propertyId)
        {
            return await _db.UnitPayments
                .Where(x => x.PropertyId == propertyId)
                .Include(x => x.Transactions)
                .ToListAsync();
        }

        public async Task<List<UnitPayments>> GetByTenantIdAsync(long tenantId)
        {
            return await _db.UnitPayments
                .Where(x => x.TenantId == tenantId)
                .Include(x => x.Transactions)
                .ToListAsync();
        }
        public async Task<List<UnitPayments>> GetByUnitIdAsync(long unitId)
        {
            return await _db.UnitPayments
                .Where(x => x.UnitId == unitId)
                .Include(x => x.Transactions)
                .ToListAsync();
        }

        public async Task<UnitPayments?> GetByUnitAndPeriodAsync(long unitId, int month, int year)
        {
            return await _db.UnitPayments
                .Include(x => x.Transactions)
                .FirstOrDefaultAsync(x =>
                    x.UnitId == unitId &&
                    x.PeriodMonth == month &&
                    x.PeriodYear == year);
        }
    }
}
