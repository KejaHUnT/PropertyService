namespace KejaHUnt_PropertiesAPI.Models.Enums;

public enum WaterMeterReadingStatus
{
    Confirmed,
    Baseline // first-ever reading for a unit — no bill generated, just establishes a starting point
}

public enum WaterBillStatus
{
    Unpaid,
    Paid,
    PartiallyPaid,
    Waived
}