using System.ComponentModel;
using SAM.Core.Attributes;

namespace SAM.Analytical
{
    [Types(typeof(InternalCondition)), Description("Internal Condition Parameter")]
    public enum InternalConditionParameter
    {
        [ParameterName("SAM_NoPeople"), DoubleParameterType(0), ParameterDisplayName("Number Of People")] NumberOfPeople,
        [ParameterName("SAM_OccupacyProfile"), ParameterType(Core.ParameterType.String), ParameterDisplayName("Occupancy Profile Name")] OccupancyProfileName,
        [ParameterName("SAM_OccupantSensGain"), ParameterType(Core.ParameterType.Double), ParameterDisplayName("Occupant Sensible Gain")] OccupantSensibleGain,
        [ParameterName("SAM_OccupantLatGain"), ParameterType(Core.ParameterType.Double), ParameterDisplayName("Occupant Latent Gain")] OccupantLatentGain,
        [ParameterName("SAM_SmallPowerSensProfile"), ParameterType(Core.ParameterType.String), ParameterDisplayName("Equipment Sensible Profile Name")] EquipmentSensibleProfileName,
    }
}