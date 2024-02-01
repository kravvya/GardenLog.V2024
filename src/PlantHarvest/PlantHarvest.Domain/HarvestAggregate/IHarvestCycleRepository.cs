namespace PlantHarvest.Domain.HarvestAggregate;


public interface IHarvestCycleRepository : IRepository<HarvestCycle>
{    
    Task<String> GetIdByNameAsync(string harvestCycleName, string userProfileId);
    Task<IReadOnlyCollection<HarvestCycleViewModel>> GetAllHarvestCycles(string userProfileId);
    Task<HarvestCycle> ReadHarvestCycle(string harvestCycleId, string userProfileId);
    Task<HarvestCycle> ReadHarvestCycle(string harvestCycleId, string plantHarvestCycleId, string userProfileId);
}
