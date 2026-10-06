// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;

namespace SAM.Analytical.Enums
{
    /// <summary>
    /// What a ventilation system is able to do, as far as choosing one requires knowing.
    /// <para>
    /// <b>Capabilities, not equipment.</b> Nothing here is an identity: the identity of a system is
    /// <c>SystemTemplate</c> and remains so. In particular <see cref="HeatRecovery"/> is a property of
    /// <c>MVRE</c>, not a rival name for it - <c>MVRE</c> already <i>is</i> SAM's heat-recovery
    /// ventilation, and an <c>MVHR</c> identity alongside it would split one established concept in two.
    /// For the same reason <see cref="Boost"/> and <see cref="SummerBypass"/> are here rather than in the
    /// system vocabulary: they are operating states a system either supports or does not.
    /// </para>
    /// <para>
    /// <b>Deliberately small.</b> Only what a selection turns on. This is not a description of a plant
    /// room, and it is not trying to be: the detailed equipment network lives in the
    /// <c>SystemEnergyCentre</c> templates, which choosing a system must never have to open.
    /// </para>
    /// <para>
    /// <b>Which of these Approved Document F asks for.</b> Part F asks for continuous ventilation and, where
    /// a room's high rate exceeds it, for boost. It asks for neither summer bypass nor heat recovery - those
    /// are stated by a Part O scenario as mitigation, and no rule derived from a Part F assessment may
    /// invent them.
    /// </para>
    /// </summary>
    [Flags]
    public enum SystemCapability
    {
        /// <summary>Nothing stated.</summary>
        None = 0,

        /// <summary>Runs continuously at a design rate - Approved Document F systems 1, 3 and 4.</summary>
        ContinuousVentilation = 1,

        /// <summary>
        /// Supplies air mechanically, rather than only extracting it - the difference between Approved
        /// Document F system 4 and system 3.
        /// <para>
        /// <b>Added after a review found a real misselection.</b> Without it, a dwelling designed with
        /// balanced mechanical supply and extract - paragraph 1.67, a supply terminal in every habitable
        /// room - was met by a <c>Local Extract Only</c> template, because extract-only satisfies
        /// "continuous, and can boost". The overheating simulation would then have run a system with no
        /// supply and no heat recovery against a building that has both, which is exactly the outcome
        /// <c>SystemCapabilitySelection</c> exists to prevent.
        /// </para>
        /// </summary>
        MechanicalSupply = 16,

        /// <summary>
        /// Can be raised above its continuous rate on demand, for a wet room's Table 1.2 high rate.
        /// </summary>
        Boost = 2,

        /// <summary>
        /// Can route supply air around its heat exchanger, so that recovering heat in summer does not make
        /// overheating worse. An operating state of a heat-recovery system, not a system in itself.
        /// </summary>
        SummerBypass = 4,

        /// <summary>
        /// Recovers heat between extract and supply - what distinguishes <c>MVRE</c> from <c>MV</c>.
        /// <para>
        /// Present so that a system which recovers heat counts as <b>more</b> capable than one that does
        /// not: a selection asked only for continuous ventilation then returns the simpler system rather
        /// than being unable to tell them apart and falling back to whichever came first in a list.
        /// </para>
        /// </summary>
        HeatRecovery = 8
    }
}
