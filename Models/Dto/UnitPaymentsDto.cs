using KejaHUnt_PropertiesAPI.Models.Enums;

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

        public decimal RentAmount { get; set; }
        public decimal WaterAmount { get; set; }
        public decimal ExpectedAmount { get; set; } // RentAmount + WaterAmount
        public decimal PaidAmount { get; set; }

        /// <summary>True if a WaterBill has been attached to this period's expected amount.</summary>
        public bool IsWaterBilled { get; set; }
        public long? WaterBillId { get; set; }

        public UnitPaymentStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<PaymentTransactionDto> Transactions { get; set; }
    }
}
