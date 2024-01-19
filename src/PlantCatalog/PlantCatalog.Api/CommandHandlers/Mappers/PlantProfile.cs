using AutoMapper;

namespace PlantCatalog.Api.CommandHandlers.Mappers;

public class PlantProfile : Profile
{
    public PlantProfile()
    {
        CreateMap<Plant, PlantViewModel>()
            .ForMember(dest => dest.PlantId, opt => opt.MapFrom(src =>src.Id));

        CreateMap<PlantVariety, PlantVarietyViewModel>()
           .ForMember(dest => dest.PlantVarietyId, opt => opt.MapFrom(src => src.Id));

        //CreateMap<PlantGrowInstruction, PlantGrowInstructionViewModel>()
        //    .ForMember(dest => dest.PlantGrowInstructionId, opt => opt.MapFrom(src => src.Id));
    }
}
