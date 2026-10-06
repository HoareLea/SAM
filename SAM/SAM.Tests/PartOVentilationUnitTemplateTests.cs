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
    /// <b>The manufacturer template seam - a real product behind Approved Document O Iteration 2's plant,
    /// and the raw performance data Iteration 3 will need.</b>
    /// <para>
    /// Iteration 2 selects on <see cref="VentilationUnitCapacityDescriptor"/>, which is handed in. This
    /// suite pins the thing that hands it in: <see cref="VentilationUnitTemplate"/>, a transcription of
    /// what a manufacturer published, and the one mapping that turns it into a descriptor. The sizing
    /// kernel is not changed by any of it, and several tests below exist only to prove that.
    /// </para>
    /// <para>
    /// The four quantities Iteration 2 keeps apart gain a fifth neighbour here, and it is the one most
    /// easily confused with capability:
    /// </para>
    /// <code>
    /// requirement        what Approved Document F demands of a room
    /// capability         what the product can do            &lt;- VentilationUnitTemplate maximum airflows
    /// PUBLISHED DUTY     conditions the manufacturer measured at   &lt;- performance table axes. NOT capability.
    /// design             what this dwelling is designed to move
    /// operating          what it moves at 3pm in August     (Iteration 3)
    /// </code>
    /// <para>
    /// <b>The templates below are fixtures, not copies of a shipped catalogue.</b> Nothing here names a
    /// real manufacturer - the same arrangement, and the same reason, as
    /// <see cref="PartOVentilationUnitSelectionTests"/>. The shipped products, and the assertions that
    /// their figures match their brochure, live with the catalogue in <c>SAM_Systems</c>.
    /// </para>
    /// </summary>
    public class PartOVentilationUnitTemplateTests
    {
        private const string source_Fixture = "Test Fixture, Performance Data, v.1 - not a real product";

        // =================================================================================================
        // A. Template -> capacity descriptor: the Iteration 2 seam
        // =================================================================================================

        /// <summary>
        /// A template becomes exactly the descriptor its figures state - identity, both capacities, rank -
        /// and that descriptor is what the existing selection kernel consumes, unmodified.
        /// </summary>
        [Fact]
        public void AManufacturerTemplate_MapsToTheCapacityDescriptorItStates()
        {
            VentilationUnitTemplate ventilationUnitTemplate = Template("UNIT-A", 120, 100, rank: 7);

            VentilationUnitCapacityDescriptor ventilationUnitCapacityDescriptor = ventilationUnitTemplate.CapacityDescriptor();

            Assert.NotNull(ventilationUnitCapacityDescriptor);
            Assert.True(ventilationUnitCapacityDescriptor.IsValid);

            Assert.Equal("Test Fixture", ventilationUnitCapacityDescriptor.VentilationUnitReference.Manufacturer);
            Assert.Equal("UNIT-A", ventilationUnitCapacityDescriptor.VentilationUnitReference.Model);
            Assert.Equal("COOL-A", ventilationUnitCapacityDescriptor.VentilationUnitReference.Reference);

            Assert.Equal(120, ventilationUnitCapacityDescriptor.MaximumSupplyFlowRate_Lps, 6);
            Assert.Equal(100, ventilationUnitCapacityDescriptor.MaximumExtractFlowRate_Lps, 6);
            Assert.Equal(7, ventilationUnitCapacityDescriptor.Rank);

            //And the unchanged Iteration 2 kernel accepts it.
            VentilationUnitSelection ventilationUnitSelection = new List<VentilationUnitCapacityDescriptor> { ventilationUnitCapacityDescriptor }.SelectSmallestCapableVentilationUnit(115, 95);

            Assert.True(ventilationUnitSelection.IsSelected);
            Assert.Equal("UNIT-A", ventilationUnitSelection.VentilationUnitReference.Model);
        }

        /// <summary>
        /// The two sides are separate facts about the product and are checked separately. A unit rated
        /// 120 supply / 80 extract serves a 110/80 dwelling and does not serve a 110/90 one, however
        /// comfortable its total looks.
        /// </summary>
        [Fact]
        public void SupplyAndExtractCapacities_StayIndependent()
        {
            VentilationUnitTemplate ventilationUnitTemplate = Template("UNIT-B", 120, 80);

            VentilationUnitCapacityDescriptor ventilationUnitCapacityDescriptor = ventilationUnitTemplate.CapacityDescriptor();

            Assert.True(ventilationUnitCapacityDescriptor.IsSufficientFor(110, 80));
            Assert.False(ventilationUnitCapacityDescriptor.IsSufficientFor(110, 90));

            //The sum would have said yes to both - 200 covers 110 + 90 - which is exactly why compliance is
            //never asked of the sum.
            Assert.Equal(200, ventilationUnitCapacityDescriptor.Size_Lps, 6);
        }

        /// <summary>
        /// <b>The central refusal.</b> A template that publishes a full performance table but whose maximum
        /// airflows nobody has established is not offered to a selection, and the largest airflow on its
        /// performance table is not quietly promoted into the gap.
        /// </summary>
        [Fact]
        public void AnUnresolvedCapacity_IsNeverTakenFromThePerformanceTable()
        {
            VentilationUnitTemplate ventilationUnitTemplate = UnresolvedTemplate();

            //The performance table really does state an airflow, and it really is the obvious wrong answer.
            Assert.Equal(120, ventilationUnitTemplate.PerformanceTable.Axis(VentilationUnitPerformanceAxis.Name_AirFlowRate).Maximum, 6);

            Assert.True(ventilationUnitTemplate.IsValid);
            Assert.False(ventilationUnitTemplate.HasSelectionCapacity);

            Assert.Null(ventilationUnitTemplate.CapacityDescriptor());
            Assert.Empty(new List<VentilationUnitTemplate> { ventilationUnitTemplate }.CapacityDescriptors());

            //A duty the 120 l/s endpoint would have covered selects nothing at all.
            VentilationUnitSelection ventilationUnitSelection = new List<VentilationUnitTemplate> { ventilationUnitTemplate }
                .CapacityDescriptors()
                .SelectSmallestCapableVentilationUnit(100, 100);

            Assert.False(ventilationUnitSelection.IsSelected);
            Assert.Null(ventilationUnitSelection.Descriptor);
        }

        /// <summary>
        /// The absence is reportable rather than invisible: the refusal names the product, says which side
        /// is missing, states the duty-point rule, and repeats whatever the catalogue said about resolving
        /// it.
        /// </summary>
        [Fact]
        public void AnUnresolvedCapacity_RefusesInWordsAnEngineerCanActOn()
        {
            VentilationUnitTemplate ventilationUnitTemplate = UnresolvedTemplate();

            string reason = ventilationUnitTemplate.SelectionCapacityRefusal;

            Assert.Contains("UNIT-C", reason);
            Assert.Contains("maximum supply and maximum extract airflow", reason);
            Assert.Contains("published duty point, not the unit's maximum", reason);
            Assert.Contains("Ask the manufacturer", reason);

            KeyValuePair<VentilationUnitTemplate, string> unselectable = Assert.Single(new List<VentilationUnitTemplate> { ventilationUnitTemplate }.UnselectableVentilationUnitTemplates());

            Assert.Same(ventilationUnitTemplate, unselectable.Key);
            Assert.Equal(reason, unselectable.Value);
        }

        /// <summary>
        /// A catalogue holding both kinds offers the resolved ones and reports the rest. Nothing is
        /// approximated, and nothing disappears.
        /// </summary>
        [Fact]
        public void ACatalogue_OffersTheResolvedProductsAndReportsTheRest()
        {
            List<VentilationUnitTemplate> ventilationUnitTemplates =
            [
                Template("UNIT-A", 60, 60),
                UnresolvedTemplate(),
                Template("UNIT-B", 90, 90),
            ];

            List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors = ventilationUnitTemplates.CapacityDescriptors();

            Assert.Equal(2, ventilationUnitCapacityDescriptors.Count);
            Assert.Equal("UNIT-A", ventilationUnitCapacityDescriptors[0].VentilationUnitReference.Model);
            Assert.Equal("UNIT-B", ventilationUnitCapacityDescriptors[1].VentilationUnitReference.Model);

            Assert.Single(ventilationUnitTemplates.UnselectableVentilationUnitTemplates());
        }

        /// <summary>
        /// Half a capacity is no capacity. A template stating only its supply side would let the extract
        /// side be compared against nothing.
        /// </summary>
        [Fact]
        public void AHalfStatedCapacity_IsNotOffered()
        {
            VentilationUnitTemplate ventilationUnitTemplate = Template("UNIT-D", 120, 100);
            ventilationUnitTemplate.MaximumExtractFlowRate_Lps = double.NaN;

            Assert.False(ventilationUnitTemplate.HasSelectionCapacity);
            Assert.Null(ventilationUnitTemplate.CapacityDescriptor());
            Assert.Contains("maximum extract airflow", ventilationUnitTemplate.SelectionCapacityRefusal);
        }

        /// <summary>
        /// A negative capacity is a broken entry rather than a very small unit - it would be compliant with
        /// nothing and would quietly shrink the field of candidates.
        /// </summary>
        [Fact]
        public void ANegativeCapacity_IsNotOffered()
        {
            VentilationUnitTemplate ventilationUnitTemplate = Template("UNIT-E", -1, 100);

            Assert.False(ventilationUnitTemplate.HasSelectionCapacity);
            Assert.Null(ventilationUnitTemplate.CapacityDescriptor());
        }

        /// <summary>
        /// A template whose figures cannot be traced to a document is not manufacturer data, and is not
        /// offered however complete its numbers look.
        /// </summary>
        [Fact]
        public void ATemplateWithNoSource_IsNotOffered()
        {
            VentilationUnitTemplate ventilationUnitTemplate = Template("UNIT-F", 120, 100);
            ventilationUnitTemplate.Source = null;

            Assert.False(ventilationUnitTemplate.IsValid);
            Assert.False(ventilationUnitTemplate.HasSelectionCapacity);
            Assert.Null(ventilationUnitTemplate.CapacityDescriptor());
            Assert.Contains("states no source", ventilationUnitTemplate.SelectionCapacityRefusal);
        }

        /// <summary>
        /// A product identity resolves back to exactly one template, and a catalogue that gives it two
        /// resolves to none - the same refusal, for the same reason, the selection kernel already makes.
        /// This is the lookup Iteration 3 will cross to reach performance data from a stored identity.
        /// </summary>
        [Fact]
        public void AProductIdentity_ResolvesToOneTemplateOrToNone()
        {
            VentilationUnitTemplate ventilationUnitTemplate = Template("UNIT-A", 60, 60);

            List<VentilationUnitTemplate> ventilationUnitTemplates = [ventilationUnitTemplate, Template("UNIT-B", 90, 90)];

            Assert.Same(ventilationUnitTemplate, ventilationUnitTemplates.MatchingVentilationUnitTemplate(Reference("UNIT-A")));
            Assert.Null(ventilationUnitTemplates.MatchingVentilationUnitTemplate(Reference("UNIT-Z")));

            List<VentilationUnitTemplate> ventilationUnitTemplates_Ambiguous = [Template("UNIT-A", 60, 60), Template("UNIT-A", 90, 90)];

            Assert.Null(ventilationUnitTemplates_Ambiguous.MatchingVentilationUnitTemplate(Reference("UNIT-A")));
        }

        // =================================================================================================
        // B. Selecting a real product leaves the design alone
        // =================================================================================================

        /// <summary>
        /// Selecting from a catalogue of manufacturer templates chooses the smallest compliant product and
        /// <b>writes nothing but its identity</b>. Every design airflow in the dwelling is exactly what it
        /// was, and the headroom the product brings is reported rather than taken up.
        /// </summary>
        [Fact]
        public void SelectingAManufacturerProduct_LeavesEveryDesignAirflowAlone()
        {
            AdjacencyCluster adjacencyCluster = Fixture(out AirHandlingUnit airHandlingUnit);

            Dictionary<string, double> designs_Before = Designs(adjacencyCluster);

            VentilationUnitSelection ventilationUnitSelection = adjacencyCluster.SelectVentilationUnit(airHandlingUnit, Catalogue().CapacityDescriptors(), out List<string> notes, out List<string> refusals);

            Assert.True(ventilationUnitSelection.IsSelected);
            Assert.Empty(refusals);
            Assert.Single(notes);

            //Duty 30/30 against 25 / 40 / 60 - the smallest compliant, never the nearest.
            Assert.Equal("UNIT-40", ventilationUnitSelection.VentilationUnitReference.Model);
            Assert.Equal(10, ventilationUnitSelection.SupplyHeadroom_Lps, 6);
            Assert.Equal(10, ventilationUnitSelection.ExtractHeadroom_Lps, 6);

            //The whole point: capability arrived, and the design did not move.
            Assert.Equal(designs_Before, Designs(adjacencyCluster));

            AirHandlingUnit airHandlingUnit_Selected = Assert.Single(adjacencyCluster.GetObjects<AirHandlingUnit>());

            Assert.True(Reference("UNIT-40").Matches(airHandlingUnit_Selected.SelectedVentilationUnitReference()));
        }

        /// <summary>
        /// <b>Nothing from the template but the identity reaches the model.</b> The capacities stay in the
        /// catalogue, and the performance table and control curve - which the template is carrying in full
        /// - do not appear on the air handling unit in any form.
        /// </summary>
        [Fact]
        public void SelectingAManufacturerProduct_WritesNoCapabilityAndNoPerformanceDataOntoTheModel()
        {
            AdjacencyCluster adjacencyCluster = Fixture(out AirHandlingUnit airHandlingUnit);

            Assert.True(adjacencyCluster.SelectVentilationUnit(airHandlingUnit, Catalogue().CapacityDescriptors(), out _, out _).IsSelected);

            AirHandlingUnit airHandlingUnit_Selected = Assert.Single(adjacencyCluster.GetObjects<AirHandlingUnit>());

            string json = Core.Convert.ToString(airHandlingUnit_Selected);

            Assert.Contains("UNIT-40", json);

            foreach (string token in new[] { "PerformanceTable", "FlowFraction", "MaximumSupplyFlowRate", "MaximumExtractFlowRate", "SupplyAirTemperature", "CombinedCoolingCapacity", "Source" })
            {
                Assert.DoesNotContain(token, json);
            }
        }

        /// <summary>
        /// The sizing kernel does not read template data, and this proves it rather than asserting it: two
        /// catalogues identical in identity, capacity and rank but carrying <i>completely different</i>
        /// performance tables and control curves produce the same selection.
        /// </summary>
        [Fact]
        public void TemplateAndControlData_AreNotReadByTheIteration2Kernel()
        {
            List<VentilationUnitTemplate> ventilationUnitTemplates_1 = Catalogue();

            List<VentilationUnitTemplate> ventilationUnitTemplates_2 = Catalogue();
            foreach (VentilationUnitTemplate ventilationUnitTemplate in ventilationUnitTemplates_2)
            {
                ventilationUnitTemplate.PerformanceTable = null;
                ventilationUnitTemplate.FlowFractionByControlTemperature = new FlowFractionControlCurve([5, 10], [1, 0]);
            }

            AdjacencyCluster adjacencyCluster_1 = Fixture(out AirHandlingUnit airHandlingUnit_1);
            AdjacencyCluster adjacencyCluster_2 = Fixture(out AirHandlingUnit airHandlingUnit_2);

            VentilationUnitSelection ventilationUnitSelection_1 = adjacencyCluster_1.SelectVentilationUnit(airHandlingUnit_1, ventilationUnitTemplates_1.CapacityDescriptors(), out _, out _);
            VentilationUnitSelection ventilationUnitSelection_2 = adjacencyCluster_2.SelectVentilationUnit(airHandlingUnit_2, ventilationUnitTemplates_2.CapacityDescriptors(), out _, out _);

            Assert.True(ventilationUnitSelection_1.IsSelected);
            Assert.Equal(ventilationUnitSelection_1.VentilationUnitReference.Model, ventilationUnitSelection_2.VentilationUnitReference.Model);
            Assert.Equal(ventilationUnitSelection_1.SupplyHeadroom_Lps, ventilationUnitSelection_2.SupplyHeadroom_Lps, 6);
            Assert.Equal(Designs(adjacencyCluster_1), Designs(adjacencyCluster_2));
        }

        // =================================================================================================
        // C. Serialization
        // =================================================================================================

        /// <summary>
        /// A template survives a round trip with its identity, its cooling module and - the field that
        /// makes its numbers evidence rather than numbers - its source.
        /// </summary>
        [Fact]
        public void ATemplate_SurvivesSerializationWithItsIdentityAndSource()
        {
            VentilationUnitTemplate ventilationUnitTemplate = Template("UNIT-A", 120, 100, rank: 7);

            VentilationUnitTemplate ventilationUnitTemplate_RoundTripped = Helpers.RoundTrip.Once(ventilationUnitTemplate);

            Assert.True(ventilationUnitTemplate.VentilationUnitReference.Matches(ventilationUnitTemplate_RoundTripped.VentilationUnitReference));
            Assert.Equal("Test Fixture", ventilationUnitTemplate_RoundTripped.VentilationUnitReference.Manufacturer);
            Assert.Equal("UNIT-A", ventilationUnitTemplate_RoundTripped.VentilationUnitReference.Model);
            Assert.Equal("COOL-A", ventilationUnitTemplate_RoundTripped.CoolingModuleModel);
            Assert.Equal(source_Fixture, ventilationUnitTemplate_RoundTripped.Source);
            Assert.Equal(120, ventilationUnitTemplate_RoundTripped.MaximumSupplyFlowRate_Lps, 6);
            Assert.Equal(100, ventilationUnitTemplate_RoundTripped.MaximumExtractFlowRate_Lps, 6);
            Assert.Equal(7, ventilationUnitTemplate_RoundTripped.Rank);

            //And it still maps to the same descriptor afterwards, which is what a catalogue read from disk
            //has to be able to do.
            Assert.Equal(120, ventilationUnitTemplate_RoundTripped.CapacityDescriptor().MaximumSupplyFlowRate_Lps, 6);
        }

        /// <summary>
        /// An unresolved capacity survives as unresolved. It does not become a zero, and it does not
        /// become resolvable, on the way through a file.
        /// </summary>
        [Fact]
        public void AnUnresolvedCapacity_SurvivesSerializationAsUnresolved()
        {
            VentilationUnitTemplate ventilationUnitTemplate_RoundTripped = Helpers.RoundTrip.Once(UnresolvedTemplate());

            Assert.True(double.IsNaN(ventilationUnitTemplate_RoundTripped.MaximumSupplyFlowRate_Lps));
            Assert.True(double.IsNaN(ventilationUnitTemplate_RoundTripped.MaximumExtractFlowRate_Lps));
            Assert.False(ventilationUnitTemplate_RoundTripped.HasSelectionCapacity);
            Assert.Null(ventilationUnitTemplate_RoundTripped.CapacityDescriptor());
            Assert.Contains("Ask the manufacturer", ventilationUnitTemplate_RoundTripped.UnresolvedCapacityNote);

            //And the performance data it does hold came through intact, so the record is still complete.
            Assert.Equal(96, ventilationUnitTemplate_RoundTripped.PerformanceTable.PointCount);
        }

        /// <summary>
        /// Every published value survives a round trip <b>exactly</b>. A performance table that came back
        /// rounded would no longer be the document.
        /// </summary>
        [Fact]
        public void ThePerformanceTableAndControlCurve_SurviveSerializationExactly()
        {
            VentilationUnitTemplate ventilationUnitTemplate = Template("UNIT-A", 120, 100);
            ventilationUnitTemplate.PerformanceTable = FixtureTable();
            ventilationUnitTemplate.FlowFractionByControlTemperature = ControlCurve();

            VentilationUnitTemplate ventilationUnitTemplate_RoundTripped = Helpers.RoundTrip.Once(ventilationUnitTemplate);

            VentilationUnitPerformanceTable ventilationUnitPerformanceTable = ventilationUnitTemplate_RoundTripped.PerformanceTable;

            Assert.True(ventilationUnitPerformanceTable.IsValid);
            Assert.Equal(ventilationUnitTemplate.PerformanceTable.AxisNames, ventilationUnitPerformanceTable.AxisNames);
            Assert.Equal(ventilationUnitTemplate.PerformanceTable.OutputNames, ventilationUnitPerformanceTable.OutputNames);
            Assert.Equal(ventilationUnitTemplate.PerformanceTable.Output(name_Supply).Values, ventilationUnitPerformanceTable.Output(name_Supply).Values);
            Assert.Equal(ventilationUnitTemplate.PerformanceTable.Axis(name_Flow).Values, ventilationUnitPerformanceTable.Axis(name_Flow).Values);
            Assert.Equal("degC", ventilationUnitPerformanceTable.Output(name_Supply).Unit);

            FlowFractionControlCurve flowFractionControlCurve = ventilationUnitTemplate_RoundTripped.FlowFractionByControlTemperature;

            Assert.True(flowFractionControlCurve.IsValid);
            Assert.Equal(new double[] { 22.0, 26.0 }, flowFractionControlCurve.ControlTemperatures_C);
            Assert.Equal(new double[] { 0.3, 1.0 }, flowFractionControlCurve.FlowFractions);
            Assert.Equal(PerformanceDomainPolicy.ClampToDomain, flowFractionControlCurve.PerformanceDomainPolicy);
        }

        // =================================================================================================
        // D. Raw performance data - preserved, not reduced
        // =================================================================================================

        /// <summary>
        /// The axes come back as the conditions the manufacturer published - the coordinates themselves,
        /// in order, not a range that happens to span them.
        /// </summary>
        [Fact]
        public void TheRawAxes_ArePreservedExactly()
        {
            VentilationUnitPerformanceTable ventilationUnitPerformanceTable = FixtureTable();

            Assert.Equal(3, ventilationUnitPerformanceTable.AxisCount);
            Assert.Equal(new List<string> { name_External, name_Entering, name_Flow }, ventilationUnitPerformanceTable.AxisNames);

            Assert.Equal(new double[] { 29.0, 32.0, 34.0 }, ventilationUnitPerformanceTable.Axis(name_External).Values);
            Assert.Equal(new double[] { 23.0, 24.0, 25.0, 26.0 }, ventilationUnitPerformanceTable.Axis(name_Entering).Values);
            Assert.Equal(new double[] { 50.0, 60.0, 70.0, 80.0, 90.0, 100.0, 110.0, 120.0 }, ventilationUnitPerformanceTable.Axis(name_Flow).Values);

            //Every published point is kept, not a slice of them and not a fitted curve through them.
            Assert.Equal(96, ventilationUnitPerformanceTable.PointCount);
            Assert.Equal(96, ventilationUnitPerformanceTable.Output(name_Supply).Count);
            Assert.Equal(96, ventilationUnitPerformanceTable.Output(name_Cooling).Count);

            Assert.Equal("degC", ventilationUnitPerformanceTable.Axis(name_External).Unit);
            Assert.Equal("l/s", ventilationUnitPerformanceTable.Axis(name_Flow).Unit);
            Assert.Equal("kW", ventilationUnitPerformanceTable.Output(name_Cooling).Unit);
        }

        /// <summary>
        /// A query at a published condition gives the published number back, and gives it back
        /// <b>exactly</b> - not to six places. A lookup that quietly rounds the source cannot be checked
        /// against the source.
        /// </summary>
        [Fact]
        public void ARawGridPoint_ReturnsThePublishedValueExactly()
        {
            VentilationUnitPerformanceTable ventilationUnitPerformanceTable = FixtureTable();

            //By index - the number as transcribed, with no arithmetic performed on it.
            Assert.Equal(Supply(0, 0, 0), ventilationUnitPerformanceTable.PublishedValue(name_Supply, 0, 0, 0));
            Assert.Equal(Supply(2, 3, 7), ventilationUnitPerformanceTable.PublishedValue(name_Supply, 2, 3, 7));
            Assert.Equal(Cooling(1, 2, 4), ventilationUnitPerformanceTable.PublishedValue(name_Cooling, 1, 2, 4));

            //And by condition, which goes through the interpolator and must land on the same bits.
            Assert.Equal(Supply(2, 3, 7), ventilationUnitPerformanceTable.Value(name_Supply, new double[] { 34.0, 26.0, 120.0 }));
            Assert.Equal(Supply(0, 0, 0), ventilationUnitPerformanceTable.Value(name_Supply, new double[] { 29.0, 23.0, 50.0 }));
            Assert.Equal(Cooling(1, 2, 4), ventilationUnitPerformanceTable.Value(name_Cooling, new double[] { 32.0, 25.0, 90.0 }));
        }

        /// <summary>
        /// <b>An airflow on a performance table is a published duty point.</b> The table states 120 l/s and
        /// the template states no capacity at all; the two facts sit side by side and neither becomes the
        /// other.
        /// </summary>
        [Fact]
        public void APerformanceTableAirflow_IsNotACapacity()
        {
            VentilationUnitTemplate ventilationUnitTemplate = UnresolvedTemplate();

            double airFlowRate_Maximum_Lps = ventilationUnitTemplate.PerformanceTable.Axis(VentilationUnitPerformanceAxis.Name_AirFlowRate).Maximum;

            Assert.Equal(120, airFlowRate_Maximum_Lps, 6);

            Assert.True(double.IsNaN(ventilationUnitTemplate.MaximumSupplyFlowRate_Lps));
            Assert.True(double.IsNaN(ventilationUnitTemplate.MaximumExtractFlowRate_Lps));
            Assert.NotEqual(airFlowRate_Maximum_Lps, ventilationUnitTemplate.MaximumSupplyFlowRate_Lps);

            //The performance data is fully readable at that duty point. Being able to say what the unit does
            //at 120 l/s is not being able to say it can move 120 l/s.
            Assert.False(double.IsNaN(ventilationUnitTemplate.SupplyAirTemperature_C(34, 26, 120)));
        }

        // =================================================================================================
        // E. Interpolation, and what happens outside the published conditions
        // =================================================================================================

        /// <summary>
        /// Between published conditions the answer is the multilinear one, deterministic and
        /// hand-checkable: at the centre of a cell it is the mean of that cell's corners, and each weight
        /// is what it should be.
        /// </summary>
        [Fact]
        public void InDomainInterpolation_IsDeterministicAndWeightedCorrectly()
        {
            VentilationUnitPerformanceTable ventilationUnitPerformanceTable = CornerTable();

            //Corners 1, 2, 4, 8, 16, 32, 64, 128 - powers of two, so any mis-weighted corner shows up as a
            //number that could not have come from any other combination.
            Assert.Equal(255.0 / 8.0, ventilationUnitPerformanceTable.Value(name_Supply, new double[] { 25.0, 22.0, 60.0 }), 12);

            //Halfway along one axis only: the mean of the two corners that edge joins.
            Assert.Equal(1.5, ventilationUnitPerformanceTable.Value(name_Supply, new double[] { 20.0, 20.0, 60.0 }), 12);
            Assert.Equal(2.5, ventilationUnitPerformanceTable.Value(name_Supply, new double[] { 20.0, 22.0, 40.0 }), 12);
            Assert.Equal(8.5, ventilationUnitPerformanceTable.Value(name_Supply, new double[] { 25.0, 20.0, 40.0 }), 12);

            //A quarter of the way, on one axis: 1 + 0.25 * (2 - 1).
            Assert.Equal(1.25, ventilationUnitPerformanceTable.Value(name_Supply, new double[] { 20.0, 20.0, 50.0 }), 12);

            //And it does not depend on how the question was asked.
            Assert.Equal(
                ventilationUnitPerformanceTable.Value(name_Supply, new double[] { 25.0, 22.0, 60.0 }),
                ventilationUnitPerformanceTable.PerformanceValue(name_Supply, new Dictionary<string, double>
                {
                    { name_Flow, 60.0 },
                    { name_Entering, 22.0 },
                    { name_External, 25.0 },
                }));
        }

        /// <summary>
        /// <b>Outside the published conditions the default is to refuse.</b> Nothing extrapolates by
        /// accident, on any axis, in either direction.
        /// </summary>
        [Fact]
        public void OutOfDomain_RefusesByDefault()
        {
            VentilationUnitPerformanceTable ventilationUnitPerformanceTable = FixtureTable();

            Assert.True(double.IsNaN(ventilationUnitPerformanceTable.Value(name_Supply, new double[] { 28.9, 24.0, 80.0 })));
            Assert.True(double.IsNaN(ventilationUnitPerformanceTable.Value(name_Supply, new double[] { 34.1, 24.0, 80.0 })));
            Assert.True(double.IsNaN(ventilationUnitPerformanceTable.Value(name_Supply, new double[] { 32.0, 26.1, 80.0 })));
            Assert.True(double.IsNaN(ventilationUnitPerformanceTable.Value(name_Supply, new double[] { 32.0, 24.0, 49.0 })));
            Assert.True(double.IsNaN(ventilationUnitPerformanceTable.Value(name_Supply, new double[] { 32.0, 24.0, 121.0 })));

            Assert.False(ventilationUnitPerformanceTable.InDomain(35.0, 24.0, 80.0));
            Assert.True(ventilationUnitPerformanceTable.InDomain(34.0, 26.0, 120.0));

            //An unknown output is not a domain question and is refused too.
            Assert.True(double.IsNaN(ventilationUnitPerformanceTable.Value("NoSuchOutput", new double[] { 32.0, 24.0, 80.0 })));
        }

        /// <summary>
        /// Clamping holds the published edge, and it happens only when it is asked for by name.
        /// </summary>
        [Fact]
        public void OutOfDomain_ClampsOnlyWhenAsked()
        {
            VentilationUnitPerformanceTable ventilationUnitPerformanceTable = CornerTable();

            //Beyond the top of the flow axis, holding (20, 20, 80) = 2.
            Assert.Equal(2.0, ventilationUnitPerformanceTable.Value(name_Supply, new double[] { 20.0, 20.0, 200.0 }, PerformanceDomainPolicy.ClampToDomain), 12);

            //Below the bottom of every axis, holding (20, 20, 40) = 1.
            Assert.Equal(1.0, ventilationUnitPerformanceTable.Value(name_Supply, new double[] { 0.0, 0.0, 0.0 }, PerformanceDomainPolicy.ClampToDomain), 12);

            //Inside, the policy changes nothing at all.
            Assert.Equal(
                ventilationUnitPerformanceTable.Value(name_Supply, new double[] { 25.0, 22.0, 60.0 }),
                ventilationUnitPerformanceTable.Value(name_Supply, new double[] { 25.0, 22.0, 60.0 }, PerformanceDomainPolicy.ClampToDomain),
                12);
        }

        /// <summary>
        /// <b>Extrapolation is a named compatibility behaviour and is never the default.</b> It reproduces
        /// the legacy route's "Extrapolate ticked" setting - which existed to answer entering dry bulbs
        /// above the published 26 &#176;C - and everything it produces is this library's arithmetic rather
        /// than the manufacturer's data.
        /// </summary>
        [Fact]
        public void OutOfDomain_ExtrapolatesOnlyUnderTheNamedLegacyPolicy()
        {
            VentilationUnitPerformanceTable ventilationUnitPerformanceTable = CornerTable();

            //Twice as far along the flow axis as the last published point: 1 + 2 * (2 - 1) = 3.
            Assert.Equal(3.0, ventilationUnitPerformanceTable.Value(name_Supply, new double[] { 20.0, 20.0, 120.0 }, PerformanceDomainPolicy.OuterCellLinearExtrapolation), 12);

            //The default and the clamp both decline to produce that number.
            Assert.True(double.IsNaN(ventilationUnitPerformanceTable.Value(name_Supply, new double[] { 20.0, 20.0, 120.0 })));
            Assert.Equal(2.0, ventilationUnitPerformanceTable.Value(name_Supply, new double[] { 20.0, 20.0, 120.0 }, PerformanceDomainPolicy.ClampToDomain), 12);

            //Undefined is not a licence to extrapolate - it behaves as Refuse.
            Assert.True(double.IsNaN(ventilationUnitPerformanceTable.Value(name_Supply, new double[] { 20.0, 20.0, 120.0 }, PerformanceDomainPolicy.Undefined)));
        }

        /// <summary>
        /// Conditions are named, so a table written with its axes in a different order answers identically.
        /// The order two temperature axes were typed in is not allowed to be load-bearing.
        /// </summary>
        [Fact]
        public void ALookupByNamedConditions_IsAxisOrderIndependent()
        {
            VentilationUnitPerformanceTable ventilationUnitPerformanceTable = CornerTable();
            VentilationUnitPerformanceTable ventilationUnitPerformanceTable_Transposed = CornerTableTransposed();

            Dictionary<string, double> conditions = new()
            {
                { name_External, 25.0 },
                { name_Entering, 22.0 },
                { name_Flow, 60.0 },
            };

            Assert.Equal(
                ventilationUnitPerformanceTable.PerformanceValue(name_Supply, conditions),
                ventilationUnitPerformanceTable_Transposed.PerformanceValue(name_Supply, conditions),
                12);

            //A published point too, so the agreement is not an artefact of interpolation.
            Dictionary<string, double> conditions_Published = new()
            {
                { name_External, 30.0 },
                { name_Entering, 24.0 },
                { name_Flow, 80.0 },
            };

            Assert.Equal(128.0, ventilationUnitPerformanceTable.PerformanceValue(name_Supply, conditions_Published), 12);
            Assert.Equal(128.0, ventilationUnitPerformanceTable_Transposed.PerformanceValue(name_Supply, conditions_Published), 12);
        }

        /// <summary>
        /// A condition nobody supplied, and a condition nobody recognises, are both refused rather than
        /// filled in. There is no neutral value for "the outdoor temperature".
        /// </summary>
        [Fact]
        public void AMissingOrUnrecognisedCondition_Refuses()
        {
            VentilationUnitPerformanceTable ventilationUnitPerformanceTable = CornerTable();

            Assert.True(double.IsNaN(ventilationUnitPerformanceTable.PerformanceValue(name_Supply, new Dictionary<string, double>
            {
                { name_External, 25.0 },
                { name_Entering, 22.0 },
            })));

            Assert.True(double.IsNaN(ventilationUnitPerformanceTable.PerformanceValue(name_Supply, new Dictionary<string, double>
            {
                { name_External, 25.0 },
                { name_Entering, 22.0 },
                { "AirFlowRte", 60.0 },
            })));
        }

        /// <summary>
        /// A grid whose values no longer line up with its axes is refused outright. It would otherwise
        /// answer every query, with each number attributed to the wrong conditions - the one failure
        /// nothing downstream could detect.
        /// </summary>
        [Fact]
        public void AGridThatDoesNotLineUpWithItsAxes_IsRefused()
        {
            VentilationUnitPerformanceTable ventilationUnitPerformanceTable = new(
                [
                    new VentilationUnitPerformanceAxis(name_External, "degC", [20.0, 30.0]),
                    new VentilationUnitPerformanceAxis(name_Flow, "l/s", [40.0, 80.0]),
                ],
                [new VentilationUnitPerformanceOutput(name_Supply, "degC", [1.0, 2.0, 3.0])]);

            Assert.False(ventilationUnitPerformanceTable.IsValid);
            Assert.True(double.IsNaN(ventilationUnitPerformanceTable.Value(name_Supply, new double[] { 25.0, 60.0 })));
            Assert.True(double.IsNaN(ventilationUnitPerformanceTable.PublishedValue(name_Supply, 0, 0)));

            //A repeated coordinate is refused for the same reason - a zero-width cell has no honest value.
            VentilationUnitPerformanceTable ventilationUnitPerformanceTable_Repeated = new(
                [new VentilationUnitPerformanceAxis(name_Flow, "l/s", [40.0, 40.0])],
                [new VentilationUnitPerformanceOutput(name_Supply, "degC", [1.0, 2.0])]);

            Assert.False(ventilationUnitPerformanceTable_Repeated.IsValid);
        }

        // =================================================================================================
        // F. The control curve - template data, not an algorithm
        // =================================================================================================

        /// <summary>
        /// The ramp the Nuaire arrangement is controlled on is expressible as data: 30% of full airflow at
        /// 22 &#176;C, 100% at 26 &#176;C.
        /// </summary>
        [Fact]
        public void TheControlCurve_Represents22DegreesTo30PercentAnd26DegreesTo100Percent()
        {
            FlowFractionControlCurve flowFractionControlCurve = ControlCurve();

            Assert.True(flowFractionControlCurve.IsValid);
            Assert.Equal(0.3, flowFractionControlCurve.FlowFraction(22));
            Assert.Equal(1.0, flowFractionControlCurve.FlowFraction(26));

            Assert.Equal(22, flowFractionControlCurve.MinimumControlTemperature_C, 12);
            Assert.Equal(26, flowFractionControlCurve.MaximumControlTemperature_C, 12);
        }

        /// <summary>Between the two stated points it interpolates, deterministically.</summary>
        [Fact]
        public void TheControlCurve_InterpolatesBetweenItsStatedPoints()
        {
            FlowFractionControlCurve flowFractionControlCurve = ControlCurve();

            Assert.Equal(0.65, flowFractionControlCurve.FlowFraction(24), 12);
            Assert.Equal(0.475, flowFractionControlCurve.FlowFraction(23), 12);
            Assert.Equal(0.825, flowFractionControlCurve.FlowFraction(25), 12);
        }

        /// <summary>
        /// <b>Saturating, because the source says so.</b> "100% at 26 degrees and above" is a statement
        /// about behaviour beyond 26, so the curve stores <see cref="PerformanceDomainPolicy.ClampToDomain"/>
        /// and holds flat in both directions. A caller can still ask what a stricter policy would say
        /// without editing the curve.
        /// </summary>
        [Fact]
        public void TheControlCurve_SaturatesBeyondItsStatedRange()
        {
            FlowFractionControlCurve flowFractionControlCurve = ControlCurve();

            Assert.Equal(PerformanceDomainPolicy.ClampToDomain, flowFractionControlCurve.PerformanceDomainPolicy);

            Assert.Equal(1.0, flowFractionControlCurve.FlowFraction(30), 12);
            Assert.Equal(0.3, flowFractionControlCurve.FlowFraction(18), 12);

            Assert.False(flowFractionControlCurve.InDomain(30));
            Assert.True(flowFractionControlCurve.InDomain(24));

            //Overridden on the call, never on the stored curve.
            Assert.True(double.IsNaN(flowFractionControlCurve.FlowFraction(30, PerformanceDomainPolicy.Refuse)));
            Assert.Equal(PerformanceDomainPolicy.ClampToDomain, flowFractionControlCurve.PerformanceDomainPolicy);
        }

        /// <summary>
        /// The curve is template data, so a different product controls differently with no code anywhere
        /// changing - which is the property that makes adding a second manufacturer adding a file.
        /// </summary>
        [Fact]
        public void AControlCurve_IsTemplateDataRatherThanAnAlgorithm()
        {
            FlowFractionControlCurve flowFractionControlCurve_Stepped = new([19.0, 21.0, 24.0], [0.25, 0.5, 1.0]);

            Assert.True(flowFractionControlCurve_Stepped.IsValid);
            Assert.Equal(0.25, flowFractionControlCurve_Stepped.FlowFraction(19), 12);
            Assert.Equal(0.375, flowFractionControlCurve_Stepped.FlowFraction(20), 12);
            Assert.Equal(1.0, flowFractionControlCurve_Stepped.FlowFraction(24), 12);

            //Nothing anywhere holds a 22 or a 26 - the numbers came from the curve that was handed in.
            Assert.NotEqual(ControlCurve().FlowFraction(22), flowFractionControlCurve_Stepped.FlowFraction(22), 12);
        }

        /// <summary>
        /// A fraction outside 0 to 1 is a transcription mistake and is refused rather than clamped -
        /// clamping it would produce a controller that looks reasonable and is not the one anybody wrote
        /// down.
        /// </summary>
        [Fact]
        public void AFlowFractionOutsideZeroToOne_IsRefused()
        {
            Assert.False(new FlowFractionControlCurve([22.0, 26.0], [0.3, 1.4]).IsValid);
            Assert.False(new FlowFractionControlCurve([22.0, 26.0], [-0.1, 1.0]).IsValid);
            Assert.True(double.IsNaN(new FlowFractionControlCurve([22.0, 26.0], [0.3, 1.4]).FlowFraction(24)));
        }

        // =================================================================================================
        // F2. Typed units - the curve proves degC/dimensionless, rather than trusting the axis name
        // =================================================================================================

        /// <summary>
        /// <b>The Celsius-typed API must prove the unit, not just the axis name.</b> A control-temperature
        /// axis that exists but is declared in Fahrenheit must not be read through
        /// <see cref="FlowFractionControlCurve.ControlTemperatures_C"/> or <c>FlowFraction</c> as though its
        /// raw numbers were already Celsius.
        /// </summary>
        [Fact]
        public void AControlTemperatureAxisDeclaredInFahrenheit_IsRefusedRatherThanReadAsCelsius()
        {
            VentilationUnitPerformanceTable ventilationUnitPerformanceTable = new(
                [new VentilationUnitPerformanceAxis(VentilationUnitPerformanceAxis.Name_ControlTemperature, "degF", [71.6, 78.8])],
                [new VentilationUnitPerformanceOutput(VentilationUnitPerformanceOutput.Name_FlowFraction, VentilationUnitPerformanceOutput.Unit_Dimensionless, [0.3, 1.0])]);

            FlowFractionControlCurve flowFractionControlCurve = CurveFromTable(ventilationUnitPerformanceTable);

            Assert.False(flowFractionControlCurve.IsValid);
            Assert.Null(flowFractionControlCurve.ControlTemperatures_C);
            Assert.True(double.IsNaN(flowFractionControlCurve.MinimumControlTemperature_C));
            Assert.True(double.IsNaN(flowFractionControlCurve.MaximumControlTemperature_C));
            Assert.True(double.IsNaN(flowFractionControlCurve.FlowFraction(22)));
        }

        /// <summary>A control-temperature axis with no declared unit at all is refused the same way - there is no neutral unit to assume on a manufacturer's behalf.</summary>
        [Fact]
        public void AControlTemperatureAxisWithNoDeclaredUnit_IsRefused()
        {
            VentilationUnitPerformanceTable ventilationUnitPerformanceTable = new(
                [new VentilationUnitPerformanceAxis(VentilationUnitPerformanceAxis.Name_ControlTemperature, null, [22.0, 26.0])],
                [new VentilationUnitPerformanceOutput(VentilationUnitPerformanceOutput.Name_FlowFraction, VentilationUnitPerformanceOutput.Unit_Dimensionless, [0.3, 1.0])]);

            Assert.False(CurveFromTable(ventilationUnitPerformanceTable).IsValid);
        }

        /// <summary>
        /// The flow-fraction output's dimensionless convention is proven, not assumed - an output
        /// mistakenly carrying a physical unit (here "kW") is refused rather than treated as a fraction.
        /// </summary>
        [Fact]
        public void AFlowFractionOutputNotDeclaredDimensionless_IsRefused()
        {
            VentilationUnitPerformanceTable ventilationUnitPerformanceTable = new(
                [new VentilationUnitPerformanceAxis(VentilationUnitPerformanceAxis.Name_ControlTemperature, VentilationUnitPerformanceAxis.Unit_DegreesCelsius, [22.0, 26.0])],
                [new VentilationUnitPerformanceOutput(VentilationUnitPerformanceOutput.Name_FlowFraction, "kW", [0.3, 1.0])]);

            Assert.False(CurveFromTable(ventilationUnitPerformanceTable).IsValid);
        }

        // =================================================================================================
        // G. The maths underneath, directly
        // =================================================================================================

        /// <summary>
        /// The interpolator is exact at every node of a grid, in every dimension - which is what lets a
        /// performance lookup quote a document.
        /// </summary>
        [Fact]
        public void TheInterpolator_IsExactAtEveryNode()
        {
            double[] axis_1 = [29.0, 32.0, 34.0];
            double[] axis_2 = [23.0, 24.0, 25.0, 26.0];
            double[] axis_3 = [50.0, 60.0, 70.0, 80.0, 90.0, 100.0, 110.0, 120.0];

            List<double> values = [];
            for (int i = 0; i < axis_1.Length; i++)
            {
                for (int j = 0; j < axis_2.Length; j++)
                {
                    for (int k = 0; k < axis_3.Length; k++)
                    {
                        values.Add(Supply(i, j, k));
                    }
                }
            }

            SAM.Math.MultilinearInterpolation multilinearInterpolation = new(new double[][] { axis_1, axis_2, axis_3 }, values);

            Assert.True(multilinearInterpolation.IsValid);
            Assert.Equal(3, multilinearInterpolation.Dimensions);
            Assert.Equal(96, multilinearInterpolation.Count);

            for (int i = 0; i < axis_1.Length; i++)
            {
                for (int j = 0; j < axis_2.Length; j++)
                {
                    for (int k = 0; k < axis_3.Length; k++)
                    {
                        Assert.Equal(Supply(i, j, k), multilinearInterpolation.Calculate(axis_1[i], axis_2[j], axis_3[k]));
                    }
                }
            }
        }

        /// <summary>
        /// The interpolator refuses a grid it cannot answer from rather than answering from part of one:
        /// a value count that does not match the axes, an axis that goes backwards, a hole in the values.
        /// </summary>
        [Fact]
        public void TheInterpolator_RefusesAGridItCannotAnswerFrom()
        {
            Assert.False(new SAM.Math.MultilinearInterpolation(new double[][] { [1, 2] }, new double[] { 1, 2, 3 }).IsValid);
            Assert.False(new SAM.Math.MultilinearInterpolation(new double[][] { [2, 1] }, new double[] { 1, 2 }).IsValid);
            Assert.False(new SAM.Math.MultilinearInterpolation(new double[][] { [1, 2] }, new double[] { 1, double.NaN }).IsValid);
            Assert.False(new SAM.Math.MultilinearInterpolation(new double[][] { }, new double[] { 1 }).IsValid);

            //An axis of one coordinate is legal and makes that dimension constant - it has no gradient to
            //continue, so even the extrapolating call stays flat.
            SAM.Math.MultilinearInterpolation multilinearInterpolation = new(new double[][] { [5], [1, 2] }, new double[] { 10, 20 });

            Assert.True(multilinearInterpolation.IsValid);
            Assert.Equal(15, multilinearInterpolation.Calculate(5, 1.5), 12);
            Assert.Equal(20, multilinearInterpolation.CalculateExtrapolated(99, 2), 12);
        }

        /// <summary>
        /// Many constant (singleton) dimensions around one real value answer with that value, however many
        /// of them there are. This is the shape the corner-enumeration finding was about: the OLD code
        /// still allocated and evaluated a nominal corner per singleton axis - <c>2^dimensions</c> of them,
        /// all but one immediately zero-weighted and discarded - rather than recognising that a singleton
        /// axis contributes no corner at all.
        /// </summary>
        [Fact]
        public void ManySingletonAxes_AnswerWithTheSingleRealValue()
        {
            double[][] axes = new double[20][];
            double[] coordinates = new double[20];

            for (int i = 0; i < 20; i++)
            {
                axes[i] = new double[] { i };
                coordinates[i] = i;
            }

            SAM.Math.MultilinearInterpolation multilinearInterpolation = new(axes, new double[] { 42.0 });

            Assert.True(multilinearInterpolation.IsValid);
            Assert.Equal(20, multilinearInterpolation.Dimensions);

            Assert.Equal(42.0, multilinearInterpolation.Calculate(coordinates));
            Assert.Equal(42.0, multilinearInterpolation.CalculateClamped(coordinates));
            Assert.Equal(42.0, multilinearInterpolation.CalculateExtrapolated(coordinates));
        }

        /// <summary>
        /// A mixture of singleton and varying axes interpolates only over the varying ones - the singleton
        /// axes hold their one coordinate and contribute no corner, exactly as a two-dimensional table
        /// would if the singleton axes were not there at all (see the next test).
        /// </summary>
        [Fact]
        public void AMixOfSingletonAndVaryingAxes_InterpolatesOnlyOverTheVaryingOnes()
        {
            SAM.Math.MultilinearInterpolation multilinearInterpolation = MixedSingletonAndVaryingInterpolation();

            Assert.True(multilinearInterpolation.IsValid);
            Assert.Equal(4, multilinearInterpolation.Dimensions);

            //Exact at every real node, singleton coordinates included.
            Assert.Equal(0.0, multilinearInterpolation.Calculate(5, 10, 100, 0), 12);
            Assert.Equal(12.0, multilinearInterpolation.Calculate(5, 20, 100, 2), 12);

            //Halfway between the two varying-axis nodes that bracket it.
            Assert.Equal(6.0, multilinearInterpolation.Calculate(5, 15, 100, 1), 12);
        }

        /// <summary>
        /// The same table with its singleton axes stripped out entirely is a lower-dimensional table over
        /// the identical flattened values - row-major flattening gives a singleton axis a multiplier of
        /// exactly one, so the two tables share one values array. Interpolating the full table at a
        /// singleton axis's one coordinate must equal interpolating the reduced table without it.
        /// </summary>
        [Fact]
        public void SingletonAxesInflated_MatchTheEquivalentLowerDimensionalTable()
        {
            SAM.Math.MultilinearInterpolation full = MixedSingletonAndVaryingInterpolation();

            SAM.Math.MultilinearInterpolation reduced = new(
                new double[][] { new double[] { 10.0, 20.0 }, new double[] { 0.0, 1.0, 2.0 } },
                new double[] { 0, 1, 2, 10, 11, 12 });

            Assert.True(reduced.IsValid);
            Assert.Equal(2, reduced.Dimensions);

            Assert.Equal(reduced.Calculate(15, 1), full.Calculate(5, 15, 100, 1), 12);
            Assert.Equal(reduced.Calculate(12, 1.5), full.Calculate(5, 12, 100, 1.5), 12);
        }

        // =================================================================================================
        // H. The C / l-s / kW convenience seam refuses units it was not told to expect
        // =================================================================================================

        /// <summary>
        /// The baseline the refusal tests below are contrasted against: a table declaring exactly &#176;C /
        /// &#176;C / l/s on its axes and &#176;C / kW on its outputs answers normally through the typed
        /// convenience calls.
        /// </summary>
        [Fact]
        public void TypedLookup_AcceptsATableDeclaringDegCLpsAndKw()
        {
            VentilationUnitTemplate ventilationUnitTemplate = Template("UNIT-TYPED-OK", 120, 100);

            Assert.Equal(Supply(0, 0, 0), ventilationUnitTemplate.SupplyAirTemperature_C(29, 23, 50), 12);
            Assert.Equal(Cooling(0, 0, 0), ventilationUnitTemplate.CombinedCoolingCapacity_kW(29, 23, 50), 12);
        }

        /// <summary>
        /// A required axis left with no declared unit at all refuses the typed call, even though the raw
        /// table is otherwise perfectly valid and perfectly readable through the generic lookup.
        /// </summary>
        [Fact]
        public void TypedLookup_RefusesWhenARequiredAxisUnitIsMissing()
        {
            VentilationUnitPerformanceTable ventilationUnitPerformanceTable = new(
                [
                    new VentilationUnitPerformanceAxis(name_External, null, [29.0]),
                    new VentilationUnitPerformanceAxis(name_Entering, "degC", [23.0]),
                    new VentilationUnitPerformanceAxis(name_Flow, "l/s", [50.0]),
                ],
                [new VentilationUnitPerformanceOutput(name_Supply, "degC", [15.0])]);

            //Unit is not part of IsValid - the raw table is fine, and the generic API answers from it.
            Assert.True(ventilationUnitPerformanceTable.IsValid);
            Assert.Equal(15.0, ventilationUnitPerformanceTable.PerformanceValue(name_Supply, RawConditions()));

            VentilationUnitTemplate ventilationUnitTemplate = TemplateWithTable(ventilationUnitPerformanceTable);

            Assert.True(double.IsNaN(ventilationUnitTemplate.SupplyAirTemperature_C(29, 23, 50)));
        }

        /// <summary>A temperature axis published in anything other than &#176;C refuses, rather than being read as Celsius.</summary>
        [Fact]
        public void TypedLookup_RefusesAWrongTemperatureUnit()
        {
            VentilationUnitPerformanceTable ventilationUnitPerformanceTable = new(
                [
                    new VentilationUnitPerformanceAxis(name_External, "degF", [29.0]),
                    new VentilationUnitPerformanceAxis(name_Entering, "degC", [23.0]),
                    new VentilationUnitPerformanceAxis(name_Flow, "l/s", [50.0]),
                ],
                [new VentilationUnitPerformanceOutput(name_Supply, "degC", [15.0])]);

            VentilationUnitTemplate ventilationUnitTemplate = TemplateWithTable(ventilationUnitPerformanceTable);

            Assert.True(double.IsNaN(ventilationUnitTemplate.SupplyAirTemperature_C(29, 23, 50)));

            //The raw API is unaffected - the table itself is a legitimate Fahrenheit/Celsius/l/s mixture.
            Assert.Equal(15.0, ventilationUnitPerformanceTable.PerformanceValue(name_Supply, RawConditions()));
        }

        /// <summary>An airflow axis published in anything other than l/s refuses, rather than being read as litres per second.</summary>
        [Fact]
        public void TypedLookup_RefusesAWrongAirflowUnit()
        {
            VentilationUnitPerformanceTable ventilationUnitPerformanceTable = new(
                [
                    new VentilationUnitPerformanceAxis(name_External, "degC", [29.0]),
                    new VentilationUnitPerformanceAxis(name_Entering, "degC", [23.0]),
                    new VentilationUnitPerformanceAxis(name_Flow, "m3/s", [50.0]),
                ],
                [new VentilationUnitPerformanceOutput(name_Supply, "degC", [15.0])]);

            VentilationUnitTemplate ventilationUnitTemplate = TemplateWithTable(ventilationUnitPerformanceTable);

            Assert.True(double.IsNaN(ventilationUnitTemplate.SupplyAirTemperature_C(29, 23, 50)));
        }

        /// <summary>
        /// An output published in anything other than its expected unit refuses - a &#176;C claim on a
        /// Kelvin column, or a kW claim on a BTU/h one, must not be handed back as if it had been converted.
        /// </summary>
        [Fact]
        public void TypedLookup_RefusesAWrongOutputUnit()
        {
            VentilationUnitPerformanceTable ventilationUnitPerformanceTable = new(
                [
                    new VentilationUnitPerformanceAxis(name_External, "degC", [29.0]),
                    new VentilationUnitPerformanceAxis(name_Entering, "degC", [23.0]),
                    new VentilationUnitPerformanceAxis(name_Flow, "l/s", [50.0]),
                ],
                [
                    new VentilationUnitPerformanceOutput(name_Supply, "K", [15.0]),
                    new VentilationUnitPerformanceOutput(name_Cooling, "BTU/h", [1.0]),
                ]);

            VentilationUnitTemplate ventilationUnitTemplate = TemplateWithTable(ventilationUnitPerformanceTable);

            Assert.True(double.IsNaN(ventilationUnitTemplate.SupplyAirTemperature_C(29, 23, 50)));
            Assert.True(double.IsNaN(ventilationUnitTemplate.CombinedCoolingCapacity_kW(29, 23, 50)));

            //The raw API answers both regardless - it was never told to expect Celsius or kW.
            Assert.Equal(15.0, ventilationUnitPerformanceTable.PerformanceValue(name_Supply, RawConditions()));
            Assert.Equal(1.0, ventilationUnitPerformanceTable.PerformanceValue(name_Cooling, RawConditions()));
        }

        /// <summary>
        /// <b>The generic raw table stays generic.</b> A table published entirely in other units -
        /// Fahrenheit, cubic feet per minute, Kelvin - is fully readable through
        /// <see cref="PerformanceValue(VentilationUnitPerformanceTable, string, IDictionary{string, double}, PerformanceDomainPolicy)"/>.
        /// Only the named &#176;C/l-s/kW convenience seam refuses it, and it must not be hardened into
        /// rejecting non-SI or non-Nuaire units generally.
        /// </summary>
        [Fact]
        public void GenericPerformanceValue_StillReadsArbitraryManufacturerUnits()
        {
            VentilationUnitPerformanceTable ventilationUnitPerformanceTable = new(
                [
                    new VentilationUnitPerformanceAxis(name_External, "degF", [84.0]),
                    new VentilationUnitPerformanceAxis(name_Entering, "degF", [73.0]),
                    new VentilationUnitPerformanceAxis(name_Flow, "cfm", [106.0]),
                ],
                [new VentilationUnitPerformanceOutput(name_Supply, "K", [288.0])]);

            Assert.True(ventilationUnitPerformanceTable.IsValid);
            Assert.Equal(288.0, ventilationUnitPerformanceTable.PerformanceValue(name_Supply, RawConditions(84.0, 73.0, 106.0)));

            VentilationUnitTemplate ventilationUnitTemplate = TemplateWithTable(ventilationUnitPerformanceTable);

            Assert.True(double.IsNaN(ventilationUnitTemplate.SupplyAirTemperature_C(84, 73, 106)));
        }

        // =================================================================================================
        // I. A reload mid-lookup must never leave a stale value cached
        // =================================================================================================

        /// <summary>
        /// <b>Pins the cache/generation race, deterministically - no threads, no sleeps.</b>
        /// <para>
        /// <see cref="VentilationUnitPerformanceTable.OnInterpolationSnapshotCaptured"/> is a test-only seam
        /// that fires at the exact point a concurrent <c>FromJsonObject</c> reload used to be able to land:
        /// after a lookup has captured its pre-build snapshot of the axes/output, but before it has finished
        /// building and caching the interpolator from them. Firing a reload synchronously from that hook
        /// reproduces the race on a single thread, every time.
        /// </para>
        /// <para>
        /// Without the generation check, the first lookup's in-flight build (holding the OLD value) would be
        /// written into the cache the reload just cleared, and the SECOND lookup - which never saw the old
        /// table at all - would read that stale value back instead of building fresh from the NEW table.
        /// </para>
        /// </summary>
        [Fact]
        public void AReloadDuringAnInFlightLookup_NeverLeavesTheOldValueCached()
        {
            VentilationUnitPerformanceTable seed_Before = new(
                [new VentilationUnitPerformanceAxis(name_Flow, "l/s", [50.0])],
                [new VentilationUnitPerformanceOutput(name_Supply, "degC", [10.0])]);

            VentilationUnitPerformanceTable seed_After = new(
                [new VentilationUnitPerformanceAxis(name_Flow, "l/s", [50.0])],
                [new VentilationUnitPerformanceOutput(name_Supply, "degC", [99.0])]);

            VentilationUnitPerformanceTable ventilationUnitPerformanceTable = new();
            ventilationUnitPerformanceTable.FromJsonObject(seed_Before.ToJsonObject());

            ventilationUnitPerformanceTable.OnInterpolationSnapshotCaptured = () =>
            {
                //Fires once - a second reload landing in the same window is not what this test is about.
                ventilationUnitPerformanceTable.OnInterpolationSnapshotCaptured = null;
                ventilationUnitPerformanceTable.FromJsonObject(seed_After.ToJsonObject());
            };

            double value_First = ventilationUnitPerformanceTable.Value(name_Supply, new double[] { 50.0 });
            double value_Second = ventilationUnitPerformanceTable.Value(name_Supply, new double[] { 50.0 });

            //The in-flight build still returns a correct answer - correct for the pre-reload table it was
            //actually built from, which is what it saw before the reload it is unaware of.
            Assert.Equal(10.0, value_First);

            //The point of the fix: the second, unrelated lookup was never told about the first one's stale
            //build, so it rebuilds from the CURRENT table rather than reading back what the first call
            //would otherwise have wrongly cached.
            Assert.Equal(99.0, value_Second);
        }

        /// <summary>
        /// <b>Pins the atomic-replacement finding, deterministically.</b>
        /// <para>
        /// <see cref="VentilationUnitPerformanceTable.OnReplacementLocalsPrepared"/> fires after
        /// <c>FromJsonObject</c> has parsed the incoming axes/outputs into locals but before it publishes
        /// them. A reader that runs from that hook is exercising the exact window a non-atomic replacement
        /// would expose: before the fix, the live axes/outputs fields had already been nulled out (and, for
        /// part of the window, were being rebuilt element by element) well before this point. A reader here
        /// must instead still see the complete PRE-reload table, never a nulled or half-rebuilt one.
        /// </para>
        /// </summary>
        [Fact]
        public void AReloadInProgress_NeverExposesANulledOrPartiallyRebuiltTable()
        {
            VentilationUnitPerformanceTable seed_Before = new(
                [new VentilationUnitPerformanceAxis(name_Flow, "l/s", [50.0, 60.0])],
                [new VentilationUnitPerformanceOutput(name_Supply, "degC", [10.0, 20.0])]);

            VentilationUnitPerformanceTable seed_After = new(
                [new VentilationUnitPerformanceAxis(name_Flow, "l/s", [70.0])],
                [new VentilationUnitPerformanceOutput(name_Supply, "degC", [99.0])]);

            VentilationUnitPerformanceTable ventilationUnitPerformanceTable = new();
            ventilationUnitPerformanceTable.FromJsonObject(seed_Before.ToJsonObject());

            List<string> axisNames_SeenMidReload = null;
            int? pointCount_SeenMidReload = null;

            ventilationUnitPerformanceTable.OnReplacementLocalsPrepared = () =>
            {
                //Fires once - a second reload landing in the same window is not what this test is about.
                ventilationUnitPerformanceTable.OnReplacementLocalsPrepared = null;

                axisNames_SeenMidReload = ventilationUnitPerformanceTable.AxisNames;
                pointCount_SeenMidReload = ventilationUnitPerformanceTable.PointCount;
            };

            ventilationUnitPerformanceTable.FromJsonObject(seed_After.ToJsonObject());

            //Mid-reload, the reader saw the complete OLD (2-point) table - not null, and not the 1-point NEW
            //table that was still only a local at that moment.
            Assert.NotNull(axisNames_SeenMidReload);
            Assert.Equal(2, pointCount_SeenMidReload);

            //And once the reload has returned, the table is the fully-published NEW one.
            Assert.Equal(1, ventilationUnitPerformanceTable.PointCount);
            Assert.Equal(99.0, ventilationUnitPerformanceTable.Value(name_Supply, new double[] { 70.0 }));
        }

        /// <summary>
        /// <b>Pins the "new axes + old outputs" finding, deterministically.</b>
        /// <para>
        /// The point: moving the axes/outputs swap under a lock made it atomic to a reader that ALSO
        /// takes that lock, but <c>PublishedValue</c>, <c>ToJsonObject</c>, <c>IsValid</c> and the
        /// <c>Axis</c>/<c>Output</c> accessors never did - so two sequential field assignments were not
        /// atomic to any of them, and a reader landing between the two could see the NEW axes' shape
        /// combined with the OLD outputs' values (or vice versa). If the two tables' point counts happen
        /// to coincide - as they deliberately do here - that is not a crash or a refusal: it is
        /// <c>PublishedValue</c> indexing into the wrong output row and returning a plausible-looking but
        /// WRONG manufacturer value.
        /// </para>
        /// <para>
        /// <b>Why the shapes below are chosen the way they are.</b> BEFORE is a 2 &#215; 3 table (6 points);
        /// AFTER is a reshaped 3 &#215; 2 table - the SAME 6-point total, but different per-axis lengths, so
        /// the flattening index math genuinely differs between the two rather than merely relabelling the
        /// same layout. At coordinate (1, 1), all four possible pairings - old axes/old outputs, new/new,
        /// new axes/old outputs, old axes/new outputs - read back four DIFFERENT numbers (22, 96, 21, 95).
        /// A reader that ever saw a torn pairing would return 21 or 95; this test proves it never does.
        /// </para>
        /// </summary>
        [Fact]
        public void AReloadInProgress_NeverPairsAxesFromOneGenerationWithOutputsFromAnother()
        {
            VentilationUnitPerformanceTable seed_Before = new(
                [
                    new VentilationUnitPerformanceAxis(name_External, "degC", [29.0, 32.0]),
                    new VentilationUnitPerformanceAxis(name_Flow, "l/s", [50.0, 60.0, 70.0]),
                ],
                [new VentilationUnitPerformanceOutput(name_Supply, "degC", [11.0, 12.0, 13.0, 21.0, 22.0, 23.0])]);

            //Reshaped - same 6-point total, different per-axis lengths - so a torn pairing's index math
            //genuinely diverges from either correct answer rather than coincidentally matching one.
            VentilationUnitPerformanceTable seed_After = new(
                [
                    new VentilationUnitPerformanceAxis(name_External, "degC", [29.0, 32.0, 35.0]),
                    new VentilationUnitPerformanceAxis(name_Flow, "l/s", [50.0, 60.0]),
                ],
                [new VentilationUnitPerformanceOutput(name_Supply, "degC", [99.0, 98.0, 97.0, 96.0, 95.0, 94.0])]);

            //Pins the four-way arithmetic the rest of this test relies on before touching the table under test.
            Assert.Equal(22.0, seed_Before.PublishedValue(name_Supply, 1, 1));
            Assert.Equal(96.0, seed_After.PublishedValue(name_Supply, 1, 1));

            VentilationUnitPerformanceTable ventilationUnitPerformanceTable = new();
            ventilationUnitPerformanceTable.FromJsonObject(seed_Before.ToJsonObject());

            double? value_SeenMidReload = null;
            VentilationUnitPerformanceTable table_SeenMidReload = null;

            ventilationUnitPerformanceTable.OnReplacementLocalsPrepared = () =>
            {
                //Fires once - a second reload landing in the same window is not what this test is about.
                ventilationUnitPerformanceTable.OnReplacementLocalsPrepared = null;

                //PublishedValue and ToJsonObject are both unsynchronized, both touch axes AND outputs, and
                //are exactly the two the finding named - exercised from the one window a two-step field swap
                //could have torn.
                value_SeenMidReload = ventilationUnitPerformanceTable.PublishedValue(name_Supply, 1, 1);
                table_SeenMidReload = new VentilationUnitPerformanceTable(ventilationUnitPerformanceTable.ToJsonObject());
            };

            ventilationUnitPerformanceTable.FromJsonObject(seed_After.ToJsonObject());

            //Mid-reload, both readers saw the complete OLD pairing (22) - never the torn 21 a "new axes,
            //old outputs" combination would produce, and never the NEW pairing either, since the new state
            //had not yet been published at that point.
            Assert.Equal(22.0, value_SeenMidReload);
            Assert.Equal(22.0, table_SeenMidReload.PublishedValue(name_Supply, 1, 1));
            Assert.Equal(2, table_SeenMidReload.Axis(name_External).Count);
            Assert.Equal(3, table_SeenMidReload.Axis(name_Flow).Count);

            //And once the reload has returned, the same reader sees the fully-published NEW pairing (96) -
            //never the other torn combination (95).
            Assert.Equal(96.0, ventilationUnitPerformanceTable.PublishedValue(name_Supply, 1, 1));
        }

        // =================================================================================================
        // J. Strict string fields - a JSON string only, never a stringified number/object/array
        // =================================================================================================

        /// <summary>
        /// <b>"Source": 123 is malformed data, not a number worth reading as text.</b> Before the fix,
        /// <c>PerformanceJson.Text</c> read every JSON scalar through <c>ToString()</c>, so a Source written
        /// as a bare number would silently become the string "123" and the template would look validly
        /// sourced. It must instead read as absent - which, because Source is required for
        /// <see cref="VentilationUnitTemplate.IsValid"/>, refuses the whole template.
        /// </summary>
        [Fact]
        public void ASourceWrittenAsANumber_IsRejectedRatherThanStringified()
        {
            JsonObject jsonObject = Template("UNIT-A", 120, 100).ToJsonObject();
            jsonObject["Source"] = 123;

            VentilationUnitTemplate ventilationUnitTemplate_Malformed = new(jsonObject);

            Assert.Null(ventilationUnitTemplate_Malformed.Source);
            Assert.False(ventilationUnitTemplate_Malformed.IsValid);
        }

        /// <summary>A performance axis Name written as a JSON object reads as absent, the same way - not the object's stringified JSON text.</summary>
        [Fact]
        public void AnAxisNameWrittenAsAJsonObject_IsRejectedRatherThanStringified()
        {
            JsonObject jsonObject_Axis = new()
            {
                ["_type"] = "SAM.Analytical.VentilationUnitPerformanceAxis,SAM.Analytical",
                ["Name"] = new JsonObject(),
                ["Unit"] = "degC",
                ["Values"] = new JsonArray(1.0, 2.0),
            };

            VentilationUnitPerformanceAxis ventilationUnitPerformanceAxis = new(jsonObject_Axis);

            Assert.Null(ventilationUnitPerformanceAxis.Name);
            Assert.False(ventilationUnitPerformanceAxis.IsValid);
        }

        /// <summary>A performance output Unit written as a JSON array reads as absent, the same way.</summary>
        [Fact]
        public void AnOutputUnitWrittenAsAJsonArray_IsRejectedRatherThanStringified()
        {
            JsonObject jsonObject_Output = new()
            {
                ["_type"] = "SAM.Analytical.VentilationUnitPerformanceOutput,SAM.Analytical",
                ["Name"] = VentilationUnitPerformanceOutput.Name_SupplyAirTemperature,
                ["Unit"] = new JsonArray(),
                ["Values"] = new JsonArray(1.0),
            };

            VentilationUnitPerformanceOutput ventilationUnitPerformanceOutput = new(jsonObject_Output);

            Assert.Null(ventilationUnitPerformanceOutput.Unit);
        }

        /// <summary>A genuine JSON string still reads exactly as written - the strict check does not touch this, deliberate, tolerant path.</summary>
        [Fact]
        public void AGenuineStringField_StillReadsCorrectly()
        {
            JsonObject jsonObject_Axis = new()
            {
                ["_type"] = "SAM.Analytical.VentilationUnitPerformanceAxis,SAM.Analytical",
                ["Name"] = VentilationUnitPerformanceAxis.Name_ExternalDryBulbTemperature,
                ["Unit"] = "degC",
                ["Values"] = new JsonArray(29.0, 32.0),
            };

            VentilationUnitPerformanceAxis ventilationUnitPerformanceAxis = new(jsonObject_Axis);

            Assert.Equal(VentilationUnitPerformanceAxis.Name_ExternalDryBulbTemperature, ventilationUnitPerformanceAxis.Name);
            Assert.Equal("degC", ventilationUnitPerformanceAxis.Unit);
            Assert.True(ventilationUnitPerformanceAxis.IsValid);
        }

        // =================================================================================================
        // Fixtures
        // =================================================================================================

        /// <summary>
        /// A 4-D table - singleton, varying[10,20], singleton, varying[0,1,2] - whose flattened values are a
        /// simple function of the two varying axes only: <c>10*j + l</c>. Shared by the singleton-axis
        /// corner-enumeration tests above.
        /// </summary>
        private static SAM.Math.MultilinearInterpolation MixedSingletonAndVaryingInterpolation()
        {
            double[][] axes = new double[][]
            {
                new double[] { 5.0 },
                new double[] { 10.0, 20.0 },
                new double[] { 100.0 },
                new double[] { 0.0, 1.0, 2.0 },
            };

            return new SAM.Math.MultilinearInterpolation(axes, new double[] { 0, 1, 2, 10, 11, 12 });
        }

        /// <summary>A conditions dictionary for the raw <see cref="Query.PerformanceValue"/> API, at the fixture's single tabulated point unless overridden.</summary>
        private static Dictionary<string, double> RawConditions(double external = 29.0, double entering = 23.0, double flow = 50.0)
        {
            return new Dictionary<string, double>
            {
                { name_External, external },
                { name_Entering, entering },
                { name_Flow, flow },
            };
        }

        /// <summary>A minimal template carrying a given performance table and nothing else - the unit-checking tests only care about the table.</summary>
        private static VentilationUnitTemplate TemplateWithTable(VentilationUnitPerformanceTable ventilationUnitPerformanceTable)
        {
            return new VentilationUnitTemplate(Reference("UNIT-UNITS"), source_Fixture)
            {
                PerformanceTable = ventilationUnitPerformanceTable,
            };
        }

        private const string name_External = VentilationUnitPerformanceAxis.Name_ExternalDryBulbTemperature;

        private const string name_Entering = VentilationUnitPerformanceAxis.Name_EnteringDryBulbTemperature;

        private const string name_Flow = VentilationUnitPerformanceAxis.Name_AirFlowRate;

        private const string name_Supply = VentilationUnitPerformanceOutput.Name_SupplyAirTemperature;

        private const string name_Cooling = VentilationUnitPerformanceOutput.Name_CombinedCoolingCapacity;

        /// <summary>A fixture product identity. Not a real manufacturer - see the class remarks.</summary>
        private static VentilationUnitReference Reference(string model)
        {
            return new VentilationUnitReference("Test Fixture", model, "COOL-A");
        }

        /// <summary>A fixture template with both capacities stated.</summary>
        private static VentilationUnitTemplate Template(string model, double maximumSupply_Lps, double maximumExtract_Lps, int rank = 0)
        {
            return new VentilationUnitTemplate(Reference(model), source_Fixture)
            {
                CoolingModuleModel = "COOL-A",
                MaximumSupplyFlowRate_Lps = maximumSupply_Lps,
                MaximumExtractFlowRate_Lps = maximumExtract_Lps,
                Rank = rank,
                PerformanceTable = FixtureTable(),
                FlowFractionByControlTemperature = ControlCurve(),
            };
        }

        /// <summary>
        /// A fixture template with a complete published performance table and <b>no established
        /// capacity</b> - the shape the real Nuaire entry is in, and the one every "do not infer a capacity
        /// from a duty point" test rests on.
        /// </summary>
        private static VentilationUnitTemplate UnresolvedTemplate()
        {
            return new VentilationUnitTemplate(Reference("UNIT-C"), source_Fixture)
            {
                CoolingModuleModel = "COOL-A",
                UnresolvedCapacityNote = "Ask the manufacturer for the unit's rated maximum supply and extract airflow.",
                PerformanceTable = FixtureTable(),
                FlowFractionByControlTemperature = ControlCurve(),
            };
        }

        /// <summary>A fixture catalogue of three resolved products, deliberately not all the same size.</summary>
        private static List<VentilationUnitTemplate> Catalogue()
        {
            return
            [
                Template("UNIT-25", 25, 25),
                Template("UNIT-40", 40, 40),
                Template("UNIT-60", 60, 60),
            ];
        }

        /// <summary>The 22 -&gt; 30%, 26 -&gt; 100% ramp, saturating above and below as its source states.</summary>
        private static FlowFractionControlCurve ControlCurve()
        {
            return new FlowFractionControlCurve([22.0, 26.0], [0.3, 1.0]);
        }

        /// <summary>A curve wrapping a given performance table, via the JSON constructor - the only way to hand the curve a table its own typed constructor would not have built.</summary>
        private static FlowFractionControlCurve CurveFromTable(VentilationUnitPerformanceTable ventilationUnitPerformanceTable)
        {
            JsonObject jsonObject = new()
            {
                ["_type"] = "SAM.Analytical.FlowFractionControlCurve,SAM.Analytical",
                ["PerformanceTable"] = ventilationUnitPerformanceTable.ToJsonObject(),
            };

            return new FlowFractionControlCurve(jsonObject);
        }

        /// <summary>
        /// A three-condition performance table with the shape a domestic heat recovery unit with cooling
        /// publishes on - 3 external x 4 entering x 8 airflows, two outputs, 96 points each. The
        /// <i>values</i> are generated, not transcribed: this suite tests the machinery, and the shipped
        /// catalogue's own tests check real figures against the document they came from.
        /// </summary>
        private static VentilationUnitPerformanceTable FixtureTable()
        {
            List<double> values_Supply = [];
            List<double> values_Cooling = [];

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    for (int k = 0; k < 8; k++)
                    {
                        values_Supply.Add(Supply(i, j, k));
                        values_Cooling.Add(Cooling(i, j, k));
                    }
                }
            }

            return new VentilationUnitPerformanceTable(
                [
                    new VentilationUnitPerformanceAxis(name_External, "degC", [29.0, 32.0, 34.0]),
                    new VentilationUnitPerformanceAxis(name_Entering, "degC", [23.0, 24.0, 25.0, 26.0]),
                    new VentilationUnitPerformanceAxis(name_Flow, "l/s", [50.0, 60.0, 70.0, 80.0, 90.0, 100.0, 110.0, 120.0]),
                ],
                [
                    new VentilationUnitPerformanceOutput(name_Supply, "degC", values_Supply),
                    new VentilationUnitPerformanceOutput(name_Cooling, "kW", values_Cooling),
                ]);
        }

        private static double Supply(int i, int j, int k)
        {
            return 14 + i + (0.5 * j) + (0.25 * k);
        }

        private static double Cooling(int i, int j, int k)
        {
            return 0.8 + (0.1 * i) - (0.02 * j) + (0.05 * k);
        }

        /// <summary>
        /// A 2 x 2 x 2 table whose eight values are powers of two, so that every interpolation weight can
        /// be read off the answer: no other combination of corners produces the same sum.
        /// </summary>
        private static VentilationUnitPerformanceTable CornerTable()
        {
            return new VentilationUnitPerformanceTable(
                [
                    new VentilationUnitPerformanceAxis(name_External, "degC", [20.0, 30.0]),
                    new VentilationUnitPerformanceAxis(name_Entering, "degC", [20.0, 24.0]),
                    new VentilationUnitPerformanceAxis(name_Flow, "l/s", [40.0, 80.0]),
                ],
                [new VentilationUnitPerformanceOutput(name_Supply, "degC", [1.0, 2.0, 4.0, 8.0, 16.0, 32.0, 64.0, 128.0])]);
        }

        /// <summary>
        /// <see cref="CornerTable"/> written with its axes in a different order and its values reordered to
        /// match - the same table, transcribed differently.
        /// </summary>
        private static VentilationUnitPerformanceTable CornerTableTransposed()
        {
            //Flow, entering, external - so the value at (k, j, i) here is the value at (i, j, k) there.
            double[] values = new double[8];

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        values[((k * 2) + j) * 2 + i] = new double[] { 1.0, 2.0, 4.0, 8.0, 16.0, 32.0, 64.0, 128.0 }[((i * 2) + j) * 2 + k];
                    }
                }
            }

            return new VentilationUnitPerformanceTable(
                [
                    new VentilationUnitPerformanceAxis(name_Flow, "l/s", [40.0, 80.0]),
                    new VentilationUnitPerformanceAxis(name_Entering, "degC", [20.0, 24.0]),
                    new VentilationUnitPerformanceAxis(name_External, "degC", [20.0, 30.0]),
                ],
                [new VentilationUnitPerformanceOutput(name_Supply, "degC", values)]);
        }

        /// <summary>
        /// One dwelling's worth of model: a supply room and an extract room, both on one ventilation
        /// system, that system supplied by one air handling unit. Design duty 30 l/s each way.
        /// <para>
        /// Built by hand rather than through <c>PartFCalculator</c>, because what is under test here is the
        /// catalogue seam rather than the sizing - and a hand-built fixture keeps this suite off the
        /// process-wide default Part F rule set.
        /// </para>
        /// </summary>
        private static AdjacencyCluster Fixture(out AirHandlingUnit airHandlingUnit)
        {
            AdjacencyCluster adjacencyCluster = new();

            airHandlingUnit = Analytical.Create.AirHandlingUnit("AHU-01");
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

            Assert.True(adjacencyCluster.AirHandlingUnitDesignDuty(airHandlingUnit, out double supplyDuty_Lps, out double extractDuty_Lps));
            Assert.Equal(30, supplyDuty_Lps, 6);
            Assert.Equal(30, extractDuty_Lps, 6);

            return adjacencyCluster;
        }

        /// <summary>One room, carrying a stated Approved Document F requirement.</summary>
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

        /// <summary>One design terminal, related to its room and its system.</summary>
        private static void Terminal(AdjacencyCluster adjacencyCluster, VentilationSystem ventilationSystem, Space space, FlowClassification flowClassification, double designFlowRate_Lps)
        {
            PartFVentilationTerminalRequirement partFVentilationTerminalRequirement = space.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData).Terminals[0];

            VentilationTerminal ventilationTerminal = new(space.Name + " terminal", flowClassification, designFlowRate_Lps);
            ventilationTerminal.SetValue(VentilationTerminalParameter.PartFTerminalReference, new PartFTerminalReference(partFVentilationTerminalRequirement));

            adjacencyCluster.AddObject(ventilationTerminal);
            adjacencyCluster.AddRelation(ventilationTerminal, space);
            adjacencyCluster.AddRelation(ventilationTerminal, ventilationSystem);
        }

        /// <summary>Every room's design airflow on both sides, so a whole model can be compared before and after.</summary>
        private static Dictionary<string, double> Designs(AdjacencyCluster adjacencyCluster)
        {
            Dictionary<string, double> result = [];

            foreach (VentilationTerminal ventilationTerminal in adjacencyCluster.GetObjects<VentilationTerminal>() ?? [])
            {
                result[string.Format("{0} {1}", ventilationTerminal.Name, ventilationTerminal.FlowClassification)] = System.Math.Round(ventilationTerminal.DesignFlowRate_Lps ?? double.NaN, 6);
            }

            return result;
        }
    }
}
