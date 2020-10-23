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
    }
}