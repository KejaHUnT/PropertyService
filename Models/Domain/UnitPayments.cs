using KejaHUnt_PropertiesAPI.Models.Enums;

namespace KejaHUnt_PropertiesAPI.Models.Domain
{
    public class UnitPayments
    {
        public long Id { get; set; }
        public long UnitId { get; set; }
        public long PropertyId { get; set; }
        public long TenantId { get; set; }
        public int PeriodMonth { get; set; }   // 1–12
        public int PeriodYear { get; set; }    // 2024
        public Unit Unit { get; set; }
        public Property Property { get; set; }

        public decimal RentAmount { get; set; }    // rent portion only
        public decimal WaterAmount { get; set; }   // water portion only, 0 if not billed yet
        public decimal ExpectedAmount { get; set; } // RentAmount + WaterAmount — kept in sync via RecalculateExpectedAmount()

        public decimal PaidAmount { get; set; }     // Aggregated payments

        public UnitPaymentStatus Status { get; set; } = UnitPaymentStatus.Pending;

        public long? WaterBillId { get; set; }
        public WaterBill? WaterBill { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<PaymentTransaction?> Transactions { get; set; }

        /// <summary>
        /// Single point of truth for ExpectedAmount. Call after mutating RentAmount
        /// or WaterAmount so the two never drift out of sync.
        /// </summary>
        public void RecalculateExpectedAmount() => ExpectedAmount = RentAmount + WaterAmount;
    }
}
