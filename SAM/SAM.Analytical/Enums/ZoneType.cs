using System.ComponentModel;

namespace SAM.Analytical
{
    [Description("Zone Type.")]
    public enum ZoneType
    {
        [Description("Undefined")] Undefined,
        [Description("Heating")] Heating,
        [Description("Cooling")] Cooling,
        [Description("Ventilation")] Ventilation,
        [Description("Other")] Other
    }
}