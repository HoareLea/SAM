using System.ComponentModel;
using SAM.Core.Attributes;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(InternalCondition)), Description("Internal Condition Parameter")]
    public enum InternalConditionParameter
    {
        [ParameterProperties("SAM_NoPeople", "Number Of People"), DoubleParameterValue(0)] NumberOfPeople,
        [ParameterProperties("SAM_OccupacyProfile", "Occupancy Profile Name"), ParameterValue(Core.ParameterType.String)] OccupancyProfileName,
        [ParameterProperties("SAM_OccupantSensGain", "Occupant Sensible Gain"), ParameterValue(Core.ParameterType.Double)] OccupantSensibleGain,
        [ParameterProperties("SAM_OccupantLatGain", "Occupant Latent Gain"), ParameterValue(Core.ParameterType.Double)] OccupantLatentGain,
        [ParameterProperties("SAM_SmallPowerSensProfile", "Equipment Sensible Profile Name"), ParameterValue(Core.ParameterType.String)] EquipmentSensibleProfileName,
        [ParameterProperties("SAM_SmallPowerLatProfile", "Equipment Latent Profile Name"), ParameterValue(Core.ParameterType.String)] EquipmentLatentProfileName,
        [ParameterProperties("SAM_GenLightingGain", "Lighting Gain"), ParameterValue(Core.ParameterType.Double)] LightingGain,
        [ParameterProperties("SAM_DesignLuxLevel", "Design Lux Level"), DoubleParameterValue(0)] DesignLuxLevel,
    }
}