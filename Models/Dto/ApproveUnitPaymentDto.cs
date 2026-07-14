using System.ComponentModel.DataAnnotations;

namespace KejaHUnt_PropertiesAPI.Models.Dto
{
    public class ApproveUnitPaymentDto
    {
        [Required]
        public string ApprovedByManagerId { get; set; } = string.Empty;

        [Required]
        public string MpesaCode { get; set; } = string.Empty;

        public decimal? Amount { get; set; }
    }
}