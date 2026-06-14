using KejaHUnt_PropertiesAPI.Models.Domain;

namespace KejaHUnt_PropertiesAPI.Models.Dto
{
    public class PropertyDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string? ImageUrl { get; set; }
        public List<UnitDto> Units { get; set; } = new List<UnitDto>();
        public List<FeaturesDto> GeneralFeatures { get; set; } = new List<FeaturesDto>();
        public List<FeaturesDto> IndoorFeatures { get; set; } = new List<FeaturesDto>();
        public List<FeaturesDto> OutDoorFeatures { get; set; } = new List<FeaturesDto>();
        public List<PolicydescriptionDto> PolicyDescriptions { get; set; } = new List<PolicydescriptionDto>();
    }
}