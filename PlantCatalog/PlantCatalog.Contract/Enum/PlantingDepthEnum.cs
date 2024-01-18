using System.ComponentModel;

namespace PlantCatalog.Contract.Enum
{
    public enum PlantingDepthEnum : int
    {
        [Description("Unspecified")]
        Unspecified = 0,
        [Description("On Surface")]
        Surface = 1,
        [Description("1/16 in")]
        Depth16th = 2,
        [Description("1/8 in")]
        Depth8th = 3,
        [Description("1/4 in")]
        Depth4th = 4,
        [Description("1/2 in")]
        Depth2 = 5,
        [Description("1 in")]
        Depth1 = 6,
        [Description("3 in")]
        Depth3 = 7
    }
}
