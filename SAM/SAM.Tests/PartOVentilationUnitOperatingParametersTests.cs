// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Analytical.Enums;
using SAM.Core;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// <b>Approved Document O Iteration 3, PR5A: a selected product's certified heat recovery efficiency
    /// and specific fan power, and the COM-free resolver that reads them at a unit's design duty.</b>
    /// <para>
    /// The vocabulary is optional catalogue data on <see cref="VentilationUnitTemplate"/> -
    /// <see cref="HeatRecoveryPerformance"/> and <see cref="FanPerformance"/>, both typed views of the
    /// existing <see cref="VentilationUnitPerformanceTable"/> grammar - and the resolver is
    /// <see cref="Query.VentilationUnitOperatingParameters(VentilationUnitTemplate, double, double, double)"/>.
    /// What this suite pins, above all, is that <b>missing data refuses</b>: it is never a zero, never a
    /// system template's efficiency, never an extrapolation, and never a silent fallback.
    /// </para>
    /// <para>
    /// The four airflows stay apart, and one test below proves it on a model:
    /// </para>
    /// <code>
    /// PartFRequiredAirFlow != DesignAirFlow != SelectedEquipmentCapacity != OperatingAirFlow
    /// </code>
    /// <para>
    /// <b>Every product here is a fixture.</b> No real manufacturer is named and no real figure is
    /// transcribed - the certified data for the first canonical product has not been obtained, and the
    /// shipped catalogue and its figures belong to <c>SAM_Systems</c>.
    /// </para>
    /// </summary>
    public class PartOVentilationUnitOperatingParametersTests
    {
        private const string source_Fixture = "Test Fixture, Certified Performance, v.1 - not a real product";

        private static readonly double[] airFlowRates_Lps = [30.0, 60.0, 90.0, 150.0];
        private static readonly double[] efficiencies = [0.90, 0.86, 0.82, 0.74];
        private static readonly double[] specificFanPowers_WPerLps = [0.50, 0.62, 0.80, 1.40];

        // =================================================================================================
        // A. Backward compatibility - a catalogue written before these fields reads exactly as it did
        // =================================================================================================

        /// <summary>
        /// A catalogue entry in the v1 shape - no heat recovery, no fan performance - still parses, is still
        /// a valid selectable template, and states no behaviour data rather than a default one.
        /// </summary>
        [Fact]
        public void AnOldCatalogueEntry_StillParses_AndStatesNoBehaviourData()
        {
            const string json = """
                {
                  "_type": "SAM.Analytical.VentilationUnitTemplate,SAM.Analytical",
                  "Name": "Test Fixture UNIT-V1 (R1)",
                  "VentilationUnitReference": {
                    "_type": "SAM.Analytical.VentilationUnitReference,SAM.Analytical",
                    "Name": "Test Fixture UNIT-V1",
                    "Manufacturer": "Test Fixture",
                    "Model": "UNIT-V1",
                    "Reference": "R1"
                  },
                  "Source": "Test Fixture, Performance Data, v.1 - not a real product",
                  "MaximumSupplyFlowRate_Lps": 150,
                  "MaximumExtractFlowRate_Lps": 150,
                  "Rank": 10
                }
                """;

            VentilationUnitTemplate ventilationUnitTemplate = new((JsonObject)JsonNode.Parse(json)!);

            Assert.True(ventilationUnitTemplate.IsValid);
            Assert.True(ventilationUnitTemplate.HasSelectionCapacity);
            Assert.Null(ventilationUnitTemplate.HeatRecoveryPerformance);
            Assert.Null(ventilationUnitTemplate.FanPerformance);

            JsonObject jsonObject = ventilationUnitTemplate.ToJsonObject();
            Assert.False(jsonObject.ContainsKey("HeatRecoveryPerformance"));
            Assert.False(jsonObject.ContainsKey("FanPerformance"));

            VentilationUnitTemplate ventilationUnitTemplate_RoundTripped = Helpers.RoundTrip.Once(ventilationUnitTemplate);
            Assert.Null(ventilationUnitTemplate_RoundTripped.HeatRecoveryPerformance);
            Assert.Null(ventilationUnitTemplate_RoundTripped.FanPerformance);

            //And what it does not state, it does not resolve.
            VentilationUnitOperatingParameters ventilationUnitOperatingParameters = ventilationUnitTemplate.VentilationUnitOperatingParameters(60, 60);
            Assert.False(ventilationUnitOperatingParameters.IsHeatRecoveryResolved);
            Assert.False(ventilationUnitOperatingParameters.IsFanPerformanceResolved);
        }

        /// <summary>
        /// Adding the fields changes nothing else about how a template serializes: take the new keys back
        /// out and the text is exactly what the template wrote before it had them.
        /// </summary>
        [Fact]
        public void TheNewFields_AddKeysAndChangeNothingElse()
        {
            VentilationUnitTemplate ventilationUnitTemplate = Template("UNIT-A");
            ventilationUnitTemplate.HeatRecoveryPerformance = null;
            ventilationUnitTemplate.FanPerformance = null;

            string json_Before = ventilationUnitTemplate.ToJsonObject().ToJsonString();

            ventilationUnitTemplate.HeatRecoveryPerformance = HeatRecovery();
            ventilationUnitTemplate.FanPerformance = Fan();

            JsonObject jsonObject_After = ventilationUnitTemplate.ToJsonObject();
            Assert.True(jsonObject_After.Remove("HeatRecoveryPerformance"));
            Assert.True(jsonObject_After.Remove("FanPerformance"));

            Assert.Equal(json_Before, jsonObject_After.ToJsonString());
        }

        // =================================================================================================
        // B. The new data - round trip, copy, and missing stays missing
        // =================================================================================================

        /// <summary>Every certified figure, its basis, its policy and its source survive a round trip exactly.</summary>
        [Fact]
        public void TheNewFields_SurviveSerializationExactly()
        {
            VentilationUnitTemplate ventilationUnitTemplate = Template("UNIT-A");
            ventilationUnitTemplate.FanPerformance = Fan(PerformanceDomainPolicy.ClampToDomain);

            VentilationUnitTemplate ventilationUnitTemplate_RoundTripped = Helpers.RoundTrip.Once(ventilationUnitTemplate);

            HeatRecoveryPerformance heatRecoveryPerformance = ventilationUnitTemplate_RoundTripped.HeatRecoveryPerformance;
            Assert.True(heatRecoveryPerformance.IsValid);
            Assert.Equal(airFlowRates_Lps, heatRecoveryPerformance.AirFlowRates_Lps);
            Assert.Equal(efficiencies, heatRecoveryPerformance.Values);
            Assert.Equal(HeatRecoveryEfficiencyBasis.SupplyTemperatureEfficiency, heatRecoveryPerformance.HeatRecoveryEfficiencyBasis);
            Assert.Equal(PerformanceDomainPolicy.Refuse, heatRecoveryPerformance.PerformanceDomainPolicy);
            Assert.Equal(source_Fixture, heatRecoveryPerformance.Source);

            FanPerformance fanPerformance = ventilationUnitTemplate_RoundTripped.FanPerformance;
            Assert.True(fanPerformance.IsValid);
            Assert.Equal(airFlowRates_Lps, fanPerformance.AirFlowRates_Lps);
            Assert.Equal(specificFanPowers_WPerLps, fanPerformance.Values);
            Assert.Equal(SpecificFanPowerBasis.TotalBothFans, fanPerformance.SpecificFanPowerBasis);
            Assert.Equal(PerformanceDomainPolicy.ClampToDomain, fanPerformance.PerformanceDomainPolicy);
            Assert.Equal(source_Fixture, fanPerformance.Source);

            //Standalone too, through the same _type dispatch a catalogue read uses.
            Assert.Equal(efficiencies, Helpers.RoundTrip.Once(HeatRecovery()).Values);
            Assert.Equal(specificFanPowers_WPerLps, Helpers.RoundTrip.Once(Fan()).Values);
        }

        /// <summary>A copied template carries its own copies of the new data - equal in every figure, and not shared.</summary>
        [Fact]
        public void ACopy_PreservesTheNewFields_WithoutSharingThem()
        {
            VentilationUnitTemplate ventilationUnitTemplate = Template("UNIT-A");

            VentilationUnitTemplate ventilationUnitTemplate_Copy = new(ventilationUnitTemplate);

            Assert.NotSame(ventilationUnitTemplate.HeatRecoveryPerformance, ventilationUnitTemplate_Copy.HeatRecoveryPerformance);
            Assert.NotSame(ventilationUnitTemplate.FanPerformance, ventilationUnitTemplate_Copy.FanPerformance);

            Assert.Equal(efficiencies, ventilationUnitTemplate_Copy.HeatRecoveryPerformance.Values);
            Assert.Equal(HeatRecoveryEfficiencyBasis.SupplyTemperatureEfficiency, ventilationUnitTemplate_Copy.HeatRecoveryPerformance.HeatRecoveryEfficiencyBasis);
            Assert.Equal(specificFanPowers_WPerLps, ventilationUnitTemplate_Copy.FanPerformance.Values);
            Assert.Equal(SpecificFanPowerBasis.TotalBothFans, ventilationUnitTemplate_Copy.FanPerformance.SpecificFanPowerBasis);

            //Changing the original afterwards leaves the copy alone.
            ventilationUnitTemplate.HeatRecoveryPerformance = null;
            ventilationUnitTemplate.FanPerformance = Fan(values: [0.9, 1.0, 1.1, 1.2]);

            Assert.Equal(efficiencies, ventilationUnitTemplate_Copy.HeatRecoveryPerformance.Values);
            Assert.Equal(specificFanPowers_WPerLps, ventilationUnitTemplate_Copy.FanPerformance.Values);

            //And the published arrays handed out are copies too.
            double[] values = ventilationUnitTemplate_Copy.HeatRecoveryPerformance.Values;
            values[0] = 0;
            Assert.Equal(efficiencies, ventilationUnitTemplate_Copy.HeatRecoveryPerformance.Values);
        }

        /// <summary>
        /// One field stated and the other not: after a round trip the missing one is still missing - null,
        /// not zero - and it refuses while the stated one resolves.
        /// </summary>
        [Fact]
        public void MissingData_StaysMissingThroughARoundTrip()
        {
            VentilationUnitTemplate ventilationUnitTemplate = Template("UNIT-A");
            ventilationUnitTemplate.FanPerformance = null;

            VentilationUnitTemplate ventilationUnitTemplate_RoundTripped = Helpers.RoundTrip.Once(ventilationUnitTemplate);

            Assert.NotNull(ventilationUnitTemplate_RoundTripped.HeatRecoveryPerformance);
            Assert.Null(ventilationUnitTemplate_RoundTripped.FanPerformance);

            VentilationUnitOperatingParameters ventilationUnitOperatingParameters = ventilationUnitTemplate_RoundTripped.VentilationUnitOperatingParameters(60, 60);

            Assert.True(ventilationUnitOperatingParameters.IsHeatRecoveryResolved);
            Assert.False(ventilationUnitOperatingParameters.IsFanPerformanceResolved);
            Assert.True(double.IsNaN(ventilationUnitOperatingParameters.SpecificFanPower_WPerLps));
            Assert.NotEqual(0, ventilationUnitOperatingParameters.SpecificFanPower_WPerLps);
        }

        /// <summary>
        /// A file that says nothing about the domain policy gets the strict one. A file that states a
        /// policy nobody recognises, or one that would extrapolate, gets nothing at all - a typo must never
        /// become a clamp.
        /// </summary>
        [Fact]
        public void TheDomainPolicy_DefaultsToRefuse_AndAnythingElseMustBeStatedExactly()
        {
            JsonObject jsonObject = HeatRecovery().ToJsonObject();

            jsonObject.Remove("PerformanceDomainPolicy");
            HeatRecoveryPerformance heatRecoveryPerformance_Absent = new(jsonObject);
            Assert.True(heatRecoveryPerformance_Absent.IsValid);
            Assert.Equal(PerformanceDomainPolicy.Refuse, heatRecoveryPerformance_Absent.PerformanceDomainPolicy);

            jsonObject["PerformanceDomainPolicy"] = "Clmap";
            Assert.False(new HeatRecoveryPerformance(jsonObject).IsValid);

            jsonObject["PerformanceDomainPolicy"] = PerformanceDomainPolicy.OuterCellLinearExtrapolation.ToString();
            Assert.False(new HeatRecoveryPerformance(jsonObject).IsValid);

            jsonObject["PerformanceDomainPolicy"] = PerformanceDomainPolicy.ClampToDomain.ToString();
            Assert.True(new HeatRecoveryPerformance(jsonObject).IsValid);
        }

        /// <summary>There is no default basis: a file that omits it, or names one nobody knows, is refused.</summary>
        [Fact]
        public void TheBasis_HasNoDefault()
        {
            JsonObject jsonObject_HeatRecovery = HeatRecovery().ToJsonObject();
            jsonObject_HeatRecovery.Remove("HeatRecoveryEfficiencyBasis");
            Assert.False(new HeatRecoveryPerformance(jsonObject_HeatRecovery).IsValid);

            jsonObject_HeatRecovery["HeatRecoveryEfficiencyBasis"] = "EnthalpyEfficiency";
            Assert.False(new HeatRecoveryPerformance(jsonObject_HeatRecovery).IsValid);

            JsonObject jsonObject_Fan = Fan().ToJsonObject();
            jsonObject_Fan.Remove("SpecificFanPowerBasis");
            Assert.False(new FanPerformance(jsonObject_Fan).IsValid);

            jsonObject_Fan["SpecificFanPowerBasis"] = "PerFan";
            Assert.False(new FanPerformance(jsonObject_Fan).IsValid);
        }

        // =================================================================================================
        // C. Resolving a reference - identity, never a name, never catalogue order
        // =================================================================================================

        /// <summary>
        /// A reference resolves the product it names among products that share a manufacturer, a model, or
        /// both - the <c>Reference</c> field is part of the identity.
        /// </summary>
        [Fact]
        public void AReference_ResolvesTheProductItNames()
        {
            List<VentilationUnitTemplate> ventilationUnitTemplates =
            [
                Template("UNIT-A", "R1", [0.90, 0.86, 0.82, 0.74]),
                Template("UNIT-A", "R2", [0.80, 0.76, 0.72, 0.64]),
                Template("UNIT-B", "R1", [0.70, 0.66, 0.62, 0.54]),
            ];

            Assert.Equal(0.86, Resolve(ventilationUnitTemplates, Reference("UNIT-A", "R1"), 60).SensibleHeatRecoveryEfficiency);
            Assert.Equal(0.76, Resolve(ventilationUnitTemplates, Reference("UNIT-A", "R2"), 60).SensibleHeatRecoveryEfficiency);
            Assert.Equal(0.66, Resolve(ventilationUnitTemplates, Reference("UNIT-B", "R1"), 60).SensibleHeatRecoveryEfficiency);

            Assert.True(Reference("UNIT-A", "R2").Matches(Resolve(ventilationUnitTemplates, Reference("UNIT-A", "R2"), 60).VentilationUnitReference));
        }

        /// <summary>
        /// <b>No name matching.</b> A catalogue entry that has been given another product's display name is
        /// still found - and still not found - by its identity fields alone.
        /// </summary>
        [Fact]
        public void ResolutionIgnoresDisplayNames()
        {
            VentilationUnitTemplate ventilationUnitTemplate_A = Template("UNIT-A", "R1", [0.90, 0.86, 0.82, 0.74]);

            JsonObject jsonObject_B = Template("UNIT-B", "R1", [0.70, 0.66, 0.62, 0.54]).ToJsonObject();
            jsonObject_B["Name"] = ventilationUnitTemplate_A.Name;
            ((JsonObject)jsonObject_B["VentilationUnitReference"]!)["Name"] = ventilationUnitTemplate_A.VentilationUnitReference.Name;

            VentilationUnitTemplate ventilationUnitTemplate_B = new(jsonObject_B);

            Assert.Equal(ventilationUnitTemplate_A.Name, ventilationUnitTemplate_B.Name);
            Assert.Equal(ventilationUnitTemplate_A.VentilationUnitReference.Name, ventilationUnitTemplate_B.VentilationUnitReference.Name);

            List<VentilationUnitTemplate> ventilationUnitTemplates = [ventilationUnitTemplate_B, ventilationUnitTemplate_A];

            Assert.Equal(0.86, Resolve(ventilationUnitTemplates, Reference("UNIT-A", "R1"), 60).SensibleHeatRecoveryEfficiency);
            Assert.Equal(0.66, Resolve(ventilationUnitTemplates, Reference("UNIT-B", "R1"), 60).SensibleHeatRecoveryEfficiency);
        }

        /// <summary>Every ordering of the same catalogue gives the same answer, field for field.</summary>
        [Fact]
        public void CatalogueOrder_DoesNotChangeTheResult()
        {
            VentilationUnitTemplate ventilationUnitTemplate_1 = Template("UNIT-A", "R1", [0.90, 0.86, 0.82, 0.74]);
            VentilationUnitTemplate ventilationUnitTemplate_2 = Template("UNIT-A", "R2", [0.80, 0.76, 0.72, 0.64]);
            VentilationUnitTemplate ventilationUnitTemplate_3 = Template("UNIT-B", "R1", [0.70, 0.66, 0.62, 0.54]);

            List<List<VentilationUnitTemplate>> orders =
            [
                [ventilationUnitTemplate_1, ventilationUnitTemplate_2, ventilationUnitTemplate_3],
                [ventilationUnitTemplate_1, ventilationUnitTemplate_3, ventilationUnitTemplate_2],
                [ventilationUnitTemplate_2, ventilationUnitTemplate_1, ventilationUnitTemplate_3],
                [ventilationUnitTemplate_2, ventilationUnitTemplate_3, ventilationUnitTemplate_1],
                [ventilationUnitTemplate_3, ventilationUnitTemplate_1, ventilationUnitTemplate_2],
                [ventilationUnitTemplate_3, ventilationUnitTemplate_2, ventilationUnitTemplate_1],
            ];

            foreach (VentilationUnitReference ventilationUnitReference in new[] { Reference("UNIT-A", "R1"), Reference("UNIT-A", "R2"), Reference("UNIT-B", "R1"), Reference("UNIT-Z", "R1") })
            {
                string snapshot = Snapshot(Resolve(orders[0], ventilationUnitReference, 45));

                foreach (List<VentilationUnitTemplate> order in orders)
                {
                    Assert.Equal(snapshot, Snapshot(Resolve(order, ventilationUnitReference, 45)));
                }
            }
        }

        /// <summary>
        /// A product the catalogue does not hold, a catalogue holding it twice, no catalogue, and no
        /// selection all refuse both quantities - and nothing is reselected in their place.
        /// </summary>
        [Fact]
        public void AnUnresolvableReference_RefusesBothQuantities()
        {
            List<VentilationUnitTemplate> ventilationUnitTemplates = [Template("UNIT-A"), Template("UNIT-B")];

            VentilationUnitOperatingParameters unknown = Resolve(ventilationUnitTemplates, Reference("UNIT-Z"), 60);
            AssertBothRefused(unknown, "not among the ventilation unit templates supplied");
            Assert.Contains("No other product is substituted", unknown.HeatRecoveryRefusal);
            Assert.True(Reference("UNIT-Z").Matches(unknown.VentilationUnitReference));

            AssertBothRefused(Resolve([Template("UNIT-A"), Template("UNIT-A")], Reference("UNIT-A"), 60), "2 entries");
            AssertBothRefused(Resolve(null, Reference("UNIT-A"), 60), "not among the ventilation unit templates supplied");
            AssertBothRefused(Resolve(ventilationUnitTemplates, null, 60), "No ventilation unit product is selected");
            AssertBothRefused(Resolve(ventilationUnitTemplates, new VentilationUnitReference(), 60), "No ventilation unit product is selected");
        }

        // =================================================================================================
        // D. Operating-point lookup - exact, interpolated, and never extrapolated
        // =================================================================================================

        /// <summary>At a published airflow the published figure comes back exactly - not to six places.</summary>
        [Fact]
        public void AnExactOperatingPoint_ReturnsThePublishedFigureExactly()
        {
            VentilationUnitOperatingParameters ventilationUnitOperatingParameters = Template("UNIT-A").VentilationUnitOperatingParameters(90, 90);

            Assert.True(ventilationUnitOperatingParameters.IsHeatRecoveryResolved);
            Assert.True(ventilationUnitOperatingParameters.IsFanPerformanceResolved);

            Assert.Equal(0.82, ventilationUnitOperatingParameters.SensibleHeatRecoveryEfficiency);
            Assert.Equal(0.80, ventilationUnitOperatingParameters.SpecificFanPower_WPerLps);

            Assert.Equal(HeatRecoveryEfficiencyBasis.SupplyTemperatureEfficiency, ventilationUnitOperatingParameters.HeatRecoveryEfficiencyBasis);
            Assert.Equal(SpecificFanPowerBasis.TotalBothFans, ventilationUnitOperatingParameters.SpecificFanPowerBasis);
            Assert.False(ventilationUnitOperatingParameters.HeatRecoveryClampedToDomain);
            Assert.False(ventilationUnitOperatingParameters.FanPerformanceClampedToDomain);

            //Both ends of the published range are inside it.
            Assert.Equal(0.90, Template("UNIT-A").VentilationUnitOperatingParameters(30, 30).SensibleHeatRecoveryEfficiency);
            Assert.Equal(1.40, Template("UNIT-A").VentilationUnitOperatingParameters(150, 150).SpecificFanPower_WPerLps);
        }

        /// <summary>Between two published airflows the answer is the straight line between them, hand-checkable.</summary>
        [Fact]
        public void BetweenOperatingPoints_TheAnswerIsLinear()
        {
            VentilationUnitOperatingParameters ventilationUnitOperatingParameters_45 = Template("UNIT-A").VentilationUnitOperatingParameters(45, 45);

            //Halfway from 30 to 60: 0.90 + 0.5 * (0.86 - 0.90), 0.50 + 0.5 * (0.62 - 0.50).
            Assert.Equal(0.88, ventilationUnitOperatingParameters_45.SensibleHeatRecoveryEfficiency, 12);
            Assert.Equal(0.56, ventilationUnitOperatingParameters_45.SpecificFanPower_WPerLps, 12);
            Assert.False(ventilationUnitOperatingParameters_45.HeatRecoveryClampedToDomain);

            //A quarter of the way from 90 to 150: 0.82 + 0.25 * (0.74 - 0.82), 0.80 + 0.25 * (1.40 - 0.80).
            VentilationUnitOperatingParameters ventilationUnitOperatingParameters_105 = Template("UNIT-A").VentilationUnitOperatingParameters(105, 105);
            Assert.Equal(0.80, ventilationUnitOperatingParameters_105.SensibleHeatRecoveryEfficiency, 12);
            Assert.Equal(0.95, ventilationUnitOperatingParameters_105.SpecificFanPower_WPerLps, 12);
        }

        /// <summary>
        /// <b>Below and above the published operating points, the default is to refuse.</b> A design duty
        /// the manufacturer never certified gets no figure at all.
        /// </summary>
        [Fact]
        public void BelowAndAboveThePublishedRange_Refuses()
        {
            foreach (double airFlowRate_Lps in new[] { 29.9, 20.0, 150.1, 200.0 })
            {
                VentilationUnitOperatingParameters ventilationUnitOperatingParameters = Template("UNIT-A").VentilationUnitOperatingParameters(airFlowRate_Lps, airFlowRate_Lps);

                AssertBothRefused(ventilationUnitOperatingParameters, "lies outside that");
                Assert.Contains("nothing is extrapolated", ventilationUnitOperatingParameters.HeatRecoveryRefusal);
                Assert.Contains("published for 30 to 150 l/s", ventilationUnitOperatingParameters.FanPerformanceRefusal);
            }
        }

        /// <summary>
        /// Holding the edge happens only where the data itself says so, and the result says it happened -
        /// the figure is then the manufacturer's statement about its edge, not a measurement at this duty.
        /// </summary>
        [Fact]
        public void AClampStatedByTheData_HoldsThePublishedEdge_AndSaysSo()
        {
            VentilationUnitTemplate ventilationUnitTemplate = Template("UNIT-A");
            ventilationUnitTemplate.HeatRecoveryPerformance = HeatRecovery(PerformanceDomainPolicy.ClampToDomain);

            VentilationUnitOperatingParameters above = ventilationUnitTemplate.VentilationUnitOperatingParameters(200, 200);
            Assert.True(above.IsHeatRecoveryResolved);
            Assert.Equal(0.74, above.SensibleHeatRecoveryEfficiency);
            Assert.True(above.HeatRecoveryClampedToDomain);

            VentilationUnitOperatingParameters below = ventilationUnitTemplate.VentilationUnitOperatingParameters(20, 20);
            Assert.Equal(0.90, below.SensibleHeatRecoveryEfficiency);
            Assert.True(below.HeatRecoveryClampedToDomain);

            //The fan data states nothing beyond its range, so it still refuses at the same duty.
            Assert.False(above.IsFanPerformanceResolved);
            Assert.False(above.FanPerformanceClampedToDomain);
        }

        /// <summary>
        /// A single certified point answers at exactly that point and nowhere else - unless the data states
        /// a clamp, in which case it is one flat, flagged figure.
        /// </summary>
        [Fact]
        public void ASinglePointTable_AnswersOnlyAtItsPoint()
        {
            VentilationUnitTemplate ventilationUnitTemplate = Template("UNIT-A");
            ventilationUnitTemplate.HeatRecoveryPerformance = new HeatRecoveryPerformance([60.0], [0.85], HeatRecoveryEfficiencyBasis.SupplyTemperatureEfficiency, source_Fixture);
            ventilationUnitTemplate.FanPerformance = new FanPerformance([60.0], [0.70], SpecificFanPowerBasis.TotalBothFans, source_Fixture, PerformanceDomainPolicy.ClampToDomain);

            VentilationUnitOperatingParameters exact = ventilationUnitTemplate.VentilationUnitOperatingParameters(60, 60);
            Assert.Equal(0.85, exact.SensibleHeatRecoveryEfficiency);
            Assert.Equal(0.70, exact.SpecificFanPower_WPerLps);
            Assert.False(exact.FanPerformanceClampedToDomain);

            VentilationUnitOperatingParameters off = ventilationUnitTemplate.VentilationUnitOperatingParameters(61, 61);
            Assert.False(off.IsHeatRecoveryResolved);
            Assert.Contains("published for 60 to 60 l/s", off.HeatRecoveryRefusal);
            Assert.Equal(0.70, off.SpecificFanPower_WPerLps);
            Assert.True(off.FanPerformanceClampedToDomain);
        }

        /// <summary>An empty table is not a table of zeros. It is malformed, and refused.</summary>
        [Fact]
        public void AnEmptyTable_Refuses()
        {
            VentilationUnitTemplate ventilationUnitTemplate = Template("UNIT-A");
            ventilationUnitTemplate.HeatRecoveryPerformance = new HeatRecoveryPerformance([], [], HeatRecoveryEfficiencyBasis.SupplyTemperatureEfficiency, source_Fixture);
            ventilationUnitTemplate.FanPerformance = new FanPerformance(null!, null!, SpecificFanPowerBasis.TotalBothFans, source_Fixture);

            Assert.False(ventilationUnitTemplate.HeatRecoveryPerformance.IsValid);
            Assert.Null(ventilationUnitTemplate.HeatRecoveryPerformance.Values);

            AssertBothRefused(ventilationUnitTemplate.VentilationUnitOperatingParameters(60, 60), "missing or malformed");
        }

        // =================================================================================================
        // E. Missing data - never zero, never a fallback, and the two quantities stand alone
        // =================================================================================================

        /// <summary>
        /// <b>No heat recovery data is not 0 efficiency</b> - and not a system template's 0.7 either. It
        /// refuses heat recovery, and only heat recovery: the fan data still resolves.
        /// </summary>
        [Fact]
        public void MissingHeatRecoveryData_RefusesHeatRecoveryOnly()
        {
            VentilationUnitTemplate ventilationUnitTemplate = Template("UNIT-A");
            ventilationUnitTemplate.HeatRecoveryPerformance = null;

            VentilationUnitOperatingParameters ventilationUnitOperatingParameters = ventilationUnitTemplate.VentilationUnitOperatingParameters(60, 60);

            Assert.False(ventilationUnitOperatingParameters.IsHeatRecoveryResolved);
            Assert.True(double.IsNaN(ventilationUnitOperatingParameters.SensibleHeatRecoveryEfficiency));
            Assert.Equal(HeatRecoveryEfficiencyBasis.Undefined, ventilationUnitOperatingParameters.HeatRecoveryEfficiencyBasis);
            Assert.Contains("states no heat recovery performance", ventilationUnitOperatingParameters.HeatRecoveryRefusal);
            Assert.Contains("Missing is not a zero efficiency", ventilationUnitOperatingParameters.HeatRecoveryRefusal);
            Assert.Contains("no generic or system-template value is put in its place", ventilationUnitOperatingParameters.HeatRecoveryRefusal);

            Assert.True(ventilationUnitOperatingParameters.IsFanPerformanceResolved);
            Assert.Equal(0.62, ventilationUnitOperatingParameters.SpecificFanPower_WPerLps);
        }

        /// <summary>No fan data is not 0 W/(l/s). It refuses fan performance, and only fan performance.</summary>
        [Fact]
        public void MissingFanData_RefusesFanPerformanceOnly()
        {
            VentilationUnitTemplate ventilationUnitTemplate = Template("UNIT-A");
            ventilationUnitTemplate.FanPerformance = null;

            VentilationUnitOperatingParameters ventilationUnitOperatingParameters = ventilationUnitTemplate.VentilationUnitOperatingParameters(60, 60);

            Assert.False(ventilationUnitOperatingParameters.IsFanPerformanceResolved);
            Assert.True(double.IsNaN(ventilationUnitOperatingParameters.SpecificFanPower_WPerLps));
            Assert.Equal(SpecificFanPowerBasis.Undefined, ventilationUnitOperatingParameters.SpecificFanPowerBasis);
            Assert.Contains("states no fan performance", ventilationUnitOperatingParameters.FanPerformanceRefusal);
            Assert.Contains("Missing is not a zero specific fan power", ventilationUnitOperatingParameters.FanPerformanceRefusal);

            Assert.True(ventilationUnitOperatingParameters.IsHeatRecoveryResolved);
            Assert.Equal(0.86, ventilationUnitOperatingParameters.SensibleHeatRecoveryEfficiency);
        }

        /// <summary>
        /// The project test product - a what-if capacity with no published fan or heat recovery data, by
        /// design - resolves nothing.
        /// </summary>
        [Fact]
        public void TheProjectTestProduct_ResolvesNothing()
        {
            VentilationUnitTemplate ventilationUnitTemplate = new PartOProjectTestVentilationUnit("Test unit 150", 150, 150).VentilationUnitTemplate();

            Assert.NotNull(ventilationUnitTemplate);

            VentilationUnitOperatingParameters ventilationUnitOperatingParameters = ventilationUnitTemplate.VentilationUnitOperatingParameters(60, 60);

            AssertBothRefused(ventilationUnitOperatingParameters, "states no");
        }

        /// <summary>A template that cannot be traced to a document, and no template at all, refuse both quantities.</summary>
        [Fact]
        public void AnUntraceableOrMissingTemplate_RefusesBothQuantities()
        {
            VentilationUnitTemplate ventilationUnitTemplate = Template("UNIT-A");
            ventilationUnitTemplate.Source = null;

            AssertBothRefused(ventilationUnitTemplate.VentilationUnitOperatingParameters(60, 60), "states no source");
            AssertBothRefused(((VentilationUnitTemplate)null!).VentilationUnitOperatingParameters(60, 60), "No ventilation unit template was supplied");
        }

        // =================================================================================================
        // F. Malformed, out-of-range and wrongly-denominated data refuses
        // =================================================================================================

        /// <summary>
        /// A grid that does not line up, a duplicated operating point, and a value that is not a finite
        /// number - written in code or in a file - are all malformed, and refused.
        /// </summary>
        [Fact]
        public void MalformedTables_Refuse()
        {
            //One value short of the airflows.
            AssertHeatRecoveryRefused(HeatRecoveryFromTable(Table([30.0, 60.0, 90.0], VentilationUnitPerformanceOutput.Name_SensibleHeatRecoveryEfficiency, "-", [0.9, 0.8])), "missing or malformed");

            //The same operating point stated twice - an ambiguous table.
            AssertHeatRecoveryRefused(new HeatRecoveryPerformance([60.0, 60.0], [0.85, 0.80], HeatRecoveryEfficiencyBasis.SupplyTemperatureEfficiency, source_Fixture), "no operating point is stated twice");

            //Out of order.
            AssertHeatRecoveryRefused(new HeatRecoveryPerformance([90.0, 60.0], [0.85, 0.80], HeatRecoveryEfficiencyBasis.SupplyTemperatureEfficiency, source_Fixture), "missing or malformed");

            //Non-finite values, in code.
            AssertHeatRecoveryRefused(new HeatRecoveryPerformance([30.0, 60.0], [0.9, double.NaN], HeatRecoveryEfficiencyBasis.SupplyTemperatureEfficiency, source_Fixture), "finite number");
            AssertFanRefused(new FanPerformance([30.0, 60.0], [0.5, double.PositiveInfinity], SpecificFanPowerBasis.TotalBothFans, source_Fixture), "finite number");
            AssertFanRefused(new FanPerformance([30.0, double.NaN], [0.5, 0.6], SpecificFanPowerBasis.TotalBothFans, source_Fixture), "finite number");

            //Non-numeric values, in a file.
            JsonObject jsonObject = HeatRecovery().ToJsonObject();
            JsonObject jsonObject_Output = (JsonObject)((JsonArray)((JsonObject)jsonObject["PerformanceTable"]!)["Outputs"]!)[0]!;
            jsonObject_Output["Values"] = new JsonArray(0.9, 0.86, "NaN", 0.74);
            AssertHeatRecoveryRefused(new HeatRecoveryPerformance(jsonObject), "missing or malformed");
        }

        /// <summary>
        /// An efficiency outside 0 to 1 - including a percentage typed as a whole number - and a fan power of
        /// zero or less are transcription mistakes, refused rather than clamped. So is an airflow of zero or
        /// less.
        /// </summary>
        [Fact]
        public void OutOfRangeValues_Refuse()
        {
            AssertHeatRecoveryRefused(HeatRecovery(values: [0.9, 0.86, 0.82, -0.1]), "between 0 and 1");
            AssertHeatRecoveryRefused(HeatRecovery(values: [0.9, 1.2, 0.82, 0.74]), "between 0 and 1");
            AssertHeatRecoveryRefused(HeatRecovery(values: [90, 86, 82, 74]), "between 0 and 1");

            AssertFanRefused(Fan(values: [0.5, 0.0, 0.8, 1.4]), "zero or less");
            AssertFanRefused(Fan(values: [0.5, -0.62, 0.8, 1.4]), "zero or less");

            AssertHeatRecoveryRefused(new HeatRecoveryPerformance([0.0, 60.0], [0.9, 0.86], HeatRecoveryEfficiencyBasis.SupplyTemperatureEfficiency, source_Fixture), "operating point at 0 l/s");
            AssertFanRefused(new FanPerformance([-30.0, 60.0], [0.5, 0.62], SpecificFanPowerBasis.TotalBothFans, source_Fixture), "operating point at -30 l/s");

            //A stated zero efficiency is data, not an absence - legal, and resolved as stated.
            VentilationUnitTemplate ventilationUnitTemplate = Template("UNIT-A");
            ventilationUnitTemplate.HeatRecoveryPerformance = HeatRecovery(values: [0.0, 0.0, 0.0, 0.0]);
            VentilationUnitOperatingParameters ventilationUnitOperatingParameters = ventilationUnitTemplate.VentilationUnitOperatingParameters(60, 60);
            Assert.True(ventilationUnitOperatingParameters.IsHeatRecoveryResolved);
            Assert.Equal(0.0, ventilationUnitOperatingParameters.SensibleHeatRecoveryEfficiency);
        }

        /// <summary>
        /// Units are proven, not assumed, and nothing is converted: an efficiency in per cent, a fan power in
        /// W/(m3/s), an airflow in m3/h, or a unit left blank, all refuse.
        /// </summary>
        [Fact]
        public void IncompatibleUnits_Refuse()
        {
            AssertHeatRecoveryRefused(HeatRecoveryFromTable(Table(airFlowRates_Lps, VentilationUnitPerformanceOutput.Name_SensibleHeatRecoveryEfficiency, "%", efficiencies)), "declared in '%'");
            AssertHeatRecoveryRefused(HeatRecoveryFromTable(Table(airFlowRates_Lps, VentilationUnitPerformanceOutput.Name_SensibleHeatRecoveryEfficiency, null, efficiencies)), "declared in ''");
            AssertHeatRecoveryRefused(HeatRecoveryFromTable(Table(airFlowRates_Lps, VentilationUnitPerformanceOutput.Name_SensibleHeatRecoveryEfficiency, "-", efficiencies, axisUnit: "m3/h")), "airflow axis is declared in 'm3/h'");

            AssertFanRefused(FanFromTable(Table(airFlowRates_Lps, VentilationUnitPerformanceOutput.Name_SpecificFanPower, "W/(m3/s)", specificFanPowers_WPerLps)), "declared in 'W/(m3/s)'");
        }

        /// <summary>
        /// A fan table indexed on external static pressure - as some manufacturers publish - cannot be read at
        /// a design airflow, and a table with an extra axis cannot either. Both refuse, and so does a table
        /// publishing the wrong quantity or more than one.
        /// </summary>
        [Fact]
        public void ATableNotIndexedOnAirflowAlone_OrPublishingTheWrongQuantity_Refuses()
        {
            VentilationUnitPerformanceTable ventilationUnitPerformanceTable_Pressure = new(
                [new VentilationUnitPerformanceAxis("ExternalStaticPressure", "Pa", [0.0, 100.0, 200.0])],
                [new VentilationUnitPerformanceOutput(VentilationUnitPerformanceOutput.Name_SpecificFanPower, VentilationUnitPerformanceOutput.Unit_WattsPerLitrePerSecond, [0.5, 0.7, 0.9])]);

            AssertFanRefused(FanFromTable(ventilationUnitPerformanceTable_Pressure), "not tabulated against airflow alone");

            VentilationUnitPerformanceTable ventilationUnitPerformanceTable_TwoAxes = new(
                [
                    new VentilationUnitPerformanceAxis(VentilationUnitPerformanceAxis.Name_AirFlowRate, VentilationUnitPerformanceAxis.Unit_LitresPerSecond, [30.0, 60.0]),
                    new VentilationUnitPerformanceAxis("ExternalStaticPressure", "Pa", [0.0, 100.0]),
                ],
                [new VentilationUnitPerformanceOutput(VentilationUnitPerformanceOutput.Name_SpecificFanPower, VentilationUnitPerformanceOutput.Unit_WattsPerLitrePerSecond, [0.5, 0.6, 0.7, 0.8])]);

            AssertFanRefused(FanFromTable(ventilationUnitPerformanceTable_TwoAxes), "not tabulated against airflow alone");

            //The efficiency table handed to the fan data: a real table, the wrong quantity.
            AssertFanRefused(FanFromTable(Table(airFlowRates_Lps, VentilationUnitPerformanceOutput.Name_SensibleHeatRecoveryEfficiency, "-", efficiencies)), "does not publish exactly one quantity");

            VentilationUnitPerformanceTable ventilationUnitPerformanceTable_TwoOutputs = new(
                [new VentilationUnitPerformanceAxis(VentilationUnitPerformanceAxis.Name_AirFlowRate, VentilationUnitPerformanceAxis.Unit_LitresPerSecond, airFlowRates_Lps)],
                [
                    new VentilationUnitPerformanceOutput(VentilationUnitPerformanceOutput.Name_SensibleHeatRecoveryEfficiency, "-", efficiencies),
                    new VentilationUnitPerformanceOutput(VentilationUnitPerformanceOutput.Name_SpecificFanPower, VentilationUnitPerformanceOutput.Unit_WattsPerLitrePerSecond, specificFanPowers_WPerLps),
                ]);

            AssertHeatRecoveryRefused(HeatRecoveryFromTable(ventilationUnitPerformanceTable_TwoOutputs), "does not publish exactly one quantity");
        }

        /// <summary>
        /// No basis, no source, or a policy that would extrapolate - each makes otherwise perfect figures
        /// unusable, even at a published airflow.
        /// </summary>
        [Fact]
        public void MissingBasisOrSource_OrAnExtrapolatingPolicy_Refuses()
        {
            AssertHeatRecoveryRefused(new HeatRecoveryPerformance(airFlowRates_Lps, efficiencies, HeatRecoveryEfficiencyBasis.Undefined, source_Fixture), "HeatRecoveryEfficiencyBasis");
            AssertFanRefused(new FanPerformance(airFlowRates_Lps, specificFanPowers_WPerLps, SpecificFanPowerBasis.Undefined, source_Fixture), "SpecificFanPowerBasis");

            AssertHeatRecoveryRefused(new HeatRecoveryPerformance(airFlowRates_Lps, efficiencies, HeatRecoveryEfficiencyBasis.SupplyTemperatureEfficiency, "  "), "states no source");
            AssertFanRefused(new FanPerformance(airFlowRates_Lps, specificFanPowers_WPerLps, SpecificFanPowerBasis.TotalBothFans, null!), "states no source");

            AssertHeatRecoveryRefused(HeatRecovery(PerformanceDomainPolicy.OuterCellLinearExtrapolation), "only Refuse or ClampToDomain");
            AssertFanRefused(Fan(PerformanceDomainPolicy.Undefined), "only Refuse or ClampToDomain");
        }

        // =================================================================================================
        // G. The duty itself - unusable, and unbalanced
        // =================================================================================================

        /// <summary>
        /// A design airflow that is not a finite positive number, on either side, refuses both quantities -
        /// there is no neutral duty to assume. So does an unusable tolerance.
        /// </summary>
        [Fact]
        public void AnUnusableDuty_RefusesBothQuantities()
        {
            foreach (double airFlowRate_Lps in new[] { double.NaN, double.PositiveInfinity, double.NegativeInfinity, 0.0, -60.0 })
            {
                AssertBothRefused(Template("UNIT-A").VentilationUnitOperatingParameters(airFlowRate_Lps, 60), "finite positive numbers");
                AssertBothRefused(Template("UNIT-A").VentilationUnitOperatingParameters(60, airFlowRate_Lps), "finite positive numbers");
            }

            AssertBothRefused(Template("UNIT-A").VentilationUnitOperatingParameters(60, 60, -1), "flow rate tolerance");
            AssertBothRefused(Template("UNIT-A").VentilationUnitOperatingParameters(60, 60, double.NaN), "flow rate tolerance");
        }

        /// <summary>
        /// <b>An unbalanced duty refuses heat recovery - and fan power - rather than assuming a rule.</b> Both
        /// quantities are certified at one balanced airflow; the imbalance behaviour of a real exchanger is
        /// not established, so none is invented. Within the named tolerance the duty is balanced.
        /// </summary>
        [Fact]
        public void AnUnbalancedDuty_RefusesBothQuantities()
        {
            VentilationUnitOperatingParameters ventilationUnitOperatingParameters = Template("UNIT-A").VentilationUnitOperatingParameters(60, 80);

            AssertBothRefused(ventilationUnitOperatingParameters, "unbalanced");
            Assert.Contains("No rule for an unbalanced unit is assumed", ventilationUnitOperatingParameters.HeatRecoveryRefusal);
            Assert.Equal(60, ventilationUnitOperatingParameters.SupplyAirFlowRate_Lps);
            Assert.Equal(80, ventilationUnitOperatingParameters.ExtractAirFlowRate_Lps);

            VentilationUnitOperatingParameters ventilationUnitOperatingParameters_WithinTolerance = Template("UNIT-A").VentilationUnitOperatingParameters(60, 60.0005);

            Assert.True(ventilationUnitOperatingParameters_WithinTolerance.IsHeatRecoveryResolved);
            Assert.Equal(60.00025, ventilationUnitOperatingParameters_WithinTolerance.AirFlowRate_Lps, 12);

            //And the tolerance is the caller's to name: zero means exact.
            AssertBothRefused(Template("UNIT-A").VentilationUnitOperatingParameters(60, 60.0005, 0), "unbalanced");
        }

        // =================================================================================================
        // H. The four airflows stay apart
        // =================================================================================================

        /// <summary>
        /// <b>Resolving at a unit's design duty reads the design airflow and changes nothing.</b> The unit
        /// is fitted with a 150 l/s product and designed at 30 l/s: the data is read at 30, not at the
        /// capacity - the two give different figures, so the test would see it - and every design airflow,
        /// the product's capacity and the unit's persisted state are exactly what they were.
        /// </summary>
        [Fact]
        public void ResolvingAtTheDesignDuty_NeverMovesItAndNeverReadsTheCapacity()
        {
            VentilationUnitTemplate ventilationUnitTemplate = Template("UNIT-150");
            List<VentilationUnitTemplate> ventilationUnitTemplates = [Template("UNIT-90", values: [0.5, 0.5, 0.5, 0.5]), ventilationUnitTemplate];

            AdjacencyCluster adjacencyCluster = Fixture(Reference("UNIT-150"), out AirHandlingUnit airHandlingUnit);

            Dictionary<string, double> designs_Before = Designs(adjacencyCluster);
            string airHandlingUnit_Before = airHandlingUnit.ToJsonObject().ToJsonString();

            Assert.True(adjacencyCluster.AirHandlingUnitDesignDuty(airHandlingUnit, out double supplyDuty_Lps, out double extractDuty_Lps));

            VentilationUnitTemplate ventilationUnitTemplate_Selected = airHandlingUnit.SelectedVentilationUnitTemplate(ventilationUnitTemplates);
            Assert.Same(ventilationUnitTemplate, ventilationUnitTemplate_Selected);

            VentilationUnitOperatingParameters ventilationUnitOperatingParameters = ventilationUnitTemplate_Selected.VentilationUnitOperatingParameters(supplyDuty_Lps, extractDuty_Lps);

            //Read at the design duty, exactly as given...
            Assert.Equal(30, ventilationUnitOperatingParameters.SupplyAirFlowRate_Lps);
            Assert.Equal(30, ventilationUnitOperatingParameters.ExtractAirFlowRate_Lps);
            Assert.Equal(30, ventilationUnitOperatingParameters.AirFlowRate_Lps);
            Assert.Equal(0.90, ventilationUnitOperatingParameters.SensibleHeatRecoveryEfficiency);
            Assert.Equal(0.50, ventilationUnitOperatingParameters.SpecificFanPower_WPerLps);

            //...not at the capacity, where the same data says something else.
            Assert.Equal(150, ventilationUnitTemplate.MaximumSupplyFlowRate_Lps);
            Assert.NotEqual(ventilationUnitTemplate.MaximumSupplyFlowRate_Lps, ventilationUnitOperatingParameters.AirFlowRate_Lps);
            Assert.NotEqual(ventilationUnitTemplate.VentilationUnitOperatingParameters(150, 150).SensibleHeatRecoveryEfficiency, ventilationUnitOperatingParameters.SensibleHeatRecoveryEfficiency);

            //And nothing moved: the design, the capacity, the unit.
            Assert.Equal(designs_Before, Designs(adjacencyCluster));
            Assert.True(adjacencyCluster.AirHandlingUnitDesignDuty(airHandlingUnit, out double supplyDuty_After_Lps, out double extractDuty_After_Lps));
            Assert.Equal(supplyDuty_Lps, supplyDuty_After_Lps);
            Assert.Equal(extractDuty_Lps, extractDuty_After_Lps);
            Assert.Equal(150, ventilationUnitTemplate.MaximumSupplyFlowRate_Lps);
            Assert.Equal(150, ventilationUnitTemplate.MaximumExtractFlowRate_Lps);

            string airHandlingUnit_After = Assert.Single(adjacencyCluster.GetObjects<AirHandlingUnit>()).ToJsonObject().ToJsonString();
            Assert.Equal(airHandlingUnit_Before, airHandlingUnit_After);

            foreach (string token in new[] { "HeatRecoveryPerformance", "FanPerformance", VentilationUnitPerformanceOutput.Name_SensibleHeatRecoveryEfficiency, VentilationUnitPerformanceOutput.Name_SpecificFanPower })
            {
                Assert.DoesNotContain(token, airHandlingUnit_After);
            }
        }

        /// <summary>A unit with no selection has no template - there is nothing to fall back to.</summary>
        [Fact]
        public void AUnitWithNoSelection_HasNoTemplate()
        {
            AdjacencyCluster adjacencyCluster = Fixture(null, out AirHandlingUnit airHandlingUnit);

            Assert.Null(airHandlingUnit.SelectedVentilationUnitTemplate([Template("UNIT-150")]));
            Assert.Null(((AirHandlingUnit)null!).SelectedVentilationUnitTemplate([Template("UNIT-150")]));

            AssertBothRefused(Resolve([Template("UNIT-150")], airHandlingUnit.SelectedVentilationUnitReference(), 30), "No ventilation unit product is selected");
            Assert.NotNull(adjacencyCluster);
        }

        // =================================================================================================
        // I. Determinism and isolation
        // =================================================================================================

        /// <summary>
        /// The same question gives the same answer every time, reads nothing it then changes, and hands back
        /// a result the caller cannot use to reach into the catalogue - so it is safe to memoise.
        /// </summary>
        [Fact]
        public void Resolution_IsDeterministic_AndMutatesNothing()
        {
            List<VentilationUnitTemplate> ventilationUnitTemplates = [Template("UNIT-A"), Template("UNIT-B")];

            List<string> jsons_Before = ventilationUnitTemplates.ConvertAll(x => x.ToJsonObject().ToJsonString());
            List<VentilationUnitTemplate> order_Before = [.. ventilationUnitTemplates];

            string snapshot = Snapshot(Resolve(ventilationUnitTemplates, Reference("UNIT-A"), 45));

            for (int i = 0; i < 3; i++)
            {
                Assert.Equal(snapshot, Snapshot(Resolve(ventilationUnitTemplates, Reference("UNIT-A"), 45)));
            }

            //An equivalent, separately built catalogue answers identically.
            Assert.Equal(snapshot, Snapshot(Resolve([Template("UNIT-A"), Template("UNIT-B")], Reference("UNIT-A"), 45)));

            Assert.Equal(jsons_Before, ventilationUnitTemplates.ConvertAll(x => x.ToJsonObject().ToJsonString()));
            Assert.Equal(order_Before, ventilationUnitTemplates);

            //The reference a result hands out is its own copy.
            VentilationUnitOperatingParameters ventilationUnitOperatingParameters = Resolve(ventilationUnitTemplates, Reference("UNIT-A"), 45);
            ventilationUnitOperatingParameters.VentilationUnitReference.Model = "CHANGED";

            Assert.Equal("UNIT-A", ventilationUnitOperatingParameters.VentilationUnitReference.Model);
            Assert.Equal("UNIT-A", ventilationUnitTemplates[0].VentilationUnitReference.Model);

            //And the caller's reference is not the one the result keeps.
            VentilationUnitReference ventilationUnitReference = Reference("UNIT-A");
            VentilationUnitOperatingParameters ventilationUnitOperatingParameters_2 = Resolve(ventilationUnitTemplates, ventilationUnitReference, 45);
            ventilationUnitReference.Model = "CHANGED";
            Assert.Equal("UNIT-A", ventilationUnitOperatingParameters_2.VentilationUnitReference.Model);
        }

        // =================================================================================================
        // Fixtures
        // =================================================================================================

        private static VentilationUnitReference Reference(string model, string reference = "R1")
        {
            return new VentilationUnitReference("Test Fixture", model, reference);
        }

        private static HeatRecoveryPerformance HeatRecovery(PerformanceDomainPolicy performanceDomainPolicy = PerformanceDomainPolicy.Refuse, double[]? values = null)
        {
            return new HeatRecoveryPerformance(airFlowRates_Lps, values ?? efficiencies, HeatRecoveryEfficiencyBasis.SupplyTemperatureEfficiency, source_Fixture, performanceDomainPolicy);
        }

        private static FanPerformance Fan(PerformanceDomainPolicy performanceDomainPolicy = PerformanceDomainPolicy.Refuse, double[]? values = null)
        {
            return new FanPerformance(airFlowRates_Lps, values ?? specificFanPowers_WPerLps, SpecificFanPowerBasis.TotalBothFans, source_Fixture, performanceDomainPolicy);
        }

        /// <summary>A fixture product with a 150 l/s capacity and both kinds of certified data.</summary>
        private static VentilationUnitTemplate Template(string model, string reference = "R1", double[]? values = null)
        {
            return new VentilationUnitTemplate(Reference(model, reference), source_Fixture)
            {
                MaximumSupplyFlowRate_Lps = 150,
                MaximumExtractFlowRate_Lps = 150,
                Rank = 10,
                HeatRecoveryPerformance = HeatRecovery(values: values),
                FanPerformance = Fan(),
            };
        }

        private static VentilationUnitPerformanceTable Table(double[] airFlowRates_Lps, string outputName, string? outputUnit, double[] values, string axisUnit = VentilationUnitPerformanceAxis.Unit_LitresPerSecond)
        {
            return new VentilationUnitPerformanceTable(
                [new VentilationUnitPerformanceAxis(VentilationUnitPerformanceAxis.Name_AirFlowRate, axisUnit, airFlowRates_Lps)],
                [new VentilationUnitPerformanceOutput(outputName, outputUnit, values)]);
        }

        /// <summary>Heat recovery data wrapping an arbitrary table - through the JSON constructor, the only way to hand it a table its typed constructor would not build.</summary>
        private static HeatRecoveryPerformance HeatRecoveryFromTable(VentilationUnitPerformanceTable ventilationUnitPerformanceTable)
        {
            JsonObject jsonObject = new()
            {
                ["_type"] = "SAM.Analytical.HeatRecoveryPerformance,SAM.Analytical",
                ["PerformanceTable"] = ventilationUnitPerformanceTable.ToJsonObject(),
                ["HeatRecoveryEfficiencyBasis"] = HeatRecoveryEfficiencyBasis.SupplyTemperatureEfficiency.ToString(),
                ["Source"] = source_Fixture,
            };

            return new HeatRecoveryPerformance(jsonObject);
        }

        /// <summary>Fan data wrapping an arbitrary table. See <see cref="HeatRecoveryFromTable"/>.</summary>
        private static FanPerformance FanFromTable(VentilationUnitPerformanceTable ventilationUnitPerformanceTable)
        {
            JsonObject jsonObject = new()
            {
                ["_type"] = "SAM.Analytical.FanPerformance,SAM.Analytical",
                ["PerformanceTable"] = ventilationUnitPerformanceTable.ToJsonObject(),
                ["SpecificFanPowerBasis"] = SpecificFanPowerBasis.TotalBothFans.ToString(),
                ["Source"] = source_Fixture,
            };

            return new FanPerformance(jsonObject);
        }

        private static VentilationUnitOperatingParameters Resolve(List<VentilationUnitTemplate>? ventilationUnitTemplates, VentilationUnitReference? ventilationUnitReference, double airFlowRate_Lps)
        {
            return ventilationUnitTemplates!.VentilationUnitOperatingParameters(ventilationUnitReference!, airFlowRate_Lps, airFlowRate_Lps);
        }

        /// <summary>Asserts heat recovery data is unusable, and that resolving it says why.</summary>
        private static void AssertHeatRecoveryRefused(HeatRecoveryPerformance heatRecoveryPerformance, string reason)
        {
            Assert.False(heatRecoveryPerformance.IsValid);
            Assert.True(double.IsNaN(heatRecoveryPerformance.SensibleHeatRecoveryEfficiency(60)));

            VentilationUnitTemplate ventilationUnitTemplate = Template("UNIT-A");
            ventilationUnitTemplate.HeatRecoveryPerformance = heatRecoveryPerformance;

            VentilationUnitOperatingParameters ventilationUnitOperatingParameters = ventilationUnitTemplate.VentilationUnitOperatingParameters(60, 60);

            Assert.False(ventilationUnitOperatingParameters.IsHeatRecoveryResolved);
            Assert.True(double.IsNaN(ventilationUnitOperatingParameters.SensibleHeatRecoveryEfficiency));
            Assert.Contains("cannot be used", ventilationUnitOperatingParameters.HeatRecoveryRefusal);
            Assert.Contains(reason, ventilationUnitOperatingParameters.HeatRecoveryRefusal);

            //The other quantity is unaffected.
            Assert.True(ventilationUnitOperatingParameters.IsFanPerformanceResolved);
        }

        /// <summary>Asserts fan data is unusable, and that resolving it says why.</summary>
        private static void AssertFanRefused(FanPerformance fanPerformance, string reason)
        {
            Assert.False(fanPerformance.IsValid);
            Assert.True(double.IsNaN(fanPerformance.SpecificFanPower_WPerLps(60)));

            VentilationUnitTemplate ventilationUnitTemplate = Template("UNIT-A");
            ventilationUnitTemplate.FanPerformance = fanPerformance;

            VentilationUnitOperatingParameters ventilationUnitOperatingParameters = ventilationUnitTemplate.VentilationUnitOperatingParameters(60, 60);

            Assert.False(ventilationUnitOperatingParameters.IsFanPerformanceResolved);
            Assert.True(double.IsNaN(ventilationUnitOperatingParameters.SpecificFanPower_WPerLps));
            Assert.Contains("cannot be used", ventilationUnitOperatingParameters.FanPerformanceRefusal);
            Assert.Contains(reason, ventilationUnitOperatingParameters.FanPerformanceRefusal);

            Assert.True(ventilationUnitOperatingParameters.IsHeatRecoveryResolved);
        }

        private static void AssertBothRefused(VentilationUnitOperatingParameters ventilationUnitOperatingParameters, string reason)
        {
            Assert.NotNull(ventilationUnitOperatingParameters);

            Assert.False(ventilationUnitOperatingParameters.IsHeatRecoveryResolved);
            Assert.False(ventilationUnitOperatingParameters.IsFanPerformanceResolved);

            Assert.True(double.IsNaN(ventilationUnitOperatingParameters.SensibleHeatRecoveryEfficiency));
            Assert.True(double.IsNaN(ventilationUnitOperatingParameters.SpecificFanPower_WPerLps));

            Assert.Contains(reason, ventilationUnitOperatingParameters.HeatRecoveryRefusal);
            Assert.Contains(reason, ventilationUnitOperatingParameters.FanPerformanceRefusal);
        }

        /// <summary>Every field of a result, round-trippable, so two results can be compared whole.</summary>
        private static string Snapshot(VentilationUnitOperatingParameters ventilationUnitOperatingParameters)
        {
            return string.Join(
                "|",
                ventilationUnitOperatingParameters.VentilationUnitReference?.ToString(),
                ventilationUnitOperatingParameters.SupplyAirFlowRate_Lps.ToString("R"),
                ventilationUnitOperatingParameters.ExtractAirFlowRate_Lps.ToString("R"),
                ventilationUnitOperatingParameters.AirFlowRate_Lps.ToString("R"),
                ventilationUnitOperatingParameters.SensibleHeatRecoveryEfficiency.ToString("R"),
                ventilationUnitOperatingParameters.HeatRecoveryEfficiencyBasis,
                ventilationUnitOperatingParameters.HeatRecoveryClampedToDomain,
                ventilationUnitOperatingParameters.HeatRecoveryRefusal,
                ventilationUnitOperatingParameters.SpecificFanPower_WPerLps.ToString("R"),
                ventilationUnitOperatingParameters.SpecificFanPowerBasis,
                ventilationUnitOperatingParameters.FanPerformanceClampedToDomain,
                ventilationUnitOperatingParameters.FanPerformanceRefusal);
        }

        /// <summary>
        /// One dwelling: a supply room and an extract room on one ventilation system, supplied by one air
        /// handling unit fitted with <paramref name="ventilationUnitReference"/>. Design duty 30 l/s each way.
        /// </summary>
        private static AdjacencyCluster Fixture(VentilationUnitReference? ventilationUnitReference, out AirHandlingUnit airHandlingUnit)
        {
            AdjacencyCluster adjacencyCluster = new();

            airHandlingUnit = Analytical.Create.AirHandlingUnit("AHU-01");

            if (ventilationUnitReference is not null)
            {
                airHandlingUnit.SetValue(AirHandlingUnitParameter.VentilationUnitReference, ventilationUnitReference);
            }

            adjacencyCluster.AddObject(airHandlingUnit);

            Space space_Supply = Room(adjacencyCluster, "Living Room", PartFTerminalRole.Supply, 25);
            Space space_Extract = Room(adjacencyCluster, "Bathroom", PartFTerminalRole.GeneralExtract, 25);

            VentilationSystem ventilationSystem = new("Fixture", new VentilationSystemType("Fixture MVHR", "Fixture"));
            ventilationSystem.SetValue(VentilationSystemParameter.SupplyUnitName, airHandlingUnit.Name);
            ventilationSystem.SetValue(VentilationSystemParameter.ExhaustUnitName, airHandlingUnit.Name);

            adjacencyCluster.AddObject(ventilationSystem);

            Terminal(adjacencyCluster, ventilationSystem, space_Supply, FlowClassification.Supply, 30);
            Terminal(adjacencyCluster, ventilationSystem, space_Extract, FlowClassification.Extract, 30);

            adjacencyCluster.AddRelation(ventilationSystem, space_Supply);
            adjacencyCluster.AddRelation(ventilationSystem, space_Extract);

            return adjacencyCluster;
        }

        private static Space Room(AdjacencyCluster adjacencyCluster, string name, PartFTerminalRole partFTerminalRole, double requirement_Lps)
        {
            Space result = new(name);

            PartFVentilationTerminalRequirement partFVentilationTerminalRequirement = new(name + " requirement", result.Guid, partFTerminalRole)
            {
                ContinuousDesignFlowRate_Lps = requirement_Lps,
            };

            PartFSpaceData partFSpaceData = new();
            partFSpaceData.Terminals.Add(partFVentilationTerminalRequirement);

            result.SetValue(SpaceParameter.PartFSpaceData, partFSpaceData);

            adjacencyCluster.AddObject(result);

            return result;
        }

        private static void Terminal(AdjacencyCluster adjacencyCluster, VentilationSystem ventilationSystem, Space space, FlowClassification flowClassification, double designFlowRate_Lps)
        {
            PartFVentilationTerminalRequirement partFVentilationTerminalRequirement = space.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData).Terminals[0];

            VentilationTerminal ventilationTerminal = new(space.Name + " terminal", flowClassification, designFlowRate_Lps);
            ventilationTerminal.SetValue(VentilationTerminalParameter.PartFTerminalReference, new PartFTerminalReference(partFVentilationTerminalRequirement));

            adjacencyCluster.AddObject(ventilationTerminal);
            adjacencyCluster.AddRelation(ventilationTerminal, space);
            adjacencyCluster.AddRelation(ventilationTerminal, ventilationSystem);
        }

        /// <summary>Every design airflow in the model, so the model can be compared before and after.</summary>
        private static Dictionary<string, double> Designs(AdjacencyCluster adjacencyCluster)
        {
            Dictionary<string, double> result = [];

            foreach (VentilationTerminal ventilationTerminal in adjacencyCluster.GetObjects<VentilationTerminal>() ?? [])
            {
                result[string.Format("{0} {1}", ventilationTerminal.Name, ventilationTerminal.FlowClassification)] = ventilationTerminal.DesignFlowRate_Lps ?? double.NaN;
            }

            return result;
        }
    }
}
