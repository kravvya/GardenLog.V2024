using GardenLog.SharedInfrastructure.Extensions;
using UserManagement.Contract;
using UserManagement.Contract.Command;
using UserManagement.Contract.ViewModels;

namespace UserManagement.IntegrationTest.Clients;

public class GardenClient
{
    private readonly Uri _baseUrl;
    private readonly HttpClient _httpClient;

    public GardenClient(Uri baseUrl, HttpClient httpClient)
    {
        _baseUrl = baseUrl;
        _httpClient = httpClient;
    }

    #region Garden
    public async Task<HttpResponseMessage> CreateGarden(string name)
    {
        var url = $"{this._baseUrl.OriginalString}{GardenRoutes.CreateGarden}/";

        var createGardenCommand = PopulateCreateGardenCommand(name);

        using var requestContent = createGardenCommand.ToJsonStringContent();

        return await this._httpClient.PostAsync(url, requestContent);

    }

    public async Task<HttpResponseMessage> UpdateGarden(GardenViewModel harvest)
    {
        var url = $"{this._baseUrl.OriginalString}{GardenRoutes.UpdateGarden}";

        using var requestContent = harvest.ToJsonStringContent();

        return await this._httpClient.PutAsync(url.Replace("{gardenId}", harvest.GardenId), requestContent);
    }

    public async Task<HttpResponseMessage> DeleteGarden(string id)
    {
        var url = $"{this._baseUrl.OriginalString}{GardenRoutes.DeleteGarden}";

        return await this._httpClient.DeleteAsync(url.Replace("{gardenId}", id));
    }

    public async Task<HttpResponseMessage> GetAllGardens()
    {
        var url = $"{this._baseUrl.OriginalString}{GardenRoutes.GetGardens}/";
        return await this._httpClient.GetAsync(url);
    }

    public async Task<HttpResponseMessage> GetGarden(string id)
    {
        var url = $"{this._baseUrl.OriginalString}{GardenRoutes.GetGarden}";
        return await this._httpClient.GetAsync(url.Replace("{gardedId}", id));
    }

    public async Task<HttpResponseMessage> GetGardenByName(string gardenName)
    {
        var url = $"{this._baseUrl.OriginalString}{GardenRoutes.GetGardenByName}";
        return await this._httpClient.GetAsync(url.Replace("{gardenName}", gardenName));
    }

    private static CreateGardenCommand PopulateCreateGardenCommand(string name)
    {
        return new CreateGardenCommand()
        {
            Name = name,
            City = "Mound",
            StateCode = "MN",
            Latitude = -1,
            Longitude = -1,
            Notes = "Integration test garden",
            LastFrostDate = DateTime.Parse("05/15/1900"),
            FirstFrostDate = DateTime.Parse("09/15/1900"),
            WarmSoilDate = DateTime.Parse("06/01/1900")
        };
    }
    #endregion

    #region Garden Bed

    public async Task<HttpResponseMessage> CreateGardenBed(string gardenId, string name)
    {
        var url = $"{this._baseUrl.OriginalString}{GardenRoutes.CreateGardenBed}";

        var createGardenBedCommand = PopulateCreateGardenBedCommand(gardenId, name);

        using var requestContent = createGardenBedCommand.ToJsonStringContent();

        return await this._httpClient.PostAsync(url, requestContent);

    }

    public async Task<HttpResponseMessage> UpdateGardenBed(GardenBedViewModel garden)
    {
        var url = $"{this._baseUrl.OriginalString}{GardenRoutes.UpdateGardenBed}";

        using var requestContent = garden.ToJsonStringContent();

        return await this._httpClient.PutAsync(url.Replace("{gardenId}", garden.GardenId).Replace("{id}", garden.GardenBedId), requestContent);
    }

    public async Task<HttpResponseMessage> DeleteGardenBed(string gardenId, string id)
    {
        var url = $"{this._baseUrl.OriginalString}{GardenRoutes.DeleteGardenBed}";

        return await this._httpClient.DeleteAsync(url.Replace("{gardenId}", gardenId).Replace("{id}", id));
    }

    public async Task<HttpResponseMessage> GetGardenBeds(string gardenId)
    {
        var url = $"{this._baseUrl.OriginalString}{GardenRoutes.GetGardenBeds}";
        return await this._httpClient.GetAsync(url.Replace("{gardenId}", gardenId));
    }

    public async Task<HttpResponseMessage> GetGardenBed(string gardenId, string id)
    {
        var url = $"{this._baseUrl.OriginalString}{GardenRoutes.GetGardenBed}";
        return await this._httpClient.GetAsync(url.Replace("{gardenId}", gardenId).Replace("{id}", id));
    }

    private static CreateGardenBedCommand PopulateCreateGardenBedCommand(string gardenId, string name)
    {
        return new CreateGardenBedCommand()
        {
            GardenId = gardenId,
            //BorderColor = "purple",
            Name = name,
            Length = 20,
            Width = 3,
            RowNumber = 1,
            Type = Contract.Enum.GardenBedTypeEnum.InGroundBed,
            X = 1,
            Y = 1,
            Notes = "Created by Integration test"
        };
    }
    #endregion
}
