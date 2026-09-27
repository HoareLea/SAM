// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Analytical.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// <c>Modify.AcceptPartODwellingDesign</c> - the explicit "accept 2B for a dwelling" design edit (PR0 D3): a design
    /// a run copy carries (here the materialised all-MVHR model with Flat 1 raised, balanced, exactly as a 2B round
    /// raises one) is written onto the clean baseline's own terminals, and nowhere else.
    /// </summary>
    public partial class PartODwellingStrategyMaterialisationTests
    {
        [Fact]
        public void AcceptedDesign_IsWrittenOntoTheBaselineTerminals_ForThatDwellingOnly()
        {
            AnalyticalModel baseline = Baseline();
            AnalyticalModel source = RaisedRunCopy(baseline, out double supply_Raised, out double extract_Raised);

            string json_Baseline = Core.Convert.ToString(baseline);
            string json_Source = Core.Convert.ToString(source);

            PartODwellingDesignAcceptance acceptance = baseline.AcceptPartODwellingDesign(Zone(baseline, Flat1).Guid, source);
            output.WriteLine(acceptance.Refusal ?? string.Join("\n", acceptance.Notes));

            Assert.True(acceptance.IsAccepted, acceptance.Refusal);
            AnalyticalModel accepted = acceptance.AnalyticalModel;
            AdjacencyCluster adjacencyCluster = accepted.AdjacencyCluster;

            Assert.Equal(supply_Raised, SpaceDesignFlow(adjacencyCluster, adjacencyCluster.GetSpaces().Find(x => x.Name == "Bedroom 1"), FlowClassification.Supply), 6);
            Assert.Equal(extract_Raised, SpaceDesignFlow(adjacencyCluster, adjacencyCluster.GetSpaces().Find(x => x.Name == "Kitchen"), FlowClassification.Extract), 6);
            Assert.Equal(2, acceptance.Changes.Count);
            Assert.Equal(accepted.AdjacencyCluster.PartODwellingDesignFingerprint(Zone(accepted, Flat1)), acceptance.DesignFingerprint);

            //Still a clean baseline, and terminals are all it gained: no system, unit, movement, scenario or result.
            Assert.True(accepted.IsPartOCleanBaseline(out List<PartOMaterialisationRefusal> findings), string.Join("\n", findings));
            Assert.Empty(adjacencyCluster.GetObjects<VentilationSystem>() ?? []);
            Assert.Empty(adjacencyCluster.GetObjects<AirHandlingUnit>() ?? []);
            Assert.Empty(adjacencyCluster.GetObjects<SpaceAirMovement>() ?? []);

            //No other dwelling, and no corridor, is touched.
            foreach (string name in new[] { Flat2, Flat3, Corridor })
            {
                Assert.Equal(DwellingState(baseline, name), DwellingState(accepted, name));
            }

            //Neither input is modified; no strategy is written by the edit.
            Assert.Equal(json_Baseline, Core.Convert.ToString(baseline));
            Assert.Equal(json_Source, Core.Convert.ToString(source));
            Assert.Null(accepted.GetValue<PartODwellingStrategySet>(AnalyticalModelParameter.PartODwellingStrategies));
        }

        [Fact]
        public void AcceptedDesign_IsMaterialisedAsARetainedDesign_BesideNaturalAndMvhr()
        {
            AnalyticalModel baseline = Baseline();
            AnalyticalModel source = RaisedRunCopy(baseline, out double supply_Raised, out double extract_Raised);

            PartODwellingDesignAcceptance acceptance = baseline.AcceptPartODwellingDesign(Zone(baseline, Flat1).Guid, source);
            Assert.True(acceptance.IsAccepted, acceptance.Refusal);

            AnalyticalModel model = Materialise(WithStrategies(acceptance.AnalyticalModel, Retained(Flat1, acceptance.DesignFingerprint), Natural(Flat2), Mvhr(Flat3))).AnalyticalModel;
            AdjacencyCluster adjacencyCluster = model.AdjacencyCluster;

            Assert.Equal(supply_Raised, SpaceDesignFlow(adjacencyCluster, adjacencyCluster.GetSpaces().Find(x => x.Name == "Bedroom 1"), FlowClassification.Supply), 6);
            Assert.Equal(extract_Raised, SpaceDesignFlow(adjacencyCluster, adjacencyCluster.GetSpaces().Find(x => x.Name == "Kitchen"), FlowClassification.Extract), 6);
            Assert.Empty(Terminals(model, Flat2));
        }

        [Fact]
        public void AcceptingTheSameDesignAgain_ChangesNothing_AndKeepsTheFingerprint()
        {
            AnalyticalModel baseline = Baseline();
            AnalyticalModel source = RaisedRunCopy(baseline, out _, out _);

            PartODwellingDesignAcceptance first = baseline.AcceptPartODwellingDesign(Zone(baseline, Flat1).Guid, source);
            PartODwellingDesignAcceptance second = first.AnalyticalModel.AcceptPartODwellingDesign(Zone(baseline, Flat1).Guid, source);

            Assert.True(second.IsAccepted, second.Refusal);
            Assert.Empty(second.Changes);
            Assert.Equal(first.DesignFingerprint, second.DesignFingerprint);
        }

        [Fact]
        public void AcceptingARequirementDesign_RealisesTerminals_ButChangesNoAirflow()
        {
            AnalyticalModel baseline = Baseline();
            AnalyticalModel source = Materialise(WithStrategies(baseline, Mvhr(Flat1), Mvhr(Flat2), Mvhr(Flat3))).AnalyticalModel;

            PartODwellingDesignAcceptance acceptance = baseline.AcceptPartODwellingDesign(Zone(baseline, Flat1).Guid, source);

            Assert.True(acceptance.IsAccepted, acceptance.Refusal);
            Assert.Empty(acceptance.Changes);
            Assert.NotEmpty(Terminals(acceptance.AnalyticalModel, Flat1));
        }

        [Fact]
        public void Acceptance_OntoAModelThatIsNotACleanBaseline_IsRefused()
        {
            AnalyticalModel baseline = Baseline();
            AnalyticalModel source = RaisedRunCopy(baseline, out _, out _);

            //The run copy itself is never a baseline: a previous simulated / materialised model is never patched.
            PartODwellingDesignAcceptance acceptance = source.AcceptPartODwellingDesign(Zone(source, Flat1).Guid, source);

            Assert.False(acceptance.IsAccepted);
            Assert.Null(acceptance.AnalyticalModel);
            Assert.Contains("not a clean Part O baseline", acceptance.Refusal);
        }

        [Fact]
        public void Acceptance_ForANonDwelling_OrADwellingTheSourceLacks_IsRefused()
        {
            AnalyticalModel baseline = Baseline();
            AnalyticalModel source = RaisedRunCopy(baseline, out _, out _);

            PartODwellingDesignAcceptance corridor = baseline.AcceptPartODwellingDesign(Zone(baseline, Corridor).Guid, source);
            Assert.False(corridor.IsAccepted);
            Assert.Contains("is not a dwelling", corridor.Refusal);

            AnalyticalModel other = Baseline();
            PartODwellingDesignAcceptance foreign = baseline.AcceptPartODwellingDesign(Zone(baseline, Flat1).Guid, other);
            Assert.False(foreign.IsAccepted);
            Assert.Contains("not a model of this building", foreign.Refusal);
        }

        [Fact]
        public void Acceptance_FromASourceWithoutTheWholeDesign_IsRefused()
        {
            AnalyticalModel baseline = Baseline();

            //The clean baseline states no design terminals: nothing to accept, and nothing is invented.
            PartODwellingDesignAcceptance acceptance = baseline.AcceptPartODwellingDesign(Zone(baseline, Flat1).Guid, baseline);

            Assert.False(acceptance.IsAccepted);
            Assert.Contains("A partial design is not accepted", acceptance.Refusal);
        }

        [Fact]
        public void Acceptance_OfAnUntraceableTerminal_IsRefused()
        {
            AnalyticalModel baseline = Baseline();
            AnalyticalModel source = RaisedRunCopy(baseline, out _, out _);

            AdjacencyCluster adjacencyCluster = source.AdjacencyCluster;
            Space bedroom = adjacencyCluster.GetSpaces().Find(x => x.Name == "Bedroom 1");
            VentilationTerminal ventilationTerminal = adjacencyCluster.VentilationTerminals(bedroom).First(x => x.FlowClassification == FlowClassification.Supply);
            PartFTerminalReference partFTerminalReference = new(ventilationTerminal.GetValue<PartFTerminalReference>(VentilationTerminalParameter.PartFTerminalReference)) { SourceReference = "a paragraph the baseline never stated" };
            VentilationTerminal ventilationTerminal_Foreign = new(ventilationTerminal.Guid, ventilationTerminal);
            ventilationTerminal_Foreign.SetValue(VentilationTerminalParameter.PartFTerminalReference, partFTerminalReference);
            adjacencyCluster.AddObject(ventilationTerminal_Foreign);

            PartODwellingDesignAcceptance acceptance = baseline.AcceptPartODwellingDesign(Zone(baseline, Flat1).Guid, new AnalyticalModel(source, adjacencyCluster));

            Assert.False(acceptance.IsAccepted);
            Assert.Contains("has no such continuous requirement", acceptance.Refusal);
        }

        [Fact]
        public void Acceptance_OfADesignBelowTheApprovedDocumentFFloor_IsRefused()
        {
            AnalyticalModel baseline = Baseline();
            AnalyticalModel source = Materialise(WithStrategies(baseline, Mvhr(Flat1), Mvhr(Flat2), Mvhr(Flat3))).AnalyticalModel;

            AdjacencyCluster adjacencyCluster = source.AdjacencyCluster;
            Space bedroom = adjacencyCluster.GetSpaces().Find(x => x.Name == "Bedroom 1");
            VentilationTerminal ventilationTerminal = adjacencyCluster.VentilationTerminals(bedroom).First(x => x.FlowClassification == FlowClassification.Supply);
            VentilationTerminal ventilationTerminal_Low = new(ventilationTerminal.Guid, ventilationTerminal.Name, FlowClassification.Supply, ventilationTerminal.DesignFlowRate_Lps - 3.0);
            ventilationTerminal_Low.SetValue(VentilationTerminalParameter.PartFTerminalReference, ventilationTerminal.GetValue<PartFTerminalReference>(VentilationTerminalParameter.PartFTerminalReference));
            adjacencyCluster.AddObject(ventilationTerminal_Low);

            PartODwellingDesignAcceptance acceptance = baseline.AcceptPartODwellingDesign(Zone(baseline, Flat1).Guid, new AnalyticalModel(source, adjacencyCluster));
            output.WriteLine(acceptance.Refusal);

            Assert.False(acceptance.IsAccepted);
            Assert.Null(acceptance.AnalyticalModel);
        }

        [Fact]
        public void Acceptance_OntoASpaceWithADesignerAddedTerminal_IsRefused()
        {
            AnalyticalModel baseline = Baseline();
            AnalyticalModel source = RaisedRunCopy(baseline, out _, out _);

            //A terminal the designer added to the baseline, realising no requirement.
            AdjacencyCluster adjacencyCluster = baseline.AdjacencyCluster;
            Space bedroom = adjacencyCluster.GetSpaces().Find(x => x.Name == "Bedroom 1");
            VentilationTerminal ventilationTerminal = new("Designer diffuser", FlowClassification.Supply, 5.0);
            adjacencyCluster.AddObject(ventilationTerminal);
            adjacencyCluster.AddRelation(ventilationTerminal, bedroom);
            AnalyticalModel baseline_Designer = new(baseline, adjacencyCluster);

            PartODwellingDesignAcceptance acceptance = baseline_Designer.AcceptPartODwellingDesign(Zone(baseline_Designer, Flat1).Guid, source);

            Assert.False(acceptance.IsAccepted);
            Assert.Contains("does not realise exactly one continuous Approved Document F requirement", acceptance.Refusal);
        }

        [Theory]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NaN)]
        [InlineData(-1.0)]
        public void Acceptance_WithAnUnusableTolerance_IsRefused(double tolerance_Lps)
        {
            AnalyticalModel baseline = Baseline();

            PartODwellingDesignAcceptance acceptance = baseline.AcceptPartODwellingDesign(Zone(baseline, Flat1).Guid, RaisedRunCopy(baseline, out _, out _), tolerance_Lps);

            Assert.False(acceptance.IsAccepted);
            Assert.Contains("tolerance", acceptance.Refusal);
        }

        [Fact]
        public void AcceptedChange_ReportsTheAirflowPersisted_WhenSnappedToTheFloor()
        {
            //An elevated baseline (a raised design accepted), then a source a rounding bit below the Part F floor.
            AnalyticalModel baseline = Baseline();
            AnalyticalModel baseline_Elevated = baseline.AcceptPartODwellingDesign(Zone(baseline, Flat1).Guid, RaisedRunCopy(baseline, out _, out _)).AnalyticalModel;

            AnalyticalModel source = Materialise(WithStrategies(baseline, Mvhr(Flat1), Mvhr(Flat2), Mvhr(Flat3))).AnalyticalModel;
            double floor = SpaceDesignFlow(source.AdjacencyCluster, source.AdjacencyCluster.GetSpaces().Find(x => x.Name == "Bedroom 1"), FlowClassification.Supply);
            source = WithTerminal(source, "Bedroom 1", FlowClassification.Supply, x => Replaced(x, x.FlowClassification, floor - 0.0005));

            PartODwellingDesignAcceptance acceptance = baseline_Elevated.AcceptPartODwellingDesign(Zone(baseline, Flat1).Guid, source);
            Assert.True(acceptance.IsAccepted, acceptance.Refusal);

            PartODwellingDesignChange change = acceptance.Changes.Single(x => x.SpaceName == "Bedroom 1" && x.FlowClassification == FlowClassification.Supply);
            AdjacencyCluster adjacencyCluster = acceptance.AnalyticalModel.AdjacencyCluster;
            double persisted = SpaceDesignFlow(adjacencyCluster, adjacencyCluster.GetSpaces().Find(x => x.Name == "Bedroom 1"), FlowClassification.Supply);

            Assert.Equal(floor, persisted, 9);
            Assert.Equal(persisted, change.After_Lps, 9);
        }

        [Fact]
        public void Acceptance_OfASourceTerminalWhoseDirectionContradictsItsRequirement_IsRefused()
        {
            AnalyticalModel baseline = Baseline();
            AnalyticalModel source = WithTerminal(RaisedRunCopy(baseline, out _, out _), "Bedroom 1", FlowClassification.Supply, x => Replaced(x, FlowClassification.Extract, x.DesignFlowRate_Lps ?? 0));

            PartODwellingDesignAcceptance acceptance = baseline.AcceptPartODwellingDesign(Zone(baseline, Flat1).Guid, source);

            Assert.False(acceptance.IsAccepted);
            Assert.Contains("is classified", acceptance.Refusal);
        }

        [Fact]
        public void Acceptance_OntoABaselineTerminalRealisingNoContinuousRequirement_OrTheWrongDirection_IsRefused()
        {
            AnalyticalModel baseline = Baseline();
            AnalyticalModel source = RaisedRunCopy(baseline, out _, out _);
            AdjacencyCluster adjacencyCluster_Source = source.AdjacencyCluster;
            PartFTerminalReference reference_Supply = adjacencyCluster_Source.VentilationTerminals(adjacencyCluster_Source.GetSpaces().Find(x => x.Name == "Bedroom 1")).First(x => x.FlowClassification == FlowClassification.Supply).GetValue<PartFTerminalReference>(VentilationTerminalParameter.PartFTerminalReference);

            //A lineage-tagged device realising no continuous requirement (e.g. intermittent), and one in the wrong direction.
            foreach ((PartFTerminalReference reference, FlowClassification flowClassification) in new[] { (new PartFTerminalReference(reference_Supply) { SourceReference = "intermittent device" }, FlowClassification.Supply), (new PartFTerminalReference(reference_Supply), FlowClassification.Extract) })
            {
                AdjacencyCluster adjacencyCluster = baseline.AdjacencyCluster;
                Space bedroom = adjacencyCluster.GetSpaces().Find(x => x.Name == "Bedroom 1");
                VentilationTerminal ventilationTerminal = new("Tagged device", flowClassification, 5.0);
                ventilationTerminal.SetValue(VentilationTerminalParameter.PartFTerminalReference, reference);
                adjacencyCluster.AddObject(ventilationTerminal);
                adjacencyCluster.AddRelation(ventilationTerminal, bedroom);

                PartODwellingDesignAcceptance acceptance = new AnalyticalModel(baseline, adjacencyCluster).AcceptPartODwellingDesign(Zone(baseline, Flat1).Guid, source);

                Assert.False(acceptance.IsAccepted);
                Assert.Contains("does not realise exactly one continuous Approved Document F requirement in its own direction", acceptance.Refusal);
            }
        }

        [Fact]
        public void Acceptance_RefusedAtALaterWrite_PublishesNoChangesOrNotes()
        {
            //Bedroom 1 raised (written first, by name), Kitchen below its floor (refused by SetSpaceDesignFlowRate).
            AnalyticalModel baseline = Baseline();
            AnalyticalModel source = WithTerminal(RaisedRunCopy(baseline, out _, out double extract_Raised), "Kitchen", FlowClassification.Extract, x => Replaced(x, x.FlowClassification, extract_Raised - 2.0 - 3.0));

            PartODwellingDesignAcceptance acceptance = baseline.AcceptPartODwellingDesign(Zone(baseline, Flat1).Guid, source);

            Assert.False(acceptance.IsAccepted);
            Assert.Null(acceptance.AnalyticalModel);
            Assert.Empty(acceptance.Changes);
            Assert.Empty(acceptance.Notes);
        }

        [Fact]
        public void Acceptance_OntoABaselineWithAnUnusableTerminalDuty_IsRefused_EvenWhereTheTotalAlreadyMatches()
        {
            //Bedroom 1's requirement realised as two subdivided terminals, -5 and requirement + 5: the total equals the
            //requirement, so a matching source would skip the write and the setter's validation.
            AnalyticalModel baseline = Baseline();
            AdjacencyCluster adjacencyCluster = baseline.AdjacencyCluster;
            adjacencyCluster.RealizePartFVentilationTerminals(Spaces(baseline, Flat1), out _, out List<string> refusals);
            Assert.Empty(refusals);

            Space bedroom = adjacencyCluster.GetSpaces().Find(x => x.Name == "Bedroom 1");
            VentilationTerminal ventilationTerminal = adjacencyCluster.VentilationTerminals(bedroom).First(x => x.FlowClassification == FlowClassification.Supply);
            double requirement = ventilationTerminal.DesignFlowRate_Lps ?? 0;
            adjacencyCluster.AddObject(Replaced(ventilationTerminal, FlowClassification.Supply, requirement + 5.0));

            VentilationTerminal ventilationTerminal_Negative = new("Bedroom 1 - negative duty", FlowClassification.Supply, -5.0);
            ventilationTerminal_Negative.SetValue(VentilationTerminalParameter.PartFTerminalReference, ventilationTerminal.GetValue<PartFTerminalReference>(VentilationTerminalParameter.PartFTerminalReference));
            adjacencyCluster.AddObject(ventilationTerminal_Negative);
            adjacencyCluster.AddRelation(ventilationTerminal_Negative, bedroom);

            AnalyticalModel baseline_Invalid = new(baseline, adjacencyCluster);
            AnalyticalModel source = Materialise(WithStrategies(baseline, Mvhr(Flat1), Mvhr(Flat2), Mvhr(Flat3))).AnalyticalModel;

            PartODwellingDesignAcceptance acceptance = baseline_Invalid.AcceptPartODwellingDesign(Zone(baseline_Invalid, Flat1).Guid, source);

            Assert.False(acceptance.IsAccepted);
            Assert.Contains("states no usable design airflow", acceptance.Refusal);
        }

        [Fact]
        public void Acceptance_ForADwellingSharingASpaceWithAnother_IsRefused()
        {
            AnalyticalModel baseline = Baseline();
            AnalyticalModel source = RaisedRunCopy(baseline, out _, out _);

            AdjacencyCluster adjacencyCluster = baseline.AdjacencyCluster;
            adjacencyCluster.AddRelation(adjacencyCluster.GetZones().Find(x => x.Name == Flat2), adjacencyCluster.GetSpaces().Find(x => x.Name == "Bedroom 1"));
            AnalyticalModel baseline_Overlap = new(baseline, adjacencyCluster);

            PartODwellingDesignAcceptance acceptance = baseline_Overlap.AcceptPartODwellingDesign(Zone(baseline_Overlap, Flat1).Guid, source);

            Assert.False(acceptance.IsAccepted);
            Assert.Contains("belongs to both", acceptance.Refusal);
        }

        [Fact]
        public void Acceptance_ForADwellingSharingASpaceWithTheCorridor_IsRefused()
        {
            AnalyticalModel baseline = Baseline();
            AnalyticalModel source = RaisedRunCopy(baseline, out _, out _);

            AdjacencyCluster adjacencyCluster = baseline.AdjacencyCluster;
            adjacencyCluster.AddRelation(adjacencyCluster.GetZones().Find(x => x.Name == Corridor), adjacencyCluster.GetSpaces().Find(x => x.Name == "Bedroom 1"));
            AnalyticalModel baseline_Overlap = new(baseline, adjacencyCluster);

            PartODwellingDesignAcceptance acceptance = baseline_Overlap.AcceptPartODwellingDesign(Zone(baseline_Overlap, Flat1).Guid, source);

            Assert.False(acceptance.IsAccepted);
            Assert.Contains("belongs to both", acceptance.Refusal);
        }

        [Fact]
        public void Acceptance_OntoABaselineTerminalBelowTheFloor_IsRefused_EvenWhereTheSourceStatesTheSameTotal()
        {
            //Bedroom 1 realised on the baseline, then set 3 l/s below its Part F requirement; the source states that total.
            AnalyticalModel baseline = Baseline();
            AdjacencyCluster adjacencyCluster = baseline.AdjacencyCluster;
            adjacencyCluster.RealizePartFVentilationTerminals(Spaces(baseline, Flat1), out _, out List<string> refusals);
            Assert.Empty(refusals);

            Space bedroom = adjacencyCluster.GetSpaces().Find(x => x.Name == "Bedroom 1");
            VentilationTerminal ventilationTerminal = adjacencyCluster.VentilationTerminals(bedroom).First(x => x.FlowClassification == FlowClassification.Supply);
            double low = (ventilationTerminal.DesignFlowRate_Lps ?? 0) - 3.0;
            adjacencyCluster.AddObject(Replaced(ventilationTerminal, FlowClassification.Supply, low));
            AnalyticalModel baseline_Low = new(baseline, adjacencyCluster);

            AnalyticalModel source = WithTerminal(Materialise(WithStrategies(baseline, Mvhr(Flat1), Mvhr(Flat2), Mvhr(Flat3))).AnalyticalModel, "Bedroom 1", FlowClassification.Supply, x => Replaced(x, x.FlowClassification, low));

            PartODwellingDesignAcceptance acceptance = baseline_Low.AcceptPartODwellingDesign(Zone(baseline_Low, Flat1).Guid, source);

            Assert.False(acceptance.IsAccepted);
            Assert.Null(acceptance.AnalyticalModel);
        }

        private static AnalyticalModel WithTerminal(AnalyticalModel analyticalModel, string name_Space, FlowClassification flowClassification, Func<VentilationTerminal, VentilationTerminal> func)
        {
            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;
            Space space = adjacencyCluster.GetSpaces().Find(x => x.Name == name_Space);
            VentilationTerminal ventilationTerminal = adjacencyCluster.VentilationTerminals(space).First(x => x.FlowClassification == flowClassification);
            adjacencyCluster.AddObject(func(ventilationTerminal));

            return new AnalyticalModel(analyticalModel, adjacencyCluster);
        }

        private static VentilationTerminal Replaced(VentilationTerminal ventilationTerminal, FlowClassification flowClassification, double designFlowRate_Lps)
        {
            VentilationTerminal result = new(ventilationTerminal.Guid, ventilationTerminal.Name, flowClassification, designFlowRate_Lps);
            result.SetValue(VentilationTerminalParameter.PartFTerminalReference, ventilationTerminal.GetValue<PartFTerminalReference>(VentilationTerminalParameter.PartFTerminalReference));

            return result;
        }

        /// <summary>
        /// A run copy carrying a 2B-style design for Flat 1: the all-MVHR materialisation (terminals regenerated, so
        /// only lineage links them to the baseline), with Bedroom 1 supply and Kitchen extract each raised 2 l/s -
        /// balanced, exactly as a 2B round raises a dwelling.
        /// </summary>
        private AnalyticalModel RaisedRunCopy(AnalyticalModel baseline, out double supply_Raised, out double extract_Raised)
        {
            AnalyticalModel model = Materialise(WithStrategies(baseline, Mvhr(Flat1), Mvhr(Flat2), Mvhr(Flat3))).AnalyticalModel;

            AdjacencyCluster adjacencyCluster = model.AdjacencyCluster;
            Space bedroom = adjacencyCluster.GetSpaces().Find(x => x.Name == "Bedroom 1");
            Space kitchen = adjacencyCluster.GetSpaces().Find(x => x.Name == "Kitchen");

            supply_Raised = SpaceDesignFlow(adjacencyCluster, bedroom, FlowClassification.Supply) + 2.0;
            extract_Raised = SpaceDesignFlow(adjacencyCluster, kitchen, FlowClassification.Extract) + 2.0;

            adjacencyCluster.SetSpaceDesignFlowRate(bedroom, FlowClassification.Supply, supply_Raised, out _, out List<string> refusals_Supply);
            adjacencyCluster.SetSpaceDesignFlowRate(kitchen, FlowClassification.Extract, extract_Raised, out _, out List<string> refusals_Extract);
            Assert.Empty(refusals_Supply);
            Assert.Empty(refusals_Extract);

            return new AnalyticalModel(model, adjacencyCluster);
        }

        /// <summary>Every space of a zone and every terminal related to them, as SAM serialises them.</summary>
        private static List<string> DwellingState(AnalyticalModel analyticalModel, string name_Zone)
        {
            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;
            List<string> result = [];

            foreach (Space space in Spaces(analyticalModel, name_Zone))
            {
                result.Add(space.ToJsonObject().ToJsonString());
                result.AddRange((adjacencyCluster.GetRelatedObjects<VentilationTerminal>(space) ?? []).Select(x => x.ToJsonObject().ToJsonString()));
            }

            result.Sort(StringComparer.Ordinal);
            return result;
        }
    }
}
