using System.ComponentModel;
using SAM.Core.Attributes;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(ApertureConstruction)), Description("ApertureConstruction Parameter")]
    public enum ApertureConstructionParameter
    {
        [ParameterProperties("Dafault Panel Type", "Dafault Panel Type"), ParameterValue(Core.ParameterType.String)] DefaultPanelType,
        [ParameterProperties("Description", "Description"), ParameterValue(Core.ParameterType.String)] Description,
        [ParameterProperties("Transparent", "Transparent"), ParameterValue(Core.ParameterType.Boolean)] Transparent,
        [ParameterProperties("Color", "Color"), ParameterValue(Core.ParameterType.Color)] Color,
    }
}