using KejaHUnt_PropertiesAPI.Models.Enums;

namespace KejaHUnt_PropertiesAPI.Models.Dto
{
    public class InvoiceDto
    {
        public long Id { get; set; }

        public long UnitId { get; set; }
        public string DoorNumber { get; set; }

        public long PropertyId { get; set; }
        public string PropertyName { get; set; }

        public long TenantId { get; set; }

        public long UnitPaymentsId { get; set; }

        public int PeriodMonth { get; set; }
        public int PeriodYear { get; set; }

        public decimal RentAmount { get; set; }
        public decimal WaterBillAmount { get; set; }
        public decimal TotalAmount { get; set; }

        public UnitPaymentStatus Status { get; set; }

        public DateTime DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}