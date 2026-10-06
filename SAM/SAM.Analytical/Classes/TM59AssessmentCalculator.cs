// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical
{
    /// <summary>
    /// The TM59 assessment recipe: restore the design internal conditions onto a simulated model, choose the
    /// spaces to assess from a set of spaces and zones, calculate, and split the results by criterion.
    /// <para>
    /// <b>Lifted from the <c>Tas.TSDQueryTM59Results</c> Grasshopper component, without changing what it
    /// does.</b> That component held the only working statement of this sequence, inside a
    /// <c>SolveInstance</c> interleaved with parameter plumbing - so nothing could call it, nothing could
    /// test it, and the headless runner would have had to restate it and drift. Everything here was already
    /// working; it has been moved, not redesigned.
    /// </para>
    /// <para>
    /// <b>Engine-free, and that is the point of putting it here.</b> Reading a TSD needs TAS; restoring
    /// internal conditions, selecting spaces, calculating TM59 and splitting the results do not. Only the
    /// read stays in <c>SAM.Analytical.Tas</c>, so the recipe is testable without a licensed TAS install -
    /// the same split that <c>TMOverheatingCalculator</c> already makes, for the same reason.
    /// </para>
    ///
    /// <para>
    /// <b>Nothing here matches a space by name any more, and there is no fallback that does.</b> Both
    /// <see cref="RestoreDesignInternalConditions"/> and <see cref="Spaces"/> resolve through
    /// <see cref="SimulationSpaceMap"/> and refuse what does not resolve. The name-matching code they used to
    /// contain is <b>deleted, not gated</b>: an optional identity mode would have left the wrong behaviour one
    /// forgotten argument away, in a workflow where every flat in a block has a "Bedroom 2".
    /// </para>
    /// <para>
    /// <b>The internal condition is why this had to be the calculation's input and not a label on its output.</b>
    /// Restoring the wrong flat's internal condition drives the assessment with the wrong occupancy profile and
    /// the wrong system, and then attributes the answer correctly - a wrong number that passes every check
    /// downstream of it. Associating results by identity afterwards would not have caught that.
    /// </para>
    /// <para>
    /// A caller with no engine-stable identity is not stuck: <c>SimulationSpaceMap</c> built with a null key
    /// function matches on <b>unique</b> names and refuses duplicates. Name matching therefore still exists, in
    /// exactly one place, where it is already tested and already refuses rather than guesses.
    /// </para>
    /// <para>
    /// <b>The criterion is no longer derived</b> where a caller supplies
    /// <see cref="VentilationStrategyMap"/>: the scenario states the ventilation strategy and a space it does
    /// not cover is refused rather than defaulted. Left unsupplied, <c>TMOverheatingCalculator</c>'s old
    /// derivation still applies - it falls back to matching a zone's name against a system library and then
    /// defaults to natural ventilation, so an MVRE dwelling is assessed against the wrong criterion.
    /// </para>
    /// </summary>
    public class TM59AssessmentCalculator
    {
        private AnalyticalModel analyticalModel = null;

        private readonly AnalyticalModel analyticalModel_Design = null;

        private readonly SimulationSpaceMap simulationSpaceMap = null;

        private List<string> associationRefusals = [];

        /// <param name="analyticalModel">
        /// The model read back from a simulation - fresh spaces carrying hourly series, not the design model.
        /// </param>
        /// <param name="analyticalModel_Design">
        /// The design model, which holds the internal conditions and the zone-to-space relations a scenario is
        /// stated against. Held rather than passed per call, because selecting spaces needs it as much as
        /// restoring internal conditions does.
        /// </param>
        /// <param name="simulationSpaceMap">
        /// How a simulated space is known to be a given design space. <b>Required, and there is no name-matching
        /// alternative</b> - see the class summary. A caller with no engine-stable identity passes a map built
        /// with a null key function, which matches on unique names and refuses on duplicates.
        /// </param>
        public TM59AssessmentCalculator(AnalyticalModel analyticalModel, AnalyticalModel analyticalModel_Design, SimulationSpaceMap simulationSpaceMap)
        {
            this.analyticalModel = analyticalModel;
            this.analyticalModel_Design = analyticalModel_Design;
            this.simulationSpaceMap = simulationSpaceMap;
        }

        /// <summary>The simulated model the assessment reads, as it currently stands.</summary>
        public AnalyticalModel AnalyticalModel => analyticalModel;

        /// <summary>The design model the assessment takes its intent from.</summary>
        public AnalyticalModel AnalyticalModel_Design => analyticalModel_Design;

        /// <summary>How a simulated space is known to be a given design space.</summary>
        public SimulationSpaceMap SimulationSpaceMap => simulationSpaceMap;

        /// <summary>
        /// Why a space could not be tied to the design model, one sentence each - an unresolved identity, or an
        /// identity two objects claim. A copy, and replaced by each <see cref="RestoreDesignInternalConditions"/>
        /// or <see cref="Spaces"/> call.
        /// <para>
        /// <b>These are refusals, not warnings.</b> A space named here was left out rather than paired with a
        /// same-named room from another dwelling, which is what the code this replaced would have done.
        /// </para>
        /// </summary>
        public List<string> AssociationRefusals => [.. associationRefusals];

        /// <summary>The TM52 building category the comfort limits are derived for.</summary>
        public TM52BuildingCategory TM52BuildingCategory { get; set; } = TM52BuildingCategory.CategoryII;

        /// <summary>
        /// The key each space's hourly operative-temperature series is stored under. Passed straight to
        /// <see cref="TMOverheatingCalculator"/>; see its documentation for why it is supplied rather than
        /// assumed.
        /// </summary>
        public string ResultantTemperatureSeriesKey { get; set; } = Core.Query.Name(SpaceSimulationResultParameter.ResultantTemperature);

        /// <summary>
        /// The key for the hourly occupancy-gain series. <b>This is the one the two vocabularies disagree
        /// about</b> - the TAS conversion writes "Occupant Sensible Gain" - so a TAS caller must supply its
        /// own, exactly as it does for <c>OverheatingCalculator</c>.
        /// </summary>
        public string OccupancySensibleGainSeriesKey { get; set; } = Core.Query.Name(SpaceSimulationResultParameter.OccupancySensibleGain);

        /// <summary>
        /// Passed straight to <c>TMOverheatingCalculator.HourCount_Expected</c> - how many hourly values a
        /// space's series must carry to be assessed at all, or <b>0</b> to enforce nothing. See that
        /// property for why the length a series must be is the caller's statement rather than this
        /// calculation's assumption.
        /// </summary>
        public int HourCount_Expected { get; set; } = 0;

        /// <summary>
        /// What each result reports as its <c>Source</c> where the model is unnamed. Passed straight to
        /// <see cref="TMOverheatingCalculator.SourceFallback"/>.
        /// <para>
        /// <b>Exposed because provenance is the caller's, not this assembly's.</b> Left unset, a result off a
        /// nameless model would be stamped <c>SAM.Analytical</c>; a TAS caller has always stamped
        /// <c>SAM.Analytical.Tas</c>, and taking that away when the recipe moved would have changed what a
        /// published result says about where it came from. <b>Provenance only</b> - it names no object and
        /// takes no part in any scenario, criterion or result identity.
        /// </para>
        /// </summary>
        public string SourceFallback { get; set; } = null;

        /// <summary>
        /// Which ventilation strategy governs which space, as stated by <c>OverheatingScenario</c>. Where this
        /// is supplied it is <b>authoritative</b> for the TM59 criterion, and a space it refuses is left out of
        /// the assessment with its reason in <c>TM59AssessmentResult.VentilationStrategyRefusals</c>.
        /// <para>
        /// Passed straight to <see cref="TMOverheatingCalculator.VentilationStrategyMap"/>; see there for what
        /// it replaces and why a refusal must not fall back. Left null, the old derivation applies and this
        /// service behaves exactly as the component it was lifted from did.
        /// </para>
        /// </summary>
        public VentilationStrategyMap VentilationStrategyMap { get; set; } = null;

        /// <summary>
        /// Copies each design space's <c>InternalCondition</c> onto the simulated space resolved to it.
        /// <para>
        /// <b>Why the assessment needs it.</b> A model read back from a simulation carries results but not
        /// the design intent, and TM59 chooses its criterion and its occupancy profile from the internal
        /// condition. Without this every simulated space would be assessed as though nothing had been said
        /// about how it is used.
        /// </para>
        /// <para>
        /// <b>By IDENTITY, through <see cref="SimulationSpaceMap"/>, and there is no name-matching path left.</b>
        /// This is the correctness that matters most in the whole association: the internal condition is an
        /// <i>input</i> to the calculation, not a label on its output. Getting it from the wrong flat's
        /// "Bedroom 2" would drive the assessment with the wrong occupancy profile and the wrong system, and
        /// then attribute the result correctly - producing a wrong number that survives every downstream check.
        /// A space whose identity does not resolve is <b>left alone and reported</b>, never paired with a
        /// same-named room.
        /// </para>
        /// </summary>
        /// <returns>Whether anything was restored.</returns>
        public bool RestoreDesignInternalConditions()
        {
            associationRefusals = [];

            AdjacencyCluster adjacencyCluster = analyticalModel?.AdjacencyCluster;
            if (adjacencyCluster == null || analyticalModel_Design == null || simulationSpaceMap == null)
            {
                return false;
            }

            List<Space> spaces = adjacencyCluster.GetSpaces();
            if (spaces == null)
            {
                return false;
            }

            bool result = false;

            foreach (Space space in spaces)
            {
                Space space_Design = simulationSpaceMap.Design(space);
                if (space_Design == null)
                {
                    associationRefusals.Add(string.Format("Simulated space '{0}' does not resolve to exactly one design space, so its design internal condition cannot be restored. It was left as the simulation produced it rather than paired with a space of the same name.", space.Name));

                    continue;
                }

                //A COPY, never the caller's instance. The cluster getter hands out a shallow copy that
                //shares Space objects with the supplied model, so assigning the condition in place would
                //reach back through that copy and change the caller's raw simulation model - which the
                //"restore happens on the assessment's model" contract forbids. The copy keeps its guid,
                //so AddObject replaces rather than duplicating.
                Space space_Restored = new(space)
                {
                    InternalCondition = space_Design.InternalCondition,
                };

                adjacencyCluster.AddObject(space_Restored);

                result = true;
            }

            analyticalModel = new AnalyticalModel(analyticalModel, adjacencyCluster);

            return result;
        }

        /// <summary>
        /// The simulated spaces an assessment of the given spaces and zones covers.
        /// <para>
        /// <b>No spaces and no zones means the whole model</b> - which is what the component does, and is why
        /// the real TAS run put a communal corridor into a domestic overheating export as an ordinary room.
        /// Scoping that properly is the Part O assessment-scope work, not this.
        /// </para>
        /// <para>
        /// <b>The arguments are DESIGN objects and they are resolved by identity.</b> A design space becomes the
        /// simulated space it produced, through <see cref="SimulationSpaceMap"/>. A design <i>zone</i> is looked
        /// up in the design model by guid and contributes the simulated counterparts of the spaces related to it
        /// there - the design model is what says which rooms make up Flat 2, and the simulated model's own zones
        /// are a rebuild whose guids mean nothing to a scenario.
        /// </para>
        /// <para>
        /// De-duplication is by <c>Guid</c>, not by name, so asking for a zone and one of its rooms does not
        /// return the room twice - and three rooms all called "Bedroom 2" are three entries, which is the
        /// correct answer and the one name matching could not give.
        /// </para>
        /// <para>
        /// Anything that does not resolve is <b>reported in <see cref="AssociationRefusals"/> and left out</b>.
        /// </para>
        /// </summary>
        /// <param name="spaces_Design">
        /// Design spaces. Null means every resolved simulated space only when <paramref name="zones_Design"/>
        /// is null too; with zones supplied, null means no individually selected spaces.
        /// </param>
        /// <param name="zones_Design">Design zones, resolved through the design model's relations.</param>
        public List<Space> Spaces(IEnumerable<Space> spaces_Design, IEnumerable<Zone> zones_Design)
        {
            associationRefusals = [];

            if (analyticalModel == null || simulationSpaceMap == null)
            {
                return null;
            }

            List<Space> result = [];

            //The de-duplication set, beside the list rather than inside it. The rule is unchanged and is
            //still by Guid; what changed is that asking "is this room already gathered" was a linear scan of
            //everything gathered so far, so a five thousand room scope compared twelve and a half million
            //pairs. Request scoped: it lives for this one call and states nothing the list does not.
            HashSet<System.Guid> guids = [];

            if (spaces_Design == null && zones_Design == null)
            {
                //The whole model still means every RESOLVED simulated space. Returning an unresolved space
                //here would undo RestoreDesignInternalConditions' refusal: the component would calculate it
                //with whatever intent the simulation happened to carry and publish a result anyway.
                foreach (Space space in analyticalModel.GetSpaces() ?? [])
                {
                    if (simulationSpaceMap.Design(space) == null)
                    {
                        associationRefusals.Add(string.Format("Simulated space '{0}' does not resolve to exactly one design space, so it cannot be assessed. It was left out rather than matched to a design space of the same name.", space.Name));

                        continue;
                    }

                    Add(result, guids, space);
                }
            }
            else if (spaces_Design != null)
            {
                foreach (Space space_Design in spaces_Design)
                {
                    if (space_Design == null)
                    {
                        continue;
                    }

                    Space space = simulationSpaceMap.Simulation(space_Design);
                    if (space == null)
                    {
                        associationRefusals.Add(string.Format("Design space '{0}' does not resolve to exactly one simulated space, so it cannot be assessed. It was left out rather than matched to a simulated space of the same name.", space_Design.Name));

                        continue;
                    }

                    Add(result, guids, space);
                }
            }

            //ONE index of the design model's zone identities, and ONE cluster to read the relations from,
            //for the whole zone traversal below.
            //
            //Both were taken per zone. GetZones() rebuilds the design model's whole zone list on every call
            //and the Find through it is linear, so resolving a block's dwellings was quadratic in its zones;
            //and AnalyticalModel.AdjacencyCluster hands out a NEW shallow copy of the entire cluster on
            //every read, so a five hundred dwelling scope copied the cluster five hundred times. Neither is
            //an engineering statement: both are the same objects, reached once instead of once per zone.
            //
            //Request scoped, and it cannot go stale within this traversal - nothing in the loop writes to
            //the design model. First occurrence wins, because List.Find returned the first match.
            Dictionary<System.Guid, Zone> dictionary_Zone = [];
            AdjacencyCluster adjacencyCluster_Design = null;

            if (zones_Design != null && analyticalModel_Design != null)
            {
                foreach (Zone zone in analyticalModel_Design.GetZones() ?? [])
                {
                    if (zone != null && !dictionary_Zone.ContainsKey(zone.Guid))
                    {
                        dictionary_Zone[zone.Guid] = zone;
                    }
                }

                adjacencyCluster_Design = analyticalModel_Design.AdjacencyCluster;
            }

            foreach (Zone zone_Design in zones_Design ?? [])
            {
                if (zone_Design == null)
                {
                    continue;
                }

                //The DESIGN model's relations, by guid. A zone is a statement about the design, and the
                //simulated model's zones are fresh objects that no scenario can name.
                if (!dictionary_Zone.TryGetValue(zone_Design.Guid, out Zone zone) || zone == null)
                {
                    associationRefusals.Add(string.Format("Design zone '{0}' is not in the design model, so the spaces it covers cannot be identified.", zone_Design.Name));

                    continue;
                }

                List<Space> spaces_Zone = adjacencyCluster_Design.GetRelatedObjects<Space>(zone);
                if (spaces_Zone == null || spaces_Zone.Count == 0)
                {
                    associationRefusals.Add(string.Format("Design zone '{0}' holds no spaces, so there is nothing in it to assess.", zone_Design.Name));

                    continue;
                }

                foreach (Space space_Zone in spaces_Zone)
                {
                    Space space = simulationSpaceMap.Simulation(space_Zone);
                    if (space == null)
                    {
                        associationRefusals.Add(string.Format("Design space '{0}' in zone '{1}' does not resolve to exactly one simulated space, so it cannot be assessed.", space_Zone.Name, zone_Design.Name));

                        continue;
                    }

                    Add(result, guids, space);
                }
            }

            return result;
        }

        /// <summary>
        /// Appends a space unless it is already there - <b>by <c>Guid</c></b>. Names cannot do this job: three
        /// flats' "Bedroom 2" are three different rooms, and de-duplicating them by name would silently assess
        /// one of them and drop the other two.
        /// </summary>
        private static void Add(List<Space> spaces, HashSet<System.Guid> guids, Space space)
        {
            //HashSet.Add IS the test: it answers "was this guid already gathered" and records it in one
            //step, exactly as the linear Find it replaces answered it, first occurrence winning. A null
            //space cannot reach here - both callers resolved one through SimulationSpaceMap first - and
            //would have thrown on the Find just the same.
            if (guids.Add(space.Guid))
            {
                spaces.Add(space);
            }
        }

        /// <summary>
        /// Calculates TM59 for the given spaces and splits the results by the criterion that applied.
        /// </summary>
        /// <param name="spaces">
        /// The simulated spaces to assess - normally what <see cref="Spaces"/> returned.
        /// </param>
        /// <param name="extended">
        /// Whether to return the extended results. False simplifies each one, which is the component's
        /// default and drops the per-hour detail.
        /// </param>
        public TM59AssessmentResult Calculate(IEnumerable<Space> spaces, bool extended = false)
        {
            if (analyticalModel == null || spaces == null)
            {
                return null;
            }

            List<Space> spaces_Temp = [.. spaces];

            TMOverheatingCalculator tMOverheatingCalculator = new(analyticalModel)
            {
                TM52BuildingCategory = TM52BuildingCategory,
                ResultantTemperatureSeriesKey = ResultantTemperatureSeriesKey,
                OccupancySensibleGainSeriesKey = OccupancySensibleGainSeriesKey,
                VentilationStrategyMap = VentilationStrategyMap,
                HourCount_Expected = HourCount_Expected
            };

            //Only when the caller stated one. Left null, TMOverheatingCalculator keeps its own default rather
            //than being handed an empty provenance.
            if (!string.IsNullOrWhiteSpace(SourceFallback))
            {
                tMOverheatingCalculator.SourceFallback = SourceFallback;
            }

            List<TM59ExtendedResult> tM59ExtendedResults = tMOverheatingCalculator.Calculate_TM59(spaces_Temp);
            if (tM59ExtendedResults == null)
            {
                return null;
            }

            List<TMResult> tMResults_MechanicalVentilation = Split<TM59MechanicalVentilationExtendedResult>(tM59ExtendedResults, extended);
            List<TMResult> tMResults_NaturalVentilation = Split<TM59NaturalVentilationExtendedResult>(tM59ExtendedResults, extended);
            List<TMResult> tMResults_Corridor = Split<TM59CorridorExtendedResult>(tM59ExtendedResults, extended);

            //Whole year, as the component asks for - the comfort limits are a running mean over the year and
            //are reported alongside the summer assessment rather than clipped to it.
            IndexedDoubles indexedDoubles_Max = tMOverheatingCalculator.GetMaxIndoorComfortTemperatures(0, 364);
            IndexedDoubles indexedDoubles_Min = tMOverheatingCalculator.GetMinIndoorComfortTemperatures(0, 364);

            //Both refusal records, so a caller totalling the criterion lists can account for every space it
            //asked about: one refused for an unstated ventilation strategy, the other for hourly series that
            //cannot be assessed together.
            return new TM59AssessmentResult(spaces_Temp, tMResults_MechanicalVentilation, tMResults_NaturalVentilation, tMResults_Corridor, indexedDoubles_Max, indexedDoubles_Min, tMOverheatingCalculator.VentilationStrategyRefusals, tMOverheatingCalculator.HourlySeriesRefusals, tMOverheatingCalculator.SpaceGuids_HourlySeriesRefused);
        }

        /// <summary>
        /// The results of one criterion, simplified unless the extended form was asked for.
        /// <para>
        /// One method for all three, where the component repeated the pair of lines three times and - by
        /// accident - null-guarded two of them and not the third. <c>FindAll</c> never returns null, so the
        /// guard never did anything and the behaviour is unchanged either way.
        /// </para>
        /// </summary>
        private static List<TMResult> Split<T>(List<TM59ExtendedResult> tM59ExtendedResults, bool extended) where T : TM59ExtendedResult
        {
            List<TMResult> result = tM59ExtendedResults.FindAll(x => x is T).ConvertAll(x => (TMResult)x);

            return extended ? result : result.ConvertAll(x => (x as TM59ExtendedResult)?.Simplify());
        }
    }
}
