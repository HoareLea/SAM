// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        /// <summary>
        /// Builds ONE mixed analytical model from a clean pre-Part-O baseline and the dwelling strategies
        /// persisted on it - each dwelling at its own selected Approved Document O strategy, every assessed
        /// communal corridor included automatically - in a single deterministic call.
        /// <code>
        /// clean baseline + persisted PartODwellingStrategySet (+ catalogue)
        ///     -> one materialisation -> mixed model + scenarios + materialisation record
        /// </code>
        ///
        /// <para><b>What it is, and what it replaces</b></para>
        /// <para>
        /// The PR1 authority of <c>documentation/PartO-MixedDwellingStrategies-PR0.md</c> §D-§F. Calling
        /// <see cref="PreparePartOIteration"/> once per dwelling cannot build a mixed model: its Part F application
        /// and terminal realisation are whole-model, so preparing any MVHR dwelling writes System 4 rates onto
        /// every sized space (P1, P11), and nothing ever takes a dwelling back from MVHR to natural ventilation
        /// (P4). This call scopes both to the MVHR dwellings and builds each of those with exactly Iteration 1a's
        /// per-dwelling design (<c>RealizeBaseMVHRDwelling</c>), so there is one implementation of the design and
        /// no client orchestration of 1a/1b/2.
        /// </para>
        ///
        /// <para><b>Refuses rather than repairs - and returns no model when it refuses</b></para>
        /// <list type="bullet">
        /// <item>a model that is not a clean baseline (<see cref="Query.PartOBaselineFindings"/>, owner decision D1) -
        /// nothing is cleaned or undone;</item>
        /// <item>no strategy collection (a legacy model), an unknown schema, a duplicated or unreadable strategy, a
        /// strategy for a zone that is not a dwelling, and an assessed dwelling with no strategy - absence is never
        /// read as natural ventilation;</item>
        /// <item>natural ventilation with cooling, with a retained mechanical design or with a product;</item>
        /// <item>active cooling without a product whose catalogue entry states its manufacturer's cooling guidance, or
        /// whose cooling operating airflow falls outside that guidance's published range or the unit's capacity;</item>
        /// <item>a naturally ventilated dwelling served by authored mechanical duty or plant; an authored system or
        /// unit that straddles zones (shared plant is never split or mutated); authored plant in an MVHR dwelling
        /// that is not connected to its design terminals - duty-bearing plant only, since inert template scaffolding
        /// (<see cref="Query.PartOAuthoredPlantDuty(AdjacencyCluster, VentilationSystem)"/>) is noted and left out
        /// of the assessment; a reused authored unit that states a supply
        /// temperature (P12 - an authored setpoint is conditioning nobody selected, cooled dwelling or not);</item>
        /// <item>a retained design whose terminals no longer match its fingerprint, and a Part F requirement basis
        /// over terminals that differ from the requirement;</item>
        /// <item>a product that is not in the catalogue, not permitted by the project, or cannot serve the duty; and
        /// every refusal of the Iteration 1a design itself (ticV conflict, duty, movements, transfer air, balance).</item>
        /// </list>
        ///
        /// <para><b>Active cooling (PR3)</b></para>
        /// <para>
        /// A cooled dwelling is an MVHR dwelling built exactly as any other, whose selected product's manufacturer
        /// guidance is its cooling. Nothing cooling-specific is written into the model: the cooling is materialised by
        /// SAM_Systems on the TAS Systems route from <see cref="PartOMaterialisationRecord.CooledDwellings"/> - the unit,
        /// the product, the guidance fingerprint and the cooling operating airflow
        /// (<see cref="Query.PartOCoolingOperatingAirFlow"/>). One cooled dwelling puts the whole model on that route
        /// (<see cref="PartOMaterialisationRecord.Route"/>). A cooled dwelling is assessed as
        /// <see cref="PartOIteration.ActiveTrimCooling"/>, never as <see cref="PartOIteration.BasePassive"/>. Its unit
        /// and air network must stay inside the dwelling, which is checked after materialisation.
        /// </para>
        ///
        /// <para><b>Natural ventilation writes nothing and strips nothing</b></para>
        /// <para>
        /// No Part F internal-condition rate, no generated terminal, no system, unit or movement. Design terminals
        /// the baseline already carries there are copied through unconnected and inert, and named in the notes.
        /// This is checked after materialisation and refused if it does not hold.
        /// </para>
        ///
        /// <para><b>Determinism</b></para>
        /// <para>
        /// Dwellings are processed in (zone name, zone guid) order whatever order the strategies were stated in, and
        /// each dwelling's system id and unit name are derived from its zone, not handed out in call order (P2). So
        /// the same baseline, strategies and catalogue give the same engineering state. Generated objects still take
        /// new guids, so a rebuilt model never matches an earlier run's provenance and always needs a fresh
        /// simulation - the fail-closed direction.
        /// </para>
        ///
        /// <para><b>Scope</b></para>
        /// <para>
        /// The whole building is returned; nothing is isolated. Dwellings outside
        /// <paramref name="guids_Zone_Assessed"/> are left exactly as the baseline has them and get no scenario.
        /// Assessed communal corridors - a common-space zone every space of which is assigned
        /// <see cref="TM59InternalConditionResolver.CommunalCorridorInternalConditionName"/> - get the
        /// iteration-neutral <see cref="Create.PartOCommonSpaceOverheatingScenario"/> automatically; they are not
        /// strategy rows.
        /// </para>
        /// </summary>
        /// <param name="analyticalModel_Baseline">The clean baseline carrying the strategies. <b>Not modified.</b></param>
        /// <param name="ventilationUnitCapacityDescriptors">
        /// The catalogue products may be selected from, filtered by the project's
        /// <see cref="PartOEquipmentSelection"/> and joined by its project test product exactly as Iteration 2 does.
        /// Null selects no product: every MVHR unit stays generic, and a strategy naming a product refuses.
        /// </param>
        /// <param name="guids_Zone_Assessed">
        /// The dwellings to materialise. Null means every dwelling zone of the model, each of which must then carry
        /// a strategy. A dwelling outside the scope is unassessed: untouched and unscenarioed.
        /// </param>
        /// <param name="ventilationUnitTemplates">
        /// The catalogue's product templates, read for their manufacturer operating strategy - the cooling guidance of
        /// a cooled dwelling's selected product. Null offers no guidance, so every cooled dwelling refuses.
        /// </param>
        public static PartOMaterialisation MaterialisePartODwellingStrategies(this AnalyticalModel analyticalModel_Baseline, IEnumerable<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors = null, IEnumerable<Guid> guids_Zone_Assessed = null, IEnumerable<VentilationUnitTemplate> ventilationUnitTemplates = null)
        {
            PartOMaterialisation result = new();

            void Refuse(PartOMaterialisationRefusalReason reason, string message, Zone zone = null, string subject = null)
            {
                result.Refusals.Add(new PartOMaterialisationRefusal(reason, message, zone?.Guid, subject ?? zone?.Name));
            }

            // ---- 1. The baseline ---------------------------------------------------------------------------

            result.Refusals.AddRange(Query.PartOBaselineFindings(analyticalModel_Baseline));

            AdjacencyCluster adjacencyCluster_Baseline = analyticalModel_Baseline?.AdjacencyCluster;
            if (adjacencyCluster_Baseline is null)
            {
                return result;
            }

            // ---- 2. The persisted strategies ---------------------------------------------------------------

            if (!analyticalModel_Baseline.HasValue(AnalyticalModelParameter.PartODwellingStrategies))
            {
                Refuse(PartOMaterialisationRefusalReason.NoStrategies, "The model carries no Part O dwelling strategies, so it is a legacy model with no mixed authority. Nothing is inferred from its systems, scenarios or names: select a strategy for each dwelling first.");

                return result;
            }

            PartODwellingStrategySet partODwellingStrategySet = analyticalModel_Baseline.GetValue<PartODwellingStrategySet>(AnalyticalModelParameter.PartODwellingStrategies);
            if (partODwellingStrategySet is null || !partODwellingStrategySet.IsValid)
            {
                Refuse(PartOMaterialisationRefusalReason.InvalidStrategySet, partODwellingStrategySet is null
                    ? "The model's Part O dwelling strategies could not be read, so no strategy can be trusted."
                    : partODwellingStrategySet.Conflicts.Count != 0
                        ? string.Format("The model's Part O dwelling strategies state more than one strategy for {0} dwelling(s), so which one is selected is ambiguous. Nothing was chosen between them.", partODwellingStrategySet.Conflicts.Count)
                        : string.Format("The model's Part O dwelling strategies are of schema '{0}', which this build does not read ('{1}'). They are not reinterpreted.", partODwellingStrategySet.SchemaRead ?? "-", PartODwellingStrategySet.Schema));

                return result;
            }

            // ---- 3. Which zones are dwellings, which are assessed, which are communal corridors -------------

            List<Zone> zones_All = adjacencyCluster_Baseline.GetZones() ?? [];
            zones_All.RemoveAll(x => x is null);

            Dictionary<Guid, Zone> dictionary_Zone = [];
            foreach (Zone zone in zones_All)
            {
                dictionary_Zone[zone.Guid] = zone;
            }

            //The one dwelling rule, asked rather than restated - exactly what Part F sizes.
            List<Zone> zones_Dwelling = zones_All.PartFDwellingZones() ?? [];
            zones_All.PartFClassifyDwellingZones(out List<Zone> _, out List<Zone> zones_NotDwelling, out List<Zone> _);

            HashSet<Guid> guids_Dwelling = [];
            zones_Dwelling.ForEach(x => guids_Dwelling.Add(x.Guid));

            List<Zone> zones_Assessed = [];
            if (guids_Zone_Assessed is null)
            {
                zones_Assessed.AddRange(zones_Dwelling);
            }
            else
            {
                HashSet<Guid> guids_Seen = [];
                foreach (Guid guid in guids_Zone_Assessed)
                {
                    if (!guids_Seen.Add(guid))
                    {
                        continue;
                    }

                    if (!dictionary_Zone.TryGetValue(guid, out Zone zone))
                    {
                        Refuse(PartOMaterialisationRefusalReason.UnknownZone, string.Format("The assessed scope names zone {0}, which the model does not contain.", guid));
                    }
                    else if (!guids_Dwelling.Contains(guid))
                    {
                        Refuse(PartOMaterialisationRefusalReason.NotADwelling, string.Format("The assessed scope names zone '{0}', which is not a dwelling. Common spaces are assessed automatically and take no strategy.", zone.Name), zone);
                    }
                    else
                    {
                        zones_Assessed.Add(zone);
                    }
                }
            }

            zones_Assessed.Sort(CompareZones);

            HashSet<Guid> guids_Assessed = [];
            zones_Assessed.ForEach(x => guids_Assessed.Add(x.Guid));

            foreach (PartODwellingStrategy partODwellingStrategy in partODwellingStrategySet.Strategies)
            {
                if (!dictionary_Zone.TryGetValue(partODwellingStrategy.ZoneGuid, out Zone zone))
                {
                    Refuse(PartOMaterialisationRefusalReason.UnknownZone, string.Format("A dwelling strategy names zone {0}, which the model does not contain.", partODwellingStrategy.ZoneGuid));
                }
                else if (!guids_Dwelling.Contains(zone.Guid))
                {
                    Refuse(PartOMaterialisationRefusalReason.NotADwelling, string.Format("A dwelling strategy is selected for zone '{0}', which is not a dwelling. Common spaces are assessed automatically and are not strategy rows.", zone.Name), zone);
                }
                else if (!guids_Assessed.Contains(zone.Guid))
                {
                    result.Notes.Add(string.Format("Dwelling '{0}' carries a strategy but is outside the assessed scope, so it was left exactly as the baseline has it.", zone.Name));
                }
            }

            if (zones_Assessed.Count == 0 && result.Refusals.Count == 0)
            {
                Refuse(PartOMaterialisationRefusalReason.NotADwelling, "No dwelling zone is assessed, so there is nothing to materialise. Mark the dwelling zones with 'Is Dwelling' and select a strategy for each.");
            }

            Dictionary<Guid, PartODwellingStrategy> dictionary_Strategy = [];

            foreach (Zone zone in zones_Assessed)
            {
                PartODwellingStrategy partODwellingStrategy = partODwellingStrategySet.Strategy(zone.Guid);

                if (partODwellingStrategy is null)
                {
                    Refuse(PartOMaterialisationRefusalReason.MissingStrategy, string.Format("Dwelling '{0}' has no selected strategy. A dwelling nobody has decided about is never read as naturally ventilated; select its strategy, or leave it out of the assessed scope.", zone.Name), zone);

                    continue;
                }

                if (!partODwellingStrategy.IsValid)
                {
                    Refuse(PartOMaterialisationRefusalReason.InvalidStrategy, string.Format("The strategy of dwelling '{0}' does not state every property ({1}) - an unreadable property is never defaulted.", zone.Name, partODwellingStrategy.CanonicalText()), zone);

                    continue;
                }

                bool natural = partODwellingStrategy.VentilationMode == PartOVentilationMode.NaturalVentilation;

                if (natural && partODwellingStrategy.ActiveCooling != PartOActiveCooling.None)
                {
                    Refuse(PartOMaterialisationRefusalReason.NaturalWithCooling, string.Format("Dwelling '{0}' is selected as naturally ventilated with active supply-air cooling. The only cooling path is on the MVHR supply, so the two statements contradict each other.", zone.Name), zone);
                }

                if (natural && partODwellingStrategy.DesignAirFlowBasis == PartODesignAirFlowBasis.RetainedDesign)
                {
                    Refuse(PartOMaterialisationRefusalReason.NaturalWithRetainedDesign, string.Format("Dwelling '{0}' is selected as naturally ventilated with a retained mechanical design airflow. A naturally ventilated dwelling has no mechanical design to retain.", zone.Name), zone);
                }

                if (natural && partODwellingStrategy.VentilationUnitReference is not null)
                {
                    Refuse(PartOMaterialisationRefusalReason.NaturalWithVentilationUnit, string.Format("Dwelling '{0}' is selected as naturally ventilated with ventilation unit '{1}'. A naturally ventilated dwelling is fitted with no unit.", zone.Name, partODwellingStrategy.VentilationUnitReference), zone);
                }

                dictionary_Strategy[zone.Guid] = partODwellingStrategy;
            }

            // ---- 4. Which zone each space belongs to --------------------------------------------------------

            Dictionary<Guid, List<Space>> dictionary_Spaces = [];
            Dictionary<Guid, Zone> dictionary_ZoneOfSpace = [];

            HashSet<Guid> guids_NotDwelling = [];
            zones_NotDwelling.ForEach(x => guids_NotDwelling.Add(x.Guid));

            foreach (Zone zone in zones_All)
            {
                if (!guids_Dwelling.Contains(zone.Guid) && !guids_NotDwelling.Contains(zone.Guid))
                {
                    //An unmarked zone beside marked ones, or any other grouping zone: it is neither a dwelling nor
                    //a common space, so it takes no part in who-owns-which-space.
                    continue;
                }

                List<Space> spaces = [];
                foreach (Space space in adjacencyCluster_Baseline.GetRelatedObjects<Space>(zone) ?? [])
                {
                    if (space is null || spaces.Exists(x => x.Guid == space.Guid))
                    {
                        continue;
                    }

                    spaces.Add(space);

                    if (dictionary_ZoneOfSpace.TryGetValue(space.Guid, out Zone zone_Other) && zone_Other.Guid != zone.Guid && (guids_Assessed.Contains(zone.Guid) || guids_Assessed.Contains(zone_Other.Guid)))
                    {
                        Refuse(PartOMaterialisationRefusalReason.OverlappingZones, string.Format("Space '{0}' belongs to both '{1}' and '{2}', so it has no single strategy.", space.Name, zone_Other.Name, zone.Name), zone, space.Name);
                    }

                    dictionary_ZoneOfSpace[space.Guid] = zone;
                }

                dictionary_Spaces[zone.Guid] = spaces;
            }

            List<Space> SpacesOf(Zone zone) => zone is not null && dictionary_Spaces.TryGetValue(zone.Guid, out List<Space> spaces) ? spaces : [];

            foreach (Zone zone in zones_Assessed)
            {
                if (!dictionary_Strategy.TryGetValue(zone.Guid, out PartODwellingStrategy strategy) || strategy.ActiveCooling != PartOActiveCooling.SupplyAirCooling)
                {
                    continue;
                }

                if (strategy.CoolingStatSpaceGuid == Guid.Empty)
                {
                    Refuse(PartOMaterialisationRefusalReason.CoolingControlRoomSelection, string.Format("Dwelling '{0}' has active cooling but no confirmed cooling control room. Select its room before building or simulating; older saved strategies are not assigned one automatically.", zone.Name), zone);
                }
                else if (!SpacesOf(zone).Exists(x => x.Guid == strategy.CoolingStatSpaceGuid))
                {
                    Refuse(PartOMaterialisationRefusalReason.CoolingControlRoomSelection, string.Format("Dwelling '{0}' selects cooling control room {1}, which is not one of its spaces. Select a room belonging to this dwelling.", zone.Name, strategy.CoolingStatSpaceGuid), zone);
                }
            }

            // ---- 5. The assessed common spaces, from assigned state and never from names --------------------

            List<Zone> zones_CommonSpace = [];
            foreach (Zone zone in zones_NotDwelling)
            {
                List<Space> spaces = SpacesOf(zone);
                if (spaces.Count == 0)
                {
                    continue;
                }

                bool? corridor = Query.IsTM59CommunalCorridorZone(spaces);

                if (corridor == true)
                {
                    zones_CommonSpace.Add(zone);
                }
                else if (corridor is null)
                {
                    //An overheating scenario is zone-scoped: it states one criterion for every space of its zone, and
                    //nothing in the scenario architecture states one per space. Assessing the zone as a corridor would
                    //put a non-corridor room under the corridor criterion; leaving it out would drop an assessed
                    //corridor. Neither is right, so the ambiguity is refused.
                    List<string> names_Corridor = spaces.FindAll(Query.IsTM59CommunalCorridor).ConvertAll(x => string.Format("'{0}'", x.Name));
                    List<string> names_Other = spaces.FindAll(x => !x.IsTM59CommunalCorridor()).ConvertAll(x => string.Format("'{0}'", x.Name));
                    names_Corridor.Sort(StringComparer.Ordinal);
                    names_Other.Sort(StringComparer.Ordinal);

                    Refuse(PartOMaterialisationRefusalReason.CommonSpaceUnclassifiable, string.Format("Common-space zone '{0}' mixes space(s) assigned '{1}' ({2}) with space(s) that are not ({3}). A scenario states one TM59 criterion for a whole zone, so the corridor cannot be assessed without also assessing the others as corridors. Split the zone so the corridors are a zone of their own, or assign the corridor condition consistently.", zone.Name, TM59InternalConditionResolver.CommunalCorridorInternalConditionName, string.Join(", ", names_Corridor), string.Join(", ", names_Other)), zone);
                }
                else
                {
                    result.Notes.Add(string.Format("Common-space zone '{0}' carries no space assigned '{1}', so it is not an assessed communal corridor and has no scenario. It is simulated as the baseline has it.", zone.Name, TM59InternalConditionResolver.CommunalCorridorInternalConditionName));
                }
            }

            zones_CommonSpace.Sort(CompareZones);

            //No assessed corridor is ever dropped silently. A space assigned the corridor condition that is not in a
            //whole-corridor common zone - it sits in a dwelling zone, in a grouping zone, or in no zone - cannot be
            //given the corridor scenario (scenarios are zone-scoped, one per zone), so it refuses by name.
            List<string> names_Corridor_Orphan = [];
            foreach (Space space in adjacencyCluster_Baseline.GetSpaces() ?? [])
            {
                if (space is null || !space.IsTM59CommunalCorridor())
                {
                    continue;
                }

                dictionary_ZoneOfSpace.TryGetValue(space.Guid, out Zone zone_Space);

                //In an assessed corridor zone, or in a mixed common zone that is refused above by zone.
                if (zone_Space is not null && guids_NotDwelling.Contains(zone_Space.Guid))
                {
                    continue;
                }

                names_Corridor_Orphan.Add(zone_Space is null ? string.Format("'{0}' (in no dwelling or common-space zone)", space.Name) : string.Format("'{0}' (in dwelling zone '{1}')", space.Name, zone_Space.Name));
            }

            if (names_Corridor_Orphan.Count != 0)
            {
                names_Corridor_Orphan.Sort(StringComparer.Ordinal);

                Refuse(PartOMaterialisationRefusalReason.CommonSpaceUnclassifiable, string.Format("{0} space(s) are assigned '{1}' but are not in a common-space zone made up of communal corridors: {2}. An overheating scenario covers a whole zone, so these corridors could not be assessed against the corridor criterion and would be left out of the assessment. Put each corridor in a common-space zone ('Is Dwelling' = false) of corridors only, or assign it another condition.", names_Corridor_Orphan.Count, TM59InternalConditionResolver.CommunalCorridorInternalConditionName, string.Join(", ", names_Corridor_Orphan)));
            }

            // ---- 6. Authored mechanical systems and units the strategies must not contradict ----------------

            AuthoredMechanicalSystems(adjacencyCluster_Baseline, dictionary_ZoneOfSpace, guids_Assessed, dictionary_Strategy, result, Refuse, out Dictionary<string, HashSet<Guid>> dictionary_ZonesOfUnit);

            AuthoredAirMovements(adjacencyCluster_Baseline, dictionary_ZoneOfSpace, dictionary_Strategy, dictionary_ZonesOfUnit, result, Refuse);

            foreach (Zone zone in zones_Assessed)
            {
                if (!dictionary_Strategy.TryGetValue(zone.Guid, out PartODwellingStrategy partODwellingStrategy) || partODwellingStrategy.VentilationMode != PartOVentilationMode.NaturalVentilation)
                {
                    continue;
                }

                List<string> names_Terminal = [];
                foreach (Space space in SpacesOf(zone))
                {
                    foreach (VentilationTerminal ventilationTerminal in adjacencyCluster_Baseline.VentilationTerminals(space) ?? [])
                    {
                        names_Terminal.Add(string.Format("'{0}' in '{1}'", ventilationTerminal?.Name, space.Name));
                    }
                }

                if (names_Terminal.Count != 0)
                {
                    names_Terminal.Sort(StringComparer.Ordinal);

                    result.Warnings.Add(string.Format("Naturally ventilated dwelling '{0}' carries {1} baseline design terminal(s) ({2}). They are design data, so they were copied through unconnected and inert rather than deleted; nothing ventilates through them.", zone.Name, names_Terminal.Count, string.Join(", ", names_Terminal)));
                }
            }

            if (result.Refusals.Count != 0)
            {
                return result;
            }

            // ---- 7. The Approved Document F rates and terminals - MVHR dwellings only -----------------------

            List<Zone> zones_MVHR = zones_Assessed.FindAll(x => dictionary_Strategy[x.Guid].VentilationMode == PartOVentilationMode.MVHR);
            List<Zone> zones_Natural = zones_Assessed.FindAll(x => dictionary_Strategy[x.Guid].VentilationMode == PartOVentilationMode.NaturalVentilation);

            List<Space> spaces_MVHR = [];
            zones_MVHR.ForEach(x => spaces_MVHR.AddRange(SpacesOf(x)));

            AnalyticalModel analyticalModel_Applied;

            if (spaces_MVHR.Count == 0)
            {
                //Every assessed dwelling is naturally ventilated: nothing is written at all.
                analyticalModel_Applied = new AnalyticalModel(analyticalModel_Baseline);
            }
            else
            {
                PartFOperatingMode partFOperatingMode = PartOIteration.BasePassive.PartOIterationOperatingMode(out string _) ?? PartFOperatingMode.ContinuousDesign;

                analyticalModel_Applied = analyticalModel_Baseline.ApplyPartFVentilationRates(partFOperatingMode, spaces_MVHR, out List<string> refusals_Rates, out List<string> notes_Rates);

                result.Notes.AddRange(notes_Rates);

                //As in Iteration 1a: a sized space with no rate on either direction, or no internal condition, is
                //reported and not applied; the design below still refuses anything that cannot be built.
                result.Warnings.AddRange(refusals_Rates);

                if (analyticalModel_Applied is null)
                {
                    Refuse(PartOMaterialisationRefusalReason.PartFApplication, string.Format("No Approved Document F rate could be applied to the MVHR dwellings, so there is nothing to simulate. {0}", string.Join(" ", refusals_Rates)));

                    return result;
                }
            }

            //ONE cluster instance for everything below - AnalyticalModel.AdjacencyCluster returns a fresh copy on
            //every read.
            AdjacencyCluster adjacencyCluster = analyticalModel_Applied.AdjacencyCluster;
            ProfileLibrary profileLibrary = analyticalModel_Applied.ProfileLibrary;

            if (spaces_MVHR.Count != 0)
            {
                adjacencyCluster.RealizePartFVentilationTerminals(spaces_MVHR, out List<string> notes_Terminals, out List<string> refusals_Terminals);

                result.Notes.AddRange(notes_Terminals);

                foreach (string refusal_Terminal in refusals_Terminals)
                {
                    Refuse(PartOMaterialisationRefusalReason.PartFApplication, refusal_Terminal);
                }

                if (result.Refusals.Count != 0)
                {
                    return result;
                }
            }

            // ---- 8. Each MVHR dwelling's design, in canonical order -----------------------------------------

            Dictionary<Guid, string> dictionary_Label = DwellingLabels(adjacencyCluster, zones_MVHR);

            PartOEquipmentSelection partOEquipmentSelection = analyticalModel_Baseline.GetValue<PartOEquipmentSelection>(AnalyticalModelParameter.PartOEquipmentSelection) ?? new PartOEquipmentSelection();
            List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors_ProjectTest = analyticalModel_Baseline.GetValue<PartOProjectTestVentilationUnit>(AnalyticalModelParameter.PartOProjectTestVentilationUnit)?.CapacityDescriptors();
            List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors_Temp = ventilationUnitCapacityDescriptors is null ? null : [.. ventilationUnitCapacityDescriptors];

            PartOMaterialisationRecord partOMaterialisationRecord = new();

            foreach (Zone zone in zones_MVHR)
            {
                PartODwellingStrategy partODwellingStrategy = dictionary_Strategy[zone.Guid];

                List<Space> spaces_Dwelling = [];
                foreach (Space space in SpacesOf(zone))
                {
                    //The applied space, not the baseline's: the rates were written onto replacements.
                    Space space_Applied = adjacencyCluster.GetObject<Space>(space.Guid);
                    if (space_Applied is not null)
                    {
                        spaces_Dwelling.Add(space_Applied);
                    }
                }

                if (!DesignMatchesBasis(adjacencyCluster_Baseline, adjacencyCluster, zone, SpacesOf(zone), spaces_Dwelling, partODwellingStrategy, Refuse))
                {
                    continue;
                }

                string label = dictionary_Label[zone.Guid];
                bool conditioned = false;

                string refusal_Dwelling = RealizeBaseMVHRDwelling(
                    adjacencyCluster,
                    profileLibrary,
                    spaces_Dwelling,
                    label,
                    string.Format("MVHR {0}", label),
                    airHandlingUnit =>
                    {
                        //P12. A unit this call created states no supply temperature, so a finite one means the
                        //baseline's own authored unit was reused. It is refused, never cleared: clearing an
                        //authored setpoint would be a silent engineering change.
                        if (double.IsNaN(airHandlingUnit.SummerSupplyTemperature) && double.IsNaN(airHandlingUnit.WinterSupplyTemperature))
                        {
                            return null;
                        }

                        conditioned = true;

                        return string.Format("Dwelling '{0}' would reuse authored air handling unit '{1}', which states a summer supply temperature of {2} and a winter one of {3}. A supply temperature is a setpoint the supply air is conditioned to - active cooling (or heating) - that no strategy selected: a dwelling's cooling is its selected product's manufacturer guidance, never an authored setpoint. Remove the conditioning from the baseline's unit.", zone.Name, airHandlingUnit.Name, Temperature(airHandlingUnit.SummerSupplyTemperature), Temperature(airHandlingUnit.WinterSupplyTemperature));
                    },
                    result.Notes,
                    result.Warnings,
                    out VentilationSystem ventilationSystem,
                    out AirHandlingUnit airHandlingUnit,
                    out double supplyDuty_Lps,
                    out double extractDuty_Lps);

                if (refusal_Dwelling is not null)
                {
                    Refuse(conditioned ? PartOMaterialisationRefusalReason.ConditionedReusedUnit : PartOMaterialisationRefusalReason.MechanicalDesign, string.Format("Dwelling '{0}': {1}", zone.Name, refusal_Dwelling), zone, conditioned ? airHandlingUnit?.Name : null);

                    continue;
                }

                // ---- The product ----

                VentilationUnitReference ventilationUnitReference = partODwellingStrategy.VentilationUnitReference;
                List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors_Candidate = null;

                if (ventilationUnitReference is not null)
                {
                    List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors_Known = [.. ventilationUnitCapacityDescriptors_Temp ?? [], .. ventilationUnitCapacityDescriptors_ProjectTest ?? []];

                    ventilationUnitCapacityDescriptors_Candidate = partOEquipmentSelection.AllowedDescriptors(ventilationUnitCapacityDescriptors_Temp, ventilationUnitCapacityDescriptors_ProjectTest).FindAll(x => x.VentilationUnitReference is not null && x.VentilationUnitReference.Matches(ventilationUnitReference));

                    if (ventilationUnitCapacityDescriptors_Temp is null || !ventilationUnitCapacityDescriptors_Known.Exists(x => x?.VentilationUnitReference is not null && x.VentilationUnitReference.Matches(ventilationUnitReference)))
                    {
                        Refuse(PartOMaterialisationRefusalReason.VentilationUnitUnresolved, string.Format("Dwelling '{0}' is selected with ventilation unit '{1}', which the catalogue offered does not contain, so its capability - and the selection's fingerprint - cannot be established.", zone.Name, ventilationUnitReference), zone, ventilationUnitReference.ToString());

                        continue;
                    }

                    if (ventilationUnitCapacityDescriptors_Candidate.Count == 0)
                    {
                        Refuse(PartOMaterialisationRefusalReason.VentilationUnitNotAllowed, string.Format("Dwelling '{0}' is selected with ventilation unit '{1}', which the project's equipment preselection ({2}) does not permit.", zone.Name, ventilationUnitReference, partOEquipmentSelection), zone, ventilationUnitReference.ToString());

                        continue;
                    }
                }
                else if (ventilationUnitCapacityDescriptors_Temp is not null)
                {
                    ventilationUnitCapacityDescriptors_Candidate = partOEquipmentSelection.CandidateDescriptors(ventilationUnitCapacityDescriptors_Temp, ventilationUnitCapacityDescriptors_ProjectTest);

                    if (ventilationUnitCapacityDescriptors_Candidate is null)
                    {
                        result.Warnings.Add(string.Format("Dwelling '{0}' has no selected ventilation unit and the project selects manually, so its unit '{1}' stays generic.", zone.Name, airHandlingUnit.Name));
                    }
                }

                VentilationUnitCapacityDescriptor ventilationUnitCapacityDescriptor_Selected = null;

                if (ventilationUnitCapacityDescriptors_Candidate is not null)
                {
                    VentilationUnitSelection ventilationUnitSelection = adjacencyCluster.SelectVentilationUnit(airHandlingUnit, ventilationUnitCapacityDescriptors_Candidate, out List<string> notes_Unit, out List<string> refusals_Unit);

                    result.Notes.AddRange(notes_Unit);

                    if (!ventilationUnitSelection.IsSelected)
                    {
                        Refuse(PartOMaterialisationRefusalReason.VentilationUnitSelection, string.Format("Dwelling '{0}': {1}", zone.Name, refusals_Unit.Count != 0 ? string.Join(" ", refusals_Unit) : ventilationUnitSelection.Reason), zone, airHandlingUnit.Name);

                        continue;
                    }

                    result.VentilationUnitSelections.Add(ventilationUnitSelection);
                    ventilationUnitCapacityDescriptor_Selected = ventilationUnitSelection.Descriptor;

                    airHandlingUnit = adjacencyCluster.GetObject<AirHandlingUnit>(airHandlingUnit.Guid) ?? airHandlingUnit;
                }
                else if (ventilationUnitCapacityDescriptors_Temp is null)
                {
                    result.Notes.Add(string.Format("No catalogue was offered, so dwelling '{0}''s unit '{1}' stays generic, exactly as Iteration 1a builds it.", zone.Name, airHandlingUnit.Name));
                }

                // ---- Active cooling: the selected product's manufacturer guidance ----

                if (partODwellingStrategy.ActiveCooling == PartOActiveCooling.SupplyAirCooling)
                {
                    PartOCooledDwelling partOCooledDwelling = CooledDwelling(zone, airHandlingUnit, ventilationUnitCapacityDescriptor_Selected, supplyDuty_Lps, extractDuty_Lps, ventilationUnitTemplates, Refuse);
                    if (partOCooledDwelling is null)
                    {
                        continue;
                    }

                    partOCooledDwelling.CoolingStatSpaceGuid = partODwellingStrategy.CoolingStatSpaceGuid;
                    partOMaterialisationRecord.CooledDwellings.Add(partOCooledDwelling);
                }

                result.VentilationSystems.Add(ventilationSystem);
                result.AirHandlingUnits.Add(airHandlingUnit);
                partOMaterialisationRecord.VentilationSystemGuids[zone.Guid] = ventilationSystem.Guid;
            }

            partOMaterialisationRecord.CooledDwellings.Sort((x, y) => x.ZoneGuid.CompareTo(y.ZoneGuid));

            if (result.Refusals.Count != 0)
            {
                return result;
            }

            // ---- 9. Natural ventilation stayed clean -----------------------------------------------------------

            //Every authored movement the strategies accepted is carried through unchanged: the MVHR realisation
            //removes only movements related to MVHR dwellings and their units, and those were refused above.
            foreach (SpaceAirMovement spaceAirMovement_Baseline in adjacencyCluster_Baseline.GetObjects<SpaceAirMovement>() ?? [])
            {
                SpaceAirMovement spaceAirMovement = adjacencyCluster.GetObject<SpaceAirMovement>(spaceAirMovement_Baseline.Guid);
                if (spaceAirMovement is null || spaceAirMovement.AirFlow != spaceAirMovement_Baseline.AirFlow || spaceAirMovement.From != spaceAirMovement_Baseline.From || spaceAirMovement.To != spaceAirMovement_Baseline.To)
                {
                    Refuse(PartOMaterialisationRefusalReason.Invariant, string.Format("Authored air movement '{0}' was not carried through the materialisation unchanged.", spaceAirMovement_Baseline.Name), null, spaceAirMovement_Baseline.Name);
                }
            }

            foreach (AirHandlingUnitAirMovement airHandlingUnitAirMovement_Baseline in adjacencyCluster_Baseline.GetObjects<AirHandlingUnitAirMovement>() ?? [])
            {
                if (adjacencyCluster.GetObject<AirHandlingUnitAirMovement>(airHandlingUnitAirMovement_Baseline.Guid) is null)
                {
                    Refuse(PartOMaterialisationRefusalReason.Invariant, string.Format("Authored unit air movement '{0}' was not carried through the materialisation.", airHandlingUnitAirMovement_Baseline.Name), null, airHandlingUnitAirMovement_Baseline.Name);
                }
            }

            foreach (Zone zone in zones_Natural)
            {
                string refusal_Clean = NaturalDwellingClean(adjacencyCluster_Baseline, adjacencyCluster, SpacesOf(zone));
                if (refusal_Clean is not null)
                {
                    Refuse(PartOMaterialisationRefusalReason.Invariant, string.Format("Naturally ventilated dwelling '{0}' is not clean after materialisation: {1}", zone.Name, refusal_Clean), zone);
                }
            }

            //A cooled dwelling's unit and air network stay inside the dwelling, so on the Systems route its cooled supply
            //and its transfer air can reach no neighbour and no corridor.
            foreach (PartOCooledDwelling partOCooledDwelling in partOMaterialisationRecord.CooledDwellings)
            {
                Zone zone = dictionary_Zone[partOCooledDwelling.ZoneGuid];

                string refusal_Isolated = CooledDwellingIsolated(adjacencyCluster, SpacesOf(zone), partOCooledDwelling.AirHandlingUnitGuid);
                if (refusal_Isolated is not null)
                {
                    Refuse(PartOMaterialisationRefusalReason.Invariant, string.Format("Cooled dwelling '{0}' is not isolated after materialisation: {1}", zone.Name, refusal_Isolated), zone);
                }
            }

            if (result.Refusals.Count != 0)
            {
                return result;
            }

            // ---- 10. The scenarios, one per assessed zone ---------------------------------------------------

            foreach (Zone zone in zones_Assessed)
            {
                PartODwellingStrategy partODwellingStrategy = dictionary_Strategy[zone.Guid];
                bool natural = partODwellingStrategy.VentilationMode == PartOVentilationMode.NaturalVentilation;
                bool cooled = partODwellingStrategy.ActiveCooling == PartOActiveCooling.SupplyAirCooling;

                Zone zone_Applied = adjacencyCluster.GetObject<Zone>(zone.Guid) ?? zone;

                //A cooled dwelling is its own identity, never BasePassive; its criterion is the mechanical one ("MVHR"),
                //as for any mechanically ventilated dwelling.
                List<OverheatingScenario> overheatingScenarios = Create.OverheatingScenarios(
                    [zone_Applied],
                    natural ? PartOIteration.BaseNaturalVentilation : cooled ? PartOIteration.ActiveTrimCooling : PartOIteration.BasePassive,
                    new Dictionary<Guid, string> { { zone.Guid, natural ? "NV" : "MVHR" } },
                    out List<string> refusals_Scenario);

                foreach (string refusal_Scenario in refusals_Scenario)
                {
                    Refuse(PartOMaterialisationRefusalReason.Scenario, refusal_Scenario, zone);
                }

                result.OverheatingScenarios.AddRange(overheatingScenarios);
            }

            foreach (Zone zone in zones_CommonSpace)
            {
                result.OverheatingScenarios.Add(Create.PartOCommonSpaceOverheatingScenario(adjacencyCluster.GetObject<Zone>(zone.Guid) ?? zone));
            }

            if (result.Refusals.Count != 0)
            {
                return result;
            }

            // ---- 11. The record, and the model ----------------------------------------------------------------

            List<Guid> guids_Assessed_Sorted = [.. guids_Assessed];
            guids_Assessed_Sorted.Sort();

            List<PartODwellingStrategy> strategies_Assessed = [];
            guids_Assessed_Sorted.ForEach(x => strategies_Assessed.Add(dictionary_Strategy[x]));

            partOMaterialisationRecord.Fingerprint_Baseline = SimulationResultProvenance.Fingerprint(analyticalModel_Baseline);
            partOMaterialisationRecord.Fingerprint_Strategies = Query.PartOStrategyFingerprint(strategies_Assessed, guids_Assessed_Sorted);
            partOMaterialisationRecord.Fingerprint_Catalogue = Query.PartOCatalogueFingerprint(ventilationUnitCapacityDescriptors_Temp, ventilationUnitCapacityDescriptors_ProjectTest);
            partOMaterialisationRecord.ZoneGuids_Assessed.AddRange(guids_Assessed_Sorted);

            List<Guid> guids_CommonSpace = zones_CommonSpace.ConvertAll(x => x.Guid);
            guids_CommonSpace.Sort();
            partOMaterialisationRecord.ZoneGuids_CommonSpace.AddRange(guids_CommonSpace);

            AnalyticalModel analyticalModel_Materialised = new(analyticalModel_Applied, adjacencyCluster);
            analyticalModel_Materialised.SetValue(AnalyticalModelParameter.PartOMaterialisationRecord, partOMaterialisationRecord);

            //What this model was derived from: the baseline, by identity and by the state fingerprint already taken for the record. The
            //baseline's relative locator is added by SAM_UI (Modify.LocatePartOBaselineReference), because SAM does not know its file or the result folder.
            analyticalModel_Materialised.StampPartOBaselineReference(Create.PartOBaselineReferenceFromDesign(PartODerivedCase.MixedDesign, analyticalModel_Baseline, null, null, partOMaterialisationRecord.Fingerprint_Baseline));

            result.Record = new PartOMaterialisationRecord(partOMaterialisationRecord);
            result.AnalyticalModel = analyticalModel_Materialised;

            result.Notes.Add(string.Format(
                "Materialised {0} dwelling(s) - {1} MVHR ({4} cooled), {2} naturally ventilated - and {3} assessed communal corridor(s) into one model from the clean baseline; simulated on the {5} route.",
                zones_Assessed.Count,
                zones_MVHR.Count,
                zones_Natural.Count,
                zones_CommonSpace.Count,
                partOMaterialisationRecord.CooledDwellings.Count,
                Core.Query.Description(partOMaterialisationRecord.Route)));

            return result;
        }

        /// <summary>Zones in (name, guid) order - the canonical processing order, whatever order they were stated in.</summary>
        private static int CompareZones(Zone zone_1, Zone zone_2)
        {
            int comparison = string.CompareOrdinal(zone_1?.Name, zone_2?.Name);

            return comparison != 0 ? comparison : (zone_1?.Guid ?? Guid.Empty).CompareTo(zone_2?.Guid ?? Guid.Empty);
        }

        /// <summary>
        /// The cooled dwelling its selected product's manufacturer guidance makes it, or null (refused) where the unit has
        /// no product, the product states no guidance, or the cooling operating airflow falls outside it or beyond the
        /// capacity of the catalogue entry that selected the unit (<paramref name="ventilationUnitCapacityDescriptor_Selected"/>) -
        /// the template's own capacity is checked too, so where the two disagree the smaller governs.
        /// </summary>
        private static PartOCooledDwelling CooledDwelling(Zone zone, AirHandlingUnit airHandlingUnit, VentilationUnitCapacityDescriptor ventilationUnitCapacityDescriptor_Selected, double supplyDuty_Lps, double extractDuty_Lps, IEnumerable<VentilationUnitTemplate> ventilationUnitTemplates, Action<PartOMaterialisationRefusalReason, string, Zone, string> refuse)
        {
            VentilationUnitReference ventilationUnitReference = airHandlingUnit?.SelectedVentilationUnitReference();
            if (ventilationUnitReference is null || !ventilationUnitReference.IsValid)
            {
                refuse(PartOMaterialisationRefusalReason.CoolingWithoutProductGuidance, string.Format("Dwelling '{0}' is selected with active cooling, but its unit '{1}' has no selected product. A dwelling's cooling is its product's manufacturer guidance, so a generic unit cannot be cooled: offer the catalogue, or select a product.", zone.Name, airHandlingUnit?.Name), zone, airHandlingUnit?.Name);

                return null;
            }

            VentilationUnitTemplate ventilationUnitTemplate = Query.PartOCoolingTemplate(ventilationUnitTemplates, ventilationUnitReference);
            if (ventilationUnitTemplate?.OperatingStrategy is null)
            {
                refuse(PartOMaterialisationRefusalReason.CoolingWithoutProductGuidance, string.Format("Dwelling '{0}' is selected with active cooling, but its product '{1}' has no catalogue entry stating its manufacturer's cooling guidance{2}, so nothing states how it cools.", zone.Name, ventilationUnitReference, ventilationUnitTemplate is null ? " (none, or more than one, was offered)" : string.Empty), zone, ventilationUnitReference.ToString());

                return null;
            }

            double coolingOperatingAirFlow_Lps = ventilationUnitTemplate.PartOCoolingOperatingAirFlow(supplyDuty_Lps, extractDuty_Lps, out string refusal);
            if (refusal is not null)
            {
                refuse(PartOMaterialisationRefusalReason.CoolingAirFlowOutsideGuidance, string.Format("Dwelling '{0}' is selected with active cooling, but its product '{1}' {2}", zone.Name, ventilationUnitReference, refusal), zone, ventilationUnitReference.ToString());

                return null;
            }

            //The unit was selected by a catalogue entry in this call; a product that was not (a reused unit's authored
            //selection) has no established capacity, and a cooling airflow beyond the selecting entry's is refused.
            if (ventilationUnitCapacityDescriptor_Selected is null || !ventilationUnitCapacityDescriptor_Selected.IsSufficientFor(coolingOperatingAirFlow_Lps, coolingOperatingAirFlow_Lps))
            {
                refuse(PartOMaterialisationRefusalReason.CoolingAirFlowOutsideGuidance, ventilationUnitCapacityDescriptor_Selected is null
                    ? string.Format("Dwelling '{0}' is selected with active cooling, but its product '{1}' was not selected against the catalogue offered, so the selected unit's capacity for its cooling airflow is not established.", zone.Name, ventilationUnitReference)
                    : string.Format(System.Globalization.CultureInfo.InvariantCulture, "Dwelling '{0}' is selected with active cooling, but its product '{1}' would cool at {2:0.###} l/s, beyond the selected unit's {3:0.###} / {4:0.###} l/s supply / extract capacity in the catalogue.", zone.Name, ventilationUnitReference, coolingOperatingAirFlow_Lps, ventilationUnitCapacityDescriptor_Selected.MaximumSupplyFlowRate_Lps, ventilationUnitCapacityDescriptor_Selected.MaximumExtractFlowRate_Lps), zone, ventilationUnitReference.ToString());

                return null;
            }

            return new PartOCooledDwelling(zone.Guid, airHandlingUnit.Guid, ventilationUnitReference, supplyDuty_Lps, extractDuty_Lps, coolingOperatingAirFlow_Lps, ventilationUnitTemplate.PartOCoolingGuidanceFingerprint());
        }

        /// <summary>
        /// Null where every air movement of a cooled dwelling's rooms and of its unit stays between those rooms, that
        /// unit and outside, and every system naming the unit serves only those rooms.
        /// </summary>
        private static string CooledDwellingIsolated(AdjacencyCluster adjacencyCluster, List<Space> spaces_Dwelling, Guid guid_AirHandlingUnit)
        {
            AirHandlingUnit airHandlingUnit = adjacencyCluster.GetObject<AirHandlingUnit>(guid_AirHandlingUnit);
            if (airHandlingUnit is null)
            {
                return "its unit is missing";
            }

            HashSet<Guid> guids_Space = [];
            HashSet<string> references = [new Core.ObjectReference(airHandlingUnit).ToString()];
            foreach (Space space in spaces_Dwelling)
            {
                guids_Space.Add(space.Guid);
                references.Add(new Core.ObjectReference(space).ToString());
            }

            List<SpaceAirMovement> spaceAirMovements = [.. adjacencyCluster.GetRelatedObjects<SpaceAirMovement>(airHandlingUnit) ?? []];
            foreach (Space space in spaces_Dwelling)
            {
                spaceAirMovements.AddRange(adjacencyCluster.GetRelatedObjects<SpaceAirMovement>(space) ?? []);
            }

            foreach (SpaceAirMovement spaceAirMovement in spaceAirMovements)
            {
                if (spaceAirMovement is null)
                {
                    continue;
                }

                foreach (string endpoint in new[] { spaceAirMovement.From, spaceAirMovement.To })
                {
                    if (!string.IsNullOrWhiteSpace(endpoint) && !references.Contains(endpoint))
                    {
                        return string.Format("air movement '{0}' reaches '{1}', outside the dwelling and its unit", spaceAirMovement.Name, endpoint);
                    }
                }

                foreach (Space space in adjacencyCluster.GetRelatedObjects<Space>(spaceAirMovement) ?? [])
                {
                    if (space is not null && !guids_Space.Contains(space.Guid))
                    {
                        return string.Format("air movement '{0}' is related to '{1}', outside the dwelling", spaceAirMovement.Name, space.Name);
                    }
                }
            }

            foreach (VentilationSystem ventilationSystem in adjacencyCluster.GetObjects<VentilationSystem>() ?? [])
            {
                if (ventilationSystem?.GetValue<string>(VentilationSystemParameter.SupplyUnitName) != airHandlingUnit.Name && ventilationSystem?.GetValue<string>(VentilationSystemParameter.ExhaustUnitName) != airHandlingUnit.Name)
                {
                    continue;
                }

                foreach (Space space in adjacencyCluster.GetRelatedObjects<Space>(ventilationSystem) ?? [])
                {
                    if (space is not null && !guids_Space.Contains(space.Guid))
                    {
                        return string.Format("system '{0}' of its unit serves '{1}', outside the dwelling", ventilationSystem.FullName, space.Name);
                    }
                }
            }

            return null;
        }

        private static string Temperature(double value) => double.IsNaN(value) ? "none" : string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.###} degC", value);

        /// <summary>
        /// Each MVHR dwelling's label, from its zone name: the system id, and (as <c>MVHR &lt;label&gt;</c>) the
        /// unit name. Unique against every unit and space name the model carries (TAS names the unit's plant zone
        /// after the unit) and against each other, disambiguated <c>" (n)"</c> in canonical dwelling order, so it
        /// depends on the baseline alone and never on processing order.
        /// </summary>
        private static Dictionary<Guid, string> DwellingLabels(AdjacencyCluster adjacencyCluster, List<Zone> zones_MVHR)
        {
            HashSet<string> names = new(StringComparer.OrdinalIgnoreCase);

            foreach (AirHandlingUnit airHandlingUnit in adjacencyCluster.GetObjects<AirHandlingUnit>() ?? [])
            {
                if (!string.IsNullOrWhiteSpace(airHandlingUnit?.Name))
                {
                    names.Add(airHandlingUnit.Name.Trim());
                }
            }

            foreach (Space space in adjacencyCluster.GetSpaces() ?? [])
            {
                if (!string.IsNullOrWhiteSpace(space?.Name))
                {
                    names.Add(space.Name.Trim());
                }
            }

            Dictionary<Guid, string> result = [];

            foreach (Zone zone in zones_MVHR)
            {
                string label_Base = string.IsNullOrWhiteSpace(zone.Name) ? zone.Guid.ToString("N").Substring(0, 8) : zone.Name.Trim();
                string label = label_Base;

                int index = 2;
                while (names.Contains(string.Format("MVHR {0}", label)))
                {
                    label = string.Format("{0} ({1})", label_Base, index);
                    index++;
                }

                names.Add(string.Format("MVHR {0}", label));
                result[zone.Guid] = label;
            }

            return result;
        }

        /// <summary>
        /// Refuses authored mechanical duty or plant the selected strategies contradict: a system or unit that
        /// straddles zones touching an assessed dwelling (shared plant), mechanical duty in a naturally
        /// ventilated dwelling, and plant in an MVHR dwelling not connected to its design terminals.
        /// <para>
        /// Only duty-bearing plant is judged (PR-2, <see cref="Query.PartOAuthoredPlantDuty(AdjacencyCluster, VentilationSystem)"/>).
        /// A system whose terminals, movements and units state no duty - no finite non-zero design airflow, no air
        /// movement, no selected product - is inert template metadata, whether or not it
        /// names a unit that exists: noted, left as authored, never refused. The unit-to-zones map is still recorded
        /// for every named unit, because the authored air-movement rule reads it.
        /// </para>
        /// </summary>
        private static void AuthoredMechanicalSystems(AdjacencyCluster adjacencyCluster, Dictionary<Guid, Zone> dictionary_ZoneOfSpace, HashSet<Guid> guids_Assessed, Dictionary<Guid, PartODwellingStrategy> dictionary_Strategy, PartOMaterialisation result, Action<PartOMaterialisationRefusalReason, string, Zone, string> refuse, out Dictionary<string, HashSet<Guid>> dictionary_ZonesOfUnit)
        {
            List<AirHandlingUnit> airHandlingUnits = adjacencyCluster.GetObjects<AirHandlingUnit>() ?? [];

            //Unit name -> the zones every effective system naming it serves. A unit is bound to its systems by
            //name, so that is how a unit shared across dwellings is found.
            dictionary_ZonesOfUnit = new(StringComparer.Ordinal);
            Dictionary<Guid, Zone> dictionary_Zone = [];

            List<VentilationSystem> ventilationSystems = adjacencyCluster.GetObjects<VentilationSystem>() ?? [];
            ventilationSystems.Sort((x, y) => string.CompareOrdinal(x?.FullName, y?.FullName));

            foreach (VentilationSystem ventilationSystem in ventilationSystems)
            {
                if (ventilationSystem is null)
                {
                    continue;
                }

                List<VentilationTerminal> ventilationTerminals = adjacencyCluster.GetRelatedObjects<VentilationTerminal>(ventilationSystem) ?? [];

                List<Space> spaces = [.. adjacencyCluster.GetRelatedObjects<Space>(ventilationSystem) ?? []];
                foreach (VentilationTerminal ventilationTerminal in ventilationTerminals)
                {
                    spaces.AddRange(adjacencyCluster.GetRelatedObjects<Space>(ventilationTerminal) ?? []);
                }

                HashSet<Guid> guids_Zone = [];
                bool unzoned = false;
                foreach (Space space in spaces)
                {
                    if (space is null)
                    {
                        continue;
                    }

                    if (dictionary_ZoneOfSpace.TryGetValue(space.Guid, out Zone zone))
                    {
                        guids_Zone.Add(zone.Guid);
                        dictionary_Zone[zone.Guid] = zone;
                    }
                    else
                    {
                        unzoned = true;
                    }
                }

                string name_Unit_Supply = ventilationSystem.GetValue<string>(VentilationSystemParameter.SupplyUnitName);
                string name_Unit_Exhaust = ventilationSystem.GetValue<string>(VentilationSystemParameter.ExhaustUnitName);

                List<string> names_Unit = [];
                foreach (string name_Unit in new[] { name_Unit_Supply, name_Unit_Exhaust })
                {
                    if (!string.IsNullOrWhiteSpace(name_Unit) && !names_Unit.Contains(name_Unit) && airHandlingUnits.Exists(x => x?.Name == name_Unit))
                    {
                        names_Unit.Add(name_Unit);
                    }
                }

                //Recorded for EVERY system that serves spaces, assessed or not, before anything below is skipped. A
                //unit is shared when its systems together reach an assessed dwelling and anything else - another
                //dwelling (assessed or not), a common space, or rooms in no zone - and only the whole set of its
                //systems can show that. Recording only the systems that touch an assessed dwelling would miss a unit
                //shared with an unassessed neighbour, and the MVHR realisation would then rebuild that unit's
                //movements for one dwelling and delete the other system's.
                if (spaces.Count != 0)
                {
                    foreach (string name_Unit in names_Unit)
                    {
                        if (!dictionary_ZonesOfUnit.TryGetValue(name_Unit, out HashSet<Guid> guids_Zone_Unit))
                        {
                            guids_Zone_Unit = [];
                            dictionary_ZonesOfUnit[name_Unit] = guids_Zone_Unit;
                        }

                        guids_Zone_Unit.UnionWith(guids_Zone);
                        if (unzoned)
                        {
                            guids_Zone_Unit.Add(Guid.Empty);
                        }
                    }
                }

                List<Guid> guids_Assessed_Touched = [.. guids_Zone];
                guids_Assessed_Touched.RemoveAll(x => !guids_Assessed.Contains(x));

                if (guids_Assessed_Touched.Count == 0)
                {
                    continue;
                }

                //PR-2: plant is judged by the duty it states, never by a unit merely existing. AddMechanicalSystems
                //names and creates a unit on every mechanical template system (MV 1 -> AHU1) whether or not anyone
                //designs it, and that scaffolding is not plant.
                if (adjacencyCluster.PartOAuthoredPlantDuty(ventilationSystem).IsInert)
                {
                    result.Notes.Add(names_Unit.Count == 0
                        ? string.Format("Ventilation system '{0}' ({1}) is related to assessed spaces but carries no design duty and no unit, so it is template metadata: left exactly as authored.", ventilationSystem.FullName, ventilationSystem.Type?.Name ?? "-")
                        : string.Format("Ventilation system '{0}' ({1}) is related to assessed spaces and names unit {2}, but neither states any duty - no design terminal airflow, no air movement, no selected product - so they are inert template metadata: left exactly as authored and not part of this assessment.", ventilationSystem.FullName, ventilationSystem.Type?.Name ?? "-", string.Join(", ", names_Unit.ConvertAll(x => string.Format("'{0}'", x)))));

                    continue;
                }

                if (guids_Zone.Count > 1 || unzoned)
                {
                    List<string> names_Zone = [];
                    foreach (Guid guid in guids_Zone)
                    {
                        names_Zone.Add(string.Format("'{0}'", dictionary_Zone[guid].Name));
                    }

                    names_Zone.Sort(StringComparer.Ordinal);
                    if (unzoned)
                    {
                        names_Zone.Add("spaces in no assessed zone");
                    }

                    refuse(PartOMaterialisationRefusalReason.SharedSystem, string.Format("Authored ventilation system '{0}' carries mechanical duty or plant and serves {1}, so it cannot belong to one dwelling's selected design. Shared plant is never split or rewritten to fit the strategies: give each dwelling its own system in the baseline, or remove the shared one.", ventilationSystem.FullName, string.Join(", ", names_Zone)), null, ventilationSystem.FullName);

                    continue;
                }

                Zone zone_Single = dictionary_Zone[guids_Assessed_Touched[0]];
                PartODwellingStrategy partODwellingStrategy = dictionary_Strategy.TryGetValue(zone_Single.Guid, out PartODwellingStrategy value) ? value : null;

                if (partODwellingStrategy?.VentilationMode == PartOVentilationMode.NaturalVentilation)
                {
                    refuse(PartOMaterialisationRefusalReason.NaturalOverMechanicalDuty, string.Format("Dwelling '{0}' is selected as naturally ventilated, but authored ventilation system '{1}' serves it with mechanical duty or plant. The authored design says mechanical and the strategy says natural; one of them is wrong, and removing the system would delete authored design data.", zone_Single.Name, ventilationSystem.FullName), zone_Single, ventilationSystem.FullName);
                }
                else if (partODwellingStrategy?.VentilationMode == PartOVentilationMode.MVHR && ventilationTerminals.Count == 0)
                {
                    refuse(PartOMaterialisationRefusalReason.UnconnectedAuthoredPlant, string.Format("Dwelling '{0}' is selected as MVHR, but authored ventilation system '{1}' serves it with a unit while being connected to none of its design terminals, so the Part O design would sit beside a second plant. Connect the system to the dwelling's terminals, or remove it from the baseline.", zone_Single.Name, ventilationSystem.FullName), zone_Single, ventilationSystem.FullName);
                }
            }

            foreach (KeyValuePair<string, HashSet<Guid>> keyValuePair in dictionary_ZonesOfUnit)
            {
                //Shared only matters where it reaches an assessed dwelling: a unit shared between two unassessed
                //dwellings is not this materialisation's to judge, and it is left exactly as authored.
                bool assessed = false;
                foreach (Guid guid in keyValuePair.Value)
                {
                    assessed |= guids_Assessed.Contains(guid);
                }

                //An inert unit is scaffolding, not plant, so it cannot be shared plant either (PR-2).
                bool inert = !airHandlingUnits.Exists(x => x?.Name == keyValuePair.Key && !adjacencyCluster.PartOAuthoredPlantDuty(x).IsInert);

                if (assessed && !inert && keyValuePair.Value.Count > 1)
                {
                    List<string> names_Zone = [];
                    foreach (Guid guid in keyValuePair.Value)
                    {
                        names_Zone.Add(guid == Guid.Empty ? "spaces in no assessed zone" : dictionary_Zone.TryGetValue(guid, out Zone zone) ? string.Format("'{0}'", zone.Name) : guid.ToString());
                    }

                    names_Zone.Sort(StringComparer.Ordinal);

                    refuse(PartOMaterialisationRefusalReason.SharedSystem, string.Format("Authored air handling unit '{0}' supplies systems serving {1}, so it cannot belong to one dwelling's selected design. It is never split or rewritten to fit the strategies: give each dwelling its own unit in the baseline.", keyValuePair.Key, string.Join(", ", names_Zone)), null, keyValuePair.Key);
                }
            }
        }

        /// <summary>
        /// Decides, against the selected strategies, whether each air movement the baseline authors can be carried
        /// into the mixed model. The rule follows where the movement's air goes:
        /// <list type="bullet">
        /// <item>
        /// <b>It reaches an MVHR dwelling</b> (an endpoint or related space in one, or the unit an effective system
        /// serving one names): <b>refused</b> (<see cref="PartOMaterialisationRefusalReason.AuthoredAirMovementConflict"/>).
        /// That dwelling's runtime air network belongs to the materialisation. <c>RealizeBaseMVHRDwelling</c> first
        /// removes every movement related to the dwelling's spaces and its unit (<c>RemoveBaseMVHRAirMovementObjects</c>),
        /// then rebuilds and balances the network from the design terminals. So the authored movement can only be
        /// deleted (authored design data lost) or kept beside the new network (the dwelling ventilated twice).
        /// </item>
        /// <item>
        /// <b>It exchanges air between a naturally ventilated dwelling and a unit, or outside</b>, meaning a prescribed
        /// supply from plant or a prescribed extract to plant or outside: <b>refused</b>
        /// (<see cref="PartOMaterialisationRefusalReason.NaturalOverMechanicalDuty"/>). That is continuous mechanical
        /// supply or extract, and the natural-ventilation scenario key states the dwelling has none.
        /// </item>
        /// <item>
        /// <b>It transfers air between spaces</b>, at least one in a naturally ventilated dwelling and none in an MVHR
        /// one: <b>carried through unchanged</b> and reported. It states no mechanical route, and a homogeneous
        /// Iteration 1b run carries it to TAS in exactly the same way.
        /// </item>
        /// <item>
        /// <b>It touches no assessed dwelling</b> (unassessed dwellings, common spaces, other plant): carried through.
        /// </item>
        /// </list>
        /// A unit's own plant-zone movement (<c>AirHandlingUnitAirMovement</c>, for example from <c>CreateIZAMBySetPoint</c>)
        /// follows its unit. It is refused when the unit serves an MVHR dwelling, for the same deletion reason. A unit
        /// serving a natural dwelling is already refused together with its system.
        /// </summary>
        private static void AuthoredAirMovements(AdjacencyCluster adjacencyCluster, Dictionary<Guid, Zone> dictionary_ZoneOfSpace, Dictionary<Guid, PartODwellingStrategy> dictionary_Strategy, Dictionary<string, HashSet<Guid>> dictionary_ZonesOfUnit, PartOMaterialisation result, Action<PartOMaterialisationRefusalReason, string, Zone, string> refuse)
        {
            PartOVentilationMode Mode(Guid guid_Zone) => dictionary_Strategy.TryGetValue(guid_Zone, out PartODwellingStrategy value) ? value.VentilationMode : PartOVentilationMode.Undefined;

            Dictionary<Guid, Zone> dictionary_Zone = [];
            foreach (Zone zone in dictionary_ZoneOfSpace.Values)
            {
                dictionary_Zone[zone.Guid] = zone;
            }

            //The MVHR dwelling a unit serves, if any.
            Zone ZoneOfUnitMVHR(AirHandlingUnit airHandlingUnit)
            {
                if (airHandlingUnit?.Name is null || !dictionary_ZonesOfUnit.TryGetValue(airHandlingUnit.Name, out HashSet<Guid> guids_Zone))
                {
                    return null;
                }

                foreach (Guid guid in guids_Zone)
                {
                    if (Mode(guid) == PartOVentilationMode.MVHR && dictionary_Zone.TryGetValue(guid, out Zone zone))
                    {
                        return zone;
                    }
                }

                return null;
            }

            List<string> names_Carried = [];

            List<SpaceAirMovement> spaceAirMovements = adjacencyCluster.GetObjects<SpaceAirMovement>() ?? [];
            spaceAirMovements.Sort((x, y) => string.CompareOrdinal(x?.Name, y?.Name));

            foreach (SpaceAirMovement spaceAirMovement in spaceAirMovements)
            {
                if (spaceAirMovement is null)
                {
                    continue;
                }

                Core.SAMObject sAMObject_From = Query.AirMovementEndpoint(adjacencyCluster, spaceAirMovement.From, out bool resolved_From);
                Core.SAMObject sAMObject_To = Query.AirMovementEndpoint(adjacencyCluster, spaceAirMovement.To, out bool resolved_To);

                if (!resolved_From || !resolved_To || sAMObject_From is null)
                {
                    //Refused by the baseline findings.
                    continue;
                }

                List<Space> spaces = [.. adjacencyCluster.GetRelatedObjects<Space>(spaceAirMovement) ?? []];
                List<AirHandlingUnit> airHandlingUnits = [.. adjacencyCluster.GetRelatedObjects<AirHandlingUnit>(spaceAirMovement) ?? []];

                foreach (Core.SAMObject sAMObject in new[] { sAMObject_From, sAMObject_To })
                {
                    if (sAMObject is Space space_Endpoint)
                    {
                        spaces.Add(space_Endpoint);
                    }
                    else if (sAMObject is AirHandlingUnit airHandlingUnit_Endpoint)
                    {
                        airHandlingUnits.Add(airHandlingUnit_Endpoint);
                    }
                }

                Zone zone_MVHR = null;
                Zone zone_Natural = null;
                foreach (Space space in spaces)
                {
                    if (space is null || !dictionary_ZoneOfSpace.TryGetValue(space.Guid, out Zone zone))
                    {
                        continue;
                    }

                    PartOVentilationMode partOVentilationMode = Mode(zone.Guid);
                    if (partOVentilationMode == PartOVentilationMode.MVHR)
                    {
                        zone_MVHR ??= zone;
                    }
                    else if (partOVentilationMode == PartOVentilationMode.NaturalVentilation)
                    {
                        zone_Natural ??= zone;
                    }
                }

                foreach (AirHandlingUnit airHandlingUnit in airHandlingUnits)
                {
                    zone_MVHR ??= ZoneOfUnitMVHR(airHandlingUnit);
                }

                if (zone_MVHR is not null)
                {
                    refuse(PartOMaterialisationRefusalReason.AuthoredAirMovementConflict, string.Format("Authored air movement '{0}' reaches MVHR dwelling '{1}'. The materialisation owns that dwelling's runtime air network: it removes every movement related to the dwelling's rooms and unit, then rebuilds a balanced one from the design terminals. So the authored movement would be deleted, or the dwelling ventilated twice. Remove it from the baseline, or select natural ventilation for the dwelling.", spaceAirMovement.Name, zone_MVHR.Name), zone_MVHR, spaceAirMovement.Name);

                    continue;
                }

                if (zone_Natural is not null)
                {
                    if (airHandlingUnits.Count != 0 || sAMObject_To is null)
                    {
                        refuse(PartOMaterialisationRefusalReason.NaturalOverMechanicalDuty, string.Format(System.Globalization.CultureInfo.InvariantCulture, "Dwelling '{0}' is selected as naturally ventilated, but authored air movement '{1}' moves {2:0.###} l/s {3}. That is prescribed mechanical supply or extract, and the natural-ventilation scenario states the dwelling has none.", zone_Natural.Name, spaceAirMovement.Name, spaceAirMovement.AirFlow * 1000, airHandlingUnits.Count != 0 ? "to or from an air handling unit" : "to outside"), zone_Natural, spaceAirMovement.Name);

                        continue;
                    }

                    names_Carried.Add(string.Format("'{0}' ({1})", spaceAirMovement.Name, zone_Natural.Name));
                }
            }

            foreach (AirHandlingUnitAirMovement airHandlingUnitAirMovement in adjacencyCluster.GetObjects<AirHandlingUnitAirMovement>() ?? [])
            {
                foreach (AirHandlingUnit airHandlingUnit in adjacencyCluster.GetRelatedObjects<AirHandlingUnit>(airHandlingUnitAirMovement) ?? [])
                {
                    Zone zone_MVHR = ZoneOfUnitMVHR(airHandlingUnit);
                    if (zone_MVHR is not null)
                    {
                        refuse(PartOMaterialisationRefusalReason.AuthoredAirMovementConflict, string.Format("Authored unit air movement '{0}' belongs to air handling unit '{1}', which serves MVHR dwelling '{2}'. The materialisation rebuilds that unit's plant-zone conditions from the unit itself, so the authored conditions would be deleted. Remove them from the baseline.", airHandlingUnitAirMovement.Name, airHandlingUnit.Name, zone_MVHR.Name), zone_MVHR, airHandlingUnitAirMovement.Name);

                        break;
                    }
                }
            }

            if (names_Carried.Count != 0)
            {
                names_Carried.Sort(StringComparer.Ordinal);

                result.Warnings.Add(string.Format("{0} authored inter-zone transfer movement(s) reach naturally ventilated dwellings and were carried through unchanged, just as a homogeneous Iteration 1b run carries them: {1}.", names_Carried.Count, string.Join(", ", names_Carried)));
            }
        }

        /// <summary>
        /// Whether an MVHR dwelling's realised design terminals are the design its strategy says they are - the
        /// Approved Document F requirement, or the retained design its fingerprint guards. Refuses otherwise.
        /// </summary>
        private static bool DesignMatchesBasis(AdjacencyCluster adjacencyCluster_Baseline, AdjacencyCluster adjacencyCluster, Zone zone, List<Space> spaces_Baseline, List<Space> spaces_Dwelling, PartODwellingStrategy partODwellingStrategy, Action<PartOMaterialisationRefusalReason, string, Zone, string> refuse)
        {
            if (partODwellingStrategy.DesignAirFlowBasis == PartODesignAirFlowBasis.RetainedDesign)
            {
                //Counted on the BASELINE: the realisation above creates a terminal for every requirement that has
                //none, so on the working cluster a dwelling always has terminals. A retained design exists only
                //where the engineer wrote it onto the baseline.
                int count = 0;
                spaces_Baseline.ForEach(x => count += adjacencyCluster_Baseline.VentilationTerminals(x)?.Count ?? 0);

                //Compared AFTER the realisation, so a requirement that gained no terminal on the baseline (a new
                //room, a recalculated Part F) makes the design differ from what was accepted, and it is stale.
                string fingerprint = adjacencyCluster.PartODwellingDesignFingerprint(zone);

                if (count == 0 || fingerprint != partODwellingStrategy.DesignFingerprint)
                {
                    refuse(PartOMaterialisationRefusalReason.RetainedDesignStale, count == 0
                        ? string.Format("Dwelling '{0}' is selected with a retained design airflow, but carries no design terminals to retain it on. Accept the design onto the baseline's terminals first.", zone.Name)
                        : string.Format("Dwelling '{0}' is selected with a retained design airflow, but its design terminals no longer match the design that was accepted (fingerprint {1}, now {2}). The retained design is stale; accept it again or select the Part F requirement.", zone.Name, partODwellingStrategy.DesignFingerprint, fingerprint), zone, null);

                    return false;
                }

                return true;
            }

            const double tolerance_Lps = 0.001;
            List<string> differences = [];

            foreach (Space space in spaces_Dwelling)
            {
                List<VentilationTerminal> ventilationTerminals = adjacencyCluster.VentilationTerminals(space) ?? [];
                List<PartFVentilationTerminalRequirement> requirements = space.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData)?.Terminals ?? [];

                double[] sums = new double[requirements.Count];

                foreach (VentilationTerminal ventilationTerminal in ventilationTerminals)
                {
                    PartFTerminalReference partFTerminalReference = ventilationTerminal?.GetValue<PartFTerminalReference>(VentilationTerminalParameter.PartFTerminalReference);
                    int index = partFTerminalReference is null ? -1 : requirements.FindIndex(x => x is not null && partFTerminalReference.Matches(x));

                    if (index < 0)
                    {
                        differences.Add(string.Format("'{0}' carries terminal '{1}', which realises no requirement", space.Name, ventilationTerminal?.Name));

                        continue;
                    }

                    sums[index] += ventilationTerminal.DesignFlowRate_Lps ?? 0;
                }

                for (int i = 0; i < requirements.Count; i++)
                {
                    double? continuous_Lps = requirements[i]?.ContinuousDesignFlowRate_Lps;
                    if (!continuous_Lps.HasValue || double.IsNaN(continuous_Lps.Value))
                    {
                        continue;
                    }

                    if (System.Math.Abs(sums[i] - continuous_Lps.Value) > tolerance_Lps)
                    {
                        differences.Add(string.Format(System.Globalization.CultureInfo.InvariantCulture, "'{0}' {1}: {2:0.###} l/s designed against {3:0.###} l/s required", space.Name, requirements[i].Name, sums[i], continuous_Lps.Value));
                    }
                }
            }

            if (differences.Count == 0)
            {
                return true;
            }

            differences.Sort(StringComparer.Ordinal);

            refuse(PartOMaterialisationRefusalReason.DesignDiffersFromRequirement, string.Format("Dwelling '{0}' is selected at the Approved Document F requirement, but its design terminals differ from it ({1}). They are not reset silently: select the retained design it was accepted as, or correct the terminals.", zone.Name, string.Join("; ", differences)), zone, null);

            return false;
        }

        /// <summary>
        /// Null where a naturally ventilated dwelling carries, after materialisation, exactly its baseline state:
        /// the same internal conditions, the same terminals and none connected, no system and no air movement.
        /// </summary>
        private static string NaturalDwellingClean(AdjacencyCluster adjacencyCluster_Baseline, AdjacencyCluster adjacencyCluster, List<Space> spaces_Baseline)
        {
            //By object reference only, which is what Modify.AddAirMovementObjects writes. Never by name: two
            //dwellings routinely both have a "Bedroom 1".
            HashSet<string> references = [];
            foreach (Space space in spaces_Baseline)
            {
                references.Add(new Core.ObjectReference(space).ToString());
            }

            foreach (Space space_Baseline in spaces_Baseline)
            {
                Space space = adjacencyCluster.GetObject<Space>(space_Baseline.Guid);
                if (space is null)
                {
                    return string.Format("space '{0}' is missing", space_Baseline.Name);
                }

                if (space.InternalCondition?.Guid != space_Baseline.InternalCondition?.Guid)
                {
                    return string.Format("the internal condition of '{0}' was replaced", space.Name);
                }

                if ((adjacencyCluster.GetRelatedObjects<VentilationSystem>(space) ?? []).Exists(x => x?.Type?.Guid == guid_VentilationSystemType_MVHR))
                {
                    return string.Format("'{0}' is served by a Part O MVHR system", space.Name);
                }

                List<VentilationTerminal> ventilationTerminals = adjacencyCluster.VentilationTerminals(space) ?? [];
                if (ventilationTerminals.Count != (adjacencyCluster_Baseline.VentilationTerminals(space_Baseline)?.Count ?? 0))
                {
                    return string.Format("the design terminals of '{0}' changed", space.Name);
                }

                if (ventilationTerminals.Exists(x => (adjacencyCluster.GetRelatedObjects<VentilationSystem>(x)?.Count ?? 0) != (adjacencyCluster_Baseline.GetRelatedObjects<VentilationSystem>(x)?.Count ?? 0)))
                {
                    return string.Format("a design terminal of '{0}' was connected", space.Name);
                }
            }

            //A movement the baseline already carried is authored data the strategies accepted (AuthoredAirMovements);
            //only a movement the materialisation generated would be a mechanical design reaching this dwelling.
            HashSet<Guid> guids_Movement_Baseline = [];
            (adjacencyCluster_Baseline.GetObjects<SpaceAirMovement>() ?? []).ForEach(x => guids_Movement_Baseline.Add(x.Guid));

            foreach (SpaceAirMovement spaceAirMovement in adjacencyCluster.GetObjects<SpaceAirMovement>() ?? [])
            {
                if (spaceAirMovement is null || guids_Movement_Baseline.Contains(spaceAirMovement.Guid))
                {
                    continue;
                }

                if ((spaceAirMovement.From is not null && references.Contains(spaceAirMovement.From)) || (spaceAirMovement.To is not null && references.Contains(spaceAirMovement.To)))
                {
                    return string.Format("generated air movement '{0}' reaches it", spaceAirMovement.Name);
                }
            }

            return null;
        }
    }
}
