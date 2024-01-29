

using PlantHarvest.Contract.Query;

namespace PlantHarvest.Domain.PlantTaskAggregate;

public interface IPlantTaskRepository : IRepository<PlantTask>
{
    Task<IReadOnlyCollection<PlantTaskViewModel>> SearchPlantTasksForUser(PlantTaskSearch search, string userProfileId);
    Task<IReadOnlyCollection<PlantTaskViewModel>> GetPlantTasksForUser(string userProfileId);
    Task<IReadOnlyCollection<PlantTaskViewModel>> GetActivePlantTasksForUser(string userProfileId);
    Task<long> GetNumberOfCompletedTasksForUser(string userProfileId, string harvestCycelId);
}
