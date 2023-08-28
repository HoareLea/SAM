using System.ComponentModel;
using SAM.Core.Attributes;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(AirHandlingUnit)), Description("AirHandlingUnit Parameter")]
    public enum AirHandlingUnitParameter
    {
        [ParameterProperties("Winter Heating Coil Supply Temperature", "Winter Heating Coil Supply Temperature [°C]"), ParameterValue(Core.ParameterType.Double)] WinterHeatingCoilSupplyTemperature,
        [ParameterProperties("Summer Heating Coil", "Summer Heating Coil"), ParameterValue(Core.ParameterType.Boolean)] SummerHeatingCoil,
    }
}