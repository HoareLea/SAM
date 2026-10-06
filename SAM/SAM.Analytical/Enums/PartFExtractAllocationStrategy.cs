// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical.Enums
{
    /// <summary>
    /// How continuous extract above the Table 1.2 minimums is shared between the dwelling's extract
    /// terminals.
    /// <para>
    /// Approved Document F, Volume 1: Dwellings (2021 edition, England) prescribes two things only: each
    /// wet room reaches at least its Table 1.2 minimum high rate (paragraph 1.70, page 17), and the sum
    /// of all extract on its continuous rate is at least the whole dwelling ventilation rate (Table 1.2
    /// continuous rate column, page 10). Everything above those two constraints is an engineering
    /// strategy, not a regulatory requirement, so the strategy is named, recorded on the result and can
    /// be changed.
    /// </para>
    /// </summary>
    public enum PartFExtractAllocationStrategy
    {
        /// <summary>
        /// Every extract terminal takes its Table 1.2 minimum first, then all remaining continuous
        /// extract goes to the local kitchen extract terminals, in proportion to their minimums where
        /// there is more than one.
        /// <para>
        /// The default. The cooking function is the dwelling's largest single source of moisture and
        /// cooking pollutants, so concentrating the surplus there removes them closest to source, which
        /// is the stated aim of extract ventilation in requirement F1(1)(a). Where the dwelling has no
        /// local kitchen extract terminal in the balanced flow, this falls back to
        /// <see cref="VolumeWeighted"/> so the surplus is still distributed.
        /// </para>
        /// </summary>
        [Description("Minimum First, Cooking Priority")] MinimumFirstCookingPriority,

        /// <summary>
        /// Every extract terminal takes its Table 1.2 minimum first, then the remaining continuous
        /// extract is shared between the extract terminals in proportion to room volume.
        /// <para>
        /// The strategy SAM used before terminal-level sizing existed. Retained so an existing model can
        /// reproduce its previous extract rates exactly.
        /// </para>
        /// </summary>
        [Description("Volume Weighted")] VolumeWeighted,
    }
}
