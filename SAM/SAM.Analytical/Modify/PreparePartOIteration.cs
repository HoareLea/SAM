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
        /// Prepares a model for one Approved Document O base iteration: settles the ventilation route the
        /// assessment is being made over, decides from that route whether the Approved Document F airflows
        /// belong on the model, carries them where they do, reports how the model's authored opening
        /// behaviour compares with the stage, and states the scenarios the results are attributed to.
        /// <para>
        /// <b>This is the whole preparation, in the library.</b>
        /// <c>SAMAnalytical.PreparePartOIteration</c> reads Grasshopper parameters and raises Grasshopper
        /// messages; every decision it used to make inline is made here, so the decisions are testable
        /// without a Grasshopper assembly and the component and the tests cannot drift apart.
        /// </para>
        ///
        /// <para><b>The order of the four gates, and why it is that order</b></para>
        /// <list type="number">
        /// <item>
        /// <b>The route the assessment states.</b> <see cref="Enums.PartOVentilationMode"/>, resolved from
        /// what the assessed zones state and from nothing else. Settled first because it is the fact every
        /// later decision depends on, and because reaching the airflow application without it is the defect
        /// this whole design closes.
        /// </item>
        /// <item>
        /// <b>The route the iteration is defined over.</b> An iteration is not neutral about how the
        /// dwelling is ventilated - its operating assumptions go into the permanent scenario key - so a
        /// stage and a route that disagree produce a true simulation filed under a false claim.
        /// </item>
        /// <item>
        /// <b>Whether the Part F airflows belong on the model</b>, which on the MVHR route additionally
        /// needs the Approved Document F condition the stage runs at.
        /// </item>
        /// <item>
        /// <b>How the authored openings compare with the stage.</b> Reported, never acted on.
        /// </item>
        /// </list>
        ///
        /// <para>
        /// <b>The route is stated, never inferred, and never written.</b> No <c>SAM_System</c>,
        /// <c>SystemTemplate</c> or <c>InternalCondition.VentilationSystemTypeName</c> on the model is read
        /// to decide what is simulated - they are metadata that may be stale, may predate the assessment,
        /// and may describe a different design stage. Nor is any of them mutated to force a route: that
        /// would put the decision straight back into the metadata it was taken out of, and would make the
        /// model on disk a lie about the building. See <c>documentation/PartO-ARCHITECTURE.md</c>.
        /// </para>
        /// <para>
        /// <b>The model is never rewritten to fit the stage.</b> An <c>OpeningRestriction</c> is authored
        /// building data and <c>PartOOpeningProperties.Schedule</c> is derived from it, so resetting a
        /// restriction to match the stage's assumption deletes the aperture's <c>PartO_DayOpen_HH_HH</c>
        /// availability schedule from the model that reaches TAS. The disagreement is reported instead.
        /// </para>
        /// </summary>
        /// <param name="analyticalModel">
        /// The model to prepare. <b>Not modified</b> - a prepared copy is returned on the result.
        /// </param>
        /// <param name="partOIteration">
        /// The base iteration being prepared - <c>BasePassive</c> (1a, MVHR) or
        /// <c>BaseNaturalVentilation</c> (1b). These are alternatives, not successive stages.
        /// </param>
        /// <param name="zones">
        /// The zones to assess, already resolved against <paramref name="analyticalModel"/>. Null means
        /// every zone the model carries; an empty sequence means the caller named zones and none of them
        /// resolved. Either way, no assessed zone means no stated route, which refuses.
        /// </param>
        /// <param name="dictionary_VentilationStrategy">
        /// The Part O ventilation route each assessed zone states, by zone guid - <c>NV</c> /
        /// <c>NaturalVentilation</c>, or <c>MVHR</c> / <c>MVRE</c>. Required, never defaulted.
        /// </param>
        /// <param name="ventilationUnitCapacityDescriptors">
        /// The reusable ventilation unit products each dwelling may be fitted with. <b>Optional, and null
        /// keeps Iteration 1a's behaviour exactly</b>: the generic unit is built, its design duty is
        /// derived, and no product is selected - which is the honest state of a model nobody has offered a
        /// catalogue to.
        /// <para>
        /// Where a catalogue is supplied, each dwelling selects <b>its own</b> smallest compliant unit
        /// from its own duty. Nothing is aggregated across dwellings, nothing is balanced between them,
        /// and one dwelling's answer cannot move another's. A dwelling for which nothing is compliant is
        /// refused by name while the others still select - see <c>Modify.SelectVentilationUnit</c>.
        /// </para>
        /// <para>
        /// An argument rather than a library read, because which products exist is a fact about whoever is
        /// asking - the same boundary <c>Query.CapableSystems</c> draws for system templates.
        /// </para>
        /// </param>
        public static PartOIterationPreparation PreparePartOIteration(this AnalyticalModel analyticalModel, PartOIteration partOIteration, IEnumerable<Zone> zones, Dictionary<Guid, string> dictionary_VentilationStrategy, IEnumerable<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors = null, bool isolate = false)
        {
            PartOIterationPreparation result = new();

            if (analyticalModel == null)
            {
                result.Refusal = "No analytical model was supplied, so there is nothing to prepare.";

                return result;
            }

            //Resolved off the SUPPLIED model rather than the applied one, because the route has to be known
            //before anything is applied. The airflow application never adds, removes or re-guids a zone -
            //it only replaces spaces - so the two sets are the same set, and the scenarios below are still
            //stated over zone objects taken from the model that will be simulated.
            List<Zone> zones_Assessed = [];
            if (zones == null)
            {
                zones_Assessed = analyticalModel.GetZones() ?? [];
            }
            else
            {
                foreach (Zone zone in zones)
                {
                    if (zone != null)
                    {
                        zones_Assessed.Add(zone);
                    }
                }
            }

            // ---- 1. The route the assessment states -----------------------------------------------------

            result.VentilationMode = Query.PartOVentilationMode(zones_Assessed, dictionary_VentilationStrategy, out string refusal_VentilationMode);

            result.AirflowApplication = Query.PartOPartFAirflowApplication(result.VentilationMode, out string diagnostic_Airflow);

            if (result.VentilationMode == Enums.PartOVentilationMode.Undefined)
            {
                //Refused BEFORE anything is applied, and with no model on the result. Continuing here is
                //materially different from continuing past an opening-restriction disagreement: that one
                //only mislabels which assumption a true result was obtained under, while this one would put
                //continuous mechanical supply and extract into a dwelling that may have none.
                result.Refusal = refusal_VentilationMode;

                return result;
            }

            // ---- 2. The route the iteration is defined over ---------------------------------------------

            PartOVentilationMode partOVentilationMode_Iteration = partOIteration.PartOIterationVentilationMode(out string refusal_Iteration);

            if (partOVentilationMode_Iteration == Enums.PartOVentilationMode.Undefined)
            {
                result.Refusal = refusal_Iteration;

                return result;
            }

            if (partOVentilationMode_Iteration != result.VentilationMode)
            {
                //Not a formality. The iteration's operating assumptions are part of the derived
                //OverheatingScenario.Key, so this pairing would not merely be confusing - it would mint a
                //permanent identity asserting something false about the building, and every result ever
                //attributed to that key would carry the assertion with it.
                result.Refusal = string.Format(
                    "The {0} iteration is the base configuration for the {1} route, but the assessed zones state {2}. These are alternative base configurations of the same dwelling, not successive stages, so one of the two statements is wrong. Assess a {2} dwelling at {3}. Preparing it here would attribute a true {2} result to a scenario whose permanent identity asserts the {1} route's operating assumptions.",
                    partOIteration,
                    partOVentilationMode_Iteration,
                    result.VentilationMode,
                    result.VentilationMode == Enums.PartOVentilationMode.NaturalVentilation ? PartOIteration.BaseNaturalVentilation : PartOIteration.BasePassive);

                return result;
            }

            // ---- 3. The Approved Document F airflows ----------------------------------------------------

            AnalyticalModel analyticalModel_Applied;

            if (result.AirflowApplication == PartOPartFAirflowApplication.SkipNaturalVentilation)
            {
                //A copy, so the "not modified, a prepared copy is returned" contract holds on this path too
                //and a caller comparing the two objects sees the same thing either way. Nothing is written
                //to it: absence of PartFSpaceData is not an error here, because there are no continuous
                //mechanical rates to look for.
                analyticalModel_Applied = new AnalyticalModel(analyticalModel);

                result.Notes.Add(diagnostic_Airflow);
                result.Warnings.Add(diagnostic_Airflow);
            }
            else
            {
                //The join between the two documents: which Approved Document F condition this stage runs at.
                //Asked only on the MVHR route, because it is the only route with a continuous mechanical
                //rate to have a condition for. A stage with no settled condition refuses rather than being
                //simulated at whichever rate happens to be handy.
                PartFOperatingMode? partFOperatingMode = partOIteration.PartOIterationOperatingMode(out string refusal_OperatingMode);
                if (!partFOperatingMode.HasValue)
                {
                    result.Refusal = refusal_OperatingMode;

                    return result;
                }

                analyticalModel_Applied = analyticalModel.ApplyPartFVentilationRates(partFOperatingMode.Value, out List<string> refusals_Airflow, out List<string> notes_Airflow);

                result.Refusals.AddRange(refusals_Airflow);
                result.Notes.AddRange(notes_Airflow);

                if (analyticalModel_Applied == null)
                {
                    result.Refusal = "No Part F rates could be applied, so there is nothing to simulate. Run a Part F component first.";

                    return result;
                }

                // ---- 4. The Base MVHR design realization ------------------------------------------------

                //This is what makes the stage's own assertion true. BasePassive states "Mechanical
                //Ventilation At Design Rate = True" and that claim is inside the permanent scenario key -
                //but until now nothing made a simulation obey it. Applying the rates above put the
                //requirement onto each space's internal condition; only the chain below turns that
                //requirement into air the simulation actually moves.
                //
                //It runs HERE, on the MVHR branch, and nowhere else. A Part F calculation on its own
                //creates requirements and nothing more. The trigger is the Base MVHR operating scenario
                //asking for design-rate operation, never the presence of an airflow on a space.
                //
                //Null where the caller assessed the whole model - every test, the licensed acceptance run
                //and a Grasshopper zones_ left unconnected all pass null here, and PrepareBaseMVHR keeps
                //exactly its previous whole-model behaviour for them. A caller-named subset is carried
                //through instead of discarded: see PrepareBaseMVHR for why a subset cannot be ignored.
                analyticalModel_Applied = PrepareBaseMVHR(analyticalModel_Applied, zones == null ? null : zones_Assessed, ventilationUnitCapacityDescriptors, result);

                if (result.Refusal != null)
                {
                    return result;
                }
            }

            // ---- 4b. The isolated thermal model, where one was asked for ---------------------------------
            //
            // AFTER everything above, never before. Approved Document F requirements, design terminals,
            // system duties and the dwelling's internal transfer air are all settled on the whole building
            // first, and isolation then extracts the assessed dwellings carrying that state unchanged. So
            // isolation cannot move a Part F number: it is a thermal model scope, which is the one thing it
            // claims to be. (The dwelling's transfer air is solved over the DWELLING - a communal corridor
            // belongs to no dwelling - so removing the entrance door to an excluded corridor cannot change
            // it either, which is what makes re-preparing an isolated model in a later optimisation round
            // give the same answer as the first.)
            //
            // Placed before the steps below so that the openings report and the overheating scenarios both
            // describe the model that will actually be simulated, rather than a building around it.

            if (isolate)
            {
                analyticalModel_Applied = Isolate(analyticalModel_Applied, zones_Assessed, result);

                if (result.Refusal != null)
                {
                    return result;
                }
            }

            // ---- 5. The authored openings, reported ------------------------------------------------------

            //The join between the stage's "Openings Restricted" assumption and the opening behaviour the
            //model carries. The model is REPORTED ON, never rewritten to fit the stage.
            result.OpeningCompatibility = analyticalModel_Applied.PartOIterationOpeningCompatibility(partOIteration, out string summary_Openings, out List<string> evidence_Openings);

            if (evidence_Openings != null)
            {
                result.Notes.AddRange(evidence_Openings);
            }

            if (summary_Openings != null)
            {
                result.Notes.Add(summary_Openings);
                result.Warnings.Add(summary_Openings);
            }

            result.AnalyticalModel = analyticalModel_Applied;

            // ---- 6. The scenarios the results are attributed to ------------------------------------------

            //Stated over zone objects taken from the APPLIED model, so a zone guid on a scenario and a zone
            //guid in the model that will be simulated are the same identity.
            List<Zone> zones_Applied = analyticalModel_Applied.GetZones() ?? [];

            List<Zone> zones_Scenario = [];
            foreach (Zone zone in zones_Assessed)
            {
                zones_Scenario.Add(zones_Applied.Find(x => x != null && x.Guid == zone.Guid) ?? zone);
            }

            result.OverheatingScenarios.AddRange(Create.OverheatingScenarios(zones_Scenario, partOIteration, dictionary_VentilationStrategy, out List<string> refusals_Scenarios));

            result.Refusals.AddRange(refusals_Scenarios);

            return result;
        }

        /// <summary>
        /// Turns the applied Approved Document F requirement into a Base MVHR design the simulation can
        /// actually run: design terminals, one generic system and unit PER ASSESSED DWELLING, a derived
        /// duty checked against the requirement, and the directional air movements that carry it into TAS.
        /// <para>
        /// <b>Every step refuses rather than half-completing.</b> A model that reached the export with
        /// terminals but no system, or a system but no air movements, would simulate a dwelling whose
        /// mechanical ventilation is partly there - and would do it successfully, which is the failure mode
        /// this whole design exists to prevent.
        /// </para>
        /// <para>
        /// <b>One system per dwelling, never one shared system for several.</b>
        /// <c>PartFCalculator</c> sizes each dwelling zone independently - <c>Query.PartFDwellingZones</c>
        /// is its own dwelling-selection authority, asked here rather than restated - so combining two
        /// independent dwellings' spaces onto one generic AHU would route separate flats through shared
        /// plant that neither dwelling's Part F sizing describes, and every design terminal below is
        /// realized whole-model regardless. This method processes ONE dwelling zone at a time and builds
        /// (or reuses) that dwelling's own system, so the model's physical topology never implies plant no
        /// dwelling actually has.
        /// </para>
        /// <para>
        /// <b>Why an assessed subset has to be carried in, and what "the whole model" now means.</b>
        /// <c>Query.PartOVentilationMode(zones, ...)</c> settles the route from the ASSESSED zones alone -
        /// it has no way to know that a caller-named subset does not cover the whole model. A Grasshopper
        /// <c>zones_</c> input is explicitly built to take a subset ("leave unconnected to use every
        /// zone"), so a model can genuinely carry an assessed dwelling and an unassessed one side by side,
        /// and the unassessed one may state a different route or none at all - the mixed-route refusal
        /// never sees it. Terminal realization stays whole-model, because
        /// <c>Modify.ApplyPartFVentilationRates</c> already wrote every sized space and a terminal is inert
        /// until something connects it - but each dwelling's system and air handling unit must serve only
        /// that dwelling's spaces. An unconnected <c>zones_</c> - "assess the whole model" - is NOT "treat
        /// the whole model as one dwelling": it processes every dwelling zone the cluster carries, each
        /// getting its own system, exactly as a caller who named all of them explicitly would. The one
        /// genuine single-dwelling case is a model with NO zone structure at all, which is
        /// <c>PartFCalculator</c>'s own whole-model sizing mode - see <c>Query.PartFTransferAirSpaces</c>.
        /// </para>
        /// </summary>
        private static AnalyticalModel PrepareBaseMVHR(AnalyticalModel analyticalModel, List<Zone> zones_Assessed, IEnumerable<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors, PartOIterationPreparation result)
        {
            //ONE cluster instance, taken once and put back once. AnalyticalModel.AdjacencyCluster returns a
            //fresh copy on every read, so reading it twice would silently discard the first half of this.
            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;

            // ---- Design terminals --------------------------------------------------------------------

            //Every space, not only the assessed zones' spaces: Modify.ApplyPartFVentilationRates is a
            //whole-model write, so the sized spaces may be wider than the assessed ones. Realizing a subset
            //would leave the applied airflow of the rest with no terminal behind it. A design terminal by
            //itself moves no air and connects to nothing - it is the system connection below, not this
            //realization, that has to be scoped to keep an unassessed dwelling out of the assessed one's
            //simulation.
            List<VentilationTerminal> ventilationTerminals = adjacencyCluster.RealizePartFVentilationTerminals(null, out List<string> notes_Terminals, out List<string> refusals_Terminals);

            result.Notes.AddRange(notes_Terminals);

            if (refusals_Terminals.Count != 0)
            {
                result.Refusal = string.Join(" ", refusals_Terminals);

                return analyticalModel;
            }

            if (ventilationTerminals == null || ventilationTerminals.Count == 0)
            {
                result.Refusal = "The MVHR route was stated and the Approved Document F rates were applied, but no space carries a continuous requirement that could be realized as a design ventilation terminal, so there is no mechanical ventilation for this iteration to simulate. Run the Part F calculation before preparing the iteration.";

                return analyticalModel;
            }

            // ---- Partition the scope into one group per assessed dwelling zone -------------------------

            //Each entry is one dwelling zone's own spaces to serve; a null entry means "no zone structure at
            //all", PartFCalculator's own single-dwelling whole-model mode, and is the ONLY case that still
            //builds one system for everything. Every other case - including an unconnected zones_ - is
            //partitioned dwelling by dwelling, never merged.
            List<List<Space>> spaceGroups_Dwelling = DwellingSpaceGroups(adjacencyCluster, zones_Assessed, out List<string> notes_Partition);

            result.Notes.AddRange(notes_Partition);

            if (spaceGroups_Dwelling.Count == 0)
            {
                result.Refusal = "None of the assessed zones is a dwelling zone Approved Document F would size (Query.PartFDwellingZones), so there is no Base MVHR system to build. Assess a zone stated as a dwelling.";

                return analyticalModel;
            }

            double supplyDuty_Total = 0;
            double extractDuty_Total = 0;

            foreach (List<Space> spaces_Dwelling_Assessed in spaceGroups_Dwelling)
            {
                //The system, the unit, the derived duty, the runtime movements, the transfer air and the
                //balance of THIS dwelling - shared with Modify.MaterialisePartODwellingStrategies, which builds
                //the same design per MVHR dwelling of a mixed model. See RealizeBaseMVHRDwelling.
                string refusal_Dwelling = RealizeBaseMVHRDwelling(adjacencyCluster, analyticalModel.ProfileLibrary, spaces_Dwelling_Assessed, null, null, null, result.Notes, result.Warnings, out VentilationSystem ventilationSystem, out AirHandlingUnit airHandlingUnit, out double supplyDuty_Lps, out double extractDuty_Lps);

                if (refusal_Dwelling != null)
                {
                    result.Refusal = refusal_Dwelling;

                    return analyticalModel;
                }

                // ---- The unit this dwelling is fitted with ---------------------------------------------

                //Iteration 2, and only where a catalogue was offered. With none, the unit stays generic and
                //this dwelling's answer is exactly Iteration 1a's.
                //
                //THIS dwelling's own duty and no other's. The selection sits inside the per-dwelling loop
                //deliberately: aggregating the duties first and choosing one unit for the assessment would
                //size a block of flats as though the air moved between them, and letting one dwelling's
                //shortfall abandon the run would make the answer depend on which dwelling was processed
                //first. A dwelling nothing can serve is refused by name and the loop continues.
                //
                //It runs AFTER the network is realized and balanced, because the duty it selects against is
                //the duty of the network as built - not the one the terminals happened to carry before the
                //transfer air was routed.
                if (ventilationUnitCapacityDescriptors is not null)
                {
                    VentilationUnitSelection ventilationUnitSelection = adjacencyCluster.SelectVentilationUnit(airHandlingUnit, ventilationUnitCapacityDescriptors, out List<string> notes_Unit, out List<string> refusals_Unit);

                    result.Notes.AddRange(notes_Unit);
                    result.Refusals.AddRange(refusals_Unit);

                    if (ventilationUnitSelection.IsSelected)
                    {
                        result.VentilationUnitSelections.Add(ventilationUnitSelection);
                    }

                    //Re-resolved by guid, because the selection is written onto a replacement unit rather
                    //than onto this object - Modify.SelectVentilationUnit says why. Without this the
                    //preparation would report a unit that carries no selected product while the model it
                    //hands back carries one.
                    airHandlingUnit = adjacencyCluster.GetObject<AirHandlingUnit>(airHandlingUnit.Guid) ?? airHandlingUnit;
                }

                result.VentilationSystems.Add(ventilationSystem);
                result.AirHandlingUnits.Add(airHandlingUnit);
                supplyDuty_Total += supplyDuty_Lps;
                extractDuty_Total += extractDuty_Lps;
            }

            result.VentilationTerminals.AddRange(ventilationTerminals);
            result.VentilationSystem = result.VentilationSystems.Count != 0 ? result.VentilationSystems[0] : null;
            result.AirHandlingUnit = result.AirHandlingUnits.Count != 0 ? result.AirHandlingUnits[0] : null;
            result.DesignSupplyDuty_Lps = supplyDuty_Total;
            result.DesignExtractDuty_Lps = extractDuty_Total;

            return new AnalyticalModel(analyticalModel, adjacencyCluster);
        }

        /// <summary>
        /// Realises the Base MVHR design of ONE dwelling over a cluster whose Approved Document F rates and
        /// design terminals are already in place: the dwelling's own system and unit (created or reused), the
        /// ticV conflict gate, the derived duty checked against the requirement, the directional air movements,
        /// the internal transfer air, and the per-node balance.
        /// <para>
        /// The body of <see cref="PrepareBaseMVHR"/>'s per-dwelling loop, lifted out unchanged so that
        /// <c>Modify.MaterialisePartODwellingStrategies</c> builds each MVHR dwelling of a mixed model with
        /// exactly the design Iteration 1a builds, rather than a second implementation of it. The only
        /// additions are caller-stated and default to the legacy behaviour: the created system's id and unit's
        /// name (null keeps <c>Query.NextId</c> and <c>MVHR-NN</c>), and a gate on the unit (null states none).
        /// </para>
        /// </summary>
        /// <returns>Null where the dwelling was realised; the refusal otherwise.</returns>
        internal static string RealizeBaseMVHRDwelling(AdjacencyCluster adjacencyCluster, ProfileLibrary profileLibrary, List<Space> spaces_Dwelling_Assessed, string id_VentilationSystem, string name_AirHandlingUnit, Func<AirHandlingUnit, string> refuseAirHandlingUnit, List<string> notes, List<string> warnings, out VentilationSystem ventilationSystem, out AirHandlingUnit airHandlingUnit, out double supplyDuty_Lps, out double extractDuty_Lps)
        {
            ventilationSystem = null;
            airHandlingUnit = null;
            supplyDuty_Lps = 0;
            extractDuty_Lps = 0;

            // ---- The generic system and unit for THIS dwelling -------------------------------------

            //spaces_Dwelling_Assessed - null only for the genuine no-zone-structure whole-model case,
            //exactly the previous behaviour there; one dwelling zone's own spaces otherwise, so the
            //unit built or reused here serves only that dwelling.
            ventilationSystem = adjacencyCluster.AddPartOBaseMVHRSystem(spaces_Dwelling_Assessed, id_VentilationSystem, name_AirHandlingUnit, out airHandlingUnit, out List<string> notes_System, out List<string> warnings_System, out List<string> refusals_System);

            notes.AddRange(notes_System);
            notes.AddRange(warnings_System);
            warnings.AddRange(warnings_System);

            if (ventilationSystem == null || airHandlingUnit == null)
            {
                return refusals_System.Count != 0
                    ? string.Join(" ", refusals_System)
                    : "The Base MVHR ventilation system could not be established, so there is nothing to move the design airflow.";
            }

            //A caller-stated gate on the unit this dwelling will be served by, asked BEFORE anything is
            //realized from it. The legacy preparation states none. The mixed materialisation refuses a reused
            //authored unit that states a supply temperature, which would otherwise carry active cooling
            //behind the cooling gate (PR0 P12).
            string refusal_Unit = refuseAirHandlingUnit?.Invoke(airHandlingUnit);
            if (refusal_Unit != null)
            {
                return refusal_Unit;
            }

            // ---- The legacy Ventilation profile / ticV conflict, refused rather than double-counted --

            //Modify.UpdateInternalCondition (SAM_Tas) activates TBD's own mechanical ventilation (ticV)
            //from exactly this same test - InternalCondition.GetProfile(ProfileType.Ventilation, ...)
            //resolving to a non-null Profile - independent of the directional inter-zone air movements
            //this method is about to build. A served space arriving here already carrying a Ventilation
            //profile that resolves in this model's own library would therefore reach the simulation
            //through BOTH runtime representations at once: the profiled ticV air change rate, and this
            //design's supply/extract movements. Refusing here is the smallest safe correction - the
            //VentilationProfileName is READ, never cleared or rewritten, so a model arriving with one is
            //untouched and Preparation_PreservesEveryProfileNameOnEveryInternalCondition stays true. See
            //VENTILATION_TICV_ROUND_TRIP.md for the gate this mirrors.
            List<Space> spaces_Served = adjacencyCluster.GetRelatedObjects<Space>(ventilationSystem) ?? [];

            List<string> names_ConflictingProfile = [];
            foreach (Space space_Served in spaces_Served)
            {
                if (space_Served?.InternalCondition?.GetProfile(ProfileType.Ventilation, profileLibrary) != null)
                {
                    names_ConflictingProfile.Add(space_Served.Name);
                }
            }

            if (names_ConflictingProfile.Count != 0)
            {
                names_ConflictingProfile.Sort(StringComparer.Ordinal);

                return string.Format(
                    "Space(s) {0} already carry a Ventilation profile that resolves in this model's profile library, so SAM_Tas's Modify.UpdateInternalCondition would activate TBD's own mechanical ventilation (ticV) from it - independently of the directional inter-zone air movements ventilation system '{1}' is about to build for the same space(s). Simulating both would move the design airflow twice. Remove the Ventilation profile assignment from the affected space(s) (SAMAnalytical.UpdateVentilationProfile) before preparing this iteration, or decide which runtime representation should carry the requirement.",
                    string.Join(", ", names_ConflictingProfile),
                    ventilationSystem.FullName);
            }

            // ---- The derived duty, checked against the requirement ---------------------------------

            bool reconciled = adjacencyCluster.ReconcileVentilationSystemDesignDuty(ventilationSystem, out List<string> notes_Duty, out List<string> warnings_Duty, out List<string> refusals_Duty);

            notes.AddRange(notes_Duty);
            warnings.AddRange(warnings_Duty);

            if (!reconciled)
            {
                return string.Join(" ", refusals_Duty);
            }

            adjacencyCluster.VentilationSystemDesignDuty(ventilationSystem, out supplyDuty_Lps, out extractDuty_Lps);

            // ---- The runtime realization -------------------------------------------------------------

            //The dwelling the transfer air is routed over, settled ONCE per dwelling and used by
            //everything below for it.
            //
            //Wider than the system's served spaces on purpose. The ventilation system is related only
            //to the rooms carrying a design terminal, which is correct - it moves no air into an
            //internal hall - but that hall is the room the dwelling's transfer air crosses and divides
            //at, and a network solved without it reports a supplied bedroom and an extracted bathroom
            //as having no connection. Narrower than the model on purpose too: a communal corridor
            //belongs to no dwelling and must never become a shortcut between two of them, and - now that
            //each dwelling is its own system - nor may a DIFFERENT assessed dwelling's rooms. Since
            //spaces_Served is now scoped to this one dwelling, Query.PartFTransferAirSpaces expands it
            //only within that same dwelling's own zone.
            List<Space> spaces_Dwelling = adjacencyCluster.PartFTransferAirSpaces(spaces_Served, out List<string> notes_Scope);

            notes.AddRange(notes_Scope ?? []);

            //Stale movements first, whatever built them. Preparing the same model twice must produce the
            //same model, not two sets of air movements that a TBD would write as two sets of inter-zone
            //air movements - and a model arriving with its own system-template air movements on these
            //rooms would otherwise add its supply to this design's.
            RemoveBaseMVHRAirMovementObjects(adjacencyCluster, ventilationSystem, airHandlingUnit, spaces_Dwelling);

            //SCOPED to the system THIS dwelling built. The model's own ventilation systems, and every
            //other assessed dwelling's system, are left exactly as they are and are not realized here:
            //walking them too would ventilate their rooms a second time.
            List<IAirMovementObject> airMovementObjects = adjacencyCluster.AddAirMovementObjects(profileLibrary, ventilationSystem);

            if (airMovementObjects == null || airMovementObjects.Count == 0)
            {
                return string.Format("Ventilation system '{0}' was built with a design duty of {1:0.###} l/s supply and {2:0.###} l/s extract, but no air movement could be realized from it, so nothing would reach the simulation.", ventilationSystem.FullName, supplyDuty_Lps, extractDuty_Lps);
            }

            notes.Add(string.Format(
                "Realized {0} air movement object(s) for the Base MVHR design-rate operating state: supply into each space from '{1}' and extract from each space back to it, each direction sized from that space's own design terminals.",
                airMovementObjects.Count,
                airHandlingUnit.Name));

            // ---- The internal transfer air that closes each room -----------------------------------

            //A balanced heat recovery dwelling balances at the SYSTEM, so almost every room is
            //individually out of balance - and TAS refuses to simulate a zone whose inter-zone air
            //movements do not balance. The air that closes each room is transfer air, routed by the
            //Approved Document F airflow network over the model's own internal adjacencies. Nothing is
            //invented: where the network cannot route a room's net, this refuses rather than making a
            //route up.
            List<SpaceAirMovement> spaceAirMovements_Transfer = adjacencyCluster.AddPartFTransferAirMovements(profileLibrary, spaces_Dwelling, out List<string> notes_Transfer, out List<string> refusals_Transfer);

            notes.AddRange(notes_Transfer);

            if (spaceAirMovements_Transfer == null || refusals_Transfer.Count != 0)
            {
                return refusals_Transfer.Count != 0
                    ? string.Join(" ", refusals_Transfer)
                    : "The dwelling's internal transfer air could not be established, so its rooms would not balance and TAS would refuse to simulate the model.";
            }

            // ---- Conservation, checked at every node -----------------------------------------------

            //Checked over the DWELLING, not over the served spaces: a zero-terminal hall that passes air
            //on is a TAS zone carrying inter-zone air movements like any other, and one that gained more
            //than it passed on would be refused by TAS while every served room balanced perfectly.
            string refusal_Balance = RefuseUnbalancedAirMovement(adjacencyCluster, spaces_Dwelling, airHandlingUnit);

            if (refusal_Balance != null)
            {
                return refusal_Balance;
            }

            notes.Add(string.Format(
                "Every space and the air handling unit of '{0}' balance: each passes on exactly what it receives, which is what TAS requires of a zone carrying inter-zone air movements.",
                ventilationSystem.FullName));

            return null;
        }

        /// <summary>
        /// Partitions the scope <see cref="PrepareBaseMVHR"/> builds Base MVHR plant over into one group of
        /// spaces per assessed dwelling zone - the "one system per dwelling" rule, applied once, ahead of
        /// the loop that builds each system.
        /// <para>
        /// <b>The dwelling-selection authority is asked, never restated.</b> <c>Query.PartFDwellingZones</c>
        /// is exactly what <c>PartFCalculator</c> itself sizes with, so a zone this method treats as a
        /// dwelling is exactly a zone Part F sized as one, and the two cannot drift apart.
        /// </para>
        /// <para>
        /// <b>Null means every dwelling zone the cluster carries, never "the whole model is one
        /// dwelling".</b> Where <paramref name="zones_Assessed"/> is null - an unconnected Grasshopper
        /// <c>zones_</c>, "assess the whole model" - every dwelling zone the current cluster carries is
        /// processed, each as its own group. Where a subset was named, it is intersected with the
        /// dwelling zones, so an unassessed dwelling never contributes a group.
        /// </para>
        /// <para>
        /// <b>One real exception: no zone structure at all.</b> A model the cluster carries zero
        /// <see cref="Zone"/>s for is <c>PartFCalculator</c>'s own whole-model sizing mode - see
        /// <c>Query.PartFTransferAirSpaces</c> - and there genuinely is only one dwelling to build a system
        /// for. That case returns a single <b>null</b> entry, which <c>AddPartOBaseMVHRSystem</c> already
        /// reads as "every space in the cluster", exactly the previous unscoped behaviour.
        /// </para>
        /// </summary>
        private static List<List<Space>> DwellingSpaceGroups(AdjacencyCluster adjacencyCluster, List<Zone> zones_Assessed, out List<string> notes)
        {
            notes = [];

            List<Zone> zones_Cluster = adjacencyCluster.GetZones() ?? [];

            if (zones_Cluster.Count == 0)
            {
                notes.Add("The model carries no zones, so it was treated as a single dwelling - the same whole-model mode Approved Document F sizes a zone-less model at - and one Base MVHR system was built for every space.");

                return [null];
            }

            List<Zone> zones_Dwelling = zones_Cluster.PartFDwellingZones() ?? [];

            if (zones_Assessed != null)
            {
                HashSet<Guid> guids_Assessed = [];
                foreach (Zone zone in zones_Assessed)
                {
                    if (zone != null)
                    {
                        guids_Assessed.Add(zone.Guid);
                    }
                }

                zones_Dwelling = zones_Dwelling.FindAll(x => x != null && guids_Assessed.Contains(x.Guid));
            }

            zones_Dwelling = zones_Dwelling.FindAll(x => x != null);
            zones_Dwelling.Sort((x, y) =>
            {
                int comparison = string.CompareOrdinal(x.Name, y.Name);
                return comparison != 0 ? comparison : x.Guid.CompareTo(y.Guid);
            });

            List<List<Space>> result = [];

            foreach (Zone zone in zones_Dwelling)
            {
                HashSet<Guid> guids_Space = [];
                List<Space> spaces_Zone = [];

                foreach (Space space in adjacencyCluster.GetRelatedObjects<Space>(zone) ?? [])
                {
                    if (space != null && guids_Space.Add(space.Guid))
                    {
                        spaces_Zone.Add(space);
                    }
                }

                result.Add(spaces_Zone);
            }

            notes.Add(result.Count == 1
                ? string.Format("One assessed dwelling zone ('{0}') was found, so one Base MVHR system was built for it.", zones_Dwelling[0].Name)
                : string.Format("{0} assessed dwelling zone(s) were found ({1}), so a separate Base MVHR system was built for each - PartFCalculator sizes each dwelling independently, and this configuration does not combine them onto shared plant.", result.Count, string.Join(", ", zones_Dwelling.ConvertAll(x => x.Name))));

            return result;
        }

        /// <summary>
        /// Checks that every node the system's air movements touch passes on exactly what it receives, and
        /// states what is wrong where one does not.
        ///
        /// <para>
        /// <b>Summed per node, never matched per route.</b> These movements form a directed network: the
        /// unit feeds several rooms, a room may draw from several rooms and pass air on to several more, and
        /// flows split and recombine along the way - a bedroom of the acceptance dwelling divides its supply
        /// between three rooms, and one of its ensuites draws from two. No movement has a partner.
        /// Conservation, and TAS, ask only that the sums agree at each zone.
        /// </para>
        /// <para>
        /// This is a check, not a correction: nothing is rescaled to make it pass. A model that fails it is
        /// one TAS would refuse with <c>Simulation Failed</c>, and saying so here is the difference between
        /// a refusal an engineer can act on and a simulation that reports success having produced nothing.
        /// </para>
        /// </summary>
        /// <param name="adjacencyCluster">The model the air movements are read from.</param>
        /// <param name="spaces">
        /// Every space of the dwelling, terminal or no terminal - the same scope the transfer air was routed
        /// over. An intermediate space left out of this would be a zone TAS refused while this reported the
        /// model balanced.
        /// </param>
        /// <param name="airHandlingUnit">The unit, which is a node of the network like any room.</param>
        /// <returns><b>Null</b> where every node balances; the refusal otherwise.</returns>
        private static string RefuseUnbalancedAirMovement(AdjacencyCluster adjacencyCluster, List<Space> spaces, AirHandlingUnit airHandlingUnit)
        {
            List<SpaceAirMovement> spaceAirMovements = [];

            void Collect(Core.IJSAMObject jSAMObject)
            {
                foreach (SpaceAirMovement spaceAirMovement in adjacencyCluster.GetRelatedObjects<SpaceAirMovement>(jSAMObject) ?? [])
                {
                    if (spaceAirMovement != null && spaceAirMovements.Find(x => x.Guid == spaceAirMovement.Guid) == null)
                    {
                        spaceAirMovements.Add(spaceAirMovement);
                    }
                }
            }

            Collect(airHandlingUnit);

            foreach (Space space in spaces ?? [])
            {
                Collect(space);
            }

            Dictionary<Guid, double> dictionary_Residual = adjacencyCluster.AirMovementResidual(spaceAirMovements, [airHandlingUnit]);

            List<string> diagnostics = [];

            void Check(Core.SAMObject sAMObject)
            {
                if (sAMObject == null || !dictionary_Residual.TryGetValue(sAMObject.Guid, out double residual))
                {
                    return;
                }

                if (System.Math.Abs(residual) <= Query.AirMovementResidualTolerance)
                {
                    return;
                }

                diagnostics.Add(string.Format("{0} {1} {2:0.###} l/s", sAMObject.Name, residual > 0 ? "gains" : "loses", System.Math.Abs(residual) * 1000));
            }

            foreach (Space space in spaces ?? [])
            {
                Check(space);
            }

            Check(airHandlingUnit);

            if (diagnostics.Count == 0)
            {
                return null;
            }

            diagnostics.Sort(StringComparer.Ordinal);

            return string.Format("The air movements do not balance: {0}. TAS refuses to simulate a zone that gains air it never loses, so this model would fail rather than produce a result. The design terminal duties are authoritative and are not adjusted to close the difference - correct the design, or the internal adjacencies the transfer air is routed over.", string.Join(", ", diagnostics));
        }

        /// <summary>
        /// Removes the air movement objects belonging to this system and unit, so that re-preparing a model
        /// replaces them rather than adding a second set beside them.
        /// <para>
        /// Scoped to the system's own unit and to the dwelling it serves on purpose: an inter-zone air
        /// movement a modeller created by hand elsewhere in the building is not this iteration's to delete.
        /// </para>
        /// <para>
        /// The dwelling rather than the served spaces, because a transfer movement is related to the space it
        /// ARRIVES in - so the one arriving in a zero-terminal hall is related to that hall alone. Collecting
        /// only the served spaces would leave it behind, and re-preparing the model would write a second
        /// transfer beside it.
        /// </para>
        /// </summary>
        private static void RemoveBaseMVHRAirMovementObjects(AdjacencyCluster adjacencyCluster, VentilationSystem ventilationSystem, AirHandlingUnit airHandlingUnit, List<Space> spaces)
        {
            List<Core.SAMObject> sAMObjects = [];

            void Collect(Core.IJSAMObject jSAMObject)
            {
                foreach (SpaceAirMovement spaceAirMovement in adjacencyCluster.GetRelatedObjects<SpaceAirMovement>(jSAMObject) ?? [])
                {
                    if (spaceAirMovement != null && sAMObjects.Find(x => x.Guid == spaceAirMovement.Guid) == null)
                    {
                        sAMObjects.Add(spaceAirMovement);
                    }
                }
            }

            Collect(airHandlingUnit);

            foreach (Space space in spaces ?? adjacencyCluster.GetRelatedObjects<Space>(ventilationSystem) ?? [])
            {
                Collect(space);
            }

            foreach (AirHandlingUnitAirMovement airHandlingUnitAirMovement in adjacencyCluster.GetRelatedObjects<AirHandlingUnitAirMovement>(airHandlingUnit) ?? [])
            {
                if (airHandlingUnitAirMovement != null && sAMObjects.Find(x => x.Guid == airHandlingUnitAirMovement.Guid) == null)
                {
                    sAMObjects.Add(airHandlingUnitAirMovement);
                }
            }

            if (sAMObjects.Count != 0)
            {
                adjacencyCluster.Remove(sAMObjects);
            }
        }

        /// <summary>
        /// Replaces the prepared model with the isolated derived model of the assessed dwellings, and stamps
        /// what was isolated onto it - or sets the refusal and returns the model untouched.
        /// <para>
        /// The extraction itself is <see cref="IsolateSpaces(AdjacencyCluster, IEnumerable{Space})"/>. This
        /// only decides WHICH spaces (every space of every assessed dwelling zone, by identity) and records
        /// the answer so a reopened run can still say it was isolated.
        /// </para>
        /// </summary>
        private static AnalyticalModel Isolate(AnalyticalModel analyticalModel, List<Zone> zones_Assessed, PartOIterationPreparation result)
        {
            AdjacencyCluster adjacencyCluster = analyticalModel?.AdjacencyCluster;
            if (adjacencyCluster == null)
            {
                result.Refusal = "The prepared model carries no adjacency cluster, so it cannot be isolated.";

                return analyticalModel;
            }

            //Whole dwellings, never part of one: the scope is the assessed zones, and a dwelling simulated
            //without some of its own rooms would be a different dwelling.
            HashSet<Guid> guids_Space = [];
            List<Space> spaces = [];
            List<Guid> guids_Zone = [];
            List<string> names_Zone = [];

            foreach (Zone zone in zones_Assessed ?? [])
            {
                if (zone == null)
                {
                    continue;
                }

                guids_Zone.Add(zone.Guid);
                names_Zone.Add(zone.Name);

                foreach (Space space in adjacencyCluster.GetRelatedObjects<Space>(zone) ?? [])
                {
                    if (space != null && guids_Space.Add(space.Guid))
                    {
                        spaces.Add(space);
                    }
                }
            }

            if (spaces.Count == 0)
            {
                result.Refusal = "No space belongs to the dwellings in scope, so there is nothing to simulate in isolation.";

                return analyticalModel;
            }

            SpaceIsolation spaceIsolation = adjacencyCluster.IsolateSpaces(spaces);

            //Refused BEFORE the model is handed on, so nothing converts or simulates a scope that was never
            //valid. The reasons are the isolation's own and name what is shared.
            if (!spaceIsolation.IsIsolated)
            {
                result.Refusal = string.Join("\n", spaceIsolation.Refusals);

                return analyticalModel;
            }

            AnalyticalModel result_Model = new(analyticalModel, spaceIsolation.AdjacencyCluster);

            //Stamped on the model, so it travels into the run's .sam and a later session reviewing those
            //results can still tell they came from an isolated thermal model rather than a whole building.
            result_Model.SetValue(AnalyticalModelParameter.PartOIsolationContext, new PartOIsolationContext(guids_Space, guids_Zone, names_Zone));

            result.Notes.AddRange(spaceIsolation.Notes);

            return result_Model;
        }
    }
}
