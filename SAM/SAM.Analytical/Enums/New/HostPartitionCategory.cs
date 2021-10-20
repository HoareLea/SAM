using System.ComponentModel;

namespace SAM.Analytical
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