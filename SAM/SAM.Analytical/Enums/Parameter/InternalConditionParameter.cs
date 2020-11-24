using System.ComponentModel;
using SAM.Core.Attributes;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(InternalCondition)), Description("Internal Condition Parameter")]
    public enum InternalConditionParameter
    {
        [ParameterProperties("Occupancy", "Occupancy [p]"), DoubleParameterValue(0)] Occupancy,
        [ParameterProperties("Occupancy Per Area", "Occupancy Per Area [p/m2]"), DoubleParameterValue(0)] OccupancyPerArea,
        [ParameterProperties("Occupancy Profile Name", "Occupancy Profile Name"), ParameterValue(Core.ParameterType.String)] OccupancyProfileName,
        [ParameterProperties("Occupancy Sensible Gain", "Occupancy Sensible Gain [W]"), ParameterValue(Core.ParameterType.Double)] OccupancySensibleGain,
        [ParameterProperties("Occupancy Latent Gain", "Occupancy Latent Gain [W]"), ParameterValue(Core.ParameterType.Double)] OccupancyLatentGain,
        [ParameterProperties("Occupancy Sensible Gain Per Person", "Occupancy Sensible Gain Per Person [W/p]"), ParameterValue(Core.ParameterType.Double)] OccupancySensibleGainPerPerson,
        [ParameterProperties("Occupancy Latent Gain Per Person", "Occupancy Latent Gain Per Person [W/p]"), ParameterValue(Core.ParameterType.Double)] OccupancyLatentGainPerPerson,
        [ParameterProperties("Equipment Sensible Profile Name", "Equipment Sensible Profile Name"), ParameterValue(Core.ParameterType.String)] EquipmentSensibleProfileName,
        [ParameterProperties("Equipment Sensible Gain", "Equipment Sensible Gain [W]"), ParameterValue(Core.ParameterType.Double)] EquipmentSensibleGain,
        [ParameterProperties("Equipment Sensible Gain Per Area", "Equipment Sensible Gain Per Area [W/m2]"), ParameterValue(Core.ParameterType.Double)] EquipmentSensibleGainPerArea,
        [ParameterProperties("Equipment Latent Profile Name", "Equipment Latent Profile Name"), ParameterValue(Core.ParameterType.String)] EquipmentLatentProfileName,
        [ParameterProperties("Equipment Latent Gain", "Equipment Latent Gain [W]"), ParameterValue(Core.ParameterType.Double)] EquipmentLatentGain,
        [ParameterProperties("Equipment Latent Gain Per Area", "Equipment Latent Gain Per Area [W/m2]"), ParameterValue(Core.ParameterType.Double)] EquipmentLatentGainPerArea,
        [ParameterProperties("Lighting Gain Per Area", "Lighting Gain Per Area [W/m2]"), ParameterValue(Core.ParameterType.Double)] LightingGainPerArea,
        [ParameterProperties("Lighting Gain", "Lighting Gain [W]"), ParameterValue(Core.ParameterType.Double)] LightingGain,
        [ParameterProperties("Lighting Level", "Lighting Level [lux]"), DoubleParameterValue(0)] LightingLevel,
        [ParameterProperties("Lighting Profile Name", "Lighting Profile Name"), ParameterValue(Core.ParameterType.String)] LightingProfileName,
        [ParameterProperties("Lighting Efficiency", "Lighting Efficiency [W/m²/100lux]"), ParameterValue(Core.ParameterType.Double)] LightingEfficiency,
        [ParameterProperties("Infiltration Air Changes Per Hour", "Infiltration Air Changes Per Hour [ACH]"), ParameterValue(Core.ParameterType.Double)] InfiltrationAirChangesPerHour,
        [ParameterProperties("Infiltration Profile Name", "Infiltration Profile Name"), ParameterValue(Core.ParameterType.String)] InfiltrationProfileName,
        [ParameterProperties("Pollutant Generation Per Person", "Pollutant Generation Per Person [g/h/p]"), ParameterValue(Core.ParameterType.Double)] PollutantGenerationPerPerson,
        [ParameterProperties("Pollutant Generation Per Area", "Pollutant Generation Per Area [g/h/m2]"), ParameterValue(Core.ParameterType.Double)] PollutantGenerationPerArea,
        [ParameterProperties("Pollutant Profile Name", "Pollutant Profile Name"), ParameterValue(Core.ParameterType.String)] PollutantProfileName,
        [ParameterProperties("Heating Emmiter Radiant Proportion", "Heating Emmiter Radiant Proportion [0-1]"), DoubleParameterValue(0, 1)] HeatingEmmiterRadiantProportion,
        [ParameterProperties("Heating Emmiter Coefficient", "Heating Emmiter Coefficient [0-1]"), DoubleParameterValue(0, 1)] HeatingEmmiterCoefficient,
        [ParameterProperties("Heating Profile Name", "Heating Profile Name"), ParameterValue(Core.ParameterType.String)] HeatingProfileName,
        [ParameterProperties("Cooling Emmiter Radiant Proportion", "Cooling Emmiter Radiant Proportion [0-1]"), DoubleParameterValue(0, 1)] CoolingEmmiterRadiantProportion,
        [ParameterProperties("Cooling Emmiter Coefficient", "Cooling Emmiter Coefficient [0-1]"), DoubleParameterValue(0, 1)] CoolingEmmiterCoefficient,
        [ParameterProperties("Cooling Profile Name", "Cooling Profile Name"), ParameterValue(Core.ParameterType.String)] CoolingProfileName,
        [ParameterProperties("Humidification Profile Name", "Humidification Profile Name"), ParameterValue(Core.ParameterType.String)] HumidificationProfileName,
        [ParameterProperties("Dehumidification Profile Name", "Dehumidification Profile Name"), ParameterValue(Core.ParameterType.String)] DehumidificationProfileName,
    }
}