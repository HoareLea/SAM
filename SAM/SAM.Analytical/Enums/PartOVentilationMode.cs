// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical.Enums
{
    /// <summary>
    /// The ventilation route an Approved Document O assessment is being made over - the engineering
    /// statement of how the dwelling is ventilated, as opposed to what a physical system object on the
    /// model happens to be called.
    /// <para>
    /// <b>This sits between the regulatory requirement and the equipment.</b> Approved Document F
    /// calculates <i>what the dwelling needs</i>; the route says <i>how it is provided</i>; equipment
    /// selection then finds a unit that meets the requirement on the routes that have equipment. Those are
    /// three different facts, and collapsing any two of them is how a regulatory airflow ends up derived
    /// from an MVHR unit's capacity, or an MVHR system ends up invented for a dwelling that has none. See
    /// <c>documentation/PartO-ARCHITECTURE.md</c>.
    /// </para>
    /// <para>
    /// <b>The route is stated, never inferred.</b> It is not read from
    /// <c>InternalCondition.VentilationSystemTypeName</c>, from a <c>SAM_System</c>, or from a
    /// <c>SystemTemplate</c> that happens to be on the model - all of which are metadata that may be stale,
    /// may predate the assessment, and may describe a different design stage. Those remain useful as
    /// evidence and as a future validation cross-check; none of them is authority here. Equally, nothing
    /// in the Part O preparation <i>writes</i> them to force a route: that would put the decision straight
    /// back into the metadata it was taken out of.
    /// </para>
    /// <para>
    /// <b>There is no fallback member and there must never be one.</b> A route that was not stated, was
    /// stated as something this enum has no meaning for, or was stated differently by different zones is
    /// <see cref="Undefined"/>, and <see cref="Undefined"/> refuses. The rule
    /// <c>anything that is not "NV" is mechanical</c> is what this type exists to delete.
    /// </para>
    /// </summary>
    [Description("Part O Ventilation Mode.")]
    public enum PartOVentilationMode
    {
        /// <summary>
        /// No route is settled. Nothing was stated, what was stated has no Part O meaning, or the assessed
        /// zones disagree. <b>Refuses</b> - it is never read as mechanical and never read as natural.
        /// </summary>
        [Description("Undefined")] Undefined,

        /// <summary>
        /// Natural ventilation - Iteration 1b, <see cref="PartOIteration.BaseNaturalVentilation"/>.
        /// <para>
        /// No continuous mechanical supply and no continuous mechanical extract are applied. Opening and
        /// background ventilation remain available and the model's authored opening behaviour is
        /// untouched. Intermittent extract stays a separate concept and is not turned into a continuous
        /// flow. The scenario and the TAS export both state natural ventilation.
        /// </para>
        /// </summary>
        [Description("Natural Ventilation")] NaturalVentilation,

        /// <summary>
        /// Mechanical ventilation with heat recovery - Iteration 1a,
        /// <see cref="PartOIteration.BasePassive"/> until that member is renamed.
        /// <para>
        /// The Approved Document F continuous supply and extract requirement is applied, and a physical
        /// MVHR unit is later selected to satisfy it. The scenario and the TAS export both state
        /// mechanical ventilation.
        /// </para>
        /// <para>
        /// <b>Only heat-recovery-shaped mechanical ventilation belongs here.</b> What
        /// <c>PartFCalculator</c> sizes is System 4 - a mechanical supply terminal in every habitable room
        /// and a continuous extract terminal in every wet room. Continuous mechanical extract alone
        /// (System 3) is a different building, and applying System 4 rates to it invents the supply half.
        /// </para>
        /// </summary>
        [Description("MVHR")] MVHR,
    }
}
