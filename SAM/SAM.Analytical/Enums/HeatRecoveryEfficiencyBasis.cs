// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical.Enums
{
    /// <summary>
    /// What a published heat recovery efficiency is a ratio <i>of</i>.
    /// <para>
    /// <b>An efficiency without a basis is not a number anybody can use.</b> A supply-side temperature
    /// ratio, an extract-side one, an enthalpy ratio and a "dry" versus "wet" figure are all published as
    /// "efficiency" and all differ, sometimes by tens of percent, for the same unit at the same airflow.
    /// The basis is therefore stated beside the figures and checked, never assumed - see
    /// <see cref="HeatRecoveryPerformance"/>, which refuses data whose basis is
    /// <see cref="Undefined"/>.
    /// </para>
    /// </summary>
    [Description("Heat Recovery Efficiency Basis")]
    public enum HeatRecoveryEfficiencyBasis
    {
        /// <summary>Nothing was stated. Never usable - the data carrying it is refused.</summary>
        [Description("Undefined")] Undefined,

        /// <summary>
        /// The supply-air temperature ratio at balanced airflow:
        /// <c>(T_supply,out - T_outdoor) / (T_extract,in - T_outdoor)</c>.
        /// <para>
        /// The ratio the product-testing standards publish a domestic heat recovery unit's thermal
        /// efficiency on (EN 13141-7, and the certified figures the SAP Product Characteristics Database
        /// lists), measured with supply and extract airflow equal. A figure on this basis says nothing
        /// about an unbalanced duty, which is why the resolver refuses one - see
        /// <c>Query.VentilationUnitOperatingParameters</c>.
        /// </para>
        /// </summary>
        [Description("Supply Temperature Efficiency")] SupplyTemperatureEfficiency,
    }
}
