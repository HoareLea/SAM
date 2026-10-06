// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    /// <summary>
    /// Which ventilation strategy governs which space, as stated by <see cref="OverheatingScenario"/> - the
    /// authoritative answer a TM59 assessment reads instead of deriving one.
    /// <para>
    /// <b>Why this exists.</b> Three different places in this workspace decided how a space is ventilated and
    /// they disagreed. The one that picks the TM59 criterion took the space's internal condition, then fell
    /// back to matching a <i>zone's name</i> against a system-type library, then defaulted to <c>"NV"</c>. A
    /// zone called "Flat 1" matches nothing in that library, so a real MVRE dwelling was assessed against the
    /// natural-ventilation criterion - a wrong answer that looks like a right one. The other two fed the TM59
    /// XML from the internal condition and from any related mechanical <c>VentilationSystem</c>, which is how
    /// one building came back with "Nat Vent" and "Mech Vent" mixed across three identical flats.
    /// </para>
    /// <para>
    /// <b>The scenario wins over all of them.</b> A scenario is a statement of engineering intent - this
    /// dwelling, this mitigation stage, this system - and the model's internal conditions, its mechanical
    /// systems and its zone names are inputs to a simulation, not statements about which Approved Document O
    /// criterion applies. Where this map is supplied, none of the three derivations is consulted.
    /// </para>
    /// <para>
    /// <b>And it refuses rather than falls back.</b> Where no scenario covers a space, or the scenario that
    /// does states no strategy, or two scenarios state different ones, the space is refused with a reason. It
    /// then produces no assessment - a visible gap - instead of an assessment against a guessed criterion.
    /// Refusing is the whole point: the defect being fixed is a silent default, not the absence of one.
    /// </para>
    /// <para>
    /// <b>Nothing here is inferred from provenance.</b> The strategy comes from the scenario and from nowhere
    /// else - not from a model's <c>Source</c>, not from whether the results came through a TSD or a TPD, not
    /// from which engine wrote them. Provenance says where a number came from; it says nothing about how a
    /// building is ventilated.
    /// </para>
    /// <para>
    /// <b>Keyed on <see cref="Space.Guid"/>, never on a name</b> - every flat in a block has a "Bedroom 2".
    /// The caller supplies the spaces each scenario governs and must supply the <b>same space objects the
    /// assessment will run over</b>, because those carry the guids the assessment looks up. Resolving a
    /// simulation space back to its design space is <c>SimulationSpaceMap</c>'s job and a separate step; this
    /// class deliberately does no matching of its own so that it cannot quietly reintroduce matching by name.
    /// </para>
    /// </summary>
    public class VentilationStrategyMap
    {
        /// <summary>
        /// The ventilation identities a TM59 criterion is known for. <b>A closed set, and that is the point.</b>
        /// <para>
        /// The criterion selection reads <c>UV</c> as the corridor criterion, <c>NV</c> as natural and
        /// <b>everything else</b> as mechanical. That last step is an open default, and an open default in the
        /// authoritative path is the same defect as the <c>"NV"</c> one this class removes, only pointing the
        /// other way: a scenario stating <c>"Natural"</c>, or <c>"N-V"</c>, or any typo, would be assessed
        /// mechanically and exported as "Mech Vent" with no refusal and no diagnostic. So a strategy outside
        /// this set is <b>refused</b> rather than assumed mechanical.
        /// </para>
        /// <para>
        /// These are the nine ventilation identities the shipped <c>SAM_SystemTypeLibrary</c> defines, which are
        /// also the nine <c>SAM_Systems</c> keys its capability index on, plus <c>MVHR</c>. It is a list of
        /// <b>names</b>, which is vocabulary and belongs here - <c>Query.IsMechanicalVentilation</c> already
        /// names <c>UV</c> and <c>NV</c> in this assembly. It is deliberately <b>not</b> read from
        /// <c>Query.DefaultSystemTypeLibrary()</c>: that comes from <c>ActiveSetting</c> and can be absent, and
        /// making the authoritative path depend on the same installed resource the defective derivation used
        /// would trade one silent failure for another.
        /// </para>
        /// <para>
        /// <b><c>MVHR</c> is here because <c>Query.PartOVentilationMode</c> accepts it.</b> That mapping takes
        /// <c>MVHR</c> and <c>MVRE</c> as two spellings of the one Approved Document O route, so an assessment
        /// could state the word the regulation itself uses, prepare successfully, simulate, and then produce
        /// <b>no assessment at all</b> - every space refused here, for a strategy that had already been
        /// accepted upstream. The word is added rather than rewritten to <c>MVRE</c> on the way in: a scenario
        /// has to keep saying what the assessment said, and quietly substituting one identity for another is
        /// the class of change this map exists to stop. The criterion selection needs nothing further -
        /// <c>MVHR</c> is neither <c>NV</c> nor <c>UV</c>, so it already selects the mechanical criterion.
        /// </para>
        /// <para>
        /// <b>A project with a custom system-type library needs this extended</b>, and the accepted set is
        /// declared policy rather than anything Part F requires - the same footing as <c>SAM_Systems</c>' rank.
        /// </para>
        /// </summary>
        private static readonly HashSet<string> ventilationStrategies_Recognised = new(StringComparer.Ordinal)
        {
            "NV", "MV", "MVRE", "MVHR", "UV", "EOL", "EOC", "CAV", "VAV", "DISP"
        };

        /// <summary>What one scenario said about one space.</summary>
        private class Claim
        {
            /// <summary>
            /// The design zone the claiming scenario names, for the refusal message. <b>The space's own name is
            /// deliberately not stored</b> - a refusal quotes the space the caller asked about, so that the
            /// message can never name a different object from the one that was queried.
            /// </summary>
            public Guid ZoneGuid;

            /// <summary>
            /// The normalised strategy, or null where the scenario stated none <b>or stated one this assembly
            /// has no TM59 criterion for</b>. The two are told apart by <see cref="IsUnrecognised"/>, because
            /// "you said nothing" and "you said something I cannot act on" need different fixes.
            /// </summary>
            public string VentilationStrategy;

            /// <summary>What the scenario stated where it was not recognised, for the refusal message.</summary>
            public string VentilationStrategy_Unrecognised;

            /// <summary>Whether the scenario stated a strategy outside the recognised vocabulary.</summary>
            public bool IsUnrecognised => VentilationStrategy_Unrecognised != null;

            /// <summary>Whether a second scenario claimed this space and said something different.</summary>
            public bool IsConflicted;
        }

        private readonly Dictionary<Guid, Claim> dictionary_Claim = [];

        /// <summary>
        /// Records that a scenario governs the given spaces.
        /// <para>
        /// <b>A scenario stating no strategy is still recorded</b>, as a claim that states nothing. It has to
        /// be: "you did not say how this dwelling is ventilated" and "no scenario mentions this space at all"
        /// are different mistakes with different fixes, and collapsing them into one refusal would send
        /// somebody looking in the wrong place. Both refuse.
        /// </para>
        /// <para>
        /// <b>A strategy outside the recognised vocabulary is recorded as unrecognised</b>, not as mechanical
        /// and not as silence. See <see cref="ventilationStrategies_Recognised"/> for why that matters.
        /// </para>
        /// <para>
        /// <b>Two scenarios over one space refuse unless they agree.</b> Identical strategies are not a
        /// conflict - the same thing said twice is still one answer. Anything else means the scenarios have
        /// not said how the space is ventilated, and the same rule applies when one of them states nothing.
        /// </para>
        /// <para>
        /// <b>The map is live and is held by reference</b>, here and by the calculators it is given to. A
        /// caller may keep adding after handing it over and the next assessment will see the additions. That is
        /// deliberate - a map is built up scenario by scenario, unlike <c>OverheatingScenario</c>, which copies
        /// everything in because it is an identity - but it does mean the map must not be shared between two
        /// callers that disagree about what is in it.
        /// </para>
        /// </summary>
        /// <param name="overheatingScenario">The scenario. Ignored where null or not <c>IsValid</c>.</param>
        /// <param name="spaces">
        /// The spaces it governs - the objects the assessment will run over, for their guids.
        /// </param>
        /// <returns>
        /// Whether the scenario applied to at least one space. True does not promise the claim changed
        /// anything: re-stating the same strategy for the same space is applicable and idempotent.
        /// </returns>
        public bool Add(OverheatingScenario overheatingScenario, IEnumerable<Space> spaces)
        {
            if (overheatingScenario == null || !overheatingScenario.IsValid || spaces == null)
            {
                //A scenario that names nothing assessable states nothing about ventilation either. Recording
                //it would put a claim in the map that no caller could act on.
                return false;
            }

            //Upper-cased, not trimmed: OverheatingScenario.Normalized already rebuilt the SystemTemplate
            //through its setters, which strip EVERY space, so " uv " and "MV RE" cannot arrive here. Case is
            //the one thing those setters leave alone, and the criterion selection compares upper-case.
            //ToUpperInvariant, not ToUpper: the current culture's casing rules (Turkish is the classic case)
            //would otherwise map "disp" to "DİSP" and a valid strategy would read as unrecognised.
            string ventilationStrategy = overheatingScenario.HasVentilationStrategy
                ? overheatingScenario.VentilationStrategy.ToUpperInvariant()
                : null;

            //A strategy this assembly has no TM59 criterion for is not silence and not mechanical. Recorded as
            //itself so the refusal can quote it back.
            string ventilationStrategy_Unrecognised = null;

            if (ventilationStrategy != null && !ventilationStrategies_Recognised.Contains(ventilationStrategy))
            {
                ventilationStrategy_Unrecognised = ventilationStrategy;
                ventilationStrategy = null;
            }

            bool result = false;

            foreach (Space space in spaces)
            {
                if (space == null)
                {
                    continue;
                }

                result = true;

                if (!dictionary_Claim.TryGetValue(space.Guid, out Claim claim))
                {
                    dictionary_Claim[space.Guid] = new Claim
                    {
                        ZoneGuid = overheatingScenario.ZoneGuid,
                        VentilationStrategy = ventilationStrategy,
                        VentilationStrategy_Unrecognised = ventilationStrategy_Unrecognised
                    };

                    continue;
                }

                if (claim.IsConflicted)
                {
                    //Already refused for disagreement. Nothing said now can settle it.
                    continue;
                }

                if (string.Equals(claim.VentilationStrategy, ventilationStrategy, StringComparison.Ordinal)
                    && string.Equals(claim.VentilationStrategy_Unrecognised, ventilationStrategy_Unrecognised, StringComparison.Ordinal))
                {
                    //The same answer again - including the same unrecognised word twice, which is still one
                    //answer and still refused, just not for ambiguity.
                    continue;
                }

                claim.IsConflicted = true;
            }

            return result;
        }

        /// <summary>
        /// The strategy governing a space, or a refusal naming what was missing.
        /// </summary>
        public VentilationStrategySelection Selection(Space space)
        {
            if (space == null)
            {
                return VentilationStrategySelection.Refused("No space was given, so no ventilation strategy can apply to one.");
            }

            if (!dictionary_Claim.TryGetValue(space.Guid, out Claim claim))
            {
                return VentilationStrategySelection.Refused(string.Format("No overheating scenario covers space '{0}', so nothing states how it is ventilated and it cannot be assessed against a TM59 criterion.", space.Name));
            }

            if (claim.IsConflicted)
            {
                return VentilationStrategySelection.Refused(string.Format("More than one overheating scenario covers space '{0}' and they state different ventilation strategies, so it is not settled how it is ventilated.", space.Name));
            }

            if (claim.IsUnrecognised)
            {
                return VentilationStrategySelection.Refused(string.Format("The overheating scenario for design zone {0} states ventilation strategy '{1}', which is not a ventilation identity this assessment has a TM59 criterion for, so space '{2}' cannot be assessed. Expected one of: {3}.", claim.ZoneGuid, claim.VentilationStrategy_Unrecognised, space.Name, string.Join(", ", ventilationStrategies_Recognised)));
            }

            if (claim.VentilationStrategy == null)
            {
                return VentilationStrategySelection.Refused(string.Format("The overheating scenario for design zone {0} states no ventilation strategy, so space '{1}' cannot be assessed against a TM59 criterion.", claim.ZoneGuid, space.Name));
            }

            return VentilationStrategySelection.Selected(claim.VentilationStrategy);
        }

        /// <summary>How many spaces a scenario has claimed, whether or not it stated a strategy for them.</summary>
        public int Count => dictionary_Claim.Count;
    }
}
