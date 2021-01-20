using System.ComponentModel;

namespace SAM.Analytical
{
    [Description("Sizing Method.")]
    public enum SizingMethod
    {
        [Description("Undefined")] Undefined,
        [Description("Cooling Design Day")] CDD,
        [Description("Heating Design Day")] HDD,
        [Description("Simulation")] Simulation,
    }
}