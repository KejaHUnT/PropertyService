using KejaHUnt_PropertiesAPI.Models.Domain;
using KejaHUnt_PropertiesAPI.Models.Dto;

namespace KejaHUnt_PropertiesAPI.Services.WaterBills;

public interface IWaterBillingService
{
    Task<WaterRate> SetPropertyRateAsync(SetWaterRateDto dto);
    Task<List<WaterRate>> GetRateHistoryAsync(long propertyId);

    Task<GenerateWaterBillsResponseDto> GenerateMonthlyBillsAsync(GenerateWaterBillsRequestDto request);

    Task<List<WaterBill>> GetPropertyBillsAsync(long propertyId, int year, int month);
    Task<List<WaterMeterReading>> GetUnitReadingHistoryAsync(long unitId);
}