namespace UserManagement.Contract.ViewModels;

public record  GardenViewModel : GardenBase
{
    public string GardenId { get; set; } = string.Empty;
}
