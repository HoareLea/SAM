// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Core;
using SAM.Weather;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// The TM59 assessment recipe lifted out of the <c>Tas.TSDQueryTM59Results</c> Grasshopper component.
    /// <para>
    /// <b>These tests are the reason the extraction was worth doing.</b> The recipe used to live inside a
    /// <c>SolveInstance</c>, interleaved with parameter plumbing, in an assembly whose tests need a licensed
    /// TAS install - so none of it could be exercised. Only the TSD read needs TAS; restoring internal
    /// conditions, choosing spaces, calculating and splitting do not, and all of that runs here under plain
    /// <c>dotnet test</c>.
    /// </para>
    /// <para>
    /// Fixture is the validation shape: three flats each with a "Bedroom 2", plus a corridor - which is also
    /// what makes the preserved name matching visible rather than theoretical.
    /// </para>
    /// </summary>
    public class TM59AssessmentCalculatorTests
    {
        //The TAS spelling. The TSD converter writes "Occupant"; the analytical vocabulary says "Occupancy".
        private const string key_Tas_OccupantSensibleGain = "Occupant Sensible Gain";

        /// <summary>
        /// The design internal conditions are copied onto the simulated spaces, which is what lets TM59
        /// choose a criterion at all - a model read back from a simulation carries results but no design
        /// intent.
        /// </summary>
        [Fact]
        public void DesignInternalConditions_AreRestoredOntoTheSimulatedSpaces()
        {
            AnalyticalModel analyticalModel_Simulation = Model_Simulation();
            AnalyticalModel analyticalModel_Design = Model_Design();

            TM59AssessmentCalculator tM59AssessmentCalculator = Calculator(analyticalModel_Simulation, analyticalModel_Design);

            //Nothing to begin with - the simulated model is a rebuild.
            foreach (Space space in tM59AssessmentCalculator.AnalyticalModel.GetSpaces())
            {
                Assert.Null(space.InternalCondition);
            }

            Assert.True(tM59AssessmentCalculator.RestoreDesignInternalConditions());

            foreach (Space space in tM59AssessmentCalculator.AnalyticalModel.GetSpaces())
            {
                Assert.NotNull(space.InternalCondition);
                Assert.Equal(space.Name, space.InternalCondition.Name);
            }

            //And it does not throw or wipe anything when there is nothing to restore from.
            Assert.False(new TM59AssessmentCalculator(Model_Simulation(), null, new SimulationSpaceMap(null, null, null)).RestoreDesignInternalConditions());
        }

        /// <summary>
        /// <b>The supplied simulation model is not the assessment's scratchpad.</b>
        /// <para>
        /// The cluster getter hands out a shallow copy that shares <c>Space</c> instances with the supplied
        /// model, so restoring the design condition in place would write it back into the caller's raw
        /// simulation model. The restore works on copies: the calculator's own model carries the restored
        /// conditions, and the supplied model keeps the spaces exactly as it handed them over.
        /// </para>
        /// </summary>
        [Fact]
        public void RestoreDesignInternalConditions_DoesNotMutateTheSuppliedModel()
        {
            AnalyticalModel analyticalModel_Simulation = Model_Simulation();
            AnalyticalModel analyticalModel_Design = Model_Design();

            TM59AssessmentCalculator tM59AssessmentCalculator = Calculator(analyticalModel_Simulation, analyticalModel_Design);

            Assert.True(tM59AssessmentCalculator.RestoreDesignInternalConditions());

            //The assessment's own model carries the restored conditions...
            foreach (Space space in tM59AssessmentCalculator.AnalyticalModel.GetSpaces())
            {
                Assert.NotNull(space.InternalCondition);
                Assert.Equal(space.Name, space.InternalCondition.Name);
            }

            //...while the supplied raw simulation model is untouched.
            foreach (Space space in analyticalModel_Simulation.GetSpaces())
            {
                Assert.Null(space.InternalCondition);
            }
        }

        /// <summary>
        /// <b>Explicit scope classifies by the RESTORED condition, not the stale listed instance.</b>
        /// <para>
        /// <c>Spaces</c> resolves a requested design space to the simulation-space instance the
        /// <c>SimulationSpaceMap</c> retained - which predates <c>RestoreDesignInternalConditions</c> and
        /// carries no internal condition. Classifying THAT instance would fall through to the space name:
        /// here a design condition that is plainly not a bedroom ("2 Bed Apt. Living Room" - the qualifier
        /// names the apartment, not the room) sits on a space whose name says "Bedroom 2", so the stale path
        /// manufactures a bedroom result. The criterion must come from the model's own space, carrying the
        /// restored design condition.
        /// </para>
        /// </summary>
        [Fact]
        public void ExplicitlyScopedAssessment_ClassifiesByTheRestoredCondition()
        {
            AnalyticalModel analyticalModel_Simulation = Model_Simulation();
            AnalyticalModel analyticalModel_Design = Model_Design();

            //A living-room condition on a space whose NAME says sleeping. Written through the cluster so the
            //design MODEL carries it - AnalyticalModel.GetSpaces() hands out clones, and mutating one of
            //those would leave the model itself unchanged.
            AdjacencyCluster adjacencyCluster_Design = analyticalModel_Design.AdjacencyCluster;
            Space space_Design = adjacencyCluster_Design.GetSpaces().Find(x => x.Name == "Flat 1 Bedroom 2");
            space_Design.InternalCondition = new InternalCondition("2 Bed Apt. Living Room");
            analyticalModel_Design = new AnalyticalModel(analyticalModel_Design, adjacencyCluster_Design);

            TM59AssessmentCalculator tM59AssessmentCalculator = Calculator(analyticalModel_Simulation, analyticalModel_Design);

            Assert.True(tM59AssessmentCalculator.RestoreDesignInternalConditions());

            //Control: the restored model carries the design condition on the model's own space instance.
            Space space_Restored = tM59AssessmentCalculator.AnalyticalModel.GetSpaces().Find(x => x.Name == "Flat 1 Bedroom 2");
            Assert.NotNull(space_Restored.InternalCondition);
            Assert.Equal("2 Bed Apt. Living Room", space_Restored.InternalCondition.Name);

            List<Space> spaces_Assessed = tM59AssessmentCalculator.Spaces([space_Design], null);
            Assert.Single(spaces_Assessed);
            Assert.Null(spaces_Assessed[0].InternalCondition);

            TM59AssessmentResult tM59AssessmentResult = tM59AssessmentCalculator.Calculate(spaces_Assessed, true);

            TMResult tMResult = Assert.Single(tM59AssessmentResult.NaturalVentilationResults);

            Assert.True(tMResult is TM59NaturalVentilationExtendedResult);
            Assert.False(tMResult is TM59NaturalVentilationBedroomExtendedResult, "The stale space's name would have classified this as a bedroom; the restored living-room condition must decide.");
        }

        /// <summary>
        /// <b>No spaces and no zones means the whole model.</b> That is the component's behaviour, preserved -
        /// and it is exactly why the real TAS run exported a communal corridor into a domestic overheating
        /// assessment as an ordinary room. Pinned, not endorsed.
        /// </summary>
        [Fact]
        public void NoSpacesAndNoZones_AssessesTheWholeModel()
        {
            TM59AssessmentCalculator tM59AssessmentCalculator = Calculator(Model_Simulation());

            List<Space> spaces = tM59AssessmentCalculator.Spaces(null, null);

            Assert.Equal(4, spaces.Count);
            Assert.Contains("Corridor", spaces.ConvertAll(x => x.Name));
        }

        /// <summary>
        /// <b>Requested spaces are DESIGN objects resolved by identity.</b> A real design space resolves to the
        /// simulated space it produced; one the design model does not hold is refused with a reason.
        /// <para>
        /// <b>And a fabricated space with the right NAME no longer resolves</b>, which is the behaviour change.
        /// Before step 8 a caller could conjure <c>new Space("Flat 1 Bedroom 2")</c> and be handed a result; now
        /// only the actual design object counts, because in a block of flats a name identifies nothing.
        /// </para>
        /// </summary>
        [Fact]
        public void RequestedSpaces_AreResolvedByIdentityAndNotByName()
        {
            AnalyticalModel analyticalModel_Design = Model_Design();

            TM59AssessmentCalculator tM59AssessmentCalculator = Calculator(Model_Simulation(), analyticalModel_Design);

            Space space_Design = analyticalModel_Design.GetSpaces().Find(x => x.Name == "Flat 1 Bedroom 2");

            List<Space> spaces = tM59AssessmentCalculator.Spaces([space_Design], null);

            Assert.Equal(["Flat 1 Bedroom 2"], spaces.ConvertAll(x => x.Name));
            Assert.Empty(tM59AssessmentCalculator.AssociationRefusals);

            //A fabricated space carrying the same name is refused, not resolved.
            Assert.Empty(tM59AssessmentCalculator.Spaces([new Space("Flat 1 Bedroom 2")], null));
            Assert.Single(tM59AssessmentCalculator.AssociationRefusals);
            Assert.Contains("does not resolve to exactly one simulated space", tM59AssessmentCalculator.AssociationRefusals[0]);

            //As is one that names nothing in the model at all.
            Assert.Empty(tM59AssessmentCalculator.Spaces([new Space("Nowhere")], null));
            Assert.Single(tM59AssessmentCalculator.AssociationRefusals);
        }

        /// <summary>
        /// <b>A zone contributes its spaces through the DESIGN model's relations</b>, and a space already
        /// selected is not added twice - de-duplicated by <c>Guid</c>, not by name.
        /// <para>
        /// The design model is what says which rooms make up Flat 1. The simulated model's zones are a rebuild
        /// with fresh guids that no scenario and no design zone can name, so they are not consulted.
        /// </para>
        /// </summary>
        [Fact]
        public void Zones_ContributeTheirSpacesByIdentityWithoutDuplicating()
        {
            AnalyticalModel analyticalModel_Design = Model_Design();

            TM59AssessmentCalculator tM59AssessmentCalculator = Calculator(Model_Simulation(), analyticalModel_Design);

            Zone zone_Design = analyticalModel_Design.GetZones().Find(x => x.Name == "Flat 1");

            //A zone-only request is a zone scope, not "the whole model" merely because spaces is null.
            Assert.Equal(["Flat 1 Bedroom 2"], tM59AssessmentCalculator.Spaces(null, [zone_Design]).ConvertAll(x => x.Name));

            //Asked for the zone alone, only its spaces come back.
            Assert.Equal(["Flat 1 Bedroom 2"], tM59AssessmentCalculator.Spaces([], [zone_Design]).ConvertAll(x => x.Name));

            //A fabricated zone with a real zone's name is refused rather than resolved by that name.
            Assert.Empty(tM59AssessmentCalculator.Spaces([], [new Zone("Flat 1")]));
            Assert.Contains("is not in the design model", tM59AssessmentCalculator.AssociationRefusals[0]);

            //And a zone that names nothing contributes nothing rather than throwing.
            Assert.Empty(tM59AssessmentCalculator.Spaces([], [new Zone("Flat 9")]));
            Assert.Single(tM59AssessmentCalculator.AssociationRefusals);
        }

        /// <summary>
        /// The whole recipe end to end, with the TAS series key the TSD converter actually writes: results
        /// come back split by criterion, and the comfort limit series come with them.
        /// </summary>
        [Fact]
        public void Calculate_SplitsResultsByCriterionAndReturnsTheComfortLimits()
        {
            TM59AssessmentCalculator tM59AssessmentCalculator = Calculator(Model_Simulation());

            Assert.True(tM59AssessmentCalculator.RestoreDesignInternalConditions());

            TM59AssessmentResult tM59AssessmentResult = tM59AssessmentCalculator.Calculate(tM59AssessmentCalculator.Spaces(null, null));

            Assert.NotNull(tM59AssessmentResult);
            Assert.Equal(4, tM59AssessmentResult.Spaces.Count);

            //Every result landed in exactly one of the three criterion lists.
            int count = tM59AssessmentResult.MechanicalVentilationResults.Count + tM59AssessmentResult.NaturalVentilationResults.Count + tM59AssessmentResult.CorridorResults.Count;

            Assert.Equal(4, count);

            //A whole year of comfort limits, as the component asks for.
            Assert.NotNull(tM59AssessmentResult.MaxIndoorComfortTemperatures);
            Assert.NotNull(tM59AssessmentResult.MinIndoorComfortTemperatures);
            Assert.NotEmpty(tM59AssessmentResult.MaxIndoorComfortTemperatures.Values);
        }

        /// <summary>
        /// <c>extended</c> chooses between the full result and the simplified one, and nothing else changes -
        /// the same spaces are assessed and land in the same criterion lists either way.
        /// </summary>
        [Fact]
        public void Extended_ChoosesTheResultFormAndNothingElse()
        {
            TM59AssessmentCalculator tM59AssessmentCalculator = Calculator(Model_Simulation());
            tM59AssessmentCalculator.RestoreDesignInternalConditions();

            List<Space> spaces = tM59AssessmentCalculator.Spaces(null, null);

            TM59AssessmentResult tM59AssessmentResult_Simple = tM59AssessmentCalculator.Calculate(spaces, false);
            TM59AssessmentResult tM59AssessmentResult_Extended = tM59AssessmentCalculator.Calculate(spaces, true);

            Assert.Equal(tM59AssessmentResult_Simple.MechanicalVentilationResults.Count, tM59AssessmentResult_Extended.MechanicalVentilationResults.Count);
            Assert.Equal(tM59AssessmentResult_Simple.NaturalVentilationResults.Count, tM59AssessmentResult_Extended.NaturalVentilationResults.Count);
            Assert.Equal(tM59AssessmentResult_Simple.CorridorResults.Count, tM59AssessmentResult_Extended.CorridorResults.Count);

            //The extended form really is the extended type; the default really is simplified.
            foreach (TMResult tMResult in tM59AssessmentResult_Extended.NaturalVentilationResults)
            {
                Assert.IsAssignableFrom<TM59ExtendedResult>(tMResult);
            }

            foreach (TMResult tMResult in tM59AssessmentResult_Simple.NaturalVentilationResults)
            {
                Assert.NotNull(tMResult);
                Assert.False(tMResult is TM59ExtendedResult, "The default result form must be the simplified one.");
            }
        }

        /// <summary>
        /// <b>The series key is instance state, and the extraction did not reconcile the two spellings.</b>
        /// The TSD converter writes "Occupant Sensible Gain"; the analytical default says "Occupancy". Reading
        /// the wrong one produces no assessment at all - silently, which is pinned here rather than endorsed.
        /// </summary>
        [Fact]
        public void TheWrongSeriesKey_ProducesNoAssessment()
        {
            //Left at the analytical default, against a model written by the TAS converter.
            AnalyticalModel analyticalModel_Simulation = Model_Simulation();
            AnalyticalModel analyticalModel_Design = Model_Design();

            TM59AssessmentCalculator tM59AssessmentCalculator = new(analyticalModel_Simulation, analyticalModel_Design, new SimulationSpaceMap(analyticalModel_Design.GetSpaces(), analyticalModel_Simulation.GetSpaces(), null));

            Assert.Equal(Core.Query.Name(SpaceSimulationResultParameter.OccupancySensibleGain), tM59AssessmentCalculator.OccupancySensibleGainSeriesKey);

            tM59AssessmentCalculator.RestoreDesignInternalConditions();

            TM59AssessmentResult tM59AssessmentResult = tM59AssessmentCalculator.Calculate(tM59AssessmentCalculator.Spaces(null, null));

            //Not an error, not a warning - just nothing, for all four spaces.
            Assert.Empty(tM59AssessmentResult.MechanicalVentilationResults);
            Assert.Empty(tM59AssessmentResult.NaturalVentilationResults);
            Assert.Empty(tM59AssessmentResult.CorridorResults);

            //And with the key the converter actually wrote, the same model does assess.
            TM59AssessmentCalculator tM59AssessmentCalculator_Tas = Calculator(Model_Simulation());
            tM59AssessmentCalculator_Tas.RestoreDesignInternalConditions();

            TM59AssessmentResult tM59AssessmentResult_Tas = tM59AssessmentCalculator_Tas.Calculate(tM59AssessmentCalculator_Tas.Spaces(null, null));

            Assert.NotEqual(0, tM59AssessmentResult_Tas.MechanicalVentilationResults.Count + tM59AssessmentResult_Tas.NaturalVentilationResults.Count + tM59AssessmentResult_Tas.CorridorResults.Count);
        }

        /// <summary>
        /// A null model or null spaces produce no result rather than an exception.
        /// </summary>
        [Fact]
        public void NothingToAssess_ProducesNoResult()
        {
            Assert.Null(new TM59AssessmentCalculator(null, null, null).Calculate([]));
            Assert.Null(new TM59AssessmentCalculator(null, null, null).Spaces(null, null));
            Assert.Null(Calculator(Model_Simulation()).Calculate(null));
            Assert.False(new TM59AssessmentCalculator(null, Model_Design(), new SimulationSpaceMap(null, null, null)).RestoreDesignInternalConditions());
        }

        /// <summary>
        /// <b>The equivalence test, and the one that makes this an extraction rather than a rewrite.</b> The
        /// sequence inlined below is <c>Tas.TSDQueryTM59Results</c>'s own, verbatim from the
        /// <c>SolveInstance</c> it had before the component was repointed at this service, minus the parameter
        /// plumbing - and its output is compared with the service's. Everything else here says the service
        /// behaves sensibly; this says it behaves <i>the same</i>.
        /// <para>
        /// <b>It did not stop mattering when the component was repointed.</b> The component now calls the
        /// service, so the two agree by construction and the component can no longer disagree with itself.
        /// What this pins is the thing that is still falsifiable: that the service continues to do what the
        /// component <i>used</i> to do. Delete it and the last statement of the original behaviour goes with
        /// it.
        /// </para>
        /// <para>
        /// The inlined copy is deliberately not factored - it is a transcript, and tidying it would defeat
        /// the point.
        /// </para>
        /// </summary>
        [Fact]
        public void TheService_MatchesTheComponentsOwnSequence()
        {
            //---- the service ----
            TM59AssessmentCalculator tM59AssessmentCalculator = Calculator(Model_Simulation());
            tM59AssessmentCalculator.RestoreDesignInternalConditions();

            TM59AssessmentResult tM59AssessmentResult = tM59AssessmentCalculator.Calculate(tM59AssessmentCalculator.Spaces(null, null));

            //---- the component, inlined ----
            AnalyticalModel analyticalModel_TSD = Model_Simulation();
            AnalyticalModel analyticalModel = Model_Design();

            AdjacencyCluster adjacencyCluster_TSD = analyticalModel_TSD?.AdjacencyCluster;
            if (adjacencyCluster_TSD != null)
            {
                List<Space> spaces_AnalyticalModel = analyticalModel?.GetSpaces();
                if (spaces_AnalyticalModel != null)
                {
                    List<Space> spaces_TSD = adjacencyCluster_TSD.GetSpaces();
                    if (spaces_TSD != null)
                    {
                        foreach (Space space_TSD in spaces_TSD)
                        {
                            Space space_AnalyticalModel = spaces_AnalyticalModel.Find(x => x.Name == space_TSD.Name);
                            if (space_AnalyticalModel != null)
                            {
                                space_TSD.InternalCondition = space_AnalyticalModel.InternalCondition;
                                adjacencyCluster_TSD.AddObject(space_TSD);
                            }
                        }

                        analyticalModel_TSD = new AnalyticalModel(analyticalModel_TSD, adjacencyCluster_TSD);
                    }
                }
            }

            TMOverheatingCalculator tMOverheatingCalculator = new(analyticalModel_TSD)
            {
                TM52BuildingCategory = TM52BuildingCategory.CategoryII,
                OccupancySensibleGainSeriesKey = key_Tas_OccupantSensibleGain
            };

            List<Space> spaces_Result = analyticalModel_TSD.GetSpaces();

            List<TM59ExtendedResult> tM59ExtendedResults = tMOverheatingCalculator.Calculate_TM59(spaces_Result);

            List<TMResult> tM59MechanicalVentilationResults = tM59ExtendedResults.FindAll(x => x is TM59MechanicalVentilationExtendedResult)?.ConvertAll(x => (TMResult)x);
            List<TMResult> tM59NaturalVentilationResults = tM59ExtendedResults.FindAll(x => x is TM59NaturalVentilationExtendedResult)?.ConvertAll(x => (TMResult)x);
            List<TMResult> tM59CorridorResults = tM59ExtendedResults.FindAll(x => x is TM59CorridorExtendedResult)?.ConvertAll(x => (TMResult)x);

            tM59MechanicalVentilationResults = tM59MechanicalVentilationResults?.ConvertAll(x => (x as TM59ExtendedResult)?.Simplify());
            tM59NaturalVentilationResults = tM59NaturalVentilationResults.ConvertAll(x => (x as TM59ExtendedResult)?.Simplify());
            tM59CorridorResults = tM59CorridorResults?.ConvertAll(x => (x as TM59ExtendedResult)?.Simplify());

            IndexedDoubles maxIndoorComfortTemperatures = tMOverheatingCalculator.GetMaxIndoorComfortTemperatures(0, 364);
            IndexedDoubles minIndoorComfortTemperatures = tMOverheatingCalculator.GetMinIndoorComfortTemperatures(0, 364);

            //---- and they agree ----
            Assert.Equal(spaces_Result.ConvertAll(x => x.Name), tM59AssessmentResult.Spaces.ConvertAll(x => x.Name));

            Assert.Equal(Names(tM59MechanicalVentilationResults), Names(tM59AssessmentResult.MechanicalVentilationResults));
            Assert.Equal(Names(tM59NaturalVentilationResults), Names(tM59AssessmentResult.NaturalVentilationResults));
            Assert.Equal(Names(tM59CorridorResults), Names(tM59AssessmentResult.CorridorResults));

            Assert.Equal(maxIndoorComfortTemperatures.Values, tM59AssessmentResult.MaxIndoorComfortTemperatures.Values);
            Assert.Equal(minIndoorComfortTemperatures.Values, tM59AssessmentResult.MinIndoorComfortTemperatures.Values);

            //Not vacuous: the fixture really does produce results to compare.
            Assert.NotEmpty(tM59ExtendedResults);
        }

        /// <summary>
        /// <b>End to end, through the real engine.</b> A naturally ventilated apartment "Living Room" (no
        /// bedroom in its name) must not be routed as a bedroom just because its InternalCondition's name
        /// happens to contain the token "Bed" or "Room" - see the fixed "room"/"bedroom" substring collision
        /// in <c>TM59MatcherTests</c>/<c>TM59SpaceApplicationClassificationTests</c>. Confirmed here at the
        /// level that actually decides which result TYPE a space becomes:
        /// <c>TM59NaturalVentilationExtendedResult</c>, not the bedroom subtype - which is what makes
        /// Criterion 2 read N/A once the result reaches the report.
        /// </summary>
        [Fact]
        public void ApartmentLivingRoomCondition_IsNotRoutedAsABedroom_SoCriterion2IsNotApplicable()
        {
            AdjacencyCluster adjacencyCluster = new();

            Space space = Space("Living Room");
            space.InternalCondition = new InternalCondition("2 Bed Apt. Living Room");

            Zone zone = new("Flat 1");

            adjacencyCluster.AddObject(space);
            adjacencyCluster.AddObject(zone);
            adjacencyCluster.AddRelation(zone, space);

            AnalyticalModel analyticalModel = new("Test", null, null, null, adjacencyCluster);
            analyticalModel.SetValue(AnalyticalModelParameter.WeatherData, new WeatherData("Test", "Test", 51.5, -0.1, 0, WeatherYear()));

            TMOverheatingCalculator tMOverheatingCalculator = new(analyticalModel)
            {
                TM52BuildingCategory = TM52BuildingCategory.CategoryII,
                OccupancySensibleGainSeriesKey = key_Tas_OccupantSensibleGain,
            };

            List<TM59ExtendedResult> tM59ExtendedResults = tMOverheatingCalculator.Calculate_TM59([space]);

            TM59ExtendedResult tM59ExtendedResult = Assert.Single(tM59ExtendedResults);
            Assert.IsType<TM59NaturalVentilationExtendedResult>(tM59ExtendedResult);

            TM59AssessmentReport tM59AssessmentReport = new(null, null, [tM59ExtendedResult], null, null, "test");

            TM59AssessmentReportCheck check_Criterion2 = tM59AssessmentReport.NaturalVentilationChecks.Find(x => x.Check == TM59AssessmentReport.Check_Criterion2);

            Assert.NotNull(check_Criterion2);
            Assert.Equal(TM59ComplianceStatus.NotApplicable, check_Criterion2.ComplianceStatus);

            //Criterion 1 is still applicable - the space is simply not a bedroom, not unassessed.
            TM59AssessmentReportCheck check_Criterion1 = tM59AssessmentReport.NaturalVentilationChecks.Find(x => x.Check == TM59AssessmentReport.Check_Criterion1);
            Assert.NotEqual(TM59ComplianceStatus.NotApplicable, check_Criterion1.ComplianceStatus);
        }

        // ---------------------------------------------------------------------------------------------
        // Fixture - the three-flat validation shape, TAS-free
        // ---------------------------------------------------------------------------------------------

        private static List<string> Names(List<TMResult> tMResults)
        {
            return tMResults.ConvertAll(x => x?.Name);
        }

        /// <summary>
        /// A calculator over a simulated model and a design model, tied together by a
        /// <c>SimulationSpaceMap</c>.
        /// <para>
        /// <b>A null key function on purpose.</b> This fixture's spaces have distinct names, so unique-name
        /// matching resolves every one of them - which is exactly the condition under which the name matching
        /// this class used to do was correct. That is what keeps the equivalence transcript below meaningful.
        /// The three-flat fixture where names REPEAT is in <c>PartOResultAssociationTests</c>, and it is the one
        /// that needs a real identity.
        /// </para>
        /// </summary>
        private static TM59AssessmentCalculator Calculator(AnalyticalModel analyticalModel, AnalyticalModel analyticalModel_Design = null)
        {
            analyticalModel_Design ??= Model_Design();

            SimulationSpaceMap simulationSpaceMap = new(analyticalModel_Design?.GetSpaces(), analyticalModel?.GetSpaces(), null);

            return new TM59AssessmentCalculator(analyticalModel, analyticalModel_Design, simulationSpaceMap)
            {
                //What the TAS wrapper supplies, because the TSD converter writes this spelling.
                OccupancySensibleGainSeriesKey = key_Tas_OccupantSensibleGain
            };
        }

        /// <summary>
        /// A model as read back from a TSD: fresh spaces carrying hourly series and no internal conditions,
        /// grouped into zones. Three flats each with a "Bedroom 2" - so the preserved name matching is
        /// exercised on the shape that makes it dangerous - plus a corridor.
        /// </summary>
        private static AnalyticalModel Model_Simulation()
        {
            AdjacencyCluster adjacencyCluster = new();

            foreach (string name in new[] { "Flat 1", "Flat 2", "Flat 3" })
            {
                Space space = Space(name + " Bedroom 2");

                Zone zone = new(name);

                adjacencyCluster.AddObject(space);
                adjacencyCluster.AddObject(zone);
                adjacencyCluster.AddRelation(zone, space);
            }

            Space space_Corridor = Space("Corridor");
            Zone zone_Corridor = new("Corridor");

            adjacencyCluster.AddObject(space_Corridor);
            adjacencyCluster.AddObject(zone_Corridor);
            adjacencyCluster.AddRelation(zone_Corridor, space_Corridor);

            AnalyticalModel result = new("Three Flats", null, null, null, adjacencyCluster);

            result.SetValue(AnalyticalModelParameter.WeatherData, new WeatherData("Test", "Test", 51.5, -0.1, 0, WeatherYear()));

            return result;
        }

        /// <summary>
        /// The design model - the same space names, carrying the internal conditions, <b>and zoned</b>, because a
        /// requested zone is now resolved through the design model's own relations rather than the simulated
        /// model's rebuilt ones.
        /// </summary>
        private static AnalyticalModel Model_Design()
        {
            AdjacencyCluster adjacencyCluster = new();

            foreach (string name in new[] { "Flat 1", "Flat 2", "Flat 3", "Corridor" })
            {
                string name_Space = name == "Corridor" ? "Corridor" : name + " Bedroom 2";

                Space space = new(name_Space) { InternalCondition = new InternalCondition(name_Space) };
                Zone zone = new(name);

                adjacencyCluster.AddObject(space);
                adjacencyCluster.AddObject(zone);
                adjacencyCluster.AddRelation(zone, space);
            }

            return new AnalyticalModel("Three Flats", null, null, null, adjacencyCluster);
        }

        /// <summary>
        /// One space carrying a short run of hourly values, stored exactly as the TSD converter stores them:
        /// a <c>JsonArray</c> in a <c>ParameterSet</c> added to the space.
        /// </summary>
        private static Space Space(string name)
        {
            Space result = new(name);

            ParameterSet parameterSet = new("SAM.Analytical.Tas.dll");

            parameterSet.Add(Core.Query.Name(SpaceSimulationResultParameter.ResultantTemperature), Values([21.0, 24.5, 27.5, 29.0]));
            parameterSet.Add(key_Tas_OccupantSensibleGain, Values([0, 80.0, 80.0, 0]));

            result.Add(parameterSet);

            return result;
        }

        /// <summary>
        /// A full year of flat 20 C dry-bulb hours - the comfort band is a running mean of these, so the year
        /// has to be populated or the running mean throws.
        /// </summary>
        private static WeatherYear WeatherYear()
        {
            WeatherYear result = new(2018);

            for (int day = 0; day < 365; day++)
            {
                for (int hour = 0; hour < 24; hour++)
                {
                    result.Add(day, hour, new Dictionary<string, double> { { WeatherDataType.DryBulbTemperature.ToString(), 20.0 } });
                }
            }

            return result;
        }

        private static JsonArray Values(IEnumerable<double> values)
        {
            JsonArray result = [];

            foreach (double value in values)
            {
                result.Add(value);
            }

            return result;
        }
    }
}
