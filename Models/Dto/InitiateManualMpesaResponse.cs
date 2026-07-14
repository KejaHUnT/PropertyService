using KejaHUnt_PropertiesAPI.Models.Domain;

namespace KejaHUnt_PropertiesAPI.Models.Dto
{
    public class InitiateManualMpesaResponse
    {
        public long UnitPaymentId { get; set; }
        public InitializePaymentResponse PaymentResponse { get; set; } = null!;
    }
}