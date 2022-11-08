using System.ComponentModel;
using SAM.Core.Attributes;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(IOpeningProperties)), Description("OpeningProperties Parameter")]
    public enum OpeningPropertiesParameter
    {
        [ParameterProperties("Function", "Function"), ParameterValue(Core.ParameterType.String)] Function,
        [ParameterProperties("Description", "Description"), ParameterValue(Core.ParameterType.String)] Description,
    }
}