using System.ComponentModel;

namespace PlantHarvest.Contract.Enum;

public enum PlantingMethodEnum : int
{
    [Description("Unspecified")]
    Unspecified = 0,
    [Description("Direct Seed outdoors")]
    DirectSeed = 1,
    [Description("Start Seed Indoors")]
    SeedIndoors = 2,
    [Description("Transplating")]
    Transplanting = 3

}
