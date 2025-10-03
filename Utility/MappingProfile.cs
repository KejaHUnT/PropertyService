using AutoMapper;
using KejaHUnt_PropertiesAPI.Models.Domain;
using KejaHUnt_PropertiesAPI.Models.Dto;
using Newtonsoft.Json;

namespace KejaHUnt_PropertiesAPI.Utility
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {


            // Property -> CreatePropertyRequestDto
            CreateMap<Property, CreatePropertyRequestDto>()
                .ForMember(dest => dest.GeneralFeatures, opt => opt.MapFrom(src => src.GeneralFeatures.Select(gf => gf.Id)))
                .ForMember(dest => dest.OutdoorFeatures, opt => opt.MapFrom(src => src.OutdoorFeatures.Select(gf => gf.Id)))
                .ForMember(dest => dest.IndoorFeatures, opt => opt.MapFrom(src => src.IndoorFeatures.Select(gf => gf.Id)))
                .ReverseMap()
                .ForMember(dest => dest.GeneralFeatures, opt => opt.Ignore()) // Manual mapping later
                .ForMember(dest => dest.OutdoorFeatures, opt => opt.Ignore()) // Manual mapping later
                .ForMember(dest => dest.IndoorFeatures, opt => opt.Ignore()); // Manual mapping later


            CreateMap<Unit, CreateUnitRequestDto>().ReverseMap();

            CreateMap<Property, UpdatePropertyRequestDto>()
                 .ForMember(dest => dest.GeneralFeatures, opt => opt.MapFrom(src => src.GeneralFeatures.Select(gf => gf.Id)))
                .ForMember(dest => dest.OutDoorFeatures, opt => opt.MapFrom(src => src.OutdoorFeatures.Select(gf => gf.Id)))
                .ForMember(dest => dest.IndoorFeatures, opt => opt.MapFrom(src => src.IndoorFeatures.Select(gf => gf.Id)))
                .ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.GeneralFeatures, opt => opt.Ignore()) // Manual mapping later
                .ForMember(dest => dest.OutdoorFeatures, opt => opt.Ignore()) // Manual mapping later
                .ForMember(dest => dest.IndoorFeatures, opt => opt.Ignore()) // Manual mapping later
                .ForMember(dest => dest.Units, opt => opt.Ignore()) // Manual mapping later
                .ForMember(dest => dest.PolicyDescriptions, opt => opt.Ignore());


            CreateMap<Unit, UnitDto>().ReverseMap();

            CreateMap<UpdateUnitRequestDto, Unit>().ReverseMap();

            CreateMap<Property, PropertyDto>().ReverseMap();
            CreateMap<Property, PendingProperty>().ReverseMap();
            CreateMap<PendingProperty, PendingPropertyDto>().ReverseMap();

            CreateMap<GeneralFeatures, FeaturesDto>().ReverseMap();
            CreateMap<IndoorFeatures, FeaturesDto>().ReverseMap();
            CreateMap<OutDoorFeatures, FeaturesDto>().ReverseMap();
            CreateMap<CreatePolicyDto, PolicyDescription>().ReverseMap();
            CreateMap<Policy, PolicyDto>().ReverseMap();
            CreateMap<PolicyDescription, PolicydescriptionDto>().ReverseMap();
            CreateMap<PendingPolicyDescription, CreatePolicyDto>().ReverseMap();
            CreateMap<PendingPolicyDescription, PolicydescriptionDto>().ReverseMap();
            CreateMap<PolicyDescription, PendingPolicyDescription>().ReverseMap();
            // Property -> CreatePropertyRequestDto
            CreateMap<PendingProperty, PendingPropertyRequestDto>()
                .ForMember(dest => dest.GeneralFeatures, opt => opt.MapFrom(src => src.GeneralFeatures.Select(gf => gf.Id)))
                .ForMember(dest => dest.OutdoorFeatures, opt => opt.MapFrom(src => src.OutdoorFeatures.Select(gf => gf.Id)))
                .ForMember(dest => dest.IndoorFeatures, opt => opt.MapFrom(src => src.IndoorFeatures.Select(gf => gf.Id)))
                .ReverseMap()
                .ForMember(dest => dest.GeneralFeatures, opt => opt.Ignore()) // Manual mapping later
                .ForMember(dest => dest.OutdoorFeatures, opt => opt.Ignore()) // Manual mapping later
                .ForMember(dest => dest.IndoorFeatures, opt => opt.Ignore()); // Manual mapping later

        }
    }
}
