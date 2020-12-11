using System.ComponentModel;
using SAM.Core.Attributes;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(CoolingSystemType)), Description("Cooling System Type Parameter")]
    public enum CoolingSystemTypeParameter
    {
        [ParameterProperties("Radiant Proportion", "Radiant Proportion"), DoubleParameterValue(0, 1)] RadiantProportion,
        [ParameterProperties("View Coefficient", "View Coefficient"), DoubleParameterValue(0, 1)] ViewCoefficient,
        [ParameterProperties("Supply Circuit Temperature", "Supply Circuit Temperature"), DoubleParameterValue(0)] SupplyCircuitTemperature,
        [ParameterProperties("Return Circuit Temperature", "Return Circuit Temperature"), DoubleParameterValue(0)] ReturnCircuitTemperature,
        [ParameterProperties("Temperature Difference", "Supply And Room Remperature Difference [K]"), DoubleParameterValue(0)] TemperatureDifference,
        [ParameterProperties("Supply Temperature", "Supply Temperature [K]"), DoubleParameterValue(0)] SupplyTemperature
    }
}