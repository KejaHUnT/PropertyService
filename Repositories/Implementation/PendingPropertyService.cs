using System.Net.Http;
using AutoMapper;
using KejaHUnt_PropertiesAPI.Models.Domain;
using KejaHUnt_PropertiesAPI.Models.Dto;
using KejaHUnt_PropertiesAPI.Repositories.Interface;

namespace KejaHUnt_PropertiesAPI.Repositories.Implementation
{
    public class PendingPropertyService : IPendingPropertyService
    {
        private readonly IPendingPropertyRepository _pendingRepo;
        private readonly IPropertyRepository _propertyRepo;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClientFactory;
        private readonly IFeatureRepository _featureRepository;

        public PendingPropertyService(IPendingPropertyRepository pendingRepo, IPropertyRepository propertyRepo, IMapper mapper, HttpClient httpClientFactory,  IFeatureRepository featureRepository, IConfiguration configuration)
        {
            _pendingRepo = pendingRepo;
            _propertyRepo = propertyRepo;
            _mapper = mapper;
            _httpClientFactory = httpClientFactory;
            _featureRepository = featureRepository;
            _configuration = configuration;
        }

        public async Task<PendingPropertyDto> SubmitAsync(PendingPropertyRequestDto dto, string userId, Guid documentId)
        {
            var pendingEntity = _mapper.Map<PendingProperty>(dto);
            pendingEntity.SubmittedByUserId = userId;
            pendingEntity.DocumentId = documentId;
            var property = await _pendingRepo.AddAsync(pendingEntity, dto.OutdoorFeatures, dto.IndoorFeatures, dto.GeneralFeatures);
            return property;
        }

        public async Task<IEnumerable<PendingPropertyRequestDto>> GetAllPendingAsync()
        {
            var pendingEntities = await _pendingRepo.GetAllAsync();
            return _mapper.Map<IEnumerable<PendingPropertyRequestDto>>(pendingEntities);
        }

        public async Task ApproveAsync(long id)
        {
            var pending = await _pendingRepo.GetByIdAsync(id);
            if (pending == null)
                throw new Exception("Pending property not found.");
            var pendingPolicyDescriptions = await _featureRepository.GetPolicyDescriptionByPropertyIdAsync(id);
            var approvedProperty = _mapper.Map<Property>(pending);
            await _propertyRepo.AddAsync(approvedProperty);

            await AssignRoleAsync(pending.Email, "Manager");

            // Map and save policy descriptions with the generated PropertyId
            foreach (var pendingPolicy in pendingPolicyDescriptions)
            {
                var approvedPolicy = new PolicyDescription
                {
                    Name = pendingPolicy.Name,
                    PolicyId = pendingPolicy.PolicyId,
                    PropertyId = approvedProperty.Id // Use the now-generated ID
                };

                await _featureRepository.AddPolicyDescriptionAsync(approvedPolicy);
            }
            await _pendingRepo.DeleteAsync(pending);
        }

        private async Task AssignRoleAsync(string email, string role)
        {
            var accessBaseUrl = _configuration["AccessService:BaseUrl"];
            var endpoint = $"{accessBaseUrl}/api/Auth/assign-role";

            var request = new AddUserToRoleRequestDto
            {
                Email = email,
                RoleName = role
            };

            try
            {
                var response = await _httpClientFactory.PostAsJsonAsync(endpoint, request);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new ApplicationException($"Failed to assign role: {error}");
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error occurred while assigning role: {ex.Message}", ex);
            }
        }
    }
}
