using System.ComponentModel;

namespace SAM.Analytical
{
    [Description("Analytical Profile Type.")]
    public enum ProfileType
    {
        [Description("Undefined")] Undefined,
        [Description("Gain")] Gain,
        [Description("Thermostat")] Thermostat,
        [Description("Humidistat")] Humidistat,
        [Description("Other")] Other
    }
}