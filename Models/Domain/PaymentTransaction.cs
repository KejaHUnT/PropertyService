using KejaHUnt_PropertiesAPI.Models.Enums;
namespace KejaHUnt_PropertiesAPI.Models.Domain
{
    public class PaymentTransaction
    {
        public long Id { get; set; }
        public long UnitPaymentId { get; set; }
        public UnitPayments UnitPayment { get; set; }
        public long ExternalPaymentId { get; set; }
        public decimal Amount { get; set; }
        public PaymentTransactionStatus Status { get; set; } = PaymentTransactionStatus.Initialized;
        public string? Reference { get; set; }
        public string? MpesaCode { get; set; }   // manual-entry Mpesa confirmation code, when PaymentType == "mpesa"
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}