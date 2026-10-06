// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Analytical.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.Json.Nodes;
using Xunit;
using Xunit.Abstractions;

namespace SAM.Tests
{
    /// <summary>
    /// Mixed Part O dwelling strategies, PR1: the persisted per-dwelling authority and
    /// <c>Modify.MaterialisePartODwellingStrategies</c>. These are the PR0 proof tests
    /// (<c>documentation/PartO-MixedDwellingStrategies-PR0.md</c>, P0-P12) promoted to production acceptance and
    /// inverted to the target behaviour: an NV dwelling stays clean, the design is order-independent and
    /// reproducible with equal names, products stay per dwelling, a retained design is kept or refused, a
    /// materialised or result-bearing baseline is refused, and a reused conditioned unit refuses.
    /// <para>
    /// The fixture is PR0's: Flats 1 and 2 from <c>PartOIterationPreparationTests.ModelWithTwoAssessedDwellings</c>
    /// (by reflection, so the two cannot drift), an authored Flat 3, and a communal corridor zone
    /// (<c>IsDwelling = false</c>) adjacent to Flats 1 and 2, assigned the TM59 communal corridor condition.
    /// State is compared by a GUID-insensitive signature labelled by dwelling.
    /// </para>
    /// </summary>
    public partial class PartODwellingStrategyMaterialisationTests
    {
        private const string Flat1 = "Flat 1";
        private const string Flat2 = "Flat 2";
        private const string Flat3 = "Flat 3";
        private const string Corridor = "Corridor";

        private static readonly VentilationUnitCapacityDescriptor Small = new(new VentilationUnitReference("Proof", "Small", null), 500, 500);
        private static readonly VentilationUnitCapacityDescriptor Large = new(new VentilationUnitReference("Proof", "Large", null), 1000, 1000);

        private readonly ITestOutputHelper output;

        public PartODwellingStrategyMaterialisationTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        // =================================================================================================
        // Baseline (D1)
        // =================================================================================================

        [Fact]
        public void CleanBaseline_IsAccepted_AndMaterialises()
        {
            AnalyticalModel baseline = WithStrategies(Baseline(), Mvhr(Flat1), Natural(Flat2), Mvhr(Flat3));

            Assert.True(baseline.IsPartOCleanBaseline(out List<PartOMaterialisationRefusal> findings), string.Join("\n", findings));

            PartOMaterialisation materialisation = Materialise(baseline);

            Assert.NotNull(materialisation.Record);
            Assert.True(materialisation.AnalyticalModel.HasValue(AnalyticalModelParameter.PartOMaterialisationRecord));

            //The baseline itself is not modified.
            Assert.True(baseline.IsPartOCleanBaseline(out _));
            Assert.Empty(baseline.AdjacencyCluster.GetObjects<VentilationSystem>() ?? []);
        }

        [Fact]
        public void LegacyPreparedModel_IsRefused_AsMaterialised()
        {
            AnalyticalModel baseline = Baseline();

            AnalyticalModel prepared = baseline.PreparePartOIteration(PartOIteration.BasePassive, Zones(baseline, Flat1), Words(baseline, "MVHR", Flat1)).AnalyticalModel;
            Assert.NotNull(prepared);

            prepared = WithStrategies(prepared, Mvhr(Flat1), Natural(Flat2), Mvhr(Flat3));

            PartOMaterialisation materialisation = prepared.MaterialisePartODwellingStrategies();

            Assert.Null(materialisation.AnalyticalModel);
            Assert.Contains(materialisation.Refusals, x => x.Reason == PartOMaterialisationRefusalReason.MaterialisedBaseline && x.Message.Contains("Part O MVHR system"));
            Assert.Contains(materialisation.Refusals, x => x.Reason == PartOMaterialisationRefusalReason.MaterialisedBaseline && x.Message.Contains("ApplyPartFVentilationRates"));
        }

        [Fact]
        public void MaterialisedOutput_IsNotABaseline()
        {
            AnalyticalModel baseline = WithStrategies(Baseline(), Natural(Flat1), Natural(Flat2), Natural(Flat3));

            //Even an all-natural output, which carries no mechanical object at all, carries its record.
            AnalyticalModel materialised = Materialise(baseline).AnalyticalModel;

            PartOMaterialisation again = materialised.MaterialisePartODwellingStrategies();

            Assert.Null(again.AnalyticalModel);
            Assert.Contains(again.Refusals, x => x.Reason == PartOMaterialisationRefusalReason.MaterialisedBaseline && x.Message.Contains("materialisation record"));
        }

        [Fact]
        public void ModelWithScenarios_IsRefused_AsRunOutput()
        {
            AnalyticalModel baseline = WithStrategies(Baseline(), Natural(Flat1), Natural(Flat2), Natural(Flat3));
            baseline.SetValue(AnalyticalModelParameter.OverheatingScenarios, new Core.SAMCollection<OverheatingScenario>([new OverheatingScenario(PartOAssessmentScope.Dwelling, Zone(baseline, Flat1).Guid, PartOIteration.BaseNaturalVentilation)]));

            AssertRefused(baseline, PartOMaterialisationRefusalReason.RunOutputBaseline, "overheating scenarios");
        }

        [Fact]
        public void ModelWithProvenance_IsRefused_AsRunOutput()
        {
            AnalyticalModel baseline = WithStrategies(Baseline(), Natural(Flat1), Natural(Flat2), Natural(Flat3));
            baseline.SetValue(AnalyticalModelParameter.SimulationResultProvenance, new SimulationResultProvenance(baseline, null));

            AssertRefused(baseline, PartOMaterialisationRefusalReason.RunOutputBaseline, "provenance");
        }

        [Theory]
        [InlineData("space")]
        [InlineData("model")]
        public void ModelWithSimulationResults_IsRefused_AsRunOutput(string kind)
        {
            //A simulated all-natural model carries no MVHR object, air movement or Part F rate - only results.
            //A legacy or imported model holding just an AnalyticalModelSimulationResult is refused too.
            AnalyticalModel baseline = WithStrategies(Baseline(), Natural(Flat1), Natural(Flat2), Natural(Flat3));

            AdjacencyCluster adjacencyCluster = baseline.AdjacencyCluster;
            Space space = adjacencyCluster.GetSpaces().First();
            Core.Result result = kind == "space" ? new SpaceSimulationResult(space.Name, "Tas", space.Guid.ToString()) : new AnalyticalModelSimulationResult("Run", "Tas", baseline.Guid.ToString());
            adjacencyCluster.AddObject(result);
            if (kind == "space")
            {
                adjacencyCluster.AddRelation(space, result);
            }

            AssertRefused(new AnalyticalModel(baseline, adjacencyCluster), PartOMaterialisationRefusalReason.RunOutputBaseline, "simulation result");
        }

        [Fact]
        public void DesignDay_ModelLevelIsAuthoredInput_ClusterRecordsAreRunOutput()
        {
            //The PR0 design gate. The model's own Heating/Cooling Design Days are design inputs: accepted.
            AnalyticalModel baseline = WithStrategies(Baseline(), Natural(Flat1), Natural(Flat2), Natural(Flat3));
            baseline.SetValue(AnalyticalModelParameter.HeatingDesignDays, new Core.SAMCollection<DesignDay>([new DesignDay("Authored HTG", 2018, 1, 15)]));
            baseline.SetValue(AnalyticalModelParameter.CoolingDesignDays, new Core.SAMCollection<DesignDay>([new DesignDay("Authored CLG", 2018, 7, 15)]));

            Assert.True(baseline.IsPartOCleanBaseline(out List<PartOMaterialisationRefusal> findings), string.Join("\n", findings));
            Assert.True(baseline.MaterialisePartODwellingStrategies().IsMaterialised);

            //The TAS workflow writes design-day records INTO THE CLUSTER after a run (ReplaceDesignDays): refused.
            AdjacencyCluster adjacencyCluster = baseline.AdjacencyCluster;
            adjacencyCluster.AddObject(new DesignDay(new DesignDay("London ANN CLG", 2018, 7, 1), LoadType.Cooling));

            AssertRefused(new AnalyticalModel(baseline, adjacencyCluster), PartOMaterialisationRefusalReason.RunOutputBaseline, "design-day record");
        }

        // =================================================================================================
        // Authored air movements (an engineering rule, not a contamination detector)
        // =================================================================================================

        [Fact]
        public void AuthoredTransferMovement_IntoANaturalDwelling_IsCarriedThroughUnchanged()
        {
            //An engineer-authored inter-zone air movement (SAMAnalytical.CreateIZAMBySpaces, space -> space): the
            //corridor passes 5 l/s into Flat 2's bedroom. It is design data, so the baseline is still clean.
            AnalyticalModel baseline = WithAuthoredMovement(Baseline(), Corridor, "Flat 2 Bedroom", 5.0, out Guid guid_Movement);
            Assert.True(baseline.IsPartOCleanBaseline(out List<PartOMaterialisationRefusal> findings), string.Join("\n", findings));

            PartOMaterialisation materialisation = Materialise(WithStrategies(baseline, Mvhr(Flat1), Natural(Flat2), Mvhr(Flat3)));

            SpaceAirMovement spaceAirMovement = materialisation.AnalyticalModel.AdjacencyCluster.GetObject<SpaceAirMovement>(guid_Movement);
            Assert.NotNull(spaceAirMovement);
            Assert.Equal(0.005, spaceAirMovement.AirFlow, 9);
            Assert.Contains(materialisation.Warnings, x => x.Contains("carried through unchanged") && x.Contains("Flat 2"));

            //Flat 2 is otherwise exactly its baseline.
            Assert.Equal(Signature(baseline, Flat2), Signature(materialisation.AnalyticalModel, Flat2));
        }

        [Fact]
        public void AuthoredMovement_IntoAnMvhrDwelling_IsRefused_BecauseTheRebuildWouldDeleteIt()
        {
            //The same kind of movement into Flat 1's living room. The MVHR realisation removes every movement related
            //to the dwelling's rooms before rebuilding its network, so the authored one would be lost.
            AnalyticalModel baseline = WithAuthoredMovement(Baseline(), Corridor, "Living Room", 5.0, out _);
            Assert.True(baseline.IsPartOCleanBaseline(out _));

            AssertRefused(WithStrategies(baseline, Mvhr(Flat1), Natural(Flat2), Natural(Flat3)), PartOMaterialisationRefusalReason.AuthoredAirMovementConflict, Flat1);

            //With Flat 1 natural instead, the same baseline materialises and keeps the movement.
            Assert.True(WithStrategies(baseline, Natural(Flat1), Natural(Flat2), Natural(Flat3)).MaterialisePartODwellingStrategies().IsMaterialised);
        }

        [Fact]
        public void AuthoredExtractToOutside_FromANaturalDwelling_IsMechanicalDuty_AndRefused()
        {
            //A prescribed 8 l/s extract from Flat 2's bathroom to outside is continuous mechanical extract.
            AnalyticalModel baseline = WithAuthoredMovement(Baseline(), "Flat 2 Bathroom", null, 8.0, out _);
            Assert.True(baseline.IsPartOCleanBaseline(out _));

            AssertRefused(WithStrategies(baseline, Mvhr(Flat1), Natural(Flat2), Natural(Flat3)), PartOMaterialisationRefusalReason.NaturalOverMechanicalDuty, Flat2);
        }

        [Fact]
        public void AuthoredMovement_OnlyInAnUnassessedDwelling_IsCarriedThrough()
        {
            AnalyticalModel baseline = WithAuthoredMovement(Baseline(), "Flat 3 Bedroom", "Flat 3 Bathroom", 3.0, out Guid guid_Movement);

            PartOMaterialisation materialisation = Materialise(WithStrategies(baseline, Mvhr(Flat1), Natural(Flat2)), null, Flat1, Flat2);

            Assert.NotNull(materialisation.AnalyticalModel.AdjacencyCluster.GetObject<SpaceAirMovement>(guid_Movement));
        }

        [Fact]
        public void AuthoredUnitPlantZoneMovement_OnAUnitServingAnMvhrDwelling_IsRefused()
        {
            //A CreateIZAMBySetPoint-style plant-zone condition on Flat 1's authored unit: the materialisation rebuilds
            //the unit's movement from the unit, so the authored conditions would be deleted.
            AnalyticalModel baseline = WithAuthoredUnit(Baseline(), double.NaN, out string name_Unit);
            AdjacencyCluster adjacencyCluster = baseline.AdjacencyCluster;
            AirHandlingUnit airHandlingUnit = adjacencyCluster.GetObjects<AirHandlingUnit>().Single(x => x.Name == name_Unit);
            AirHandlingUnitAirMovement airHandlingUnitAirMovement = new(name_Unit, new Profile(name_Unit + " Heating", ProfileType.Heating, [16.0]), new Profile(name_Unit + " Cooling", ProfileType.Cooling, [24.0]), null, null, null);
            adjacencyCluster.AddObject(airHandlingUnitAirMovement);
            adjacencyCluster.AddRelation(airHandlingUnitAirMovement, airHandlingUnit);
            baseline = new AnalyticalModel(baseline, adjacencyCluster);
            Assert.True(baseline.IsPartOCleanBaseline(out _));

            AssertRefused(WithStrategies(baseline, Mvhr(Flat1), Natural(Flat2), Natural(Flat3)), PartOMaterialisationRefusalReason.AuthoredAirMovementConflict, name_Unit);
        }

        [Fact]
        public void AirMovement_WithAnEndpointTheModelDoesNotContain_IsRefusedAsBaseline()
        {
            //TAS resolves endpoints by object reference and would send this air to outside instead of to the room.
            AnalyticalModel baseline = Baseline();
            AdjacencyCluster adjacencyCluster = baseline.AdjacencyCluster;
            Space living = adjacencyCluster.GetSpaces().Find(x => x.Name == "Living Room");
            Space missing = new("Deleted Room");
            SpaceAirMovement spaceAirMovement = new("Living to deleted", 0.004, new Core.ObjectReference(living).ToString(), new Core.ObjectReference(missing).ToString());
            adjacencyCluster.AddObject(spaceAirMovement);
            adjacencyCluster.AddRelation(spaceAirMovement, living);
            baseline = new AnalyticalModel(baseline, adjacencyCluster);

            Assert.False(baseline.IsPartOCleanBaseline(out List<PartOMaterialisationRefusal> findings));
            Assert.Contains(findings, x => x.Reason == PartOMaterialisationRefusalReason.UnresolvedAirMovement && x.Message.Contains("Living to deleted"));
        }

        // =================================================================================================
        // Isolation (P1, P11 inverted)
        // =================================================================================================

        [Fact]
        public void MvhrDwelling_LeavesNaturalAndUnassessedDwellingsExactlyAsTheBaselineHasThem()
        {
            AnalyticalModel baseline = Baseline();

            //An authored supply basis on the natural Flat 2 - P11 showed the whole-model call zeroing it.
            AdjacencyCluster adjacencyCluster_Authored = baseline.AdjacencyCluster;
            Space bedroom_2 = adjacencyCluster_Authored.GetSpaces().Find(x => x.Name == "Flat 2 Bedroom");
            InternalCondition internalCondition_Authored = new(bedroom_2.InternalCondition);
            internalCondition_Authored.SetValue(InternalConditionParameter.SupplyAirFlowPerArea, 0.0015);
            adjacencyCluster_Authored.AddObject(new Space(bedroom_2) { InternalCondition = internalCondition_Authored });
            baseline = new AnalyticalModel(baseline, adjacencyCluster_Authored);

            //Flat 3 is unassessed: outside the scope, no strategy.
            baseline = WithStrategies(baseline, Mvhr(Flat1), Natural(Flat2));
            AnalyticalModel model = Materialise(baseline, null, Flat1, Flat2).AnalyticalModel;

            //Flat 1 is MVHR.
            Assert.Single(SystemsServing(model, Flat1));
            Assert.NotEmpty(AirMovementsTouching(model, Flat1));

            foreach (string name in new[] { Flat2, Flat3 })
            {
                //Nothing of Flat 1's design reached them - the exact P1 defect.
                Assert.Equal(Signature(baseline, name), Signature(model, name));
                Assert.Empty(Terminals(model, name));
                Assert.Empty(SystemsServing(model, name));
                Assert.Empty(AirMovementsTouching(model, name));

                foreach (Space space in Spaces(model, name))
                {
                    Space space_Baseline = baseline.AdjacencyCluster.GetObject<Space>(space.Guid);
                    Assert.Equal(space_Baseline.InternalCondition.Guid, space.InternalCondition.Guid);
                    Assert.Equal(space_Baseline.InternalCondition.Name, space.InternalCondition.Name);
                    Assert.False(space.InternalCondition.HasValue(InternalConditionParameter.SupplyAirFlow));
                }
            }

            //The authored basis survives (P11 inverted).
            InternalCondition internalCondition_After = model.AdjacencyCluster.GetSpaces().Find(x => x.Name == "Flat 2 Bedroom").InternalCondition;
            Assert.True(internalCondition_After.TryGetValue(InternalConditionParameter.SupplyAirFlowPerArea, out double perArea));
            Assert.Equal(0.0015, perArea, 9);

            //Only the assessed zones are scenarioed.
            PartOMaterialisation materialisation = Materialise(baseline, null, Flat1, Flat2);
            Assert.DoesNotContain(materialisation.OverheatingScenarios, x => x.ZoneGuid == Zone(model, Flat3).Guid);
        }

        [Fact]
        public void TwoMvhrDwellings_AreIndependent()
        {
            AnalyticalModel model_MM = Materialise(WithStrategies(Baseline(), Mvhr(Flat1), Natural(Flat2), Mvhr(Flat3))).AnalyticalModel;
            AnalyticalModel model_MN = Materialise(WithStrategies(Baseline(), Mvhr(Flat1), Natural(Flat2), Natural(Flat3))).AnalyticalModel;

            //Flat 1's design does not depend on whether Flat 3 is also MVHR (P2/P5 isolation).
            Assert.Equal(Signature(model_MN, Flat1), Signature(model_MM, Flat1));
            Assert.NotEqual(UnitOf(model_MM, Flat1).Guid, UnitOf(model_MM, Flat3).Guid);
            Assert.Equal(2, model_MM.AdjacencyCluster.GetObjects<AirHandlingUnit>().Count);
        }

        [Fact]
        public void DifferentProducts_StayPerDwelling_AndAnAutomaticNeighbourNeverReselectsAManualOne()
        {
            List<VentilationUnitCapacityDescriptor> catalogue = [Small, Large];

            //Flat 1 manual Large, Flat 3 automatic (smallest capable, Small) - in ONE call (P3 inverted).
            AnalyticalModel model = Materialise(WithStrategies(Baseline(), Mvhr(Flat1, Large.VentilationUnitReference), Natural(Flat2), Mvhr(Flat3)), catalogue).AnalyticalModel;

            Assert.Equal("Large", UnitOf(model, Flat1).SelectedVentilationUnitReference()?.Model);
            Assert.Equal("Small", UnitOf(model, Flat3).SelectedVentilationUnitReference()?.Model);

            //Changing Flat 3's product changes nothing about Flat 1.
            AnalyticalModel model_2 = Materialise(WithStrategies(Baseline(), Mvhr(Flat1, Large.VentilationUnitReference), Natural(Flat2), Mvhr(Flat3, Large.VentilationUnitReference)), catalogue).AnalyticalModel;

            Assert.Equal(Signature(model, Flat1), Signature(model_2, Flat1));
            Assert.Equal("Large", UnitOf(model_2, Flat3).SelectedVentilationUnitReference()?.Model);
        }

        // =================================================================================================
        // Natural ventilation (P4 inverted)
        // =================================================================================================

        [Fact]
        public void NaturalDwelling_OnTheSameBaseline_CarriesNoMechanicalState()
        {
            //The same baseline materialised with Flat 1 MVHR and then with Flat 1 natural: the natural one
            //carries nothing, because it is rebuilt from the baseline, never from the MVHR model (P4 inverted).
            AnalyticalModel baseline = Baseline();

            Assert.NotEmpty(SystemsServing(Materialise(WithStrategies(baseline, Mvhr(Flat1), Natural(Flat2), Natural(Flat3))).AnalyticalModel, Flat1));

            PartOMaterialisation materialisation = Materialise(WithStrategies(baseline, Natural(Flat1), Natural(Flat2), Natural(Flat3)));
            AnalyticalModel model = materialisation.AnalyticalModel;

            Assert.Equal(Signature(baseline, Flat1), Signature(model, Flat1));
            Assert.Empty(SystemsServing(model, Flat1));
            Assert.Empty(model.AdjacencyCluster.GetObjects<AirHandlingUnit>() ?? []);
            Assert.Empty(model.AdjacencyCluster.GetObjects<SpaceAirMovement>() ?? []);
            Assert.Empty(Terminals(model, Flat1));

            OverheatingScenario overheatingScenario = materialisation.OverheatingScenarios.Single(x => x.ZoneGuid == Zone(model, Flat1).Guid);
            Assert.Equal(PartOIteration.BaseNaturalVentilation, overheatingScenario.Iteration);
            Assert.Equal("NV", overheatingScenario.VentilationStrategy);
        }

        [Fact]
        public void Natural_WithRetainedDesign_WithCooling_OrWithAProduct_IsRefused()
        {
            AssertRefused(WithStrategies(Baseline(), new PartODwellingStrategy(Guid.Empty, PartOVentilationMode.NaturalVentilation, null, PartOActiveCooling.None, PartODesignAirFlowBasis.RetainedDesign, "x").For(Flat1), Natural(Flat2), Natural(Flat3)), PartOMaterialisationRefusalReason.NaturalWithRetainedDesign, Flat1);
            AssertRefused(WithStrategies(Baseline(), new PartODwellingStrategy(Guid.Empty, PartOVentilationMode.NaturalVentilation, null, PartOActiveCooling.SupplyAirCooling).For(Flat1), Natural(Flat2), Natural(Flat3)), PartOMaterialisationRefusalReason.NaturalWithCooling, Flat1);
            AssertRefused(WithStrategies(Baseline(), new PartODwellingStrategy(Guid.Empty, PartOVentilationMode.NaturalVentilation, Small.VentilationUnitReference).For(Flat1), Natural(Flat2), Natural(Flat3)), PartOMaterialisationRefusalReason.NaturalWithVentilationUnit, Flat1);
        }

        [Fact]
        public void Natural_OverAuthoredMechanicalDuty_IsRefused_AndTheSystemIsNamed()
        {
            AnalyticalModel baseline = Baseline();
            AdjacencyCluster adjacencyCluster = baseline.AdjacencyCluster;

            //An authored dwelling-local mechanical system with a positive design terminal in Flat 2.
            Space bedroom_2 = adjacencyCluster.GetSpaces().Find(x => x.Name == "Flat 2 Bedroom");
            VentilationTerminal ventilationTerminal = new("Authored supply", FlowClassification.Supply, 8.0);
            adjacencyCluster.AddObject(ventilationTerminal);
            adjacencyCluster.AddRelation(ventilationTerminal, bedroom_2);
            VentilationSystem ventilationSystem = new("Authored MEV", new VentilationSystemType("MV", "Authored mechanical ventilation"));
            adjacencyCluster.AddObject(ventilationSystem);
            adjacencyCluster.AddRelation(ventilationSystem, bedroom_2);
            adjacencyCluster.AddRelation(ventilationSystem, ventilationTerminal);

            AssertRefused(WithStrategies(new AnalyticalModel(baseline, adjacencyCluster), Mvhr(Flat1), Natural(Flat2), Natural(Flat3)), PartOMaterialisationRefusalReason.NaturalOverMechanicalDuty, "Authored MEV");
        }

        [Fact]
        public void Natural_WithBaselineDesignTerminals_CopiesThemThroughUnconnectedAndReportsThem()
        {
            AnalyticalModel baseline = Baseline();
            AdjacencyCluster adjacencyCluster = baseline.AdjacencyCluster;
            adjacencyCluster.RealizePartFVentilationTerminals(Spaces(baseline, Flat2), out _, out List<string> refusals);
            Assert.Empty(refusals);
            baseline = WithStrategies(new AnalyticalModel(baseline, adjacencyCluster), Mvhr(Flat1), Natural(Flat2), Natural(Flat3));
            Assert.True(baseline.IsPartOCleanBaseline(out _));

            PartOMaterialisation materialisation = Materialise(baseline);

            List<VentilationTerminal> terminals = Terminals(materialisation.AnalyticalModel, Flat2);
            Assert.Equal(2, terminals.Count);
            Assert.All(terminals, x => Assert.Empty(materialisation.AnalyticalModel.AdjacencyCluster.GetRelatedObjects<VentilationSystem>(x) ?? []));
            Assert.Contains(materialisation.Warnings, x => x.Contains("Flat 2") && x.Contains("unconnected and inert"));
            Assert.Equal(Signature(baseline, Flat2), Signature(materialisation.AnalyticalModel, Flat2));
        }

        // =================================================================================================
        // Mechanical
        // =================================================================================================

        [Fact]
        public void MvhrDwelling_GetsItsOwnPartFRates_Terminals_System_AndDwellingDerivedNames()
        {
            PartOMaterialisation materialisation = Materialise(WithStrategies(Baseline(), Mvhr(Flat1), Natural(Flat2), Mvhr(Flat3)));
            AnalyticalModel model = materialisation.AnalyticalModel;

            //Part F rates and terminals on the MVHR dwellings' sized spaces, and only there.
            foreach (string name in new[] { Flat1, Flat3 })
            {
                foreach (Space space in Spaces(model, name).Where(x => x.HasValue(SpaceParameter.PartFSpaceData)))
                {
                    PartFSpaceData partFSpaceData = space.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData);
                    Assert.True(space.InternalCondition.TryGetValue(InternalConditionParameter.SupplyAirFlow, out double supply));
                    Assert.Equal((partFSpaceData.ContinuousSupplyFlowRate_Lps ?? 0) / 1000.0, supply, 9);
                }

                Assert.NotEmpty(Terminals(model, name));
            }

            Assert.Empty(Terminals(model, Flat2));

            //Identity: one Part O MVHR system per dwelling, its unit named after the dwelling - never MVHR-01.
            Assert.Equal("MVHR Flat 1", UnitOf(model, Flat1).Name);
            Assert.Equal("MVHR Flat 3", UnitOf(model, Flat3).Name);

            VentilationSystem ventilationSystem = SystemsServing(model, Flat1).Single();
            Assert.Equal("MVHR", ventilationSystem.Type.Name);
            Assert.Equal(ventilationSystem.Guid, materialisation.Record.VentilationSystemGuids[Zone(model, Flat1).Guid]);
            Assert.Equal(2, materialisation.Record.VentilationSystemGuids.Count);

            //The unit movement's profile names follow the dwelling, not call order.
            AirHandlingUnitAirMovement airHandlingUnitAirMovement = model.AdjacencyCluster.GetRelatedObjects<AirHandlingUnitAirMovement>(UnitOf(model, Flat1)).Single();
            Assert.StartsWith("MVHR Flat 1", airHandlingUnitAirMovement.Humidification?.Name ?? "MVHR Flat 1");
            Assert.Null(airHandlingUnitAirMovement.Cooling);

            OverheatingScenario overheatingScenario = materialisation.OverheatingScenarios.Single(x => x.ZoneGuid == Zone(model, Flat1).Guid);
            Assert.Equal(PartOIteration.BasePassive, overheatingScenario.Iteration);
            Assert.Equal("MVHR", overheatingScenario.VentilationStrategy);
        }

        [Fact]
        public void Product_Unresolved_NotAllowed_OrInsufficient_IsRefused()
        {
            List<VentilationUnitCapacityDescriptor> catalogue = [Small, Large];

            //Not in the catalogue - and no catalogue at all.
            VentilationUnitReference unknown = new("Proof", "Unknown", null);
            AssertRefused(WithStrategies(Baseline(), Mvhr(Flat1, unknown), Natural(Flat2), Natural(Flat3)), PartOMaterialisationRefusalReason.VentilationUnitUnresolved, "Unknown", catalogue);
            AssertRefused(WithStrategies(Baseline(), Mvhr(Flat1, Small.VentilationUnitReference), Natural(Flat2), Natural(Flat3)), PartOMaterialisationRefusalReason.VentilationUnitUnresolved, "Small");

            //Not permitted by the project's pool (project policy lives at project level, D3).
            AnalyticalModel baseline_Pool = WithStrategies(Baseline(), Mvhr(Flat1, Large.VentilationUnitReference), Natural(Flat2), Natural(Flat3));
            baseline_Pool.SetValue(AnalyticalModelParameter.PartOEquipmentSelection, new PartOEquipmentSelection(PartOEquipmentSelectionMode.AutomaticSelectedPool, [Small.VentilationUnitReference]));
            AssertRefused(baseline_Pool, PartOMaterialisationRefusalReason.VentilationUnitNotAllowed, "Large", catalogue);

            //Cannot serve the duty.
            VentilationUnitCapacityDescriptor tiny = new(new VentilationUnitReference("Proof", "Tiny", null), 1, 1);
            AssertRefused(WithStrategies(Baseline(), Mvhr(Flat1, tiny.VentilationUnitReference), Natural(Flat2), Natural(Flat3)), PartOMaterialisationRefusalReason.VentilationUnitSelection, Flat1, [tiny]);
        }

        [Fact]
        public void UnbalancedBaselineDesign_IsRefused_NotRescaled()
        {
            //A single raised terminal on the baseline, accepted as retained: it does not balance (P5).
            AnalyticalModel baseline = Baseline();
            AdjacencyCluster adjacencyCluster = baseline.AdjacencyCluster;
            adjacencyCluster.RealizePartFVentilationTerminals(Spaces(baseline, Flat1), out _, out _);
            Space bedroom = adjacencyCluster.GetSpaces().Find(x => x.Name == "Bedroom 1");
            adjacencyCluster.SetSpaceDesignFlowRate(bedroom, FlowClassification.Supply, SpaceDesignFlow(adjacencyCluster, bedroom, FlowClassification.Supply) + 2.0, out _, out List<string> refusals);
            Assert.Empty(refusals);
            baseline = new AnalyticalModel(baseline, adjacencyCluster);

            string fingerprint = baseline.AdjacencyCluster.PartODwellingDesignFingerprint(Zone(baseline, Flat1));
            baseline = WithStrategies(baseline, Retained(Flat1, fingerprint), Natural(Flat2), Natural(Flat3));

            PartOMaterialisation materialisation = baseline.MaterialisePartODwellingStrategies();
            output.WriteLine(materialisation.Refusal);
            Assert.Null(materialisation.AnalyticalModel);
            Assert.Contains(materialisation.Refusals, x => x.Reason == PartOMaterialisationRefusalReason.MechanicalDesign && x.Message.Contains("do not balance"));
        }

        [Fact]
        public void ManuallySelectedUndersizedProduct_IsAStructuredRefusal_NeverAMaterialisation()
        {
            //Owner decision (PR1 review): a manual product that cannot meet the dwelling's design duty refuses.
            VentilationUnitCapacityDescriptor tiny = new(new VentilationUnitReference("Proof", "Tiny", null), 1, 1);

            PartOMaterialisation materialisation = WithStrategies(Baseline(), Mvhr(Flat1, tiny.VentilationUnitReference), Natural(Flat2), Natural(Flat3)).MaterialisePartODwellingStrategies([tiny, Large]);

            Assert.Null(materialisation.AnalyticalModel);
            Assert.False(materialisation.IsMaterialised);
            PartOMaterialisationRefusal refusal = Assert.Single(materialisation.Refusals);
            Assert.Equal(PartOMaterialisationRefusalReason.VentilationUnitSelection, refusal.Reason);
            Assert.Equal(Zone(Baseline(), Flat1).Name, Flat1);
            Assert.Equal("MVHR Flat 1", refusal.Subject);

            //An automatic neighbour is not offered as a silent substitute: the same set with Flat 1 automatic selects Large.
            Assert.Equal("Large", UnitOf(Materialise(WithStrategies(Baseline(), Mvhr(Flat1), Natural(Flat2), Natural(Flat3)), [tiny, Large]).AnalyticalModel, Flat1).SelectedVentilationUnitReference()?.Model);
        }

        [Fact]
        public void Materialisation_NeverIsolates_AndAnIsolatedModelIsNotABaseline()
        {
            //Final-materialisation invariant: the whole clean building, never an isolated derivative.
            AnalyticalModel baseline = WithStrategies(Baseline(), Mvhr(Flat1), Natural(Flat2), Natural(Flat3));
            AnalyticalModel model = Materialise(baseline, null, Flat1).AnalyticalModel;

            Assert.False(model.HasValue(AnalyticalModelParameter.PartOIsolationContext));
            Assert.Equal(baseline.AdjacencyCluster.GetSpaces().Count, model.AdjacencyCluster.GetSpaces().Count);

            AnalyticalModel isolated = baseline.PreparePartOIteration(PartOIteration.BaseNaturalVentilation, Zones(baseline, Flat2), Words(baseline, "NV", Flat2), null, true).AnalyticalModel;
            Assert.True(isolated.HasValue(AnalyticalModelParameter.PartOIsolationContext));
            AssertRefused(isolated, PartOMaterialisationRefusalReason.MaterialisedBaseline, "isolation context");
        }

        // =================================================================================================
        // Retained 2B design (D3/D4)
        // =================================================================================================

        [Fact]
        public void RetainedBalancedDesign_IsMaterialisedFromTheTerminals_AndTheStrategyHoldsNoAirflow()
        {
            AnalyticalModel baseline = WithAcceptedRaisedDesign(Baseline(), out double supply_Raised, out double extract_Raised);
            string fingerprint = baseline.AdjacencyCluster.PartODwellingDesignFingerprint(Zone(baseline, Flat1));

            baseline = WithStrategies(baseline, Retained(Flat1, fingerprint), Natural(Flat2), Mvhr(Flat3));

            AnalyticalModel model = Materialise(baseline).AnalyticalModel;
            AdjacencyCluster adjacencyCluster = model.AdjacencyCluster;

            Assert.Equal(supply_Raised, SpaceDesignFlow(adjacencyCluster, adjacencyCluster.GetSpaces().Find(x => x.Name == "Bedroom 1"), FlowClassification.Supply), 6);
            Assert.Equal(extract_Raised, SpaceDesignFlow(adjacencyCluster, adjacencyCluster.GetSpaces().Find(x => x.Name == "Kitchen"), FlowClassification.Extract), 6);

            //The movements are derived from those terminals.
            Assert.Contains(AirMovementsTouching(model, Flat1), x => System.Math.Abs(x.AirFlow * 1000 - supply_Raised) < 1e-6);

            //One airflow authority: the persisted strategy carries no number but the fingerprint.
            string json = baseline.GetValue<PartODwellingStrategySet>(AnalyticalModelParameter.PartODwellingStrategies).ToJsonObject().ToJsonString();
            Assert.DoesNotContain(supply_Raised.ToString(CultureInfo.InvariantCulture), json);
            Assert.DoesNotContain("FlowRate", json);
            Assert.DoesNotContain("_Lps", json);
        }

        [Fact]
        public void RetainedDesign_IsNotChangedByAnotherDwellingsStrategy()
        {
            AnalyticalModel baseline = WithAcceptedRaisedDesign(Baseline(), out _, out _);
            string fingerprint = baseline.AdjacencyCluster.PartODwellingDesignFingerprint(Zone(baseline, Flat1));

            AnalyticalModel model_A = Materialise(WithStrategies(baseline, Retained(Flat1, fingerprint), Natural(Flat2), Natural(Flat3)), [Small, Large]).AnalyticalModel;
            AnalyticalModel model_B = Materialise(WithStrategies(baseline, Retained(Flat1, fingerprint), Natural(Flat2), Mvhr(Flat3, Large.VentilationUnitReference)), [Small, Large]).AnalyticalModel;

            Assert.Equal(Signature(model_A, Flat1), Signature(model_B, Flat1));
        }

        [Fact]
        public void RetainedDesign_WhoseTerminalsMoved_IsStale_AndARequirementBasisOverChangedTerminalsRefuses()
        {
            AnalyticalModel baseline = WithAcceptedRaisedDesign(Baseline(), out _, out _);
            string fingerprint = baseline.AdjacencyCluster.PartODwellingDesignFingerprint(Zone(baseline, Flat1));

            //A later edit of the baseline's terminals.
            AdjacencyCluster adjacencyCluster = baseline.AdjacencyCluster;
            Space bedroom = adjacencyCluster.GetSpaces().Find(x => x.Name == "Bedroom 1");
            adjacencyCluster.SetSpaceDesignFlowRate(bedroom, FlowClassification.Supply, SpaceDesignFlow(adjacencyCluster, bedroom, FlowClassification.Supply) + 1.0, out _, out _);
            AnalyticalModel baseline_Edited = new(baseline, adjacencyCluster);

            AssertRefused(WithStrategies(baseline_Edited, Retained(Flat1, fingerprint), Natural(Flat2), Natural(Flat3)), PartOMaterialisationRefusalReason.RetainedDesignStale, "stale");

            //The accepted (raised) design under a Part F requirement basis is not silently reset.
            AssertRefused(WithStrategies(baseline, Mvhr(Flat1), Natural(Flat2), Natural(Flat3)), PartOMaterialisationRefusalReason.DesignDiffersFromRequirement, "Bedroom 1");

            //A retained design with no terminals to retain it on.
            AssertRefused(WithStrategies(Baseline(), Retained(Flat1, "0000"), Natural(Flat2), Natural(Flat3)), PartOMaterialisationRefusalReason.RetainedDesignStale, "no design terminals");
        }

        // =================================================================================================
        // Cooling gate (D5, P12)
        // =================================================================================================

        [Fact]
        public void ActiveCooling_Persists_AndWithoutAProductIsRefused_NeverGated()
        {
            PartODwellingStrategy cooled = new(Zone(Baseline(), Flat1).Guid, PartOVentilationMode.MVHR, null, PartOActiveCooling.SupplyAirCooling);
            Assert.True(cooled.IsValid);

            AnalyticalModel baseline = WithStrategies(Baseline(), new PartODwellingStrategy(Guid.Empty, PartOVentilationMode.MVHR, null, PartOActiveCooling.SupplyAirCooling).For(Flat1), Natural(Flat2), Natural(Flat3));

            //It persists...
            Assert.Equal(PartOActiveCooling.SupplyAirCooling, new AnalyticalModel(baseline.ToJsonObject()).GetValue<PartODwellingStrategySet>(AnalyticalModelParameter.PartODwellingStrategies).Strategy(Zone(baseline, Flat1).Guid).ActiveCooling);

            //...and a generic unit has no cooling authority. The PR1 gate is gone (PR3B-1).
            AssertRefused(baseline, PartOMaterialisationRefusalReason.CoolingWithoutProductGuidance, "has no selected product");
            Assert.DoesNotContain(baseline.MaterialisePartODwellingStrategies().Refusals, x => x.Reason == PartOMaterialisationRefusalReason.CoolingGated);
        }

        [Fact]
        public void ReusedConditionedUnit_IsRefused_RatherThanLeakingCooling()
        {
            //An authored, dwelling-local unit connected to Flat 1's baseline design terminals, conditioned to
            //18 degC in summer (P12). A clean baseline: no Part O type, no movements, no Part F rates.
            AnalyticalModel baseline = WithAuthoredUnit(Baseline(), 18.0, out string name_Unit);

            PartOMaterialisation materialisation = WithStrategies(baseline, Mvhr(Flat1), Natural(Flat2), Natural(Flat3)).MaterialisePartODwellingStrategies();
            output.WriteLine(materialisation.Refusal);

            Assert.Null(materialisation.AnalyticalModel);
            PartOMaterialisationRefusal refusal = Assert.Single(materialisation.Refusals);
            Assert.Equal(PartOMaterialisationRefusalReason.ConditionedReusedUnit, refusal.Reason);
            Assert.Equal(name_Unit, refusal.Subject);

            //The same authored unit without a supply temperature is reused, and its movement carries no cooling.
            AnalyticalModel model = Materialise(WithStrategies(WithAuthoredUnit(Baseline(), double.NaN, out _), Mvhr(Flat1), Natural(Flat2), Natural(Flat3))).AnalyticalModel;
            AirHandlingUnit airHandlingUnit = UnitOf(model, Flat1);
            Assert.Equal(name_Unit, airHandlingUnit.Name);
            Assert.Null(model.AdjacencyCluster.GetRelatedObjects<AirHandlingUnitAirMovement>(airHandlingUnit).Single().Cooling);
        }

        // =================================================================================================
        // Common spaces (D2)
        // =================================================================================================

        [Fact]
        public void CommunalCorridor_IsIncludedAutomatically_WithAnIterationNeutralScenario()
        {
            AnalyticalModel baseline = Baseline();
            PartOMaterialisation materialisation = Materialise(WithStrategies(baseline, Mvhr(Flat1), Natural(Flat2), Mvhr(Flat3)));
            AnalyticalModel model = materialisation.AnalyticalModel;
            Zone corridor = Zone(model, Corridor);

            OverheatingScenario overheatingScenario = materialisation.OverheatingScenarios.Single(x => x.ZoneGuid == corridor.Guid);
            Assert.Equal(PartOAssessmentScope.CommonSpace, overheatingScenario.Scope);
            Assert.Equal(PartOIteration.DwellingIndependent, overheatingScenario.Iteration);
            Assert.Equal("UV", overheatingScenario.VentilationStrategy);
            Assert.Equal(0, overheatingScenario.OperatingAssumptions.Count);
            Assert.Contains(corridor.Guid, materialisation.Record.ZoneGuids_CommonSpace);

            //A new key: never the corridor key a homogeneous 1a or 1b run states.
            foreach (PartOIteration partOIteration in new[] { PartOIteration.BasePassive, PartOIteration.BaseNaturalVentilation })
            {
                OverheatingScenario legacy = Analytical.Create.OverheatingScenarios([corridor], partOIteration, new Dictionary<Guid, string> { { corridor.Guid, "UV" } }, out _).Single();
                Assert.NotEqual(legacy.Key, overheatingScenario.Key);
            }

            //Iteration-neutral: the key does not move with the dwellings' strategies.
            PartOMaterialisation materialisation_Other = Materialise(WithStrategies(baseline, Natural(Flat1), Natural(Flat2), Natural(Flat3)));
            Assert.Equal(overheatingScenario.Key, materialisation_Other.OverheatingScenarios.Single(x => x.ZoneGuid == corridor.Guid).Key);

            //One map carries NV, MVHR and the corridor criterion at once, with no conflict.
            OverheatingScenarioMap overheatingScenarioMap = new(materialisation.OverheatingScenarios, model, SimulationSpaceMap.Identity(model.GetSpaces()));
            Assert.Empty(overheatingScenarioMap.Refusals);
            Assert.All(Spaces(model, Flat1), x => Assert.Equal("MVHR", overheatingScenarioMap.VentilationStrategyMap.Selection(x).VentilationStrategy));
            Assert.All(Spaces(model, Flat2), x => Assert.Equal("NV", overheatingScenarioMap.VentilationStrategyMap.Selection(x).VentilationStrategy));
            Assert.All(Spaces(model, Corridor), x => Assert.Equal("UV", overheatingScenarioMap.VentilationStrategyMap.Selection(x).VentilationStrategy));
        }

        [Fact]
        public void CorridorClassification_ReadsTheAssignedInternalCondition_NeverTheName()
        {
            //A space NAMED "Corridor" that is not assigned the corridor condition is not an assessed corridor.
            AnalyticalModel baseline_Named = Baseline(corridorCondition: "Corridor IC");
            PartOMaterialisation materialisation_Named = Materialise(WithStrategies(baseline_Named, Natural(Flat1), Natural(Flat2), Natural(Flat3)));
            Assert.DoesNotContain(materialisation_Named.OverheatingScenarios, x => x.Scope == PartOAssessmentScope.CommonSpace);
            Assert.Contains(materialisation_Named.Notes, x => x.Contains("not an assessed communal corridor"));

            //A space named nothing like a corridor that IS assigned it, is.
            AnalyticalModel baseline_Renamed = Baseline(corridorSpaceName: "Level 1 Lobby 3");
            PartOMaterialisation materialisation_Renamed = Materialise(WithStrategies(baseline_Renamed, Natural(Flat1), Natural(Flat2), Natural(Flat3)));
            Assert.Single(materialisation_Renamed.OverheatingScenarios, x => x.Scope == PartOAssessmentScope.CommonSpace);
            Assert.True(Spaces(baseline_Renamed, Corridor).Single().IsTM59CommunalCorridor());
        }

        [Fact]
        public void CommonSpaceZone_MixingCorridorAndOtherSpaces_IsRefusedAsAmbiguous_ByAssignedConditionNotName()
        {
            //The corridor space is named like a lobby; the non-corridor store is named like a corridor. The refusal
            //classifies by the assigned condition, and names each side.
            AnalyticalModel baseline = Baseline(corridorSpaceName: "Level 1 Lobby");
            AdjacencyCluster adjacencyCluster = baseline.AdjacencyCluster;
            Space store = new("Corridor Store") { InternalCondition = new InternalCondition("Store IC") };
            adjacencyCluster.AddObject(store);
            adjacencyCluster.AddRelation(adjacencyCluster.GetObjects<Zone>().Find(x => x.Name == Corridor), store);
            baseline = WithStrategies(new AnalyticalModel(baseline, adjacencyCluster), Natural(Flat1), Natural(Flat2), Natural(Flat3));

            PartOMaterialisation materialisation = baseline.MaterialisePartODwellingStrategies();
            output.WriteLine(materialisation.Refusal);

            Assert.Null(materialisation.AnalyticalModel);
            PartOMaterialisationRefusal refusal = Assert.Single(materialisation.Refusals);
            Assert.Equal(PartOMaterialisationRefusalReason.CommonSpaceUnclassifiable, refusal.Reason);
            Assert.Equal(Zone(baseline, Corridor).Guid, refusal.ZoneGuid);
            Assert.Contains("('Level 1 Lobby') with space(s) that are not ('Corridor Store')", refusal.Message);
        }

        [Fact]
        public void CorridorAssignedSpace_OutsideAWholeCorridorCommonZone_IsNeverSilentlyOmitted()
        {
            //In a dwelling zone.
            AnalyticalModel baseline = Baseline();
            AdjacencyCluster adjacencyCluster = baseline.AdjacencyCluster;
            Space landing = new("Flat 3 Landing") { InternalCondition = new InternalCondition(TM59InternalConditionResolver.CommunalCorridorInternalConditionName) };
            adjacencyCluster.AddObject(landing);
            adjacencyCluster.AddRelation(adjacencyCluster.GetObjects<Zone>().Find(x => x.Name == Flat3), landing);
            AssertRefused(WithStrategies(new AnalyticalModel(baseline, adjacencyCluster), Natural(Flat1), Natural(Flat2), Natural(Flat3)), PartOMaterialisationRefusalReason.CommonSpaceUnclassifiable, "in dwelling zone 'Flat 3'");

            //In no zone at all.
            baseline = Baseline();
            adjacencyCluster = baseline.AdjacencyCluster;
            adjacencyCluster.AddObject(new Space("Stair Core") { InternalCondition = new InternalCondition(TM59InternalConditionResolver.CommunalCorridorInternalConditionName) });
            AssertRefused(WithStrategies(new AnalyticalModel(baseline, adjacencyCluster), Natural(Flat1), Natural(Flat2), Natural(Flat3)), PartOMaterialisationRefusalReason.CommonSpaceUnclassifiable, "'Stair Core' (in no dwelling or common-space zone)");
        }

        [Fact]
        public void CommonSpaceZone_MixingCorridorAndOtherSpaces_IsRefused()
        {
            AnalyticalModel baseline = Baseline();
            AdjacencyCluster adjacencyCluster = baseline.AdjacencyCluster;
            Space store = new("Bin Store") { InternalCondition = new InternalCondition("Store IC") };
            adjacencyCluster.AddObject(store);
            adjacencyCluster.AddRelation(adjacencyCluster.GetObjects<Zone>().Find(x => x.Name == Corridor), store);

            AssertRefused(WithStrategies(new AnalyticalModel(baseline, adjacencyCluster), Natural(Flat1), Natural(Flat2), Natural(Flat3)), PartOMaterialisationRefusalReason.CommonSpaceUnclassifiable, Corridor);
        }

        [Fact]
        public void DwellingIndependent_IsACommonSpaceIdentityOnly()
        {
            AnalyticalModel baseline = Baseline();

            //Never a dwelling scenario...
            List<OverheatingScenario> overheatingScenarios = Analytical.Create.OverheatingScenarios(Zones(baseline, Flat1), PartOIteration.DwellingIndependent, Words(baseline, "MVHR", Flat1), out List<string> refusals);
            Assert.Empty(overheatingScenarios);
            Assert.Contains(refusals, x => x.Contains("common space only"));

            //...and it prepares nothing: no route, no Part F operating condition, no opening assumption, no model.
            Assert.Equal(PartOVentilationMode.Undefined, PartOIteration.DwellingIndependent.PartOIterationVentilationMode(out string refusal_Mode));
            Assert.NotNull(refusal_Mode);
            Assert.Null(PartOIteration.DwellingIndependent.PartOIterationOperatingMode(out string refusal_Operating));
            Assert.Contains("prepares nothing", refusal_Operating);
            Assert.Equal(0, PartOIteration.DwellingIndependent.PartOOperatingAssumptions(out _).Count);
            Assert.Equal(PartOOpeningCompatibility.Compatible, baseline.PartOIterationOpeningCompatibility(PartOIteration.DwellingIndependent, out _, out _));

            PartOIterationPreparation preparation = baseline.PreparePartOIteration(PartOIteration.DwellingIndependent, Zones(baseline, Flat1), Words(baseline, "MVHR", Flat1));
            Assert.NotNull(preparation.Refusal);
            Assert.Null(preparation.AnalyticalModel);

            //It round-trips by name.
            OverheatingScenario overheatingScenario = Analytical.Create.PartOCommonSpaceOverheatingScenario(Zone(baseline, Corridor));
            OverheatingScenario overheatingScenario_RoundTrip = new(overheatingScenario.ToJsonObject());
            Assert.Equal(overheatingScenario.Key, overheatingScenario_RoundTrip.Key);
            Assert.Equal(PartOIteration.DwellingIndependent, overheatingScenario_RoundTrip.Iteration);
        }

        // =================================================================================================
        // Shared systems (P7)
        // =================================================================================================

        [Fact]
        public void AuthoredSystemWithDuty_StraddlingTwoDwellings_IsRefused_AndLeftAlone()
        {
            AnalyticalModel baseline = Baseline();
            AdjacencyCluster adjacencyCluster = baseline.AdjacencyCluster;

            VentilationSystem shared = new("Legacy MV", new VentilationSystemType("MV", "Authored legacy mechanical ventilation"));
            adjacencyCluster.AddObject(shared);

            foreach (string name in new[] { "Living Room", "Flat 2 Bedroom" })
            {
                Space space = adjacencyCluster.GetSpaces().Find(x => x.Name == name);
                VentilationTerminal ventilationTerminal = new(name + " authored", FlowClassification.Supply, 5.0);
                adjacencyCluster.AddObject(ventilationTerminal);
                adjacencyCluster.AddRelation(ventilationTerminal, space);
                adjacencyCluster.AddRelation(shared, space);
                adjacencyCluster.AddRelation(shared, ventilationTerminal);
            }

            AssertRefused(WithStrategies(new AnalyticalModel(baseline, adjacencyCluster), Mvhr(Flat1), Mvhr(Flat2), Natural(Flat3)), PartOMaterialisationRefusalReason.SharedSystem, "Legacy MV");
        }

        [Theory]
        [InlineData("unassessed dwelling")]
        [InlineData("corridor")]
        public void AuthoredUnit_SharedWithASystemOutsideTheAssessedDwellings_IsRefused(string other)
        {
            //Review finding: Flat 1's authored system and a second system both name the same unit. The second
            //serves either Flat 3, left out of the assessed scope, or the communal corridor - neither is an assessed
            //dwelling, so only the unit's full set of systems shows that it is shared.
            AnalyticalModel baseline = WithAuthoredUnit(Baseline(), double.NaN, out string name_Unit);
            AdjacencyCluster adjacencyCluster = baseline.AdjacencyCluster;

            VentilationSystem ventilationSystem_Other = new("Neighbour authored MV", new VentilationSystemType("MV authored", "Authored mechanical ventilation"));
            ventilationSystem_Other.SetValue(VentilationSystemParameter.SupplyUnitName, name_Unit);
            ventilationSystem_Other.SetValue(VentilationSystemParameter.ExhaustUnitName, name_Unit);
            adjacencyCluster.AddObject(ventilationSystem_Other);
            adjacencyCluster.AddRelation(ventilationSystem_Other, adjacencyCluster.GetSpaces().Single(x => x.Name == (other == "corridor" ? Corridor : "Flat 3 Bedroom")));
            baseline = new AnalyticalModel(baseline, adjacencyCluster);

            AnalyticalModel baseline_Strategies = other == "corridor"
                ? WithStrategies(baseline, Mvhr(Flat1), Natural(Flat2), Natural(Flat3))
                : WithStrategies(baseline, Mvhr(Flat1), Natural(Flat2));

            PartOMaterialisation materialisation = baseline_Strategies.MaterialisePartODwellingStrategies(null, other == "corridor" ? null : [Zone(baseline, Flat1).Guid, Zone(baseline, Flat2).Guid]);
            output.WriteLine(materialisation.Refusal ?? "(materialised)");

            Assert.Null(materialisation.AnalyticalModel);
            Assert.Contains(materialisation.Refusals, x => x.Reason == PartOMaterialisationRefusalReason.SharedSystem && x.Subject == name_Unit && x.Message.Contains("'Flat 1'") && x.Message.Contains(other == "corridor" ? "'Corridor'" : "'Flat 3'"));

            //Without the shared unit the same baseline materialises: the refusal is the unit, not the reuse.
            Assert.True(WithStrategies(WithAuthoredUnit(Baseline(), double.NaN, out _), Mvhr(Flat1), Natural(Flat2), Natural(Flat3)).MaterialisePartODwellingStrategies().IsMaterialised);
        }

        [Fact]
        public void AuthoredSystemWithoutDuty_IsTemplateMetadata_AndPassesUntouched()
        {
            //PR0 P7's exact system: related to rooms of two dwellings, no terminals, no unit.
            AnalyticalModel baseline = Baseline();
            AdjacencyCluster adjacencyCluster = baseline.AdjacencyCluster;
            VentilationSystem shared = new("Legacy MV", new VentilationSystemType("MV", "Authored legacy mechanical ventilation"));
            adjacencyCluster.AddObject(shared);
            adjacencyCluster.AddRelation(shared, adjacencyCluster.GetSpaces().Find(x => x.Name == "Living Room"));
            adjacencyCluster.AddRelation(shared, adjacencyCluster.GetSpaces().Find(x => x.Name == "Flat 2 Bedroom"));

            PartOMaterialisation materialisation = Materialise(WithStrategies(new AnalyticalModel(baseline, adjacencyCluster), Mvhr(Flat1), Natural(Flat2), Natural(Flat3)));

            Assert.Contains(materialisation.Notes, x => x.Contains("Legacy MV") && x.Contains("metadata"));
            VentilationSystem shared_After = materialisation.AnalyticalModel.AdjacencyCluster.GetObject<VentilationSystem>(shared.Guid);
            Assert.Equal(2, materialisation.AnalyticalModel.AdjacencyCluster.GetRelatedObjects<Space>(shared_After).Count);
        }

        // =================================================================================================
        // Catalogue + materialisation fingerprint
        // =================================================================================================

        [Fact]
        public void CatalogueFingerprint_CoversEverySelectionRelevantField_AndNotOrder()
        {
            List<VentilationUnitCapacityDescriptor> catalogue = [Small, Large];
            string fingerprint = Analytical.Query.PartOCatalogueFingerprint(catalogue);

            Assert.Equal(fingerprint, Analytical.Query.PartOCatalogueFingerprint([Large, Small]));
            Assert.Equal(fingerprint, Analytical.Query.PartOCatalogueFingerprint([new VentilationUnitCapacityDescriptor(new VentilationUnitReference("Proof", "Small", null), 500, 500), Large]));

            Assert.NotEqual(fingerprint, Analytical.Query.PartOCatalogueFingerprint([new VentilationUnitCapacityDescriptor(Small.VentilationUnitReference, 501, 500), Large]));
            Assert.NotEqual(fingerprint, Analytical.Query.PartOCatalogueFingerprint([new VentilationUnitCapacityDescriptor(Small.VentilationUnitReference, 500, 499), Large]));
            Assert.NotEqual(fingerprint, Analytical.Query.PartOCatalogueFingerprint([new VentilationUnitCapacityDescriptor(Small.VentilationUnitReference, 500, 500, 3), Large]));
            Assert.NotEqual(fingerprint, Analytical.Query.PartOCatalogueFingerprint([new VentilationUnitCapacityDescriptor(new VentilationUnitReference("Proof", "Small", "B"), 500, 500), Large]));
            Assert.NotEqual(fingerprint, Analytical.Query.PartOCatalogueFingerprint(null));
            Assert.NotEqual(Analytical.Query.PartOCatalogueFingerprint(null), Analytical.Query.PartOCatalogueFingerprint([]));
        }

        [Fact]
        public void MaterialisationRecord_IsCurrent_UntilTheBaselineStrategiesOrCatalogueMove()
        {
            List<VentilationUnitCapacityDescriptor> catalogue = [Small, Large];
            AnalyticalModel baseline = WithStrategies(Baseline(), Mvhr(Flat1), Natural(Flat2), Mvhr(Flat3));

            PartOMaterialisationRecord record = new(Materialise(baseline, catalogue).AnalyticalModel.ToJsonObject() is JsonObject jsonObject
                ? new AnalyticalModel(jsonObject).GetValue<PartOMaterialisationRecord>(AnalyticalModelParameter.PartOMaterialisationRecord)
                : null);

            Assert.True(record.IsValid);
            Assert.True(record.IsCurrent(baseline, catalogue, out string reason), reason);

            //A capacity correction that changes which unit is selected (Small can no longer serve).
            Assert.False(record.IsCurrent(baseline, [new VentilationUnitCapacityDescriptor(Small.VentilationUnitReference, 1, 1), Large], out reason));
            Assert.Contains("catalogue", reason);
            Assert.False(record.IsCurrent(baseline, [new VentilationUnitCapacityDescriptor(Small.VentilationUnitReference, 500, 500, 9), Large], out _));

            //A strategy edit.
            Assert.False(record.IsCurrent(WithStrategies(baseline, Natural(Flat1), Natural(Flat2), Mvhr(Flat3)), catalogue, out reason));
            Assert.Contains("strategy", reason);

            //A baseline edit.
            AdjacencyCluster adjacencyCluster = baseline.AdjacencyCluster;
            Space space = new(adjacencyCluster.GetSpaces().First());
            space.SetValue(SpaceParameter.Area, 99.0);
            adjacencyCluster.AddObject(space);
            Assert.False(record.IsCurrent(new AnalyticalModel(baseline, adjacencyCluster), catalogue, out reason));
            Assert.Contains("baseline", reason);
        }

        // =================================================================================================
        // Persistence
        // =================================================================================================

        [Fact]
        public void StrategySet_RoundTripsThroughTheModelJson_Canonically()
        {
            AnalyticalModel baseline = WithAcceptedRaisedDesign(Baseline(), out _, out _);
            baseline = WithStrategies(baseline, Retained(Flat1, baseline.AdjacencyCluster.PartODwellingDesignFingerprint(Zone(baseline, Flat1))), Natural(Flat2), Mvhr(Flat3, Large.VentilationUnitReference));

            AnalyticalModel roundTrip = new(baseline.ToJsonObject());
            PartODwellingStrategySet set = baseline.GetValue<PartODwellingStrategySet>(AnalyticalModelParameter.PartODwellingStrategies);
            PartODwellingStrategySet set_RoundTrip = roundTrip.GetValue<PartODwellingStrategySet>(AnalyticalModelParameter.PartODwellingStrategies);

            Assert.True(set_RoundTrip.IsValid);
            Assert.Equal(set.Strategies.Select(x => x.CanonicalText()), set_RoundTrip.Strategies.Select(x => x.CanonicalText()));
            Assert.Equal(set.ToJsonObject().ToJsonString(), set_RoundTrip.ToJsonObject().ToJsonString());

            //Logically identical sets, built in another order from re-created product references, write the same
            //bytes - so they cannot make the baseline fingerprint move.
            PartODwellingStrategySet set_Reordered = new(set.Strategies.AsEnumerable().Reverse().Select(x => new PartODwellingStrategy(x) { VentilationUnitReference = x.VentilationUnitReference is null ? null : new VentilationUnitReference(x.VentilationUnitReference.Manufacturer, x.VentilationUnitReference.Model, x.VentilationUnitReference.Reference) }));
            Assert.Equal(set.ToJsonObject().ToJsonString(), set_Reordered.ToJsonObject().ToJsonString());
            Assert.Equal(SimulationResultProvenance.Fingerprint(baseline), SimulationResultProvenance.Fingerprint(WithSet(baseline, set_Reordered)));

            //Materialising the round-tripped baseline gives the same state.
            Assert.Equal(Signature(Materialise(baseline, [Small, Large]).AnalyticalModel), Signature(Materialise(roundTrip, [Small, Large]).AnalyticalModel));
        }

        [Fact]
        public void StrategySet_OfAnUnknownSchema_DuplicatedDwelling_OrUnreadableProperty_IsRefused_AndSurvivesResave()
        {
            AnalyticalModel baseline = WithStrategies(Baseline(), Mvhr(Flat1), Natural(Flat2), Natural(Flat3));
            JsonObject jsonObject = baseline.GetValue<PartODwellingStrategySet>(AnalyticalModelParameter.PartODwellingStrategies).ToJsonObject();

            JsonObject jsonObject_Schema = (JsonObject)jsonObject.DeepClone();
            jsonObject_Schema["Schema"] = "PartODwellingStrategies:v99";
            AssertRefused(WithSet(baseline, new PartODwellingStrategySet(jsonObject_Schema)), PartOMaterialisationRefusalReason.InvalidStrategySet, "v99");

            JsonObject jsonObject_Duplicate = (JsonObject)jsonObject.DeepClone();
            JsonObject jsonObject_Flat1 = (JsonObject)((JsonArray)jsonObject_Duplicate["Strategies"]).First(x => x["VentilationMode"].GetValue<string>() == "MVHR").DeepClone();
            jsonObject_Flat1["VentilationMode"] = "NaturalVentilation";
            ((JsonArray)jsonObject_Duplicate["Strategies"]).Add(jsonObject_Flat1);
            PartODwellingStrategySet set_Duplicate = new(jsonObject_Duplicate);
            Assert.False(set_Duplicate.IsValid);
            AssertRefused(WithSet(baseline, set_Duplicate), PartOMaterialisationRefusalReason.InvalidStrategySet, "more than one");

            //Re-saving an invalid set writes the conflict back, never a silently chosen winner.
            Assert.False(new PartODwellingStrategySet(set_Duplicate.ToJsonObject()).IsValid);

            JsonObject jsonObject_Unreadable = (JsonObject)jsonObject.DeepClone();
            ((JsonArray)jsonObject_Unreadable["Strategies"]).First(x => x["VentilationMode"].GetValue<string>() == "MVHR")["VentilationMode"] = "MVHRBoostCooled";
            AssertRefused(WithSet(baseline, new PartODwellingStrategySet(jsonObject_Unreadable)), PartOMaterialisationRefusalReason.InvalidStrategy, Flat1);

            //A product that is stated but names nothing is unreadable - never "select from the pool" - and stays so
            //through a re-save.
            JsonObject jsonObject_Product = (JsonObject)jsonObject.DeepClone();
            ((JsonArray)jsonObject_Product["Strategies"]).First(x => x["VentilationMode"].GetValue<string>() == "MVHR")["VentilationUnitReference"] = new JsonObject();
            PartODwellingStrategySet set_Product = new(jsonObject_Product);
            Assert.False(set_Product.Strategy(Zone(baseline, Flat1).Guid).IsValid);
            Assert.False(new PartODwellingStrategySet(set_Product.ToJsonObject()).Strategy(Zone(baseline, Flat1).Guid).IsValid);
            AssertRefused(WithSet(baseline, set_Product), PartOMaterialisationRefusalReason.InvalidStrategy, Flat1);
        }

        [Fact]
        public void LegacyModel_WithNoStrategyCollection_IsLegacy_NotAnError()
        {
            AnalyticalModel legacy = Baseline();

            //No mixed authority is invented...
            PartOMaterialisation materialisation = legacy.MaterialisePartODwellingStrategies();
            Assert.Null(materialisation.AnalyticalModel);
            Assert.Equal(PartOMaterialisationRefusalReason.NoStrategies, Assert.Single(materialisation.Refusals).Reason);

            //...and the legacy path is untouched: it still prepares, and nothing reads a strategy.
            PartOIterationPreparation preparation = legacy.PreparePartOIteration(PartOIteration.BasePassive, Zones(legacy, Flat1), Words(legacy, "MVHR", Flat1));
            Assert.Null(preparation.Refusal);
            Assert.False(preparation.AnalyticalModel.HasValue(AnalyticalModelParameter.PartODwellingStrategies));
            Assert.Equal("MVHR-01", preparation.AirHandlingUnit.Name);
        }

        [Fact]
        public void MissingStrategy_IsNeverReadAsNatural()
        {
            AnalyticalModel baseline = WithStrategies(Baseline(), Mvhr(Flat1), Natural(Flat2));

            AssertRefused(baseline, PartOMaterialisationRefusalReason.MissingStrategy, Flat3);

            //A strategy for the corridor is refused: common spaces are not strategy rows.
            AssertRefused(WithStrategies(Baseline(), Mvhr(Flat1), Natural(Flat2), Natural(Flat3), Natural(Corridor)), PartOMaterialisationRefusalReason.NotADwelling, Corridor);
        }

        // =================================================================================================
        // Scoped Part F seam
        // =================================================================================================

        [Fact]
        public void ScopedPartFRates_WriteOnlyTheScope_AndNullIsTheWholeModelCall()
        {
            AnalyticalModel baseline = Baseline();

            AnalyticalModel model_Whole = baseline.ApplyPartFVentilationRates(PartFOperatingMode.ContinuousDesign, out List<string> refusals_Whole, out List<string> notes_Whole);
            AnalyticalModel model_Null = baseline.ApplyPartFVentilationRates(PartFOperatingMode.ContinuousDesign, null, out List<string> refusals_Null, out List<string> notes_Null);

            Assert.Equal(refusals_Whole, refusals_Null);
            Assert.Equal(notes_Whole, notes_Null);
            Assert.Equal(Signature(model_Whole), Signature(model_Null));

            AnalyticalModel model_Scoped = baseline.ApplyPartFVentilationRates(PartFOperatingMode.ContinuousDesign, Spaces(baseline, Flat1), out _, out _);

            Assert.Equal(Signature(model_Whole, Flat1), Signature(model_Scoped, Flat1));
            Assert.Equal(Signature(baseline, Flat2), Signature(model_Scoped, Flat2));
            Assert.Equal(Signature(baseline, Flat3), Signature(model_Scoped, Flat3));
        }

        // =================================================================================================
        // Determinism (P2, P10 promoted)
        // =================================================================================================

        [Fact]
        public void SameBaselineAndStrategies_OnIndependentClones_GiveTheSameEngineeringState_AndRequireAFreshSimulation()
        {
            AnalyticalModel baseline = WithStrategies(Baseline(), Mvhr(Flat1, Large.VentilationUnitReference), Natural(Flat2), Mvhr(Flat3));
            List<VentilationUnitCapacityDescriptor> catalogue = [Small, Large];

            PartOMaterialisation materialisation_1 = Materialise(new AnalyticalModel(baseline.ToJsonObject()), catalogue);
            PartOMaterialisation materialisation_2 = Materialise(new AnalyticalModel(baseline.ToJsonObject()), catalogue);

            List<string> signature_1 = Signature(materialisation_1.AnalyticalModel);
            List<string> signature_2 = Signature(materialisation_2.AnalyticalModel);
            signature_1.Except(signature_2).ToList().ForEach(x => output.WriteLine("only 1: " + x));
            signature_2.Except(signature_1).ToList().ForEach(x => output.WriteLine("only 2: " + x));
            Assert.Equal(signature_1, signature_2);

            //Names are now part of the equal state.
            Assert.Equal(Names(materialisation_1.AnalyticalModel), Names(materialisation_2.AnalyticalModel));

            //The record is the same...
            Assert.Equal(materialisation_1.Record.Fingerprint_Baseline, materialisation_2.Record.Fingerprint_Baseline);
            Assert.Equal(materialisation_1.Record.Fingerprint_Strategies, materialisation_2.Record.Fingerprint_Strategies);
            Assert.Equal(materialisation_1.Record.Fingerprint_Catalogue, materialisation_2.Record.Fingerprint_Catalogue);
            Assert.Equal(materialisation_1.OverheatingScenarios.Select(x => x.Key).OrderBy(x => x), materialisation_2.OverheatingScenarios.Select(x => x.Key).OrderBy(x => x));

            //...but generated objects have new guids, so a rebuilt model never matches an earlier run's provenance.
            Assert.NotEqual(SimulationResultProvenance.Fingerprint(materialisation_1.AnalyticalModel), SimulationResultProvenance.Fingerprint(materialisation_2.AnalyticalModel));
        }

        [Fact]
        public void StrategyAndScopeOrder_DoNotChangeTheResult()
        {
            AnalyticalModel baseline = Baseline();
            List<VentilationUnitCapacityDescriptor> catalogue = [Small, Large];

            AnalyticalModel model_X = Materialise(WithStrategies(baseline, Mvhr(Flat1), Natural(Flat2), Mvhr(Flat3, Large.VentilationUnitReference)), catalogue, Flat1, Flat2, Flat3).AnalyticalModel;
            AnalyticalModel model_Y = Materialise(WithStrategies(baseline, Mvhr(Flat3, Large.VentilationUnitReference), Natural(Flat2), Mvhr(Flat1)), catalogue, Flat3, Flat2, Flat1).AnalyticalModel;

            Assert.Equal(Signature(model_X), Signature(model_Y));
            Assert.Equal(Names(model_X), Names(model_Y));
            Assert.Equal(UnitOf(model_X, Flat1).Name, UnitOf(model_Y, Flat1).Name);

            //And the dwelling order inside the loop is canonical: materialising only Flat 3 gives Flat 3 the same
            //design as materialising it after Flat 1.
            AnalyticalModel model_Z = Materialise(WithStrategies(baseline, Mvhr(Flat3, Large.VentilationUnitReference)), catalogue, Flat3).AnalyticalModel;
            Assert.Equal(Signature(model_X, Flat3), Signature(model_Z, Flat3));
            Assert.Equal(UnitOf(model_X, Flat3).Name, UnitOf(model_Z, Flat3).Name);
        }

        // =================================================================================================
        // Fixture
        // =================================================================================================

        private static AnalyticalModel Baseline(string corridorCondition = null, string corridorSpaceName = Corridor)
        {
            MethodInfo methodInfo = typeof(PartOIterationPreparationTests).GetMethod("ModelWithTwoAssessedDwellings", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(methodInfo);

            object[] arguments = [null, null];
            AnalyticalModel analyticalModel = (AnalyticalModel)methodInfo.Invoke(null, arguments);

            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;

            Space space_Supply = AuthoredSpace("Flat 3 Bedroom", PartFType.Habitable, PartFVentilationType.supply, PartFTerminalRole.Supply, 10.0);
            Space space_Extract = AuthoredSpace("Flat 3 Bathroom", PartFType.WetRoom, PartFVentilationType.extract, PartFTerminalRole.GeneralExtract, 10.0);
            adjacencyCluster.AddObject(space_Supply);
            adjacencyCluster.AddObject(space_Extract);
            Helpers.DwellingPartitions.Partition(adjacencyCluster, space_Supply.Name, space_Extract.Name, 200);

            Zone zone_3 = new(Flat3);
            adjacencyCluster.AddObject(zone_3);
            adjacencyCluster.AddRelation(zone_3, space_Supply);
            adjacencyCluster.AddRelation(zone_3, space_Extract);

            Space space_Corridor = new(corridorSpaceName);
            space_Corridor.SetValue(SpaceParameter.Area, 20.0);
            space_Corridor.SetValue(SpaceParameter.Volume, 50.0);
            space_Corridor.InternalCondition = new InternalCondition(corridorCondition ?? TM59InternalConditionResolver.CommunalCorridorInternalConditionName);
            adjacencyCluster.AddObject(space_Corridor);
            Helpers.DwellingPartitions.Partition(adjacencyCluster, corridorSpaceName, "Living Room", 300);
            Helpers.DwellingPartitions.Partition(adjacencyCluster, corridorSpaceName, "Flat 2 Bedroom", 310);

            Zone zone_Corridor = new(Corridor);
            adjacencyCluster.AddObject(zone_Corridor);
            adjacencyCluster.AddRelation(zone_Corridor, space_Corridor);

            foreach (Zone zone in adjacencyCluster.GetObjects<Zone>())
            {
                zone.SetValue(ZoneParameter.IsDwelling, zone.Name != Corridor);
                adjacencyCluster.AddObject(zone);
            }

            return new AnalyticalModel(analyticalModel, adjacencyCluster);
        }

        private static Space AuthoredSpace(string name, PartFType partFType, PartFVentilationType partFVentilationType, PartFTerminalRole partFTerminalRole, double flow_Lps)
        {
            Space space = new(name);
            space.SetValue(SpaceParameter.Area, 10.0);
            space.SetValue(SpaceParameter.Volume, 25.0);

            InternalCondition internalCondition = new(name + " IC");
            internalCondition.SetValue(InternalConditionParameter.VentilationSystemTypeName, "Ventilation System");
            space.InternalCondition = internalCondition;

            bool supply = partFVentilationType == PartFVentilationType.supply;
            PartFSpaceData partFSpaceData = new(name, partFType, partFVentilationType, supply, null, true, true, supply, false, "Volume", flow_Lps);

            partFSpaceData.Terminals.Add(new PartFVentilationTerminalRequirement(name + (supply ? " - Supply" : " - Extract"), space.Guid, partFTerminalRole)
            {
                SpaceName = name,
                OperatingMode = PartFOperatingMode.ContinuousDesign,
                ContinuousDesignFlowRate_Lps = flow_Lps,
                IsInBalancedFlow = true,
                IsRequired = true,
                SourceReference = "PR1 fixture",
            });

            space.SetValue(SpaceParameter.PartFSpaceData, partFSpaceData);

            return space;
        }

        /// <summary>
        /// The D3 acceptance of a 2B outcome, done on the baseline: realise Flat 1's terminals (scoped), then
        /// write a BALANCED raise - +2 l/s supply in Bedroom 1 and +2 l/s extract in the Kitchen.
        /// </summary>
        private static AnalyticalModel WithAcceptedRaisedDesign(AnalyticalModel baseline, out double supply_Raised, out double extract_Raised)
        {
            AdjacencyCluster adjacencyCluster = baseline.AdjacencyCluster;
            adjacencyCluster.RealizePartFVentilationTerminals(Spaces(baseline, Flat1), out _, out List<string> refusals);
            Assert.Empty(refusals);

            Space bedroom = adjacencyCluster.GetSpaces().Find(x => x.Name == "Bedroom 1");
            Space kitchen = adjacencyCluster.GetSpaces().Find(x => x.Name == "Kitchen");

            supply_Raised = SpaceDesignFlow(adjacencyCluster, bedroom, FlowClassification.Supply) + 2.0;
            extract_Raised = SpaceDesignFlow(adjacencyCluster, kitchen, FlowClassification.Extract) + 2.0;

            adjacencyCluster.SetSpaceDesignFlowRate(bedroom, FlowClassification.Supply, supply_Raised, out _, out List<string> refusals_Supply);
            adjacencyCluster.SetSpaceDesignFlowRate(kitchen, FlowClassification.Extract, extract_Raised, out _, out List<string> refusals_Extract);
            Assert.Empty(refusals_Supply);
            Assert.Empty(refusals_Extract);

            AnalyticalModel result = new(baseline, adjacencyCluster);
            Assert.True(result.IsPartOCleanBaseline(out List<PartOMaterialisationRefusal> findings), string.Join("\n", findings));

            return result;
        }

        /// <summary>
        /// An engineer-authored inter-zone air movement, as SAMAnalytical.CreateIZAMBySpaces builds it: from one space
        /// to another (or to outside where <paramref name="name_To"/> is null), related to both.
        /// </summary>
        private static AnalyticalModel WithAuthoredMovement(AnalyticalModel baseline, string name_From, string name_To, double airFlow_Lps, out Guid guid)
        {
            AdjacencyCluster adjacencyCluster = baseline.AdjacencyCluster;
            Space from = adjacencyCluster.GetSpaces().Single(x => x.Name == name_From);
            Space to = name_To is null ? null : adjacencyCluster.GetSpaces().Single(x => x.Name == name_To);

            SpaceAirMovement spaceAirMovement = new(string.Format("{0} authored", name_From), airFlow_Lps / 1000.0, new Core.ObjectReference(from).ToString(), to is null ? null : new Core.ObjectReference(to).ToString());
            adjacencyCluster.AddObject(spaceAirMovement);
            adjacencyCluster.AddRelation(spaceAirMovement, from);
            if (to is not null)
            {
                adjacencyCluster.AddRelation(spaceAirMovement, to);
            }

            guid = spaceAirMovement.Guid;

            return new AnalyticalModel(baseline, adjacencyCluster);
        }

        /// <summary>An authored (not Part O typed) system and unit connected to Flat 1's baseline design terminals.</summary>
        private static AnalyticalModel WithAuthoredUnit(AnalyticalModel baseline, double summerSupplyTemperature, out string name_Unit)
        {
            name_Unit = "Flat 1 authored AHU";

            AdjacencyCluster adjacencyCluster = baseline.AdjacencyCluster;
            List<Space> spaces = Spaces(baseline, Flat1);
            List<VentilationTerminal> ventilationTerminals = adjacencyCluster.RealizePartFVentilationTerminals(spaces, out _, out List<string> refusals);
            Assert.Empty(refusals);

            AirHandlingUnit airHandlingUnit = Analytical.Create.AirHandlingUnit(name_Unit);
            airHandlingUnit.SummerSupplyTemperature = summerSupplyTemperature;
            airHandlingUnit.WinterSupplyTemperature = double.NaN;
            adjacencyCluster.AddObject(airHandlingUnit);

            VentilationSystem ventilationSystem = new("Flat 1 authored MVHR", new VentilationSystemType("MVHR authored", "Authored heat recovery"));
            ventilationSystem.SetValue(VentilationSystemParameter.SupplyUnitName, name_Unit);
            ventilationSystem.SetValue(VentilationSystemParameter.ExhaustUnitName, name_Unit);
            adjacencyCluster.AddObject(ventilationSystem);

            foreach (VentilationTerminal ventilationTerminal in ventilationTerminals)
            {
                adjacencyCluster.AddRelation(ventilationSystem, ventilationTerminal);
            }

            foreach (Space space in spaces.Where(x => (adjacencyCluster.VentilationTerminals(x)?.Count ?? 0) != 0))
            {
                adjacencyCluster.AddRelation(ventilationSystem, space);
            }

            AnalyticalModel result = new(baseline, adjacencyCluster);
            Assert.True(result.IsPartOCleanBaseline(out List<PartOMaterialisationRefusal> findings), string.Join("\n", findings));

            return result;
        }

        private static (string Zone, PartODwellingStrategy Strategy) Mvhr(string name_Zone, VentilationUnitReference ventilationUnitReference = null) => (name_Zone, new PartODwellingStrategy(Guid.Empty, PartOVentilationMode.MVHR, ventilationUnitReference));

        private static (string Zone, PartODwellingStrategy Strategy) Natural(string name_Zone) => (name_Zone, new PartODwellingStrategy(Guid.Empty, PartOVentilationMode.NaturalVentilation));

        private static (string Zone, PartODwellingStrategy Strategy) Retained(string name_Zone, string fingerprint) => (name_Zone, new PartODwellingStrategy(Guid.Empty, PartOVentilationMode.MVHR, null, PartOActiveCooling.None, PartODesignAirFlowBasis.RetainedDesign, fingerprint));

        private static AnalyticalModel WithStrategies(AnalyticalModel analyticalModel, params (string Zone, PartODwellingStrategy Strategy)[] strategies)
        {
            PartODwellingStrategySet partODwellingStrategySet = new();

            foreach ((string name_Zone, PartODwellingStrategy partODwellingStrategy) in strategies)
            {
                PartODwellingStrategy selected = new(partODwellingStrategy) { ZoneGuid = Zone(analyticalModel, name_Zone).Guid };
                if (selected.ActiveCooling == PartOActiveCooling.SupplyAirCooling && selected.VentilationMode == PartOVentilationMode.MVHR && selected.CoolingStatSpaceGuid == Guid.Empty)
                {
                    //Existing success fixtures state their deliberate control choice explicitly. Legacy tests use WithSet.
                    selected.CoolingStatSpaceGuid = Spaces(analyticalModel, name_Zone).First().Guid;
                }
                partODwellingStrategySet.Set(selected);
            }

            return WithSet(analyticalModel, partODwellingStrategySet);
        }

        private static AnalyticalModel WithSet(AnalyticalModel analyticalModel, PartODwellingStrategySet partODwellingStrategySet)
        {
            AnalyticalModel result = new(analyticalModel);
            result.SetValue(AnalyticalModelParameter.PartODwellingStrategies, partODwellingStrategySet);

            return result;
        }

        private PartOMaterialisation Materialise(AnalyticalModel analyticalModel, IEnumerable<VentilationUnitCapacityDescriptor> descriptors = null, params string[] names_Zone)
        {
            PartOMaterialisation result = analyticalModel.MaterialisePartODwellingStrategies(descriptors, names_Zone.Length == 0 ? null : names_Zone.Select(x => Zone(analyticalModel, x).Guid));

            Assert.True(result.IsMaterialised, result.Refusal);

            return result;
        }

        private void AssertRefused(AnalyticalModel analyticalModel, PartOMaterialisationRefusalReason reason, string text, IEnumerable<VentilationUnitCapacityDescriptor> descriptors = null)
        {
            PartOMaterialisation materialisation = analyticalModel.MaterialisePartODwellingStrategies(descriptors);
            output.WriteLine(materialisation.Refusal ?? "(materialised)");

            Assert.Null(materialisation.AnalyticalModel);
            Assert.Contains(materialisation.Refusals, x => x.Reason == reason && ((x.Message?.Contains(text) ?? false) || x.Subject == text));
        }

        private static Zone Zone(AnalyticalModel analyticalModel, string name) => analyticalModel.GetZones().Find(x => x.Name == name);

        private static List<Zone> Zones(AnalyticalModel analyticalModel, params string[] names) => names.Select(x => Zone(analyticalModel, x)).ToList();

        private static Dictionary<Guid, string> Words(AnalyticalModel analyticalModel, string word, params string[] names) => names.ToDictionary(x => Zone(analyticalModel, x).Guid, x => word);

        private static List<Space> Spaces(AnalyticalModel analyticalModel, string name_Zone)
        {
            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;
            return adjacencyCluster.GetRelatedObjects<Space>(Zone(analyticalModel, name_Zone)) ?? [];
        }

        private static List<VentilationTerminal> Terminals(AnalyticalModel analyticalModel, string name_Zone)
        {
            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;
            return Spaces(analyticalModel, name_Zone).SelectMany(x => adjacencyCluster.GetRelatedObjects<VentilationTerminal>(x) ?? []).ToList();
        }

        private static List<VentilationSystem> SystemsServing(AnalyticalModel analyticalModel, string name_Zone)
        {
            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;
            HashSet<Guid> guids = Spaces(analyticalModel, name_Zone).Select(x => x.Guid).ToHashSet();

            return (adjacencyCluster.GetObjects<VentilationSystem>() ?? []).FindAll(x => (adjacencyCluster.GetRelatedObjects<Space>(x) ?? []).Exists(y => guids.Contains(y.Guid)));
        }

        private static AirHandlingUnit UnitOf(AnalyticalModel analyticalModel, string name_Zone)
        {
            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;
            string name_Unit = SystemsServing(analyticalModel, name_Zone).Single().GetValue<string>(VentilationSystemParameter.SupplyUnitName);

            return adjacencyCluster.GetObjects<AirHandlingUnit>().Single(x => x.Name == name_Unit);
        }

        private static List<SpaceAirMovement> AirMovementsTouching(AnalyticalModel analyticalModel, string name_Zone)
        {
            HashSet<string> references = Spaces(analyticalModel, name_Zone).Select(x => new Core.ObjectReference(x).ToString()).ToHashSet();

            return (analyticalModel.AdjacencyCluster.GetObjects<SpaceAirMovement>() ?? []).FindAll(x => (x.From != null && references.Contains(x.From)) || (x.To != null && references.Contains(x.To)));
        }

        private static double SpaceDesignFlow(AdjacencyCluster adjacencyCluster, Space space, FlowClassification flowClassification)
        {
            return (adjacencyCluster.GetRelatedObjects<VentilationTerminal>(space) ?? []).Where(x => x.FlowClassification == flowClassification).Sum(x => x.DesignFlowRate_Lps ?? 0);
        }

        /// <summary>Every system and unit name, sorted - identity that must now be order-independent too.</summary>
        private static List<string> Names(AnalyticalModel analyticalModel)
        {
            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;
            List<string> result = [.. (adjacencyCluster.GetObjects<VentilationSystem>() ?? []).Select(x => "system " + x.FullName), .. (adjacencyCluster.GetObjects<AirHandlingUnit>() ?? []).Select(x => "unit " + x.Name)];
            result.Sort(StringComparer.Ordinal);

            return result;
        }

        /// <summary>
        /// PR0's GUID-insensitive statement of the engineering state, one sorted line per fact, each prefixed by
        /// the dwelling it belongs to: systems and served spaces; units and products; internal-condition supply
        /// and extract; terminals with flow and connection; every space movement with endpoints, flow and
        /// profile; every unit movement with its heating, cooling, humidification, dehumidification and density
        /// profiles. Unit names are normalised to the dwelling, so this compares engineering state, and
        /// <see cref="Names"/> compares identity separately.
        /// </summary>
        private static List<string> Signature(AnalyticalModel analyticalModel, string name_Zone = null)
        {
            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;
            string F(double value) => value.ToString("0.######", CultureInfo.InvariantCulture);

            Dictionary<Guid, string> dwelling_Space = [];
            foreach (Zone zone in adjacencyCluster.GetObjects<Zone>() ?? [])
            {
                foreach (Space space in adjacencyCluster.GetRelatedObjects<Space>(zone) ?? [])
                {
                    dwelling_Space[space.Guid] = zone.Name;
                }
            }

            string DwellingOf(Space space) => space != null && dwelling_Space.TryGetValue(space.Guid, out string name) ? name : "(none)";

            Dictionary<string, string> label_Unit = [];
            List<string> result = [];

            foreach (VentilationSystem ventilationSystem in adjacencyCluster.GetObjects<VentilationSystem>() ?? [])
            {
                List<Space> spaces = adjacencyCluster.GetRelatedObjects<Space>(ventilationSystem) ?? [];
                string label = string.Join("+", spaces.Select(DwellingOf).Distinct().OrderBy(x => x, StringComparer.Ordinal));
                string name_Unit = ventilationSystem.GetValue<string>(VentilationSystemParameter.SupplyUnitName);
                if (name_Unit != null)
                {
                    label_Unit[name_Unit] = label;
                }

                result.Add(string.Format("{0}|system|{1}|serves {2}", label, ventilationSystem.Type?.Name, string.Join(",", spaces.Select(x => x.Name).OrderBy(x => x, StringComparer.Ordinal))));
            }

            Dictionary<string, string> name_Reference = [];
            foreach (Space space in adjacencyCluster.GetSpaces() ?? [])
            {
                name_Reference[new Core.ObjectReference(space).ToString()] = space.Name;
            }

            foreach (AirHandlingUnit airHandlingUnit in adjacencyCluster.GetObjects<AirHandlingUnit>() ?? [])
            {
                string label = label_Unit.TryGetValue(airHandlingUnit.Name, out string value) ? value : "(unlinked)";
                name_Reference[new Core.ObjectReference(airHandlingUnit).ToString()] = "unit[" + label + "]";

                VentilationUnitReference ventilationUnitReference = airHandlingUnit.SelectedVentilationUnitReference();
                result.Add(string.Format("{0}|unit|product {1}|summer {2}", label, ventilationUnitReference == null ? "-" : ventilationUnitReference.Manufacturer + " " + ventilationUnitReference.Model, F(airHandlingUnit.SummerSupplyTemperature)));
            }

            foreach (Space space in adjacencyCluster.GetSpaces() ?? [])
            {
                string dwelling = DwellingOf(space);
                InternalCondition internalCondition = space.InternalCondition;
                string supply = internalCondition != null && internalCondition.TryGetValue(InternalConditionParameter.SupplyAirFlow, out double s) ? F(s * 1000) : "-";
                string extract = internalCondition != null && internalCondition.TryGetValue(InternalConditionParameter.ExhaustAirFlow, out double e) ? F(e * 1000) : "-";
                result.Add(string.Format("{0}|ic|{1}|{2}|supply {3} extract {4}", dwelling, space.Name, internalCondition?.Name, supply, extract));

                foreach (VentilationTerminal ventilationTerminal in adjacencyCluster.GetRelatedObjects<VentilationTerminal>(space) ?? [])
                {
                    List<VentilationSystem> systems = adjacencyCluster.GetRelatedObjects<VentilationSystem>(ventilationTerminal) ?? [];
                    result.Add(string.Format("{0}|terminal|{1}|{2} {3} l/s|connected {4}", dwelling, space.Name, ventilationTerminal.FlowClassification, F(ventilationTerminal.DesignFlowRate_Lps ?? double.NaN), systems.Count));
                }
            }

            string Resolve(string reference) => reference == null ? "outside" : name_Reference.TryGetValue(reference, out string name) ? name : "?" + reference;

            string DwellingOfReference(string reference)
            {
                string name = Resolve(reference);
                if (name.StartsWith("unit[", StringComparison.Ordinal))
                {
                    return name.Substring(5, name.Length - 6);
                }

                Space space = adjacencyCluster.GetSpaces()?.Find(x => new Core.ObjectReference(x).ToString() == reference);
                return space == null ? null : DwellingOf(space);
            }

            string ProfileText(Profile profile)
            {
                if (profile == null)
                {
                    return "-";
                }

                string name = profile.Name ?? string.Empty;
                foreach (KeyValuePair<string, string> keyValuePair in label_Unit.OrderByDescending(x => x.Key.Length))
                {
                    name = name.Replace(keyValuePair.Key, "unit[" + keyValuePair.Value + "]");
                }

                double[] values = profile.GetValues() ?? [];
                return string.Format("{0}/{1}[{2}:{3}]", name, profile.ProfileType, values.Length, F(values.Sum()));
            }

            foreach (SpaceAirMovement spaceAirMovement in adjacencyCluster.GetObjects<SpaceAirMovement>() ?? [])
            {
                string dwelling = DwellingOfReference(spaceAirMovement.To) ?? DwellingOfReference(spaceAirMovement.From) ?? "(none)";
                result.Add(string.Format("{0}|movement|{1} -> {2}|{3} l/s|profile {4}", dwelling, Resolve(spaceAirMovement.From), Resolve(spaceAirMovement.To), F(spaceAirMovement.AirFlow * 1000), ProfileText(spaceAirMovement.Profile)));
            }

            foreach (AirHandlingUnitAirMovement airHandlingUnitAirMovement in adjacencyCluster.GetObjects<AirHandlingUnitAirMovement>() ?? [])
            {
                List<AirHandlingUnit> airHandlingUnits = adjacencyCluster.GetRelatedObjects<AirHandlingUnit>(airHandlingUnitAirMovement) ?? [];
                string label = string.Join("+", airHandlingUnits.Select(x => label_Unit.TryGetValue(x.Name, out string value) ? value : "(unlinked)").OrderBy(x => x, StringComparer.Ordinal));
                result.Add(string.Format(
                    "{0}|unit-movement|units {1}|heating {2} cooling {3} humidification {4} dehumidification {5} density {6}",
                    label.Length == 0 ? "(none)" : label,
                    airHandlingUnits.Count,
                    ProfileText(airHandlingUnitAirMovement.Heating),
                    ProfileText(airHandlingUnitAirMovement.Cooling),
                    ProfileText(airHandlingUnitAirMovement.Humidification),
                    ProfileText(airHandlingUnitAirMovement.Dehumidification),
                    ProfileText(airHandlingUnitAirMovement.Density)));
            }

            result.Sort(StringComparer.Ordinal);

            return name_Zone == null ? result : result.FindAll(x => x.StartsWith(name_Zone + "|", StringComparison.Ordinal));
        }
    }

    internal static class PartODwellingStrategyTestExtensions
    {
        /// <summary>Marks a strategy template with the zone it is for; <c>WithStrategies</c> resolves the guid.</summary>
        public static (string Zone, PartODwellingStrategy Strategy) For(this PartODwellingStrategy partODwellingStrategy, string name_Zone) => (name_Zone, partODwellingStrategy);
    }
}
