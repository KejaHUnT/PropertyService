namespace KejaHUnt_PropertiesAPI.Models.Dto
{
    public class InitializePaymentRequest
    {
        public string Gateway { get; set; } = string.Empty;
        public string AccountId { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Description { get; set; } = "Payment";
        public string CallbackUrl { get; set; } = string.Empty;
        public string GatewaySecretKey { get; set; } = string.Empty;
    }
}
