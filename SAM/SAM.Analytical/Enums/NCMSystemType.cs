using System.ComponentModel;

namespace SAM.Analytical
{
    [Description("NCM System Type")]
    public enum NCMSystemType
    {
        [Description("Undefined")] Undefined,
        [Description("Active Chilled Beams")] ActiveChilledBeams,
        [Description("CAV")] CAV,
        [Description("CAV (Terminal)")] CAV_Terminal,
        [Description("CAV (With Reheat)")] CAV_WithReheat,
        [Description("Displacement Ventilation")] DisplacementVentilation,
        [Description("Fancoil")] Fancoil,
        [Description("FATVAV")] FATVAV,
        [Description("Mixed Mode")] MixedMode,
        [Description("VAV")] VAV,
        [Description("VRF")] VRF,
        [Description("Extract Only")] ExtractOnly,
        [Description("Mechanical Ventilation")] MechanicalVentilation,
        [Description("Natural Ventilation")] NaturalVentilation,
        [Description("Occupied and Unheated")] OccupiedAndUnheated,
    }
}