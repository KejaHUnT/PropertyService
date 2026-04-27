using KejaHUnt_PropertiesAPI.Data;
using KejaHUnt_PropertiesAPI.Models.Domain;
using KejaHUnt_PropertiesAPI.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace KejaHUnt_PropertiesAPI.Repositories.Implementation
{
    public class PaymentTransactionRepository : IPaymentTransactionRepository
    {
        private readonly ApplicationDbContext _db;

        public PaymentTransactionRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<PaymentTransaction> CreateAsync(PaymentTransaction transaction)
        {
            await _db.PaymentTransactions.AddAsync(transaction);
            await _db.SaveChangesAsync();
            return transaction;
        }

        public async Task<PaymentTransaction?> GetByIdAsync(long id)
        {
            return await _db.PaymentTransactions
                .Include(x => x.UnitPayment)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<PaymentTransaction?> GetByReferenceAsync(string reference)
        {
            return await _db.PaymentTransactions
                .Include(x => x.UnitPayment)
                .FirstOrDefaultAsync(x => x.Reference == reference);
        }

        public async Task<List<PaymentTransaction>> GetByUnitPaymentIdAsync(long unitPaymentId)
        {
            return await _db.PaymentTransactions
                .Where(x => x.UnitPaymentId == unitPaymentId)
                .ToListAsync();
        }

        public async Task<PaymentTransaction?> UpdateAsync(PaymentTransaction transaction)
        {
            var existing = await _db.PaymentTransactions
                .FirstOrDefaultAsync(x => x.Id == transaction.Id);

            if (existing == null) return null;

            existing.Status = transaction.Status;
            existing.Reference = transaction.Reference;
            existing.Amount = transaction.Amount;

            await _db.SaveChangesAsync();
            return existing;
        }

        public async Task<PaymentTransaction?> DeleteAsync(long id)
        {
            var existing = await _db.PaymentTransactions
                .FirstOrDefaultAsync(x => x.Id == id);

            if (existing == null) return null;

            _db.PaymentTransactions.Remove(existing);
            await _db.SaveChangesAsync();
            return existing;
        }
    }
}
