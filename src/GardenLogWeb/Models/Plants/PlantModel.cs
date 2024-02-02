namespace GardenLogWeb.Models.Plants;

public record PlantModel : PlantViewModel
{
    public List<ImageViewModel>? Images { get; set; }
    public string ImageFileName { get; set; }=string.Empty;
    public string ImageLabel { get; set; } = string.Empty;
}