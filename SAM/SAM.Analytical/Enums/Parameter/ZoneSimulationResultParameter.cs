using System.ComponentModel;
using SAM.Core.Attributes;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(ZoneSimulationResult)), Description("Analytical Zone Simulation Result Parameter")]
    public enum ZoneSimulationResultParameter
    {
        [ParameterProperties("Max Cooling Sensible Load", "Max Cooling Sensible Load"), DoubleParameterValue(0)] MaxCoolingSensibleLoad,
        [ParameterProperties("Max Cooling Sensible Load Index", "Max Cooling Sensible Load Index"), IntegerParameterValue(0, 8759)] MaxCoolingSensibleLoadIndex,
    }
}