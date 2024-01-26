using UserManagement.Contract.Base;

namespace UserManagement.Contract.ViewModels;

public record GardenBedViewModel: GardenBedBase
{
    public string GardenBedId { get; set; } = string.Empty;
}
