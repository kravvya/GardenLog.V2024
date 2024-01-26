using System.ComponentModel;

namespace PlantHarvest.Contract.Enum;

public enum WorkLogReasonEnum : int
{
    [Description("Unspecified")]
    Unspecified = 0,
    [Description("Fertilize Indoors")]
    FertilizeIndoors = 1,
    [Description("Fertilize")]
    FertilizeOutside = 2,
    [Description("Harden Off")]
    Harden = 3,
    [Description("Harvest")]
    Harvest = 4,
    [Description("Information")]
    Information =5,
    [Description("Issue")]
    Issue = 6,
    [Description("Issue Resolution")]
    IssueResolution = 7,
    [Description("Plant")]
    Plant = 8,
    [Description("Maintenance")]
    Maintenance = 9,
    [Description("Sow Indoors")]
    SowIndoors = 10,
    [Description("Sow Outside")]
    SowOutside = 11,
    [Description("Transplant Outside")]
    TransplantOutside = 12,
    [Description("Water Indoors")]
    WaterIndoors = 13,
    [Description("Water Outside")]
    WaterOutside = 14,
}
