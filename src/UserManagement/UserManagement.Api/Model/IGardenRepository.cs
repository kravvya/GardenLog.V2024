namespace UserManagement.Api.Model;

public interface IGardenRepository :  IRepository<Garden>
{
    Task<GardenViewModel> GetGarden(string gardenId);
    Task<IReadOnlyCollection<GardenViewModel>> GetGardens(string userProfileId);
    Task<IReadOnlyCollection<GardenViewModel>> GetAllGardens();
    Task<GardenViewModel> GetGardenByName(string gardenName, string userProfileId);

    void AddGardenBed(string gardenBedId, Garden garden);
    void DeleteGardenBed(string gardenBedId, Garden garden);
    void UpdateGardenBed(string gardenBedId, Garden garden);

    Task<GardenBedViewModel?> GetGardenBed(string gardenId, string id);
    Task<IReadOnlyCollection<GardenBedViewModel>> GetGardenBeds(string gardenId);
    Task<string> GetIdByNameAsync(string name, string userProfileId);
    Task<Garden> ReadGarden(string gardenId, string userProfileId);
}
