using System.ComponentModel;
using SAM.Core.Attributes;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(SpaceSimulationResult)), Description("SpaceSimulationResult Parameter")]
    public enum SpaceSimulationResultParameter
    {
        [ParameterProperties("Dry Bulb Temperature", "Dry Bulb Temperature [K]"), DoubleParameterValue(0)] DryBulbTempearture,
        [ParameterProperties("Resultant Temperature", "Resultant Temperature [K]"), DoubleParameterValue(0)] ResultantTemperature,
        [ParameterProperties("Cooling Load", "Cooling Load [W]"), DoubleParameterValue(0)] CoolingLoad,
        [ParameterProperties("Heating Load", "Heating Load [W]"), DoubleParameterValue(0)] HeatingLoad,
        [ParameterProperties("Solar Gain", "Solar Gain [W]"), DoubleParameterValue(0)] SolarGain,
        [ParameterProperties("Lighting Gain", "Lighting Gain [W]"), DoubleParameterValue(0)] LightingGain,
        [ParameterProperties("Infiltartion Gain", "Infiltration Gain [W]"), DoubleParameterValue(0)] InfiltrationGain,
        [ParameterProperties("Air Movement Gain", "Air Movement Gain [W]"), DoubleParameterValue(0)] AirMovementGain,
        [ParameterProperties("Building Heat Transfer", "Building Heat Transfer [W]"), DoubleParameterValue(0)] BuildingHeatTransfer,
        [ParameterProperties("Glizing External Conduction", "Glizing External Conduction"), DoubleParameterValue(0)] GlazingExternalConduction, 
        [ParameterProperties("Opaque External Conduction", "Opaque External Conduction"), DoubleParameterValue(0)] OpaqueExternalConduction,
        [ParameterProperties("Occupancy Sensible Gain", "Occupancy Sensible Gain [W]"), DoubleParameterValue(0)] OccupancySensibleGain,
        [ParameterProperties("Occupancy Latent Gain", "Occupancy Latent Gain [W]"), DoubleParameterValue(0)] OccupancyLatentGain,
        [ParameterProperties("Equipment Sensible Gain", "Equipment Sensible Gain [W]"), DoubleParameterValue(0)] EquipmentSensibleGain,
        [ParameterProperties("Equipment Latent Gain", "Equipment Latent Gain [W]"), DoubleParameterValue(0)] EquipmentLatentGain,
        [ParameterProperties("Humidity Ratio", "Humidity Ratio"), DoubleParameterValue(0)] HumidityRatio,
        [ParameterProperties("Relative Humidity", "Relative Humidity"), DoubleParameterValue(0)] RelativeHumidity,
        [ParameterProperties("Aperture Flow In", "Aperture Flow In"), DoubleParameterValue(0)] ApertureFlowIn,
        [ParameterProperties("Aperture Flow Out", "Aperture Flow Out"), DoubleParameterValue(0)] ApertureFlowOut,
        [ParameterProperties("Pollutant Generation", "Pollutant Generation"), DoubleParameterValue(0)] PollutantGeneration,
        [ParameterProperties("Volumne", "Volume [m3]"), DoubleParameterValue(0)] Volume,
        [ParameterProperties("Area", "Area [m2]"), DoubleParameterValue(0)] Area,
        [ParameterProperties("Sizing Method", "Sizing Method"), ParameterValue(Core.ParameterType.String)] SizingMethod,
    }
}