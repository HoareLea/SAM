using System.ComponentModel;

namespace SAM.Architectural
{
    [Description("HostBuildingElement Category")]
    public enum HostBuildingElementCategory
    {
        [Description("Undefined")] Undefined,
        [Description("Wall")] Wall,
        [Description("Roof")] Roof,
        [Description("Floor")] Floor,
    }
}