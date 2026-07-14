namespace KejaHUnt_PropertiesAPI.Models.Dto
{
    public class PendingUnitPaymentDto
    {
        public long ManualPaymentId { get; set; }   // payment API's ManualRentPayment.Id — needed for approve call
        public long UnitPaymentId { get; set; }
        public long UnitId { get; set; }
        public long TenantId { get; set; }
        public long PropertyId { get; set; }
        public string Reference { get; set; } = string.Empty;
        public decimal? TenantAmount { get; set; }
        public string? TenantRawSms { get; set; }
        public DateTime? TenantSubmittedAt { get; set; }
    }
}