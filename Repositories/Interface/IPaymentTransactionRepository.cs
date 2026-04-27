using KejaHUnt_PropertiesAPI.Models.Domain;

namespace KejaHUnt_PropertiesAPI.Repositories.Interface
{
    public interface IPaymentTransactionRepository
    {
        Task<PaymentTransaction> CreateAsync(PaymentTransaction transaction);

        Task<PaymentTransaction?> GetByIdAsync(long id);
        Task<PaymentTransaction?> GetByReferenceAsync(string reference);

        Task<List<PaymentTransaction>> GetByUnitPaymentIdAsync(long unitPaymentId);

        Task<PaymentTransaction?> UpdateAsync(PaymentTransaction transaction);

        Task<PaymentTransaction?> DeleteAsync(long id);
    }
}
