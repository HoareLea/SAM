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
    /// <b>Approved Document O Iteration 2B - one deterministic design airflow optimisation round.</b>
    /// <para>
    /// A TM59 run fails several rooms at once. Raising them one at a time, rebalancing between each, gives
    /// an answer that depends on the order the results happened to come back in - and, under the
    /// cooking-priority extract strategy, can land a balancing consequence on the very kitchen the next
    /// step is about to target. <c>Modify.EvaluateTargetedDesignAirFlows</c> exists to make both impossible,
    /// and these tests pin the properties that claim rests on.
    /// </para>
    /// <para>
    /// <b>Not a second copy of the Approved Document F or capacity arithmetic.</b> Every rule the round
    /// applies is applied by the code that already owns it - the same allocator, the same compliance
    /// reconciliation, the same capacity query a single manual change uses. What is tested here is what the
    /// round adds: order independence, the single combined derivation, the targeted/derived boundary, and
    /// the all-or-nothing transaction that a fixed-step automatic optimiser needs and a clamp cannot give.
    /// </para>
    /// <para>
    /// The fixture is built by hand rather than through <c>PartFCalculator</c>: these tests are about the
    /// round's transaction semantics, and a hand-built dwelling states the requirements and designs under
    /// test plainly instead of implying them through a sizing calculation that is exercised elsewhere.
    /// </para>
    /// </summary>
    public class PartODesignAirFlowRoundTests
    {
        private const double tolerance_Lps = 0.001;

        private const string name_Bedroom = "Bedroom";

        private const string name_Kitchen = "Kitchen";

        private const string name_Bathroom = "Bathroom";

        // ---- A. Order independence -----------------------------------------------------------------------

        /// <summary>
        /// <b>A.</b> The same set of targets, supplied in opposite orders, produces the same design - every
        /// room, both sides, to the last decimal.
        /// <para>
        /// This is the whole reason the operation exists. Two extract targets in one flat rebalance onto the
        /// same supply room, and applying them one at a time would compute the second allocation over a
        /// dwelling the first had already moved.
        /// </para>
        /// </summary>
        [Fact]
        public void MultipleTargetsInOneDwelling_AreOrderIndependent()
        {
            AdjacencyCluster adjacencyCluster = Fixture();

            DesignAirFlowRoundCandidate candidate_Forward = Round(adjacencyCluster, [
                Target(adjacencyCluster, Name(name_Kitchen, 1), FlowClassification.Extract, 27),
                Target(adjacencyCluster, Name(name_Bathroom, 1), FlowClassification.Extract, 13)]);

            DesignAirFlowRoundCandidate candidate_Reversed = Round(adjacencyCluster, [
                Target(adjacencyCluster, Name(name_Bathroom, 1), FlowClassification.Extract, 13),
                Target(adjacencyCluster, Name(name_Kitchen, 1), FlowClassification.Extract, 27)]);

            Assert.True(candidate_Forward.IsAccepted);
            Assert.True(candidate_Reversed.IsAccepted);

            Assert.Equal(Designs(candidate_Forward.AdjacencyCluster), Designs(candidate_Reversed.AdjacencyCluster));

            //And the reports agree too, not just the models - a round that answered the same design through
            //a differently ordered set of adjustments would still be order dependent to anyone reading it.
            Assert.Equal(
                candidate_Forward.TargetedAdjustments.ConvertAll(x => x.ToString()),
                candidate_Reversed.TargetedAdjustments.ConvertAll(x => x.ToString()));

            Assert.Equal(
                candidate_Forward.DerivedAdjustments.ConvertAll(x => x.ToString()),
                candidate_Reversed.DerivedAdjustments.ConvertAll(x => x.ToString()));
        }

        /// <summary>
        /// <b>A, and the defect it is really about.</b> Under cooking priority a derived extract change goes
        /// to the local kitchen extract - so a round that also targets that kitchen must not let the
        /// consequence of a supply change overwrite the figure somebody chose for it.
        /// <code>
        /// targeted: Bedroom 1 supply  30 -> 40   (+10)
        /// targeted: Kitchen 1 extract 22 -> 24   (+2)   &lt;- deliberate, and cooking priority's favourite room
        /// derived:  Bathroom 1 extract 8 -> 16   (+8)   &lt;- the only extract room nobody targeted
        /// </code>
        /// <para>
        /// Applied one at a time the kitchen would have absorbed the whole 10 l/s and ended at 32, or at 24
        /// having discarded it, depending purely on which target went first.
        /// </para>
        /// </summary>
        [Fact]
        public void ADeliberateTarget_IsNeverOverwrittenByTheBalancingAllocation()
        {
            AdjacencyCluster adjacencyCluster = Fixture();

            DesignAirFlowRoundCandidate candidate = Round(adjacencyCluster, [
                Target(adjacencyCluster, Name(name_Bedroom, 1), FlowClassification.Supply, 40),
                Target(adjacencyCluster, Name(name_Kitchen, 1), FlowClassification.Extract, 24)]);

            Assert.True(candidate.IsAccepted);

            //Exactly the figure that was asked for, not the one cooking priority would have preferred.
            Assert.Equal(24, Design(candidate.AdjacencyCluster, Name(name_Kitchen, 1), FlowClassification.Extract), 6);

            //The whole balancing consequence went to the one extract room nobody targeted.
            DesignAirFlowAdjustment designAirFlowAdjustment = Assert.Single(candidate.DerivedAdjustments);

            Assert.Equal(Name(name_Bathroom, 1), designAirFlowAdjustment.SpaceName);
            Assert.True(designAirFlowAdjustment.IsDerived);
            Assert.Equal(16, designAirFlowAdjustment.After_Lps, 6);
        }

        // ---- B. Targeted versus derived --------------------------------------------------------------------

        /// <summary>
        /// <b>B.</b> Two deliberate extract targets derive exactly one supply consequence, and the two are
        /// reported apart. The derived change is the COMBINED +10, computed once - not two separate +5s.
        /// </summary>
        [Fact]
        public void TargetedAndDerivedAdjustments_AreReportedSeparatelyAndDerivedOnce()
        {
            AdjacencyCluster adjacencyCluster = Fixture();

            DesignAirFlowRoundCandidate candidate = Round(adjacencyCluster, [
                Target(adjacencyCluster, Name(name_Kitchen, 1), FlowClassification.Extract, 27),
                Target(adjacencyCluster, Name(name_Bathroom, 1), FlowClassification.Extract, 13)]);

            Assert.True(candidate.IsAccepted);

            Assert.Equal(2, candidate.TargetedAdjustments.Count);
            Assert.All(candidate.TargetedAdjustments, x => Assert.False(x.IsDerived));

            DesignAirFlowAdjustment designAirFlowAdjustment = Assert.Single(candidate.DerivedAdjustments);

            Assert.Equal(Name(name_Bedroom, 1), designAirFlowAdjustment.SpaceName);
            Assert.Equal(FlowClassification.Supply, designAirFlowAdjustment.FlowClassification);
            Assert.True(designAirFlowAdjustment.IsDerived);
            Assert.Equal(30, designAirFlowAdjustment.Before_Lps, 6);
            Assert.Equal(40, designAirFlowAdjustment.After_Lps, 6);
        }

        // ---- C. Approved Document F floors ------------------------------------------------------------------

        /// <summary>
        /// <b>C.</b> Not one Approved Document F requirement moves. The round writes design airflow and
        /// nothing else - the requirement is the floor it is measured against, never a thing it adjusts.
        /// </summary>
        [Fact]
        public void ARound_LeavesEveryPartFRequirementUnchanged()
        {
            AdjacencyCluster adjacencyCluster = Fixture();

            Dictionary<string, double> requirements_Before = Requirements(adjacencyCluster);

            DesignAirFlowRoundCandidate candidate = Round(adjacencyCluster, [
                Target(adjacencyCluster, Name(name_Kitchen, 1), FlowClassification.Extract, 27),
                Target(adjacencyCluster, Name(name_Bathroom, 1), FlowClassification.Extract, 13)]);

            Assert.True(candidate.IsAccepted);

            Assert.Equal(requirements_Before, Requirements(candidate.AdjacencyCluster));
            Assert.Equal(requirements_Before, Requirements(adjacencyCluster));
        }

        /// <summary>
        /// <b>C.</b> A target below its own room's floor refuses the whole round, and writes nothing -
        /// including the other target, which was perfectly legal on its own.
        /// </summary>
        [Fact]
        public void ATargetBelowItsPartFFloor_RefusesTheWholeRound()
        {
            AdjacencyCluster adjacencyCluster = Fixture();

            Dictionary<string, double> designs_Before = Designs(adjacencyCluster);

            DesignAirFlowRoundCandidate candidate = Round(adjacencyCluster, [
                Target(adjacencyCluster, Name(name_Kitchen, 1), FlowClassification.Extract, 27),
                Target(adjacencyCluster, Name(name_Bathroom, 1), FlowClassification.Extract, 4)]);

            Assert.False(candidate.IsAccepted);
            Assert.Null(candidate.AdjacencyCluster);
            Assert.NotEmpty(candidate.Refusals);

            Assert.Equal(designs_Before, Designs(adjacencyCluster));
        }

        // ---- D. Dwelling isolation ---------------------------------------------------------------------------

        /// <summary>
        /// <b>D.</b> A round over one flat leaves the other one exactly as it was - every design airflow and
        /// both duties.
        /// </summary>
        [Fact]
        public void ARoundInOneDwelling_LeavesAnotherDwellingUntouched()
        {
            AdjacencyCluster adjacencyCluster = Fixture();

            DesignAirFlowRoundCandidate candidate = Round(adjacencyCluster, [
                Target(adjacencyCluster, Name(name_Kitchen, 1), FlowClassification.Extract, 27),
                Target(adjacencyCluster, Name(name_Bathroom, 1), FlowClassification.Extract, 13)]);

            Assert.True(candidate.IsAccepted);

            //One dwelling round, for one system - the other flat is not even reported on.
            DwellingDesignAirFlowRound dwellingDesignAirFlowRound = Assert.Single(candidate.DwellingRounds);

            Assert.Contains(Name("Flat", 1), dwellingDesignAirFlowRound.VentilationSystem.FullName);

            foreach (string name in new[] { Name(name_Bedroom, 2), Name(name_Kitchen, 2), Name(name_Bathroom, 2) })
            {
                Assert.Equal(Design(adjacencyCluster, name, FlowClassification.Supply), Design(candidate.AdjacencyCluster, name, FlowClassification.Supply), 6);
                Assert.Equal(Design(adjacencyCluster, name, FlowClassification.Extract), Design(candidate.AdjacencyCluster, name, FlowClassification.Extract), 6);
            }
        }

        /// <summary>
        /// <b>D.</b> Targets in two flats are one round, resolved to their own serving systems and rebalanced
        /// within them - no consequence of one dwelling reaches the other.
        /// </summary>
        [Fact]
        public void TargetsInTwoDwellings_AreEachRebalancedWithinTheirOwnSystem()
        {
            AdjacencyCluster adjacencyCluster = Fixture();

            DesignAirFlowRoundCandidate candidate = Round(adjacencyCluster, [
                Target(adjacencyCluster, Name(name_Kitchen, 1), FlowClassification.Extract, 27),
                Target(adjacencyCluster, Name(name_Kitchen, 2), FlowClassification.Extract, 25)]);

            Assert.True(candidate.IsAccepted);
            Assert.Equal(2, candidate.DwellingRounds.Count);

            Assert.Equal(35, Design(candidate.AdjacencyCluster, Name(name_Bedroom, 1), FlowClassification.Supply), 6);
            Assert.Equal(33, Design(candidate.AdjacencyCluster, Name(name_Bedroom, 2), FlowClassification.Supply), 6);

            //Untargeted wet rooms are untouched: the balancing consequence of an extract target belongs on
            //the supply side, and there is exactly one supply room in each flat.
            Assert.Equal(8, Design(candidate.AdjacencyCluster, Name(name_Bathroom, 1), FlowClassification.Extract), 6);
            Assert.Equal(8, Design(candidate.AdjacencyCluster, Name(name_Bathroom, 2), FlowClassification.Extract), 6);
        }

        // ---- E. Balance ----------------------------------------------------------------------------------------

        /// <summary>
        /// <b>E.</b> Every accepted round leaves every dwelling it touched balanced, and says so through the
        /// duties it reports.
        /// </summary>
        [Fact]
        public void EveryAcceptedRound_LeavesEveryDwellingBalanced()
        {
            AdjacencyCluster adjacencyCluster = Fixture();

            DesignAirFlowRoundCandidate candidate = Round(adjacencyCluster, [
                Target(adjacencyCluster, Name(name_Kitchen, 1), FlowClassification.Extract, 27),
                Target(adjacencyCluster, Name(name_Bathroom, 1), FlowClassification.Extract, 13),
                Target(adjacencyCluster, Name(name_Bedroom, 2), FlowClassification.Supply, 36)]);

            Assert.True(candidate.IsAccepted);

            foreach (DwellingDesignAirFlowRound dwellingDesignAirFlowRound in candidate.DwellingRounds)
            {
                Assert.Equal(dwellingDesignAirFlowRound.SupplyDuty_After_Lps, dwellingDesignAirFlowRound.ExtractDuty_After_Lps, 6);

                candidate.AdjacencyCluster.VentilationSystemDesignDuty(dwellingDesignAirFlowRound.VentilationSystem, out double supplyDuty_Lps, out double extractDuty_Lps);

                Assert.Equal(supplyDuty_Lps, extractDuty_Lps, 6);
                Assert.Equal(dwellingDesignAirFlowRound.SupplyDuty_After_Lps, supplyDuty_Lps, 6);
            }
        }

        // ---- F. Capacity ---------------------------------------------------------------------------------------

        /// <summary>
        /// <b>F.</b> A combined round beyond the selected unit's rating is not adopted - even though each
        /// target on its own would fit. The check is against the recalculated system duty, never a room.
        /// <code>
        /// selected  MVHR-35, 35/35 l/s          dwelling at 30/30
        /// kitchen  +5  -> 35/35   would fit
        /// bathroom +5  -> 35/35   would fit
        /// both     +10 -> 40/40   REFUSED, and nothing is reselected
        /// </code>
        /// </summary>
        [Fact]
        public void ACombinedRoundBeyondSelectedCapacity_IsNotAdopted()
        {
            AdjacencyCluster adjacencyCluster = Fixture(out List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors);

            //Each target on its own is within the rating, so the refusal below can only be the combination.
            Assert.True(Round(adjacencyCluster, [Target(adjacencyCluster, Name(name_Kitchen, 1), FlowClassification.Extract, 27)], ventilationUnitCapacityDescriptors).IsAccepted);
            Assert.True(Round(adjacencyCluster, [Target(adjacencyCluster, Name(name_Bathroom, 1), FlowClassification.Extract, 13)], ventilationUnitCapacityDescriptors).IsAccepted);

            DesignAirFlowRoundCandidate candidate = Round(adjacencyCluster, [
                Target(adjacencyCluster, Name(name_Kitchen, 1), FlowClassification.Extract, 27),
                Target(adjacencyCluster, Name(name_Bathroom, 1), FlowClassification.Extract, 13)], ventilationUnitCapacityDescriptors);

            Assert.False(candidate.IsAccepted);
            Assert.Null(candidate.AdjacencyCluster);

            DwellingDesignAirFlowRound dwellingDesignAirFlowRound = Assert.Single(candidate.VentilationUnitRefusals);

            Assert.Equal(VentilationUnitSelectionOutcome.Refused, dwellingDesignAirFlowRound.VentilationUnitSelectionOutcome);

            //The report an automatic optimiser stops on: what it would have designed, what the unit is rated
            //at, and how much of it is left.
            Assert.Equal(40, dwellingDesignAirFlowRound.SupplyDuty_After_Lps, 6);
            Assert.Equal(35, dwellingDesignAirFlowRound.VentilationUnitCapacityDescriptor.MaximumSupplyFlowRate_Lps, 6);
            Assert.Equal(-5, dwellingDesignAirFlowRound.SupplyHeadroom_Lps, 6);

            //And the product is exactly the one that was selected: an optimisation round never buys a bigger
            //unit to make its own proposal fit.
            Assert.Equal("MVHR-35", dwellingDesignAirFlowRound.VentilationUnitReference.Model);
            Assert.Equal("MVHR-35", SelectedModel(adjacencyCluster, Name("AHU", 1)));
        }

        /// <summary>
        /// <b>F.</b> One dwelling hitting its unit's rating refuses the whole round, including the dwelling
        /// that had room to spare. A round is one transaction; adopting half of it would leave a design the
        /// caller's own policy never approved, and the next round would then be computed from it.
        /// </summary>
        [Fact]
        public void OneDwellingBeyondCapacity_RefusesTheWholeRoundIncludingTheOtherDwelling()
        {
            AdjacencyCluster adjacencyCluster = Fixture(out List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors);

            Dictionary<string, double> designs_Before = Designs(adjacencyCluster);

            DesignAirFlowRoundCandidate candidate = Round(adjacencyCluster, [
                Target(adjacencyCluster, Name(name_Kitchen, 1), FlowClassification.Extract, 27),
                Target(adjacencyCluster, Name(name_Bathroom, 1), FlowClassification.Extract, 13),
                Target(adjacencyCluster, Name(name_Kitchen, 2), FlowClassification.Extract, 24)], ventilationUnitCapacityDescriptors);

            Assert.False(candidate.IsAccepted);
            Assert.Null(candidate.AdjacencyCluster);

            //Flat 2 was fine on its own terms and is reported as accepted - but there is still no model.
            Assert.Equal(2, candidate.DwellingRounds.Count);
            Assert.Single(candidate.VentilationUnitRefusals);

            Assert.Equal(designs_Before, Designs(adjacencyCluster));
        }

        /// <summary>
        /// <b>F, for a unit serving more than one system.</b> A unit's duty is the sum over every system it
        /// supplies, so it can only be judged once the WHOLE round is written. Here one system rises 5 l/s
        /// and another falls 5 l/s on the same unit, which is sitting exactly on its rating: the round
        /// leaves the unit exactly where it found it and must be accepted.
        /// <code>
        /// MVHR-S rated 35/35, serving Flat A at 10/10 and Flat B at 25/25 - so 35/35, on the nose
        /// targeted: Bedroom A supply 10 -> 15   derived: Kitchen A extract 10 -> 15
        /// targeted: Bedroom B supply 25 -> 20   derived: Kitchen B extract 25 -> 20
        /// unit after: 35/35 - unchanged
        /// </code>
        /// <para>
        /// Checked per dwelling as the round was written, the system processed first would have been judged
        /// against a duty that included the other system's OLD design - 40/35 one way round, and a stale
        /// 30/35 the other - so this refused a valid round or reported headroom for a state that never
        /// existed, depending only on which guid sorted first. Both are asserted against below.
        /// </para>
        /// </summary>
        [Fact]
        public void ASharedVentilationUnit_IsJudgedOnTheWholeRoundRatherThanPartOfIt()
        {
            AdjacencyCluster adjacencyCluster = SharedUnitFixture(out List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors);

            DesignAirFlowRoundCandidate candidate = Round(adjacencyCluster, [
                Target(adjacencyCluster, "Bedroom A", FlowClassification.Supply, 15),
                Target(adjacencyCluster, "Bedroom B", FlowClassification.Supply, 20)], ventilationUnitCapacityDescriptors);

            Assert.True(candidate.IsAccepted, string.Join(" ", candidate.Refusals));

            Assert.Equal(2, candidate.DwellingRounds.Count);

            //The unit ends exactly where it started, which is exactly on its rating.
            AirHandlingUnit airHandlingUnit = Assert.Single(candidate.AdjacencyCluster.GetObjects<AirHandlingUnit>());

            Assert.True(candidate.AdjacencyCluster.AirHandlingUnitDesignDuty(airHandlingUnit, out double supplyDuty_Lps, out double extractDuty_Lps));

            Assert.Equal(35, supplyDuty_Lps, 6);
            Assert.Equal(35, extractDuty_Lps, 6);

            //One verdict per unit, shared by every dwelling on it - and measured against the design the
            //round actually produces, not an intermediate one.
            foreach (DwellingDesignAirFlowRound dwellingDesignAirFlowRound in candidate.DwellingRounds)
            {
                Assert.Equal(VentilationUnitSelectionOutcome.Kept, dwellingDesignAirFlowRound.VentilationUnitSelectionOutcome);
                Assert.Equal(0, dwellingDesignAirFlowRound.SupplyHeadroom_Lps, 6);
                Assert.Equal(0, dwellingDesignAirFlowRound.ExtractHeadroom_Lps, 6);
            }
        }

        /// <summary>
        /// <b>F, again for a shared unit.</b> Where the completed round really does outgrow the unit, every
        /// dwelling on it carries the refusal - because every one of them is subject to it, and a caller
        /// retrying without the dwellings that hit capacity has to drop all of them together or meet the
        /// same unit again on the next attempt.
        /// </summary>
        [Fact]
        public void ASharedVentilationUnitThatIsOutgrown_RefusesEveryDwellingOnIt()
        {
            AdjacencyCluster adjacencyCluster = SharedUnitFixture(out List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors);

            Dictionary<string, double> designs_Before = Designs(adjacencyCluster);

            //Both systems rise, so the shared unit is asked for 45/45 against its 35/35 rating.
            DesignAirFlowRoundCandidate candidate = Round(adjacencyCluster, [
                Target(adjacencyCluster, "Bedroom A", FlowClassification.Supply, 15),
                Target(adjacencyCluster, "Bedroom B", FlowClassification.Supply, 30)], ventilationUnitCapacityDescriptors);

            Assert.False(candidate.IsAccepted);
            Assert.Null(candidate.AdjacencyCluster);

            Assert.Equal(2, candidate.VentilationUnitRefusals.Count);

            //The duty reported is the UNIT's, not either dwelling's.
            Assert.Contains(candidate.Refusals, x => x.Contains("moving 45"));

            Assert.Equal(designs_Before, Designs(adjacencyCluster));
        }

        // ---- G. A partial clamp is not a step ---------------------------------------------------------------------

        /// <summary>
        /// <b>G.</b> The generic resolver clamps, and it is right to. A fixed-step round must not adopt that
        /// clamp as though it were the step.
        /// <code>
        /// ResolveTargetedDesignAirFlow  kitchen -> 32   achieves 27, IsRequestSatisfied false   (a useful answer)
        /// EvaluateTargetedDesignAirFlows kitchen -> 32   REFUSED, no model                      (the round's answer)
        /// </code>
        /// <para>
        /// If the round settled for 27 the optimiser would simulate a design that is not the one its policy
        /// says it is testing, and every later reading of that iteration would be wrong about what was tried.
        /// </para>
        /// </summary>
        [Fact]
        public void APartialResolverClamp_IsNotAdoptedAsAFullRound()
        {
            AdjacencyCluster adjacencyCluster = Fixture(out List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors);

            Space space = Space(adjacencyCluster, Name(name_Kitchen, 1));

            //The resolver: accepted, clamped, and honest about being clamped.
            DwellingDesignAirFlowResolution resolution = adjacencyCluster.ResolveTargetedDesignAirFlow(space, FlowClassification.Extract, 32, PartFExtractAllocationStrategy.MinimumFirstCookingPriority, tolerance_Lps, ventilationUnitCapacityDescriptors);

            Assert.True(resolution.IsAccepted);
            Assert.False(resolution.IsRequestSatisfied);
            //Resolved to within the tolerance the search was given, which is what "clamped" means here -
            //not to an exact figure the search never claimed.
            Assert.Equal(27, resolution.Achieved_Lps, 2);

            //The round: the same request, refused outright, with nothing to adopt.
            DesignAirFlowRoundCandidate candidate = Round(adjacencyCluster, [Target(adjacencyCluster, Name(name_Kitchen, 1), FlowClassification.Extract, 32)], ventilationUnitCapacityDescriptors);

            Assert.False(candidate.IsAccepted);
            Assert.Null(candidate.AdjacencyCluster);
            Assert.Single(candidate.VentilationUnitRefusals);
        }

        // ---- H. The last valid model survives ---------------------------------------------------------------------

        /// <summary>
        /// <b>H.</b> After a round refused at capacity, the caller's model is byte-for-byte the design it
        /// was: every room, both sides, both flats, every requirement, and the selected product. That is
        /// what "preserve the last valid model" has to mean for an optimiser that stops at capacity.
        /// </summary>
        [Fact]
        public void TheLastValidModel_SurvivesACapacityRefusalExactly()
        {
            AdjacencyCluster adjacencyCluster = Fixture(out List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors);

            Dictionary<string, double> designs_Before = Designs(adjacencyCluster);
            Dictionary<string, double> requirements_Before = Requirements(adjacencyCluster);
            string model_Before = SelectedModel(adjacencyCluster, Name("AHU", 1));

            DesignAirFlowRoundCandidate candidate = Round(adjacencyCluster, [
                Target(adjacencyCluster, Name(name_Kitchen, 1), FlowClassification.Extract, 27),
                Target(adjacencyCluster, Name(name_Bathroom, 1), FlowClassification.Extract, 13)], ventilationUnitCapacityDescriptors);

            Assert.False(candidate.IsAccepted);

            Assert.Equal(designs_Before, Designs(adjacencyCluster));
            Assert.Equal(requirements_Before, Requirements(adjacencyCluster));
            Assert.Equal(model_Before, SelectedModel(adjacencyCluster, Name("AHU", 1)));

            //And the same is true of an ACCEPTED round: the answer is a copy, and adopting it is a
            //deliberate act by the caller.
            DesignAirFlowRoundCandidate candidate_Accepted = Round(adjacencyCluster, [Target(adjacencyCluster, Name(name_Kitchen, 1), FlowClassification.Extract, 27)], ventilationUnitCapacityDescriptors);

            Assert.True(candidate_Accepted.IsAccepted);
            Assert.Equal(designs_Before, Designs(adjacencyCluster));
        }

        // ---- I. No appropriate terminal ------------------------------------------------------------------------------

        /// <summary>
        /// <b>I.</b> A room with no design terminal on the side being asked for is reported as not
        /// optimisable, with the reason - and the rest of the round goes on. No terminal is invented: that
        /// would size a duty the Approved Document F assessment never asked for.
        /// </summary>
        [Fact]
        public void ATargetWithNoDesignTerminal_IsReportedAsNotOptimisableAndDropped()
        {
            AdjacencyCluster adjacencyCluster = Fixture();

            //A wet room has an extract terminal and no supply one.
            DesignAirFlowRoundCandidate candidate = Round(adjacencyCluster, [
                Target(adjacencyCluster, Name(name_Bathroom, 1), FlowClassification.Supply, 5),
                Target(adjacencyCluster, Name(name_Kitchen, 1), FlowClassification.Extract, 27)]);

            Assert.True(candidate.IsAccepted);

            DesignAirFlowTargetRefusal designAirFlowTargetRefusal = Assert.Single(candidate.TargetRefusals);

            Assert.Equal(Name(name_Bathroom, 1), designAirFlowTargetRefusal.DesignAirFlowTarget.SpaceName);
            Assert.Contains("no design supply terminal", designAirFlowTargetRefusal.Reason, StringComparison.OrdinalIgnoreCase);

            //The round is exactly the one remaining target, and no supply terminal was created for the
            //bathroom on the way.
            Assert.Single(candidate.TargetedAdjustments);
            Assert.Equal(0, Design(candidate.AdjacencyCluster, Name(name_Bathroom, 1), FlowClassification.Supply), 6);
        }

        /// <summary>
        /// <b>I.</b> A round in which <i>every</i> target is not optimisable is refused - there is then
        /// nothing to evaluate, and returning an unchanged model as a success would tell an optimiser it had
        /// made progress.
        /// </summary>
        [Fact]
        public void ARoundWhereNoTargetCanBeTaken_IsRefused()
        {
            AdjacencyCluster adjacencyCluster = Fixture();

            DesignAirFlowRoundCandidate candidate = Round(adjacencyCluster, [
                Target(adjacencyCluster, Name(name_Bathroom, 1), FlowClassification.Supply, 5),
                Target(adjacencyCluster, Name(name_Kitchen, 1), FlowClassification.Supply, 5)]);

            Assert.False(candidate.IsAccepted);
            Assert.Null(candidate.AdjacencyCluster);
            Assert.Equal(2, candidate.TargetRefusals.Count);
            Assert.NotEmpty(candidate.Refusals);
        }

        /// <summary>
        /// <b>I, and A.</b> The refusals a round reports are part of its report, so they obey the same
        /// order independence the adjustments do: the same unoptimisable set, handed over reversed, reads
        /// identically.
        /// </summary>
        [Fact]
        public void TargetRefusals_AreOrderIndependentToo()
        {
            AdjacencyCluster adjacencyCluster = Fixture();

            DesignAirFlowRoundCandidate candidate_Forward = Round(adjacencyCluster, [
                Target(adjacencyCluster, Name(name_Bathroom, 1), FlowClassification.Supply, 5),
                Target(adjacencyCluster, Name(name_Kitchen, 1), FlowClassification.Supply, 5),
                Target(adjacencyCluster, Name(name_Bathroom, 2), FlowClassification.Supply, 5)]);

            DesignAirFlowRoundCandidate candidate_Reversed = Round(adjacencyCluster, [
                Target(adjacencyCluster, Name(name_Bathroom, 2), FlowClassification.Supply, 5),
                Target(adjacencyCluster, Name(name_Kitchen, 1), FlowClassification.Supply, 5),
                Target(adjacencyCluster, Name(name_Bathroom, 1), FlowClassification.Supply, 5)]);

            Assert.Equal(3, candidate_Forward.TargetRefusals.Count);

            Assert.Equal(
                candidate_Forward.TargetRefusals.ConvertAll(x => x.ToString()),
                candidate_Reversed.TargetRefusals.ConvertAll(x => x.ToString()));
        }

        /// <summary>
        /// <b>I, taken to the last tie.</b> Two dropped targets can name the SAME room and direction and
        /// differ only in the figure - a bathroom with no supply terminal asked for two different supplies
        /// is dropped twice with the same reason, and the report prints the rate. Room, direction and
        /// reason alone therefore cannot order the pair; the requested rate is in the sort key, so even
        /// they read identically handed over either way round.
        /// </summary>
        [Fact]
        public void TargetRefusals_OnTheSameRoomAndDirection_AreOrderIndependentToo()
        {
            AdjacencyCluster adjacencyCluster = Fixture();

            DesignAirFlowRoundCandidate candidate_Forward = Round(adjacencyCluster, [
                Target(adjacencyCluster, Name(name_Bathroom, 1), FlowClassification.Supply, 5),
                Target(adjacencyCluster, Name(name_Bathroom, 1), FlowClassification.Supply, 9)]);

            DesignAirFlowRoundCandidate candidate_Reversed = Round(adjacencyCluster, [
                Target(adjacencyCluster, Name(name_Bathroom, 1), FlowClassification.Supply, 9),
                Target(adjacencyCluster, Name(name_Bathroom, 1), FlowClassification.Supply, 5)]);

            //Dropped, both of them - a bathroom has no supply terminal, and a dropped target never reaches
            //the duplicate check, which exists for targets the round could otherwise have taken. With
            //nothing left to take, the round itself is refused.
            Assert.False(candidate_Forward.IsAccepted);
            Assert.Equal(2, candidate_Forward.TargetRefusals.Count);

            //The pair really is distinguishable in the report - this is the case a key without the rate
            //leaves unordered.
            Assert.NotEqual(candidate_Forward.TargetRefusals[0].ToString(), candidate_Forward.TargetRefusals[1].ToString());

            Assert.Equal(
                candidate_Forward.TargetRefusals.ConvertAll(x => x.ToString()),
                candidate_Reversed.TargetRefusals.ConvertAll(x => x.ToString()));
        }

        // ---- J. Mixed supply and extract targets --------------------------------------------------------------------

        /// <summary>
        /// <b>J.</b> A supply target and an extract target of the same size in one dwelling balance each
        /// other directly - the round derives <b>nothing</b> rather than inventing a consequence and then
        /// cancelling it.
        /// <code>
        /// targeted: Bedroom 1  supply  30 -> 35   (+5)
        /// targeted: Bathroom 1 extract  8 -> 13   (+5)
        /// derived:  none                          duty 35/35
        /// </code>
        /// </summary>
        [Fact]
        public void MixedSupplyAndExtractTargetsThatBalance_DeriveNothing()
        {
            AdjacencyCluster adjacencyCluster = Fixture();

            DesignAirFlowRoundCandidate candidate = Round(adjacencyCluster, [
                Target(adjacencyCluster, Name(name_Bedroom, 1), FlowClassification.Supply, 35),
                Target(adjacencyCluster, Name(name_Bathroom, 1), FlowClassification.Extract, 13)]);

            Assert.True(candidate.IsAccepted);

            Assert.Equal(2, candidate.TargetedAdjustments.Count);
            Assert.Empty(candidate.DerivedAdjustments);

            //The kitchen is untouched - cooking priority never ran, because there was nothing to allocate.
            Assert.Equal(22, Design(candidate.AdjacencyCluster, Name(name_Kitchen, 1), FlowClassification.Extract), 6);

            DwellingDesignAirFlowRound dwellingDesignAirFlowRound = Assert.Single(candidate.DwellingRounds);

            Assert.Equal(35, dwellingDesignAirFlowRound.SupplyDuty_After_Lps, 6);
            Assert.Equal(35, dwellingDesignAirFlowRound.ExtractDuty_After_Lps, 6);
        }

        /// <summary>
        /// <b>J.</b> Where mixed targets do NOT balance each other, the dwelling moves to the larger of the
        /// two - so neither deliberate figure is written back down - and the difference is made up on the
        /// side that asked for less, out of rooms nobody targeted.
        /// <code>
        /// targeted: Bedroom 1  supply  30 -> 34   (+4)
        /// targeted: Bathroom 1 extract  8 -> 18   (+10)   &lt;- the larger
        /// derived:  Bedroom 1 is a target, so the balancing +6 has nowhere on the supply side to go
        /// </code>
        /// <para>
        /// Refused, and the message says exactly that rather than quietly moving the bedroom to 40.
        /// </para>
        /// </summary>
        [Fact]
        public void MixedTargetsWithNoUntargetedRoomToBalanceOver_RefuseRatherThanOverwriteATarget()
        {
            AdjacencyCluster adjacencyCluster = Fixture();

            DesignAirFlowRoundCandidate candidate = Round(adjacencyCluster, [
                Target(adjacencyCluster, Name(name_Bedroom, 1), FlowClassification.Supply, 34),
                Target(adjacencyCluster, Name(name_Bathroom, 1), FlowClassification.Extract, 18)]);

            Assert.False(candidate.IsAccepted);
            Assert.Null(candidate.AdjacencyCluster);
            Assert.Contains(candidate.Refusals, x => x.Contains("is itself a deliberate target"));
        }

        // ---- K. Only the rooms that were named are targets ---------------------------------------------------------

        /// <summary>
        /// <b>K.</b> No room becomes a target by being moved. Every adjustment for a room the caller did not
        /// name is marked derived, and every room the caller did name is marked targeted - which is what
        /// lets a report say afterwards which figures were engineering decisions.
        /// </summary>
        [Fact]
        public void OnlyTheRoomsTheCallerNamed_AreTargets()
        {
            AdjacencyCluster adjacencyCluster = Fixture();

            List<string> names_Targeted = [Name(name_Kitchen, 1), Name(name_Bathroom, 1)];

            DesignAirFlowRoundCandidate candidate = Round(adjacencyCluster, [
                Target(adjacencyCluster, names_Targeted[0], FlowClassification.Extract, 27),
                Target(adjacencyCluster, names_Targeted[1], FlowClassification.Extract, 13)]);

            Assert.True(candidate.IsAccepted);

            foreach (DesignAirFlowAdjustment designAirFlowAdjustment in candidate.TargetedAdjustments)
            {
                Assert.Contains(designAirFlowAdjustment.SpaceName, names_Targeted);
            }

            foreach (DesignAirFlowAdjustment designAirFlowAdjustment in candidate.DerivedAdjustments)
            {
                Assert.DoesNotContain(designAirFlowAdjustment.SpaceName, names_Targeted);
            }
        }

        // ---- The transaction's own preconditions -------------------------------------------------------------------

        /// <summary>
        /// The same room and direction asked for twice is a caller that has not decided, and guessing
        /// between the two figures is exactly the silent behaviour a round exists to remove.
        /// </summary>
        [Fact]
        public void TheSameRoomAndDirectionTargetedTwice_RefusesTheRound()
        {
            AdjacencyCluster adjacencyCluster = Fixture();

            DesignAirFlowRoundCandidate candidate = Round(adjacencyCluster, [
                Target(adjacencyCluster, Name(name_Kitchen, 1), FlowClassification.Extract, 27),
                Target(adjacencyCluster, Name(name_Kitchen, 1), FlowClassification.Extract, 30)]);

            Assert.False(candidate.IsAccepted);
            Assert.Null(candidate.AdjacencyCluster);
            Assert.Contains(candidate.Refusals, x => x.Contains("more than one deliberate"));
        }

        /// <summary>
        /// A request that is not a design airflow at all refuses the round - it is not merely dropped as
        /// "not optimisable" while the rest of the batch is applied.
        /// <para>
        /// A room with no terminal to move is an engineering fact about the building and the round goes on
        /// without it. A NaN rate, a direction that is neither supply nor extract, or a room that is not in
        /// the model are none of those things: they are a caller asking for something incoherent, and
        /// quietly doing the rest would simulate a design missing one of its deliberate targets while
        /// reporting the round a success.
        /// </para>
        /// </summary>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(-1)]
        public void AMalformedRate_RefusesTheRoundRatherThanBeingDropped(double designFlowRate_Lps)
        {
            AdjacencyCluster adjacencyCluster = Fixture();

            Dictionary<string, double> designs_Before = Designs(adjacencyCluster);

            DesignAirFlowRoundCandidate candidate = Round(adjacencyCluster, [
                Target(adjacencyCluster, Name(name_Kitchen, 1), FlowClassification.Extract, 27),
                Target(adjacencyCluster, Name(name_Bathroom, 1), FlowClassification.Extract, designFlowRate_Lps)]);

            Assert.False(candidate.IsAccepted);
            Assert.Null(candidate.AdjacencyCluster);

            //Refused, not dropped.
            Assert.Empty(candidate.TargetRefusals);
            Assert.Contains(candidate.Refusals, x => x.Contains("is not a design airflow"));

            Assert.Equal(designs_Before, Designs(adjacencyCluster));
        }

        /// <summary>A direction that is neither supply nor extract is not a design airflow either.</summary>
        [Fact]
        public void AMalformedDirection_RefusesTheRound()
        {
            AdjacencyCluster adjacencyCluster = Fixture();

            DesignAirFlowRoundCandidate candidate = Round(adjacencyCluster, [
                Target(adjacencyCluster, Name(name_Kitchen, 1), FlowClassification.Extract, 27),
                Target(adjacencyCluster, Name(name_Bathroom, 1), FlowClassification.Undefined, 13)]);

            Assert.False(candidate.IsAccepted);
            Assert.Empty(candidate.TargetRefusals);
            Assert.Contains(candidate.Refusals, x => x.Contains("supply or extract"));
        }

        /// <summary>
        /// And a room with no lever still only drops itself - the distinction this whole classification
        /// exists to make.
        /// </summary>
        [Fact]
        public void ARoomWithNoLever_StillOnlyDropsItself()
        {
            AdjacencyCluster adjacencyCluster = Fixture();

            DesignAirFlowRoundCandidate candidate = Round(adjacencyCluster, [
                Target(adjacencyCluster, Name(name_Kitchen, 1), FlowClassification.Extract, 27),
                Target(adjacencyCluster, Name(name_Bathroom, 1), FlowClassification.Supply, 5)]);

            Assert.True(candidate.IsAccepted);
            Assert.Single(candidate.TargetRefusals);
            Assert.Single(candidate.TargetedAdjustments);
        }

        /// <summary>
        /// A duplicate does not stop the round examining the rest of what it was given. The round is
        /// refused either way, but <b>what it says about why</b> has to be the same whichever way round the
        /// same set was handed over - an earlier revision returned on the first duplicate, so a target
        /// sitting after it went unexamined and unreported.
        /// </summary>
        [Fact]
        public void ADuplicateTarget_DoesNotHideTheOtherTargetsReasons()
        {
            AdjacencyCluster adjacencyCluster = Fixture();

            DesignAirFlowRoundCandidate candidate_Forward = Round(adjacencyCluster, [
                Target(adjacencyCluster, Name(name_Bathroom, 1), FlowClassification.Supply, 5),
                Target(adjacencyCluster, Name(name_Kitchen, 1), FlowClassification.Extract, 27),
                Target(adjacencyCluster, Name(name_Kitchen, 1), FlowClassification.Extract, 30),
                Target(adjacencyCluster, Name(name_Bathroom, 2), FlowClassification.Supply, 5)]);

            DesignAirFlowRoundCandidate candidate_Reversed = Round(adjacencyCluster, [
                Target(adjacencyCluster, Name(name_Bathroom, 2), FlowClassification.Supply, 5),
                Target(adjacencyCluster, Name(name_Kitchen, 1), FlowClassification.Extract, 30),
                Target(adjacencyCluster, Name(name_Kitchen, 1), FlowClassification.Extract, 27),
                Target(adjacencyCluster, Name(name_Bathroom, 1), FlowClassification.Supply, 5)]);

            Assert.False(candidate_Forward.IsAccepted);
            Assert.False(candidate_Reversed.IsAccepted);

            //Both wet rooms were examined and both reasons reported, whichever side of the duplicate they
            //were listed on.
            Assert.Equal(2, candidate_Forward.TargetRefusals.Count);

            Assert.Equal(
                candidate_Forward.TargetRefusals.ConvertAll(x => x.ToString()),
                candidate_Reversed.TargetRefusals.ConvertAll(x => x.ToString()));

            //And the duplicate is stated once, identically.
            Assert.Equal(candidate_Forward.Refusals, candidate_Reversed.Refusals);
            Assert.Contains(candidate_Forward.Refusals, x => x.Contains("more than one deliberate"));
        }

        /// <summary>
        /// A room stated three times is one mistake, not two.
        /// </summary>
        [Fact]
        public void ATargetRepeatedThreeTimes_IsReportedOnce()
        {
            AdjacencyCluster adjacencyCluster = Fixture();

            DesignAirFlowRoundCandidate candidate = Round(adjacencyCluster, [
                Target(adjacencyCluster, Name(name_Kitchen, 1), FlowClassification.Extract, 27),
                Target(adjacencyCluster, Name(name_Kitchen, 1), FlowClassification.Extract, 30),
                Target(adjacencyCluster, Name(name_Kitchen, 1), FlowClassification.Extract, 32)]);

            Assert.False(candidate.IsAccepted);

            Assert.Single(candidate.Refusals, x => x.Contains("more than one deliberate"));
        }

        /// <summary>
        /// A round writes design airflow and nothing else - no runtime or operating airflow, which stays
        /// entirely a matter for re-preparing the iteration. This is the Iteration 3 boundary, pinned.
        /// </summary>
        [Fact]
        public void ARound_WritesNoRuntimeAirflow()
        {
            AdjacencyCluster adjacencyCluster = Fixture();

            List<string> airMovements_Before = AirMovements(adjacencyCluster);

            DesignAirFlowRoundCandidate candidate = Round(adjacencyCluster, [
                Target(adjacencyCluster, Name(name_Kitchen, 1), FlowClassification.Extract, 27),
                Target(adjacencyCluster, Name(name_Bathroom, 1), FlowClassification.Extract, 13)]);

            Assert.True(candidate.IsAccepted);

            Assert.Equal(airMovements_Before, AirMovements(candidate.AdjacencyCluster));
        }

        /// <summary>An empty round is refused: a round is defined by the rooms it deliberately targets.</summary>
        [Fact]
        public void ARoundWithNoTargets_IsRefused()
        {
            AdjacencyCluster adjacencyCluster = Fixture();

            DesignAirFlowRoundCandidate candidate = adjacencyCluster.EvaluateTargetedDesignAirFlows([], PartFExtractAllocationStrategy.MinimumFirstCookingPriority, tolerance_Lps);

            Assert.False(candidate.IsAccepted);
            Assert.Null(candidate.AdjacencyCluster);
            Assert.NotEmpty(candidate.Refusals);
        }

        // ---- Fixture --------------------------------------------------------------------------------------------------

        /// <summary>
        /// Two flats, each its own ventilation system and its own air handling unit, each balanced at
        /// 30/30 l/s and each meeting its Approved Document F requirements with headroom.
        /// <code>
        /// Flat 1   Bedroom 1  supply   requirement 13   design 30
        ///          Kitchen 1  extract  requirement 13   design 22   (local kitchen extract)
        ///          Bathroom 1 extract  requirement  8   design  8
        /// Flat 2   the same rooms, so a round in one flat can be shown not to reach the other
        /// </code>
        /// </summary>
        private static AdjacencyCluster Fixture()
        {
            return Fixture(out List<VentilationUnitCapacityDescriptor> _, false);
        }

        /// <summary>The same fixture with a product selected on each unit - <c>MVHR-35</c>, rated 35/35 l/s,
        /// which the dwelling at 30/30 fits inside with 5 l/s of headroom on each side.</summary>
        private static AdjacencyCluster Fixture(out List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors)
        {
            return Fixture(out ventilationUnitCapacityDescriptors, true);
        }

        private static AdjacencyCluster Fixture(out List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors, bool select)
        {
            ventilationUnitCapacityDescriptors =
            [
                new VentilationUnitCapacityDescriptor(new VentilationUnitReference("Test Fixture", "MVHR-35", null), 35, 35, 0),
                new VentilationUnitCapacityDescriptor(new VentilationUnitReference("Test Fixture", "MVHR-100", null), 100, 100, 1),
            ];

            AdjacencyCluster result = new();

            for (int i = 1; i <= 2; i++)
            {
                AirHandlingUnit airHandlingUnit = new(Name("AHU", i), 20, 20);

                if (select)
                {
                    //Written directly rather than through Modify.SelectVentilationUnit, so the fixture states
                    //which product is selected instead of depending on the selection rule these tests are not
                    //about. MVHR-35 is deliberately NOT the smallest capable product for every round below.
                    airHandlingUnit.SetValue(AirHandlingUnitParameter.VentilationUnitReference, ventilationUnitCapacityDescriptors[0].VentilationUnitReference);
                }

                result.AddObject(airHandlingUnit);

                VentilationSystem ventilationSystem = new(Name("Flat", i), new VentilationSystemType("Fixture MVHR", "Fixture"));
                ventilationSystem.SetValue(VentilationSystemParameter.SupplyUnitName, airHandlingUnit.Name);

                result.AddObject(ventilationSystem);

                Space space_Bedroom = Room(result, Name(name_Bedroom, i), PartFTerminalRole.Supply, 13);
                Space space_Kitchen = Room(result, Name(name_Kitchen, i), PartFTerminalRole.LocalKitchenExtract, 13);
                Space space_Bathroom = Room(result, Name(name_Bathroom, i), PartFTerminalRole.GeneralExtract, 8);

                Terminal(result, ventilationSystem, space_Bedroom, FlowClassification.Supply, 30);
                Terminal(result, ventilationSystem, space_Kitchen, FlowClassification.Extract, 22);
                Terminal(result, ventilationSystem, space_Bathroom, FlowClassification.Extract, 8);

                result.AddRelation(ventilationSystem, space_Bedroom);
                result.AddRelation(ventilationSystem, space_Kitchen);
                result.AddRelation(ventilationSystem, space_Bathroom);
            }

            return result;
        }

        /// <summary>
        /// <b>One</b> air handling unit serving <b>two</b> ventilation systems - the general MEP
        /// arrangement the round promises to stay correct for, and the one the Approved Document O
        /// workflow's one-unit-per-dwelling shape hides.
        /// <code>
        /// MVHR-S rated 35/35
        ///   Flat A   Bedroom A supply 10 (requires 5)   Kitchen A extract 10 (requires 5)
        ///   Flat B   Bedroom B supply 25 (requires 5)   Kitchen B extract 25 (requires 5)
        ///                                               unit duty 35/35 - exactly on the rating
        /// </code>
        /// Both wet rooms are given a low Approved Document F floor so a reduction has real headroom to
        /// come out of, which is what lets one system fall while the other rises.
        /// </summary>
        private static AdjacencyCluster SharedUnitFixture(out List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors)
        {
            ventilationUnitCapacityDescriptors =
            [
                new VentilationUnitCapacityDescriptor(new VentilationUnitReference("Test Fixture", "MVHR-35", null), 35, 35, 0),
            ];

            AdjacencyCluster result = new();

            AirHandlingUnit airHandlingUnit = new("MVHR-S", 20, 20);

            airHandlingUnit.SetValue(AirHandlingUnitParameter.VentilationUnitReference, ventilationUnitCapacityDescriptors[0].VentilationUnitReference);

            result.AddObject(airHandlingUnit);

            Add(result, airHandlingUnit, "A", 10);
            Add(result, airHandlingUnit, "B", 25);

            return result;
        }

        /// <summary>One dwelling of <see cref="SharedUnitFixture"/>, hung off the unit both of them share.</summary>
        private static void Add(AdjacencyCluster adjacencyCluster, AirHandlingUnit airHandlingUnit, string suffix, double designFlowRate_Lps)
        {
            VentilationSystem ventilationSystem = new(string.Format("Flat {0}", suffix), new VentilationSystemType("Fixture MVHR", "Fixture"));

            //The SAME unit name for both, which is how Query.AirHandlingUnit resolves them onto one unit.
            ventilationSystem.SetValue(VentilationSystemParameter.SupplyUnitName, airHandlingUnit.Name);

            adjacencyCluster.AddObject(ventilationSystem);

            Space space_Bedroom = Room(adjacencyCluster, string.Format("Bedroom {0}", suffix), PartFTerminalRole.Supply, 5);
            Space space_Kitchen = Room(adjacencyCluster, string.Format("Kitchen {0}", suffix), PartFTerminalRole.LocalKitchenExtract, 5);

            Terminal(adjacencyCluster, ventilationSystem, space_Bedroom, FlowClassification.Supply, designFlowRate_Lps);
            Terminal(adjacencyCluster, ventilationSystem, space_Kitchen, FlowClassification.Extract, designFlowRate_Lps);

            adjacencyCluster.AddRelation(ventilationSystem, space_Bedroom);
            adjacencyCluster.AddRelation(ventilationSystem, space_Kitchen);
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

        private static string Name(string name, int index)
        {
            return string.Format("{0} {1}", name, index);
        }

        private static DesignAirFlowRoundCandidate Round(AdjacencyCluster adjacencyCluster, List<DesignAirFlowTarget> designAirFlowTargets, List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors = null)
        {
            return adjacencyCluster.EvaluateTargetedDesignAirFlows(designAirFlowTargets, PartFExtractAllocationStrategy.MinimumFirstCookingPriority, tolerance_Lps, ventilationUnitCapacityDescriptors);
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

        /// <summary>Every room's Approved Document F requirement on both sides - the values no round may move.</summary>
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

        private static double Design(AdjacencyCluster adjacencyCluster, string name, FlowClassification flowClassification)
        {
            return Design(adjacencyCluster, Space(adjacencyCluster, name), flowClassification);
        }

        private static double Design(AdjacencyCluster adjacencyCluster, Space space, FlowClassification flowClassification)
        {
            return adjacencyCluster.VentilationTerminals(space).VentilationTerminalDesignDuty_Lps(flowClassification) ?? 0;
        }

        private static string SelectedModel(AdjacencyCluster adjacencyCluster, string name)
        {
            AirHandlingUnit airHandlingUnit = (adjacencyCluster.GetObjects<AirHandlingUnit>() ?? []).Find(x => x?.Name == name);

            Assert.NotNull(airHandlingUnit);

            return airHandlingUnit.SelectedVentilationUnitReference()?.Model;
        }

        /// <summary>
        /// Every inter-zone air movement in the model, as text. A design airflow round must not create,
        /// remove or re-rate one - that is the preparation's job, and doing it here would collapse design
        /// airflow into runtime airflow.
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
