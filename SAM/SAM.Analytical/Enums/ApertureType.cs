using System.ComponentModel;

namespace SAM.Analytical
{
    /// <summary>
    /// Analytical Aperture Type
    /// </summary>
    [Description("Analytical Aperture Type.")]
    public enum ApertureType
    {
        /// <summary>
        /// Undefined Analytical Aperture Type
        /// </summary>
        [Description("Undefined")] Undefined,

        /// <summary>
        /// Window Analytical Aperture Type
        /// </summary>
        [Description("Window")] Window,

        /// <summary>
        /// Door Analytical Aperture Type
        /// </summary>
        [Description("Door")] Door
    }
}