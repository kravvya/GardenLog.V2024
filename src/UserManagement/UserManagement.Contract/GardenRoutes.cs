namespace UserManagement.Contract;

public class GardenRoutes
{
    public const string GardenBase = "/v1/api/Gardens";
    public const string GetGarden = GardenBase + "/{gardenId}";
    public const string GetGardenByName = GardenBase + "/name/{gardenName}";
    public const string GetGardens = GardenBase;
    public const string GetAllGardens = GardenBase + "/all";
    public const string CreateGarden = GardenBase;
    public const string UpdateGarden = GardenBase + "/{gardenId}";
    public const string DeleteGarden = GardenBase + "/{gardenId}";

    public const string GetGardenBeds = GardenBase + "/{gardenId}/GardenBeds";
    public const string GetGardenBed = GardenBase + "/{gardenId}/GardenBeds/{id}";
    public const string CreateGardenBed = GardenBase + "/{gardenId}/GardenBeds";
    public const string UpdateGardenBed = GardenBase + "/{gardenId}/GardenBeds/{id}";
    public const string DeleteGardenBed = GardenBase + "/{gardenId}/GardenBeds/{id}";
}
