// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core.Reporting;
using SAM.Units;

namespace SAM.Analytical.Reporting
{
    /// <summary>
    /// Room set points (from the internal condition profiles) and outdoor design conditions (from the model's design
    /// days). These are authored assumptions, not simulated room states.
    /// </summary>
    public sealed class SpaceDesignCriteriaData
    {
        /// <summary>
        /// Heating set point [°C]: yearly maximum of the heating profile. Not applicable when it is SAM's "No Heating"
        /// value all year (<see cref="Query.NoHeatingSetPoint"/>).
        /// </summary>
        public ReportValue<Quantity> HeatingSetPoint { get; init; }

        /// <summary>
        /// Cooling set point [°C]: yearly minimum of the cooling profile. Not applicable when it is SAM's "No Cooling"
        /// value all year (<see cref="Query.NoCoolingSetPoint"/>).
        /// </summary>
        public ReportValue<Quantity> CoolingSetPoint { get; init; }

        /// <summary>
        /// Humidification set point [%RH]: the zone humidity lower limit (Tas ticHLL), the yearly maximum of the
        /// humidification profile. Not applicable when the limit is 0 % (Tas "no humidification").
        /// </summary>
        public ReportValue<Quantity> HumidificationSetPoint { get; init; }

        /// <summary>
        /// Dehumidification set point [%RH]: the zone humidity upper limit (Tas ticHUL), the yearly minimum of the
        /// dehumidification profile. Not applicable when the limit is 100 % (Tas "no dehumidification").
        /// </summary>
        public ReportValue<Quantity> DehumidificationSetPoint { get; init; }

        /// <summary>
        /// Lowest dry bulb [°C] over the heating design days.
        /// </summary>
        public ReportValue<Quantity> OutdoorHeatingDryBulb { get; init; }

        /// <summary>
        /// Relative humidity [%] coincident with <see cref="OutdoorHeatingDryBulb"/>.
        /// </summary>
        public ReportValue<Quantity> OutdoorHeatingRelativeHumidity { get; init; }

        /// <summary>
        /// Highest dry bulb [°C] over the cooling design days.
        /// </summary>
        public ReportValue<Quantity> OutdoorCoolingDryBulb { get; init; }

        /// <summary>
        /// Relative humidity [%] coincident with <see cref="OutdoorCoolingDryBulb"/>.
        /// </summary>
        public ReportValue<Quantity> OutdoorCoolingRelativeHumidity { get; init; }
    }
}
