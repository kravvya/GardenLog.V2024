using PlantCatalog.Contract.ViewModels;

namespace PlantCatalog.Domain.PlantAggregate;

public interface IPlantVarietyRepository : IRepository<PlantVariety>
{
    Task<IReadOnlyCollection<PlantVarietyViewModel>> GetPlantVarieties(string plantId);
    Task<PlantVariety> GetByNameAsync(string plantId, string plantName);
    Task<long> GetCountOfPlantVarieties(string plantId);
    Task<string> GetIdByNameAsync(string plantId, string plantName);
    Task<IReadOnlyCollection<PlantVarietyViewModel>> GetPlantVarieties();
}
