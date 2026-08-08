using KejaHUnt_PropertiesAPI.Data;
using KejaHUnt_PropertiesAPI.Models.Domain;
using KejaHUnt_PropertiesAPI.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace KejaHUnt_PropertiesAPI.Repositories.Implementation
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly ApplicationDbContext _db;

        public InvoiceRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Invoice> CreateAsync(Invoice invoice)
        {
            await _db.Invoices.AddAsync(invoice);
            await _db.SaveChangesAsync();
            return invoice;
        }

        public async Task<Invoice> UpdateAsync(Invoice invoice)
        {
            var existing = await _db.Invoices.FirstOrDefaultAsync(x => x.Id == invoice.Id);

            if (existing == null)
                throw new ArgumentException($"Invoice {invoice.Id} not found");

            existing.WaterBillAmount = invoice.WaterBillAmount;
            existing.RentAmount = invoice.RentAmount;
            existing.TotalAmount = invoice.TotalAmount;

            await _db.SaveChangesAsync();
            return existing;
        }

        public async Task<Invoice> UpdateStatusAsync(Invoice invoice)
        {
            var existing = await _db.Invoices.FirstOrDefaultAsync(x => x.Id == invoice.Id);

            if (existing == null)
                throw new ArgumentException($"Invoice {invoice.Id} not found");

            existing.Status = invoice.Status;

            await _db.SaveChangesAsync();
            return existing;
        }

        public async Task<Invoice?> GetByIdAsync(long id)
        {
            return await _db.Invoices
                .Include(x => x.Unit)
                .Include(x => x.Property)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<Invoice>> GetAllAsync()
        {
            return await _db.Invoices
                .Include(x => x.Unit)
                .Include(x => x.Property)
                .ToListAsync();
        }

        public async Task<List<Invoice>> GetByPropertyIdAsync(long propertyId)
        {
            return await _db.Invoices
                .Where(x => x.PropertyId == propertyId)
                .Include(x => x.Unit)
                .Include(x => x.Property)
                .ToListAsync();
        }

        public async Task<List<Invoice>> GetByUnitIdAsync(long unitId)
        {
            return await _db.Invoices
                .Where(x => x.UnitId == unitId)
                .Include(x => x.Unit)
                .Include(x => x.Property)
                .ToListAsync();
        }

        public async Task<List<Invoice>> GetByTenantIdAsync(long tenantId)
        {
            return await _db.Invoices
                .Where(x => x.TenantId == tenantId)
                .Include(x => x.Unit)
                .Include(x => x.Property)
                .ToListAsync();
        }

        public async Task<Invoice?> GetByUnitAndPeriodAsync(long unitId, int month, int year)
        {
            return await _db.Invoices
                .Include(x => x.Unit)
                .Include(x => x.Property)
                .FirstOrDefaultAsync(x =>
                    x.UnitId == unitId &&
                    x.PeriodMonth == month &&
                    x.PeriodYear == year);
        }

        public async Task<Invoice?> GetByUnitPaymentsIdAsync(long unitPaymentsId)
        {
            return await _db.Invoices
                .FirstOrDefaultAsync(x => x.UnitPaymentsId == unitPaymentsId);
        }
    }
}