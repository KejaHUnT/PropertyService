namespace KejaHUnt_PropertiesAPI.Models.Dto;


    public record SetWaterRateDto(long PropertyId, decimal PricePerUnit, DateTime EffectiveFrom);

    public record UnitReadingInputDto(long UnitId, double CurrentReading);

    public record GenerateWaterBillsRequestDto(
        long PropertyId,
        int BillingYear,
        int BillingMonth,
        List<UnitReadingInputDto> Readings);

    public record WaterBillResultDto(
        long UnitId,
        string DoorNumber,
        double? UnitsConsumed,
        decimal? Amount,
        bool IsBaseline,
        bool AppliedToPayment,   // false if no UnitPayments row existed yet for this period
        string? Error);

    public record GenerateWaterBillsResponseDto(
        int BillsGenerated,
        int Baselined,
        int Failed,
        int NotAppliedToPayment, // generated fine, but no payment record found to attach to
        List<WaterBillResultDto> Results);
