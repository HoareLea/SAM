using System.ComponentModel;
using SAM.Core.Attributes;

namespace SAM.Analytical
{
    [ParameterTypes(typeof(InternalCondition)), Description("Internal Condition Parameter")]
    public enum InternalConditionParameter
    {
        [ParameterProperties("SAM_NoPeople", "Number Of People"), DoubleParameterType(0)] NumberOfPeople,
        [ParameterProperties("SAM_OccupacyProfile", "Occupancy Profile Name"), ParameterType(Core.ParameterType.String)] OccupancyProfileName,
        [ParameterProperties("SAM_OccupantSensGain", "Occupant Sensible Gain"), ParameterType(Core.ParameterType.Double)] OccupantSensibleGain,
        [ParameterProperties("SAM_OccupantLatGain", "Occupant Latent Gain"), ParameterType(Core.ParameterType.Double)] OccupantLatentGain,
        [ParameterProperties("SAM_SmallPowerSensProfile", "Equipment Sensible Profile Name"), ParameterType(Core.ParameterType.String)] EquipmentSensibleProfileName,
        [ParameterProperties("SAM_SmallPowerLatProfile", "Equipment Latent Profile Name"), ParameterType(Core.ParameterType.String)] EquipmentLatentProfileName,
        [ParameterProperties("SAM_GenLightingGain", "Lighting Gain"), ParameterType(Core.ParameterType.Double)] LightingGain,
        [ParameterProperties("SAM_DesignLuxLevel", "Design Lux Level"), DoubleParameterType(0)] DesignLuxLevel,
    }
}