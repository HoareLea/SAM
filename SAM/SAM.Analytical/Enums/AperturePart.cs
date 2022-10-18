using System.ComponentModel;

namespace SAM.Analytical
{
    /// <summary>
    /// Analytical Aperture Type
    /// </summary>
    [Description("Analytical Aperture Part.")]
    public enum AperturePart
    {
        /// <summary>
        /// Undefined Analytical Aperture Part
        /// </summary>
        [Description("Undefined")] Undefined,

        /// <summary>
        /// Window Analytical Aperture Pane
        /// </summary>
        [Description("Pane")] Pane,

        /// <summary>
        /// Door Analytical Aperture Frame
        /// </summary>
        [Description("Frame")] Frame
    }
}