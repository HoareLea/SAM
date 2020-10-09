using System.ComponentModel;

namespace SAM.Analytical
{
    [Core.Attributes.Types(typeof(InternalCondition)), Description("Internal Condition Parameter")]
    public enum InternalConditionParameter
    {
        [Core.Attributes.ParameterName("SAM_NoPeople"), Core.Attributes.DoubleParameterType(0), Core.Attributes.ParameterDisplayName("Number Of People")] NumberOfPeople,
        [Core.Attributes.ParameterName("SAM_OccupacyProfile"), Core.Attributes.ParameterType(Core.ParameterType.String), Core.Attributes.ParameterDisplayName("Occupancy Profile Name")] OccupancyProfileName,
        [Core.Attributes.ParameterName("SAM_OccupantSensGain"), Core.Attributes.ParameterType(Core.ParameterType.Double), Core.Attributes.ParameterDisplayName("Occupant Sensible Gain")] OccupantSensibleGain,
        [Core.Attributes.ParameterName("SAM_OccupantLatGain"), Core.Attributes.ParameterType(Core.ParameterType.Double), Core.Attributes.ParameterDisplayName("Occupant Latent Gain")] OccupantLatentGain,
        [Core.Attributes.ParameterName("SAM_SmallPowerSensProfile"), Core.Attributes.ParameterType(Core.ParameterType.String), Core.Attributes.ParameterDisplayName("Equipment Sensible Profile Name")] EquipmentSensibleProfileName,
    }
}