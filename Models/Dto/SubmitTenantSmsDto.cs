using System.ComponentModel.DataAnnotations;

namespace KejaHUnt_PropertiesAPI.Models.Dto
{
    public class SubmitTenantSmsDto
    {
        [Required]
        public string RawSms { get; set; } = string.Empty;

        [Required]
        public decimal Amount { get; set; }
    }
}