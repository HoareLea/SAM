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
    }
}