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
    /// <b>What a manufacturer catalogue decides, and therefore what a run must not be allowed to change
    /// halfway through.</b>
    ///
    /// <para><b>Why these tests exist</b></para>
    /// <para>
    /// Iteration 2B stops raising a dwelling's design airflow when the next step would exceed the
    /// ventilation unit it is fitted with. The model stores only the product's IDENTITY - a
    /// <see cref="VentilationUnitReference"/> - and never its capability, deliberately: capability is a
    /// catalogue fact belonging to whoever ships the catalogue, and a capacity copied onto the model would
    /// be a second answer sitting beside the design duty and the Approved Document F requirement. So every
    /// capacity question is answered by looking the stored identity up in a catalogue handed in at the time.
    /// </para>
    /// <para>
    /// That is the right arrangement, and it has one consequence worth pinning rather than assuming: the
    /// stopping point of an optimisation is a property of the CATALOGUE it was checked against, not only of
    /// the design. These tests demonstrate that, so the decision recorded in
    /// <c>SAM.Analytical.UI.WPF.Modify.CanOptimise</c> - that a run reopened from a saved model may be
    /// reviewed but not resumed into Iteration 2B - rests on a measured fact rather than on a plausible
    /// story.
    /// </para>
    ///
    /// <para><b>The invariant these tests must never blur</b></para>
    /// <para>
    /// A capacity is not an airflow. Nothing here writes a capacity into a design airflow, a Part F
    /// requirement or an operating airflow: the capacity only ever decides whether a proposed round is
    /// ADOPTED, and a refused round changes nothing at all.
    /// </para>
    /// </summary>
    public class PartOCatalogueDriftTests
    {
        private const double tolerance_Lps = 0.001;
        private const string manufacturer = "Test Fixture";
        private const string model = "MVHR-35";

        /// <summary>
        /// The same design, the same selected product and the same proposed round, checked against two
        /// catalogues that rate that product differently: one adopts the round and the other refuses it.
        /// <para>
        /// This is the whole of the risk. A dwelling sitting on its unit's ceiling is what
        /// <c>guids_AtCapacity</c> records and what makes an optimisation stop, so a catalogue corrected
        /// between the baseline and a resumed optimisation moves the answer without anything in the model
        /// having changed.
        /// </para>
        /// </summary>
        [Fact]
        public void TheSameRound_IsAdoptedOrRefusedDependingOnlyOnTheCatalogue()
        {
            AdjacencyCluster adjacencyCluster = Fixture();

            //38/30 after the round: inside a unit rated 40, outside one rated 35.
            List<DesignAirFlowTarget> designAirFlowTargets = [Target(adjacencyCluster, "Bedroom", FlowClassification.Supply, 38)];

            DesignAirFlowRoundCandidate candidate_Generous = Round(adjacencyCluster, designAirFlowTargets, Catalogue(40, 40));
            DesignAirFlowRoundCandidate candidate_Corrected = Round(adjacencyCluster, designAirFlowTargets, Catalogue(35, 35));

            Assert.True(candidate_Generous.IsAccepted);
            Assert.False(candidate_Corrected.IsAccepted);

            //And it is an EQUIPMENT refusal naming the very product the baseline selected - not a design
            //problem that happens to look like one.
            DwellingDesignAirFlowRound dwellingDesignAirFlowRound = Assert.Single(candidate_Corrected.VentilationUnitRefusals);

            Assert.Equal(model, dwellingDesignAirFlowRound.VentilationUnitReference.Model);
            Assert.Equal(35, dwellingDesignAirFlowRound.VentilationUnitCapacityDescriptor.MaximumSupplyFlowRate_Lps, 6);
            Assert.Equal(38, dwellingDesignAirFlowRound.SupplyDuty_After_Lps, 6);
        }

        /// <summary>
        /// A refused round changes nothing: the design airflows are exactly what they were. So a resumed
        /// optimisation checked against the wrong catalogue does not corrupt a model - it stops in the wrong
        /// place, which is worse, because the design it hands back looks like an answer.
        /// </summary>
        [Fact]
        public void ARefusedRound_LeavesTheDesignExactlyAsItWas()
        {
            AdjacencyCluster adjacencyCluster = Fixture();

            double design_Before = Design(adjacencyCluster, "Bedroom", FlowClassification.Supply);

            DesignAirFlowRoundCandidate candidate = Round(adjacencyCluster, [Target(adjacencyCluster, "Bedroom", FlowClassification.Supply, 38)], Catalogue(35, 35));

            Assert.False(candidate.IsAccepted);
            Assert.Null(candidate.AdjacencyCluster);
            Assert.Equal(design_Before, Design(adjacencyCluster, "Bedroom", FlowClassification.Supply), 6);
        }

        /// <summary>
        /// A catalogue that no longer offers the selected product cannot show the round is within capacity,
        /// and the round is refused rather than passed.
        /// <para>
        /// This is what makes "no evidence" safe: a product withdrawn, renamed or re-referenced between the
        /// baseline and a later session leaves the capacity unknown, and an unknown capacity is never
        /// treated as a big enough one.
        /// </para>
        /// </summary>
        [Fact]
        public void ACatalogueMissingTheSelectedProduct_RefusesRatherThanPasses()
        {
            AdjacencyCluster adjacencyCluster = Fixture();

            List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors =
            [
                new VentilationUnitCapacityDescriptor(new VentilationUnitReference(manufacturer, "MVHR-200", null), 200, 200, 0),
            ];

            DesignAirFlowRoundCandidate candidate = Round(adjacencyCluster, [Target(adjacencyCluster, "Bedroom", FlowClassification.Supply, 31)], ventilationUnitCapacityDescriptors);

            Assert.False(candidate.IsAccepted);
            Assert.NotEmpty(candidate.VentilationUnitRefusals);
        }

        /// <summary>
        /// Two entries disagreeing about one product identity leave the capacity unknown too, so the round
        /// is refused rather than answered by whichever line was read first.
        /// </summary>
        [Fact]
        public void ACatalogueContradictingItself_RefusesRatherThanPicksALine()
        {
            AdjacencyCluster adjacencyCluster = Fixture();

            List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors =
            [
                new VentilationUnitCapacityDescriptor(new VentilationUnitReference(manufacturer, model, null), 100, 100, 0),
                new VentilationUnitCapacityDescriptor(new VentilationUnitReference(manufacturer, model, null), 35, 35, 0),
            ];

            DesignAirFlowRoundCandidate candidate = Round(adjacencyCluster, [Target(adjacencyCluster, "Bedroom", FlowClassification.Supply, 31)], ventilationUnitCapacityDescriptors);

            Assert.False(candidate.IsAccepted);
        }

        /// <summary>
        /// The model itself carries the product's identity and <b>no capacity at all</b>, which is why the
        /// catalogue has to be handed in and why a saved model cannot answer the capacity question on its
        /// own. This is the fact the whole decision turns on, asserted rather than assumed.
        /// </summary>
        [Fact]
        public void TheModelStoresTheProductIdentityAndNoCapacity()
        {
            AdjacencyCluster adjacencyCluster = Fixture();

            AirHandlingUnit airHandlingUnit = Assert.Single(adjacencyCluster.GetObjects<AirHandlingUnit>());

            VentilationUnitReference ventilationUnitReference = airHandlingUnit.SelectedVentilationUnitReference();

            Assert.NotNull(ventilationUnitReference);
            Assert.Equal(manufacturer, ventilationUnitReference.Manufacturer);
            Assert.Equal(model, ventilationUnitReference.Model);

            //Nothing on the unit states what it can move - asserted against what the unit actually
            //PERSISTS, not against a lookup handed an empty catalogue. That lookup iterates the descriptors
            //it is given, so a null answer to a null catalogue is true whatever the model stores, and would
            //keep this test passing if a later change began writing capacity onto the unit while leaving
            //the lookup catalogue-based. The saved state is the thing the decision rests on, so the saved
            //state is what is read.
            string text = airHandlingUnit.ToJsonObject().ToString();

            Assert.Contains(manufacturer, text, StringComparison.Ordinal);
            Assert.Contains(model, text, StringComparison.Ordinal);

            Assert.DoesNotContain("MaximumSupplyFlowRate", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("MaximumExtractFlowRate", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("CapacityDescriptor", text, StringComparison.OrdinalIgnoreCase);
        }

        // ---- The fixture -----------------------------------------------------------------------------

        /// <summary>
        /// One dwelling on one unit, designed at 30/30 l/s and fitted with <c>MVHR-35</c> - close enough to
        /// any of the ratings below that one step decides the answer.
        /// </summary>
        private static AdjacencyCluster Fixture()
        {
            AdjacencyCluster result = new();

            AirHandlingUnit airHandlingUnit = new("AHU 1", 20, 20);

            //Written directly, so the fixture states which product is selected rather than depending on the
            //selection rule these tests are not about.
            airHandlingUnit.SetValue(AirHandlingUnitParameter.VentilationUnitReference, new VentilationUnitReference(manufacturer, model, null));

            result.AddObject(airHandlingUnit);

            VentilationSystem ventilationSystem = new("Flat 1", new VentilationSystemType("Fixture MVHR", "Fixture"));
            ventilationSystem.SetValue(VentilationSystemParameter.SupplyUnitName, airHandlingUnit.Name);

            result.AddObject(ventilationSystem);

            Space space_Bedroom = Room(result, "Bedroom", PartFTerminalRole.Supply, 13);
            Space space_Kitchen = Room(result, "Kitchen", PartFTerminalRole.LocalKitchenExtract, 13);
            Space space_Bathroom = Room(result, "Bathroom", PartFTerminalRole.GeneralExtract, 8);

            Terminal(result, ventilationSystem, space_Bedroom, FlowClassification.Supply, 30);
            Terminal(result, ventilationSystem, space_Kitchen, FlowClassification.Extract, 22);
            Terminal(result, ventilationSystem, space_Bathroom, FlowClassification.Extract, 8);

            result.AddRelation(ventilationSystem, space_Bedroom);
            result.AddRelation(ventilationSystem, space_Kitchen);
            result.AddRelation(ventilationSystem, space_Bathroom);

            return result;
        }

        /// <summary>One catalogue, offering the selected product at the stated rating and nothing else.</summary>
        private static List<VentilationUnitCapacityDescriptor> Catalogue(double maximumSupplyFlowRate_Lps, double maximumExtractFlowRate_Lps)
        {
            return [new VentilationUnitCapacityDescriptor(new VentilationUnitReference(manufacturer, model, null), maximumSupplyFlowRate_Lps, maximumExtractFlowRate_Lps, 0)];
        }

        private static DesignAirFlowRoundCandidate Round(AdjacencyCluster adjacencyCluster, List<DesignAirFlowTarget> designAirFlowTargets, List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors)
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

        private static double Design(AdjacencyCluster adjacencyCluster, string name, FlowClassification flowClassification)
        {
            return adjacencyCluster.VentilationTerminals(Space(adjacencyCluster, name)).VentilationTerminalDesignDuty_Lps(flowClassification) ?? 0;
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
    }
}
