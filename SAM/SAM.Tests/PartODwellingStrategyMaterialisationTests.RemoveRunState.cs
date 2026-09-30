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
    /// <c>Modify.RemovePartORunState</c> - Results &gt; Remove Results in SAM_UI: a model that has been through a
    /// Part O Prepare &amp; Run becomes the clean baseline Mixed Design starts from. Judged throughout by
    /// <c>Query.PartOBaselineFindings</c>, and proven by materialising the cleaned copy and the original baseline
    /// to the same engineering signature.
    /// </summary>
    public partial class PartODwellingStrategyMaterialisationTests
    {
        [Fact]
        public void RemoveRunState_PreparedAndRunModel_BecomesTheBaselineItWasPreparedFrom()
        {
            AnalyticalModel baseline = WithHeldConditions(Baseline());
            AnalyticalModel run = PreparedAndRun(baseline);

            List<PartOMaterialisationRefusal> findings_Run = run.PartOBaselineFindings();
            Assert.Contains(findings_Run, x => x.Reason == PartOMaterialisationRefusalReason.RunOutputBaseline && x.Message.Contains("overheating scenarios"));
            Assert.Contains(findings_Run, x => x.Reason == PartOMaterialisationRefusalReason.RunOutputBaseline && x.Message.Contains("provenance"));
            Assert.Contains(findings_Run, x => x.Reason == PartOMaterialisationRefusalReason.RunOutputBaseline && x.Message.Contains("simulation result"));
            Assert.Contains(findings_Run, x => x.Reason == PartOMaterialisationRefusalReason.RunOutputBaseline && x.Message.Contains("design-day record"));
            Assert.Contains(findings_Run, x => x.Reason == PartOMaterialisationRefusalReason.MaterialisedBaseline && x.Message.Contains("Part O MVHR system"));
            Assert.Contains(findings_Run, x => x.Reason == PartOMaterialisationRefusalReason.MaterialisedBaseline && x.Message.Contains("ApplyPartFVentilationRates"));
            string json_Run = Core.Convert.ToString(run);

            AnalyticalModel cleaned = run.RemovePartORunState(out List<string> removed, out List<string> kept);
            output.WriteLine(string.Join("\n", removed));

            Assert.Empty(kept);
            Assert.True(cleaned.IsPartOCleanBaseline(out List<PartOMaterialisationRefusal> findings), string.Join("\n", findings));

            //The input is not modified.
            Assert.Equal(json_Run, Core.Convert.ToString(run));

            //Every design input survives: the same spaces with the same Part F requirements, zones, panels and weather.
            Assert.Equal(baseline.GetSpaces().Select(x => x.Guid).OrderBy(x => x), cleaned.GetSpaces().Select(x => x.Guid).OrderBy(x => x));
            Assert.All(cleaned.GetSpaces(), x => Assert.Equal(baseline.GetSpaces().Single(y => y.Guid == x.Guid).HasValue(SpaceParameter.PartFSpaceData), x.HasValue(SpaceParameter.PartFSpaceData)));
            Assert.Equal(baseline.GetZones().Count, cleaned.GetZones().Count);
            Assert.Equal(baseline.GetPanels().Count, cleaned.GetPanels().Count);

            //The restored conditions are the baseline's: name and no airflow (a space re-guids every condition it is given).
            foreach (Space space in cleaned.GetSpaces())
            {
                InternalCondition internalCondition_Baseline = baseline.GetSpaces().Single(x => x.Guid == space.Guid).InternalCondition;
                Assert.Equal(internalCondition_Baseline?.Name, space.InternalCondition?.Name);
                Assert.False(space.InternalCondition?.HasValue(InternalConditionParameter.SupplyAirFlow) ?? false);
                Assert.False(space.InternalCondition?.HasValue(InternalConditionParameter.ExhaustAirFlow) ?? false);
            }

            //The proof that matters: mixed materialisation from the cleaned copy is the one from the baseline.
            (string, PartODwellingStrategy)[] strategies = [Mvhr(Flat1), Natural(Flat2), Mvhr(Flat3)];
            List<string> signature_Baseline = Signature(Materialise(WithStrategies(baseline, strategies)).AnalyticalModel);
            List<string> signature_Cleaned = Signature(Materialise(WithStrategies(cleaned, strategies)).AnalyticalModel);
            Assert.Equal(signature_Baseline, signature_Cleaned);
            Assert.Equal(Names(Materialise(WithStrategies(baseline, strategies)).AnalyticalModel), Names(Materialise(WithStrategies(cleaned, strategies)).AnalyticalModel));
        }

        [Fact]
        public void RemoveRunState_CleanBaseline_IsReturnedUnchanged()
        {
            AnalyticalModel baseline = WithHeldConditions(Baseline());

            AnalyticalModel cleaned = baseline.RemovePartORunState(out List<string> removed, out List<string> kept);

            Assert.Empty(removed);
            Assert.Empty(kept);
            Assert.Equal(Core.Convert.ToString(baseline), Core.Convert.ToString(cleaned));
        }

        [Fact]
        public void RemoveRunState_WithoutTheBaseConditionInTheModel_KeepsThePartFCondition_AndTheValidatorStillRefuses()
        {
            //Each space authored its own condition and the model holds no other copy, so nothing proves what the
            //Part F rates replaced.
            AnalyticalModel run = PreparedAndRun(Baseline());

            AnalyticalModel cleaned = run.RemovePartORunState(out List<string> removed, out List<string> kept);
            output.WriteLine(string.Join("\n", kept));

            Assert.NotEmpty(kept);
            Assert.All(kept, x => Assert.Contains("the model holds no condition named", x));
            Assert.Contains(removed, x => x.Contains("Part O MVHR ventilation system"));

            List<PartOMaterialisationRefusal> findings = cleaned.PartOBaselineFindings();
            PartOMaterialisationRefusal finding = Assert.Single(findings);
            Assert.Equal(PartOMaterialisationRefusalReason.MaterialisedBaseline, finding.Reason);
            Assert.Contains("ApplyPartFVentilationRates", finding.Message);

            //A kept space keeps the whole Part F condition, not half of it.
            Assert.All(cleaned.GetSpaces().Where(x => x.HasValue(SpaceParameter.PartFSpaceData)), x => Assert.True(x.InternalCondition.HasValue(InternalConditionParameter.SupplyAirFlow)));
        }

        [Fact]
        public void RemoveRunState_BaseConditionStatingAnAirflow_IsNotRestored()
        {
            //The base states an airflow basis the Part F rates would have zeroed: what the space held is unknowable.
            AnalyticalModel baseline = Baseline();
            AdjacencyCluster adjacencyCluster = baseline.AdjacencyCluster;
            foreach (Space space in adjacencyCluster.GetSpaces().Where(x => x.HasValue(SpaceParameter.PartFSpaceData)))
            {
                InternalCondition internalCondition = new(space.InternalCondition);
                internalCondition.SetValue(InternalConditionParameter.SupplyAirChangesPerHour, 0.5);
                adjacencyCluster.AddObject(new Space(space) { InternalCondition = internalCondition });
                adjacencyCluster.AddObject(new InternalCondition(internalCondition));
            }

            AnalyticalModel cleaned = PreparedAndRun(new AnalyticalModel(baseline, adjacencyCluster)).RemovePartORunState(out _, out List<string> kept);

            Assert.NotEmpty(kept);
            Assert.All(kept, x => Assert.Contains("Supply Air Changes Per Hour", x));
            Assert.Contains(cleaned.PartOBaselineFindings(), x => x.Message.Contains("ApplyPartFVentilationRates"));
        }

        [Fact]
        public void RemoveRunState_ConditionThatDiffersFromItsBase_IsNotRestored()
        {
            AnalyticalModel run = PreparedAndRun(WithHeldConditions(Baseline()));

            //An edit after preparation: the space's condition no longer agrees with the base it names.
            AdjacencyCluster adjacencyCluster = run.AdjacencyCluster;
            Space space = adjacencyCluster.GetSpaces().First(x => x.HasValue(SpaceParameter.PartFSpaceData));
            InternalCondition internalCondition = new(space.InternalCondition);
            internalCondition.SetValue(InternalConditionParameter.VentilationSystemTypeName, "Edited");
            adjacencyCluster.AddObject(new Space(space) { InternalCondition = internalCondition });

            new AnalyticalModel(run, adjacencyCluster).RemovePartORunState(out _, out List<string> kept);

            string text = Assert.Single(kept);
            Assert.Contains(space.Name, text);
            Assert.Contains("not proven to have been made from it", text);
        }

        [Fact]
        public void RemoveRunState_KeepsAuthoredMovementsAndAnAcceptedDesign()
        {
            //An authored transfer into natural Flat 2, and an accepted (2B) design on Flat 1's baseline terminals:
            //baseline content, so a model carrying them and no run state is left exactly as it is.
            AnalyticalModel baseline = WithHeldConditions(Baseline());
            AnalyticalModel source = RaisedRunCopy(baseline, out _, out _);
            PartODwellingDesignAcceptance acceptance = baseline.AcceptPartODwellingDesign(Zone(baseline, Flat1).Guid, source);
            Assert.True(acceptance.IsAccepted, acceptance.Refusal);
            AnalyticalModel accepted = WithAuthoredMovement(acceptance.AnalyticalModel, Corridor, "Flat 2 Bedroom", 5.0, out Guid guid_Movement);

            AnalyticalModel cleaned = accepted.RemovePartORunState(out List<string> removed, out List<string> kept);

            Assert.Empty(removed);
            Assert.Empty(kept);
            Assert.NotNull(cleaned.AdjacencyCluster.GetObject<SpaceAirMovement>(guid_Movement));
            Assert.Equal(Terminals(accepted, Flat1).Count, Terminals(cleaned, Flat1).Count);
            Assert.Equal(Core.Convert.ToString(accepted), Core.Convert.ToString(cleaned));
        }

        [Fact]
        public void RemoveRunState_MaterialisedMixedModel_KeepsItsRecord_AndIsStillRefused()
        {
            AnalyticalModel materialised = Materialise(WithStrategies(WithHeldConditions(Baseline()), Mvhr(Flat1), Natural(Flat2), Mvhr(Flat3))).AnalyticalModel;

            AnalyticalModel cleaned = materialised.RemovePartORunState(out _, out List<string> kept);

            Assert.Contains(kept, x => x.Contains("materialisation record"));
            Assert.Contains(cleaned.PartOBaselineFindings(), x => x.Message.Contains("materialisation record"));
        }

        /// <summary>
        /// The baseline as Map IC (TM59) leaves it: every space's condition is also held in the cluster, which is
        /// where the cleaner proves what a Part F rewrite replaced.
        /// </summary>
        private static AnalyticalModel WithHeldConditions(AnalyticalModel analyticalModel)
        {
            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;
            foreach (Space space in adjacencyCluster.GetSpaces())
            {
                if (space.InternalCondition is not null && !adjacencyCluster.Contains<InternalCondition>(space.InternalCondition.Guid))
                {
                    adjacencyCluster.AddObject(new InternalCondition(space.InternalCondition));
                }
            }

            AnalyticalModel result = new(analyticalModel, adjacencyCluster);
            Assert.True(result.IsPartOCleanBaseline(out List<PartOMaterialisationRefusal> findings), string.Join("\n", findings));

            return result;
        }

        /// <summary>
        /// What Prepare &amp; Run leaves as the open model: a legacy Iteration 1a preparation of every dwelling, its
        /// scenarios, and what TAS writes back - a provenance, results and cluster design days.
        /// </summary>
        private static AnalyticalModel PreparedAndRun(AnalyticalModel baseline)
        {
            PartOIterationPreparation preparation = baseline.PreparePartOIteration(PartOIteration.BasePassive, Zones(baseline, Flat1, Flat2, Flat3), Words(baseline, "MVHR", Flat1, Flat2, Flat3));
            Assert.Null(preparation.Refusal);

            AnalyticalModel result = new(preparation.AnalyticalModel);
            result.SetValue(AnalyticalModelParameter.OverheatingScenarios, new Core.SAMCollection<OverheatingScenario>(preparation.OverheatingScenarios));
            result.SetValue(AnalyticalModelParameter.SimulationResultProvenance, new SimulationResultProvenance(result, null));

            AdjacencyCluster adjacencyCluster = result.AdjacencyCluster;
            foreach (Space space in adjacencyCluster.GetSpaces())
            {
                SpaceSimulationResult spaceSimulationResult = new(space.Name, "Tas", space.Guid.ToString());
                adjacencyCluster.AddObject(spaceSimulationResult);
                adjacencyCluster.AddRelation(space, spaceSimulationResult);
            }

            adjacencyCluster.AddObject(new DesignDay(new DesignDay("London ANN CLG", 2018, 7, 1), LoadType.Cooling));

            return new AnalyticalModel(result, adjacencyCluster);
        }
    }
}
