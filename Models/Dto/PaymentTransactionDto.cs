using KejaHUnt_PropertiesAPI.Models.Enums;

namespace KejaHUnt_PropertiesAPI.Models.Dto
{
    public class PaymentTransactionDto
    {
        public long Id { get; set; }
        public long ExternalPaymentId
        {
            get; set;
        }
        public decimal Amount { get; set; }
        public PaymentTransactionStatus Status { get; set; }
        public string? Reference { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
