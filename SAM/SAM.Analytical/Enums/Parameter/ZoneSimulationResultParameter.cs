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
        [ParameterProperties("Max Sensible Load", "Max Sensible Load"), DoubleParameterValue(0)] MaxSensibleLoad,
        [ParameterProperties("Max Sensible Load Index", "Max Sensible Load Index"), IntegerParameterValue(0, 8759)] MaxSensibleLoadIndex,

        [ParameterProperties("Simulation Type", "Simulation Type"), ParameterValue(Core.ParameterType.String)] SimulationType,
    }
}