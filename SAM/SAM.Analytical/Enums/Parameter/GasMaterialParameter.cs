using System.ComponentModel;
using SAM.Core.Attributes;

namespace SAM.Analytical
{
    [ParameterTypes(typeof(Core.GasMaterial)), Description("GasMaterial Parameter")]
    public enum GasMaterialParameter
    {
        [ParameterProperties("Heat Transfer Coefficient", "Heat Transfer Coefficient"), DoubleParameterType(0)] HeatTransferCoefficient,
        [ParameterProperties("SAM_Material_Width", "Default Thickness"), DoubleParameterType(0)] DefaultThickness,
    }
}