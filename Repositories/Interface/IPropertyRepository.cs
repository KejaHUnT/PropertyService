using KejaHUnt_PropertiesAPI.Models.Domain;
using KejaHUnt_PropertiesAPI.Models.Dto;

namespace KejaHUnt_PropertiesAPI.Repositories.Interface
{
    public interface IPropertyRepository
    {
        Task<Property> CreatePropertyAsync(Property property, long[] generalFeatureIds, long[] indoorFeaturesIds, long[] outdoorFeaturesIds);
        Task<Property> AddAsync(Property property);
        Task<IEnumerable<Property>> GetAllAsync();

        Task<Property?> GetPropertyByIdAsync(long id);
        Task<IEnumerable<Property?>> GetPropertyByEmail(string email);

        Task<Property?> UpdateAsync(long id,UpdatePropertyRequestDto request);

        Task<Property?> UpdateShowPriceAsync(long id, bool showPrice);

        Task<Property?> DeleteAync(long id);
    }
}
