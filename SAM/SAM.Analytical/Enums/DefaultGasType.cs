using System.ComponentModel;

namespace SAM.Analytical
{
    /// <summary>
    /// Default Gas Types
    /// </summary>
    [Description("Default Gas Types.")]
    public enum DefaultGasType
    {
        /// <summary>
        /// Undefined Gas Type
        /// </summary>
        [Description("Undefined")] Undefined,

        /// <summary>
        /// Air Gas Type
        /// </summary>
        [Description("Air")] Air,

        /// <summary>
        /// Xenon Gas Type
        /// </summary>
        [Description("Xenon")] Xenon,

        /// <summary>
        /// Argon Gas Type
        /// </summary>
        [Description("Argon")] Argon,

        /// <summary>
        /// Krypton Gas Type
        /// </summary>
        [Description("Krypton")] Krypton,

        /// <summary>
        /// Sulfur HexaFluoride (SF6) Gas Type
        /// </summary>
        [Description("Sulfur HexaFluoride (SF6)")] SulfurHexaFluoride
    }
}