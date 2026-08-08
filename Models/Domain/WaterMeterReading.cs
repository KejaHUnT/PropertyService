using KejaHUnt_PropertiesAPI.Models.Enums;

namespace KejaHUnt_PropertiesAPI.Models.Domain;

/// <summary>
/// One meter reading for one unit in one billing period. UnitsConsumed is stored
/// (not computed on read) so historical records never drift if reading data is
/// inspected later — it reflects exactly what was billed at generation time.
/// </summary>
public class WaterMeterReading
{
    public long Id { get; set; }

    public long UnitId { get; set; }
    public Unit Unit { get; set; }

    public int BillingYear { get; set; }
    public int BillingMonth { get; set; }

    public double PreviousReading { get; set; }
    public double CurrentReading { get; set; }
    public double UnitsConsumed { get; set; }

    public WaterMeterReadingStatus Status { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    public string? RecordedByUserId { get; set; }

    public WaterBill? Bill { get; set; }
}