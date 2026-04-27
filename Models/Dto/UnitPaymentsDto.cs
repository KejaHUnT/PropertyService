using KejaHUnt_PropertiesAPI.Models.enums;

namespace KejaHUnt_PropertiesAPI.Models.Dto
{
    public class UnitPaymentsDto
    {
        public long Id { get; set; }
        public long UnitId { get; set; }
        public long PropertyId { get; set; }
        public long TenantId { get; set; }

        public int PeriodMonth { get; set; }
        public int PeriodYear { get; set; }

        public decimal ExpectedAmount { get; set; }
        public decimal PaidAmount { get; set; }

        public UnitPaymentStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<PaymentTransactionDto> Transactions { get; set; }
    }
}
