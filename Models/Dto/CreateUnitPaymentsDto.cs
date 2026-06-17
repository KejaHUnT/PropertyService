namespace KejaHUnt_PropertiesAPI.Models.Dto
{
    public class CreateUnitPaymentsDto
    {
        public long UnitId { get; set; }
        public long PropertyId { get; set; }
        public long TenantId { get; set; }

        public string UserEmail { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;

        public decimal Amount { get; set; }
        public string Currency { get; set; } = "KES";

        public int PeriodMonth { get; set; }
        public int PeriodYear { get; set; }

        public string Gateway { get; set; } = "paystack";
        public string AccountId { get; set; } = string.Empty;
        
        public string? CallbackUrl { get; set; }
    }
}
