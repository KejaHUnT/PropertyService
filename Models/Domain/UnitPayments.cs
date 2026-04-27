using KejaHUnt_PropertiesAPI.Models.enums;

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
        public decimal ExpectedAmount { get; set; } // Rent amount
        public decimal PaidAmount { get; set; }     // Aggregated payments

        public UnitPaymentStatus Status { get; set; } = UnitPaymentStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<PaymentTransaction?> Transactions { get; set; }
    }
}
