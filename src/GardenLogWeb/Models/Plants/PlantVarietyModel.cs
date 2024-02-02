namespace GardenLogWeb.Models.Plants;

public record PlantVarietyModel : PlantVarietyViewModel
{
    public List<ImageViewModel>? Images { get; set; }
    public string ImageLabel { get; set; } = string.Empty;
    public string ImageFileName { get; set; } = string.Empty;
}