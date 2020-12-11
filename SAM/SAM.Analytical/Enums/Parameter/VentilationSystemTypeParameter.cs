using System.ComponentModel;
using SAM.Core.Attributes;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(VentilationSystemType)), Description("Ventilation System Type Parameter")]
    public enum VentilationSystemTypeParameter
    {
        [ParameterProperties("Temperature Difference", "Supply Air And Room Remperature Difference [K]"), DoubleParameterValue(0)] TemperatureDifference,
        [ParameterProperties("Air Supply Method", "Air Supply Method"), ParameterValue(Core.ParameterType.String)] AirSupplyMethod,
    }
}