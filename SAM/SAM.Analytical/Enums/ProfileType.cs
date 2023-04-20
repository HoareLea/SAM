 using System.ComponentModel;

namespace SAM.Analytical
{
    [Description("Analytical Profile Type.")]
    public enum ProfileType
    {
        [Description("Undefined")] Undefined,
        [Description("Occupancy")] Occupancy,
        [Description("Equipment Sensible")] EquipmentSensible,
        [Description("Equipment Latent")] EquipmentLatent,
        [Description("Lighting")] Lighting,
        [Description("Infiltration")] Infiltration,
        [Description("Pollutant")] Pollutant,
        [Description("Heating")] Heating,
        [Description("Cooling")] Cooling,
        [Description("Humidification")] Humidification,
        [Description("Dehumidification")] Dehumidification,
        [Description("Ventilation")] Ventilation,
        [Description("Other")] Other,
    }
}