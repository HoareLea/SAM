using System.ComponentModel;

namespace SAM.Analytical
{
    [Description("Analytical Boundary Type.")]
    public enum BoundaryType
    {
        [Description("Undefined")] Undefined,
        [Description("Ground")] Ground,
        [Description("Exposed")] Exposed,
        [Description("Adiabatic")] Adiabatic,
        [Description("Linked")] Linked
    }
}