using System.ComponentModel;
using SAM.Core.Attributes;

namespace SAM.Core
{
    [AssociatedTypes(typeof(Material)), Description("Material Parameter")]
    public enum MaterialParameter
    {
        [ParameterProperties("Default Thickness", "Default Material Thickness"), DoubleParameterValue(0)] DefaultThickness,
    }
}