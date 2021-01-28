using System.ComponentModel;

namespace SAM.Analytical
{
    [Description("Simulation Type.")]
    public enum SimulationType
    {
        [Description("Undefined")] Undefined,
        [Description("Cooling")] Cooling,
        [Description("Heating")] Heating,
    }
}