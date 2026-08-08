using KejaHUnt_PropertiesAPI.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace KejaHUnt_PropertiesAPI.Models.Domain;

/// <summary>
/// The billable outcome of a meter reading. PricePerUnit and Amount are snapshotted
/// at generation time — if the property's WaterRate changes later, past bills stay
/// exactly as they were charged.
/// </summary>
public class WaterBill
{
    public long Id { get; set; }

    public long WaterMeterReadingId { get; set; }
    public WaterMeterReading Reading { get; set; }

    public long UnitId { get; set; }
    public Unit Unit { get; set; }

    [Precision(18, 2)]
    public decimal PricePerUnit { get; set; }

    [Precision(18, 2)]
    public decimal Amount { get; set; }

    public int BillingYear { get; set; }
    public int BillingMonth { get; set; }

    public WaterBillStatus Status { get; set; } = WaterBillStatus.Unpaid;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    // Set once a matching UnitPayments record exists for this unit+period.
    // Null means: bill exists, but no payment record to attach it to yet.
    public long? UnitPaymentsId { get; set; }
    public UnitPayments? UnitPayments { get; set; }
}