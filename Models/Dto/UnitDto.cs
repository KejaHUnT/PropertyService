using KejaHUnt_PropertiesAPI.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace KejaHUnt_PropertiesAPI.Models.Dto
{
    public class UnitDto
    {
        public long Id { get; set; }
        [Precision(18, 2)]
        public decimal Price { get; set; }
        public string Type { get; set; }
        public int Bathrooms { get; set; }
        public double Size { get; set; }
        public int Floor { get; set; }
        public string DoorNumber { get; set; }
        public UnitStatus Status { get; set; }
        public long PropertyId { get; set; }
        public string? ImageUrl { get; set; }
    }
}