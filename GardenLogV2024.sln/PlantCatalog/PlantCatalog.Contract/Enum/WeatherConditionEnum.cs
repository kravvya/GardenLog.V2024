using System.ComponentModel;

namespace PlantCatalog.Contract.Enum
{
    public enum WeatherConditionEnum : int
    {
        [Description("Unspecified")]
        Unspecified = 0,
        [Description("Before last frost")]
        BeforeLastFrost = 1,
        [Description("Before first frost")]
        BeforeFirstFrost = 2,
        [Description("In Early Spring")]
        EarlySpring = 3,
        [Description("In Warm Soil")]
        WarmSoil = 4,
        [Description("After danger of frost")]
        AfterDangerOfFrost = 5,
        [Description("Mid Summer")]
        MidSummer = 6
    }
}