using System.ComponentModel;

namespace SAM.Analytical
{
    [Description("Analytical Emitter Category.")]
    public enum EmitterCategory
    {
        [Description("Undefined")] Undefined,
        [Description("Heating")] Heating,
        [Description("Cooling")] Cooling,
        [Description("Light")] Light,
        [Description("Occupant")] Occupant,
        [Description("Equipment")] Equipment,
    }
}