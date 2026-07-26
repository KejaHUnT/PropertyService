namespace KejaHUnt_PropertiesAPI.Models.Enums
{
    public enum UnitPaymentStatus
    {
        Pending,        // No payment made yet
        Partial,        // Some amount paid
        Paid,           // Fully paid
        Overpaid,       // Paid more than expected
        Failed,         // Payment attempts failed
        Cancelled,      // Payment cancelled manually/admin
        Refunded,       // Payment reversed
        Disputed        // Payment under dispute
    }
}
