using System.ComponentModel;

namespace SAM.Analytical
{
    [Description("Load Type.")]
    public enum LoadType
    {
        [Description("Undefined")] Undefined,
        [Description("Cooling")] Cooling,
        [Description("Heating")] Heating,
    }
}