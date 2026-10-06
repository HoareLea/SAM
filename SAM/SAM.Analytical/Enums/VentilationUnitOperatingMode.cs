// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical.Enums
{
    /// <summary>
    /// Which of a ventilation unit's stated operating modes its manufacturer's control logic selects for
    /// one hour's intake and extract air temperatures.
    /// <para>
    /// <b>Mutually exclusive by construction.</b> A manufacturer describing a hybrid unit typically states
    /// its modes as several independent "if this, run that" conditions which it then asserts can never be
    /// true together. <see cref="VentilationUnitOperatingStrategy.OperatingMode"/> evaluates them as one
    /// ordered decision instead, so exactly one mode is selected for every pair of temperatures - including
    /// the pairs at which independently-written conditions would disagree or would all be false. Which
    /// conditions those are, and what the fall-through is, is stated on the strategy as data.
    /// </para>
    /// <para>
    /// <b>This is an operating state, not a product feature and not a compliance mode.</b> It says what the
    /// unit is doing in one hour. It is not <c>PartOVentilationMode</c>, which says how a dwelling is
    /// assessed.
    /// </para>
    /// </summary>
    [Description("Ventilation Unit Operating Mode")]
    public enum VentilationUnitOperatingMode
    {
        /// <summary>
        /// Nothing was selected - the strategy could not be read, or a temperature was not a number. Never
        /// a mode a unit runs in: a caller that receives this has a refusal to report, not a state to
        /// simulate.
        /// </summary>
        [Description("Undefined")] Undefined,

        /// <summary>
        /// The unit recovers heat - or coolth - between its extract and intake air streams. The default
        /// state: everything that is neither bypass nor cooling.
        /// </summary>
        [Description("Heat / coolth recovery")] HeatCoolthRecovery,

        /// <summary>
        /// The unit passes intake air without recovery, because recovering against warmer extract air would
        /// push the supply the wrong way while the dwelling is still below the cooling activation
        /// temperature.
        /// </summary>
        [Description("Summer bypass")] SummerBypass,

        /// <summary>
        /// The unit's cooling module is running, at the elevated airflow the manufacturer states for it.
        /// </summary>
        [Description("Cooling")] Cooling,
    }
}
