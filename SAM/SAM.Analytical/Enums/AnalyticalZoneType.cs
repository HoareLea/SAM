using System.ComponentModel;

namespace SAM.Analytical
{
    [Description("Analytical Zone Type.")]
    public enum AnalyticalZoneType
    {
        [Description("Undefined")] Undefined,
        [Description("Heating")] Heating,
        [Description("Cooling")] Cooling,
        [Description("Ventilation")] Ventilation,
    }
}