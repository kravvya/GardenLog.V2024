

namespace PlantHarvest.UnitTest;

internal class UserManagementHelper
{
    public const string GARDEN_ID = "TestGarden";
    public const string USER_PROFILE_ID = "TestUserId";

    public static string GetGardenAsString()
    {
        var garden = UserManagementHelper.GetGarden();

        return JsonSerializer.Serialize(garden, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }

    public static GardenViewModel GetGarden()
    {
       return new GardenViewModel()
        {
            GardenId = GARDEN_ID,
            Name = "Unit test garden",
            City = "Mound",
            StateCode = "MN",
            Latitude = 44.9366M,
            Longitude = 93.6661M,
            Notes = "Integration test garden",
            LastFrostDate = DateTime.Parse("05/15/1900"),
            FirstFrostDate = DateTime.Parse("09/15/1900")
        };
    }
}
