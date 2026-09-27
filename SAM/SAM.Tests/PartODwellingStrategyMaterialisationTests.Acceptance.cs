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
