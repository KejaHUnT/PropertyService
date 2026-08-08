using KejaHUnt_PropertiesAPI.Models.Domain;

namespace KejaHUnt_PropertiesAPI.Repositories.Interface;

public interface IWaterBillRepository
{
    Task<WaterBill> CreateAsync(WaterBill bill);
    Task<WaterBill?> GetByIdAsync(long id);
    Task<List<WaterBill>> GetByUnitIdAsync(long unitId);
    Task<List<WaterBill>> GetByPropertyAndPeriodAsync(long propertyId, int month, int year);
}