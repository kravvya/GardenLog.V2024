namespace GardenLogWeb.Models.UserProfile;

public record GardenModel : GardenViewModel
{
    public string ImageFileName { get; set; } = string.Empty;
    public string ImageLabel { get; set; } = string.Empty;
    public List<ImageViewModel>? Images { get; set; }
}
