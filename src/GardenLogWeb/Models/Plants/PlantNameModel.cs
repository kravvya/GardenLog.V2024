namespace GardenLogWeb.Models.Plants;

public record PlantNameModel : PlantNameOnlyViewModel
{
    public int NumberOfPlants { get; set; }
}
