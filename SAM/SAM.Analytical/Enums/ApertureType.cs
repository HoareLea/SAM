using System.ComponentModel;

namespace SAM.Analytical
{
    [Description("Analytical Aperture Type.")]
    public enum ApertureType
    {
        [Description("Undefined")] Undefined,
        [Description("Window")] Window,
        [Description("Door")] Door
    }
}