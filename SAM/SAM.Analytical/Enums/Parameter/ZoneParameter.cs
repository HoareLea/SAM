using System.ComponentModel;
using SAM.Core.Attributes;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(Zone)), Description("Analytical Zone Parameter")]
    public enum ZoneParameter
    {
        [ParameterProperties("Zone Category", "Zone Category"), ParameterValue(Core.ParameterType.String)] ZoneCategory,

        [ParameterProperties("Max Cooling Sensible Load", "Max Cooling Sensible Load"), DoubleParameterValue(0)] MaxCoolingSensibleLoad,
        [ParameterProperties("Max Cooling Sensible Load Index", "Max Cooling Sensible Load Index"), IntegerParameterValue(0, 8759)] MaxCoolingSensibleLoadIndex,
    }
}