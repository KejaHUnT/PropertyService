namespace KejaHUnt_PropertiesAPI.Models.Dto
{
    public class UpdateWaterBillDto
    {
        public long UnitId { get; set; }
        public int PeriodMonth { get; set; }
        public int PeriodYear { get; set; }
        public decimal WaterBillAmount { get; set; }
    }
}