// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical
{
    /// <summary>
    /// One term of a space's heat balance at a <see cref="SpaceLoadPeak"/>, in W. Every term is signed as the
    /// simulation engine reports it: <b>positive is a gain to the room air, negative a loss</b>. A term is
    /// present on a peak only when the engine reported it for that timestep.
    /// <para>
    /// The sensible terms are every member except <see cref="OccupancyLatent"/> and <see cref="EquipmentLatent"/>.
    /// </para>
    /// </summary>
    [Description("Load Peak Component.")]
    public enum LoadPeakComponent
    {
        [Description("Undefined")] Undefined,
        [Description("Solar Gain")] Solar,
        [Description("Lighting Gain")] Lighting,
        [Description("Occupancy Sensible Gain")] OccupancySensible,
        [Description("Equipment Sensible Gain")] EquipmentSensible,
        [Description("Infiltration and Ventilation Gain")] InfiltrationVentilation,
        [Description("Air Movement Gain")] AirMovement,
        [Description("Building Heat Transfer")] BuildingHeatTransfer,
        [Description("Opaque External Conduction")] ExternalConductionOpaque,
        [Description("Glazing External Conduction")] ExternalConductionGlazing,
        [Description("Air Handling Unit Gain")] AirHandlingUnit,
        [Description("Occupancy Latent Gain")] OccupancyLatent,
        [Description("Equipment Latent Gain")] EquipmentLatent,
    }
}
