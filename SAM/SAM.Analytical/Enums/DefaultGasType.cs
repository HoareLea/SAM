using System.ComponentModel;

namespace SAM.Analytical
{
    [Description("Default Gas Types.")]
    public enum DefaultGasType
    {
        [Description("Undefined")] Undefined,
        [Description("Air")] Air,
        [Description("Xenon")] Xenon,
        [Description("Argon")] Argon,
        [Description("Krypton")] Krypton,
        [Description("Sulfur HexaFluoride (SF6)")] SulfurHexaFluoride
    }
}