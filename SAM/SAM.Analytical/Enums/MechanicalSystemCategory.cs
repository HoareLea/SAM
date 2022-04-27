using System.ComponentModel;

namespace SAM.Analytical
{
    [Description("Mechanical System Category.")]
    public enum MechanicalSystemCategory
    {
        [Description("Undefined")] Undefined,
        [Description("Heating")] Heating,
        [Description("Cooling")] Cooling,
        [Description("Ventilation")] Ventilation,
        [Description("Other")] Other
    }
}