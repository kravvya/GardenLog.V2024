
namespace PlantHarvest.Domain.HarvestAggregate;


public interface IPlantHarvestCycleRepository : IRepository<PlantHarvestCycle>
{
    void AddPlantHarvestCycle(string plantHarvestCyclceId, HarvestCycle harvestCyclce);
    void AddPlantSchedule(string plantCalendarId, string plantHarvestCycleId, HarvestCycle harvestCyclce);
    void DeletePlantHarvestCycle(string plantHarvestCyclceId, HarvestCycle harvestCyclce);
    void DeletePlantSchedule(string plantCalendarId, string plantHarvestCycleId, HarvestCycle harvestCyclce);
    Task<List<PlantHarvestCycle>> GetPlantHarvestCyclesByHarvestCyleId(string harvestCycleId);
    void UpdatePlantHarvestCycle(string plantHarvestCyclceId, HarvestCycle harvestCyclce);
    void UpdatePlantSchedule(string plantCalendarId, string plantHarvestCycleId, HarvestCycle harvestCyclce);
    Task<PlantHarvestCycleViewModel> GetPlantHarvestCycleView(string harvestCycleId, string id);
    Task<IReadOnlyCollection<PlantHarvestCycleViewModel>> GetPlantHarvestCycleViews(string harvestCycleId);
    Task<IReadOnlyCollection<PlantHarvestCycleIdentityOnlyViewModel>> GetPlantHarvestCyclesByPlantId(string plantId);
    void DeletePlantHarvestCycle(string harvestCyclceId);
}
