using System.ComponentModel;

namespace SAM.Core
{
    [Description("Period")]
    public enum Period
    {
        [Description("Undefined")] Undefined,
        [Description("Hourly")] Hourly,
        [Description("Daily")] Daily,
        [Description("Weekly")] Weekly,
        [Description("Monthly")] Monthly,
    }
}