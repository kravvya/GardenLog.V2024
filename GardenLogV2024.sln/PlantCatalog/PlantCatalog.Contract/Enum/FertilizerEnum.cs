using System.ComponentModel;

namespace PlantCatalog.Contract.Enum
{
    public enum FertilizerEnum : int
    {
        [Description("Unspecified")]
        Unspecified = 0,

        [Description("All Purpose")]
        AllPurpose = 1,

        [Description("Nitrogen")]
        Nitrogen = 2,

        [Description("Half Strength Balanced")]
        HalfBalanced = 3,

        [Description("Balanced")]
        Balanced = 4,

        [Description("Compost")]
        Compost = 5,

        [Description("Starter")]
        Starter = 6,

        [Description("Low Nitrogen")]
        LowNitrogen = 7
    }
}
