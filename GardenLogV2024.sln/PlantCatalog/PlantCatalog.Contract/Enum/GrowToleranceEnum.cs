using System.ComponentModel;

namespace PlantCatalog.Contract.Enum;

[Flags]
public enum GrowToleranceEnum : int
{
    [Description("Unspecified")]
    Unspecified = 0,
    [Description("Light Frost")]
    LightFrost = 1,
    [Description("Hard Frost")]
    HardFrost = 2
}

