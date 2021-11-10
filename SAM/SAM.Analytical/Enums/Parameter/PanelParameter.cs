using System.ComponentModel;
using SAM.Core.Attributes;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(Panel)), Description("Panel Parameter")]
    public enum PanelParameter
    {
        [ParameterProperties("Transparent", "Transparent"), ParameterValue(Core.ParameterType.Boolean)] Transparent,
        [ParameterProperties("Color", "Color"), ParameterValue(Core.ParameterType.Color)] Color,
    }
}