using System.ComponentModel.DataAnnotations;

namespace KejaHUnt_PropertiesAPI.Models.Dto
{
    public class CreateManualUnitPaymentDto
    {
        [Required]
        public long UnitId { get; set; }

        [Required]
        public long PropertyId { get; set; }

        [Required]
        public long TenantId { get; set; }

        [Required]
        public int PeriodMonth { get; set; }

        [Required]
        public int PeriodYear { get; set; }

        [Required]
        public decimal Amount { get; set; }

        public string? PhoneNumber { get; set; }

        [Required]
        [RegularExpression("^(cash|mpesa)$", ErrorMessage = "PaymentType must be 'cash' or 'mpesa'.")]
        public string PaymentType { get; set; } = "cash";

        public string? MpesaCode { get; set; }

        public string? Notes { get; set; }

        [Required]
        public string ApprovedByManagerId { get; set; } = string.Empty;
    }
}