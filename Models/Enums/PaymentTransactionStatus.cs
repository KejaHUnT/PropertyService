namespace KejaHUnt_PropertiesAPI.Models.Enums
{
    public enum PaymentTransactionStatus
    {
        Initialized,    // Created but not sent to gateway
        Pending,        // Sent to gateway, awaiting response
        Processing,     // Gateway is processing (MPESA/Paystack delay)
        Success,        // Payment successful
        Failed,         // Payment failed
        Cancelled,      // User cancelled
        Timeout,        // No response from gateway
        Reversed,       // Reversal done (e.g., MPESA reversal)
        Refunded        // Refund issued
    }
}
