using System.ComponentModel;

namespace SAM.Geometry
{
    [Description("Edge Orientation Method")]
    public enum EdgeOrientationMethod
    {
        [Description("Undefined")] Undefined,
        [Description("Similar")] Similar,
        [Description("Opposite")] Opposite,
    }
}