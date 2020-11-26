using System.ComponentModel;

namespace SAM.Analytical
{
    /// <summary>
    /// https://www.edsl.net/htmlhelp/Building_Simulator/ [Radiant Property Parameters, Calculation of View Coefficients]
    /// </summary>
    [Description("Analytical Emitter Type.")]
    public enum EmitterType
    {
        [Description("Undefined")] Undefined,
        [Description("Warm Air Heater")] WarmAirHeater,
        [Description("Air Conditioning (Heating)")] AirConditioning_Heating,
        [Description("Natrural Convector")] NaturalConvector,
        [Description("Radiator Type Convector")] RadiatorTypeConvector,
        [Description("Multi-Column Radiator")] MultiColumnRadiator,
        [Description("Double or Treble Radiator")] DoubleOrTrebleRadiator,
        [Description("Double Column Radiator")] DoubleColumnRadiator,
        [Description("Single Column Radiator")] SingleColumnRadiator,
        [Description("Floor Warming System")] FloorWarmingSystem,
        [Description("Block Storage Heater")] BlockStorageHeater,
        [Description("Wall Panel Heater")] WallPanelHeater,
        [Description("Ceiling Panel Heater")] CeilingPanelHeater,
        [Description("High Temperature Radiant System")] HighTemperatureRadiantSystem,
        [Description("Air Conditioning (Cooling)")] AirConditioning_Cooling,
        [Description("Wall Panel Cooler")] WallPanelCooler,
        [Description("Ceiling Panel Cooler")] CeilingPanelCooler,
        [Description("Chilled Beam")] ChilledBeam,
        [Description("Tungsten Ceiling Light")] TungstenCeilingLight,
        [Description("Tungsten Task Light")] TungstenTaskLight,
        [Description("Tungsten Ceiling Light")] FluorescentCeilingLight,
        [Description("Fluorescent Task Light")] FluorescentTaskLight,
        [Description("Occupant")] Occupant,
        [Description("Equipment Distributed Over")] EquipmentDistributedOver,
        [Description("Equipment Distributed")] EquipmentDistributed,
    }
}