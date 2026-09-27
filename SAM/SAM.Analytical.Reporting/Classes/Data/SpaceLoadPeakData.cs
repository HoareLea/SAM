// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core.Reporting;
using SAM.Units;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Reporting
{
    /// <summary>
    /// One heating or cooling peak of a space - the design-day peak or the full-year peak, never a mix of the two -
    /// as a report reads it from a typed <see cref="SpaceLoadPeak"/>. Nothing is computed here: every value is the
    /// stored one, in SAM canonical units.
    /// <para>
    /// Every available value has Source <see cref="ReportValueSource.SimulationResult"/> and Freshness Unknown: no
    /// record ties a stored result to the current design yet.
    /// </para>
    /// <para>
    /// <b>Values by <see cref="State"/>.</b> <see cref="LoadPeakState.Unavailable"/>: every value NotAvailable, with
    /// the reason, and no components. <see cref="LoadPeakState.Zero"/>: <see cref="Load"/> is an available 0 W; there
    /// is no peak timestep, so a value the result does not carry is NotApplicable. <see cref="LoadPeakState.Value"/>:
    /// a value the result does not carry is NotAvailable (not reported).
    /// </para>
    /// </summary>
    public sealed class SpaceLoadPeakData
    {
        /// <summary>Which peak this is. Set even when the peak is unavailable.</summary>
        public LoadPeakBasis Basis { get; init; }

        public LoadPeakState State { get; init; }

        /// <summary>The peak load, W, a non-negative magnitude for heating and cooling alike.</summary>
        public ReportValue<Quantity> Load { get; init; }

        /// <summary>The design day the peak came from. NotApplicable for the full-year peak.</summary>
        public ReportValue<string> DesignDayName { get; init; }

        /// <summary>0-based hour of the day of the peak timestep (0 = 00:00–01:00).</summary>
        public ReportValue<int> HourOfDay { get; init; }

        /// <summary>
        /// 0-based hour of the year (0 = 1 January 00:00–01:00). Full-year peak only: a design-day peak has no
        /// calendar date, so this is NotApplicable for it.
        /// </summary>
        public ReportValue<int> HourOfYear { get; init; }

        /// <summary>
        /// Start of the peak hour as a calendar time, full-year peak only, from <see cref="SpaceLoadPeak.TryGetDateTime"/>
        /// in <see cref="Create.ReferenceYear"/>. Results carry no year: show month, day and time only.
        /// </summary>
        public ReportValue<DateTime> Time { get; init; }

        /// <summary>Room air dry-bulb temperature at the peak, °C.</summary>
        public ReportValue<Quantity> RoomDryBulbTemperature { get; init; }

        /// <summary>Room resultant temperature at the peak, °C.</summary>
        public ReportValue<Quantity> RoomResultantTemperature { get; init; }

        /// <summary>Room relative humidity at the peak, %.</summary>
        public ReportValue<Quantity> RoomRelativeHumidity { get; init; }

        /// <summary>Room humidity ratio at the peak, kg/kg.</summary>
        public ReportValue<Quantity> RoomHumidityRatio { get; init; }

        /// <summary>Outdoor dry-bulb temperature at the peak, °C, as the simulation results record it.</summary>
        public ReportValue<Quantity> OutdoorDryBulbTemperature { get; init; }

        /// <summary>Outdoor relative humidity at the peak, %, as the simulation results record it.</summary>
        public ReportValue<Quantity> OutdoorRelativeHumidity { get; init; }

        /// <summary>The sensible heat-balance terms the result stores, signed, in <see cref="LoadPeakComponent"/> order.</summary>
        public IReadOnlyList<SpaceLoadPeakComponentData> SensibleComponents { get; init; }

        /// <summary>The latent terms the result stores, signed, kept apart from the sensible ones.</summary>
        public IReadOnlyList<SpaceLoadPeakComponentData> LatentComponents { get; init; }
    }
}
