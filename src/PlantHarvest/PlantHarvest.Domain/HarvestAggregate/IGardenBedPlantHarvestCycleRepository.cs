

namespace PlantHarvest.Domain.HarvestAggregate;


public interface IGardenBedPlantHarvestCycleRepository : IRepository<GardenBedPlantHarvestCycle>
{
    void AddGardenBedPlantHarvestCycle(string gardenBedPlantHarvestCycleId, string plantHarvestCyclceId, HarvestCycle harvestCyclce);
    void DeleteGardenBedPlantHarvestCycle(string gardenBedPlantHarvestCycleId, string plantHarvestCyclceId, HarvestCycle harvestCyclce);
    void DeleteGardenBedPlantHarvestCycle(string harvestCyclceId);
    void DeleteGardenBedPlantHarvestCycle(string plantHarvestCyclceId, HarvestCycle harvestCyclce);
    Task<IReadOnlyCollection<GardenBedPlantHarvestCycle>> GetGardenBedsByHarvestCycleId(string harvestCycleId);
    Task<IReadOnlyCollection<GardenBedPlantHarvestCycle>> GetGardenBedsByHarvestCycleId(string harvestCycleId, string? plantHarvestCycleId);
    Task<IReadOnlyCollection<GardenBedPlantHarvestCycleViewModel>> GetGardenBedViewsByGardenBedId(string gardenId, string gardenBedId);
    Task<IReadOnlyCollection<GardenBedPlantHarvestCycleViewModel>> GetGardenBedViewsByHarvestCycleId(string harvestCycleId);
    Task<IReadOnlyCollection<GardenBedPlantHarvestCycleViewModel>> GetGardenBedViewsByPlantHarvestCycleId(string harvestCycleId, string plantHarvestCycleId);
    void UpdateGardenBedPlantHarvestCycle(string gardenBedPlantHarvestCycleId, string plantHarvestCyclceId, HarvestCycle harvestCyclce);
}
