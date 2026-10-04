// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Analytical.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// Mixed Part O dwelling strategies, PR3B-1: active cooling in the SAM authority
    /// (<c>documentation/PartO-MixedDwellingStrategies-PR3A.md</c> §14, owner decisions of 27 Sep 2026). A cooled
    /// dwelling is an MVHR dwelling whose selected product's manufacturer guidance is its cooling: it materialises
    /// exactly as the same dwelling uncooled, the record carries the unit, product, guidance fingerprint and cooling
    /// operating airflow, the whole model goes on the Systems route, and the dwelling is assessed as
    /// ActiveTrimCooling.
    /// </summary>
    public partial class PartODwellingStrategyMaterialisationTests
    {
        //The shipped Nuaire guidance's airflow figures (default 80, published 60-120 l/s) on the fixture's Small unit.
        private static VentilationUnitTemplate Cooling(VentilationUnitCapacityDescriptor descriptor, double default_Lps = 80.0, double minimum_Lps = 60.0, double maximum_Lps = 120.0, double capacity_Lps = double.NaN)
        {
            return new VentilationUnitTemplate(descriptor.VentilationUnitReference, "PR3B-1 fixture")
            {
                MaximumSupplyFlowRate_Lps = double.IsNaN(capacity_Lps) ? descriptor.MaximumSupplyFlowRate_Lps : capacity_Lps,
                MaximumExtractFlowRate_Lps = double.IsNaN(capacity_Lps) ? descriptor.MaximumExtractFlowRate_Lps : capacity_Lps,
                OperatingStrategy = new VentilationUnitOperatingStrategy
                {
                    Source = "PR3B-1 fixture - Nuaire-shaped figures, not a product",
                    CoolingActivationSignal = CoolingActivationSignal.RoomTemperature,
                    CoolingActivationTemperature_C = 22.0,
                    MinimumCoolingActivationTemperature_C = 22.0,
                    MaximumCoolingActivationTemperature_C = 25.0,
                    BypassMinimumIntakeTemperature_C = 12.0,
                    BypassMinimumExtractTemperature_C = 19.0,
                    DefaultElevatedAirFlow_Lps = default_Lps,
                    MinimumElevatedAirFlow_Lps = minimum_Lps,
                    MaximumElevatedAirFlow_Lps = maximum_Lps,
                    SummerBypassSupplyTemperatureRule = SupplyTemperatureRule.OutdoorAir(),
                    HeatCoolthRecoverySupplyTemperatureRule = SupplyTemperatureRule.LinearBlend(0.8),
                    CoolingSupplyTemperatureRule = SupplyTemperatureRule.ExchangerThenCoil([60.0, 80.0, 100.0, 120.0], [0.8796, 0.8576, 0.8356, 0.8136], [9.265, 8.745, 8.225, 7.705], [0.3, 0.5, 0.8, 1.1], 13.0),
                },
            };
        }

        private static (string Zone, PartODwellingStrategy Strategy) Cooled(string name_Zone, VentilationUnitReference ventilationUnitReference = null, PartODesignAirFlowBasis partODesignAirFlowBasis = PartODesignAirFlowBasis.PartFRequirement, string fingerprint = null) => (name_Zone, new PartODwellingStrategy(Guid.Empty, PartOVentilationMode.MVHR, ventilationUnitReference, PartOActiveCooling.SupplyAirCooling, partODesignAirFlowBasis, fingerprint));

        [Fact]
        public void CoolingControlRoom_PersistsThroughReopen_AndChangesStrategyIdentity()
        {
            AnalyticalModel baseline = Baseline();
            Guid bedroom = Spaces(baseline, Flat1).First().Guid;
            Guid other = Spaces(baseline, Flat1).Last().Guid;
            PartODwellingStrategy selected = new(Zone(baseline, Flat1).Guid, PartOVentilationMode.MVHR, Small.VentilationUnitReference, PartOActiveCooling.SupplyAirCooling)
            {
                CoolingStatSpaceGuid = bedroom,
            };
            string canonical = selected.CanonicalText();
            Assert.NotEqual(canonical, new PartODwellingStrategy(selected) { CoolingStatSpaceGuid = other }.CanonicalText());

            PartODwellingStrategySet set = new([selected]);
            AnalyticalModel reopened = new(WithSet(baseline, set).ToJsonObject());
            Assert.Equal(bedroom, reopened.GetValue<PartODwellingStrategySet>(AnalyticalModelParameter.PartODwellingStrategies).Strategy(selected.ZoneGuid).CoolingStatSpaceGuid);
        }

        [Fact]
        public void LegacyCoolingControlRoom_RequiresExplicitSelection_AndForeignRoomIsRefused()
        {
            AnalyticalModel baseline = Baseline();
            Guid zone = Zone(baseline, Flat1).Guid;
            PartODwellingStrategy legacy = new(zone, PartOVentilationMode.MVHR, Small.VentilationUnitReference, PartOActiveCooling.SupplyAirCooling);
            PartODwellingStrategy loaded = new(legacy.ToJsonObject());
            Assert.Equal(Guid.Empty, loaded.CoolingStatSpaceGuid);
            Assert.False(loaded.ToJsonObject().ContainsKey("CoolingStatSpaceGuid"));

            AnalyticalModel oldModel = WithSet(baseline, new PartODwellingStrategySet([loaded, new PartODwellingStrategy(Zone(baseline, Flat2).Guid, PartOVentilationMode.NaturalVentilation), new PartODwellingStrategy(Zone(baseline, Flat3).Guid, PartOVentilationMode.NaturalVentilation)]));
            AssertCoolingRefused(oldModel, [Cooling(Small)], PartOMaterialisationRefusalReason.CoolingControlRoomSelection, "no confirmed cooling control room", [Small]);

            loaded.CoolingStatSpaceGuid = Spaces(baseline, Flat2).First().Guid;
            AnalyticalModel foreign = WithSet(baseline, new PartODwellingStrategySet([loaded, new PartODwellingStrategy(Zone(baseline, Flat2).Guid, PartOVentilationMode.NaturalVentilation), new PartODwellingStrategy(Zone(baseline, Flat3).Guid, PartOVentilationMode.NaturalVentilation)]));
            AssertCoolingRefused(foreign, [Cooling(Small)], PartOMaterialisationRefusalReason.CoolingControlRoomSelection, "not one of its spaces", [Small]);
        }

        private PartOMaterialisation MaterialiseCooled(AnalyticalModel analyticalModel, IEnumerable<VentilationUnitTemplate> templates, IEnumerable<VentilationUnitCapacityDescriptor> descriptors = null)
        {
            PartOMaterialisation result = analyticalModel.MaterialisePartODwellingStrategies(descriptors ?? [Small], null, templates);
            output.WriteLine(result.Refusal ?? "(materialised)");

            Assert.True(result.IsMaterialised, result.Refusal);

            return result;
        }

        private void AssertCoolingRefused(AnalyticalModel analyticalModel, IEnumerable<VentilationUnitTemplate> templates, PartOMaterialisationRefusalReason reason, string text, IEnumerable<VentilationUnitCapacityDescriptor> descriptors = null)
        {
            PartOMaterialisation materialisation = analyticalModel.MaterialisePartODwellingStrategies(descriptors, null, templates);
            output.WriteLine(materialisation.Refusal ?? "(materialised)");

            Assert.Null(materialisation.AnalyticalModel);
            Assert.Contains(materialisation.Refusals, x => x.Reason == reason && (x.Message?.Contains(text) ?? false));
        }

        private static double DesignTotal(AnalyticalModel analyticalModel, string name_Zone)
        {
            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;
            double supply = Spaces(analyticalModel, name_Zone).Sum(x => SpaceDesignFlow(adjacencyCluster, x, FlowClassification.Supply));
            double extract = Spaces(analyticalModel, name_Zone).Sum(x => SpaceDesignFlow(adjacencyCluster, x, FlowClassification.Extract));

            return System.Math.Max(supply, extract);
        }

        // =================================================================================================
        // The cooling operating airflow (§14.2)
        // =================================================================================================

        [Theory]
        [InlineData(63.0, 63.0, 80.0)]
        [InlineData(100.0, 100.0, 100.0)]
        [InlineData(60.0, 100.0, 100.0)]
        [InlineData(120.0, 120.0, 120.0)]
        public void CoolingOperatingAirFlow_IsTheLargerOfTheDesignAndTheGuidance(double supply_Lps, double extract_Lps, double expected_Lps)
        {
            double result = Cooling(Small).PartOCoolingOperatingAirFlow(supply_Lps, extract_Lps, out string refusal);

            Assert.Null(refusal);
            Assert.Equal(expected_Lps, result, 9);
        }

        [Fact]
        public void CoolingOperatingAirFlow_BeyondThePublishedRange_IsRefused_NotExtrapolated()
        {
            //The accepted PR2 Flat 3 design: 143 l/s supply / 143 l/s extract.
            double result = Cooling(Small).PartOCoolingOperatingAirFlow(143.0, 143.0, out string refusal);

            Assert.True(double.IsNaN(result));
            Assert.Contains("beyond the manufacturer's published cooling airflow range of 60-120 l/s", refusal);
        }

        [Fact]
        public void CoolingOperatingAirFlow_IsRefused_BeyondCapacity_OutsideItsOwnRange_OrWithoutGuidance()
        {
            Assert.Contains("unit's 90 / 90 l/s supply / extract capacity", Refusal(Cooling(Small, capacity_Lps: 90.0), 95.0, 95.0));
            Assert.Contains("outside the 60 to 120 l/s its guidance states", Refusal(Cooling(Small, default_Lps: 50.0), 30.0, 30.0));
            Assert.Contains("no manufacturer operating strategy", Refusal(new VentilationUnitTemplate(Small.VentilationUnitReference, "no guidance") { MaximumSupplyFlowRate_Lps = 500, MaximumExtractFlowRate_Lps = 500 }, 30.0, 30.0));
            Assert.Contains("states no cooling airflow", Refusal(Cooling(Small, default_Lps: double.NaN), 30.0, 30.0));
            Assert.Contains("publishes no cooling airflow range", Refusal(Cooling(Small, minimum_Lps: double.NaN, maximum_Lps: double.NaN), 30.0, 30.0));

            static string Refusal(VentilationUnitTemplate template, double supply_Lps, double extract_Lps)
            {
                Assert.True(double.IsNaN(template.PartOCoolingOperatingAirFlow(supply_Lps, extract_Lps, out string refusal)));
                return refusal;
            }
        }

        // =================================================================================================
        // Identity: ActiveTrimCooling (§14.3, each value verified against SAM_Tas GroundGuidanceCooling)
        // =================================================================================================

        [Fact]
        public void ActiveTrimCooling_StatesTheManufacturerGuidanceCoolingProvision()
        {
            OverheatingOperatingAssumptions assumptions = PartOIteration.ActiveTrimCooling.PartOOperatingAssumptions(out string refusal);

            Assert.Null(refusal);
            Assert.Equal(OverheatingOperatingAssumptions.Text(false), assumptions.Value(Analytical.Query.OpeningsRestricted));
            Assert.Equal(OverheatingOperatingAssumptions.Text(true), assumptions.Value(Analytical.Query.MechanicalVentilationAtDesignRate));
            Assert.Equal(OverheatingOperatingAssumptions.Text(false), assumptions.Value(Analytical.Query.BoostAvailable));
            Assert.Equal(OverheatingOperatingAssumptions.Text(true), assumptions.Value(Analytical.Query.SummerBypassAvailable));
            Assert.Equal(Analytical.Query.ActiveCooling_SupplyAirManufacturerGuidance, assumptions.Value(Analytical.Query.ActiveCooling));

            //Never the BasePassive identity, and never the same key.
            Assert.NotEqual(PartOIteration.BasePassive.PartOOperatingAssumptions(out _).ToList(), assumptions.ToList());
        }

        // =================================================================================================
        // Materialisation
        // =================================================================================================

        [Fact]
        public void CooledDwelling_BesideUncooledMvhrAndNatural_MaterialisesOneModel_OnTheSystemsRoute()
        {
            AnalyticalModel baseline = WithStrategies(Baseline(), Cooled(Flat1), Natural(Flat2), Mvhr(Flat3));
            string json_Baseline = baseline.ToJsonObject().ToJsonString();

            PartOMaterialisation materialisation = MaterialiseCooled(baseline, [Cooling(Small)]);
            AnalyticalModel model = materialisation.AnalyticalModel;

            //One model, the Systems route, one cooled dwelling: its own unit, its product, the guidance fingerprint.
            Assert.Equal(PartOSimulationRoute.Systems, materialisation.Route);
            PartOCooledDwelling cooled = Assert.Single(materialisation.Record.CooledDwellings);
            Assert.Equal(Zone(model, Flat1).Guid, cooled.ZoneGuid);
            Assert.Equal(UnitOf(model, Flat1).Guid, cooled.AirHandlingUnitGuid);
            Assert.True(cooled.VentilationUnitReference.Matches(Small.VentilationUnitReference));
            Assert.Equal(Cooling(Small).PartOCoolingGuidanceFingerprint(), cooled.Fingerprint_Guidance);
            Assert.Equal(System.Math.Max(80.0, DesignTotal(model, Flat1)), cooled.CoolingOperatingAirFlow_Lps, 9);

            //Identities: the cooled dwelling is ActiveTrimCooling under the mechanical criterion; the others keep theirs.
            OverheatingScenario Scenario(string name) => materialisation.OverheatingScenarios.Single(x => x.ZoneGuid == Zone(model, name).Guid);
            Assert.Equal(PartOIteration.ActiveTrimCooling, Scenario(Flat1).Iteration);
            Assert.Equal("MVHR", Scenario(Flat1).VentilationStrategy);
            Assert.Equal(PartOIteration.BaseNaturalVentilation, Scenario(Flat2).Iteration);
            Assert.Equal(PartOIteration.BasePassive, Scenario(Flat3).Iteration);
            Assert.Equal(PartOIteration.DwellingIndependent, Scenario(Corridor).Iteration);

            //Nothing cooling-specific is written into the model: the unit states no supply temperature and its movement
            //no cooling profile - the cooling is the Systems route's, from the record.
            AirHandlingUnit airHandlingUnit = UnitOf(model, Flat1);
            Assert.True(double.IsNaN(airHandlingUnit.SummerSupplyTemperature));
            Assert.Null(model.AdjacencyCluster.GetRelatedObjects<AirHandlingUnitAirMovement>(airHandlingUnit).Single().Cooling);

            //The baseline is unchanged.
            Assert.Equal(json_Baseline, baseline.ToJsonObject().ToJsonString());
        }

        [Fact]
        public void CooledDwelling_HasExactlyTheEngineeringStateOfTheSameDwellingUncooled()
        {
            AnalyticalModel model_Cooled = MaterialiseCooled(WithStrategies(Baseline(), Cooled(Flat1), Natural(Flat2), Mvhr(Flat3)), [Cooling(Small)]).AnalyticalModel;
            AnalyticalModel model_Uncooled = Materialise(WithStrategies(Baseline(), Mvhr(Flat1), Natural(Flat2), Mvhr(Flat3)), [Small]).AnalyticalModel;

            Assert.Equal(Signature(model_Uncooled), Signature(model_Cooled));
            Assert.Equal(Names(model_Uncooled), Names(model_Cooled));
        }

        [Fact]
        public void RemovingCooling_RematerialisesFromTheBaseline_WithNoCoolingLeft()
        {
            AnalyticalModel baseline = Baseline();

            PartOMaterialisation cooled = MaterialiseCooled(WithStrategies(baseline, Cooled(Flat1), Natural(Flat2), Mvhr(Flat3)), [Cooling(Small)]);
            PartOMaterialisation uncooled = MaterialiseCooled(WithStrategies(baseline, Mvhr(Flat1), Natural(Flat2), Mvhr(Flat3)), [Cooling(Small)]);

            Assert.Equal(PartOSimulationRoute.Izam, uncooled.Route);
            Assert.Empty(uncooled.Record.CooledDwellings);
            Assert.Equal(PartOIteration.BasePassive, uncooled.OverheatingScenarios.Single(x => x.ZoneGuid == Zone(baseline, Flat1).Guid).Iteration);
            Assert.Equal(Signature(cooled.AnalyticalModel), Signature(uncooled.AnalyticalModel));

            //An uncooled record is written exactly as PR1 wrote it.
            JsonObject json = uncooled.Record.ToJsonObject();
            Assert.Equal(PartOMaterialisationRecord.Schema, json["Schema"].GetValue<string>());
            Assert.False(json.ContainsKey("Route"));
            Assert.False(json.ContainsKey("CooledDwellings"));
        }

        [Fact]
        public void CooledOptimisedDwelling_CoolsAtTheLargerOfItsRetainedDesignAndTheGuidance()
        {
            AnalyticalModel accepted = WithAcceptedRaisedDesign(Baseline(), out _, out _);
            string fingerprint = accepted.AdjacencyCluster.PartODwellingDesignFingerprint(Zone(accepted, Flat1));

            PartOMaterialisation materialisation = MaterialiseCooled(WithStrategies(accepted, Cooled(Flat1, null, PartODesignAirFlowBasis.RetainedDesign, fingerprint), Natural(Flat2), Natural(Flat3)), [Cooling(Small)]);

            PartOCooledDwelling cooled = Assert.Single(materialisation.Record.CooledDwellings);
            Assert.Equal(System.Math.Max(80.0, DesignTotal(materialisation.AnalyticalModel, Flat1)), cooled.CoolingOperatingAirFlow_Lps, 9);

            //The retained design is the design: the terminals carry it, the cooling airflow is not written onto them.
            Assert.Equal(fingerprint, materialisation.AnalyticalModel.AdjacencyCluster.PartODwellingDesignFingerprint(Zone(materialisation.AnalyticalModel, Flat1)));
        }

        [Fact]
        public void CooledDwelling_WhoseDesignExceedsThePublishedCoolingRange_IsRefused()
        {
            //A published range that ends below this dwelling's design: the cooling would have no data.
            AnalyticalModel baseline = WithStrategies(Baseline(), Cooled(Flat1), Natural(Flat2), Natural(Flat3));
            double design_Lps = DesignTotal(Materialise(WithStrategies(Baseline(), Mvhr(Flat1), Natural(Flat2), Natural(Flat3))).AnalyticalModel, Flat1);

            AssertCoolingRefused(baseline, [Cooling(Small, default_Lps: 1.0, minimum_Lps: 1.0, maximum_Lps: design_Lps - 1.0)], PartOMaterialisationRefusalReason.CoolingAirFlowOutsideGuidance, "published cooling airflow range", [Small]);
        }

        [Fact]
        public void Cooling_WithoutProductGuidance_IsRefused()
        {
            AnalyticalModel baseline = WithStrategies(Baseline(), Cooled(Flat1), Natural(Flat2), Natural(Flat3));

            //A generic unit - no catalogue offered.
            AssertCoolingRefused(baseline, [Cooling(Small)], PartOMaterialisationRefusalReason.CoolingWithoutProductGuidance, "has no selected product");

            //A product, but no guidance offered for it, or an entry that states none.
            AssertCoolingRefused(baseline, null, PartOMaterialisationRefusalReason.CoolingWithoutProductGuidance, "manufacturer's cooling guidance", [Small]);
            AssertCoolingRefused(baseline, [new VentilationUnitTemplate(Small.VentilationUnitReference, "no guidance")], PartOMaterialisationRefusalReason.CoolingWithoutProductGuidance, "manufacturer's cooling guidance", [Small]);

            //Guidance for a different product is not this product's.
            AssertCoolingRefused(baseline, [Cooling(Large)], PartOMaterialisationRefusalReason.CoolingWithoutProductGuidance, "manufacturer's cooling guidance", [Small]);
        }

        [Fact]
        public void NaturalWithCooling_AndAReusedConditionedUnit_StayRefused()
        {
            AssertCoolingRefused(WithStrategies(Baseline(), Mvhr(Flat1), new PartODwellingStrategy(Guid.Empty, PartOVentilationMode.NaturalVentilation, null, PartOActiveCooling.SupplyAirCooling).For(Flat2), Natural(Flat3)), [Cooling(Small)], PartOMaterialisationRefusalReason.NaturalWithCooling, "naturally ventilated with active supply-air cooling", [Small]);

            //An authored supply setpoint is conditioning nobody selected - refused for a cooled dwelling too.
            AssertCoolingRefused(WithStrategies(WithAuthoredUnit(Baseline(), 18.0, out _), Cooled(Flat1), Natural(Flat2), Natural(Flat3)), [Cooling(Small)], PartOMaterialisationRefusalReason.ConditionedReusedUnit, "never an authored setpoint", [Small]);
        }

        [Fact]
        public void CooledDwelling_CannotReachTheCorridorOrANeighbour_ThroughAuthoredTransferOrSharedPlant()
        {
            //Authored transfer air from the cooled dwelling into the corridor.
            AnalyticalModel transfer = WithAuthoredMovement(Baseline(), "Living Room", Corridor, 10.0, out _);
            AssertCoolingRefused(WithStrategies(transfer, Cooled(Flat1), Natural(Flat2), Natural(Flat3)), [Cooling(Small)], PartOMaterialisationRefusalReason.AuthoredAirMovementConflict, "reaches MVHR dwelling 'Flat 1'", [Small]);

            //Every movement and system of the cooled dwelling's unit stays inside it (the post-materialisation invariant
            //holds on the accepted case, and the corridor is served by nothing).
            AnalyticalModel model = MaterialiseCooled(WithStrategies(Baseline(), Cooled(Flat1), Mvhr(Flat2), Natural(Flat3)), [Cooling(Small)]).AnalyticalModel;
            Assert.Empty(SystemsServing(model, Corridor));
            Assert.Empty(AirMovementsTouching(model, Corridor));
        }

        [Fact]
        public void TwoCooledDwellings_AreIndependent_AndTheMaterialisationIsDeterministic()
        {
            AnalyticalModel baseline = WithStrategies(Baseline(), Cooled(Flat1), Cooled(Flat2), Mvhr(Flat3));

            PartOMaterialisation materialisation_1 = MaterialiseCooled(new AnalyticalModel(baseline.ToJsonObject()), [Cooling(Small)]);
            PartOMaterialisation materialisation_2 = MaterialiseCooled(new AnalyticalModel(baseline.ToJsonObject()), [Cooling(Small)]);

            Assert.Equal(2, materialisation_1.Record.CooledDwellings.Count);
            Assert.NotEqual(materialisation_1.Record.CooledDwellings[0].AirHandlingUnitGuid, materialisation_1.Record.CooledDwellings[1].AirHandlingUnitGuid);
            Assert.True(materialisation_1.Record.CooledDwellings[0].ZoneGuid.CompareTo(materialisation_1.Record.CooledDwellings[1].ZoneGuid) < 0);

            string Engineering(PartOCooledDwelling x) => string.Join("|", x.ZoneGuid, x.VentilationUnitReference, x.CoolingOperatingAirFlow_Lps, x.Fingerprint_Guidance);
            Assert.Equal(materialisation_1.Record.CooledDwellings.Select(Engineering), materialisation_2.Record.CooledDwellings.Select(Engineering));
            Assert.Equal(materialisation_1.OverheatingScenarios.Select(x => x.Key).OrderBy(x => x), materialisation_2.OverheatingScenarios.Select(x => x.Key).OrderBy(x => x));
            Assert.Equal(Signature(materialisation_1.AnalyticalModel), Signature(materialisation_2.AnalyticalModel));
        }

        // =================================================================================================
        // The record: route, schema, staleness
        // =================================================================================================

        [Fact]
        public void CooledRecord_RoundTripsAsV2_AndIsCurrentOnlyWithUnchangedGuidance()
        {
            AnalyticalModel baseline = WithStrategies(Baseline(), Cooled(Flat1), Natural(Flat2), Mvhr(Flat3));
            PartOMaterialisationRecord record = MaterialiseCooled(baseline, [Cooling(Small)]).Record;

            JsonObject json = record.ToJsonObject();
            Assert.Equal(PartOMaterialisationRecord.Schema_Cooled, json["Schema"].GetValue<string>());
            Assert.Equal("Systems", json["Route"].GetValue<string>());

            PartOMaterialisationRecord reopened = new(JsonNode.Parse(json.ToJsonString()).AsObject());
            Assert.True(reopened.IsValid);
            Assert.Equal(PartOSimulationRoute.Systems, reopened.Route);
            Assert.Equal(json.ToJsonString(), reopened.ToJsonObject().ToJsonString());

            Assert.True(reopened.IsCurrent(baseline, [Small], [Cooling(Small)], out string reason), reason);

            //Without the guidance it cannot be shown current; with a changed figure it is stale.
            Assert.False(reopened.IsCurrent(baseline, [Small], out reason));
            Assert.Contains("cooling guidance", reason);
            Assert.False(reopened.IsCurrent(baseline, [Small], [Cooling(Small, default_Lps: 90.0)], out reason));
            Assert.Contains("cooling guidance", reason);

            //Switching the cooling off is a strategy change.
            Assert.False(reopened.IsCurrent(WithStrategies(Baseline(), Mvhr(Flat1), Natural(Flat2), Mvhr(Flat3)), [Small], [Cooling(Small)], out reason));
            Assert.Contains("strategy", reason);
        }

        [Fact]
        public void CooledRecord_ThatContradictsItsSchema_IsInvalid()
        {
            JsonObject json = MaterialiseCooled(WithStrategies(Baseline(), Cooled(Flat1), Natural(Flat2), Mvhr(Flat3)), [Cooling(Small)]).Record.ToJsonObject();

            JsonObject json_NoCooled = JsonNode.Parse(json.ToJsonString()).AsObject();
            json_NoCooled["CooledDwellings"] = new JsonArray();
            Assert.False(new PartOMaterialisationRecord(json_NoCooled).IsValid);

            JsonObject json_Izam = JsonNode.Parse(json.ToJsonString()).AsObject();
            json_Izam["Route"] = "Izam";
            Assert.False(new PartOMaterialisationRecord(json_Izam).IsValid);

            JsonObject json_Unreadable = JsonNode.Parse(json.ToJsonString()).AsObject();
            json_Unreadable["CooledDwellings"][0]["Fingerprint_Guidance"] = null;
            Assert.False(new PartOMaterialisationRecord(json_Unreadable).IsValid);
        }

        [Fact]
        public void InvalidCooledRecord_StaysInvalidWhenSavedAgain_NeverDowngradedToV1()
        {
            AnalyticalModel baseline = WithStrategies(Baseline(), Cooled(Flat1), Natural(Flat2), Mvhr(Flat3));
            JsonObject json = MaterialiseCooled(baseline, [Cooling(Small)]).Record.ToJsonObject();

            //A truncated v2 record (no cooled dwellings) and one stating the IZAM route: each is invalid, and so is
            //every later save of it.
            JsonObject json_NoCooled = JsonNode.Parse(json.ToJsonString()).AsObject();
            json_NoCooled.Remove("CooledDwellings");

            JsonObject json_Izam = JsonNode.Parse(json.ToJsonString()).AsObject();
            json_Izam["Route"] = "Izam";

            foreach (JsonObject json_Invalid in new[] { json_NoCooled, json_Izam })
            {
                PartOMaterialisationRecord record = new(json_Invalid);
                Assert.False(record.IsValid);

                JsonObject json_Saved = new PartOMaterialisationRecord(record).ToJsonObject();
                Assert.Equal(PartOMaterialisationRecord.Schema_Cooled, json_Saved["Schema"].GetValue<string>());

                PartOMaterialisationRecord reopened = new(JsonNode.Parse(json_Saved.ToJsonString()).AsObject());
                Assert.False(reopened.IsValid);
                Assert.False(reopened.IsCurrent(baseline, [Small], [Cooling(Small)], out _));
            }
        }

        [Fact]
        public void CooledRecord_WithAnAlteredCoolingAirFlow_IsNotCurrent()
        {
            AnalyticalModel baseline = WithStrategies(Baseline(), Cooled(Flat1), Natural(Flat2), Mvhr(Flat3));
            JsonObject json = MaterialiseCooled(baseline, [Cooling(Small)]).Record.ToJsonObject();

            double coolingOperatingAirFlow_Lps = json["CooledDwellings"][0]["CoolingOperatingAirFlow_Lps"].GetValue<double>();
            json["CooledDwellings"][0]["CoolingOperatingAirFlow_Lps"] = coolingOperatingAirFlow_Lps + 20.0;

            PartOMaterialisationRecord reopened = new(JsonNode.Parse(json.ToJsonString()).AsObject());
            Assert.True(reopened.IsValid);
            Assert.False(reopened.IsCurrent(baseline, [Small], [Cooling(Small)], out string reason));
            Assert.Contains("cooling operating airflow", reason);
        }

        [Fact]
        public void CoolingAirFlow_BeyondTheSelectedDescriptorsCapacity_IsRefused_WhateverTheTemplateStates()
        {
            AnalyticalModel baseline = WithStrategies(Baseline(), Cooled(Flat1), Natural(Flat2), Natural(Flat3));
            double design_Lps = DesignTotal(Materialise(WithStrategies(Baseline(), Mvhr(Flat1), Natural(Flat2), Natural(Flat3))).AnalyticalModel, Flat1);

            //The catalogue entry that selects the unit covers the design with 1 l/s to spare; its template states a far
            //larger capacity and guidance 10 l/s above the design - more than the selected unit can move.
            VentilationUnitCapacityDescriptor tight = new(Small.VentilationUnitReference, design_Lps + 1.0, design_Lps + 1.0);
            VentilationUnitTemplate template = Cooling(tight, default_Lps: design_Lps + 10.0, minimum_Lps: 1.0, maximum_Lps: design_Lps + 100.0, capacity_Lps: design_Lps + 100.0);

            AssertCoolingRefused(baseline, [template], PartOMaterialisationRefusalReason.CoolingAirFlowOutsideGuidance, "selected unit's", [tight]);
        }
    }

}
