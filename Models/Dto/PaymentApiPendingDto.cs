namespace KejaHUnt_PropertiesAPI.Models.Dto
{
    public class PaymentApiPendingDto
    {
        public long Id { get; set; }
        public string Reference { get; set; } = string.Empty;
        public string? MpesaCode { get; set; }
        public decimal? TenantAmount { get; set; }
        public string? TenantRawSms { get; set; }
        public DateTime? TenantSubmittedAt { get; set; }
    }
}