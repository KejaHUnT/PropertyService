using Microsoft.EntityFrameworkCore;
using KejaHUnt_PropertiesAPI.Models.Enums;

namespace KejaHUnt_PropertiesAPI.Models.Domain
{
    public class Unit
    {
        public long Id { get; set; }
        [Precision(18, 2)]
        public decimal Price { get; set; }
        public string Type { get; set; }
        public int Bathrooms { get; set; }
        public double Size { get; set; }
        public int Floor { get; set; }
        public string DoorNumber { get; set; }
        public UnitStatus Status { get; set; } = UnitStatus.Available;
        public string? ImageUrl { get; set; }
        public long PropertyId { get; set; }
        public Property Property { get; set; }
        public ICollection<UnitPayments> Payments { get; set; } = new List<UnitPayments>();
    }
}