using System.ComponentModel;
using SAM.Core.Attributes;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(SpaceSimulationResult)), Description("SpaceSimulationResult Parameter")]
    public enum SpaceSimulationResultParameter
    {
        [ParameterProperties("Dry Bulb Temperature", "Dry Bulb Temperature [C]"), DoubleParameterValue()] DryBulbTempearture,
        [ParameterProperties("Resultant Temperature", "Resultant Temperature [C]"), DoubleParameterValue()] ResultantTemperature,
        [ParameterProperties("Cooling Load", "Cooling Load [W]"), DoubleParameterValue()] CoolingLoad,
        [ParameterProperties("Heating Load", "Heating Load [W]"), DoubleParameterValue()] HeatingLoad,
        [ParameterProperties("Solar Gain", "Solar Gain [W]"), DoubleParameterValue()] SolarGain,
        [ParameterProperties("Lighting Gain", "Lighting Gain [W]"), DoubleParameterValue()] LightingGain,
        [ParameterProperties("Infiltration Gain", "Infiltration Gain [W]"), DoubleParameterValue()] InfiltrationGain,
        [ParameterProperties("Air Movement Gain", "Air Movement Gain [W]"), DoubleParameterValue()] AirMovementGain,
        [ParameterProperties("Building Heat Transfer", "Building Heat Transfer [W]"), DoubleParameterValue()] BuildingHeatTransfer,
        [ParameterProperties("Glazing External Conduction", "Glazing External Conduction [W]"), DoubleParameterValue()] GlazingExternalConduction, 
        [ParameterProperties("Opaque External Conduction", "Opaque External Conduction [W]"), DoubleParameterValue()] OpaqueExternalConduction,
        [ParameterProperties("Occupancy Sensible Gain", "Occupancy Sensible Gain [W]"), DoubleParameterValue()] OccupancySensibleGain,
        [ParameterProperties("Occupancy Latent Gain", "Occupancy Latent Gain [W]"), DoubleParameterValue()] OccupancyLatentGain,
        [ParameterProperties("Equipment Sensible Gain", "Equipment Sensible Gain [W]"), DoubleParameterValue()] EquipmentSensibleGain,
        [ParameterProperties("Equipment Latent Gain", "Equipment Latent Gain [W]"), DoubleParameterValue()] EquipmentLatentGain,
        [ParameterProperties("Humidity Ratio", "Humidity Ratio [kg/kg]"), DoubleParameterValue(0)] HumidityRatio,
        [ParameterProperties("Relative Humidity", "Relative Humidity [%]"), DoubleParameterValue(0, 100)] RelativeHumidity,
        [ParameterProperties("Aperture Flow In", "Aperture Flow In [kg/s]"), DoubleParameterValue(0)] ApertureFlowIn,
        [ParameterProperties("Aperture Flow Out", "Aperture Flow Out [kg/s]"), DoubleParameterValue(0)] ApertureFlowOut,
        [ParameterProperties("Pollutant", "Pollutant [ppm]"), DoubleParameterValue(0)] Pollutant,
        [ParameterProperties("Volume", "Volume [m3]"), DoubleParameterValue(0)] Volume,
        [ParameterProperties("Area", "Area [m2]"), DoubleParameterValue(0)] Area,
        [ParameterProperties("Sizing Method", "Sizing Method"), ParameterValue(Core.ParameterType.String)] SizingMethod,
    }
}