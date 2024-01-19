using System.ComponentModel;

namespace PlantCatalog.Contract.Enum
{
    [Flags]
    public enum HarvestSeasonEnum :int
    {
        [Description("Unspecified")]
        Unspecified = 0,
        [Description("Spring")]
        Spring = 1,
        [Description("Early Summer")]
        EarlySummer = 2,
        [Description("Summer")]
        Summer = 4,
        [Description("Fall")]
        Fall = 8,
        [Description("Late Fall")]
        LateFall = 16
    }
}
