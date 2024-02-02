

namespace GardenLogWeb.Models.Harvest;

public record HarvestCycleModel : HarvestCycleViewModel
{
    public string? GardenName { get;  set; }
    public bool CanShowLayout { get;  set; }
}
