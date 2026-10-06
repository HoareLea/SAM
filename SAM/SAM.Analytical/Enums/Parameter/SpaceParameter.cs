// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core.Attributes;
using System.ComponentModel;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(Space)), Description("Space Parameter")]
    public enum SpaceParameter
    {
        [ParameterProperties("Color", "Color"), ParameterValue(Core.ParameterType.Color)] Color,

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

        [ParameterProperties("Outside Supply Air Flow", "Outside Supply Air Flow [m3/s]"), DoubleParameterValue(0)] OutsideSupplyAirFlow,
        [ParameterProperties("Supply Air Flow", "Supply Air Flow [m3/s]"), DoubleParameterValue(0)] SupplyAirFlow,
        [ParameterProperties("Exhaust Air Flow", "Exhaust Air Flow [m3/s]"), DoubleParameterValue(0)] ExhaustAirFlow,

        [ParameterProperties("Daylight Factor", "Daylight Factor [-]"), DoubleParameterValue(0)] DaylightFactor,

        [ParameterProperties("PartF Space Data", "PartF Space Data"), SAMObjectParameterValue(typeof(PartFSpaceData))] PartFSpaceData,

        /// <summary>
        /// Shared semantic classification of the space - what the space is, and the independent
        /// semantic roles that follow from it. Written by the space use mapping and read by Approved
        /// Document F, Approved Document O and CIBSE TM59, so a space is classified once and reused.
        /// </summary>
        [ParameterProperties("Space Semantics", "Space Semantics"), SAMObjectParameterValue(typeof(SpaceSemantics))] SpaceSemantics,

        /// <summary>
        /// Explicit user override of the semantic classification, holding a <see cref="SpaceUse"/>
        /// name. Highest authority: when set, no name matching is attempted. An unrecognised value is
        /// reported rather than ignored.
        /// </summary>
        [ParameterProperties("Space Use Override", "Space Use Override"), ParameterValue(Core.ParameterType.String)] SpaceUseOverride,

        /// <summary>
        /// How the local kitchen or cooker extract required by Approved Document F, Volume 1 (2021
        /// edition) paragraph 1.17a is actually provided in this cooking space, holding a
        /// <see cref="Enums.PartFExtractMethod"/> name.
        /// <para>
        /// An engineering input, because it cannot be derived from the room: a studio with a mechanical
        /// ventilation with heat recovery extract terminal over the hob, a studio with a cooker hood
        /// ducted outside, and a studio with a recirculating hood are three different designs with three
        /// different requirements, and the analytical model looks identical in all three.
        /// </para>
        /// <para>
        /// Absent means the design has not said, in which case the mechanical ventilation with heat
        /// recovery terminal of paragraph 1.70 is assumed, since that is the arrangement the rest of the
        /// system implies, and the assumption is reported as needing confirmation rather than passed
        /// silently.
        /// </para>
        /// </summary>
        [ParameterProperties("PartF Local Extract Method", "PartF Local Extract Method"), ParameterValue(Core.ParameterType.String)] PartFLocalExtractMethod,
    }
}
