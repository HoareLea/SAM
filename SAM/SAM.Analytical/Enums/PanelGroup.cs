using System.ComponentModel;

namespace SAM.Analytical
{
    [Description("Panel Group")]
    public enum PanelGroup
    {
        [Description("Undefined")] Undefined,
        [Description("Floor")] Floor,
        [Description("Roof")] Roof,
        [Description("Wall")] Wall,
        [Description("Other")] Other
    }
}