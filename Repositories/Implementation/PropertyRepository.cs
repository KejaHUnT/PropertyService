using AutoMapper;
using KejaHUnt_PropertiesAPI.Data;
using KejaHUnt_PropertiesAPI.Migrations;
using KejaHUnt_PropertiesAPI.Models.Domain;
using KejaHUnt_PropertiesAPI.Models.Dto;
using KejaHUnt_PropertiesAPI.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace KejaHUnt_PropertiesAPI.Repositories.Implementation
{
    public class PropertyRepository : IPropertyRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IImageRepository _imageRepository;
        private readonly IFeatureRepository _featureRepository;
        private readonly IUnitRepository _unitRepository;

        public PropertyRepository(ApplicationDbContext dbContext, IMapper mapper, IImageRepository imageRepository, IFeatureRepository featureRepository, IUnitRepository unitRepository)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _imageRepository = imageRepository;
            _featureRepository = featureRepository;
            _unitRepository = unitRepository;
        }

        public async Task<Property> AddAsync(Property property)
        {
            await _dbContext.Properties.AddAsync(property);
            await _dbContext.SaveChangesAsync();

            return property;
        }

        public async Task<Property> CreatePropertyAsync(Property property, long[] generalFeatureIds, long[] indoorFeaturesIds, long[] outdoorFeaturesIds)
        {
            // Fetch the actual GeneralFeatures from DB using IDs
            var features = await _dbContext.GeneralFeatures
                .Where(f => generalFeatureIds.Contains(f.Id))
                .ToListAsync();
            var indoorFeatures = await _dbContext.IndoorFeatures
                .Where(f => indoorFeaturesIds.Contains(f.Id))
                .ToListAsync();
            var outDoorFeatures = await _dbContext.OutDoorFeatures
                .Where(f => outdoorFeaturesIds.Contains(f.Id))
                .ToListAsync();

            // Assign features to property
            property.GeneralFeatures = features;
            property.IndoorFeatures = indoorFeatures;
            property.OutdoorFeatures = outDoorFeatures;

            await _dbContext.Properties.AddAsync(property);
            await _dbContext.SaveChangesAsync();

            return property;
        }


        public async Task<Property?> DeleteAync(long id)
        {
            var existingProperty = await _dbContext.Properties.FirstOrDefaultAsync(x => x.Id == id);

            if (existingProperty != null)
            {
                _dbContext.Units.RemoveRange(existingProperty.Units);
                _dbContext.Properties.Remove(existingProperty);
                await _dbContext.SaveChangesAsync();
                return existingProperty;
            }
            return null;
        }

        public async Task<IEnumerable<Property>> GetAllAsync()
        {
            return await _dbContext.Properties.Include(x => x.Units).Include(f => f.IndoorFeatures).Include(f => f.OutdoorFeatures).Include(f => f.GeneralFeatures).Include(p => p.PolicyDescriptions).ToListAsync();
        }

        public async Task<Property?> GetPropertyByIdAsync(long id)
        {
            return await _dbContext.Properties.Include(x => x.Units).Include(f => f.IndoorFeatures).Include(f => f.OutdoorFeatures).Include(f => f.GeneralFeatures).Include(p => p.PolicyDescriptions).FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<Property>> GetPropertyByEmail(string email)
        {
            return await _dbContext.Properties
                .Include(x => x.Units)
                .Include(f => f.IndoorFeatures)
                .Include(f => f.OutdoorFeatures)
                .Include(f => f.GeneralFeatures)
                .Include(p => p.PolicyDescriptions)
                .Where(x => x.Email == email)
                .ToListAsync();
        }


        public async Task<Property?> UpdateAsync(long id, UpdatePropertyRequestDto request)
        {
            var property = await _dbContext.Properties
                .Include(p => p.GeneralFeatures)
                .Include(p => p.IndoorFeatures)
                .Include(p => p.OutdoorFeatures)
                .Include(p => p.Units)
                .Include(p => p.PolicyDescriptions)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (property == null)
                return null;

            // Map scalar fields (e.g., Name, Description, etc.)
            _mapper.Map(request, property);

            // Handle image upload/update
            if (request.ImageFile != null)
            {
                var newDocumentId = (request.DocumentId != null && request.DocumentId != Guid.Empty)
                    ? await _imageRepository.Edit(request.DocumentId.Value, request.ImageFile)
                    : await _imageRepository.Upload(request.ImageFile);

                property.DocumentId = newDocumentId;
            }

            // Clear and reassign feature collections
            property.GeneralFeatures?.Clear();
            property.IndoorFeatures?.Clear();
            property.OutdoorFeatures?.Clear();

            // Fetch and reassign feature entities from database
            var generalFeatures = await _dbContext.GeneralFeatures
                .Where(f => request.GeneralFeatures.Contains(f.Id))
                .ToListAsync();

            var indoorFeatures = await _dbContext.IndoorFeatures
                .Where(f => request.IndoorFeatures.Contains(f.Id))
                .ToListAsync();

            var outdoorFeatures = await _dbContext.OutDoorFeatures
                .Where(f => request.OutDoorFeatures.Contains(f.Id))
                .ToListAsync();

            property.GeneralFeatures = generalFeatures;
            property.IndoorFeatures = indoorFeatures;
            property.OutdoorFeatures = outdoorFeatures;

            // Remove existing PolicyDescriptions from database
            if (property.PolicyDescriptions != null && property.PolicyDescriptions.Any())
            {
                _dbContext.PolicyDescriptions.RemoveRange(property.PolicyDescriptions);
            }

            // Add new PolicyDescriptions
            if (!string.IsNullOrWhiteSpace(request.PolicyDescriptions))
            {
                try
                {
                    var policies = JsonConvert.DeserializeObject<List<CreatePolicyDto>>(request.PolicyDescriptions);

                    if (policies != null && policies.Any())
                    {
                        foreach (var policy in policies)
                        {
                            var newPolicy = new PolicyDescription
                            {
                                Name = policy.Name,
                                PolicyId = policy.PolicyId,
                                PropertyId = property.Id
                            };

                            _dbContext.PolicyDescriptions.Add(newPolicy);
                        }
                    }
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"Failed to deserialize policy descriptions: {ex.Message}");
                }
            }

            if (!string.IsNullOrWhiteSpace(request.Units))
            {
                try
                {
                    var unitDtos = JsonConvert.DeserializeObject<List<UnitDto>>(request.Units);

                    if (unitDtos != null && unitDtos.Any())
                    {
                        for (int i = 0; i < unitDtos.Count; i++)
                        {
                            var unitDto = unitDtos[i];

                            // Map image index from form: "unitImageFiles[0]", "unitImageFiles[1]" etc.
                            var imageKey = $"unitImageFiles[{i}]";

                            if (request.UnitImageFiles != null)
                            {
                                if (unitDto.Id == 0)
                                {
                                    var unitEntity = _mapper.Map<Unit>(unitDto);

                                    var image = request.UnitImageFiles[i];
                                    if (image != null)
                                    {
                                        Guid? documentId = await _imageRepository.Upload(image);
                                        unitEntity.DocumentId = documentId;
                                    }

                                    await _unitRepository.CreateUnitAsync(unitEntity);
                                }
                                else
                                {
                                    // Existing unit: replace image if provided
                                    var existingUnit = await _dbContext.Units.FirstOrDefaultAsync(u => u.Id == unitDto.Id);
                                    var unitEntity = _mapper.Map<Unit>(unitDto);
                                    if(request.UnitImageFiles.Count > i)
                                    {
                                        if (existingUnit != null && existingUnit.DocumentId.HasValue)
                                        {
                                            var image = request.UnitImageFiles[i];
                                            var updatedDocId = await _imageRepository.Edit(existingUnit.DocumentId, image);
                                            unitEntity.DocumentId = updatedDocId;
                                        }
                                        else
                                        {
                                            var image = request.UnitImageFiles[i];
                                            var newDocId = await _imageRepository.Upload(image);
                                            unitEntity.DocumentId = newDocId;
                                        }
                                    }
                                    await _unitRepository.UpdateAsync(unitEntity);
                                }
                            }

                        }
                    }
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"Error parsing units JSON: {ex.Message}");
                }
            }

            await _dbContext.SaveChangesAsync();

            return property;
        }


    }
}
