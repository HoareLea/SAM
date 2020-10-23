using System.ComponentModel;
using SAM.Core.Attributes;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(Construction)), Description("Construction Parameter")]
    public enum ConstructionParameter
    {
        [ParameterProperties("Dafault Panel Type", "Dafault Panel Type"), ParameterValue(Core.ParameterType.String)] DefaultPanelType,
        [ParameterProperties("Description", "Description"), ParameterValue(Core.ParameterType.String)] Description,

        //TODO: To be removed
        //[ParameterProperties("SAM_BuildingElementType", "Building Element Type"), ParameterValue(Core.ParameterType.String)] BuildingElementType,
        //[ParameterProperties("SAM_BuildingElementDescription", "Description"), ParameterValue(Core.ParameterType.String)] BuildingElementDescription,
    }
}