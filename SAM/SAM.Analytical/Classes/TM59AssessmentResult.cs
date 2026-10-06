// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical
{
    /// <summary>
    /// What a TM59 assessment of a simulated model produces: the spaces assessed, their results split by
    /// the criterion that applied, and the comfort temperature limits the criteria were measured against.
    /// <para>
    /// <b>The three lists are the three TM59 criteria, not three arbitrary groupings.</b> A mechanically
    /// ventilated space, a naturally ventilated space and a corridor are judged against different rules, so
    /// they cannot be reported in one list without losing the thing that makes the number mean something.
    /// </para>
    /// <para>
    /// A plain carrier - it holds what the calculation produced and decides nothing. Deliberately not a
    /// <c>Result</c>: these are not attached to a model, and attributing them to design objects is separate
    /// work (<c>SimulationSpaceMap</c>, and by identity rather than by name).
    /// </para>
    /// </summary>
    public class TM59AssessmentResult
    {
        private readonly List<string> ventilationStrategyRefusals;

        private readonly List<string> hourlySeriesRefusals;

        private readonly List<System.Guid> spaceGuids_HourlySeriesRefused;

        internal TM59AssessmentResult(List<Space> spaces, List<TMResult> tMResults_MechanicalVentilation, List<TMResult> tMResults_NaturalVentilation, List<TMResult> tMResults_Corridor, IndexedDoubles indexedDoubles_MaxIndoorComfortTemperatures, IndexedDoubles indexedDoubles_MinIndoorComfortTemperatures, List<string> ventilationStrategyRefusals = null, List<string> hourlySeriesRefusals = null, List<System.Guid> spaceGuids_HourlySeriesRefused = null)
        {
            Spaces = spaces;
            MechanicalVentilationResults = tMResults_MechanicalVentilation;
            NaturalVentilationResults = tMResults_NaturalVentilation;
            CorridorResults = tMResults_Corridor;
            MaxIndoorComfortTemperatures = indexedDoubles_MaxIndoorComfortTemperatures;
            MinIndoorComfortTemperatures = indexedDoubles_MinIndoorComfortTemperatures;
            //Copied in, and copied out again by the property. A reporting layer that normalises or de-duplicates
            //in place would otherwise erase the record of which dwellings went unassessed - while the three
            //criterion lists still showed a short count, which is precisely what this list exists to explain.
            this.ventilationStrategyRefusals = ventilationStrategyRefusals == null ? [] : [.. ventilationStrategyRefusals];
            this.hourlySeriesRefusals = hourlySeriesRefusals == null ? [] : [.. hourlySeriesRefusals];
            this.spaceGuids_HourlySeriesRefused = spaceGuids_HourlySeriesRefused == null ? [] : [.. spaceGuids_HourlySeriesRefused];
        }

        /// <summary>
        /// The spaces the assessment covered - the <b>simulation</b> spaces, not the design ones. Attributing
        /// them back to the design model is separate work and must go by identity, never by name.
        /// </summary>
        public List<Space> Spaces { get; }

        /// <summary>Results judged against the mechanically ventilated criterion.</summary>
        public List<TMResult> MechanicalVentilationResults { get; }

        /// <summary>Results judged against the naturally ventilated criterion.</summary>
        public List<TMResult> NaturalVentilationResults { get; }

        /// <summary>
        /// Results judged against the corridor criterion. A communal corridor is assessed in its own right
        /// and belongs to no dwelling.
        /// </summary>
        public List<TMResult> CorridorResults { get; }

        /// <summary>The upper comfort limit series the criteria were measured against.</summary>
        public IndexedDoubles MaxIndoorComfortTemperatures { get; }

        /// <summary>The lower comfort limit series.</summary>
        public IndexedDoubles MinIndoorComfortTemperatures { get; }

        /// <summary>
        /// Spaces left out because the <c>OverheatingScenario</c> did not settle how they are ventilated, one
        /// sentence each. Empty where no scenario was supplied or nothing was refused.
        /// <para>
        /// <b>These are the assessment's gaps and they must be read.</b> A space named here is absent from all
        /// three criterion lists, so a caller totalling the lists and comparing the total with the number of
        /// spaces it asked about will see fewer - and this says why. The alternative, and what this replaces,
        /// was assessing the space against a defaulted natural-ventilation criterion and reporting it as a
        /// result.
        /// </para>
        /// <para>
        /// <b>It does not account for the whole shortfall on its own.</b> A space whose hourly series cannot
        /// be assessed also produces no result, and that is itemised separately in
        /// <see cref="HourlySeriesRefusals"/>. Read both: together they account for every space that was
        /// asked about and is absent from all three criterion lists.
        /// </para>
        /// <para>A copy, so a reporting layer cannot edit the record of what went unassessed.</para>
        /// </summary>
        public List<string> VentilationStrategyRefusals => [.. ventilationStrategyRefusals];

        /// <summary>
        /// Spaces left out because their hourly series could not be assessed - absent, empty, or the two
        /// series of different lengths - one sentence each. See
        /// <c>TMOverheatingCalculator.HourlySeriesRefusals</c> for why each of those is a refusal rather
        /// than an assessment over whatever hours survived.
        /// <para>A copy, so a reporting layer cannot edit the record of what went unassessed.</para>
        /// </summary>
        public List<string> HourlySeriesRefusals => [.. hourlySeriesRefusals];

        /// <summary>
        /// The identity of every space named in <see cref="HourlySeriesRefusals"/>, so a caller can keep a
        /// refused room out of a verdict without parsing a sentence.
        /// </summary>
        public List<System.Guid> SpaceGuids_HourlySeriesRefused => [.. spaceGuids_HourlySeriesRefused];
    }
}
