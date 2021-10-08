using System.ComponentModel;

namespace SAM.Architectural
{
    [Description("hostPartition Category")]
    public enum HostPartitionCategory
    {
        [Description("Undefined")] Undefined,
        [Description("Wall")] Wall,
        [Description("Roof")] Roof,
        [Description("Floor")] Floor,
    }
}