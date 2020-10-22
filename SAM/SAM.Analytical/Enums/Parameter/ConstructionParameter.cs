using System.ComponentModel;
using SAM.Core.Attributes;

namespace SAM.Analytical
{
    [ParameterTypes(typeof(Construction)), Description("Construction Parameter")]
    public enum ConstructionParameter
    {
        [ParameterProperties("SAM_BuildingElementType", "Type Name"), ParameterType(Core.ParameterType.String)] TypeName,
    }
}