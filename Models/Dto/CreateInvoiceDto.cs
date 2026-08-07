namespace KejaHUnt_PropertiesAPI.Models.Dto
{
    public class CreateInvoiceDto
    {
        public long UnitId { get; set; }

        // Needed in case no UnitPayments row exists yet for this period and one must be created
        public long TenantId { get; set; }

        public int PeriodMonth { get; set; }
        public int PeriodYear { get; set; }

        public decimal WaterBillAmount { get; set; }
    }
}