using System.ComponentModel;
using SAM.Core.Attributes;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(AirHandlingUnit)), Description("AirHandlingUnit Parameter")]
    public enum AirHandlingUnitParameter
    {
        [ParameterProperties("Summer Supply Temperature", "Summer Supply Temperature [°C]"), ParameterValue(Core.ParameterType.Double)] SummerSupplyTemperature,
        [ParameterProperties("Winter Supply Temperature", "Winter Supply Temperature [°C]"), ParameterValue(Core.ParameterType.Double)] WinterSupplyTemperature,
        [ParameterProperties("Frost Coil Off Temperature", "Frost Coil Off Temperature [°C]"), ParameterValue(Core.ParameterType.Double)] FrostCoilOffTemperature,
        [ParameterProperties("Heat Recovery Sensible Efficiency", "Heat Recovery Sensible Efficiency [%]"), DoubleParameterValue(0, 100)] HeatRecoverySensibleEfficiency,
        [ParameterProperties("Heat Recovery Latent Efficiency", "Heat Recovery Latent Efficiency [%]"), DoubleParameterValue(0, 100)] HeatRecoveryLatentEfficiency,
        [ParameterProperties("Cooling Coil On Temperature", "Cooling Coil On Temperature [°C]"), ParameterValue(Core.ParameterType.Double)] CoolingCoilOnTemperature,
        [ParameterProperties("Cooling Coil Off Temperature", "Cooling Coil Off Temperature [°C]"), ParameterValue(Core.ParameterType.Double)] CoolingCoilOffTemperature,
        [ParameterProperties("Cooling Coil Fluid Supply Temperature", "Cooling Coil Fluid Supply Temperature [°C]"), ParameterValue(Core.ParameterType.Double)] CoolingCoilFluidSupplyTemperature,
        [ParameterProperties("Cooling Coil Fluid Return Temperature", "Cooling Coil Fluid Return Temperature [°C]"), ParameterValue(Core.ParameterType.Double)] CoolingCoilFluidReturnTemperature,
        [ParameterProperties("Heating Coil Fluid Supply Temperature", "Heating Coil Fluid Supply Temperature [°C]"), ParameterValue(Core.ParameterType.Double)] HeatingCoilFluidSupplyTemperature,
        [ParameterProperties("Heating Coil Fluid Return Temperature", "Heating Coil Fluid Return Temperature [°C]"), ParameterValue(Core.ParameterType.Double)] HeatingCoilFluidReturnTemperature,
    }
}