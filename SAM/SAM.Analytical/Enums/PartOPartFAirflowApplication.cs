// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical.Enums
{
    /// <summary>
    /// Whether the Approved Document F continuous mechanical airflows may be carried onto the internal
    /// conditions a Part O iteration simulates, decided from the <see cref="PartOVentilationMode"/> the
    /// assessment states.
    /// <para>
    /// <b>Why this decision has to exist.</b> <c>PartFCalculator</c> is unconditionally System 4 shaped:
    /// paragraph 1.67 gives every habitable room a mechanical supply terminal and every wet room a
    /// continuous extract terminal, with no input anywhere for how the dwelling is actually ventilated.
    /// <see cref="Modify.ApplyPartFVentilationRates(AnalyticalModel, PartFOperatingMode, out System.Collections.Generic.List{string}, out System.Collections.Generic.List{string})"/>
    /// then writes those rates onto <c>InternalConditionParameter.SupplyAirFlow</c> and
    /// <c>ExhaustAirFlow</c> - the values the simulation and the TAS export read. Applied to a dwelling
    /// that is not on the MVHR route, that produces a successful run of a building that does not exist,
    /// which is worse than a refusal: nothing in the result says the mechanical system was invented.
    /// </para>
    /// <para>
    /// <b>This is a gate, not a sizing rule.</b> Skipping the application means SAM has not invented an
    /// MVHR or MVRE system for a naturally ventilated dwelling. It does <b>not</b> mean the dwelling's
    /// natural-ventilation Part F design - System 1 background ventilators, purge provision - has been
    /// sized. Nothing here sizes anything.
    /// </para>
    /// </summary>
    [Description("Part O Part F Airflow Application.")]
    public enum PartOPartFAirflowApplication
    {
        /// <summary>No decision was reached.</summary>
        [Description("Undefined")] Undefined,

        /// <summary>
        /// Carry the Part F continuous mechanical rates onto the internal conditions. Reached from
        /// <see cref="PartOVentilationMode.MVHR"/> and from nothing else - in particular, never from the
        /// absence of a stated route.
        /// </summary>
        [Description("Apply")] Apply,

        /// <summary>
        /// Apply nothing. The assessment states <see cref="PartOVentilationMode.NaturalVentilation"/>, so
        /// there is no continuous mechanical supply or extract to carry, and absence of
        /// <c>PartFSpaceData</c> is not an error on this path.
        /// </summary>
        [Description("Skip - Natural Ventilation")] SkipNaturalVentilation,

        /// <summary>
        /// Refuse, and prepare nothing. The assessed zones did not settle on exactly one Part O
        /// ventilation route: none was stated, what was stated has no Part O meaning (<c>MV</c>, <c>UV</c>,
        /// an unrecognised word, an empty panel), or different zones stated different routes.
        /// <para>
        /// <b>All of those are one answer on purpose.</b> Each is an absence of a settled route, and the
        /// only safe thing to do with an absence is nothing - the alternative, which this replaced, was to
        /// keep applying and let an unstated route write System 4 supply and extract onto every sized
        /// space. Which absence it was is in the refusal text, which names each zone and what it stated.
        /// </para>
        /// <para>
        /// The mixed case additionally has no third option to reach for:
        /// <see cref="Modify.ApplyPartFVentilationRates(AnalyticalModel, PartFOperatingMode, out System.Collections.Generic.List{string}, out System.Collections.Generic.List{string})"/>
        /// is whole-model rather than zone-scoped, so applying would put mechanical airflow into the
        /// naturally ventilated zones and skipping would strip it from the MVHR ones. Making it zone-scoped
        /// is a separate change with its own transfer-air and balance consequences.
        /// </para>
        /// </summary>
        [Description("Refuse - Unstated Route")] RefuseUnstatedRoute,
    }
}
