using AutoMapper;

namespace PlantHarvest.Api.QueryHandlers.Mappers;

public class HarvestProfile : Profile
{
    public HarvestProfile()
    {
        CreateMap<HarvestCycle, HarvestCycleViewModel>()
            .ForMember(dest => dest.HarvestCycleId, opt => opt.MapFrom(src =>src.Id));

        //CreateMap<PlantVariety, PlantVarietyViewModel>()
        //   .ForMember(dest => dest.PlantVarietyId, opt => opt.MapFrom(src => src.Id));

        //CreateMap<PlantGrowInstruction, PlantGrowInstructionViewModel>()
        //    .ForMember(dest => dest.PlantGrowInstructionId, opt => opt.MapFrom(src => src.Id));
    }
}
