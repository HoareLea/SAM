using System.ComponentModel;
using SAM.Core.Attributes;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(ApertureConstruction)), Description("ApertureConstruction Parameter")]
    public enum ApertureConstructionParameter
    {
        [ParameterProperties("SAM_BuildingElementType", "Type Name"), ParameterType(Core.ParameterType.String)] TypeName,
        [ParameterProperties("SAM_BuildingElementDescription", "Description"), ParameterType(Core.ParameterType.String)] Description,
    }
}