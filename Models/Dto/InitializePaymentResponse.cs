namespace KejaHUnt_PropertiesAPI.Models.Dto
{
    public class InitializePaymentResponse
    {
        public long Id { get; set; }
        public string Reference { get; set; } = string.Empty;
        public string GatewayReference { get; set; } = string.Empty;
        public string? CheckoutRequestId { get; set; }
        public string PaymentUrl { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Gateway { get; set; } = string.Empty;
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
