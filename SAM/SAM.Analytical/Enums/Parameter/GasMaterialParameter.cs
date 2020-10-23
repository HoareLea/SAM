using System.ComponentModel;
using SAM.Core.Attributes;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(Core.GasMaterial)), Description("GasMaterial Parameter")]
    public enum GasMaterialParameter
    {
        [ParameterProperties("Heat Transfer Coefficient", "Heat Transfer Coefficient"), DoubleParameterValue(0)] HeatTransferCoefficient,
        [ParameterProperties("SAM_Material_Width", "Default Thickness"), DoubleParameterValue(0)] DefaultThickness,
    }
}