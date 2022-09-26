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
        [ParameterProperties("Winter Heat Recovery Sensible Efficiency", "Winter Heat Recovery Sensible Efficiency [%]"), DoubleParameterValue(0, 100)] WinterHeatRecoverySensibleEfficiency,
        [ParameterProperties("Winter Heat Recovery Latent Efficiency", "Winter Heat Recovery Latent Efficiency [%]"), DoubleParameterValue(0, 100)] WinterHeatRecoveryLatentEfficiency,
        [ParameterProperties("Summer Heat Recovery Sensible Efficiency", "Summer Heat Recovery Sensible Efficiency [%]"), DoubleParameterValue(0, 100)] SummerHeatRecoverySensibleEfficiency,
        [ParameterProperties("Summer Heat Recovery Latent Efficiency", "Summer Heat Recovery Latent Efficiency [%]"), DoubleParameterValue(0, 100)] SummerHeatRecoveryLatentEfficiency,
        [ParameterProperties("Cooling Coil Fluid Flow Temperature", "Cooling Coil Fluid Flow Temperature [°C]"), ParameterValue(Core.ParameterType.Double)] CoolingCoilFluidFlowTemperature,
        [ParameterProperties("Cooling Coil Fluid Return Temperature", "Cooling Coil Fluid Return Temperature [°C]"), ParameterValue(Core.ParameterType.Double)] CoolingCoilFluidReturnTemperature,
        [ParameterProperties("Cooling Coil Contact Factor", "Cooling Coil Contact Factor [0-1]"), DoubleParameterValue(0, 1)] CoolingCoilContactFactor,
        [ParameterProperties("Heating Coil Fluid Flow Temperature", "Heating Coil Fluid Flow Temperature [°C]"), ParameterValue(Core.ParameterType.Double)] HeatingCoilFluidFlowTemperature,
        [ParameterProperties("Heating Coil Fluid Return Temperature", "Heating Coil Fluid Return Temperature [°C]"), ParameterValue(Core.ParameterType.Double)] HeatingCoilFluidReturnTemperature,
        [ParameterProperties("Winter Heating Coil Supply Temperature", "Winter Heating Coil Supply Temperature [°C]"), ParameterValue(Core.ParameterType.Double)] WinterHeatingCoilSupplyTemperature,
        [ParameterProperties("Winter Heat Recovery Dry Bulb Temperature", "Winter Heat Recovery Dry Bulb Temperature [°C]"), ParameterValue(Core.ParameterType.Double)] WinterHeatRecoveryDryBulbTemperature,
        [ParameterProperties("Winter Heat Recovery Relative Humidity", "Winter Heat Recovery Relative Humidity [%]"), DoubleParameterValue(0, 100)] WinterHeatRecoveryRelativeHumidity,
        [ParameterProperties("Summer Heat Recovery Dry Bulb Temperature", "Summer Heat Recovery Dry Bulb Temperature [°C]"), ParameterValue(Core.ParameterType.Double)] SummerHeatRecoveryDryBulbTemperature,
        [ParameterProperties("Summer Heat Recovery Relative Humidity", "Summer Heat Recovery Relative Humidity [%]"), DoubleParameterValue(0, 100)] SummerHeatRecoveryRelativeHumidity,
        [ParameterProperties("Summer Heating Coil", "Summer Heating Coil"), ParameterValue(Core.ParameterType.Boolean)] SummerHeatingCoil,
    }
}