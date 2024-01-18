using System.ComponentModel;

namespace PlantCatalog.Contract.Enum
{
    public enum MoistureRequirementEnum : int
    {
        [Description("Unspecified")]
        Unspecified = 0,
        [Description("Drought  Tolerant")]
        DroughtTolerant = 1,
        [Description("At least 1in per week")]
        InchPerWeek = 2,
        [Description("1 to 2 in per week")]
        TwoInchPerWeek = 3,
        [Description("Consistent Moisture")]
        ConsistentMoisture = 4

    }
}
