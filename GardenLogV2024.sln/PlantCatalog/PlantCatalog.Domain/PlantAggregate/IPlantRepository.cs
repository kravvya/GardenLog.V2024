using PlantCatalog.Contract.ViewModels;

namespace PlantCatalog.Domain.PlantAggregate;

public interface IPlantRepository : IRepository<Plant>
{
    void AddPlantGrowInstruction(string growInstructionId, Plant plant);
    void DeletePlantGrowInstruction(string growInstructionId, Plant plant);
    void UpdatePlantGrowInstruction(string growInstructionId, Plant plant);

    Task<bool> ExistsAsync(string plantId);
    Task<IReadOnlyCollection<PlantViewModel>> GetAllPlants();
    Task<IReadOnlyCollection<PlantNameOnlyViewModel>> GetAllPlantNames();
    Task<Plant> GetByNameAsync(string plantName);
    Task<string> GetIdByNameAsync(string plantName);

    Task<PlantGrowInstructionViewModel> GetPlantGrowInstraction(string plantId, string id);
    Task<IReadOnlyCollection<PlantGrowInstructionViewModel>> GetPlantGrowInstractions(string plantId);
    
}
