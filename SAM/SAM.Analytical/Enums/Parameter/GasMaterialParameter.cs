using System.ComponentModel;
using SAM.Core.Attributes;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(Core.GasMaterial)), Description("GasMaterial Parameter")]
    public enum GasMaterialParameter
    {
        [ParameterProperties("Heat Transfer Coefficient", "Heat Transfer Coefficient"), DoubleParameterValue(0)] HeatTransferCoefficient,
        [ParameterProperties("Default Gas Type", "Default Gas Type"), ParameterValue(Core.ParameterType.String)] DefaultGasType

    }
}
