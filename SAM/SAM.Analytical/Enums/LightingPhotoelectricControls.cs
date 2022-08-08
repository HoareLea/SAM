using System.ComponentModel;

namespace SAM.Analytical
{
    [Description("Lighting Photoelectric Controls")]
    public enum LightingPhotoelectricControls
    {
        [Description("Undefined")] Undefined,
        [Description("None")] None,
        [Description("Manual")] Manual,
        [Description("Photocell On Off")] PhotocellOnOff,
        [Description("Photocell Dimming")] PhotocellDimming,
    }
}