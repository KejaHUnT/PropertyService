using KejaHUnt_PropertiesAPI.Models.Dto;

namespace KejaHUnt_PropertiesAPI.Services.Tenants
{
    public interface ITenantServiceClient
    {
        Task<TenantInfoDto?> GetActiveTenantByUnitIdAsync(long unitId);
        Task<TenantInfoDto?> GetTenantByIdAsync(long tenantId);
    }
}