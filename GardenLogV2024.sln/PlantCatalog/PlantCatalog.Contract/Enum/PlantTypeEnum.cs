using System.ComponentModel;

namespace PlantCatalog.Contract.Enum
{
    public enum PlantTypeEnum : int
    {
        [Description("Unspecified")]
        Unspecified = 0,
        [Description("Vegetable")]
        Vegetable = 1,
        [Description("Berry")]
        Berry = 2,
        [Description("Flower")]
        Flower = 3,
        [Description("Herb")]
        Herb = 4
    }
}
