using Microsoft.EntityFrameworkCore;

namespace KejaHUnt_PropertiesAPI.Models.Domain
{
    public class Invoice
    {
        public long Id { get; set; }

        public long UnitId { get; set; }
        public Unit Unit { get; set; }

        public long PropertyId { get; set; }
        public Property Property { get; set; }

        // Cross-service reference — tenant service owns the name, we just keep the ID
        public long TenantId { get; set; }

        // Links back to the UnitPayments row this invoice's rent figure came from
        public long UnitPaymentsId { get; set; }
        public UnitPayments UnitPayments { get; set; }

        public int PeriodMonth { get; set; }   // 1–12
        public int PeriodYear { get; set; }    // e.g. 2026

        [Precision(18, 2)]
        public decimal RentAmount { get; set; }      // snapshot of UnitPayments.ExpectedAmount at generation time

        [Precision(18, 2)]
        public decimal WaterBillAmount { get; set; }

        [Precision(18, 2)]
        public decimal TotalAmount { get; set; }     // RentAmount + WaterBillAmount

        public DateTime DueDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}