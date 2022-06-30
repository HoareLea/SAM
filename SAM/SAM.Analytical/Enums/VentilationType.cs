using System.ComponentModel;

namespace SAM.Analytical
{
    [Description("Ventilation Type.")]
    public enum VentilationType
    {
        [Description("Undefined")] Undefined,
        [Description("Outside Supply Air")] OSA,
        [Description("Total Supply Air")] TSA,
    }
}