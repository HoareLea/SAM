using System.ComponentModel;
using SAM.Core.Attributes;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(Construction)), Description("Construction Parameter")]
    public enum ConstructionParameter
    {
        [ParameterProperties("Dafault Panel Type", "Dafault Panel Type"), ParameterValue(Core.ParameterType.String)] DefaultPanelType,
        [ParameterProperties("Description", "Description"), ParameterValue(Core.ParameterType.String)] Description,
        [ParameterProperties("Color", "Color"), ParameterValue(Core.ParameterType.Color)] Color,
        [ParameterProperties("Is Air", "Is Air"), ParameterValue(Core.ParameterType.Boolean)] IsAir,
        [ParameterProperties("Is Ground", "Is Ground"), ParameterValue(Core.ParameterType.Boolean)] IsGround,
        [ParameterProperties("Is Internal Shadow", "Is Internal Shadow"), ParameterValue(Core.ParameterType.Boolean)] IsInternalShadow,
        [ParameterProperties("Transparent", "Transparent"), ParameterValue(Core.ParameterType.Boolean)] Transparent,
        [ParameterProperties("Default Thickness", "Default Thickness"), DoubleParameterValue(0)] DefaultThickness,
    }
}