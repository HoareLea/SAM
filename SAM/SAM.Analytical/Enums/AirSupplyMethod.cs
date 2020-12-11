using System.ComponentModel;

namespace SAM.Analytical
{
    [Description("Air Supply Method")]
    public enum AirSupplyMethod
    {
        [Description("Undefined")] Undefined,
        [Description("Outside")] Outside,
        [Description("Total")] Total
    }
}