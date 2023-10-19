using System.ComponentModel;

namespace SAM.Analytical
{
    [Description("Lighting Occupancy Controls")]
    public enum LightingOccupancyControls
    {
        [Description("Undefined")] Undefined,
        [Description("ManualOn ManulaOff")] ManualOn_ManualOff,
        [Description("ManualOn ManulaOff with add auto")] ManualOn_ManualOff_WithAdditionalAutomatic,
        [Description("AutoOn Dimmed")] AutoOn_Dimmed,
        [Description("AutoOn AutoOff")] AutoOn_AutoOff,
        [Description("ManualOn Dimmed")] ManualOn_Dimmed,
        [Description("ManualOn AutoOff")] ManualOn_AutoOff,
    }
}