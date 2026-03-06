using KejaHUnt_PropertiesAPI.Models.Domain;
using Microsoft.AspNetCore.Mvc;

namespace KejaHUnt_PropertiesAPI.Models.Dto
{
    public class UpdatePropertyRequestDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        [FromForm(Name = "generalFeatures")]
        public List<long?> GeneralFeatures { get; set; } = new();

        [FromForm(Name = "indoorFeatures")]
        public List<long?> IndoorFeatures { get; set; } = new();

        [FromForm(Name = "outdoorFeatures")]
        public List<long?> OutDoorFeatures { get; set; } = new();

        [FromForm(Name = "policyDescriptions")]
        public string? PolicyDescriptions { get; set; } // JSON stringified array
        public Guid? DocumentId { get; set; }
        [FromForm(Name = "imageFile")]
        public IFormFile? ImageFile { get; set; } // Attach the file here
        [FromForm(Name = "units")]
        public string? Units { get; set; }
        [FromForm(Name="unitImageFiles")]
        public List<IFormFile>? UnitImageFiles { get; set; } = new();
    }
}

