// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Analytical.Enums;
using SAM.Core;
using SAM.Weather;
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// <b>The Part O iteration slice: stating a mitigation stage, and being assessed at it.</b>
    /// <para>
    /// <c>PartOIteration</c> has always been identity-only - its own documentation says no member of it causes
    /// any behaviour anywhere. That left a hole: "BasePassive" was a label a caller could attach, but the
    /// operating assumptions that make base provision <i>base provision</i> had to be hand-populated, so two
    /// engineers stating the same assessment derived two different keys. These tests cover the stage being
    /// stated canonically and carried through to a TM59 answer.
    /// </para>
    /// <para>
    /// <b>What is deliberately NOT claimed here.</b> Stating a stage does not make a simulation obey it. Nothing
    /// in this slice writes aperture control profiles or ventilation rates into a TBD, so a BasePassive scenario
    /// asserts that the model was assessed as base provision - not that the model was <i>built</i> as base
    /// provision. That remains the modeller's responsibility and is separate, unwritten work.
    /// </para>
    /// </summary>
    public class PartOIterationSliceTests
    {
        private const string key_Tas_ResultantTemperature = "Resultant Temperature";
        private const string key_Tas_OccupantSensibleGain = "Occupant Sensible Gain";

        private static readonly string[] flats = ["Flat 1", "Flat 2", "Flat 3"];

        // -------------------------------------------------------------------------------------------------
        // The stage's assumptions
        // -------------------------------------------------------------------------------------------------

        /// <summary>BasePassive states the base provision: openings unrestricted, mechanical ventilation at rate.</summary>
        [Fact]
        public void BasePassive_StatesTheBaseProvision()
        {
            OverheatingOperatingAssumptions overheatingOperatingAssumptions = PartOIteration.BasePassive.PartOOperatingAssumptions(out string refusal);

            Assert.Null(refusal);
            Assert.NotNull(overheatingOperatingAssumptions);

            Assert.Equal(OverheatingOperatingAssumptions.Text(false), overheatingOperatingAssumptions.Value(Analytical.Query.OpeningsRestricted));
            Assert.Equal(OverheatingOperatingAssumptions.Text(true), overheatingOperatingAssumptions.Value(Analytical.Query.MechanicalVentilationAtDesignRate));
            Assert.Equal(OverheatingOperatingAssumptions.Text(false), overheatingOperatingAssumptions.Value(Analytical.Query.BoostAvailable));
            Assert.Equal(OverheatingOperatingAssumptions.Text(false), overheatingOperatingAssumptions.Value(Analytical.Query.SummerBypassAvailable));
        }

        /// <summary>
        /// <b>The point of stating a stage canonically: two independent statements of the same assessment derive
        /// the same key.</b> Before this, each caller populated the assumptions itself, so they did not.
        /// </summary>
        [Fact]
        public void TheSameStageStatedTwice_DerivesOneKey()
        {
            Guid guid_Zone = Guid.NewGuid();

            OverheatingScenario overheatingScenario_1 = Scenario(guid_Zone, PartOIteration.BasePassive, "MVRE");
            OverheatingScenario overheatingScenario_2 = Scenario(guid_Zone, PartOIteration.BasePassive, "MVRE");

            Assert.Equal(overheatingScenario_1.Key, overheatingScenario_2.Key);
        }

        /// <summary>
        /// <b>Two stages over the same dwelling are two assessments, not one computed twice.</b> The whole reason
        /// the iteration is in the key - and now the assumptions differ too, so it is not resting on the enum
        /// alone.
        /// </summary>
        [Fact]
        public void BasePassiveAndAcousticRestricted_AreDifferentAssessments()
        {
            Guid guid_Zone = Guid.NewGuid();

            OverheatingScenario overheatingScenario_Base = Scenario(guid_Zone, PartOIteration.BasePassive, "MVRE");
            OverheatingScenario overheatingScenario_Acoustic = Scenario(guid_Zone, PartOIteration.AcousticRestricted, "MVRE");

            Assert.NotEqual(overheatingScenario_Base.Key, overheatingScenario_Acoustic.Key);

            //And the difference is in the assumptions, not only the enum name.
            Assert.Equal(OverheatingOperatingAssumptions.Text(false), overheatingScenario_Base.OperatingAssumptions.Value(Analytical.Query.OpeningsRestricted));
            Assert.Equal(OverheatingOperatingAssumptions.Text(true), overheatingScenario_Acoustic.OperatingAssumptions.Value(Analytical.Query.OpeningsRestricted));
            Assert.Equal(OverheatingOperatingAssumptions.Text(true), overheatingScenario_Acoustic.OperatingAssumptions.Value(Analytical.Query.SummerBypassAvailable));
        }

        /// <summary>
        /// <b>An uncharacterised stage refuses rather than returning an empty set.</b> An empty set is a valid
        /// statement - "nothing assumed" - so returning one would derive a perfectly good key for a stage nobody
        /// has characterised, which is worse than failing.
        /// </summary>
        [Fact]
        public void AnUnknownStage_IsRefusedRatherThanGuessedAt()
        {
            OverheatingOperatingAssumptions overheatingOperatingAssumptions = ((PartOIteration)99).PartOOperatingAssumptions(out string refusal);

            Assert.Null(overheatingOperatingAssumptions);
            Assert.False(string.IsNullOrWhiteSpace(refusal));
        }

        /// <summary>
        /// <c>Undefined</c> is not a refusal: a scenario built to exercise the machinery is entitled to say it
        /// states no stage.
        /// </summary>
        [Fact]
        public void Undefined_StatesNothingWithoutRefusing()
        {
            OverheatingOperatingAssumptions overheatingOperatingAssumptions = PartOIteration.Undefined.PartOOperatingAssumptions(out string refusal);

            Assert.Null(refusal);
            Assert.NotNull(overheatingOperatingAssumptions);
            Assert.Equal(0, overheatingOperatingAssumptions.Count);
        }

        // -------------------------------------------------------------------------------------------------
        // The scenario set
        // -------------------------------------------------------------------------------------------------

        /// <summary>
        /// The set classifies flats as dwellings and the communal corridor as common space - from the dwelling
        /// marking, not the name, and with every scenario stating the same stage.
        /// </summary>
        [Fact]
        public void TheScenarioSet_TellsAFlatFromACorridor()
        {
            AnalyticalModel analyticalModel = Model_Design();

            List<OverheatingScenario> overheatingScenarios = Analytical.Create.OverheatingScenarios(analyticalModel.GetZones(), PartOIteration.BasePassive, Strategies(analyticalModel), out List<string> refusals);

            Assert.Empty(refusals);
            Assert.Equal(4, overheatingScenarios.Count);

            foreach (OverheatingScenario overheatingScenario in overheatingScenarios)
            {
                Zone zone = analyticalModel.GetZones().Find(x => x.Guid == overheatingScenario.ZoneGuid);

                PartOAssessmentScope expected = zone.Name == "Corridor" ? PartOAssessmentScope.CommonSpace : PartOAssessmentScope.Dwelling;

                Assert.Equal(expected, overheatingScenario.Scope);
                Assert.Equal(PartOIteration.BasePassive, overheatingScenario.Iteration);
            }

            //Four zones, four distinct assessments - the corridor is not attributed to a flat.
            Assert.Equal(4, new HashSet<Guid>(overheatingScenarios.ConvertAll(x => x.Key)).Count);
        }

        /// <summary>
        /// A zone stating no ventilation strategy is <b>refused</b>. A silent default is the defect this replaced:
        /// it assessed a mechanically ventilated dwelling against the natural-ventilation criterion.
        /// </summary>
        [Fact]
        public void AZoneWithNoVentilationStrategy_IsRefused()
        {
            AnalyticalModel analyticalModel = Model_Design();

            Dictionary<Guid, string> dictionary = Strategies(analyticalModel);
            dictionary.Remove(analyticalModel.GetZones().Find(x => x.Name == "Flat 2").Guid);

            List<OverheatingScenario> overheatingScenarios = Analytical.Create.OverheatingScenarios(analyticalModel.GetZones(), PartOIteration.BasePassive, dictionary, out List<string> refusals);

            Assert.Single(refusals);
            Assert.Equal(3, overheatingScenarios.Count);
        }

        /// <summary>
        /// <b>An unmarked zone beside marked ones is refused, not quietly assessed as a corridor.</b>
        /// <para>
        /// The classification query calls everything that is not a dwelling a common space, which is right for
        /// classifying a marked-up model. Applied blindly to scenario creation it would assess an unmarked bedroom
        /// against the corridor criterion, so this layer refuses instead and names the missing marking.
        /// </para>
        /// </summary>
        [Fact]
        public void AnUnmarkedZoneBesideMarkedOnes_IsRefused()
        {
            AnalyticalModel analyticalModel = Model_Design(unmarked: "Flat 3");

            List<OverheatingScenario> overheatingScenarios = Analytical.Create.OverheatingScenarios(analyticalModel.GetZones(), PartOIteration.BasePassive, Strategies(analyticalModel), out List<string> refusals);

            Assert.Single(refusals);
            Assert.Equal(3, overheatingScenarios.Count);

            //And it was not silently given the corridor's scope.
            Guid guid_Zone = analyticalModel.GetZones().Find(x => x.Name == "Flat 3").Guid;
            Assert.Null(overheatingScenarios.Find(x => x.ZoneGuid == guid_Zone));
        }

        /// <summary>An uncharacterised stage produces no scenarios at all, rather than a set that assumes nothing.</summary>
        [Fact]
        public void AnUncharacterisedStage_ProducesNoScenarios()
        {
            AnalyticalModel analyticalModel = Model_Design();

            List<OverheatingScenario> overheatingScenarios = Analytical.Create.OverheatingScenarios(analyticalModel.GetZones(), (PartOIteration)99, Strategies(analyticalModel), out List<string> refusals);

            Assert.Empty(overheatingScenarios);
            Assert.Single(refusals);
        }

        // -------------------------------------------------------------------------------------------------
        // The vertical slice
        // -------------------------------------------------------------------------------------------------

        /// <summary>
        /// <b>The BasePassive slice end to end, on the TSD-simple route.</b>
        /// <code>
        /// zones -> Analytical.Create.OverheatingScenarios(BasePassive) -> OverheatingScenarioMap
        ///       -> TM59AssessmentCalculator -> TM59 results, each attributed to its own scenario
        /// </code>
        /// <para>
        /// This is the sequence <c>Tas.TSDQueryTM59Results</c> runs once the new component feeds it scenarios,
        /// exercised over the real failure shape: three flats whose room is called <i>exactly</i>
        /// <c>"Bedroom 2"</c>, plus the communal corridor. The design data states the <b>opposite</b> ventilation
        /// strategy in every case, so a result on the expected criterion can only have come from the scenario.
        /// </para>
        /// </summary>
        [Fact]
        public void TheBasePassiveSlice_ProducesTM59ResultsAttributedToTheirOwnScenarios()
        {
            AnalyticalModel analyticalModel_Design = Model_Design();
            AnalyticalModel analyticalModel_TSD = Model_TSD();

            //Flat 1 MVRE, Flat 2 NV, Flat 3 MVRE, corridor UV - and the internal conditions say otherwise.
            List<OverheatingScenario> overheatingScenarios = Analytical.Create.OverheatingScenarios(analyticalModel_Design.GetZones(), PartOIteration.BasePassive, Strategies(analyticalModel_Design), out List<string> refusals);

            Assert.Empty(refusals);

            SimulationSpaceMap simulationSpaceMap = new(analyticalModel_Design.GetSpaces(), analyticalModel_TSD.GetSpaces(), StableKeyOf);

            OverheatingScenarioMap overheatingScenarioMap = new(overheatingScenarios, analyticalModel_Design, simulationSpaceMap);

            Assert.True(overheatingScenarioMap.IsComplete);
            Assert.Empty(overheatingScenarioMap.Refusals);

            TM59AssessmentCalculator tM59AssessmentCalculator = new(analyticalModel_TSD, analyticalModel_Design, simulationSpaceMap)
            {
                ResultantTemperatureSeriesKey = key_Tas_ResultantTemperature,
                OccupancySensibleGainSeriesKey = key_Tas_OccupantSensibleGain,
                VentilationStrategyMap = overheatingScenarioMap.VentilationStrategyMap,
            };

            Assert.True(tM59AssessmentCalculator.RestoreDesignInternalConditions());
            Assert.Empty(tM59AssessmentCalculator.AssociationRefusals);

            TM59AssessmentResult tM59AssessmentResult = tM59AssessmentCalculator.Calculate(tM59AssessmentCalculator.Spaces(null, null));

            Assert.NotNull(tM59AssessmentResult);
            Assert.Empty(tM59AssessmentResult.VentilationStrategyRefusals);

            //The criterion each room was assessed against is the SCENARIO's, not the model's.
            Assert.Equal(2, tM59AssessmentResult.MechanicalVentilationResults.Count);
            Assert.Single(tM59AssessmentResult.NaturalVentilationResults);
            Assert.Single(tM59AssessmentResult.CorridorResults);

            //And every result goes back to the scenario that asked for it - three same-named bedrooms included.
            Dictionary<OverheatingScenario, List<TMResult>> dictionary = overheatingScenarioMap.Associate(tM59AssessmentResult, out List<TMResult> tMResults_Unassociated);

            Assert.Empty(tMResults_Unassociated);
            Assert.Equal(4, dictionary.Count);

            foreach (OverheatingScenario overheatingScenario in overheatingScenarios)
            {
                Assert.Single(dictionary[overheatingScenario]);
                Assert.Equal(PartOIteration.BasePassive, overheatingScenario.Iteration);
            }
        }

        /// <summary>
        /// The 2026-09-29 real project, as SAM sees it: a Prepare &amp; Run names only the three DWELLING zones, and the
        /// communal corridor - its own zone, <c>IsDwelling = false</c>, assigned exactly the TM59 communal-corridor
        /// condition, simulated in the whole building - reached no scenario. TM59 then refused it ("No overheating
        /// scenario covers space") and the report said "COMMUNAL CORRIDOR RISK: -". With the iteration-neutral corridor
        /// scenario the preparation now states beside the dwelling scenarios, the corridor gets a communal-corridor
        /// result and the report files it under the corridor criterion, not as supplementary information.
        /// </summary>
        [Fact]
        public void DwellingOnlyScenarios_PlusTheNeutralCorridorScenario_GiveTheCorridorACommunalCorridorResult()
        {
            AnalyticalModel analyticalModel_Design = Model_Design(corridorCondition: TM59InternalConditionResolver.CommunalCorridorInternalConditionName);
            AnalyticalModel analyticalModel_TSD = Model_TSD();

            List<Zone> zones_Dwelling = analyticalModel_Design.GetZones().FindAll(x => x.Name != "Corridor");
            Zone zone_Corridor = analyticalModel_Design.GetZones().Find(x => x.Name == "Corridor");

            List<OverheatingScenario> overheatingScenarios_Dwelling = Analytical.Create.OverheatingScenarios(zones_Dwelling, PartOIteration.BasePassive, Strategies(analyticalModel_Design), out List<string> refusals);
            Assert.Empty(refusals);

            //Before: the dwelling-only set leaves the corridor with no strategy at all.
            TM59AssessmentResult tM59AssessmentResult_Before = Calculate(analyticalModel_Design, analyticalModel_TSD, overheatingScenarios_Dwelling, out TM59AssessmentReport tM59AssessmentReport_Before);
            Assert.Empty(tM59AssessmentResult_Before.CorridorResults);
            Assert.Contains(tM59AssessmentResult_Before.VentilationStrategyRefusals, x => x.Contains("No overheating scenario covers space 'Corridor'"));
            Assert.Empty(tM59AssessmentReport_Before.CorridorChecks);

            //After: the same set plus the corridor's neutral scenario.
            List<OverheatingScenario> overheatingScenarios = [.. overheatingScenarios_Dwelling, Analytical.Create.PartOCommonSpaceOverheatingScenario(zone_Corridor)];

            TM59AssessmentResult tM59AssessmentResult = Calculate(analyticalModel_Design, analyticalModel_TSD, overheatingScenarios, out TM59AssessmentReport tM59AssessmentReport);

            Assert.Empty(tM59AssessmentResult.VentilationStrategyRefusals);
            Assert.Single(tM59AssessmentResult.CorridorResults);
            Assert.Equal(2, tM59AssessmentResult.MechanicalVentilationResults.Count);
            Assert.Single(tM59AssessmentResult.NaturalVentilationResults);

            TM59AssessmentReportCheck corridorCheck = Assert.Single(tM59AssessmentReport.CorridorChecks);
            Assert.Equal("Corridor", corridorCheck.SpaceName);
            Assert.Empty(tM59AssessmentReport.SupplementaryChecks);
            Assert.NotEqual(TM59RiskStatus.Undefined, tM59AssessmentReport.CorridorRiskStatus);
        }

        private static TM59AssessmentResult Calculate(AnalyticalModel analyticalModel_Design, AnalyticalModel analyticalModel_TSD, List<OverheatingScenario> overheatingScenarios, out TM59AssessmentReport tM59AssessmentReport)
        {
            SimulationSpaceMap simulationSpaceMap = new(analyticalModel_Design.GetSpaces(), analyticalModel_TSD.GetSpaces(), StableKeyOf);

            OverheatingScenarioMap overheatingScenarioMap = new(overheatingScenarios, analyticalModel_Design, simulationSpaceMap);

            TM59AssessmentCalculator tM59AssessmentCalculator = new(analyticalModel_TSD, analyticalModel_Design, simulationSpaceMap)
            {
                ResultantTemperatureSeriesKey = key_Tas_ResultantTemperature,
                OccupancySensibleGainSeriesKey = key_Tas_OccupantSensibleGain,
                VentilationStrategyMap = overheatingScenarioMap.VentilationStrategyMap,
            };

            Assert.True(tM59AssessmentCalculator.RestoreDesignInternalConditions());

            List<Space> spaces = tM59AssessmentCalculator.Spaces(null, null);
            TM59AssessmentResult tM59AssessmentResult = tM59AssessmentCalculator.Calculate(spaces);

            Assert.NotNull(tM59AssessmentResult);

            //The report classifies by the DESIGN space's assigned condition, as the production review does.
            tM59AssessmentReport = new TM59AssessmentReport(tM59AssessmentResult, "test");

            return tM59AssessmentResult;
        }

        /// <summary>
        /// <b>The same fabric at two stages gives two separately attributable answers.</b> Which is the whole
        /// reason an iteration is part of the identity: a building is tested at base provision first, and the
        /// mitigated run has to be tellable apart from it.
        /// </summary>
        [Fact]
        public void TwoStagesOverOneBuilding_StayTellableApart()
        {
            AnalyticalModel analyticalModel_Design = Model_Design();

            List<OverheatingScenario> overheatingScenarios_Base = Analytical.Create.OverheatingScenarios(analyticalModel_Design.GetZones(), PartOIteration.BasePassive, Strategies(analyticalModel_Design), out List<string> refusals_Base);
            List<OverheatingScenario> overheatingScenarios_Acoustic = Analytical.Create.OverheatingScenarios(analyticalModel_Design.GetZones(), PartOIteration.AcousticRestricted, Strategies(analyticalModel_Design), out List<string> refusals_Acoustic);

            Assert.Empty(refusals_Base);
            Assert.Empty(refusals_Acoustic);

            HashSet<Guid> keys = new(overheatingScenarios_Base.ConvertAll(x => x.Key));

            foreach (OverheatingScenario overheatingScenario in overheatingScenarios_Acoustic)
            {
                Assert.DoesNotContain(overheatingScenario.Key, keys);
            }

            //Eight assessments over four zones, all distinct.
            keys.UnionWith(overheatingScenarios_Acoustic.ConvertAll(x => x.Key));
            Assert.Equal(8, keys.Count);
        }

        // -------------------------------------------------------------------------------------------------
        // Fixture
        // -------------------------------------------------------------------------------------------------

        private static OverheatingScenario Scenario(Guid guid_Zone, PartOIteration partOIteration, string ventilationStrategy)
        {
            return new OverheatingScenario(
                PartOAssessmentScope.Dwelling,
                guid_Zone,
                partOIteration,
                new SystemTemplate(ventilationStrategy, null, null, null, null, null),
                partOIteration.PartOOperatingAssumptions(out string _));
        }

        private static Dictionary<Guid, string> Strategies(AnalyticalModel analyticalModel)
        {
            Dictionary<Guid, string> result = [];

            foreach (KeyValuePair<string, string> keyValuePair in new Dictionary<string, string> { { "Flat 1", "MVRE" }, { "Flat 2", "NV" }, { "Flat 3", "MVRE" }, { "Corridor", "UV" } })
            {
                result[analyticalModel.GetZones().Find(x => x.Name == keyValuePair.Key).Guid] = keyValuePair.Value;
            }

            return result;
        }

        /// <summary>The identity the TAS workflow stamps, restated here so the test needs no TAS assembly.</summary>
        private static string StableKeyOf(Space space)
        {
            return space != null && space.TryGetValue("Zone Guid", out string result) ? result : null;
        }

        private static string StableKey(string name)
        {
            int index = Array.IndexOf(flats, name);

            return index == -1 ? (name == "Corridor" ? "tas-zone-corridor" : null) : "tas-zone-" + index;
        }

        /// <summary>
        /// Three flats each with a room called exactly "Bedroom 2", plus a communal corridor. Flats are marked as
        /// dwellings and the corridor explicitly is not; internal conditions state the OPPOSITE ventilation
        /// strategy to the scenarios, so the criterion cannot be tracking the model's own data.
        /// </summary>
        private static AnalyticalModel Model_Design(string unmarked = null, string corridorCondition = null)
        {
            AdjacencyCluster adjacencyCluster = new();

            foreach (string name in new[] { "Flat 1", "Flat 2", "Flat 3", "Corridor" })
            {
                Space space = new(name == "Corridor" ? "Corridor" : "Bedroom 2");

                InternalCondition internalCondition = new(name == "Corridor" && corridorCondition is not null ? corridorCondition : name + " IC");
                internalCondition.SetValue(InternalConditionParameter.VentilationSystemTypeName, name == "Flat 2" ? "MVRE" : "NV");

                space.InternalCondition = internalCondition;
                space.SetValue("Zone Guid", StableKey(name));

                Zone zone = new(name);

                if (name != unmarked)
                {
                    zone.SetValue(ZoneParameter.IsDwelling, name != "Corridor");
                }

                adjacencyCluster.AddObject(space);
                adjacencyCluster.AddObject(zone);
                adjacencyCluster.AddRelation(zone, space);
            }

            return new AnalyticalModel("Three Flats", null, null, null, adjacencyCluster);
        }

        /// <summary>The model as a TSD read rebuilds it: fresh spaces, hourly series, no internal conditions.</summary>
        private static AnalyticalModel Model_TSD()
        {
            AdjacencyCluster adjacencyCluster = new();

            foreach (string name in new[] { "Flat 1", "Flat 2", "Flat 3", "Corridor" })
            {
                Space space = new(name == "Corridor" ? "Corridor" : "Bedroom 2");

                space.SetValue("Zone Guid", StableKey(name));

                ParameterSet parameterSet = new("SAM.Analytical.Tas.dll");
                parameterSet.Add(key_Tas_ResultantTemperature, Values([21.0, 24.5, 27.5, 29.0]));
                parameterSet.Add(key_Tas_OccupantSensibleGain, Values([0, 80.0, 80.0, 0]));

                space.Add(parameterSet);

                adjacencyCluster.AddObject(space);
            }

            AnalyticalModel result = new("Three Flats", null, null, null, adjacencyCluster);

            result.SetValue(AnalyticalModelParameter.WeatherData, new WeatherData("Test", "Test", 51.5, -0.1, 0, WeatherYear()));

            return result;
        }

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
