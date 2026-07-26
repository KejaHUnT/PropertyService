using KejaHUnt_PropertiesAPI.Models.Enums;

namespace KejaHUnt_PropertiesAPI.Models.Dto
{
    public class UnitStatusDto
    {
        public long UnitId { get; set; }
        public UnitStatus Status { get; set; }
    }
}