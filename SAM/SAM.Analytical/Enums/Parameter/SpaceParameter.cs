using System.ComponentModel;
using SAM.Core.Attributes;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(Space)), Description("Space Parameter")]
    public enum SpaceParameter
    {
        [ParameterProperties("Design Heating Load", "Design Heating Load [W]"), DoubleParameterValue(0)] DesignHeatingLoad,
        [ParameterProperties("Design Cooling Load", "Design Cooling Load [W]"), DoubleParameterValue(0)] DesignCoolingLoad,
        //[ParameterProperties("Specified Exhaust Airflow", "Extract Airflow"), DoubleParameterValue(0)] ExtractAirflow,
        //[ParameterProperties("Specified Supply Airflow", "Supply Airflow"), DoubleParameterValue(0)] SupplyAirflow,
        [ParameterProperties("Volume", "Volume [m3]"), DoubleParameterValue(0)] Volume,
        [ParameterProperties("Area", "Area [m2]"), DoubleParameterValue(0)] Area,
        [ParameterProperties("Occupancy", "Occupancy [p]"), DoubleParameterValue(0)] Occupancy,
        [ParameterProperties("Facing External", "Facing External"), ParameterValue(Core.ParameterType.Boolean)] FacingExternal,
        [ParameterProperties("Facing External Glazing", "Facing External Glazing"), ParameterValue(Core.ParameterType.Boolean)] FacingExternalGlazing,
        [ParameterProperties("Level Name", "Level Name"), ParameterValue(Core.ParameterType.String)] LevelName,
        [ParameterProperties("Cooling Sizing Factor", "Cooling Sizing Factor"), DoubleParameterValue(0)] CoolingSizingFactor,
        [ParameterProperties("Heating Sizing Factor", "Heating Sizing Factor"), DoubleParameterValue(0)] HeatingSizingFactor,
        [ParameterProperties("Ventilation Riser Name", "Ventilation Riser Name"), ParameterValue(Core.ParameterType.String)] VentilationRiserName,
        [ParameterProperties("Heating Riser Name", "Heating Riser Name"), ParameterValue(Core.ParameterType.String)] HeatingRiserName,
        [ParameterProperties("Cooling Riser Name", "Cooling Riser Name"), ParameterValue(Core.ParameterType.String)] CoolingRiserName,
        //[ParameterProperties("Ventilation Zone Name", "Ventilation Zone Name"), ParameterValue(Core.ParameterType.String)] VentilationZoneName,
        //[ParameterProperties("Heating Zone Name", "Heating Zone Name"), ParameterValue(Core.ParameterType.String)] HeatingZoneName,
        //[ParameterProperties("Cooling Zone Name", "Cooling Zone Name"), ParameterValue(Core.ParameterType.String)] CoolingZoneName,

        [ParameterProperties("Outside Supply Airflow", "Outside Supply Airflow [m3/s]"), DoubleParameterValue(0)] OutsideSupplyAirFlow,
        [ParameterProperties("Supply Airflow", "Supply Airflow [m3/s]"), DoubleParameterValue(0)] SupplyAirFlow,
        [ParameterProperties("Exhaust Airflow", "Exhaust Airflow [m3/s]"), DoubleParameterValue(0)] ExhaustAirFlow,
    }
}