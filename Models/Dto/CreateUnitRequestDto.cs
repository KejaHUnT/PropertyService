namespace KejaHUnt_PropertiesAPI.Models.Dto
{
    public class CreateUnitRequestDto
    {
        public decimal Price { get; set; }
        public string Type { get; set; }
        public int Bathrooms { get; set; }
        public double Size { get; set; }
        public int Floor { get; set; }
        public string Doornumber { get; set; }
        public string Status { get; set; }
        public long PropertyId { get; set; }
    }
}
