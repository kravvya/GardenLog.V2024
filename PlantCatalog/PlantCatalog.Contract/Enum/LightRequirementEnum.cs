using System.ComponentModel;

namespace PlantCatalog.Contract.Enum
{
    public enum LightRequirementEnum : int
    {
        [Description("Unspecified")]
        Unspecified = 0,
        [Description("Full Shade")]
        FullShade = 1,
        [Description("Part Shade")]
        PartShade = 2,
        [Description("Full Sun")]
        FullSun = 3
    }
}
