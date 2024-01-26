using System.ComponentModel;

namespace UserManagement.Contract.Enum
{
    public enum GardenBedTypeEnum : int
    {
        [Description("Unspecified")]
        Unspecified = 0,

        [Description("In Ground Bed")]
        InGroundBed = 1,

        [Description("Raised Bed")]
        RaisedBed = 2,

        [Description("Container")]
        Container = 3,

        [Description("Low Tunnel")]
        LowTunnel = 4,

        [Description("High Tunnel")]
        HighTunnel = 5,

        [Description("Greenhouse")]
        Greenhouse = 6
    }
}
