using System.ComponentModel.DataAnnotations;

namespace KejaHUnt_PropertiesAPI.Models.Dto
{
    public class InitiateManualMpesaDto
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
    }
}