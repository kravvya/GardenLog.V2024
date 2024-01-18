using System.ComponentModel;

namespace GardenLog.SharedKernel.Enum
{
    public enum RelatedEntityTypEnum : int
    {
        [Description("Unspecified")]
        Unspecified = 0,
        [Description("Plant")]
        Plant = 1,
        [Description("PlantVariety")]
        PlantVariety = 2,
        [Description("Garden")]
        Garden = 3,
        [Description("Seed")]
        Seed = 4,
        [Description("Seed Vendor")]
        SeedVendor = 5,
        [Description("Harvest cycle")]
        HarvestCycle = 6,
        [Description("Plant Harvest cycle")]
        PlantHarvestCycle = 7,
        [Description("Task")]
        Task = 8,
        [Description("Work Log")]
        WorkLog = 9
    }

}
