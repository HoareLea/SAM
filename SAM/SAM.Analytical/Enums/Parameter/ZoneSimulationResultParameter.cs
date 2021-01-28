using System.ComponentModel;
using SAM.Core.Attributes;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(ZoneSimulationResult)), Description("Analytical Zone Simulation Result Parameter")]
    public enum ZoneSimulationResultParameter
    {
        [ParameterProperties("Area", "Area [m2]"), DoubleParameterValue(0)] Area,
        [ParameterProperties("Volume", "Volume [m3]"), DoubleParameterValue(0)] Volume,
        [ParameterProperties("Occupancy", "Occupancy [p]"), DoubleParameterValue(0)] Occupancy,
        [ParameterProperties("Max Cooling Sensible Load", "Max Cooling Sensible Load"), DoubleParameterValue(0)] MaxCoolingSensibleLoad,
        [ParameterProperties("Max Cooling Sensible Load Index", "Max Cooling Sensible Load Index"), IntegerParameterValue(0, 8759)] MaxCoolingSensibleLoadIndex,

        [ParameterProperties("Simulation Type", "Simulation Type"), ParameterValue(Core.ParameterType.String)] SimulationType,
    }
}