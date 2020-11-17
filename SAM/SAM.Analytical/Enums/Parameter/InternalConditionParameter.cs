using System.ComponentModel;
using SAM.Core.Attributes;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(InternalCondition)), Description("Internal Condition Parameter")]
    public enum InternalConditionParameter
    {
        [ParameterProperties("Number Of People", "Number Of People"), DoubleParameterValue(0)] NumberOfPeople,
        [ParameterProperties("Occupancy Profile Name", "Occupancy Profile Name"), ParameterValue(Core.ParameterType.String)] OccupancyProfileName,
        [ParameterProperties("Occupant Sensible Gain", "Occupant Sensible Gain"), ParameterValue(Core.ParameterType.Double)] OccupantSensibleGain,
        [ParameterProperties("Occupant Latent Gain", "Occupant Latent Gain"), ParameterValue(Core.ParameterType.Double)] OccupantLatentGain,
        [ParameterProperties("Equipment Sensible Profile Name", "Equipment Sensible Profile Name"), ParameterValue(Core.ParameterType.String)] EquipmentSensibleProfileName,
        [ParameterProperties("Equipment Latent Profile Name", "Equipment Latent Profile Name"), ParameterValue(Core.ParameterType.String)] EquipmentLatentProfileName,
        [ParameterProperties("Lighting Gain", "Lighting Gain"), ParameterValue(Core.ParameterType.Double)] LightingGain,
        [ParameterProperties("Design Lux Level", "Design Lux Level"), DoubleParameterValue(0)] DesignLuxLevel,
        [ParameterProperties("Lighting Profile Name", "Lighting Profile Name"), ParameterValue(Core.ParameterType.String)] LightingProfileName,
        [ParameterProperties("Light Level", "Light Level"), ParameterValue(Core.ParameterType.Double)] LightLevel,
        [ParameterProperties("Light Efficiency", "Light Efficiency"), ParameterValue(Core.ParameterType.Double)] LightEfficiency,
        [ParameterProperties("Natural Ventilation Outdoor Air Per Person", "Natural Ventilation Outdoor Air Per Person"), ParameterValue(Core.ParameterType.Double)] NaturalVentilationOutdoorAirPerPerson,
        [ParameterProperties("Natural Ventilation Outdoor Air Changes Per Hour", "Natural Ventilation Outdoor Air Changes Per Hour"), ParameterValue(Core.ParameterType.Double)] NaturalVentilationOutdoorAirChangesPerHour,
        [ParameterProperties("Natural Ventilation Outdoor Air Profile Name", "Natural Ventilation Outdoor Air Profile Name"), ParameterValue(Core.ParameterType.String)] NaturalVentilationOutdoorAirProfileName,
        [ParameterProperties("Infiltration Air Changes Per Hour", "Infiltration Air Changes Per Hour"), ParameterValue(Core.ParameterType.Double)] InfiltrationAirChangesPerHour,
        [ParameterProperties("Infiltration Profile Name", "Infiltration Profile Name"), ParameterValue(Core.ParameterType.String)] InfiltrationProfileName,
        [ParameterProperties("Pollutant Generation Per Person", "Pollutant Generation Per Person"), ParameterValue(Core.ParameterType.Double)] PollutantGenerationPerPerson,
    }
}