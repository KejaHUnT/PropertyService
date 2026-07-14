public class PendingUnitPaymentDto
{
    public long ManualPaymentId { get; set; }
    public long UnitPaymentId { get; set; }
    public long UnitId { get; set; }
    public long TenantId { get; set; }
    public long PropertyId { get; set; }
    public int PeriodMonth { get; set; }
    public int PeriodYear { get; set; }
    public string Reference { get; set; } = string.Empty;
    public decimal? TenantAmount { get; set; }
    public string? TenantRawSms { get; set; }
    public DateTime? TenantSubmittedAt { get; set; }
}