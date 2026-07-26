using KejaHUnt_PropertiesAPI.Models.Enums;

namespace KejaHUnt_PropertiesAPI.Models.Domain
{
    public class PaymentTransaction
    {
        public long Id { get; set; }

        public long UnitPaymentId { get; set; }
        public UnitPayments UnitPayment { get; set; }

        public long ExternalPaymentId { get; set; } // From gateway

        public decimal Amount { get; set; }

        public PaymentTransactionStatus Status { get; set; } = PaymentTransactionStatus.Initialized;

        public string? Reference { get; set; } // gateway ref

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
