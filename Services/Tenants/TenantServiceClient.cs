using System.Text.Json;
using KejaHUnt_PropertiesAPI.Models.Dto;

namespace KejaHUnt_PropertiesAPI.Services.Tenants
{
    public class TenantServiceClient : ITenantServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly ILogger<TenantServiceClient> _logger;

        public TenantServiceClient(
            HttpClient httpClient,
            IConfiguration config,
            ILogger<TenantServiceClient> logger)
        {
            _httpClient = httpClient;
            _config = config;
            _logger = logger;
        }

        public async Task<TenantInfoDto?> GetActiveTenantByUnitIdAsync(long unitId)
        {
            var baseUrl = _config["TenantService:BaseUrl"];
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"{baseUrl}/api/tenant/by-unit/{unitId}");

            var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogInformation("No active tenant found for unit {UnitId}", unitId);
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("Tenant service call failed for unit {UnitId}: {Body}", unitId, errorBody);
                throw new Exception($"Tenant service error: {errorBody}");
            }

            var body = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TenantInfoDto>(
                body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        public async Task<TenantInfoDto?> GetTenantByIdAsync(long tenantId)
        {
            var baseUrl = _config["TenantService:BaseUrl"];
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"{baseUrl}/api/tenant/{tenantId}");
            var response = await _httpClient.SendAsync(request);
        
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogInformation("No tenant found for id {TenantId}", tenantId);
                return null;
            }
        
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("Tenant service call failed for tenant {TenantId}: {Body}", tenantId, errorBody);
                throw new Exception($"Tenant service error: {errorBody}");
            }
        
            var body = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TenantInfoDto>(
                body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
    }
}