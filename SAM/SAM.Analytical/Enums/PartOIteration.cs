// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Analytical.Enums
{
    /// <summary>
    /// The Approved Document O operating iteration a scenario states - the mitigation stage the building is
    /// being assessed at.
    /// <para>
    /// Part O is assessed in stages: a building is tested with its base provision first, and further
    /// mitigation is added only where that fails. Each stage is a genuinely different set of operating
    /// assumptions over the same fabric, so it is a different assessment of the same dwelling and must
    /// carry a different identity. That is the whole reason this appears in the scenario key.
    /// </para>
    /// <para>
    /// <b>The base provision has two alternative forms, not one.</b> <see cref="BasePassive"/> is the base
    /// MVHR configuration (Iteration 1a) and <see cref="BaseNaturalVentilation"/> is the base natural
    /// ventilation one (Iteration 1b). A dwelling is assessed at one or the other according to its
    /// <see cref="PartOVentilationMode"/> - they are alternatives, and only
    /// <see cref="AcousticRestricted"/> onwards are further mitigation.
    /// </para>
    /// <para>
    /// <b>Identity only.</b> No member of this enum causes any behaviour anywhere. Naming a stage here is
    /// not implementing it: the operating assumptions that make a stage what it is live in
    /// <c>OverheatingOperatingAssumptions</c>, and the ones for the mitigated stages are not written yet.
    /// </para>
    /// <para>
    /// <b>There is deliberately no foundation-stage member.</b> The foundation work is a development stage
    /// of SAM, not an operating scenario of a building, and giving it a member would put a fact about this
    /// codebase's schedule into an engineering identity that outlives it - and would then have to be
    /// migrated or kept forever. A scenario built during that work states <see cref="Undefined"/>, which is
    /// true: it has not chosen an operating iteration.
    /// </para>
    /// </summary>
    public enum PartOIteration
    {
        /// <summary>
        /// No operating iteration is stated. The honest value for a scenario built to exercise the
        /// machinery rather than to assess a building against a mitigation stage.
        /// </summary>
        Undefined,

        /// <summary>
        /// <b>Iteration 1a - base MVHR.</b> Openings operated without restriction, mechanical ventilation
        /// at its design continuous rate. Nothing has been added to mitigate overheating.
        /// <para>
        /// <b>The name is older than the concept it now carries.</b> This is the base configuration for
        /// <see cref="PartOVentilationMode.MVHR"/>, and <c>BaseMVHR</c> is what it should be called. It is
        /// not renamed here because the member NAME is inside the derived <c>OverheatingScenario.Key</c>,
        /// so renaming re-keys every assessment ever attributed to it - a migration, not an edit. See
        /// <c>documentation/PartO-ARCHITECTURE.md</c> section 8.
        /// </para>
        /// </summary>
        BasePassive,

        /// <summary>
        /// Openings restricted for noise, with the mechanical system's boost and summer bypass states
        /// available to compensate.
        /// </summary>
        AcousticRestricted,

        /// <summary>
        /// Active trim cooling added on top of the ventilation provision.
        /// </summary>
        ActiveTrimCooling,

        /// <summary>
        /// <b>Iteration 1b - base natural ventilation.</b> Openings operated as the model authors them, no
        /// continuous mechanical supply and no continuous mechanical extract. Nothing has been added to
        /// mitigate overheating.
        /// <para>
        /// <b>This is an alternative to <see cref="BasePassive"/>, not a stage after it.</b> 1a and 1b are
        /// two base configurations of the same dwelling, and a dwelling is assessed at one or the other
        /// according to its <see cref="PartOVentilationMode"/>. <see cref="AcousticRestricted"/> is the
        /// first member that is genuinely further mitigation. Reading this enum as an ordered list of
        /// stages gets the engineering wrong.
        /// </para>
        /// <para>
        /// <b>Why this could not be <see cref="BasePassive"/>.</b> An iteration's operating assumptions are
        /// part of the permanent <c>OverheatingScenario.Key</c>, and <see cref="BasePassive"/> asserts
        /// <c>Mechanical Ventilation At Design Rate = True</c>. Attributing a naturally ventilated result
        /// to it would mint a permanent identity stating something false about the building - so the
        /// alternative to adding this member was not reuse, it was a lie.
        /// </para>
        /// <para>
        /// <b>Appended rather than placed beside <see cref="BasePassive"/> on purpose.</b> Inserting it
        /// would renumber <see cref="AcousticRestricted"/> and <see cref="ActiveTrimCooling"/>, which are
        /// persisted in <c>OverheatingScenario</c> JSON. The key derives from the NAME, so position carries
        /// no engineering meaning and the reading order of this file is not the order of the assessment -
        /// see <c>documentation/PartO-ARCHITECTURE.md</c> section 2.
        /// </para>
        /// </summary>
        BaseNaturalVentilation,

        /// <summary>
        /// <b>The iteration-neutral identity of an assessed common space</b> - a communal corridor in a
        /// model whose dwellings do not share one ventilation strategy.
        /// <para>
        /// <b>Why a common space cannot take a dwelling iteration.</b> Until mixed dwelling strategies, a
        /// corridor scenario inherited the iteration of the one call that stated it, and with it that
        /// iteration's operating assumptions inside the permanent key - so a corridor in an MVHR block
        /// asserted "mechanical ventilation at design rate". Harmless while every call had one iteration;
        /// false in a model whose Flat 1 is naturally ventilated and whose Flat 2 is MVHR, because the
        /// corridor is neither. SAM itself says common spaces are covered by neither base iteration
        /// (<c>Query.PartOVentilationMode</c>).
        /// </para>
        /// <para>
        /// <b>It asserts nothing about any dwelling.</b> Its operating assumptions are the empty set
        /// (<c>Query.PartOOperatingAssumptions</c>), it states no ventilation route, it prepares nothing
        /// (<c>PartOIterationVentilationMode</c> and <c>PartOIterationOperatingMode</c> refuse it), and it is
        /// valid only at <c>PartOAssessmentScope.CommonSpace</c> - <c>Create.OverheatingScenarios</c> refuses
        /// it for a dwelling. It is a <b>new key</b>, never a reinterpretation of an existing one: the
        /// corridor scenarios of homogeneous runs keep their old keys exactly.
        /// </para>
        /// <para>
        /// <b>Not <see cref="Undefined"/>.</b> A scenario whose persisted iteration is unreadable loads as
        /// <see cref="Undefined"/>, so "neutral by design" and "unreadable" must not share a value. See
        /// <c>documentation/PartO-MixedDwellingStrategies-PR1.md</c>.
        /// </para>
        /// </summary>
        DwellingIndependent
    }
}
