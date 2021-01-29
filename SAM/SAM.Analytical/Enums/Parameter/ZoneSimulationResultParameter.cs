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
        [ParameterProperties("Air Movement Gain", "Air Movement Gain [W]"), DoubleParameterValue()] AirMovementGain,
        [ParameterProperties("Building Heat Transfer", "Building Heat Transfer [W]"), DoubleParameterValue()] BuildingHeatTransfer,
        [ParameterProperties("Equipment Sensible Gain", "Equipment Sensible Gain [W]"), DoubleParameterValue()] EquipmentSensibleGain,
        [ParameterProperties("Glazing External Conduction", "Glazing External Conduction [W]"), DoubleParameterValue()] GlazingExternalConduction,
        [ParameterProperties("Lighting Gain", "Lighting Gain [W]"), DoubleParameterValue()] LightingGain,
        [ParameterProperties("Infiltration Gain", "Infiltration Gain [W]"), DoubleParameterValue()] InfiltrationGain,
        [ParameterProperties("Occupancy Sensible Gain", "Occupancy Sensible Gain [W]"), DoubleParameterValue()] OccupancySensibleGain,
        [ParameterProperties("Opaque External Conduction", "Opaque External Conduction [W]"), DoubleParameterValue()] OpaqueExternalConduction,
        [ParameterProperties("Solar Gain", "Solar Gain [W]"), DoubleParameterValue()] SolarGain,

        [ParameterProperties("Load Type", "Load Type"), ParameterValue(Core.ParameterType.String)] LoadType,
    }
}