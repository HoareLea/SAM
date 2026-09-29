// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Analytical.Enums;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// The communal corridor on the HOMOGENEOUS Part O route (2026-09-29 real project). A Prepare &amp; Run names only
    /// its dwelling zones, so a correctly classified corridor - its own zone, <c>IsDwelling = false</c>, assigned
    /// exactly the TM59 communal-corridor condition - reached no overheating scenario and TM59 refused it. The
    /// preparation now states the same iteration-neutral corridor scenario the mixed route states, by the same rule.
    /// </summary>
    public partial class PartODwellingStrategyMaterialisationTests
    {
        [Fact]
        public void HomogeneousPreparation_StatesTheCorridorsIterationNeutralScenario()
        {
            AnalyticalModel baseline = Baseline();

            PartOIterationPreparation preparation = baseline.PreparePartOIteration(PartOIteration.BasePassive, Zones(baseline, Flat1, Flat2), Words(baseline, "MVHR", Flat1, Flat2));

            Assert.Null(preparation.Refusal);

            AnalyticalModel model = preparation.AnalyticalModel;
            Zone corridor = Zone(model, Corridor);

            OverheatingScenario overheatingScenario = preparation.OverheatingScenarios.Single(x => x.ZoneGuid == corridor.Guid);
            Assert.Equal(PartOAssessmentScope.CommonSpace, overheatingScenario.Scope);
            Assert.Equal(PartOIteration.DwellingIndependent, overheatingScenario.Iteration);
            Assert.Equal("UV", overheatingScenario.VentilationStrategy);

            //The same identity the mixed route states for the same corridor - one corridor, one key.
            Assert.Equal(Analytical.Create.PartOCommonSpaceOverheatingScenario(corridor).Key, overheatingScenario.Key);

            //The dwellings keep their own scenarios, and one map carries both without conflict.
            Assert.Equal(2, preparation.OverheatingScenarios.Count(x => x.Scope == PartOAssessmentScope.Dwelling && x.Iteration == PartOIteration.BasePassive));

            OverheatingScenarioMap overheatingScenarioMap = new(preparation.OverheatingScenarios, model, SimulationSpaceMap.Identity(model.GetSpaces()));
            Assert.Empty(overheatingScenarioMap.Refusals);
            Assert.All(Spaces(model, Corridor), x => Assert.Equal("UV", overheatingScenarioMap.VentilationStrategyMap.Selection(x).VentilationStrategy));
            Assert.All(Spaces(model, Flat1), x => Assert.Equal("MVHR", overheatingScenarioMap.VentilationStrategyMap.Selection(x).VentilationStrategy));

            //Never by marking the corridor a dwelling.
            Assert.False(corridor.TryGetValue(ZoneParameter.IsDwelling, out bool isDwelling) && isDwelling);
        }

        [Fact]
        public void HomogeneousPreparation_DoesNotRestateACorridorTheCallerNamed()
        {
            AnalyticalModel baseline = Baseline();

            //The caller names the corridor itself, stating the route every other named zone states (a homogeneous
            //preparation refuses a zone that states none): it already has its scenario, and must not get a second.
            Dictionary<System.Guid, string> words = Words(baseline, "MVHR", Flat1, Flat2, Corridor);

            PartOIterationPreparation preparation = baseline.PreparePartOIteration(PartOIteration.BasePassive, Zones(baseline, Flat1, Flat2, Corridor), words);

            Assert.Null(preparation.Refusal);
            Assert.Single(preparation.OverheatingScenarios, x => x.ZoneGuid == Zone(baseline, Corridor).Guid);
        }

        [Fact]
        public void HomogeneousPreparation_GivesNoScenarioToACommonZoneWithoutTheCorridorCondition()
        {
            AnalyticalModel baseline = Baseline(corridorCondition: "Corridor IC");

            PartOIterationPreparation preparation = baseline.PreparePartOIteration(PartOIteration.BasePassive, Zones(baseline, Flat1, Flat2), Words(baseline, "MVHR", Flat1, Flat2));

            Assert.Null(preparation.Refusal);
            Assert.DoesNotContain(preparation.OverheatingScenarios, x => x.ZoneGuid == Zone(baseline, Corridor).Guid);
        }

        [Fact]
        public void HomogeneousPreparation_WarnsRatherThanGuessesForAMixedCommonZone()
        {
            AnalyticalModel baseline = Baseline();
            AdjacencyCluster adjacencyCluster = baseline.AdjacencyCluster;
            Space store = new("Corridor Store") { InternalCondition = new InternalCondition("Store IC") };
            adjacencyCluster.AddObject(store);
            adjacencyCluster.AddRelation(adjacencyCluster.GetObjects<Zone>().Find(x => x.Name == Corridor), store);
            baseline = new AnalyticalModel(baseline, adjacencyCluster);

            PartOIterationPreparation preparation = baseline.PreparePartOIteration(PartOIteration.BasePassive, Zones(baseline, Flat1, Flat2), Words(baseline, "MVHR", Flat1, Flat2));

            Assert.Null(preparation.Refusal);
            Assert.DoesNotContain(preparation.OverheatingScenarios, x => x.ZoneGuid == Zone(baseline, Corridor).Guid);
            Assert.Contains(preparation.Warnings, x => x.Contains("mixes space(s) assigned") && x.Contains(Corridor));
        }

        [Fact]
        public void IsolatedPreparation_StatesNoCorridorScenario()
        {
            AnalyticalModel baseline = Baseline();

            PartOIterationPreparation preparation = baseline.PreparePartOIteration(PartOIteration.BasePassive, Zones(baseline, Flat1, Flat2), Words(baseline, "MVHR", Flat1, Flat2), null, true);

            Assert.Null(preparation.Refusal);
            Assert.DoesNotContain(preparation.OverheatingScenarios, x => x.Scope == PartOAssessmentScope.CommonSpace);
        }

        [Fact]
        public void TheCorridorZoneRule_IsTheAssignedCondition()
        {
            Space corridor = new("Level 1 Lobby") { InternalCondition = new InternalCondition(TM59InternalConditionResolver.CommunalCorridorInternalConditionName) };
            Space named = new("Corridor") { InternalCondition = new InternalCondition("Corridor IC") };

            Assert.True(Analytical.Query.IsTM59CommunalCorridorZone([corridor]));
            Assert.False(Analytical.Query.IsTM59CommunalCorridorZone([named]));
            Assert.False(Analytical.Query.IsTM59CommunalCorridorZone([]));
            Assert.Null(Analytical.Query.IsTM59CommunalCorridorZone([corridor, named]));
        }
    }
}
