using System.ComponentModel;

namespace SAM.Architectural
{
    [Description("HostPartition Category")]
    public enum HostPartitionCategory
    {
        [Description("Undefined")] Undefined,
        [Description("Wall")] Wall,
        [Description("Roof")] Roof,
        [Description("Floor")] Floor,
    }
}