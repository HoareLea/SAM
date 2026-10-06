// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Analytical.Enums;
using System;
using System.Collections.Generic;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// <b>Approved Document O Iteration 2B - the selected-equipment capacity envelope.</b>
    /// <para>
    /// The ordinary optimisation stops. It stopped because the selected ventilation unit cannot carry
    /// another whole +5 l/s round, or because the iteration guard ran out - and rooms are still failing
    /// TM59. The engineer's next question is:
    /// </para>
    /// <para>
    /// <i>"If the last valid design were increased coherently while preserving its terminal airflow
    /// proportions, what design could the already-selected unit support at its capacity ceiling?"</i>
    /// </para>
    /// <para>
    /// <c>Modify.EvaluateDesignAirFlowCapacityEnvelope</c> answers that one, and these tests pin what makes
    /// the answer trustworthy.
    /// </para>
    ///
    /// <para><b>What changed, and why these tests were rewritten</b></para>
    /// <para>
    /// The envelope used to scale the optimisation's <i>target vector</i> - the deliberate increments the
    /// next ordinary round would have asked for. That answers "how far can the current targeted direction
    /// continue?", which is coherent but is not the useful diagnostic: it spends the remaining headroom only
    /// on the rooms the optimiser happened to be pushing. The real Flat 1 case - 40 supply / 22 + 18 extract
    /// on a 150/150 unit - came out at <c>150 supply / 22 + 128 extract</c>, the bathroom carrying the entire
    /// increase and the studio's extract left exactly where it was. Nobody would build that.
    /// </para>
    /// <para>
    /// It now grows the <b>whole design vector</b> of every system the unit supplies by <b>one factor</b>,
    /// so the same flat comes out at <c>150 supply / 82.5 + 67.5 extract</c> - the same dwelling, larger.
    /// The target vector is still supplied, and is read for <b>scope only</b>: which equipment the diagnostic
    /// is about. <see cref="TheTargetVectorsFigures_HaveNoEffectOnTheEnvelope"/> is the test that pins that
    /// distinction, and <see cref="TheLastValidDesign_IsGrownProportionallyToTheSelectedUnitsCeiling"/> is
    /// the brief's own example.
    /// </para>
    ///
    /// <para><b>The ordinary round's all-or-nothing rule is not weakened, and this is not a way round it.</b></para>
    /// <para>
    /// An envelope is a separate operation producing a separate, clearly diagnostic model - so the test that
    /// matters most here is still the one asserting that the source design is untouched.
    /// </para>
    /// <para>
    /// <b>Not a second copy of the balancing or capacity arithmetic.</b> Every rule the envelope applies is
    /// applied by asking the ordinary round authority - the same allocator, the same Approved Document F
    /// floors, the same <c>Query.AirHandlingUnitDesignDuty</c>. What is tested here is what the envelope
    /// adds: one coherent factor per equipment group, solved against that group's whole design vector,
    /// bounded by the first limiting rating, and every "no" stated as its own outcome.
    /// </para>
    /// </summary>
    public class PartOCapacityEnvelopeTests
    {
        private const double tolerance_Lps = 0.001;

        private const string name_Bedroom = "Bedroom 1";

        private const string name_Kitchen = "Kitchen 1";

        private const string name_Bathroom = "Bathroom 1";

        private const string name_Studio = "Studio 1_0";

        private const string name_Bathroom_Studio = "Bathroom_2";

        private const string name_AirHandlingUnit_RealLadder = "MVHR-Flat1";

        /// <summary>The shipped Nuaire hybrid unit's model, 150/150 l/s.</summary>
        private const string model_MRXBOX = "MRXBOXAB-ECO5-AECV";

        /// <summary>The shipped Nuaire XBOXER commercial unit's model, 190/190 l/s.</summary>
        private const string model_XBC15 = "XBC15";

        // ---- 1 and 2. The brief's own example, and the proportions it turns on ---------------------------

        /// <summary>
        /// <b>1, 2.</b> The real Flat 1 case, which is the whole reason this diagnostic was re-specified.
        /// <code>
        /// last valid   Studio 1_0 supply  40      selected  MVHR-150, 150/150 l/s
        ///              Studio 1_0 extract 22      scale     min(150/40, 150/40) = 3.75
        ///              Bathroom_2 extract 18      system    40/40
        /// envelope     Studio 1_0 supply  150     system    150/150, exactly on the rating
        ///              Studio 1_0 extract 82.5
        ///              Bathroom_2 extract 67.5
        /// </code>
        /// <para>
        /// The old behaviour gave <c>150 / 22 + 128</c> - arithmetically consistent, and a design nobody
        /// would build. The assertion that separates the two is the studio's <b>extract</b>: 22 l/s under the
        /// old reading, 82.5 under this one.
        /// </para>
        /// </summary>
        [Fact]
        public void TheLastValidDesign_IsGrownProportionallyToTheSelectedUnitsCeiling()
        {
            AdjacencyCluster adjacencyCluster = FlatOneFixture(150, out List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors);

            DesignAirFlowCapacityEnvelope designAirFlowCapacityEnvelope = Envelope(adjacencyCluster, [
                Target(adjacencyCluster, name_Studio, FlowClassification.Supply, 45)], ventilationUnitCapacityDescriptors);

            Assert.True(designAirFlowCapacityEnvelope.IsScaled, designAirFlowCapacityEnvelope.Reason);
            Assert.Equal(DesignAirFlowCapacityEnvelopeOutcome.Scaled, designAirFlowCapacityEnvelope.Outcome);

            DesignAirFlowCapacityEnvelopeGroup designAirFlowCapacityEnvelopeGroup = Assert.Single(designAirFlowCapacityEnvelope.Groups);

            Assert.Equal(3.75, designAirFlowCapacityEnvelopeGroup.Scale, 9);

            Assert.Equal(150, Design(designAirFlowCapacityEnvelope.AdjacencyCluster, name_Studio, FlowClassification.Supply), 6);

            //THE assertion. 22 under the old target-vector reading; 82.5 under this one.
            Assert.Equal(82.5, Design(designAirFlowCapacityEnvelope.AdjacencyCluster, name_Studio, FlowClassification.Extract), 6);
            Assert.Equal(67.5, Design(designAirFlowCapacityEnvelope.AdjacencyCluster, name_Bathroom_Studio, FlowClassification.Extract), 6);

            //On the rating, and not a thousandth past it.
            Assert.Equal(150, designAirFlowCapacityEnvelopeGroup.SupplyDuty_After_Lps, 6);
            Assert.Equal(150, designAirFlowCapacityEnvelopeGroup.ExtractDuty_After_Lps, 6);

            Assert.Equal(0, designAirFlowCapacityEnvelopeGroup.SupplyHeadroom_Lps, 6);
            Assert.Equal(0, designAirFlowCapacityEnvelopeGroup.ExtractHeadroom_Lps, 6);
        }

        /// <summary>
        /// <b>2.</b> The proportion between the two extract rooms is preserved <i>exactly</i>:
        /// <c>22 / 18 == 82.5 / 67.5</c>. Asserted as a ratio rather than as two figures, because that is the
        /// property the diagnostic promises - the figures follow from it and the ceiling.
        /// </summary>
        [Fact]
        public void TheProportionBetweenTwoRooms_IsPreservedExactly()
        {
            AdjacencyCluster adjacencyCluster = FlatOneFixture(150, out List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors);

            DesignAirFlowCapacityEnvelope designAirFlowCapacityEnvelope = Envelope(adjacencyCluster, Step_FlatOne(adjacencyCluster), ventilationUnitCapacityDescriptors);

            Assert.True(designAirFlowCapacityEnvelope.IsScaled, designAirFlowCapacityEnvelope.Reason);

            AdjacencyCluster adjacencyCluster_Envelope = designAirFlowCapacityEnvelope.AdjacencyCluster;

            Assert.Equal(
                Design(adjacencyCluster, name_Studio, FlowClassification.Extract) / Design(adjacencyCluster, name_Bathroom_Studio, FlowClassification.Extract),
                Design(adjacencyCluster_Envelope, name_Studio, FlowClassification.Extract) / Design(adjacencyCluster_Envelope, name_Bathroom_Studio, FlowClassification.Extract),
                9);

            //And across the two SIDES as well, which is what makes the answer one coherent design rather
            //than two independently grown halves.
            Assert.Equal(
                Design(adjacencyCluster, name_Studio, FlowClassification.Supply) / Design(adjacencyCluster, name_Studio, FlowClassification.Extract),
                Design(adjacencyCluster_Envelope, name_Studio, FlowClassification.Supply) / Design(adjacencyCluster_Envelope, name_Studio, FlowClassification.Extract),
                9);
        }

        // ---- 3 and 4. Several rooms on each side, all keeping their share --------------------------------

        /// <summary>
        /// <b>3, 4.</b> Three supply rooms and three extract rooms, all at different figures, and every one
        /// of them keeps its share of the design vector. This is the assertion an "each terminal to its
        /// maximum" or "split the headroom equally" rule cannot pass.
        /// <code>
        /// last valid  supply  Bedroom A 12  Bedroom B 18  Living 20     50/50, unit rated 100/100
        ///             extract Kitchen   30  Bathroom  15  Ensuite  5    scale x2
        /// envelope    supply  Bedroom A 24  Bedroom B 36  Living 40    100/100
        ///             extract Kitchen   60  Bathroom  30  Ensuite 10
        /// </code>
        /// </summary>
        [Fact]
        public void EveryRoomOnBothSides_KeepsItsShareOfTheDesignVector()
        {
            AdjacencyCluster adjacencyCluster = MultiRoomFixture(100, out List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors);

            DesignAirFlowCapacityEnvelope designAirFlowCapacityEnvelope = Envelope(adjacencyCluster, [
                Target(adjacencyCluster, "Bathroom M", FlowClassification.Extract, 20)], ventilationUnitCapacityDescriptors);

            Assert.True(designAirFlowCapacityEnvelope.IsScaled, designAirFlowCapacityEnvelope.Reason);

            Assert.Equal(2, Assert.Single(designAirFlowCapacityEnvelope.Groups).Scale, 9);

            AdjacencyCluster adjacencyCluster_Envelope = designAirFlowCapacityEnvelope.AdjacencyCluster;

            Assert.Equal(24, Design(adjacencyCluster_Envelope, "Bedroom M A", FlowClassification.Supply), 6);
            Assert.Equal(36, Design(adjacencyCluster_Envelope, "Bedroom M B", FlowClassification.Supply), 6);
            Assert.Equal(40, Design(adjacencyCluster_Envelope, "Living M", FlowClassification.Supply), 6);

            Assert.Equal(60, Design(adjacencyCluster_Envelope, "Kitchen M", FlowClassification.Extract), 6);
            Assert.Equal(30, Design(adjacencyCluster_Envelope, "Bathroom M", FlowClassification.Extract), 6);
            Assert.Equal(10, Design(adjacencyCluster_Envelope, "Ensuite M", FlowClassification.Extract), 6);

            //Every ROOM in the model, in one comparison: exactly twice what it was, and nothing else moved.
            Dictionary<string, double> designs_Expected = [];

            foreach (KeyValuePair<string, double> keyValuePair in Designs(adjacencyCluster))
            {
                designs_Expected[keyValuePair.Key] = System.Math.Round(keyValuePair.Value * 2, 6);
            }

            Assert.Equal(designs_Expected, Designs(adjacencyCluster_Envelope));
        }

        // ---- 5. The first limiting ratio decides ONE factor for both sides -------------------------------

        /// <summary>
        /// <b>5.</b> A unit whose two sides are rated differently - 150 supply against 120 extract, serving a
        /// design balanced at 40/40 - reaches its <b>extract</b> rating first, at x3. That one factor binds
        /// <i>both</i> sides, so the supply side lands at 120 with 30 l/s of its rating deliberately unspent.
        /// <para>
        /// Spending it would need a second, larger multiplier on the supply side - which would change the
        /// relationship between supply and extract, and that relationship <i>is</i> the design vector this
        /// operation exists to preserve. So the diagnostic stops at the first limiting ratio and says which
        /// side that was.
        /// </para>
        /// </summary>
        [Fact]
        public void WhereTheTwoRatingsDiffer_TheFirstLimitingOneBindsBothSides()
        {
            AdjacencyCluster adjacencyCluster = FlatOneFixture(150, 120, out List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors);

            DesignAirFlowCapacityEnvelope designAirFlowCapacityEnvelope = Envelope(adjacencyCluster, Step_FlatOne(adjacencyCluster), ventilationUnitCapacityDescriptors);

            Assert.True(designAirFlowCapacityEnvelope.IsScaled, designAirFlowCapacityEnvelope.Reason);

            DesignAirFlowCapacityEnvelopeGroup designAirFlowCapacityEnvelopeGroup = Assert.Single(designAirFlowCapacityEnvelope.Groups);

            //ONE factor, from the tighter of the two RATIOS - and the tighter side is named.
            Assert.Equal(3, designAirFlowCapacityEnvelopeGroup.Scale, 9);
            Assert.Equal(FlowClassification.Extract, designAirFlowCapacityEnvelopeGroup.BindingFlowClassification);

            Assert.Equal(120, Design(designAirFlowCapacityEnvelope.AdjacencyCluster, name_Studio, FlowClassification.Supply), 6);
            Assert.Equal(66, Design(designAirFlowCapacityEnvelope.AdjacencyCluster, name_Studio, FlowClassification.Extract), 6);
            Assert.Equal(54, Design(designAirFlowCapacityEnvelope.AdjacencyCluster, name_Bathroom_Studio, FlowClassification.Extract), 6);

            //The extract side is on its rating; the supply side is deliberately 30 l/s short of its own.
            Assert.Equal(0, designAirFlowCapacityEnvelopeGroup.ExtractHeadroom_Lps, 6);
            Assert.Equal(30, designAirFlowCapacityEnvelopeGroup.SupplyHeadroom_Lps, 6);

            //And the unit is inside BOTH ratings, which is the point of taking the tighter ratio.
            Assert.True(designAirFlowCapacityEnvelopeGroup.SupplyDuty_After_Lps <= designAirFlowCapacityEnvelopeGroup.VentilationUnitCapacityDescriptor.MaximumSupplyFlowRate_Lps + tolerance_Lps);
            Assert.True(designAirFlowCapacityEnvelopeGroup.ExtractDuty_After_Lps <= designAirFlowCapacityEnvelopeGroup.VentilationUnitCapacityDescriptor.MaximumExtractFlowRate_Lps + tolerance_Lps);
        }

        /// <summary>
        /// <b>5, the other way round.</b> A tighter <i>supply</i> rating binds the extract side just the
        /// same, and the binding side reported is supply - so the choice is made by the arithmetic and not
        /// by a preference for one direction.
        /// </summary>
        [Fact]
        public void ATighterSupplyRating_BindsTheExtractSideAndIsNamedAsTheLimit()
        {
            AdjacencyCluster adjacencyCluster = FlatOneFixture(80, 150, out List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors);

            DesignAirFlowCapacityEnvelope designAirFlowCapacityEnvelope = Envelope(adjacencyCluster, Step_FlatOne(adjacencyCluster), ventilationUnitCapacityDescriptors);

            Assert.True(designAirFlowCapacityEnvelope.IsScaled, designAirFlowCapacityEnvelope.Reason);

            DesignAirFlowCapacityEnvelopeGroup designAirFlowCapacityEnvelopeGroup = Assert.Single(designAirFlowCapacityEnvelope.Groups);

            Assert.Equal(2, designAirFlowCapacityEnvelopeGroup.Scale, 9);
            Assert.Equal(FlowClassification.Supply, designAirFlowCapacityEnvelopeGroup.BindingFlowClassification);

            Assert.Equal(80, designAirFlowCapacityEnvelopeGroup.SupplyDuty_After_Lps, 6);
            Assert.Equal(80, designAirFlowCapacityEnvelopeGroup.ExtractDuty_After_Lps, 6);

            Assert.Equal(0, designAirFlowCapacityEnvelopeGroup.SupplyHeadroom_Lps, 6);
            Assert.Equal(70, designAirFlowCapacityEnvelopeGroup.ExtractHeadroom_Lps, 6);
        }

        // ---- The target vector is SCOPE, not a figure ----------------------------------------------------

        /// <summary>
        /// <b>The test that separates the new diagnostic from the old one.</b> The deliberate target vector
        /// no longer supplies a single number: whatever figures it carries, and whichever rooms of the
        /// dwelling it names, the envelope is the same design.
        /// <para>
        /// Under the previous behaviour every one of these four vectors produced a <i>different</i> envelope,
        /// because each one spent the headroom on the rooms it happened to name. Under this one they agree to
        /// the last decimal, because the vector only says which unit the diagnostic is about.
        /// </para>
        /// </summary>
        [Fact]
        public void TheTargetVectorsFigures_HaveNoEffectOnTheEnvelope()
        {
            AdjacencyCluster adjacencyCluster = FlatOneFixture(150, out List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors);

            //One whole ordinary step; a far larger request; a request that moves nothing at all; and the
            //other side of a different room.
            List<List<DesignAirFlowTarget>> designAirFlowTargets =
            [
                [Target(adjacencyCluster, name_Studio, FlowClassification.Supply, 45)],
                [Target(adjacencyCluster, name_Studio, FlowClassification.Supply, 900)],
                [Target(adjacencyCluster, name_Studio, FlowClassification.Supply, 40)],
                [Target(adjacencyCluster, name_Bathroom_Studio, FlowClassification.Extract, 23)],
            ];

            Dictionary<string, double> designs = null;

            foreach (List<DesignAirFlowTarget> designAirFlowTargets_Temp in designAirFlowTargets)
            {
                DesignAirFlowCapacityEnvelope designAirFlowCapacityEnvelope = Envelope(adjacencyCluster, designAirFlowTargets_Temp, ventilationUnitCapacityDescriptors);

                Assert.True(designAirFlowCapacityEnvelope.IsScaled, designAirFlowCapacityEnvelope.Reason);
                Assert.Equal(3.75, Assert.Single(designAirFlowCapacityEnvelope.Groups).Scale, 9);

                Dictionary<string, double> designs_Temp = Designs(designAirFlowCapacityEnvelope.AdjacencyCluster);

                if (designs is null)
                {
                    designs = designs_Temp;

                    continue;
                }

                Assert.Equal(designs, designs_Temp);
            }
        }

        /// <summary>
        /// A room the target vector never named is grown with the rest of its unit - including a room on the
        /// side nobody was pushing. This is what makes the answer a design rather than an allocation: the
        /// alternative leaves that room at its old figure while everything around it grows.
        /// </summary>
        [Fact]
        public void ARoomNoTargetNamed_IsGrownWithTheRestOfItsUnit()
        {
            AdjacencyCluster adjacencyCluster = Fixture(60, out List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors);

            //Only the bathroom is named, and it is one of three rooms on the unit.
            DesignAirFlowCapacityEnvelope designAirFlowCapacityEnvelope = Envelope(adjacencyCluster, [
                Target(adjacencyCluster, name_Bathroom, FlowClassification.Extract, 13)], ventilationUnitCapacityDescriptors);

            Assert.True(designAirFlowCapacityEnvelope.IsScaled, designAirFlowCapacityEnvelope.Reason);

            Assert.Equal(2, Assert.Single(designAirFlowCapacityEnvelope.Groups).Scale, 9);

            Assert.Equal(60, Design(designAirFlowCapacityEnvelope.AdjacencyCluster, name_Bedroom, FlowClassification.Supply), 6);
            Assert.Equal(44, Design(designAirFlowCapacityEnvelope.AdjacencyCluster, name_Kitchen, FlowClassification.Extract), 6);
            Assert.Equal(16, Design(designAirFlowCapacityEnvelope.AdjacencyCluster, name_Bathroom, FlowClassification.Extract), 6);
        }

        /// <summary>
        /// A whole ventilation SYSTEM no target named is grown too, where its unit is the one being
        /// enveloped. The rating is compared against the unit's whole duty, so a system left behind would
        /// keep its old figure while the rest grew - and the answer would sit short of the ceiling while
        /// claiming to be on it.
        /// </summary>
        [Fact]
        public void ASystemNoTargetNamed_IsGrownWithTheRestOfItsUnit()
        {
            AdjacencyCluster adjacencyCluster = SharedUnitFixture(70, out List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors);

            //Only Flat A is named. Flat B shares the unit, and shares the factor.
            DesignAirFlowCapacityEnvelope designAirFlowCapacityEnvelope = Envelope(adjacencyCluster, [
                Target(adjacencyCluster, "Bedroom A", FlowClassification.Supply, 15)], ventilationUnitCapacityDescriptors);

            Assert.True(designAirFlowCapacityEnvelope.IsScaled, designAirFlowCapacityEnvelope.Reason);

            DesignAirFlowCapacityEnvelopeGroup designAirFlowCapacityEnvelopeGroup = Assert.Single(designAirFlowCapacityEnvelope.Groups);

            //ONE group for the one unit, holding BOTH systems - not only the named one.
            Assert.Equal(2, designAirFlowCapacityEnvelopeGroup.VentilationSystems.Count);
            Assert.Equal(2, designAirFlowCapacityEnvelopeGroup.Scale, 9);

            Assert.Equal(20, Design(designAirFlowCapacityEnvelope.AdjacencyCluster, "Bedroom A", FlowClassification.Supply), 6);
            Assert.Equal(20, Design(designAirFlowCapacityEnvelope.AdjacencyCluster, "Kitchen A", FlowClassification.Extract), 6);
            Assert.Equal(50, Design(designAirFlowCapacityEnvelope.AdjacencyCluster, "Bedroom B", FlowClassification.Supply), 6);
            Assert.Equal(50, Design(designAirFlowCapacityEnvelope.AdjacencyCluster, "Kitchen B", FlowClassification.Extract), 6);

            //The UNIT's whole duty is what landed on the rating - which is the assertion a per-dwelling or
            //per-target growth could not pass.
            Assert.Equal(70, designAirFlowCapacityEnvelopeGroup.SupplyDuty_After_Lps, 6);
            Assert.Equal(70, designAirFlowCapacityEnvelopeGroup.ExtractDuty_After_Lps, 6);
        }

        /// <summary>
        /// Two dwellings on two separate units each reach their own ceiling, at their own factor - so the
        /// per-unit solve is not accidentally a global one either.
        /// </summary>
        [Fact]
        public void SeparateVentilationUnits_EachReachTheirOwnCeilingAtTheirOwnScale()
        {
            AdjacencyCluster adjacencyCluster = TwoDwellingFixture(out List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors);

            DesignAirFlowCapacityEnvelope designAirFlowCapacityEnvelope = Envelope(adjacencyCluster, [
                Target(adjacencyCluster, "Kitchen 1", FlowClassification.Extract, 27),
                Target(adjacencyCluster, "Kitchen 2", FlowClassification.Extract, 27)], ventilationUnitCapacityDescriptors);

            Assert.True(designAirFlowCapacityEnvelope.IsScaled, designAirFlowCapacityEnvelope.Reason);

            Assert.Equal(2, designAirFlowCapacityEnvelope.Groups.Count);

            //AHU 1 is rated 45 and AHU 2 is rated 90, both dwellings designed at 30/30 - so x1.5 and x3.
            Assert.Equal("AHU 1", designAirFlowCapacityEnvelope.Groups[0].Name);
            Assert.Equal(1.5, designAirFlowCapacityEnvelope.Groups[0].Scale, 9);

            Assert.Equal("AHU 2", designAirFlowCapacityEnvelope.Groups[1].Name);
            Assert.Equal(3, designAirFlowCapacityEnvelope.Groups[1].Scale, 9);

            Assert.Equal(33, Design(designAirFlowCapacityEnvelope.AdjacencyCluster, "Kitchen 1", FlowClassification.Extract), 6);
            Assert.Equal(66, Design(designAirFlowCapacityEnvelope.AdjacencyCluster, "Kitchen 2", FlowClassification.Extract), 6);

            Assert.Equal(45, designAirFlowCapacityEnvelope.Groups[0].SupplyDuty_After_Lps, 6);
            Assert.Equal(90, designAirFlowCapacityEnvelope.Groups[1].SupplyDuty_After_Lps, 6);
        }

        /// <summary>
        /// A unit only reachable through a dwelling nobody targeted is <b>not</b> enveloped. The target
        /// vector is the scope, and growing the design of a flat whose rooms all pass would be diagnosing a
        /// question nobody asked.
        /// </summary>
        [Fact]
        public void AUnitNoTargetReaches_IsNotEnvelopedAtAll()
        {
            AdjacencyCluster adjacencyCluster = TwoDwellingFixture(out List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors);

            DesignAirFlowCapacityEnvelope designAirFlowCapacityEnvelope = Envelope(adjacencyCluster, [
                Target(adjacencyCluster, "Kitchen 2", FlowClassification.Extract, 27)], ventilationUnitCapacityDescriptors);

            Assert.True(designAirFlowCapacityEnvelope.IsScaled, designAirFlowCapacityEnvelope.Reason);

            Assert.Equal("AHU 2", Assert.Single(designAirFlowCapacityEnvelope.Groups).Name);

            //Flat 2 grew; flat 1 is exactly where it was.
            Assert.Equal(66, Design(designAirFlowCapacityEnvelope.AdjacencyCluster, "Kitchen 2", FlowClassification.Extract), 6);
            Assert.Equal(22, Design(designAirFlowCapacityEnvelope.AdjacencyCluster, "Kitchen 1", FlowClassification.Extract), 6);
            Assert.Equal(30, Design(designAirFlowCapacityEnvelope.AdjacencyCluster, "Bedroom 1", FlowClassification.Supply), 6);
        }

        // ---- Determinism -------------------------------------------------------------------------------

        /// <summary>
        /// The same targets in opposite orders produce the same envelope - the same factor, the same design
        /// to the last decimal, and the same report. An envelope that allocated remaining capacity to
        /// whichever room came out of the assessment first would fail this, and would be unusable as
        /// evidence.
        /// </summary>
        [Fact]
        public void TheEnvelope_IsIndependentOfTheOrderItsTargetsArrivedIn()
        {
            AdjacencyCluster adjacencyCluster = TwoDwellingFixture(out List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors);

            DesignAirFlowCapacityEnvelope designAirFlowCapacityEnvelope_Forward = Envelope(adjacencyCluster, [
                Target(adjacencyCluster, "Kitchen 1", FlowClassification.Extract, 27),
                Target(adjacencyCluster, "Bathroom 1", FlowClassification.Extract, 13),
                Target(adjacencyCluster, "Kitchen 2", FlowClassification.Extract, 27)], ventilationUnitCapacityDescriptors);

            DesignAirFlowCapacityEnvelope designAirFlowCapacityEnvelope_Reversed = Envelope(adjacencyCluster, [
                Target(adjacencyCluster, "Kitchen 2", FlowClassification.Extract, 27),
                Target(adjacencyCluster, "Bathroom 1", FlowClassification.Extract, 13),
                Target(adjacencyCluster, "Kitchen 1", FlowClassification.Extract, 27)], ventilationUnitCapacityDescriptors);

            Assert.True(designAirFlowCapacityEnvelope_Forward.IsScaled);
            Assert.True(designAirFlowCapacityEnvelope_Reversed.IsScaled);

            Assert.Equal(Designs(designAirFlowCapacityEnvelope_Forward.AdjacencyCluster), Designs(designAirFlowCapacityEnvelope_Reversed.AdjacencyCluster));

            //And the reports agree too, not just the models.
            Assert.Equal(
                designAirFlowCapacityEnvelope_Forward.Groups.ConvertAll(x => x.ToString()),
                designAirFlowCapacityEnvelope_Reversed.Groups.ConvertAll(x => x.ToString()));

            Assert.Equal(
                designAirFlowCapacityEnvelope_Forward.TargetedAdjustments.ConvertAll(x => x.ToString()),
                designAirFlowCapacityEnvelope_Reversed.TargetedAdjustments.ConvertAll(x => x.ToString()));

            Assert.Equal(
                designAirFlowCapacityEnvelope_Forward.DerivedAdjustments.ConvertAll(x => x.ToString()),
                designAirFlowCapacityEnvelope_Reversed.DerivedAdjustments.ConvertAll(x => x.ToString()));
        }

        /// <summary>
        /// Running the same envelope twice gives the same answer and leaves the source design untouched -
        /// every evaluation reads the caller's model and none of them writes to it.
        /// </summary>
        [Fact]
        public void RepeatedEnvelopes_OverTheSameDesignAgreeAndLeaveItUnchanged()
        {
            AdjacencyCluster adjacencyCluster = Fixture(45, out List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors);

            Dictionary<string, double> designs_Before = Designs(adjacencyCluster);

            DesignAirFlowCapacityEnvelope designAirFlowCapacityEnvelope_First = Envelope(adjacencyCluster, Step(adjacencyCluster), ventilationUnitCapacityDescriptors);
            DesignAirFlowCapacityEnvelope designAirFlowCapacityEnvelope_Second = Envelope(adjacencyCluster, Step(adjacencyCluster), ventilationUnitCapacityDescriptors);

            Assert.Equal(designs_Before, Designs(adjacencyCluster));

            Assert.Equal(Designs(designAirFlowCapacityEnvelope_First.AdjacencyCluster), Designs(designAirFlowCapacityEnvelope_Second.AdjacencyCluster));
            Assert.Equal(designAirFlowCapacityEnvelope_First.Groups[0].Scale, designAirFlowCapacityEnvelope_Second.Groups[0].Scale, 12);
        }

        // ---- The evidence a report reads ----------------------------------------------------------------

        /// <summary>
        /// <b>Every room the unit serves is a deliberate target of the diagnostic, and nothing is derived.</b>
        /// <para>
        /// Two facts in one, and both matter to a report. Every room is a target because every room was
        /// <i>chosen</i> - by this operation - to keep its share of the design vector; a report has to be able
        /// to print all of them, which is what makes its visible rows reconcile to the unit's duty. And
        /// nothing is derived because the design being grown is balanced, so multiplying both sides by one
        /// factor moves them by the same amount and the round's balancing rule has nothing left to do.
        /// </para>
        /// </summary>
        [Fact]
        public void EveryRoomTheUnitServes_IsAScaledTargetAndNothingIsDerived()
        {
            AdjacencyCluster adjacencyCluster = FlatOneFixture(150, out List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors);

            DesignAirFlowCapacityEnvelope designAirFlowCapacityEnvelope = Envelope(adjacencyCluster, Step_FlatOne(adjacencyCluster), ventilationUnitCapacityDescriptors);

            Assert.True(designAirFlowCapacityEnvelope.IsScaled, designAirFlowCapacityEnvelope.Reason);

            //Nobody balanced anything: the growth is balanced by construction.
            Assert.Empty(designAirFlowCapacityEnvelope.DerivedAdjustments);

            List<string> keys = designAirFlowCapacityEnvelope.TargetedAdjustments.ConvertAll(x => string.Format("{0} {1}", x.SpaceName, x.FlowClassification));

            keys.Sort(StringComparer.Ordinal);

            Assert.Equal(
            [
                string.Format("{0} Extract", name_Bathroom_Studio),
                string.Format("{0} Extract", name_Studio),
                string.Format("{0} Supply", name_Studio),
            ], keys);

            Assert.All(designAirFlowCapacityEnvelope.TargetedAdjustments, x => Assert.False(x.IsDerived));

            //The studio's extract - the contribution the old report could not account for - is there with
            //its own before and after.
            DesignAirFlowAdjustment designAirFlowAdjustment = designAirFlowCapacityEnvelope.TargetedAdjustments.Find(x => x.SpaceName == name_Studio && x.FlowClassification == FlowClassification.Extract);

            Assert.NotNull(designAirFlowAdjustment);
            Assert.Equal(22, designAirFlowAdjustment.Before_Lps, 6);
            Assert.Equal(82.5, designAirFlowAdjustment.After_Lps, 6);
        }

        /// <summary>
        /// <b>11.</b> The adjustments a report shows <b>reconcile to the unit's duty</b> on both sides, with
        /// no hidden terminal contribution. A reader summing the visible rows has to be able to reproduce the
        /// air handling unit's supply and extract duty exactly - which was the second reporting defect this
        /// change fixes.
        /// </summary>
        [Fact]
        public void TheVisibleAdjustments_ReconcileToTheUnitsSupplyAndExtractDuty()
        {
            AdjacencyCluster adjacencyCluster = MultiRoomFixture(100, out List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors);

            DesignAirFlowCapacityEnvelope designAirFlowCapacityEnvelope = Envelope(adjacencyCluster, [
                Target(adjacencyCluster, "Bathroom M", FlowClassification.Extract, 20)], ventilationUnitCapacityDescriptors);

            Assert.True(designAirFlowCapacityEnvelope.IsScaled, designAirFlowCapacityEnvelope.Reason);

            DesignAirFlowCapacityEnvelopeGroup designAirFlowCapacityEnvelopeGroup = Assert.Single(designAirFlowCapacityEnvelope.Groups);

            double supply_Lps = 0;
            double extract_Lps = 0;

            HashSet<string> keys = [];

            foreach (DesignAirFlowAdjustment designAirFlowAdjustment in designAirFlowCapacityEnvelope.TargetedAdjustments)
            {
                //One row per space and direction, so a reader can sum them without double counting.
                Assert.True(keys.Add(string.Format("{0}|{1}", designAirFlowAdjustment.SpaceGuid, designAirFlowAdjustment.FlowClassification)));

                if (designAirFlowAdjustment.FlowClassification == FlowClassification.Supply)
                {
                    supply_Lps += designAirFlowAdjustment.After_Lps;
                }
                else
                {
                    extract_Lps += designAirFlowAdjustment.After_Lps;
                }
            }

            Assert.Equal(designAirFlowCapacityEnvelopeGroup.SupplyDuty_After_Lps, supply_Lps, 6);
            Assert.Equal(designAirFlowCapacityEnvelopeGroup.ExtractDuty_After_Lps, extract_Lps, 6);

            //And the BEFORE column reconciles to the duty the design started at, so the two columns of the
            //report are the same design read twice rather than two different measurements.
            Assert.Equal(designAirFlowCapacityEnvelopeGroup.SupplyDuty_Before_Lps, Sum(designAirFlowCapacityEnvelope, FlowClassification.Supply), 6);
            Assert.Equal(designAirFlowCapacityEnvelopeGroup.ExtractDuty_Before_Lps, Sum(designAirFlowCapacityEnvelope, FlowClassification.Extract), 6);
        }

        // ---- 6, 7, 9. Requirement, product and runtime airflow all untouched ----------------------------

        /// <summary>
        /// <b>6.</b> Not one Approved Document F requirement moves. The envelope raises design airflow, and
        /// the requirement is the floor it stays above - read, never written. A proportional growth can only
        /// raise a room, so no floor is even approached.
        /// </summary>
        [Fact]
        public void TheEnvelope_LeavesEveryApprovedDocumentFRequirementUnchanged()
        {
            AdjacencyCluster adjacencyCluster = FlatOneFixture(150, out List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors);

            Dictionary<string, double> requirements_Before = Requirements(adjacencyCluster);

            DesignAirFlowCapacityEnvelope designAirFlowCapacityEnvelope = Envelope(adjacencyCluster, Step_FlatOne(adjacencyCluster), ventilationUnitCapacityDescriptors);

            Assert.True(designAirFlowCapacityEnvelope.IsScaled, designAirFlowCapacityEnvelope.Reason);

            Assert.Equal(requirements_Before, Requirements(designAirFlowCapacityEnvelope.AdjacencyCluster));
            Assert.Equal(requirements_Before, Requirements(adjacencyCluster));

            //The studio still requires 30 l/s of supply and the bathroom 8 l/s of extract, exactly as the
            //brief states - beside a design of 150 and 67.5.
            Assert.Equal(30, Requirement(designAirFlowCapacityEnvelope.AdjacencyCluster, name_Studio, FlowClassification.Supply), 6);
            Assert.Equal(8, Requirement(designAirFlowCapacityEnvelope.AdjacencyCluster, name_Bathroom_Studio, FlowClassification.Extract), 6);

            //And every design the envelope wrote is still above its own floor.
            Assert.All(designAirFlowCapacityEnvelope.TargetedAdjustments, x => Assert.True(double.IsNaN(x.Requirement_Lps) || x.After_Lps + tolerance_Lps >= x.Requirement_Lps));
            Assert.All(designAirFlowCapacityEnvelope.DerivedAdjustments, x => Assert.True(double.IsNaN(x.Requirement_Lps) || x.After_Lps + tolerance_Lps >= x.Requirement_Lps));
        }

        /// <summary>
        /// <b>9.</b> No operating, profile or runtime airflow is written - which is the Iteration 3 boundary.
        /// The envelope changes design airflow; rebuilding the transfer and mechanical network around it is
        /// re-preparing the iteration, and is somebody else's job.
        /// </summary>
        [Fact]
        public void TheEnvelope_WritesNoRuntimeAirflow()
        {
            AdjacencyCluster adjacencyCluster = Fixture(90, out List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors);

            List<string> airMovements_Before = AirMovements(adjacencyCluster);

            DesignAirFlowCapacityEnvelope designAirFlowCapacityEnvelope = Envelope(adjacencyCluster, Step(adjacencyCluster), ventilationUnitCapacityDescriptors);

            Assert.True(designAirFlowCapacityEnvelope.IsScaled, designAirFlowCapacityEnvelope.Reason);

            Assert.Equal(airMovements_Before, AirMovements(designAirFlowCapacityEnvelope.AdjacencyCluster));
        }

        /// <summary>
        /// <b>7.</b> The selected product is the ceiling the envelope grows <i>within</i>, and is never
        /// reselected to make the answer bigger - not even when the catalogue offered contains a unit that
        /// would carry far more. Buying equipment is a deliberate decision, and a diagnostic is not one.
        /// </summary>
        [Fact]
        public void TheEnvelope_NeverReselectsTheVentilationUnit()
        {
            AdjacencyCluster adjacencyCluster = Fixture(45, out List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors);

            //A far bigger product is on offer, and is not taken.
            Assert.Contains(ventilationUnitCapacityDescriptors, x => x.MaximumSupplyFlowRate_Lps > 100);

            DesignAirFlowCapacityEnvelope designAirFlowCapacityEnvelope = Envelope(adjacencyCluster, Step(adjacencyCluster), ventilationUnitCapacityDescriptors);

            Assert.True(designAirFlowCapacityEnvelope.IsScaled, designAirFlowCapacityEnvelope.Reason);

            Assert.Equal("Selected", SelectedModel(adjacencyCluster, "AHU 1"));
            Assert.Equal("Selected", SelectedModel(designAirFlowCapacityEnvelope.AdjacencyCluster, "AHU 1"));

            Assert.Equal("Selected", Assert.Single(designAirFlowCapacityEnvelope.Groups).VentilationUnitReference.Model);

            //And the design stopped at the SELECTED product's rating rather than the larger one's.
            Assert.Equal(45, Assert.Single(designAirFlowCapacityEnvelope.Groups).SupplyDuty_After_Lps, 6);

            //No dwelling of the grown round reports a reselection.
            Assert.All(designAirFlowCapacityEnvelope.RoundCandidate.DwellingRounds, x => Assert.NotEqual(VentilationUnitSelectionOutcome.Reselected, x.VentilationUnitSelectionOutcome));
            Assert.All(designAirFlowCapacityEnvelope.RoundCandidate.DwellingRounds, x => Assert.Equal(VentilationUnitSelectionOutcome.Kept, x.VentilationUnitSelectionOutcome));
        }

        /// <summary>
        /// <b>7b.</b> And the same is true of a dwelling assigned the project's own <b>test</b> ventilation
        /// unit: its stated capacity is the ceiling Iteration 2B grows within, and no manufacturer product
        /// is reselected to make the answer bigger.
        /// <para>
        /// This is the whole reason a project test product has to persist on the project. The unit stores an
        /// identity, exactly as it would for a Nuaire box; the ceiling is looked up from the capability list
        /// the preparation recorded. If that list did not carry the what-if, this dwelling's ceiling would
        /// be unknown and the envelope would refuse - and the temptation would then be to reach for a real
        /// product instead, which is exactly what must never happen.
        /// </para>
        /// </summary>
        [Fact]
        public void TheEnvelope_GrowsWithinAnAssignedProjectTestUnit_AndReselectsNoManufacturerProduct()
        {
            PartOProjectTestVentilationUnit partOProjectTestVentilationUnit = new("Test unit", 45, 50);

            //The dwelling is fitted with the what-if, and nothing else.
            AdjacencyCluster adjacencyCluster = new();

            Dwelling(adjacencyCluster, Assert.Single(partOProjectTestVentilationUnit.CapacityDescriptors()), "AHU 1", 1, true);

            //The capability lookup a preparation would have recorded: the shipped ladder PLUS the project's
            //own test product. The shipped ladder holds units far larger, and none of them is taken.
            List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors = [.. RealLadder(), .. partOProjectTestVentilationUnit.CapacityDescriptors()];

            DesignAirFlowCapacityEnvelope designAirFlowCapacityEnvelope = Envelope(adjacencyCluster, Step(adjacencyCluster), ventilationUnitCapacityDescriptors);

            Assert.True(designAirFlowCapacityEnvelope.IsScaled, designAirFlowCapacityEnvelope.Reason);

            //The identity is locked, before and after.
            Assert.Equal("Test unit", SelectedModel(adjacencyCluster, "AHU 1"));
            Assert.Equal("Test unit", SelectedModel(designAirFlowCapacityEnvelope.AdjacencyCluster, "AHU 1"));

            DesignAirFlowCapacityEnvelopeGroup designAirFlowCapacityEnvelopeGroup = Assert.Single(designAirFlowCapacityEnvelope.Groups);

            Assert.Equal("Test unit", designAirFlowCapacityEnvelopeGroup.VentilationUnitReference.Model);
            Assert.Equal(PartOProjectTestVentilationUnit.Manufacturer, designAirFlowCapacityEnvelopeGroup.VentilationUnitReference.Manufacturer);

            //The what-if's OWN two ratings are the ceiling, each side its own.
            Assert.Equal(45, designAirFlowCapacityEnvelopeGroup.VentilationUnitCapacityDescriptor.MaximumSupplyFlowRate_Lps);
            Assert.Equal(50, designAirFlowCapacityEnvelopeGroup.VentilationUnitCapacityDescriptor.MaximumExtractFlowRate_Lps);

            //And the design stopped at 45 supply - the what-if's rating - not at 150, 190 or 500.
            Assert.Equal(45, designAirFlowCapacityEnvelopeGroup.SupplyDuty_After_Lps, 6);

            Assert.All(designAirFlowCapacityEnvelope.RoundCandidate.DwellingRounds, x => Assert.Equal(VentilationUnitSelectionOutcome.Kept, x.VentilationUnitSelectionOutcome));
        }

        // ---- 9. The design the envelope was calculated from is untouched ---------------------------------

        /// <summary>
        /// <b>9.</b> The last accepted ordinary design is not replaced, altered or in any way reachable from
        /// the envelope. This is the whole safety of the operation: an envelope is a design the ordinary
        /// policy refuses, and if the source model could be reached by it, a later round would be computed
        /// from a design nobody accepted.
        /// </summary>
        [Fact]
        public void TheSourceDesign_IsNeverTouchedByTheEnvelope()
        {
            AdjacencyCluster adjacencyCluster = FlatOneFixture(150, out List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors);

            Dictionary<string, double> designs_Before = Designs(adjacencyCluster);

            DesignAirFlowCapacityEnvelope designAirFlowCapacityEnvelope = Envelope(adjacencyCluster, Step_FlatOne(adjacencyCluster), ventilationUnitCapacityDescriptors);

            Assert.True(designAirFlowCapacityEnvelope.IsScaled, designAirFlowCapacityEnvelope.Reason);

            Assert.Equal(designs_Before, Designs(adjacencyCluster));

            //A different object, and one whose designs really are different - so the comparison above is not
            //passing because nothing happened at all.
            Assert.NotSame(adjacencyCluster, designAirFlowCapacityEnvelope.AdjacencyCluster);
            Assert.NotEqual(designs_Before, Designs(designAirFlowCapacityEnvelope.AdjacencyCluster));

            //And the envelope says of itself, in its own words, that it is not the answer.
            Assert.Contains("DIAGNOSTIC", designAirFlowCapacityEnvelope.Reason);
            Assert.Contains("not an accepted optimisation round", Assert.Single(designAirFlowCapacityEnvelope.Groups).Reason);
        }

        // ---- Nothing to target --------------------------------------------------------------------------

        /// <summary>
        /// No eligible target means no envelope and no model to simulate - stated as
        /// <see cref="DesignAirFlowCapacityEnvelopeOutcome.NoTargets"/> rather than as a refusal, because a
        /// design with nothing left to target has nothing to diagnose.
        /// </summary>
        [Fact]
        public void NoTargetAtAll_ProducesNoEnvelopeAndSaysSo()
        {
            AdjacencyCluster adjacencyCluster = Fixture(90, out List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors);

            DesignAirFlowCapacityEnvelope designAirFlowCapacityEnvelope = Envelope(adjacencyCluster, [], ventilationUnitCapacityDescriptors);

            Assert.False(designAirFlowCapacityEnvelope.IsScaled);
            Assert.Null(designAirFlowCapacityEnvelope.AdjacencyCluster);
            Assert.Null(designAirFlowCapacityEnvelope.RoundCandidate);

            Assert.Equal(DesignAirFlowCapacityEnvelopeOutcome.NoTargets, designAirFlowCapacityEnvelope.Outcome);
            Assert.NotNull(designAirFlowCapacityEnvelope.Reason);

            //Not a refusal: nothing failed.
            Assert.Empty(designAirFlowCapacityEnvelope.Refusals);
        }

        /// <summary>
        /// A failing room with no design terminal on the side asked for is dropped with its reason exactly as
        /// an ordinary round drops it - and where it is the only target, no unit is brought into scope and
        /// the envelope says which room it could not use.
        /// </summary>
        [Fact]
        public void ATargetWithNoDesignTerminal_IsDroppedWithItsReasonAndBringsNoUnitIntoScope()
        {
            AdjacencyCluster adjacencyCluster = Fixture(90, out List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors);

            //The bathroom has an extract terminal and no supply terminal at all.
            DesignAirFlowCapacityEnvelope designAirFlowCapacityEnvelope = Envelope(adjacencyCluster, [
                Target(adjacencyCluster, name_Bathroom, FlowClassification.Supply, 20)], ventilationUnitCapacityDescriptors);

            Assert.False(designAirFlowCapacityEnvelope.IsScaled);
            Assert.Null(designAirFlowCapacityEnvelope.AdjacencyCluster);

            //Dropped, not refused: the building has no lever for this request, which is a design finding
            //rather than a failure - and an engineer does something different about it than about an
            //exhausted unit.
            Assert.Equal(DesignAirFlowCapacityEnvelopeOutcome.NoTargets, designAirFlowCapacityEnvelope.Outcome);
            Assert.Empty(designAirFlowCapacityEnvelope.Refusals);

            //On the envelope's OWN dropped-target list, not buried in a round's - the envelope's round is
            //given the design that exists and drops nothing.
            DesignAirFlowTargetRefusal designAirFlowTargetRefusal = Assert.Single(designAirFlowCapacityEnvelope.TargetRefusals);

            Assert.Equal(name_Bathroom, designAirFlowTargetRefusal.DesignAirFlowTarget.SpaceName);
            Assert.NotNull(designAirFlowTargetRefusal.Reason);

            Assert.Contains(designAirFlowCapacityEnvelope.Notes, x => x.Contains(name_Bathroom));
        }

        /// <summary>
        /// One dropped target does not stop the rest. The room with no lever is reported, and the unit the
        /// other target reaches is still enveloped - refusing everything because of one room would leave an
        /// engineer with no diagnostic at all.
        /// </summary>
        [Fact]
        public void ADroppedTarget_DoesNotStopTheUnitsTheOthersReach()
        {
            AdjacencyCluster adjacencyCluster = Fixture(90, out List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors);

            DesignAirFlowCapacityEnvelope designAirFlowCapacityEnvelope = Envelope(adjacencyCluster, [
                Target(adjacencyCluster, name_Bathroom, FlowClassification.Supply, 20),
                Target(adjacencyCluster, name_Kitchen, FlowClassification.Extract, 27)], ventilationUnitCapacityDescriptors);

            Assert.True(designAirFlowCapacityEnvelope.IsScaled, designAirFlowCapacityEnvelope.Reason);

            Assert.Single(designAirFlowCapacityEnvelope.TargetRefusals);
            Assert.Equal(3, Assert.Single(designAirFlowCapacityEnvelope.Groups).Scale, 9);
        }

        // ---- No headroom --------------------------------------------------------------------------------

        /// <summary>
        /// A design already sitting on its selected unit's rating has no envelope: the answer is
        /// <see cref="DesignAirFlowCapacityEnvelopeOutcome.NoHeadroom"/>, with the rating, the duty and the
        /// remaining headroom on the record - which is itself the diagnostic. "This design IS what that
        /// product can support" is a real engineering conclusion, and not a failure of the process.
        /// </summary>
        [Fact]
        public void NoRemainingHeadroom_ProducesAnExplicitDiagnosticOutcomeAndNoModel()
        {
            AdjacencyCluster adjacencyCluster = Fixture(30, out List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors);

            DesignAirFlowCapacityEnvelope designAirFlowCapacityEnvelope = Envelope(adjacencyCluster, Step(adjacencyCluster), ventilationUnitCapacityDescriptors);

            Assert.False(designAirFlowCapacityEnvelope.IsScaled);
            Assert.Null(designAirFlowCapacityEnvelope.AdjacencyCluster);
            Assert.Null(designAirFlowCapacityEnvelope.RoundCandidate);

            Assert.Equal(DesignAirFlowCapacityEnvelopeOutcome.NoHeadroom, designAirFlowCapacityEnvelope.Outcome);

            DesignAirFlowCapacityEnvelopeGroup designAirFlowCapacityEnvelopeGroup = Assert.Single(designAirFlowCapacityEnvelope.Groups);

            Assert.Equal(DesignAirFlowCapacityEnvelopeOutcome.NoHeadroom, designAirFlowCapacityEnvelopeGroup.Outcome);

            Assert.Equal(0, designAirFlowCapacityEnvelopeGroup.SupplyHeadroom_Lps, 6);
            Assert.Equal(30, designAirFlowCapacityEnvelopeGroup.SupplyDuty_Before_Lps, 6);

            Assert.NotNull(designAirFlowCapacityEnvelopeGroup.Reason);
            Assert.True(double.IsNaN(designAirFlowCapacityEnvelopeGroup.Scale));
            Assert.Equal(FlowClassification.Undefined, designAirFlowCapacityEnvelopeGroup.BindingFlowClassification);

            //Not a refusal: the equipment answering "nothing more" is an answer.
            Assert.Empty(designAirFlowCapacityEnvelope.Refusals);
        }

        /// <summary>
        /// A design somehow already <b>past</b> its selected unit reports the same explicit "no headroom"
        /// answer, with a negative headroom on the record - rather than a factor below 1 that would quietly
        /// design the dwelling <i>downwards</i> in the name of a diagnostic.
        /// </summary>
        [Fact]
        public void ADesignAlreadyPastItsSelectedUnit_ReportsNoHeadroomRatherThanShrinkingTheDesign()
        {
            AdjacencyCluster adjacencyCluster = Fixture(25, out List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors);

            DesignAirFlowCapacityEnvelope designAirFlowCapacityEnvelope = Envelope(adjacencyCluster, Step(adjacencyCluster), ventilationUnitCapacityDescriptors);

            Assert.Equal(DesignAirFlowCapacityEnvelopeOutcome.NoHeadroom, designAirFlowCapacityEnvelope.Outcome);
            Assert.Null(designAirFlowCapacityEnvelope.AdjacencyCluster);

            DesignAirFlowCapacityEnvelopeGroup designAirFlowCapacityEnvelopeGroup = Assert.Single(designAirFlowCapacityEnvelope.Groups);

            Assert.Equal(-5, designAirFlowCapacityEnvelopeGroup.SupplyHeadroom_Lps, 6);

            //Never a factor at all - and certainly not the 25/30 that would have shrunk the design.
            Assert.True(double.IsNaN(designAirFlowCapacityEnvelopeGroup.Scale));
            Assert.True(double.IsNaN(designAirFlowCapacityEnvelopeGroup.Scale_Capacity));
        }

        /// <summary>
        /// A product rated at <b>zero</b> is a perfectly valid catalogue entry that simply cannot carry
        /// anything, so it is genuinely exhausted rather than unknown - and the honest answer is no headroom.
        /// </summary>
        [Fact]
        public void ASelectedProductRatedAtZero_IsExhaustedRatherThanUnresolved()
        {
            AdjacencyCluster adjacencyCluster = Fixture(90, out List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors);

            DesignAirFlowCapacityEnvelope designAirFlowCapacityEnvelope = Envelope(adjacencyCluster, Step(adjacencyCluster), [
                new VentilationUnitCapacityDescriptor(ventilationUnitCapacityDescriptors[0].VentilationUnitReference, 0, 0, 0)]);

            Assert.Equal(DesignAirFlowCapacityEnvelopeOutcome.NoHeadroom, designAirFlowCapacityEnvelope.Outcome);
            Assert.Null(designAirFlowCapacityEnvelope.AdjacencyCluster);

            DesignAirFlowCapacityEnvelopeGroup designAirFlowCapacityEnvelopeGroup = Assert.Single(designAirFlowCapacityEnvelope.Groups);

            Assert.True(designAirFlowCapacityEnvelopeGroup.VentilationUnitCapacityDescriptor.IsValid);

            //Already 30 l/s past a rating of nothing, and reported as exactly that.
            Assert.Equal(-30, designAirFlowCapacityEnvelopeGroup.SupplyHeadroom_Lps, 6);
        }

        /// <summary>
        /// A unit moving <b>no design air at all</b> has no design vector to grow and no proportions to
        /// preserve, however large the product selected for it. Reported as its own sentence rather than as a
        /// division by nothing: "there is nothing here to make bigger" is a different finding from "this is
        /// as big as it goes".
        /// </summary>
        [Fact]
        public void AUnitMovingNoDesignAir_HasNoDesignVectorToGrow()
        {
            AdjacencyCluster adjacencyCluster = Fixture(90, out List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors);

            //Every design terminal of the dwelling taken to nothing, which leaves the dwelling balanced at
            //0/0 and compliant only because its requirements go with it.
            Zero(adjacencyCluster);

            DesignAirFlowCapacityEnvelope designAirFlowCapacityEnvelope = Envelope(adjacencyCluster, [
                Target(adjacencyCluster, name_Kitchen, FlowClassification.Extract, 5)], ventilationUnitCapacityDescriptors);

            Assert.Equal(DesignAirFlowCapacityEnvelopeOutcome.NoHeadroom, designAirFlowCapacityEnvelope.Outcome);
            Assert.Null(designAirFlowCapacityEnvelope.AdjacencyCluster);

            Assert.Contains("moves no design air at all", Assert.Single(designAirFlowCapacityEnvelope.Groups).Reason);
        }

        // ---- The capacity authority has to resolve, and an unknown ceiling is not an unlimited one -------

        /// <summary>
        /// No catalogue means no ceiling, and therefore no envelope. This is deliberately <b>not</b> the
        /// backward-compatible "equipment is no constraint" meaning a null catalogue has elsewhere in this
        /// area: there it frees a design to be explored, here it removes the only thing being explored
        /// towards.
        /// </summary>
        [Fact]
        public void NoCatalogue_IsAnUnresolvedCeilingRatherThanAnUnlimitedOne()
        {
            AdjacencyCluster adjacencyCluster = Fixture(90, out List<VentilationUnitCapacityDescriptor> _);

            DesignAirFlowCapacityEnvelope designAirFlowCapacityEnvelope = adjacencyCluster.EvaluateDesignAirFlowCapacityEnvelope(Step(adjacencyCluster), PartFExtractAllocationStrategy.MinimumFirstCookingPriority, tolerance_Lps, null);

            Assert.Equal(DesignAirFlowCapacityEnvelopeOutcome.CapacityUnresolved, designAirFlowCapacityEnvelope.Outcome);
            Assert.Null(designAirFlowCapacityEnvelope.AdjacencyCluster);
            Assert.NotNull(designAirFlowCapacityEnvelope.Reason);
        }

        /// <summary>
        /// A unit with nothing selected on it has no product to be taken to its ceiling, so its group is
        /// reported unresolved - and nothing is selected to create one.
        /// </summary>
        [Fact]
        public void AUnitWithNothingSelected_IsReportedUnresolvedAndNothingIsSelectedForIt()
        {
            AdjacencyCluster adjacencyCluster = Fixture(90, out List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors, false);

            DesignAirFlowCapacityEnvelope designAirFlowCapacityEnvelope = Envelope(adjacencyCluster, Step(adjacencyCluster), ventilationUnitCapacityDescriptors);

            Assert.Equal(DesignAirFlowCapacityEnvelopeOutcome.CapacityUnresolved, designAirFlowCapacityEnvelope.Outcome);
            Assert.Null(designAirFlowCapacityEnvelope.AdjacencyCluster);

            Assert.Equal(DesignAirFlowCapacityEnvelopeOutcome.CapacityUnresolved, Assert.Single(designAirFlowCapacityEnvelope.Groups).Outcome);

            Assert.Null(SelectedModel(adjacencyCluster, "AHU 1"));
        }

        /// <summary>
        /// A unit selected as a product the catalogue offered does not contain has an <b>unknown</b>
        /// capacity, which is never an unlimited one - so the group is unresolved rather than grown towards
        /// infinity.
        /// </summary>
        [Fact]
        public void AUnitSelectedAsSomethingNotOffered_HasAnUnknownCeilingRatherThanNoCeiling()
        {
            AdjacencyCluster adjacencyCluster = Fixture(90, out List<VentilationUnitCapacityDescriptor> _);

            DesignAirFlowCapacityEnvelope designAirFlowCapacityEnvelope = Envelope(adjacencyCluster, Step(adjacencyCluster), [
                new VentilationUnitCapacityDescriptor(new VentilationUnitReference("Test Fixture", "Something Else", null), 500, 500, 0)]);

            Assert.Equal(DesignAirFlowCapacityEnvelopeOutcome.CapacityUnresolved, designAirFlowCapacityEnvelope.Outcome);
            Assert.Null(designAirFlowCapacityEnvelope.AdjacencyCluster);

            Assert.Null(Assert.Single(designAirFlowCapacityEnvelope.Groups).VentilationUnitCapacityDescriptor);
        }

        /// <summary>
        /// A catalogue entry that <b>exists</b> but does not state a usable capacity - a negative or
        /// non-finite maximum - is an <b>unknown</b> ceiling, not an exhausted one.
        /// <para>
        /// This is the sharper half of "an unknown capacity is never an unlimited one": left to the
        /// arithmetic, a NaN maximum gives a NaN ratio and a negative one gives a ratio below 1, and both
        /// would fall into the no-headroom branch - reporting a malformed catalogue as a perfectly good unit
        /// with nothing left to give, which is a far more convincing wrong answer than no answer.
        /// </para>
        /// </summary>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(-1)]
        public void ASelectedProductWithNoUsableCapacity_IsUnresolvedRatherThanExhausted(double maximum_Lps)
        {
            AdjacencyCluster adjacencyCluster = Fixture(90, out List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors);

            //The SAME product identity the unit is selected as, offered with a capacity that is not one.
            DesignAirFlowCapacityEnvelope designAirFlowCapacityEnvelope = Envelope(adjacencyCluster, Step(adjacencyCluster), [
                new VentilationUnitCapacityDescriptor(ventilationUnitCapacityDescriptors[0].VentilationUnitReference, maximum_Lps, maximum_Lps, 0)]);

            Assert.Equal(DesignAirFlowCapacityEnvelopeOutcome.CapacityUnresolved, designAirFlowCapacityEnvelope.Outcome);
            Assert.Null(designAirFlowCapacityEnvelope.AdjacencyCluster);

            DesignAirFlowCapacityEnvelopeGroup designAirFlowCapacityEnvelopeGroup = Assert.Single(designAirFlowCapacityEnvelope.Groups);

            Assert.Equal(DesignAirFlowCapacityEnvelopeOutcome.CapacityUnresolved, designAirFlowCapacityEnvelopeGroup.Outcome);

            //NOT NoHeadroom, which is what the arithmetic alone would have said - and never a factor.
            Assert.NotEqual(DesignAirFlowCapacityEnvelopeOutcome.NoHeadroom, designAirFlowCapacityEnvelopeGroup.Outcome);
            Assert.True(double.IsNaN(designAirFlowCapacityEnvelopeGroup.Scale));

            //The descriptor IS resolved - the entry exists - and is reported, so an engineer can see the
            //figure that is wrong rather than only that something is.
            Assert.NotNull(designAirFlowCapacityEnvelopeGroup.VentilationUnitCapacityDescriptor);
            Assert.False(designAirFlowCapacityEnvelopeGroup.VentilationUnitCapacityDescriptor.IsValid);

            Assert.Contains("not a usable capacity", designAirFlowCapacityEnvelopeGroup.Reason);
        }

        // ---- Refusals -----------------------------------------------------------------------------------

        /// <summary>
        /// A request that is not a design airflow at all refuses the envelope, for exactly the reason it
        /// refuses an ordinary round: the vector is what says which equipment the diagnostic is about, and
        /// quietly enveloping the units behind the coherent half would answer a question about a scope
        /// nobody stated.
        /// </summary>
        [Fact]
        public void ATargetThatIsNotADesignAirflow_RefusesTheEnvelopeRatherThanBeingPartlyScoped()
        {
            AdjacencyCluster adjacencyCluster = Fixture(90, out List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors);

            DesignAirFlowCapacityEnvelope designAirFlowCapacityEnvelope = Envelope(adjacencyCluster, [
                Target(adjacencyCluster, name_Kitchen, FlowClassification.Extract, 27),
                Target(adjacencyCluster, name_Bathroom, FlowClassification.Extract, double.NaN)], ventilationUnitCapacityDescriptors);

            Assert.Equal(DesignAirFlowCapacityEnvelopeOutcome.Refused, designAirFlowCapacityEnvelope.Outcome);
            Assert.Null(designAirFlowCapacityEnvelope.AdjacencyCluster);

            Assert.NotEmpty(designAirFlowCapacityEnvelope.Refusals);
            Assert.Empty(designAirFlowCapacityEnvelope.Groups);
        }

        /// <summary>
        /// A dwelling that is not a valid balanced, Approved Document F compliant design is <b>not</b> grown -
        /// it is refused, exactly as an ordinary round refuses it, and the reason says the <i>source</i>
        /// design is what is wrong.
        /// <para>
        /// This is also why there is no retreat here. No factor repairs an unbalanced dwelling, so an
        /// envelope that bisected its way down towards x1 would spend thirty-two model copies arriving at the
        /// same refusal with the wrong reason attached. The identity growth is evaluated instead - the design
        /// restated as its own vector - and its refusal is reported as it stands.
        /// </para>
        /// </summary>
        [Fact]
        public void AnUnbalancedDwelling_IsRefusedRatherThanRepairedByAnEnvelope()
        {
            AdjacencyCluster adjacencyCluster = Fixture(90, out List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors);

            //Knocked out of balance directly - replaced by a same-guid terminal designed at 50 l/s against
            //the dwelling's 30 l/s of extract - so the precondition being tested is the one under test.
            VentilationTerminal ventilationTerminal = (adjacencyCluster.VentilationTerminals(Space(adjacencyCluster, name_Bedroom)) ?? []).Find(x => x?.FlowClassification == FlowClassification.Supply);

            Assert.NotNull(ventilationTerminal);

            VentilationTerminal ventilationTerminal_Unbalanced = new(ventilationTerminal.Guid, ventilationTerminal.Name, FlowClassification.Supply, 50);

            ventilationTerminal_Unbalanced.SetValue(VentilationTerminalParameter.PartFTerminalReference, ventilationTerminal.GetValue<PartFTerminalReference>(VentilationTerminalParameter.PartFTerminalReference));

            adjacencyCluster.AddObject(ventilationTerminal_Unbalanced);

            DesignAirFlowCapacityEnvelope designAirFlowCapacityEnvelope = Envelope(adjacencyCluster, Step(adjacencyCluster), ventilationUnitCapacityDescriptors);

            Assert.Equal(DesignAirFlowCapacityEnvelopeOutcome.Refused, designAirFlowCapacityEnvelope.Outcome);
            Assert.Null(designAirFlowCapacityEnvelope.AdjacencyCluster);

            Assert.NotEmpty(designAirFlowCapacityEnvelope.Refusals);

            DesignAirFlowCapacityEnvelopeGroup designAirFlowCapacityEnvelopeGroup = Assert.Single(designAirFlowCapacityEnvelope.Groups);

            Assert.Equal(DesignAirFlowCapacityEnvelopeOutcome.Refused, designAirFlowCapacityEnvelopeGroup.Outcome);

            //The SOURCE design is named as the problem, and no factor is claimed.
            Assert.Contains("is not itself a valid design to grow", designAirFlowCapacityEnvelopeGroup.Reason);
            Assert.True(double.IsNaN(designAirFlowCapacityEnvelopeGroup.Scale));
            Assert.Equal(FlowClassification.Undefined, designAirFlowCapacityEnvelopeGroup.BindingFlowClassification);
        }

        /// <summary>
        /// A group that reaches its rating names the side that bound it, in its own reason - a diagnostic
        /// whose whole purpose is to tell an engineer what to change cannot leave the binding side implicit.
        /// </summary>
        [Fact]
        public void AGroupThatReachesItsRating_StillNamesTheBindingSide()
        {
            AdjacencyCluster adjacencyCluster = Fixture(45, out List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors);

            DesignAirFlowCapacityEnvelopeGroup designAirFlowCapacityEnvelopeGroup = Assert.Single(Envelope(adjacencyCluster, Step(adjacencyCluster), ventilationUnitCapacityDescriptors).Groups);

            Assert.Equal(designAirFlowCapacityEnvelopeGroup.Scale_Capacity, designAirFlowCapacityEnvelopeGroup.Scale, 9);

            Assert.NotEqual(FlowClassification.Undefined, designAirFlowCapacityEnvelopeGroup.BindingFlowClassification);

            Assert.Contains("binds first", designAirFlowCapacityEnvelopeGroup.Reason);
            Assert.Contains("grown proportionally", designAirFlowCapacityEnvelopeGroup.Reason);
        }

        // ---- The real catalogue's own products, on the real ~150 l/s design ------------------------------
        //
        // Everything above is proved against fixture products, because the algorithm is what those tests are
        // about. These four are the Approved Document O closeout case, on the two products SAM_Systems now
        // ships and at the design figures native acceptance recorded: a ~150 l/s Flat 1 with the Nuaire
        // XBOXER XBC15 (190/190 l/s) DELIBERATELY selected, rather than the smaller MRXBOX the automatic
        // rule would pick, so the ~40 l/s of equipment-capacity headroom is a real product's headroom.
        //
        // The identities and capacities are stated here rather than read from a file: SAM.Analytical does not
        // reference SAM_Systems and must not start. That the figures below are what the shipped catalogue
        // actually says is SAM_Systems' own assertion, in VentilationUnitCatalogueTests.

        /// <summary>
        /// <b>The closeout case.</b> A 150/150 l/s design with XBC15's 190/190 l/s selected grows
        /// proportionally onto 190/190 and stops there - the 40 l/s of headroom is spent as design airflow,
        /// coherently, and not a thousandth past the rating.
        /// <code>
        /// last valid   Studio 1_0 supply  150      selected  Nuaire XBC15, 190/190 l/s
        ///              Studio 1_0 extract  82.5    scale     min(190/150, 190/150) = 1.26666...
        ///              Bathroom_2 extract  67.5    system    150/150
        /// envelope     Studio 1_0 supply  190      system    190/190, exactly on the rating
        ///              Studio 1_0 extract 104.5
        ///              Bathroom_2 extract  85.5    82.5/67.5 == 104.5/85.5 - the proportions survive
        /// </code>
        /// </summary>
        [Fact]
        public void XBC15sRating_IsTheCeilingTheOneFiftyDesignGrowsWithin()
        {
            AdjacencyCluster adjacencyCluster = RealLadderFixture(out List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors);

            DesignAirFlowCapacityEnvelope designAirFlowCapacityEnvelope = Envelope(adjacencyCluster, Step_RealLadder(adjacencyCluster), ventilationUnitCapacityDescriptors);

            Assert.True(designAirFlowCapacityEnvelope.IsScaled, designAirFlowCapacityEnvelope.Reason);
            Assert.Equal(DesignAirFlowCapacityEnvelopeOutcome.Scaled, designAirFlowCapacityEnvelope.Outcome);

            DesignAirFlowCapacityEnvelopeGroup designAirFlowCapacityEnvelopeGroup = Assert.Single(designAirFlowCapacityEnvelope.Groups);

            Assert.Equal(190.0 / 150.0, designAirFlowCapacityEnvelopeGroup.Scale, 9);

            //Grown onto the SELECTED product's rating, and the proportions between the two extract rooms are
            //untouched: the same dwelling, larger.
            Assert.Equal(190, Design(designAirFlowCapacityEnvelope.AdjacencyCluster, name_Studio, FlowClassification.Supply), 6);
            Assert.Equal(104.5, Design(designAirFlowCapacityEnvelope.AdjacencyCluster, name_Studio, FlowClassification.Extract), 6);
            Assert.Equal(85.5, Design(designAirFlowCapacityEnvelope.AdjacencyCluster, name_Bathroom_Studio, FlowClassification.Extract), 6);

            Assert.Equal(82.5 / 67.5, 104.5 / 85.5, 9);

            //On the rating, and not past it. This is the assertion that says 190 l/s acted as a CEILING.
            Assert.Equal(150, designAirFlowCapacityEnvelopeGroup.SupplyDuty_Before_Lps, 6);
            Assert.Equal(150, designAirFlowCapacityEnvelopeGroup.ExtractDuty_Before_Lps, 6);

            Assert.Equal(190, designAirFlowCapacityEnvelopeGroup.SupplyDuty_After_Lps, 6);
            Assert.Equal(190, designAirFlowCapacityEnvelopeGroup.ExtractDuty_After_Lps, 6);

            Assert.Equal(0, designAirFlowCapacityEnvelopeGroup.SupplyHeadroom_Lps, 6);
            Assert.Equal(0, designAirFlowCapacityEnvelopeGroup.ExtractHeadroom_Lps, 6);

            //The 40 l/s the closeout is about - spent as design airflow, on both sides.
            Assert.Equal(40, designAirFlowCapacityEnvelopeGroup.SupplyDuty_After_Lps - designAirFlowCapacityEnvelopeGroup.SupplyDuty_Before_Lps, 6);
            Assert.Equal(40, designAirFlowCapacityEnvelopeGroup.ExtractDuty_After_Lps - designAirFlowCapacityEnvelopeGroup.ExtractDuty_Before_Lps, 6);
        }

        /// <summary>
        /// XBC15 stays selected. The MRXBOX is on offer, is capable of the 150/150 design the envelope
        /// started from, and is <b>smaller on both sides</b> - so an envelope that re-ran the selection rule
        /// instead of reading the model would swap the plant for a cheaper unit and then find it had no
        /// headroom. Buying equipment is a deliberate decision; a diagnostic is not one.
        /// </summary>
        [Fact]
        public void TheEnvelope_KeepsXBC15Selected_EvenThoughTheSmallerProductIsStillOffered()
        {
            AdjacencyCluster adjacencyCluster = RealLadderFixture(out List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors);

            //The premise: the other product would be preferred by the automatic rule at this duty.
            VentilationUnitCapacityDescriptor ventilationUnitCapacityDescriptor_MRXBOX = ventilationUnitCapacityDescriptors.Find(x => x.VentilationUnitReference.Model == model_MRXBOX);

            Assert.NotNull(ventilationUnitCapacityDescriptor_MRXBOX);
            Assert.True(ventilationUnitCapacityDescriptor_MRXBOX.IsSufficientFor(150, 150));
            Assert.Equal(model_MRXBOX, Analytical.Query.SelectSmallestCapableVentilationUnit(ventilationUnitCapacityDescriptors, 150, 150).VentilationUnitReference.Model);

            DesignAirFlowCapacityEnvelope designAirFlowCapacityEnvelope = Envelope(adjacencyCluster, Step_RealLadder(adjacencyCluster), ventilationUnitCapacityDescriptors);

            Assert.True(designAirFlowCapacityEnvelope.IsScaled, designAirFlowCapacityEnvelope.Reason);

            Assert.Equal(model_XBC15, SelectedModel(adjacencyCluster, name_AirHandlingUnit_RealLadder));
            Assert.Equal(model_XBC15, SelectedModel(designAirFlowCapacityEnvelope.AdjacencyCluster, name_AirHandlingUnit_RealLadder));

            Assert.Equal(model_XBC15, Assert.Single(designAirFlowCapacityEnvelope.Groups).VentilationUnitReference.Model);

            //And nothing reports a reselection.
            Assert.All(designAirFlowCapacityEnvelope.RoundCandidate.DwellingRounds, x => Assert.Equal(VentilationUnitSelectionOutcome.Kept, x.VentilationUnitSelectionOutcome));
        }

        /// <summary>
        /// <b>The authority separation, on the real case.</b>
        /// <code>
        /// PartFRequiredAirFlow != DesignAirFlow != SelectedEquipmentCapacity != OperatingAirFlow
        /// </code>
        /// Design airflow moves. The Approved Document F requirements do not. The equipment capacity is read
        /// and never written anywhere. No runtime airflow is produced. And the design the envelope was
        /// calculated from is left exactly as it was, because an envelope is a design the ordinary
        /// optimisation policy refused.
        /// </summary>
        [Fact]
        public void TheEnvelopeOverXBC15_MovesDesignAirflowAndNothingElse()
        {
            AdjacencyCluster adjacencyCluster = RealLadderFixture(out List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors);

            Dictionary<string, double> requirements_Before = Requirements(adjacencyCluster);
            Dictionary<string, double> designs_Before = Designs(adjacencyCluster);
            List<string> airMovements_Before = AirMovements(adjacencyCluster);

            DesignAirFlowCapacityEnvelope designAirFlowCapacityEnvelope = Envelope(adjacencyCluster, Step_RealLadder(adjacencyCluster), ventilationUnitCapacityDescriptors);

            Assert.True(designAirFlowCapacityEnvelope.IsScaled, designAirFlowCapacityEnvelope.Reason);

            //1. Approved Document F is untouched in the grown model - the requirement is a floor that was
            //   read, never a figure that was written.
            Assert.Equal(requirements_Before, Requirements(designAirFlowCapacityEnvelope.AdjacencyCluster));

            Assert.Equal(30, Requirement(designAirFlowCapacityEnvelope.AdjacencyCluster, name_Studio, FlowClassification.Supply), 6);
            Assert.Equal(8, Requirement(designAirFlowCapacityEnvelope.AdjacencyCluster, name_Bathroom_Studio, FlowClassification.Extract), 6);

            //2. The capacity is nowhere in the model. 190 is the ceiling that bounded the growth, and the
            //   only place it appears is the catalogue the envelope was handed.
            Assert.Equal(190, Assert.Single(designAirFlowCapacityEnvelope.Groups).VentilationUnitCapacityDescriptor.MaximumSupplyFlowRate_Lps, 6);

            //3. No runtime airflow: the envelope creates, removes and re-rates no air movement.
            Assert.Equal(airMovements_Before, AirMovements(designAirFlowCapacityEnvelope.AdjacencyCluster));

            //4. The source design is untouched, by value and by reference.
            Assert.Equal(designs_Before, Designs(adjacencyCluster));
            Assert.NotSame(adjacencyCluster, designAirFlowCapacityEnvelope.AdjacencyCluster);
            Assert.Equal(150, Design(adjacencyCluster, name_Studio, FlowClassification.Supply), 6);

            //5. The realised design DID move, and the deliberate adjustments say so rather than leaving it
            //   to be inferred - so a changed design transfer air is reported against a changed design
            //   airflow, not against a capacity.
            Assert.NotEqual(designs_Before, Designs(designAirFlowCapacityEnvelope.AdjacencyCluster));
            Assert.Equal(150, Sum(designAirFlowCapacityEnvelope, FlowClassification.Supply), 6);
            Assert.Equal(150, Sum(designAirFlowCapacityEnvelope, FlowClassification.Extract), 6);
        }

        /// <summary>
        /// A design already standing on XBC15's rating has nothing left to give, and the envelope says so
        /// rather than growing past 190 l/s or reaching for a larger product. No capacity rule is relaxed to
        /// make the diagnostic produce an answer.
        /// </summary>
        [Fact]
        public void ADesignAlreadyAtXBC15sRating_ReportsNoHeadroomRatherThanExceedingIt()
        {
            AdjacencyCluster adjacencyCluster = RealLadderFixture(190, 104.5, 85.5, out List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors);

            DesignAirFlowCapacityEnvelope designAirFlowCapacityEnvelope = Envelope(adjacencyCluster, Step_RealLadder(adjacencyCluster), ventilationUnitCapacityDescriptors);

            Assert.False(designAirFlowCapacityEnvelope.IsScaled);
            Assert.Equal(DesignAirFlowCapacityEnvelopeOutcome.NoHeadroom, designAirFlowCapacityEnvelope.Outcome);
            Assert.Null(designAirFlowCapacityEnvelope.AdjacencyCluster);

            //Still XBC15, and still not swapped for the 500 l/s product that is also on offer.
            Assert.Equal(model_XBC15, SelectedModel(adjacencyCluster, name_AirHandlingUnit_RealLadder));
            Assert.Equal(model_XBC15, Assert.Single(designAirFlowCapacityEnvelope.Groups).VentilationUnitReference.Model);

            //And the design is left where it was rather than shrunk to create headroom to report.
            Assert.Equal(190, Design(adjacencyCluster, name_Studio, FlowClassification.Supply), 6);
        }

        // ---- Fixture ---------------------------------------------------------------------------------------
        /// <summary>
        /// <b>The real Flat 1 design, on the real catalogue's products.</b> The Flat 1 shape at the design
        /// figures native acceptance recorded once the MRXBOX's own envelope had been taken up - 150 supply /
        /// 82.5 + 67.5 extract, balanced at 150/150 - with the Nuaire XBOXER XBC15 (190/190 l/s)
        /// <b>deliberately selected</b> rather than the smaller MRXBOX the automatic rule would choose.
        /// <para>
        /// Both real products are offered, and the MRXBOX is offered FIRST, so an envelope that re-ran the
        /// selection rule or took the head of the list would visibly swap the plant.
        /// </para>
        /// </summary>
        private static AdjacencyCluster RealLadderFixture(out List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors)
        {
            return RealLadderFixture(150, 82.5, 67.5, out ventilationUnitCapacityDescriptors);
        }

        private static AdjacencyCluster RealLadderFixture(double design_Studio_Supply_Lps, double design_Studio_Extract_Lps, double design_Bathroom_Extract_Lps, out List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors)
        {
            ventilationUnitCapacityDescriptors = RealLadder();

            AdjacencyCluster result = new();

            //Selected as the XBC15 - the SECOND entry, and not the smallest capable one.
            VentilationSystem ventilationSystem = Unit(result, ventilationUnitCapacityDescriptors[1], name_AirHandlingUnit_RealLadder, "Flat 1");

            Space space_Studio = Room(result, name_Studio, PartFTerminalRole.Supply, 30);

            Extract(space_Studio, 13);

            Space space_Bathroom = Room(result, name_Bathroom_Studio, PartFTerminalRole.GeneralExtract, 8);

            Terminal(result, ventilationSystem, space_Studio, FlowClassification.Supply, design_Studio_Supply_Lps, 0);
            Terminal(result, ventilationSystem, space_Studio, FlowClassification.Extract, design_Studio_Extract_Lps, 1);
            Terminal(result, ventilationSystem, space_Bathroom, FlowClassification.Extract, design_Bathroom_Extract_Lps, 0);

            result.AddRelation(ventilationSystem, space_Studio);
            result.AddRelation(ventilationSystem, space_Bathroom);

            return result;
        }

        /// <summary>
        /// The two products SAM_Systems ships, at their published capacities and declared ranks, plus the
        /// far larger fixture product every other test in this file offers and none of them may reach for.
        /// <para>
        /// Stated rather than read: <c>SAM.Analytical</c> does not reference <c>SAM.Analytical.Systems</c>
        /// and adding a manufacturer must never need a change here. That these figures are what the shipped
        /// catalogue says is asserted in <c>SAM_Systems</c>' own <c>VentilationUnitCatalogueTests</c>.
        /// </para>
        /// </summary>
        private static List<VentilationUnitCapacityDescriptor> RealLadder()
        {
            return
            [
                new VentilationUnitCapacityDescriptor(new VentilationUnitReference("Nuaire", model_MRXBOX, "MR-ECO-COOL-V"), 150, 150, 10),
                new VentilationUnitCapacityDescriptor(new VentilationUnitReference("Nuaire", model_XBC15, null), 190, 190, 20),
                new VentilationUnitCapacityDescriptor(new VentilationUnitReference("Test Fixture", "Never Selected", null), 500, 500, 99),
            ];
        }

        /// <summary>
        /// The +5 l/s step the ordinary policy would have asked for next on the real design. <b>Read for
        /// scope only</b> - which equipment the diagnostic is about, never a figure.
        /// </summary>
        private static List<DesignAirFlowTarget> Step_RealLadder(AdjacencyCluster adjacencyCluster)
        {
            return
            [
                Target(adjacencyCluster, name_Studio, FlowClassification.Supply, Design(adjacencyCluster, name_Studio, FlowClassification.Supply) + 5),
                Target(adjacencyCluster, name_Bathroom_Studio, FlowClassification.Extract, Design(adjacencyCluster, name_Bathroom_Studio, FlowClassification.Extract) + 5),
            ];
        }


        /// <summary>
        /// One flat, its own ventilation system and its own air handling unit, balanced at 30/30 l/s and
        /// meeting its Approved Document F requirements with headroom - with the unit selected as a product
        /// rated at whatever this test needs, which is what makes the remaining headroom the variable under
        /// test.
        /// <code>
        /// Bedroom 1  supply   requirement 13   design 30
        /// Kitchen 1  extract  requirement 13   design 22   (local kitchen extract)
        /// Bathroom 1 extract  requirement  8   design  8
        /// </code>
        /// </summary>
        private static AdjacencyCluster Fixture(double maximum_Lps, out List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors)
        {
            return Fixture(maximum_Lps, out ventilationUnitCapacityDescriptors, true);
        }

        private static AdjacencyCluster Fixture(double maximum_Lps, out List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors, bool select)
        {
            ventilationUnitCapacityDescriptors = Descriptors(maximum_Lps, maximum_Lps);

            AdjacencyCluster result = new();

            Dwelling(result, ventilationUnitCapacityDescriptors[0], "AHU 1", 1, select);

            return result;
        }

        /// <summary>
        /// <b>The real Flat 1 shape from the brief</b>, and the reason a room carrying design airflow on
        /// <i>both</i> sides needs its own fixture: the studio is supplied and extracted, and the studio's
        /// extract contribution is exactly what the old diagnostic left behind and the old report could not
        /// account for.
        /// <code>
        /// Studio 1_0  supply   requirement 30   design 40
        /// Studio 1_0  extract  requirement 13   design 22
        /// Bathroom_2  extract  requirement  8   design 18
        ///                                       system 40/40
        /// </code>
        /// </summary>
        private static AdjacencyCluster FlatOneFixture(double maximum_Lps, out List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors)
        {
            return FlatOneFixture(maximum_Lps, maximum_Lps, out ventilationUnitCapacityDescriptors);
        }

        private static AdjacencyCluster FlatOneFixture(double maximum_Supply_Lps, double maximum_Extract_Lps, out List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors)
        {
            ventilationUnitCapacityDescriptors = Descriptors(maximum_Supply_Lps, maximum_Extract_Lps);

            AdjacencyCluster result = new();

            VentilationSystem ventilationSystem = Unit(result, ventilationUnitCapacityDescriptors[0], "MVHR-01", "Flat 1");

            Space space_Studio = Room(result, name_Studio, PartFTerminalRole.Supply, 30);

            //A SECOND Approved Document F requirement on the same room, on the other side - which is what
            //gives the studio an extract contribution of its own.
            Extract(space_Studio, 13);

            Space space_Bathroom = Room(result, name_Bathroom_Studio, PartFTerminalRole.GeneralExtract, 8);

            Terminal(result, ventilationSystem, space_Studio, FlowClassification.Supply, 40, 0);
            Terminal(result, ventilationSystem, space_Studio, FlowClassification.Extract, 22, 1);
            Terminal(result, ventilationSystem, space_Bathroom, FlowClassification.Extract, 18, 0);

            result.AddRelation(ventilationSystem, space_Studio);
            result.AddRelation(ventilationSystem, space_Bathroom);

            return result;
        }

        /// <summary>
        /// Three supply rooms and three extract rooms at six different figures, balanced at 50/50 - so a
        /// proportional growth is distinguishable from every other allocation rule at once.
        /// <code>
        /// Bedroom M A supply  requirement  6   design 12
        /// Bedroom M B supply  requirement  6   design 18
        /// Living M    supply  requirement  6   design 20
        /// Kitchen M   extract requirement 13   design 30   (local kitchen extract)
        /// Bathroom M  extract requirement  8   design 15
        /// Ensuite M   extract requirement  5   design  5
        /// </code>
        /// </summary>
        private static AdjacencyCluster MultiRoomFixture(double maximum_Lps, out List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors)
        {
            ventilationUnitCapacityDescriptors = Descriptors(maximum_Lps, maximum_Lps);

            AdjacencyCluster result = new();

            VentilationSystem ventilationSystem = Unit(result, ventilationUnitCapacityDescriptors[0], "AHU 1", "Flat M");

            Terminal(result, ventilationSystem, Room(result, "Bedroom M A", PartFTerminalRole.Supply, 6), FlowClassification.Supply, 12, 0);
            Terminal(result, ventilationSystem, Room(result, "Bedroom M B", PartFTerminalRole.Supply, 6), FlowClassification.Supply, 18, 0);
            Terminal(result, ventilationSystem, Room(result, "Living M", PartFTerminalRole.Supply, 6), FlowClassification.Supply, 20, 0);

            Terminal(result, ventilationSystem, Room(result, "Kitchen M", PartFTerminalRole.LocalKitchenExtract, 13), FlowClassification.Extract, 30, 0);
            Terminal(result, ventilationSystem, Room(result, "Bathroom M", PartFTerminalRole.GeneralExtract, 8), FlowClassification.Extract, 15, 0);
            Terminal(result, ventilationSystem, Room(result, "Ensuite M", PartFTerminalRole.GeneralExtract, 5), FlowClassification.Extract, 5, 0);

            foreach (Space space in result.GetSpaces() ?? [])
            {
                result.AddRelation(ventilationSystem, space);
            }

            return result;
        }

        /// <summary>
        /// Two flats on two <b>separate</b> units, rated 45/45 and 90/90 - so each one's own ceiling is a
        /// different distance away and a single global factor could not satisfy both.
        /// </summary>
        private static AdjacencyCluster TwoDwellingFixture(out List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors)
        {
            ventilationUnitCapacityDescriptors =
            [
                new VentilationUnitCapacityDescriptor(new VentilationUnitReference("Test Fixture", "Selected", null), 45, 45, 0),
                new VentilationUnitCapacityDescriptor(new VentilationUnitReference("Test Fixture", "Selected Large", null), 90, 90, 1),
            ];

            AdjacencyCluster result = new();

            Dwelling(result, ventilationUnitCapacityDescriptors[0], "AHU 1", 1, true);
            Dwelling(result, ventilationUnitCapacityDescriptors[1], "AHU 2", 2, true);

            return result;
        }

        /// <summary>
        /// <b>One</b> air handling unit serving <b>two</b> ventilation systems - the general MEP arrangement
        /// the envelope has to stay correct for, and the one the Approved Document O one-unit-per-dwelling
        /// shape hides.
        /// <code>
        /// Flat A   Bedroom A supply 10 (requires 5)   Kitchen A extract 10 (requires 5)
        /// Flat B   Bedroom B supply 25 (requires 5)   Kitchen B extract 25 (requires 5)
        ///                                             unit duty 35/35
        /// </code>
        /// </summary>
        private static AdjacencyCluster SharedUnitFixture(double maximum_Lps, out List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors)
        {
            ventilationUnitCapacityDescriptors = Descriptors(maximum_Lps, maximum_Lps);

            AdjacencyCluster result = new();

            AirHandlingUnit airHandlingUnit = new("MVHR-S", 20, 20);

            airHandlingUnit.SetValue(AirHandlingUnitParameter.VentilationUnitReference, ventilationUnitCapacityDescriptors[0].VentilationUnitReference);

            result.AddObject(airHandlingUnit);

            Shared(result, airHandlingUnit, "A", 10);
            Shared(result, airHandlingUnit, "B", 25);

            return result;
        }

        /// <summary>
        /// The selected product at the rating a test needs, and a far larger one beside it that the envelope
        /// must never reach for.
        /// </summary>
        private static List<VentilationUnitCapacityDescriptor> Descriptors(double maximum_Supply_Lps, double maximum_Extract_Lps)
        {
            return
            [
                new VentilationUnitCapacityDescriptor(new VentilationUnitReference("Test Fixture", "Selected", null), maximum_Supply_Lps, maximum_Extract_Lps, 0),
                new VentilationUnitCapacityDescriptor(new VentilationUnitReference("Test Fixture", "Never Selected", null), 500, 500, 1),
            ];
        }

        private static void Dwelling(AdjacencyCluster adjacencyCluster, VentilationUnitCapacityDescriptor ventilationUnitCapacityDescriptor, string name_AirHandlingUnit, int index, bool select)
        {
            VentilationSystem ventilationSystem = Unit(adjacencyCluster, select ? ventilationUnitCapacityDescriptor : null, name_AirHandlingUnit, string.Format("Flat {0}", index));

            Space space_Bedroom = Room(adjacencyCluster, string.Format("Bedroom {0}", index), PartFTerminalRole.Supply, 13);
            Space space_Kitchen = Room(adjacencyCluster, string.Format("Kitchen {0}", index), PartFTerminalRole.LocalKitchenExtract, 13);
            Space space_Bathroom = Room(adjacencyCluster, string.Format("Bathroom {0}", index), PartFTerminalRole.GeneralExtract, 8);

            Terminal(adjacencyCluster, ventilationSystem, space_Bedroom, FlowClassification.Supply, 30, 0);
            Terminal(adjacencyCluster, ventilationSystem, space_Kitchen, FlowClassification.Extract, 22, 0);
            Terminal(adjacencyCluster, ventilationSystem, space_Bathroom, FlowClassification.Extract, 8, 0);

            adjacencyCluster.AddRelation(ventilationSystem, space_Bedroom);
            adjacencyCluster.AddRelation(ventilationSystem, space_Kitchen);
            adjacencyCluster.AddRelation(ventilationSystem, space_Bathroom);
        }

        /// <summary>One dwelling of <see cref="SharedUnitFixture"/>, hung off the unit both of them share.</summary>
        private static void Shared(AdjacencyCluster adjacencyCluster, AirHandlingUnit airHandlingUnit, string suffix, double designFlowRate_Lps)
        {
            VentilationSystem ventilationSystem = new(string.Format("Flat {0}", suffix), new VentilationSystemType("Fixture MVHR", "Fixture"));

            //The SAME unit name for both, which is how Query.AirHandlingUnit resolves them onto one unit.
            ventilationSystem.SetValue(VentilationSystemParameter.SupplyUnitName, airHandlingUnit.Name);

            adjacencyCluster.AddObject(ventilationSystem);

            Space space_Bedroom = Room(adjacencyCluster, string.Format("Bedroom {0}", suffix), PartFTerminalRole.Supply, 5);
            Space space_Kitchen = Room(adjacencyCluster, string.Format("Kitchen {0}", suffix), PartFTerminalRole.LocalKitchenExtract, 5);

            Terminal(adjacencyCluster, ventilationSystem, space_Bedroom, FlowClassification.Supply, designFlowRate_Lps, 0);
            Terminal(adjacencyCluster, ventilationSystem, space_Kitchen, FlowClassification.Extract, designFlowRate_Lps, 0);

            adjacencyCluster.AddRelation(ventilationSystem, space_Bedroom);
            adjacencyCluster.AddRelation(ventilationSystem, space_Kitchen);
        }

        /// <summary>
        /// An air handling unit - selected as <paramref name="ventilationUnitCapacityDescriptor"/> where one
        /// is supplied - and one ventilation system resolving to it by name.
        /// <para>
        /// The selection is written directly rather than through <c>Modify.SelectVentilationUnit</c>, so the
        /// fixture states which product is selected instead of depending on the selection rule these tests
        /// are not about - and so it can deliberately not be the smallest capable one.
        /// </para>
        /// </summary>
        private static VentilationSystem Unit(AdjacencyCluster adjacencyCluster, VentilationUnitCapacityDescriptor ventilationUnitCapacityDescriptor, string name_AirHandlingUnit, string name_VentilationSystem)
        {
            AirHandlingUnit airHandlingUnit = new(name_AirHandlingUnit, 20, 20);

            if (ventilationUnitCapacityDescriptor is not null)
            {
                airHandlingUnit.SetValue(AirHandlingUnitParameter.VentilationUnitReference, ventilationUnitCapacityDescriptor.VentilationUnitReference);
            }

            adjacencyCluster.AddObject(airHandlingUnit);

            VentilationSystem result = new(name_VentilationSystem, new VentilationSystemType("Fixture MVHR", "Fixture"));
            result.SetValue(VentilationSystemParameter.SupplyUnitName, airHandlingUnit.Name);

            adjacencyCluster.AddObject(result);

            return result;
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

        /// <summary>A second Approved Document F requirement on a room already sized for supply.</summary>
        private static void Extract(Space space, double requirement_Lps)
        {
            PartFSpaceData partFSpaceData = space.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData);

            partFSpaceData.Terminals.Add(new PartFVentilationTerminalRequirement(space.Name + " extract requirement", space.Guid, PartFTerminalRole.GeneralExtract)
            {
                ContinuousDesignFlowRate_Lps = requirement_Lps,
            });

            space.SetValue(SpaceParameter.PartFSpaceData, partFSpaceData);
        }

        /// <param name="index">Which of the room's Approved Document F requirements this terminal realizes -
        /// 0 for a room sized on one side, and 1 for the second side of a room sized on both.</param>
        private static void Terminal(AdjacencyCluster adjacencyCluster, VentilationSystem ventilationSystem, Space space, FlowClassification flowClassification, double designFlowRate_Lps, int index)
        {
            PartFVentilationTerminalRequirement partFVentilationTerminalRequirement = space.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData).Terminals[index];

            VentilationTerminal ventilationTerminal = new(string.Format("{0} {1} terminal", space.Name, Core.Query.Description(flowClassification)), flowClassification, designFlowRate_Lps);
            ventilationTerminal.SetValue(VentilationTerminalParameter.PartFTerminalReference, new PartFTerminalReference(partFVentilationTerminalRequirement));

            adjacencyCluster.AddObject(ventilationTerminal);
            adjacencyCluster.AddRelation(ventilationTerminal, space);
            adjacencyCluster.AddRelation(ventilationTerminal, ventilationSystem);
        }

        /// <summary>
        /// Every design terminal and every Approved Document F requirement of a model taken to nothing - a
        /// dwelling that is still balanced and still compliant, and that a capacity envelope has nothing to
        /// grow.
        /// </summary>
        private static void Zero(AdjacencyCluster adjacencyCluster)
        {
            foreach (VentilationTerminal ventilationTerminal in adjacencyCluster.GetObjects<VentilationTerminal>() ?? [])
            {
                VentilationTerminal ventilationTerminal_Zero = new(ventilationTerminal.Guid, ventilationTerminal.Name, ventilationTerminal.FlowClassification, 0);

                ventilationTerminal_Zero.SetValue(VentilationTerminalParameter.PartFTerminalReference, ventilationTerminal.GetValue<PartFTerminalReference>(VentilationTerminalParameter.PartFTerminalReference));

                adjacencyCluster.AddObject(ventilationTerminal_Zero);
            }

            foreach (Space space in adjacencyCluster.GetSpaces() ?? [])
            {
                PartFSpaceData partFSpaceData = space.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData);
                if (partFSpaceData is null)
                {
                    continue;
                }

                foreach (PartFVentilationTerminalRequirement partFVentilationTerminalRequirement in partFSpaceData.Terminals)
                {
                    partFVentilationTerminalRequirement.ContinuousDesignFlowRate_Lps = 0;
                }

                space.SetValue(SpaceParameter.PartFSpaceData, partFSpaceData);

                adjacencyCluster.AddObject(space);
            }
        }

        /// <summary>
        /// The deliberate target vector the ordinary +5 l/s policy would currently create for
        /// <see cref="Fixture(double, out List{VentilationUnitCapacityDescriptor})"/> - the two failing
        /// extract rooms, each asked for one whole step. <b>Read for scope only.</b>
        /// </summary>
        private static List<DesignAirFlowTarget> Step(AdjacencyCluster adjacencyCluster)
        {
            return
            [
                Target(adjacencyCluster, name_Kitchen, FlowClassification.Extract, 27),
                Target(adjacencyCluster, name_Bathroom, FlowClassification.Extract, 13),
            ];
        }

        /// <summary>The same, for the Flat 1 shape. <b>Read for scope only.</b></summary>
        private static List<DesignAirFlowTarget> Step_FlatOne(AdjacencyCluster adjacencyCluster)
        {
            return
            [
                Target(adjacencyCluster, name_Studio, FlowClassification.Supply, 45),
                Target(adjacencyCluster, name_Bathroom_Studio, FlowClassification.Extract, 23),
            ];
        }

        private static DesignAirFlowCapacityEnvelope Envelope(AdjacencyCluster adjacencyCluster, List<DesignAirFlowTarget> designAirFlowTargets, List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors)
        {
            return adjacencyCluster.EvaluateDesignAirFlowCapacityEnvelope(designAirFlowTargets, PartFExtractAllocationStrategy.MinimumFirstCookingPriority, tolerance_Lps, ventilationUnitCapacityDescriptors);
        }

        private static DesignAirFlowTarget Target(AdjacencyCluster adjacencyCluster, string name, FlowClassification flowClassification, double designFlowRate_Lps)
        {
            return new DesignAirFlowTarget(Space(adjacencyCluster, name), flowClassification, designFlowRate_Lps);
        }

        private static Space Space(AdjacencyCluster adjacencyCluster, string name)
        {
            Space result = (adjacencyCluster.GetSpaces() ?? []).Find(x => x?.Name == name);

            Assert.NotNull(result);

            return result;
        }

        /// <summary>Every room's design airflow on both sides, so a whole model can be compared before and after.</summary>
        private static Dictionary<string, double> Designs(AdjacencyCluster adjacencyCluster)
        {
            Dictionary<string, double> result = [];

            foreach (Space space in adjacencyCluster.GetSpaces() ?? [])
            {
                foreach (FlowClassification flowClassification in new[] { FlowClassification.Supply, FlowClassification.Extract })
                {
                    result[string.Format("{0} {1}", space.Name, flowClassification)] = System.Math.Round(Design(adjacencyCluster, space, flowClassification), 6);
                }
            }

            return result;
        }

        /// <summary>Every room's Approved Document F requirement on both sides - the values no envelope may move.</summary>
        private static Dictionary<string, double> Requirements(AdjacencyCluster adjacencyCluster)
        {
            Dictionary<string, double> result = [];

            foreach (Space space in adjacencyCluster.GetSpaces() ?? [])
            {
                foreach (FlowClassification flowClassification in new[] { FlowClassification.Supply, FlowClassification.Extract })
                {
                    result[string.Format("{0} {1}", space.Name, flowClassification)] = adjacencyCluster.PartFRequiredFlowRate_Lps(space, flowClassification) ?? double.NaN;
                }
            }

            return result;
        }

        private static double Requirement(AdjacencyCluster adjacencyCluster, string name, FlowClassification flowClassification)
        {
            return adjacencyCluster.PartFRequiredFlowRate_Lps(Space(adjacencyCluster, name), flowClassification) ?? double.NaN;
        }

        private static double Design(AdjacencyCluster adjacencyCluster, string name, FlowClassification flowClassification)
        {
            return Design(adjacencyCluster, Space(adjacencyCluster, name), flowClassification);
        }

        private static double Design(AdjacencyCluster adjacencyCluster, Space space, FlowClassification flowClassification)
        {
            return adjacencyCluster.VentilationTerminals(space).VentilationTerminalDesignDuty_Lps(flowClassification) ?? 0;
        }

        /// <summary>The BEFORE column of every deliberate adjustment on one side, summed.</summary>
        private static double Sum(DesignAirFlowCapacityEnvelope designAirFlowCapacityEnvelope, FlowClassification flowClassification)
        {
            double result = 0;

            foreach (DesignAirFlowAdjustment designAirFlowAdjustment in designAirFlowCapacityEnvelope.TargetedAdjustments)
            {
                if (designAirFlowAdjustment.FlowClassification == flowClassification)
                {
                    result += designAirFlowAdjustment.Before_Lps;
                }
            }

            return result;
        }

        private static string SelectedModel(AdjacencyCluster adjacencyCluster, string name)
        {
            AirHandlingUnit airHandlingUnit = (adjacencyCluster.GetObjects<AirHandlingUnit>() ?? []).Find(x => x?.Name == name);

            Assert.NotNull(airHandlingUnit);

            return airHandlingUnit.SelectedVentilationUnitReference()?.Model;
        }

        /// <summary>
        /// Every inter-zone air movement in the model, as text. A capacity envelope must not create, remove
        /// or re-rate one - that is the preparation's job, and doing it here would collapse design airflow
        /// into runtime airflow.
        /// </summary>
        private static List<string> AirMovements(AdjacencyCluster adjacencyCluster)
        {
            List<string> result = (adjacencyCluster.GetObjects<SpaceAirMovement>() ?? []).ConvertAll(x => string.Format("space|{0}|{1}", x.Name, x.AirFlow));

            result.AddRange((adjacencyCluster.GetObjects<AirHandlingUnitAirMovement>() ?? []).ConvertAll(x => string.Format("ahu|{0}", x.Name)));

            result.Sort(StringComparer.Ordinal);

            return result;
        }
    }
}
