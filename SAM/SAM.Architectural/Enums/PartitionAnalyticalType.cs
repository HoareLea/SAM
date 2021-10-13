using System.ComponentModel;

namespace SAM.Architectural
{
    [Description("Partition Analytical Type")]
    public enum PartitionAnalyticalType
    {
        [Description("Undefined")] Undefined,
        [Description("Curtain Wall")] CurtainWall,
        [Description("External Wall")] ExternalWall,
        [Description("Internal Wall")] InternalWall,
        [Description("Underground Wall")] UndergroundWall,
        [Description("Internal Floor")] InternalFloor,
        [Description("External Floor")] ExternalFloor,
        [Description("Floor on Grade")] OnGradeFloor,
        [Description("Underground Floor")] UndergroundFloor,
        [Description("Underground Ceiling")] UndergroundCeiling,
        [Description("Roof")] Roof,
        [Description("Air")] Air,
        [Description("Shade")] Shade
    }
}