using System.ComponentModel;
using SAM.Core.Attributes;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(Construction)), Description("Construction Parameter")]
    public enum ConstructionParameter
    {
        [ParameterProperties("SAM_BuildingElementType", "Type Name"), ParameterType(Core.ParameterType.String)] TypeName,
        [ParameterProperties("SAM_BuildingElementDescription", "Description"), ParameterType(Core.ParameterType.String)] Description,
    }
}