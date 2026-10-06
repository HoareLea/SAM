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
    /// <b>Approved Document O Iteration 2 - selecting a real ventilation unit, and the four quantities
    /// that must never collapse into each other.</b>
    /// <para>
    /// Iteration 1a established a design that realized the Approved Document F requirement exactly, on
    /// generic plant. Iteration 2 puts a product behind the plant and lets the design move above the
    /// requirement, which means four numbers now exist where one used to serve:
    /// </para>
    /// <code>
    /// PartFRequiredAirFlow  &lt;=  DesignAirFlow  &lt;=  SelectedUnitCapacity        (operating airflow: later)
    /// </code>
    /// <para>
    /// Every test here pins one joint of that chain, or pins one of the ways it could collapse - a
    /// capacity written in as a requirement, a capacity taken up as a design, a design change leaking
    /// into a neighbouring room, a dwelling sized off another dwelling's duty, or a design change
    /// reaching the simulation's own airflow fields.
    /// </para>
    /// <para>
    /// <b>The catalogues below are test fixtures, not copies of a shipped one.</b> <c>SAM.Analytical</c>
    /// owns the vocabulary and the selection rule; which products exist is a fact about whoever is
    /// asking, so nothing here names a real manufacturer - the same arrangement, and the same reason, as
    /// <see cref="SystemCapabilitySelectionTests"/>.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Shares a collection with the other readers of the default Part F rule set, so the two never run at
    /// the same time: the rule set is reached through the process-wide <c>ActiveSetting.Setting</c> and
    /// its stored <c>PartFData</c> is shared by reference between every <c>PartFCalculator</c> built
    /// from it.
    /// </remarks>
    [Collection("SAM.Analytical.ActiveSetting default Part F data")]
    public class PartOVentilationUnitSelectionTests
    {
        private const string name_LivingRoom = "Living Room";

        private const string name_Bedroom = "Bedroom";

        private const string name_Kitchen = "Kitchen";

        private const string name_Bathroom = "Bathroom";

        private const string name_ZoneCategory = "Flats";

        /// <summary>The shipped Nuaire hybrid unit's model, 150/150 l/s.</summary>
        private const string model_MRXBOX = "MRXBOXAB-ECO5-AECV";

        /// <summary>The shipped Nuaire XBOXER commercial unit's model, 190/190 l/s.</summary>
        private const string model_XBC15 = "XBC15";

        // =================================================================================================
        // A. Product selection - the pure rule, over a fixture catalogue
        // =================================================================================================

        /// <summary>
        /// The rule Approved Document O sizing rests on: the smallest product that can do the job, never
        /// the nearest one. A 115 l/s duty against 100 / 150 / 180 / 220 selects 150 - an absolute-distance
        /// "nearest" would answer 100, which cannot ventilate the dwelling at all.
        /// </summary>
        [Fact]
        public void SelectSmallestCapableVentilationUnit_TakesTheSmallestSufficientProduct()
        {
            VentilationUnitSelection selection = Catalogue().SelectSmallestCapableVentilationUnit(115, 115);

            Assert.True(selection.IsSelected);
            Assert.Equal("MVHR-150", selection.VentilationUnitReference?.Model);

            //And the headroom is reported rather than spent.
            Assert.Equal(35, selection.SupplyHeadroom_Lps, 3);
            Assert.Equal(35, selection.ExtractHeadroom_Lps, 3);
        }

        /// <summary>A duty exactly at a product's rating is met by that product, not by the next one up.</summary>
        [Fact]
        public void ExactCapacityMatch_SelectsThatProduct()
        {
            Assert.Equal("MVHR-150", Catalogue().SelectSmallestCapableVentilationUnit(150, 150).VentilationUnitReference?.Model);
            Assert.Equal("MVHR-100", Catalogue().SelectSmallestCapableVentilationUnit(100, 100).VentilationUnitReference?.Model);
        }

        /// <summary>
        /// An undersized product is never the answer, however close it is. One l/s over a rating moves the
        /// selection to the next product up.
        /// </summary>
        [Fact]
        public void UndersizedProduct_IsRejected()
        {
            Assert.Equal("MVHR-150", Catalogue().SelectSmallestCapableVentilationUnit(101, 101).VentilationUnitReference?.Model);

            Assert.DoesNotContain(
                Catalogue().CapableVentilationUnits(101, 101),
                x => x.MaximumSupplyFlowRate_Lps < 101);
        }

        /// <summary>
        /// The two sides are checked independently. A dwelling needing 140 l/s of supply and 60 l/s of
        /// extract is not served by a product rated 100 supply and 200 extract, however comfortably its
        /// total covers the total.
        /// </summary>
        [Fact]
        public void SupplyAndExtractCapacities_AreCheckedIndependently()
        {
            List<VentilationUnitCapacityDescriptor> descriptors =
            [
                Descriptor("Asymmetric", 100, 200),
                Descriptor("Balanced", 150, 150),
            ];

            //Asymmetric is not the answer despite its larger total: its supply side is short.
            Assert.Equal("Balanced", descriptors.SelectSmallestCapableVentilationUnit(140, 60).VentilationUnitReference?.Model);

            //And the mirror image, so the check is not accidentally one-sided.
            List<VentilationUnitCapacityDescriptor> descriptors_Mirror =
            [
                Descriptor("Asymmetric", 200, 100),
                Descriptor("Balanced", 150, 150),
            ];

            Assert.Equal("Balanced", descriptors_Mirror.SelectSmallestCapableVentilationUnit(60, 140).VentilationUnitReference?.Model);
        }

        /// <summary>
        /// Nothing compliant is a deterministic, explained refusal - never the biggest product on the
        /// shelf, and never nothing at all with no reason. The message names both duties and says how far
        /// short the catalogue fell.
        /// </summary>
        [Fact]
        public void NoCompliantProduct_RefusesDeterministically()
        {
            VentilationUnitSelection selection = Catalogue().SelectSmallestCapableVentilationUnit(400, 400);

            Assert.False(selection.IsSelected);
            Assert.Null(selection.VentilationUnitReference);
            Assert.NotNull(selection.Reason);
            Assert.Contains("400", selection.Reason);
            Assert.Contains("220", selection.Reason);

            //Deterministic: the same question twice gives the same refusal, and the catalogue's order does
            //not change it.
            List<VentilationUnitCapacityDescriptor> descriptors_Reversed = Catalogue();
            descriptors_Reversed.Reverse();

            Assert.Equal(selection.Reason, descriptors_Reversed.SelectSmallestCapableVentilationUnit(400, 400).Reason);
        }

        /// <summary>
        /// Two different products that are the same size on both sides and the same rank are an ambiguity
        /// the catalogue has not resolved, and neither is chosen - the rule
        /// <c>Query.SelectPreferredCapableSystem</c> already follows. A rank separates them; one product
        /// listed twice is a duplicated entry and is answered normally.
        /// </summary>
        [Fact]
        public void EquallySizedDifferentProducts_RefuseUnlessTheCatalogueRanksThem()
        {
            List<VentilationUnitCapacityDescriptor> descriptors = [Descriptor("A", 150, 150), Descriptor("B", 150, 150)];

            Assert.False(descriptors.SelectSmallestCapableVentilationUnit(115, 115).IsSelected);

            List<VentilationUnitCapacityDescriptor> descriptors_Ranked = [Descriptor("A", 150, 150, 2), Descriptor("B", 150, 150, 1)];

            Assert.Equal("B", descriptors_Ranked.SelectSmallestCapableVentilationUnit(115, 115).VentilationUnitReference?.Model);

            //The same product twice is not a choice.
            List<VentilationUnitCapacityDescriptor> descriptors_Duplicated = [Descriptor("A", 150, 150), Descriptor("A", 150, 150)];

            Assert.Equal("A", descriptors_Duplicated.SelectSmallestCapableVentilationUnit(115, 115).VentilationUnitReference?.Model);
        }

        /// <summary>
        /// The answer does not depend on the order the catalogue arrived in - a library that enumerated a
        /// directory must not let the file system choose a dwelling's plant.
        /// </summary>
        [Fact]
        public void SelectionIsIndependentOfCatalogueOrder()
        {
            List<VentilationUnitCapacityDescriptor> descriptors = Catalogue();

            for (int i = 0; i < descriptors.Count; i++)
            {
                List<VentilationUnitCapacityDescriptor> descriptors_Rotated = descriptors.GetRange(i, descriptors.Count - i);
                descriptors_Rotated.AddRange(descriptors.GetRange(0, i));

                Assert.Equal("MVHR-150", descriptors_Rotated.SelectSmallestCapableVentilationUnit(115, 115).VentilationUnitReference?.Model);
            }
        }

        // =================================================================================================
        // B. The dwelling: requirement, design and capacity held apart
        // =================================================================================================

        /// <summary>
        /// Selecting a unit changes no Approved Document F requirement, anywhere. The regulatory numbers
        /// are exactly what the Part F calculation wrote, before and after.
        /// </summary>
        [Fact]
        public void SelectingAUnit_LeavesEveryPartFRequirementUnchanged()
        {
            //ONE source model, prepared twice. Two separately built models would carry different requirement
            //guids - PartFCalculator mints them per run - and the comparison would fail for a reason that
            //has nothing to do with what is being asserted.
            AnalyticalModel analyticalModel = Model();

            PartOIterationPreparation preparation_Generic = Prepare(analyticalModel, null);
            PartOIterationPreparation preparation_Selected = Prepare(analyticalModel, DwellingCatalogue());

            Assert.Null(preparation_Selected.Refusal);
            Assert.Single(preparation_Selected.VentilationUnitSelections);

            Assert.Equal(Requirements(preparation_Generic), Requirements(preparation_Selected));
        }

        /// <summary>
        /// The Iteration 1a chain still stands with a product behind it: the terminals, the system, the
        /// unit, the duty and the air movements are all still there, and the duty is unchanged by the
        /// selection.
        /// </summary>
        [Fact]
        public void SelectingAUnit_LeavesTheIteration1aNetworkIntact()
        {
            AnalyticalModel analyticalModel = Model();

            PartOIterationPreparation preparation_Generic = Prepare(analyticalModel, null);
            PartOIterationPreparation preparation_Selected = Prepare(analyticalModel, DwellingCatalogue());

            Assert.Null(preparation_Selected.Refusal);

            Assert.Equal(preparation_Generic.DesignSupplyDuty_Lps, preparation_Selected.DesignSupplyDuty_Lps, 6);
            Assert.Equal(preparation_Generic.DesignExtractDuty_Lps, preparation_Selected.DesignExtractDuty_Lps, 6);

            Assert.Equal(preparation_Generic.VentilationTerminals.Count, preparation_Selected.VentilationTerminals.Count);
            Assert.Equal(AirMovementCount(preparation_Generic), AirMovementCount(preparation_Selected));
        }

        /// <summary>
        /// The design realizes the requirement and nothing more before anyone optimises: every room's
        /// design airflow starts equal to what Approved Document F required of it.
        /// </summary>
        [Fact]
        public void InitialDesignAirFlow_EqualsThePartFRequirement()
        {
            PartOIterationPreparation preparation = Prepared(DwellingCatalogue());

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            int compared = 0;

            foreach (Space space in adjacencyCluster.GetSpaces())
            {
                foreach (FlowClassification flowClassification in new[] { FlowClassification.Supply, FlowClassification.Extract })
                {
                    double? requirement_Lps = adjacencyCluster.PartFRequiredFlowRate_Lps(space, flowClassification);
                    if (!requirement_Lps.HasValue || requirement_Lps.Value == 0)
                    {
                        continue;
                    }

                    Assert.Equal(requirement_Lps.Value, Design(adjacencyCluster, space, flowClassification), 6);

                    compared++;
                }
            }

            Assert.True(compared >= 4, "The fixture must have sized rooms in both directions or this test proves nothing.");
        }

        /// <summary>
        /// <b>The headroom is not spent.</b> A dwelling needing 19.2 l/s fitted with a 25 l/s unit is a
        /// 19.2 l/s dwelling: the design duty is what the terminals say, the capacity is what the catalogue
        /// says, and the selection never moves the first towards the second.
        /// </summary>
        [Fact]
        public void SelectedCapacity_IsNeverTakenUpAsDesignAirFlow()
        {
            PartOIterationPreparation preparation = Prepared(DwellingCatalogue());

            VentilationUnitSelection selection = Assert.Single(preparation.VentilationUnitSelections);

            Assert.True(selection.Descriptor.MaximumSupplyFlowRate_Lps > preparation.DesignSupplyDuty_Lps, "The fixture must select a unit with headroom or this test proves nothing.");
            Assert.True(selection.SupplyHeadroom_Lps > 0);

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;
            AirHandlingUnit airHandlingUnit = Assert.Single(adjacencyCluster.GetObjects<AirHandlingUnit>());

            adjacencyCluster.AirHandlingUnitDesignDuty(airHandlingUnit, out double supplyDuty_Lps, out double extractDuty_Lps);

            Assert.Equal(preparation.DesignSupplyDuty_Lps, supplyDuty_Lps, 6);
            Assert.Equal(preparation.DesignExtractDuty_Lps, extractDuty_Lps, 6);

            //And no room's design airflow was raised either - the duty is what it was with no catalogue at all.
            Assert.Equal(Prepared(null).DesignSupplyDuty_Lps, supplyDuty_Lps, 6);
        }

        /// <summary>
        /// <b>A capacity is never written into a requirement.</b> The rating of the selected product
        /// appears nowhere in the model's Approved Document F data - not as a room rate, not as a system
        /// total.
        /// </summary>
        [Fact]
        public void SelectedCapacity_IsNeverWrittenIntoThePartFRequirement()
        {
            PartOIterationPreparation preparation = Prepared(DwellingCatalogue());

            VentilationUnitSelection selection = Assert.Single(preparation.VentilationUnitSelections);

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            VentilationSystem ventilationSystem = Assert.Single(adjacencyCluster.GetObjects<VentilationSystem>());

            Assert.True(adjacencyCluster.PartFRequiredSystemDuty(ventilationSystem, out double requirement_Supply_Lps, out double requirement_Extract_Lps));

            Assert.Equal(preparation.DesignSupplyDuty_Lps, requirement_Supply_Lps, 6);
            Assert.Equal(preparation.DesignExtractDuty_Lps, requirement_Extract_Lps, 6);

            Assert.NotEqual(selection.Descriptor.MaximumSupplyFlowRate_Lps, requirement_Supply_Lps, 3);
            Assert.NotEqual(selection.Descriptor.MaximumExtractFlowRate_Lps, requirement_Extract_Lps, 3);
        }

        // =================================================================================================
        // C. Dwelling isolation
        // =================================================================================================

        /// <summary>
        /// Two dwellings choose their own equipment from their own duties. Nothing is aggregated, nothing
        /// is balanced between them, and the small flat is not fitted with the large flat's unit.
        /// </summary>
        [Fact]
        public void TwoDwellings_SelectIndependently()
        {
            PartOIterationPreparation preparation = Prepare(TwoDwellingModel(), DwellingCatalogue());

            Assert.Null(preparation.Refusal);
            Assert.Equal(2, preparation.AirHandlingUnits.Count);
            Assert.Equal(2, preparation.VentilationUnitSelections.Count);

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            List<AirHandlingUnit> airHandlingUnits = adjacencyCluster.GetObjects<AirHandlingUnit>();

            List<double> duties = [];
            List<string> models = [];

            foreach (AirHandlingUnit airHandlingUnit in airHandlingUnits)
            {
                adjacencyCluster.AirHandlingUnitDesignDuty(airHandlingUnit, out double supplyDuty_Lps, out double extractDuty_Lps);

                VentilationUnitCapacityDescriptor descriptor = airHandlingUnit.SelectedVentilationUnitCapacityDescriptor(DwellingCatalogue());

                Assert.NotNull(descriptor);

                //Each unit covers ITS OWN dwelling's duty, and is the smallest that does - no capacity is
                //borrowed from or lent to the other dwelling.
                Assert.True(descriptor.IsSufficientFor(supplyDuty_Lps, extractDuty_Lps));
                Assert.Equal(
                    DwellingCatalogue().SelectSmallestCapableVentilationUnit(supplyDuty_Lps, extractDuty_Lps).VentilationUnitReference?.Model,
                    descriptor.VentilationUnitReference?.Model);

                duties.Add(supplyDuty_Lps);
                models.Add(descriptor.VentilationUnitReference?.Model);
            }

            //The two dwellings really are different sizes and really did pick different products, or
            //nothing above would have distinguished independence from coincidence.
            Assert.NotEqual(duties[0], duties[1], 3);
            Assert.NotEqual(models[0], models[1]);
        }

        /// <summary>
        /// Changing one dwelling's design leaves the other's design, requirement and selected unit exactly
        /// where they were. There is no cross-dwelling balancing to leak through.
        /// </summary>
        [Fact]
        public void ChangingOneDwelling_DoesNotAffectAnother()
        {
            PartOIterationPreparation preparation = Prepare(TwoDwellingModel(), DwellingCatalogue());

            Assert.Null(preparation.Refusal);

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            Space space_Changed = adjacencyCluster.GetSpaces().Find(x => x.Name == Name(name_Bedroom, 1));
            Space space_Other = adjacencyCluster.GetSpaces().Find(x => x.Name == Name(name_Bedroom, 2));

            Assert.NotNull(space_Changed);
            Assert.NotNull(space_Other);

            double design_Other_Before = Design(adjacencyCluster, space_Other, FlowClassification.Supply);
            double? requirement_Other_Before = adjacencyCluster.PartFRequiredFlowRate_Lps(space_Other, FlowClassification.Supply);

            List<string> models_Before = SelectedModels(adjacencyCluster);

            Assert.NotNull(adjacencyCluster.SetSpaceDesignFlowRate(space_Changed, FlowClassification.Supply, Design(adjacencyCluster, space_Changed, FlowClassification.Supply) + 2, out _, out List<string> refusals));
            Assert.Empty(refusals);

            Assert.Equal(design_Other_Before, Design(adjacencyCluster, space_Other, FlowClassification.Supply), 6);
            Assert.Equal(requirement_Other_Before, adjacencyCluster.PartFRequiredFlowRate_Lps(space_Other, FlowClassification.Supply));
            Assert.Equal(models_Before, SelectedModels(adjacencyCluster));
        }

        // =================================================================================================
        // D. Space-targeted design airflow
        // =================================================================================================

        /// <summary>
        /// <b>The control Approved Document O optimisation actually needs.</b> One failing bedroom is
        /// raised and the passing rooms do not move. Nothing here scales anything, so a proportional
        /// strategy stays a strategy rather than becoming the only mechanism the model has.
        /// </summary>
        [Fact]
        public void RaisingOneRoomDesignAirFlow_LeavesEveryOtherRoomAlone()
        {
            PartOIterationPreparation preparation = Prepared(DwellingCatalogue());

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            Space space_Bedroom = adjacencyCluster.GetSpaces().Find(x => x.Name == name_Bedroom);
            Space space_LivingRoom = adjacencyCluster.GetSpaces().Find(x => x.Name == name_LivingRoom);

            double design_Bedroom_Before = Design(adjacencyCluster, space_Bedroom, FlowClassification.Supply);
            double design_LivingRoom_Before = Design(adjacencyCluster, space_LivingRoom, FlowClassification.Supply);

            Assert.NotNull(adjacencyCluster.SetSpaceDesignFlowRate(space_Bedroom, FlowClassification.Supply, design_Bedroom_Before + 4, out List<string> notes, out List<string> refusals));

            Assert.Empty(refusals);
            Assert.NotEmpty(notes);

            Assert.Equal(design_Bedroom_Before + 4, Design(adjacencyCluster, space_Bedroom, FlowClassification.Supply), 6);
            Assert.Equal(design_LivingRoom_Before, Design(adjacencyCluster, space_LivingRoom, FlowClassification.Supply), 6);
        }

        /// <summary>
        /// The regulatory number does not move when the design does. That is the whole separation: a
        /// design airflow is a choice made above a floor, and the floor is not part of the choice.
        /// </summary>
        [Fact]
        public void ChangingDesignAirFlow_LeavesThePartFRequirementUnchanged()
        {
            PartOIterationPreparation preparation = Prepared(DwellingCatalogue());

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            Space space = adjacencyCluster.GetSpaces().Find(x => x.Name == name_Bedroom);

            double? requirement_Before = adjacencyCluster.PartFRequiredFlowRate_Lps(space, FlowClassification.Supply);

            Assert.True(requirement_Before.HasValue);

            Assert.NotNull(adjacencyCluster.SetSpaceDesignFlowRate(space, FlowClassification.Supply, requirement_Before.Value + 4, out _, out _));

            Space space_After = adjacencyCluster.GetSpaces().Find(x => x.Name == name_Bedroom);

            Assert.Equal(requirement_Before, adjacencyCluster.PartFRequiredFlowRate_Lps(space_After, FlowClassification.Supply));

            //And the terminal's own lineage still recovers the same regulatory number, unchanged.
            foreach (VentilationTerminal ventilationTerminal in Analytical.Query.VentilationTerminals(adjacencyCluster.VentilationTerminals(space_After), FlowClassification.Supply))
            {
                Assert.Equal(requirement_Before, adjacencyCluster.PartFRequiredFlowRate_Lps(ventilationTerminal));
            }
        }

        /// <summary>
        /// A design below the regulatory minimum is not a design decision but a compliance failure, and is
        /// refused rather than recorded. Nothing changes.
        /// </summary>
        [Fact]
        public void DesignAirFlowBelowThePartFRequirement_IsRefused()
        {
            PartOIterationPreparation preparation = Prepared(DwellingCatalogue());

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            Space space = adjacencyCluster.GetSpaces().Find(x => x.Name == name_Bedroom);

            double design_Before = Design(adjacencyCluster, space, FlowClassification.Supply);

            Assert.Null(adjacencyCluster.SetSpaceDesignFlowRate(space, FlowClassification.Supply, design_Before - 1, out _, out List<string> refusals));

            Assert.NotEmpty(refusals);
            Assert.Equal(design_Before, Design(adjacencyCluster, space, FlowClassification.Supply), 6);
        }

        /// <summary>
        /// A subdivided room keeps its subdivision, and the new total is distributed in the proportions the
        /// designer already chose. Four diffusers stay four diffusers.
        /// </summary>
        [Fact]
        public void ChangingDesignAirFlow_PreservesASubdividedRoom()
        {
            PartOIterationPreparation preparation = Prepared(DwellingCatalogue());

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            Space space = adjacencyCluster.GetSpaces().Find(x => x.Name == name_Bedroom);

            List<VentilationTerminal> ventilationTerminals = Analytical.Query.VentilationTerminals(adjacencyCluster.VentilationTerminals(space), FlowClassification.Supply);

            VentilationTerminal ventilationTerminal = Assert.Single(ventilationTerminals);

            double design_Before = ventilationTerminal.DesignFlowRate_Lps.Value;

            //Split 3 : 1, deliberately unequal, so an equal redistribution would be visible.
            ventilationTerminal.DesignFlowRate_Lps = design_Before * 0.75;
            adjacencyCluster.AddObject(ventilationTerminal);

            VentilationTerminal ventilationTerminal_Second = new(ventilationTerminal.Name + " (2)", FlowClassification.Supply, design_Before * 0.25);
            ventilationTerminal_Second.SetValue(VentilationTerminalParameter.PartFTerminalReference, ventilationTerminal.GetValue<PartFTerminalReference>(VentilationTerminalParameter.PartFTerminalReference));

            adjacencyCluster.AddObject(ventilationTerminal_Second);
            adjacencyCluster.AddRelation(ventilationTerminal_Second, space);

            List<VentilationTerminal> ventilationTerminals_After = adjacencyCluster.SetSpaceDesignFlowRate(space, FlowClassification.Supply, design_Before + 8, out _, out List<string> refusals);

            Assert.Empty(refusals);
            Assert.Equal(2, ventilationTerminals_After.Count);
            Assert.Equal(design_Before + 8, Design(adjacencyCluster, space, FlowClassification.Supply), 6);

            ventilationTerminals_After.Sort((x, y) => y.DesignFlowRate_Lps.Value.CompareTo(x.DesignFlowRate_Lps.Value));

            Assert.Equal((design_Before + 8) * 0.75, ventilationTerminals_After[0].DesignFlowRate_Lps.Value, 6);
            Assert.Equal((design_Before + 8) * 0.25, ventilationTerminals_After[1].DesignFlowRate_Lps.Value, 6);
        }

        // =================================================================================================
        // E. Recalculating the dwelling network after a local change
        // =================================================================================================

        /// <summary>
        /// <b>One targeted room, and everything else derived.</b> The failing bedroom is the only room
        /// anybody selects; the extract that appears to balance it is a consequence of the dwelling's
        /// network, decided by the allocation strategy the Part F calculation already names. The
        /// transaction says which is which, and the two are never confused.
        /// </summary>
        [Fact]
        public void ATargetedChange_IsOneRoomAndTheRestIsDerived()
        {
            PartOIterationPreparation preparation = Prepared(DwellingCatalogue());

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            Space space_Bedroom = adjacencyCluster.GetSpaces().Find(x => x.Name == name_Bedroom);

            double design_Bedroom_Before = Design(adjacencyCluster, space_Bedroom, FlowClassification.Supply);

            DwellingDesignAirFlowChange change = adjacencyCluster.ApplyTargetedDesignAirFlow(space_Bedroom, FlowClassification.Supply, design_Bedroom_Before + 4);

            Assert.True(change.Successful, string.Join(" ", change.Refusals));

            //Exactly one targeted room, and it is the one that was asked for.
            Assert.NotNull(change.TargetedAdjustment);
            Assert.False(change.TargetedAdjustment.IsDerived);
            Assert.Equal(name_Bedroom, change.TargetedAdjustment.SpaceName);
            Assert.Equal(design_Bedroom_Before, change.TargetedAdjustment.Before_Lps, 6);
            Assert.Equal(design_Bedroom_Before + 4, change.TargetedAdjustment.After_Lps, 6);

            //Everything else that moved is on the record as derived, is extract, and totals exactly the
            //4 l/s the targeted change created.
            Assert.NotEmpty(change.DerivedAdjustments);

            double derived_Lps = 0;
            foreach (DesignAirFlowAdjustment adjustment in change.DerivedAdjustments)
            {
                Assert.True(adjustment.IsDerived);
                Assert.Equal(FlowClassification.Extract, adjustment.FlowClassification);

                derived_Lps += adjustment.Change_Lps;
            }

            Assert.Equal(4, derived_Lps, 6);

            //No supply room other than the target moved.
            Assert.DoesNotContain(change.Adjustments, x => x.FlowClassification == FlowClassification.Supply && x.SpaceName != name_Bedroom);
        }

        /// <summary>
        /// <b>A local change is not a local edit.</b> After the targeted change and its derived balancing,
        /// re-preparing recalculates the whole network: the supply into the bedroom, the transfer air the
        /// dwelling routes, the extract totals and the unit's design duty all move together, and the model
        /// still balances at every node - which is what TAS requires of it.
        /// </summary>
        [Fact]
        public void ALocalDesignChange_RecalculatesTheWholeDwellingNetwork()
        {
            PartOIterationPreparation preparation = Prepared(DwellingCatalogue());

            double supplyDuty_Before = preparation.DesignSupplyDuty_Lps;
            double extractDuty_Before = preparation.DesignExtractDuty_Lps;
            int airMovements_Before = AirMovementCount(preparation);

            PartOIterationPreparation preparation_After = Prepare(Retargeted(preparation, name_Bedroom, FlowClassification.Supply, 4), DwellingCatalogue());

            //The preparation ran to completion, which means RefuseUnbalancedAirMovement passed: every space
            //and the unit still pass on exactly what they receive over the recalculated network.
            Assert.Null(preparation_After.Refusal);

            Assert.Equal(supplyDuty_Before + 4, preparation_After.DesignSupplyDuty_Lps, 6);
            Assert.Equal(extractDuty_Before + 4, preparation_After.DesignExtractDuty_Lps, 6);

            //Balanced afterwards, which is the condition the whole transaction exists to preserve.
            Assert.Equal(preparation_After.DesignSupplyDuty_Lps, preparation_After.DesignExtractDuty_Lps, 6);

            //Rebuilt, not accumulated: re-preparing produces one network, not two.
            Assert.Equal(airMovements_Before, AirMovementCount(preparation_After));
        }

        /// <summary>
        /// The derived extract lands where Approved Document F already says surplus extract belongs - at
        /// the cooking function, by
        /// <c>PartFExtractAllocationStrategy.MinimumFirstCookingPriority</c>, which is the same strategy
        /// <c>PartFCalculator</c> used to size the dwelling in the first place. The bathroom, which nobody
        /// targeted and which the strategy does not prioritise, does not move.
        /// </summary>
        [Fact]
        public void TheDerivedExtract_FollowsTheExistingAllocationStrategy()
        {
            PartOIterationPreparation preparation = Prepared(DwellingCatalogue());

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            Space space_Bedroom = adjacencyCluster.GetSpaces().Find(x => x.Name == name_Bedroom);
            Space space_Bathroom = adjacencyCluster.GetSpaces().Find(x => x.Name == name_Bathroom);

            double design_Kitchen_Before = Design(adjacencyCluster, adjacencyCluster.GetSpaces().Find(x => x.Name == name_Kitchen), FlowClassification.Extract);
            double design_Bathroom_Before = Design(adjacencyCluster, space_Bathroom, FlowClassification.Extract);

            DwellingDesignAirFlowChange change = adjacencyCluster.ApplyTargetedDesignAirFlow(space_Bedroom, FlowClassification.Supply, Design(adjacencyCluster, space_Bedroom, FlowClassification.Supply) + 4);

            Assert.True(change.Successful, string.Join(" ", change.Refusals));

            DesignAirFlowAdjustment adjustment = Assert.Single(change.DerivedAdjustments);

            Assert.Equal(name_Kitchen, adjustment.SpaceName);
            Assert.Equal(design_Kitchen_Before + 4, Design(adjacencyCluster, adjacencyCluster.GetSpaces().Find(x => x.Name == name_Kitchen), FlowClassification.Extract), 6);

            Assert.Equal(design_Bathroom_Before, Design(adjacencyCluster, space_Bathroom, FlowClassification.Extract), 6);

            Assert.Contains(change.Notes, x => x.Contains("cooking-priority"));
        }

        /// <summary>
        /// <b>An unreconciled supply-only state is invalid, and stays invalid.</b> A mechanical
        /// ventilation with heat recovery unit moves the air it takes in; 4 l/s more supply with no more
        /// extract is a dwelling that gains air it never loses, and TAS refuses to simulate one.
        /// Iteration 1a's conservation check catches it, and nothing quietly adjusts a terminal to close
        /// the difference.
        /// <para>
        /// This is the low-level invariant that makes the transaction necessary.
        /// <c>Modify.SetSpaceDesignFlowRate</c> writes exactly what it is told and does not rebalance -
        /// which is correct for a primitive - so a caller that uses it alone gets this. The operation that
        /// produces a valid design is <c>Modify.ApplyTargetedDesignAirFlow</c>.
        /// </para>
        /// </summary>
        [Fact]
        public void ASupplyOnlyDesignChange_RefusesRatherThanUnbalancingTheDwelling()
        {
            PartOIterationPreparation preparation = Prepared(DwellingCatalogue());

            PartOIterationPreparation preparation_After = Prepare(Changed(preparation, name_Bedroom, FlowClassification.Supply, 4), DwellingCatalogue());

            Assert.NotNull(preparation_After.Refusal);
            Assert.Contains("balance", preparation_After.Refusal);
        }

        /// <summary>
        /// Approved Document F requirements are immutable through the whole transaction - for the room
        /// that was targeted and for every room that moved as a consequence. The design floats above them;
        /// they do not follow it up.
        /// </summary>
        [Fact]
        public void ATargetedChange_LeavesEveryPartFRequirementUnchanged()
        {
            PartOIterationPreparation preparation = Prepared(DwellingCatalogue());

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            Dictionary<string, string> requirements_Before = Requirements(adjacencyCluster);

            DwellingDesignAirFlowChange change = Retarget(adjacencyCluster, name_Bedroom, FlowClassification.Supply, 4);

            Assert.Equal(requirements_Before, Requirements(adjacencyCluster));

            //And every adjustment - targeted and derived alike - still sits at or above its own floor.
            foreach (DesignAirFlowAdjustment adjustment in change.Adjustments)
            {
                Assert.True(double.IsNaN(adjustment.Requirement_Lps) || adjustment.After_Lps >= adjustment.Requirement_Lps - 0.001, adjustment.ToString());
            }

            //The bedroom's requirement is what it always was, whatever its design now says.
            Space space_Bedroom = adjacencyCluster.GetSpaces().Find(x => x.Name == name_Bedroom);

            Assert.Equal(change.TargetedAdjustment.Requirement_Lps, adjacencyCluster.PartFRequiredFlowRate_Lps(space_Bedroom, FlowClassification.Supply).Value, 6);
            Assert.True(change.TargetedAdjustment.After_Lps > change.TargetedAdjustment.Requirement_Lps);
        }

        /// <summary>
        /// A targeted change the selected unit can still absorb leaves the selection alone. The dwelling
        /// grew; the plant did not have to.
        /// </summary>
        [Fact]
        public void ATargetedChangeWithinCapacity_KeepsTheSelectedUnit()
        {
            AdjacencyCluster adjacencyCluster = Selected(out AirHandlingUnit airHandlingUnit, out VentilationUnitReference ventilationUnitReference);

            Retarget(adjacencyCluster, name_Bedroom, FlowClassification.Supply, 4);

            adjacencyCluster.AirHandlingUnitDesignDuty(airHandlingUnit, out double supplyDuty_Lps, out double extractDuty_Lps);

            Assert.Equal(supplyDuty_Lps, extractDuty_Lps, 6);
            Assert.True(supplyDuty_Lps < 25);

            Assert.True(adjacencyCluster.IsVentilationUnitSufficient(airHandlingUnit, DwellingCatalogue(), out string reason), reason);
            Assert.True(ventilationUnitReference.Matches(airHandlingUnit.SelectedVentilationUnitReference()));
        }

        /// <summary>
        /// A targeted change whose rebalanced duty passes the selected unit's rating exposes the
        /// exhaustion, and re-selecting escalates from the rebalanced duty. The requirement is not
        /// consulted and does not move - what grew is the design.
        /// </summary>
        [Fact]
        public void ATargetedChangeBeyondCapacity_ExposesExhaustionAndEscalates()
        {
            AdjacencyCluster adjacencyCluster = Selected(out AirHandlingUnit airHandlingUnit, out _);

            VentilationSystem ventilationSystem = Assert.Single(adjacencyCluster.GetObjects<VentilationSystem>());

            adjacencyCluster.PartFRequiredSystemDuty(ventilationSystem, out double requirement_Before_Lps, out _);

            Retarget(adjacencyCluster, name_Bedroom, FlowClassification.Supply, 12);

            adjacencyCluster.AirHandlingUnitDesignDuty(airHandlingUnit, out double supplyDuty_Lps, out double extractDuty_Lps);

            Assert.Equal(supplyDuty_Lps, extractDuty_Lps, 6);
            Assert.True(supplyDuty_Lps > 25);

            Assert.False(adjacencyCluster.IsVentilationUnitSufficient(airHandlingUnit, DwellingCatalogue(), out string reason));
            Assert.Contains("exhausted", reason);

            Assert.Equal("MVHR-35", adjacencyCluster.SelectVentilationUnit(airHandlingUnit, DwellingCatalogue(), out _, out List<string> refusals).VentilationUnitReference?.Model);
            Assert.Empty(refusals);

            adjacencyCluster.PartFRequiredSystemDuty(ventilationSystem, out double requirement_After_Lps, out _);

            Assert.Equal(requirement_Before_Lps, requirement_After_Lps, 6);
        }

        /// <summary>
        /// <b>A targeted change is reversible, and reversing it restores the original design exactly.</b>
        /// <para>
        /// The bedroom is raised by 10 l/s and the cooking-priority strategy balances it with 10 l/s of
        /// kitchen extract. Taking that kitchen extract back to its Approved Document F requirement has an
        /// obvious valid answer - remove the 10 l/s from the bedroom, which is holding exactly that much
        /// design headroom - and the operation must find it.
        /// </para>
        /// <para>
        /// <b>This is a real defect that was found in review.</b> An earlier revision shared a
        /// reduction in proportion to each room's total duty, which handed a share to the living room
        /// sitting exactly on its own floor, saw that share breach it, and refused the whole reversal as
        /// impossible. A reduction can only come out of headroom that is there to remove, so it is now
        /// shared in proportion to <c>max(0, duty - requirement)</c> and a room at its floor is never
        /// asked.
        /// </para>
        /// </summary>
        [Fact]
        public void AReductionConsumesAvailableHeadroom_AndReversesATargetedChangeExactly()
        {
            PartOIterationPreparation preparation = Prepared(DwellingCatalogue());

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            Dictionary<string, double> design_Original = Designs(adjacencyCluster);
            Dictionary<string, string> requirements_Original = Requirements(adjacencyCluster);

            double design_LivingRoom_Floor_Lps = Design(adjacencyCluster, adjacencyCluster.GetSpaces().Find(x => x.Name == name_LivingRoom), FlowClassification.Supply);

            //Out: the bedroom gains 10 l/s and the kitchen derives the matching extract.
            Retarget(adjacencyCluster, name_Bedroom, FlowClassification.Supply, 10);

            //And back: the kitchen returns to its Approved Document F requirement.
            Space space_Kitchen = adjacencyCluster.GetSpaces().Find(x => x.Name == name_Kitchen);

            double requirement_Kitchen_Lps = adjacencyCluster.PartFRequiredFlowRate_Lps(space_Kitchen, FlowClassification.Extract).Value;

            DwellingDesignAirFlowChange change = adjacencyCluster.ApplyTargetedDesignAirFlow(space_Kitchen, FlowClassification.Extract, requirement_Kitchen_Lps);

            Assert.True(change.Successful, string.Join(" ", change.Refusals));

            //The bedroom's headroom is what paid for it, and it is the only room that moved.
            DesignAirFlowAdjustment adjustment = Assert.Single(change.DerivedAdjustments);

            Assert.Equal(name_Bedroom, adjustment.SpaceName);
            Assert.Equal(FlowClassification.Supply, adjustment.FlowClassification);
            Assert.Equal(-10, adjustment.Change_Lps, 6);

            //The living room sat exactly on its Part F floor throughout and was never asked to give up air
            //it did not have.
            Assert.Equal(design_LivingRoom_Floor_Lps, Design(adjacencyCluster, adjacencyCluster.GetSpaces().Find(x => x.Name == name_LivingRoom), FlowClassification.Supply), 6);

            //The whole dwelling is back where it started, still balanced, with every requirement untouched.
            Assert.Equal(design_Original, Designs(adjacencyCluster));
            Assert.Equal(requirements_Original, Requirements(adjacencyCluster));
            Assert.Equal(change.SupplyDuty_Lps, change.ExtractDuty_Lps, 6);
        }

        /// <summary>
        /// <b>A reduction larger than all the headroom there is refuses, and writes nothing.</b>
        /// <para>
        /// <b>Built by hand, and that is the honest way to build it.</b> On a dwelling the real
        /// <c>PartFCalculator</c> sized, the two sides have equal requirement totals and equal design
        /// totals, so each side always holds exactly the same removable headroom as the other and a
        /// reduction can always be balanced - which is the point of the fix above. Reaching the refusal at
        /// all needs a system whose extract requirement floor sits close under its design while the supply
        /// side has slack, so the fixture states that directly rather than pretending a Part F run
        /// produced it.
        /// </para>
        /// </summary>
        [Fact]
        public void AReductionBeyondAllAvailableHeadroom_RefusesAndWritesNothing()
        {
            AdjacencyCluster adjacencyCluster = HeadroomFixture(out Space space_Supply, out _);

            Dictionary<string, double> design_Before = Designs(adjacencyCluster);

            //Supply 30 -> 10 needs 20 l/s off the extract side, which holds only 2 l/s above its floor.
            DwellingDesignAirFlowChange change = adjacencyCluster.ApplyTargetedDesignAirFlow(space_Supply, FlowClassification.Supply, 10);

            Assert.False(change.Successful);
            Assert.NotEmpty(change.Refusals);
            Assert.Contains("headroom", change.Refusals[0]);

            //Nothing written: not the derived rooms, and not the targeted one either.
            Assert.Null(change.TargetedAdjustment);
            Assert.Empty(change.DerivedAdjustments);
            Assert.Equal(design_Before, Designs(adjacencyCluster));
        }

        /// <summary>
        /// <b>A reduction the headroom can just cover succeeds</b> - the boundary of the test above, so
        /// the refusal is known to be about the shortfall and not about reductions in general.
        /// </summary>
        [Fact]
        public void AReductionExactlyAtAvailableHeadroom_Succeeds()
        {
            AdjacencyCluster adjacencyCluster = HeadroomFixture(out Space space_Supply, out Space space_Extract);

            DwellingDesignAirFlowChange change = adjacencyCluster.ApplyTargetedDesignAirFlow(space_Supply, FlowClassification.Supply, 28);

            Assert.True(change.Successful, string.Join(" ", change.Refusals));

            Assert.Equal(28, Design(adjacencyCluster, space_Supply, FlowClassification.Supply), 6);
            Assert.Equal(28, Design(adjacencyCluster, space_Extract, FlowClassification.Extract), 6);
            Assert.Equal(change.SupplyDuty_Lps, change.ExtractDuty_Lps, 6);
        }

        /// <summary>
        /// <b>A targeted change never reaches across into another ventilation system.</b>
        /// <para>
        /// <b>A second defect found in review.</b> A duty is summed per room and per direction, and
        /// <c>Modify.SetSpaceDesignFlowRate</c> writes every terminal of that room and direction - so a
        /// room holding terminals from this Part O system and from another one would have had both
        /// rewritten, silently moving the other system's design duty while the result claimed the change
        /// belonged to this one.
        /// </para>
        /// <para>
        /// Refused rather than filtered: writing only the subset that belongs here needs a system-scoped
        /// setter that does not exist, and inventing one would be a multi-system allocation architecture
        /// Iteration 2 has no business introducing.
        /// </para>
        /// </summary>
        [Fact]
        public void ASpaceSharedWithAnotherVentilationSystem_RefusesAndTouchesNeitherSystem()
        {
            PartOIterationPreparation preparation = Prepared(DwellingCatalogue());

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            //A second, unrelated ventilation system with an extract terminal of its own in the kitchen -
            //the room the Part O system's balancing consequence would otherwise land in.
            VentilationSystem ventilationSystem_Other = new("Other", new VentilationSystemType("Other MV", "Fixture other system"));

            Space space_Kitchen = adjacencyCluster.GetSpaces().Find(x => x.Name == name_Kitchen);

            VentilationTerminal ventilationTerminal_Other = new("Other extract", FlowClassification.Extract, 7);

            adjacencyCluster.AddObject(ventilationSystem_Other);
            adjacencyCluster.AddObject(ventilationTerminal_Other);
            adjacencyCluster.AddRelation(ventilationTerminal_Other, space_Kitchen);
            adjacencyCluster.AddRelation(ventilationTerminal_Other, ventilationSystem_Other);

            Dictionary<string, double> design_Before = Designs(adjacencyCluster);
            Dictionary<Guid, double> terminals_Before = TerminalDesigns(adjacencyCluster);

            Space space_Bedroom = adjacencyCluster.GetSpaces().Find(x => x.Name == name_Bedroom);

            DwellingDesignAirFlowChange change = adjacencyCluster.ApplyTargetedDesignAirFlow(space_Bedroom, FlowClassification.Supply, Design(adjacencyCluster, space_Bedroom, FlowClassification.Supply) + 4);

            Assert.False(change.Successful);
            Assert.NotEmpty(change.Refusals);
            Assert.Contains(name_Kitchen, change.Refusals[0]);
            Assert.Contains("not all part of ventilation system", change.Refusals[0]);

            //Every terminal in BOTH systems is value-identical, and neither system's duty moved.
            Assert.Equal(terminals_Before, TerminalDesigns(adjacencyCluster));
            Assert.Equal(design_Before, Designs(adjacencyCluster));

            Assert.Null(change.TargetedAdjustment);
            Assert.Empty(change.DerivedAdjustments);
        }

        /// <summary>
        /// The same validation catches a terminal belonging to no ventilation system at all - nothing says
        /// it is part of this dwelling, so writing it would be a guess.
        /// </summary>
        [Fact]
        public void ASpaceHoldingAnOrphanTerminal_Refuses()
        {
            PartOIterationPreparation preparation = Prepared(DwellingCatalogue());

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            Space space_Bedroom = adjacencyCluster.GetSpaces().Find(x => x.Name == name_Bedroom);

            VentilationTerminal ventilationTerminal_Orphan = new("Orphan supply", FlowClassification.Supply, 3);

            adjacencyCluster.AddObject(ventilationTerminal_Orphan);
            adjacencyCluster.AddRelation(ventilationTerminal_Orphan, space_Bedroom);

            Dictionary<Guid, double> terminals_Before = TerminalDesigns(adjacencyCluster);

            DwellingDesignAirFlowChange change = adjacencyCluster.ApplyTargetedDesignAirFlow(space_Bedroom, FlowClassification.Supply, Design(adjacencyCluster, space_Bedroom, FlowClassification.Supply) + 4);

            Assert.False(change.Successful);
            Assert.NotEmpty(change.Refusals);
            Assert.Contains("belongs to no ventilation system", change.Refusals[0]);

            Assert.Equal(terminals_Before, TerminalDesigns(adjacencyCluster));
        }

        /// <summary>
        /// <b>An already-unbalanced dwelling is refused before anything is written, not reported as a
        /// success with a warning.</b>
        /// <para>
        /// A targeted change and its derived consequence move both sides by the same amount, so they
        /// preserve a pre-existing residual rather than closing it. An earlier revision noticed the
        /// residual after writing and only warned, leaving <c>Successful</c> true - a result claiming a
        /// valid balanced design for a dwelling that gains air it never loses.
        /// </para>
        /// </summary>
        [Fact]
        public void AnAlreadyUnbalancedDwelling_RefusesBeforeWritingAnything()
        {
            PartOIterationPreparation preparation = Prepared(DwellingCatalogue());

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            //The low-level primitive writes exactly what it is told and does not rebalance, which is how an
            //unbalanced dwelling gets created in the first place.
            Space space_LivingRoom = adjacencyCluster.GetSpaces().Find(x => x.Name == name_LivingRoom);

            Assert.NotNull(adjacencyCluster.SetSpaceDesignFlowRate(space_LivingRoom, FlowClassification.Supply, Design(adjacencyCluster, space_LivingRoom, FlowClassification.Supply) + 6, out _, out List<string> refusals_Setup));
            Assert.Empty(refusals_Setup);

            Dictionary<Guid, double> terminals_Before = TerminalDesigns(adjacencyCluster);

            Space space_Bedroom = adjacencyCluster.GetSpaces().Find(x => x.Name == name_Bedroom);

            DwellingDesignAirFlowChange change = adjacencyCluster.ApplyTargetedDesignAirFlow(space_Bedroom, FlowClassification.Supply, Design(adjacencyCluster, space_Bedroom, FlowClassification.Supply) + 4);

            Assert.False(change.Successful);
            Assert.NotEmpty(change.Refusals);
            Assert.Contains("already designs", change.Refusals[0]);

            Assert.Null(change.TargetedAdjustment);
            Assert.Empty(change.DerivedAdjustments);
            Assert.Equal(terminals_Before, TerminalDesigns(adjacencyCluster));
        }

        /// <summary>
        /// Every successful transaction leaves the dwelling balanced, and says nothing to the contrary.
        /// Stated as its own fact, because "balanced afterwards" is the contract rather than an incidental
        /// property of one scenario.
        /// </summary>
        [Fact]
        public void EverySuccessfulTransaction_LeavesTheDwellingBalanced()
        {
            PartOIterationPreparation preparation = Prepared(DwellingCatalogue());

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            foreach ((string name_Space, FlowClassification flowClassification, double change_Lps) in new[]
            {
                (name_Bedroom, FlowClassification.Supply, 6.0),
                (name_Bathroom, FlowClassification.Extract, 3.0),
                (name_Bedroom, FlowClassification.Supply, -2.0),
            })
            {
                DwellingDesignAirFlowChange change = Retarget(adjacencyCluster, name_Space, flowClassification, change_Lps);

                Assert.Equal(change.SupplyDuty_Lps, change.ExtractDuty_Lps, 6);
                Assert.Empty(change.Warnings);
            }
        }

        /// <summary>
        /// A dwelling with nothing on the other side to balance against is refused rather than left
        /// gaining air it never loses.
        /// <para>
        /// <b>Built at zero on both sides, so the dwelling starts balanced.</b> Stripping the extract
        /// terminals out of a sized dwelling instead would leave supply against no extract at all, and the
        /// pre-write balance check would - correctly - refuse that for being unbalanced before this rule
        /// was ever reached. Starting from nothing isolates the rule under test.
        /// </para>
        /// </summary>
        [Fact]
        public void ATargetedChangeWithNothingToBalanceAgainst_Refuses()
        {
            AdjacencyCluster adjacencyCluster = new();

            Space space = Room(adjacencyCluster, "Supply Room", PartFTerminalRole.Supply, 0);

            VentilationSystem ventilationSystem = new("Fixture", new VentilationSystemType("Fixture MVHR", "Fixture"));

            adjacencyCluster.AddObject(ventilationSystem);

            Terminal(adjacencyCluster, ventilationSystem, space, FlowClassification.Supply, 0);

            adjacencyCluster.AddRelation(ventilationSystem, space);

            Dictionary<Guid, double> terminals_Before = TerminalDesigns(adjacencyCluster);

            DwellingDesignAirFlowChange change = adjacencyCluster.ApplyTargetedDesignAirFlow(space, FlowClassification.Supply, 4);

            Assert.False(change.Successful);
            Assert.NotEmpty(change.Refusals);
            Assert.Contains("gain air it never loses", change.Refusals[0]);

            Assert.Equal(terminals_Before, TerminalDesigns(adjacencyCluster));
        }

        /// <summary>
        /// The transfer air the dwelling routes follows the changed rooms. The extra 4 l/s supplied into
        /// the bedroom has to leave it, so the bedroom's outgoing transfer air grows by exactly that, and
        /// a room whose design did not change does not move.
        /// </summary>
        [Fact]
        public void ALocalDesignChange_MovesTheTransferAirItShould()
        {
            PartOIterationPreparation preparation = Prepared(DwellingCatalogue());

            double transfer_Bedroom_Before = TransferOut(preparation, name_Bedroom);
            double transfer_Kitchen_Before = TransferOut(preparation, name_Kitchen);

            PartOIterationPreparation preparation_After = Prepare(Retargeted(preparation, name_Bedroom, FlowClassification.Supply, 4), DwellingCatalogue());

            Assert.Null(preparation_After.Refusal);

            Assert.Equal(transfer_Bedroom_Before + 4, TransferOut(preparation_After, name_Bedroom), 3);

            //The kitchen was neither supplied more nor extracted more, so it passes on exactly what it did.
            Assert.Equal(transfer_Kitchen_Before, TransferOut(preparation_After, name_Kitchen), 3);
        }

        /// <summary>
        /// The unit's design duty follows the recalculated network exactly - it is derived from the
        /// terminals every time it is asked for, so it cannot be the stale answer a stored duty would be.
        /// </summary>
        [Fact]
        public void TheAirHandlingUnitDesignDuty_FollowsTheRecalculatedNetwork()
        {
            PartOIterationPreparation preparation = Prepared(DwellingCatalogue());

            PartOIterationPreparation preparation_After = Prepare(Retargeted(preparation, name_Bedroom, FlowClassification.Supply, 4), DwellingCatalogue());

            Assert.Null(preparation_After.Refusal);

            AdjacencyCluster adjacencyCluster = preparation_After.AnalyticalModel.AdjacencyCluster;

            AirHandlingUnit airHandlingUnit = Assert.Single(adjacencyCluster.GetObjects<AirHandlingUnit>());

            adjacencyCluster.AirHandlingUnitDesignDuty(airHandlingUnit, out double supplyDuty_Lps, out double extractDuty_Lps);

            Assert.Equal(preparation_After.DesignSupplyDuty_Lps, supplyDuty_Lps, 6);
            Assert.Equal(preparation_After.DesignExtractDuty_Lps, extractDuty_Lps, 6);

            //And it now stands above the Approved Document F requirement, which has not moved.
            VentilationSystem ventilationSystem = Assert.Single(adjacencyCluster.GetObjects<VentilationSystem>());

            adjacencyCluster.PartFRequiredSystemDuty(ventilationSystem, out double requirement_Supply_Lps, out _);

            Assert.Equal(preparation.DesignSupplyDuty_Lps, requirement_Supply_Lps, 6);
            Assert.True(supplyDuty_Lps > requirement_Supply_Lps);
        }

        // =================================================================================================
        // F. Capacity: valid, exactly at, and exhausted
        // =================================================================================================

        /// <summary>
        /// A design duty below the selected product's rating keeps that product. The remaining capacity is
        /// headroom, not a shortfall - a duty under the rating is a finished answer.
        /// </summary>
        [Fact]
        public void DesignDutyBelowTheSelectedMaximum_KeepsTheUnit()
        {
            AdjacencyCluster adjacencyCluster = Selected(out AirHandlingUnit airHandlingUnit, out VentilationUnitReference ventilationUnitReference);

            adjacencyCluster.AirHandlingUnitDesignDuty(airHandlingUnit, out double supplyDuty_Lps, out _);

            Assert.True(supplyDuty_Lps < 25);

            Assert.True(adjacencyCluster.IsVentilationUnitSufficient(airHandlingUnit, DwellingCatalogue(), out string reason), reason);
            Assert.Null(reason);

            //Raised, and still under the rating: the unit is untouched and there is nothing to re-select.
            RaiseDutyTotalTo(adjacencyCluster, airHandlingUnit, 24);

            Assert.True(adjacencyCluster.IsVentilationUnitSufficient(airHandlingUnit, DwellingCatalogue(), out _));
            Assert.True(ventilationUnitReference.Matches(airHandlingUnit.SelectedVentilationUnitReference()));
        }

        /// <summary>A design duty exactly at the selected product's rating is still valid.</summary>
        [Fact]
        public void DesignDutyExactlyAtTheSelectedMaximum_RemainsValid()
        {
            AdjacencyCluster adjacencyCluster = Selected(out AirHandlingUnit airHandlingUnit, out VentilationUnitReference ventilationUnitReference);

            RaiseDutyTotalTo(adjacencyCluster, airHandlingUnit, 25);

            Assert.True(adjacencyCluster.IsVentilationUnitSufficient(airHandlingUnit, DwellingCatalogue(), out string reason), reason);

            //And re-selecting at exactly the rating chooses the same unit, not the next one up.
            Assert.Equal("MVHR-25", adjacencyCluster.SelectVentilationUnit(airHandlingUnit, DwellingCatalogue(), out _, out _).VentilationUnitReference?.Model);
            Assert.True(ventilationUnitReference.Matches(airHandlingUnit.SelectedVentilationUnitReference()));
        }

        /// <summary>
        /// Past the rating the unit is <b>exhausted</b>: the check fails, says so by name, and re-selecting
        /// from the CURRENT design duty escalates to the next product up. The Approved Document F
        /// requirement is neither reset nor consulted - what grew is the design.
        /// </summary>
        [Fact]
        public void DesignDutyAboveTheSelectedMaximum_ExhaustsTheUnitAndEscalates()
        {
            AdjacencyCluster adjacencyCluster = Selected(out AirHandlingUnit airHandlingUnit, out _);

            VentilationSystem ventilationSystem = Assert.Single(adjacencyCluster.GetObjects<VentilationSystem>());

            adjacencyCluster.PartFRequiredSystemDuty(ventilationSystem, out double requirement_Supply_Before_Lps, out _);

            RaiseDutyTotalTo(adjacencyCluster, airHandlingUnit, 30);

            Assert.False(adjacencyCluster.IsVentilationUnitSufficient(airHandlingUnit, DwellingCatalogue(), out string reason));
            Assert.Contains("exhausted", reason);
            Assert.Contains("MVHR-25", reason);

            //Escalation: the next compliant product, chosen from the current design duty.
            VentilationUnitSelection selection = adjacencyCluster.SelectVentilationUnit(airHandlingUnit, DwellingCatalogue(), out _, out List<string> refusals);

            Assert.Empty(refusals);
            Assert.Equal("MVHR-35", selection.VentilationUnitReference?.Model);
            Assert.True(adjacencyCluster.IsVentilationUnitSufficient(airHandlingUnit, DwellingCatalogue(), out _));

            //And the requirement did not move while the design grew past a unit size.
            adjacencyCluster.PartFRequiredSystemDuty(ventilationSystem, out double requirement_Supply_After_Lps, out _);

            Assert.Equal(requirement_Supply_Before_Lps, requirement_Supply_After_Lps, 6);
            Assert.True(requirement_Supply_After_Lps < 25);
        }

        /// <summary>
        /// A dwelling nothing in the catalogue can serve is refused by name, and no product is written onto
        /// its unit - an undersized selection is never an answer.
        /// </summary>
        [Fact]
        public void ADwellingNothingCanServe_RefusesAndSelectsNothing()
        {
            AdjacencyCluster adjacencyCluster = Selected(out AirHandlingUnit airHandlingUnit, out _);

            RaiseDutyTotalTo(adjacencyCluster, airHandlingUnit, 60);

            VentilationUnitSelection selection = adjacencyCluster.SelectVentilationUnit(airHandlingUnit, DwellingCatalogue(), out _, out List<string> refusals);

            Assert.False(selection.IsSelected);
            Assert.Single(refusals);
            Assert.Contains(airHandlingUnit.Name, refusals[0]);

            //Nothing was written: the unit keeps the product it had rather than being quietly downgraded.
            Assert.Equal("MVHR-25", airHandlingUnit.SelectedVentilationUnitReference()?.Model);
        }

        // =================================================================================================
        // G. Authority separation at the runtime boundary
        // =================================================================================================

        /// <summary>
        /// <b>A design airflow change writes nothing runtime.</b> No profile is assigned, no internal
        /// condition airflow moves, and no simulation operating state is touched - turning a design number
        /// into an operating one is a separate, explicit step and is not Iteration 2's.
        /// </summary>
        [Fact]
        public void ChangingDesignAirFlow_WritesNoRuntimeAirflow()
        {
            PartOIterationPreparation preparation = Prepared(DwellingCatalogue());

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            Space space = adjacencyCluster.GetSpaces().Find(x => x.Name == name_Bedroom);

            string json_Before = Core.Convert.ToString(space.InternalCondition);

            Assert.NotNull(adjacencyCluster.SetSpaceDesignFlowRate(space, FlowClassification.Supply, Design(adjacencyCluster, space, FlowClassification.Supply) + 4, out _, out _));

            Space space_After = adjacencyCluster.GetSpaces().Find(x => x.Name == name_Bedroom);

            Assert.Equal(json_Before, Core.Convert.ToString(space_After.InternalCondition));
        }

        /// <summary>
        /// Selecting a product writes nothing runtime either - the unit gains an identity and nothing else
        /// about the model moves.
        /// </summary>
        [Fact]
        public void SelectingAUnit_WritesNoRuntimeAirflow()
        {
            AnalyticalModel analyticalModel = Model();

            PartOIterationPreparation preparation_Generic = Prepare(analyticalModel, null);
            PartOIterationPreparation preparation_Selected = Prepare(analyticalModel, DwellingCatalogue());

            Assert.Null(preparation_Selected.Refusal);

            Assert.Equal(RuntimeAirflows(preparation_Generic), RuntimeAirflows(preparation_Selected));
        }

        // =================================================================================================
        // H. Preconditions - the dwelling has to be valid before it can be changed
        // =================================================================================================

        /// <summary>
        /// <b>Balance is not compliance, and a globally balanced dwelling can still be illegal.</b>
        /// <para>
        /// The bathroom is designed at 5 l/s against a 10 l/s requirement and the kitchen at 15 against 10,
        /// so extract totals 20 against 20 l/s of supply and the dwelling balances perfectly - while the
        /// bathroom sits below its Approved Document F floor. Raising a bedroom by 1 l/s would derive 1 l/s
        /// of kitchen extract under cooking priority, never touch the bathroom, and report success: a
        /// transaction claiming a valid design for a dwelling that was never compliant.
        /// </para>
        /// <para>
        /// The precondition now checks every served room against its own requirement, through the same
        /// <c>Query.ReconcileVentilationSystemDesignDuty</c> the preparation refuses on, so there is one
        /// definition of compliant. The existing shortfall is <b>refused, never repaired</b> - quietly
        /// fixing a room nobody targeted would be an unrequested design decision.
        /// </para>
        /// </summary>
        [Fact]
        public void ARoomAlreadyBelowItsPartFFloor_RefusesBeforeWritingAnything()
        {
            AdjacencyCluster adjacencyCluster = ShortfallFixture(out Space space_Supply, out _, out _);

            Dictionary<Guid, double> terminals_Before = TerminalDesigns(adjacencyCluster);
            Dictionary<string, string> requirements_Before = Requirements(adjacencyCluster);

            //A perfectly ordinary, legal target elsewhere in the dwelling.
            DwellingDesignAirFlowChange change = adjacencyCluster.ApplyTargetedDesignAirFlow(space_Supply, FlowClassification.Supply, 21);

            Assert.False(change.Successful);
            Assert.NotEmpty(change.Refusals);

            //The deficient room is named, with both numbers.
            Assert.Contains("Bathroom", change.Refusals[0]);
            Assert.Contains("5", change.Refusals[0]);
            Assert.Contains("10", change.Refusals[0]);

            Assert.Null(change.TargetedAdjustment);
            Assert.Empty(change.DerivedAdjustments);
            Assert.Equal(terminals_Before, TerminalDesigns(adjacencyCluster));
            Assert.Equal(requirements_Before, Requirements(adjacencyCluster));
        }

        /// <summary>
        /// The boundary of the test above: with the bathroom brought up to its floor the dwelling is
        /// compliant, and the very same targeted change proceeds normally. So the refusal is about the
        /// shortfall and not about the fixture.
        /// </summary>
        [Fact]
        public void TheSameDwellingOnceCompliant_ProceedsNormally()
        {
            AdjacencyCluster adjacencyCluster = ShortfallFixture(out Space space_Supply, out Space space_Bathroom, out Space space_Kitchen);

            //Bathroom 5 -> 10 and kitchen 15 -> 10: every room now at its requirement, still balanced at 20.
            Assert.NotNull(adjacencyCluster.SetSpaceDesignFlowRate(space_Bathroom, FlowClassification.Extract, 10, out _, out List<string> refusals_Bathroom));
            Assert.Empty(refusals_Bathroom);

            Assert.NotNull(adjacencyCluster.SetSpaceDesignFlowRate(space_Kitchen, FlowClassification.Extract, 10, out _, out List<string> refusals_Kitchen));
            Assert.Empty(refusals_Kitchen);

            DwellingDesignAirFlowChange change = adjacencyCluster.ApplyTargetedDesignAirFlow(space_Supply, FlowClassification.Supply, 21);

            Assert.True(change.Successful, string.Join(" ", change.Refusals));
            Assert.Equal(21, change.SupplyDuty_Lps, 6);
            Assert.Equal(21, change.ExtractDuty_Lps, 6);
        }

        /// <summary>
        /// <b>A tolerance that cannot be compared against is refused, not worked around.</b>
        /// <para>
        /// Every Iteration 2 safety rule is a comparison against the tolerance, so <c>NaN</c> switches the
        /// derived allocation, the imbalance refusal and the capacity check off at once and the result
        /// reports success on an unbalanced dwelling. An infinity does the same wearing the opposite mask.
        /// </para>
        /// </summary>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(-1.0)]
        public void AnUnusableTolerance_RefusesAndWritesNothing(double tolerance_Lps)
        {
            PartOIterationPreparation preparation = Prepared(DwellingCatalogue());

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            Dictionary<Guid, double> terminals_Before = TerminalDesigns(adjacencyCluster);

            Space space_Bedroom = adjacencyCluster.GetSpaces().Find(x => x.Name == name_Bedroom);

            DwellingDesignAirFlowChange change = adjacencyCluster.ApplyTargetedDesignAirFlow(space_Bedroom, FlowClassification.Supply, Design(adjacencyCluster, space_Bedroom, FlowClassification.Supply) + 4, PartFExtractAllocationStrategy.MinimumFirstCookingPriority, tolerance_Lps);

            Assert.False(change.Successful);
            Assert.NotEmpty(change.Refusals);
            Assert.Contains("tolerance", change.Refusals[0]);

            Assert.Null(change.TargetedAdjustment);
            Assert.Empty(change.DerivedAdjustments);
            Assert.Equal(terminals_Before, TerminalDesigns(adjacencyCluster));
        }

        /// <summary>
        /// The same guard on the selection path, which is where an unusable tolerance would otherwise
        /// accept an undersized unit: <c>NaN</c> makes "is 100 enough for 150?" evaluate false in the
        /// direction that lets it through.
        /// </summary>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(-1.0)]
        public void AnUnusableTolerance_NeverAcceptsAnUndersizedUnit(double tolerance_Lps)
        {
            //Selection refuses outright rather than choosing.
            VentilationUnitSelection selection = Catalogue().SelectSmallestCapableVentilationUnit(150, 150, tolerance_Lps);

            Assert.False(selection.IsSelected);
            Assert.Contains("tolerance", selection.Reason);

            //And nothing is offered as compliant, so no caller can pick one off the list either.
            Assert.Empty(Catalogue().CapableVentilationUnits(150, 150, tolerance_Lps));

            //Including the predicate underneath both of them.
            Assert.False(Descriptor("MVHR-220", 220, 220).IsSufficientFor(150, 150, tolerance_Lps));

            //And an already-selected unit is never reported as adequate on an unusable tolerance.
            AdjacencyCluster adjacencyCluster = Selected(out AirHandlingUnit airHandlingUnit, out _);

            Assert.False(adjacencyCluster.IsVentilationUnitSufficient(airHandlingUnit, DwellingCatalogue(), out string reason, tolerance_Lps));
            Assert.Contains("tolerance", reason);
        }

        /// <summary>
        /// <b>A negative duty is met by nothing, not by everything.</b>
        /// <para>
        /// A capacity check is <c>maximum &gt;= duty</c>, so a negative duty is satisfied by every
        /// non-negative capacity and the smallest product on the shelf comes back as a successful answer to
        /// a physically impossible design. A duty that is not a real, non-negative quantity of air is
        /// refused instead - at the selector, in the compliant-set query, and in the predicate underneath
        /// both.
        /// </para>
        /// </summary>
        [Theory]
        [InlineData(-1.0)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NaN)]
        public void AnImpossibleDesignDuty_IsMetByNothing(double duty_Lps)
        {
            VentilationUnitSelection selection = Catalogue().SelectSmallestCapableVentilationUnit(duty_Lps, duty_Lps);

            Assert.False(selection.IsSelected);
            Assert.NotNull(selection.Reason);

            Assert.Empty(Catalogue().CapableVentilationUnits(duty_Lps, duty_Lps));
            Assert.False(Descriptor("MVHR-220", 220, 220).IsSufficientFor(duty_Lps, duty_Lps));

            //One bad side is enough - a valid extract duty does not rescue an impossible supply one.
            Assert.False(Catalogue().SelectSmallestCapableVentilationUnit(duty_Lps, 100).IsSelected);
            Assert.False(Catalogue().SelectSmallestCapableVentilationUnit(100, duty_Lps).IsSelected);
        }

        /// <summary>
        /// <b>A unit the model does not hold is refused, not quietly inserted.</b>
        /// <para>
        /// The duty is resolved through the unit's <i>name</i>, which is how a ventilation system names its
        /// plant - so a detached unit sharing a name with one in the model gets a duty and looks
        /// selectable. Writing the selection onto it and adding it would leave the cluster holding two
        /// units of that name with the product reference on the wrong one, and every name-based lookup
        /// afterwards could resolve the original, unselected unit.
        /// </para>
        /// </summary>
        [Fact]
        public void AnAirHandlingUnitOutsideTheModel_RefusesAndInsertsNothing()
        {
            AdjacencyCluster adjacencyCluster = Selected(out AirHandlingUnit airHandlingUnit, out VentilationUnitReference ventilationUnitReference);

            //A different object with the same name, not in the cluster.
            AirHandlingUnit airHandlingUnit_Detached = Analytical.Create.AirHandlingUnit(airHandlingUnit.Name);

            Assert.NotEqual(airHandlingUnit.Guid, airHandlingUnit_Detached.Guid);

            int count_Before = adjacencyCluster.GetObjects<AirHandlingUnit>().Count;

            VentilationUnitSelection selection = adjacencyCluster.SelectVentilationUnit(airHandlingUnit_Detached, DwellingCatalogue(), out _, out List<string> refusals);

            Assert.False(selection.IsSelected);
            Assert.Single(refusals);
            Assert.Contains("not in this model", refusals[0]);

            //Nothing inserted, and the model's own unit keeps the product it already had.
            Assert.Equal(count_Before, adjacencyCluster.GetObjects<AirHandlingUnit>().Count);
            Assert.Null(airHandlingUnit_Detached.SelectedVentilationUnitReference());
            Assert.True(ventilationUnitReference.Matches(airHandlingUnit.SelectedVentilationUnitReference()));
        }

        /// <summary>
        /// <b>Another system's air never satisfies this system's Part F floor.</b>
        /// <para>
        /// A room can hold terminals belonging to more than one ventilation system. Summing all of them and
        /// calling the total this system's duty would let a foreign terminal cover this system's shortfall:
        /// the room check would pass while this system's own terminal in that room stayed short, and with a
        /// little headroom in a neighbouring room the system total would pass too - both halves of the
        /// compliance check resting on air this system does not move.
        /// </para>
        /// <para>
        /// The per-room duty is therefore filtered to this system. Reads are filtered because the honest
        /// answer is available exactly; <i>writes</i> stay conservative and still refuse a room they cannot
        /// attribute - see <see cref="ASpaceSharedWithAnotherVentilationSystem_RefusesAndTouchesNeitherSystem"/>.
        /// </para>
        /// </summary>
        [Fact]
        public void AForeignSystemsTerminal_NeverSatisfiesThisSystemsPartFFloor()
        {
            AdjacencyCluster adjacencyCluster = ShortfallFixture(out _, out Space space_Bathroom, out _);

            VentilationSystem ventilationSystem = Assert.Single(adjacencyCluster.GetObjects<VentilationSystem>());

            //Before: the bathroom is 5 l/s short of its 10 l/s requirement, and reconciliation says so.
            Assert.False(adjacencyCluster.ReconcileVentilationSystemDesignDuty(ventilationSystem, out _, out _, out List<string> refusals_Before));
            Assert.Contains(refusals_Before, x => x.Contains("Bathroom"));

            //A second system puts its own 5 l/s of extract in the same room. That is the other system's
            //air, and it changes nothing about this system's compliance.
            VentilationSystem ventilationSystem_Other = new("Other", new VentilationSystemType("Other MV", "Fixture other system"));

            VentilationTerminal ventilationTerminal_Other = new("Other extract", FlowClassification.Extract, 5);

            adjacencyCluster.AddObject(ventilationSystem_Other);
            adjacencyCluster.AddObject(ventilationTerminal_Other);
            adjacencyCluster.AddRelation(ventilationTerminal_Other, space_Bathroom);
            adjacencyCluster.AddRelation(ventilationTerminal_Other, ventilationSystem_Other);

            Assert.False(adjacencyCluster.ReconcileVentilationSystemDesignDuty(ventilationSystem, out _, out _, out List<string> refusals_After));
            Assert.Contains(refusals_After, x => x.Contains("Bathroom"));
        }

        /// <summary>
        /// Adequacy is resolved from the model's own unit, so a detached or stale copy cannot report a
        /// product the cluster does not hold as adequate - which would suppress the escalation an outgrown
        /// unit needs.
        /// </summary>
        [Fact]
        public void AdequacyIsResolvedFromTheModelsOwnUnit()
        {
            AdjacencyCluster adjacencyCluster = Selected(out AirHandlingUnit airHandlingUnit, out _);

            //A detached same-named unit carrying a product the model's unit does not have.
            AirHandlingUnit airHandlingUnit_Detached = Analytical.Create.AirHandlingUnit(airHandlingUnit.Name);
            airHandlingUnit_Detached.SetValue(AirHandlingUnitParameter.VentilationUnitReference, new VentilationUnitReference("Test Fixture", "MVHR-50", null));

            Assert.False(adjacencyCluster.IsVentilationUnitSufficient(airHandlingUnit_Detached, DwellingCatalogue(), out string reason));
            Assert.Contains("not in this model", reason);

            //The model's own unit answers normally.
            Assert.True(adjacencyCluster.IsVentilationUnitSufficient(airHandlingUnit, DwellingCatalogue(), out _));
        }

        /// <summary>
        /// A terminal carrying a value that is not a quantity of air cannot have a room total redistributed
        /// across it - the proportional share would compute as <c>NaN</c> - so the change is refused before
        /// anything is written rather than producing a silently wrong duty.
        /// </summary>
        [Fact]
        public void ATerminalCarryingAnImpossibleDuty_RefusesBeforeWriting()
        {
            PartOIterationPreparation preparation = Prepared(DwellingCatalogue());

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            Space space_Bedroom = adjacencyCluster.GetSpaces().Find(x => x.Name == name_Bedroom);

            VentilationTerminal ventilationTerminal = Assert.Single(Analytical.Query.VentilationTerminals(adjacencyCluster.VentilationTerminals(space_Bedroom), FlowClassification.Supply));

            ventilationTerminal.DesignFlowRate_Lps = double.PositiveInfinity;
            adjacencyCluster.AddObject(ventilationTerminal);

            Assert.Null(adjacencyCluster.SetSpaceDesignFlowRate(space_Bedroom, FlowClassification.Supply, 24, out _, out List<string> refusals));

            Assert.NotEmpty(refusals);
            Assert.Contains("not a quantity of air", refusals[0]);

            //Untouched - and emphatically not NaN.
            Assert.Equal(double.PositiveInfinity, ventilationTerminal.DesignFlowRate_Lps.Value);
        }

        /// <summary>
        /// <b>A nonsense terminal in a room the transaction would only touch as a consequence still stops
        /// it before the target is written.</b>
        /// <para>
        /// A room total hides it: <c>VentilationTerminalDesignDuty_Lps</c> skips a <c>NaN</c>, so a room
        /// holding one NaN terminal beside healthy ones sums to a total that meets its requirement and
        /// passes every plan check. Only the setter notices - and if that happened one room in, the target
        /// would already be written and the all-or-nothing promise broken.
        /// </para>
        /// </summary>
        [Fact]
        public void ANonsenseTerminalInADerivedRoom_StopsTheTransactionBeforeTheTargetIsWritten()
        {
            PartOIterationPreparation preparation = Prepared(DwellingCatalogue());

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            //The kitchen is where cooking priority sends the derived extract. Give it a second, nonsense
            //terminal beside its healthy one, so the room TOTAL still reads correctly.
            Space space_Kitchen = adjacencyCluster.GetSpaces().Find(x => x.Name == name_Kitchen);

            VentilationTerminal ventilationTerminal_Nonsense = new("Nonsense extract", FlowClassification.Extract, double.NaN);
            ventilationTerminal_Nonsense.SetValue(VentilationTerminalParameter.PartFTerminalReference, Analytical.Query.VentilationTerminals(adjacencyCluster.VentilationTerminals(space_Kitchen), FlowClassification.Extract)[0].GetValue<PartFTerminalReference>(VentilationTerminalParameter.PartFTerminalReference));

            adjacencyCluster.AddObject(ventilationTerminal_Nonsense);
            adjacencyCluster.AddRelation(ventilationTerminal_Nonsense, space_Kitchen);
            adjacencyCluster.AddRelation(ventilationTerminal_Nonsense, Assert.Single(adjacencyCluster.GetObjects<VentilationSystem>()));

            Dictionary<Guid, double> terminals_Before = TerminalDesigns(adjacencyCluster);

            Space space_Bedroom = adjacencyCluster.GetSpaces().Find(x => x.Name == name_Bedroom);

            DwellingDesignAirFlowChange change = adjacencyCluster.ApplyTargetedDesignAirFlow(space_Bedroom, FlowClassification.Supply, Design(adjacencyCluster, space_Bedroom, FlowClassification.Supply) + 4);

            Assert.False(change.Successful);
            Assert.NotEmpty(change.Refusals);
            Assert.Contains("not a quantity of air", change.Refusals[0]);

            //THE TARGET IS UNTOUCHED - which is the whole point.
            Assert.Null(change.TargetedAdjustment);
            Assert.Empty(change.DerivedAdjustments);
            Assert.Equal(terminals_Before, TerminalDesigns(adjacencyCluster));
        }

        /// <summary>
        /// <b>A change worth making is applied whole, even where its individual shares are each smaller
        /// than the tolerance.</b>
        /// <para>
        /// The tolerance decides whether a <i>change</i> is worth making; it does not get to veto the
        /// pieces that change is made of. With a 1 l/s tolerance a 1.5 l/s change split across two rooms
        /// gives two 0.75 l/s shares - skipping both would write the target, balance nothing, and leave
        /// exactly the partial change this transaction promises never to produce.
        /// </para>
        /// </summary>
        [Fact]
        public void ASubToleranceShare_IsStillApplied()
        {
            AdjacencyCluster adjacencyCluster = new();

            Space space_Supply = Room(adjacencyCluster, "Living Room", PartFTerminalRole.Supply, 10);
            Space space_Extract_1 = Room(adjacencyCluster, "Bathroom", PartFTerminalRole.GeneralExtract, 5);
            Space space_Extract_2 = Room(adjacencyCluster, "Shower Room", PartFTerminalRole.GeneralExtract, 5);

            VentilationSystem ventilationSystem = new("Fixture", new VentilationSystemType("Fixture MVHR", "Fixture"));

            adjacencyCluster.AddObject(ventilationSystem);

            Terminal(adjacencyCluster, ventilationSystem, space_Supply, FlowClassification.Supply, 10);
            Terminal(adjacencyCluster, ventilationSystem, space_Extract_1, FlowClassification.Extract, 5);
            Terminal(adjacencyCluster, ventilationSystem, space_Extract_2, FlowClassification.Extract, 5);

            adjacencyCluster.AddRelation(ventilationSystem, space_Supply);
            adjacencyCluster.AddRelation(ventilationSystem, space_Extract_1);
            adjacencyCluster.AddRelation(ventilationSystem, space_Extract_2);

            //No cooking terminal here, so the derived 1.5 l/s splits evenly: 0.75 each, both under the
            //1 l/s tolerance this call is given.
            DwellingDesignAirFlowChange change = adjacencyCluster.ApplyTargetedDesignAirFlow(space_Supply, FlowClassification.Supply, 11.5, PartFExtractAllocationStrategy.MinimumFirstCookingPriority, 1.0);

            Assert.True(change.Successful, string.Join(" ", change.Refusals));

            Assert.Equal(2, change.DerivedAdjustments.Count);
            Assert.Equal(11.5, change.SupplyDuty_Lps, 6);
            Assert.Equal(11.5, change.ExtractDuty_Lps, 6);

            Assert.Equal(5.75, Design(adjacencyCluster, space_Extract_1, FlowClassification.Extract), 6);
            Assert.Equal(5.75, Design(adjacencyCluster, space_Extract_2, FlowClassification.Extract), 6);
        }

        /// <summary>
        /// A unit with a product but no design duty is <b>unknown</b>, not adequate. Reporting it adequate
        /// would say the plant is fine for a dwelling that currently moves no air, and would suppress the
        /// reselection the model needs.
        /// </summary>
        [Fact]
        public void AUnitWithNoDesignDuty_IsNotReportedAdequate()
        {
            AdjacencyCluster adjacencyCluster = Selected(out AirHandlingUnit airHandlingUnit, out _);

            //Every design terminal disconnected from the system, so the unit's duty derives from nothing.
            VentilationSystem ventilationSystem = Assert.Single(adjacencyCluster.GetObjects<VentilationSystem>());

            foreach (VentilationTerminal ventilationTerminal in adjacencyCluster.GetRelatedObjects<VentilationTerminal>(ventilationSystem) ?? [])
            {
                adjacencyCluster.RemoveRelation(ventilationSystem, ventilationTerminal);
            }

            Assert.False(adjacencyCluster.IsVentilationUnitSufficient(airHandlingUnit, DwellingCatalogue(), out string reason));
            Assert.Contains("no design duty", reason);
        }

        // =================================================================================================
        // I. Catalogue integrity - one identity, one meaning
        // =================================================================================================

        /// <summary>
        /// <b>An identity a catalogue gives two meanings is refused, in either order.</b>
        /// <para>
        /// The model stores only the product's identity and looks its capability up again later by that
        /// identity. Two entries sharing an identity but rated 100/100 and 200/200 leave no single answer
        /// to "what did we select", so a unit chosen for a 150 l/s duty could later be reported as
        /// undersized or as having headroom purely according to catalogue order.
        /// </para>
        /// </summary>
        [Fact]
        public void AnIdentityWithConflictingCapacities_RefusesInEitherOrder()
        {
            List<VentilationUnitCapacityDescriptor> descriptors =
            [
                new(new VentilationUnitReference("Test Fixture", "MVHR-A", null), 100, 100),
                new(new VentilationUnitReference("Test Fixture", "MVHR-A", null), 200, 200),
            ];

            VentilationUnitSelection selection = descriptors.SelectSmallestCapableVentilationUnit(150, 150);

            Assert.False(selection.IsSelected);
            Assert.Contains("two different meanings", selection.Reason);

            //Deterministic: reversing the catalogue gives the same refusal, not a different answer.
            List<VentilationUnitCapacityDescriptor> descriptors_Reversed = [descriptors[1], descriptors[0]];

            VentilationUnitSelection selection_Reversed = descriptors_Reversed.SelectSmallestCapableVentilationUnit(150, 150);

            Assert.False(selection_Reversed.IsSelected);
            Assert.Equal(selection.Reason, selection_Reversed.Reason);
        }

        /// <summary>Nothing is written onto the air handling unit when the catalogue is ambiguous.</summary>
        [Fact]
        public void AnIdentityWithConflictingCapacities_WritesNothingOntoTheUnit()
        {
            AdjacencyCluster adjacencyCluster = Selected(out AirHandlingUnit airHandlingUnit, out VentilationUnitReference ventilationUnitReference);

            List<VentilationUnitCapacityDescriptor> descriptors =
            [
                new(new VentilationUnitReference("Test Fixture", "MVHR-A", null), 100, 100),
                new(new VentilationUnitReference("Test Fixture", "MVHR-A", null), 200, 200),
            ];

            VentilationUnitSelection selection = adjacencyCluster.SelectVentilationUnit(airHandlingUnit, descriptors, out _, out List<string> refusals);

            Assert.False(selection.IsSelected);
            Assert.Single(refusals);

            //The unit keeps the product it already had - it is not cleared and not overwritten.
            Assert.True(ventilationUnitReference.Matches(airHandlingUnit.SelectedVentilationUnitReference()));
        }

        /// <summary>
        /// An exact repeat is a duplicated line in a hand-edited file, not a contradiction, and stays
        /// harmless - the same way a duplicated template entry already does in
        /// <c>Query.SelectPreferredCapableSystem</c>.
        /// </summary>
        [Fact]
        public void AnIdentityRepeatedIdentically_RemainsValid()
        {
            List<VentilationUnitCapacityDescriptor> descriptors =
            [
                new(new VentilationUnitReference("Test Fixture", "MVHR-A", null), 200, 200),
                new(new VentilationUnitReference("Test Fixture", "MVHR-A", null), 200, 200),
            ];

            VentilationUnitSelection selection = descriptors.SelectSmallestCapableVentilationUnit(150, 150);

            Assert.True(selection.IsSelected, selection.Reason);
            Assert.Equal("MVHR-A", selection.VentilationUnitReference?.Model);
        }

        /// <summary>
        /// <b>Conflicting rank on one identity is refused too</b>, and that is a deliberate classification
        /// rather than an omission: rank decides selections, so two answers for it attached to one identity
        /// is the same defect as two answers for a capacity.
        /// </summary>
        [Fact]
        public void AnIdentityWithConflictingRank_Refuses()
        {
            List<VentilationUnitCapacityDescriptor> descriptors =
            [
                new(new VentilationUnitReference("Test Fixture", "MVHR-A", null), 200, 200, 1),
                new(new VentilationUnitReference("Test Fixture", "MVHR-A", null), 200, 200, 2),
            ];

            VentilationUnitSelection selection = descriptors.SelectSmallestCapableVentilationUnit(150, 150);

            Assert.False(selection.IsSelected);
            Assert.Contains("two different meanings", selection.Reason);
        }

        /// <summary>
        /// The lookup is defensive on its own, so a unit selected from one catalogue and later checked
        /// against a malformed one reports an unknown capacity rather than an order-dependent pass.
        /// </summary>
        [Fact]
        public void TheCapabilityLookup_IsNeverOrderDependent()
        {
            AdjacencyCluster adjacencyCluster = Selected(out AirHandlingUnit airHandlingUnit, out VentilationUnitReference ventilationUnitReference);

            List<VentilationUnitCapacityDescriptor> descriptors =
            [
                new(new VentilationUnitReference(ventilationUnitReference), 15, 15),
                new(new VentilationUnitReference(ventilationUnitReference), 500, 500),
            ];

            Assert.Null(airHandlingUnit.SelectedVentilationUnitCapacityDescriptor(descriptors));

            List<VentilationUnitCapacityDescriptor> descriptors_Reversed = [descriptors[1], descriptors[0]];

            Assert.Null(airHandlingUnit.SelectedVentilationUnitCapacityDescriptor(descriptors_Reversed));

            //So adequacy is unknown rather than decided by order, in both directions.
            Assert.False(adjacencyCluster.IsVentilationUnitSufficient(airHandlingUnit, descriptors, out string reason));
            Assert.Contains("not among the ventilation unit products offered", reason);

            Assert.False(adjacencyCluster.IsVentilationUnitSufficient(airHandlingUnit, descriptors_Reversed, out _));
        }

        // =================================================================================================
        // J. Identity and serialization
        // =================================================================================================

        /// <summary>
        /// The selected product's identity survives a round trip, on the air handling unit that carries it.
        /// A model saved after a selection reopens knowing what it was fitted with.
        /// </summary>
        [Fact]
        public void SelectedProductIdentity_SurvivesSerialization()
        {
            AdjacencyCluster adjacencyCluster = Selected(out AirHandlingUnit airHandlingUnit, out VentilationUnitReference ventilationUnitReference);

            AirHandlingUnit airHandlingUnit_RoundTripped = Helpers.RoundTrip.Once(airHandlingUnit);

            VentilationUnitReference ventilationUnitReference_RoundTripped = airHandlingUnit_RoundTripped.SelectedVentilationUnitReference();

            Assert.NotNull(ventilationUnitReference_RoundTripped);
            Assert.True(ventilationUnitReference.Matches(ventilationUnitReference_RoundTripped));
            Assert.Equal("MVHR-25", ventilationUnitReference_RoundTripped.Model);
            Assert.Equal("Test Fixture", ventilationUnitReference_RoundTripped.Manufacturer);

            //Through the whole cluster too, which is how a model is actually saved.
            AdjacencyCluster adjacencyCluster_RoundTripped = Helpers.RoundTrip.Once(adjacencyCluster);

            AirHandlingUnit airHandlingUnit_Cluster = Assert.Single(adjacencyCluster_RoundTripped.GetObjects<AirHandlingUnit>());

            Assert.True(ventilationUnitReference.Matches(airHandlingUnit_Cluster.SelectedVentilationUnitReference()));
        }

        /// <summary>
        /// Two air handling units may be fitted with the same reusable product and still have completely
        /// independent required and design duties. The product definition holds capability; the instance
        /// holds duty.
        /// </summary>
        [Fact]
        public void TwoUnits_ShareAProductAndKeepIndependentDuties()
        {
            //One catalogue entry, so both dwellings must select the same product.
            List<VentilationUnitCapacityDescriptor> descriptors = [Descriptor("MVHR-50", 50, 50)];

            PartOIterationPreparation preparation = Prepare(TwoDwellingModel(), descriptors);

            Assert.Null(preparation.Refusal);

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            List<AirHandlingUnit> airHandlingUnits = adjacencyCluster.GetObjects<AirHandlingUnit>();

            Assert.Equal(2, airHandlingUnits.Count);

            VentilationUnitReference ventilationUnitReference_1 = airHandlingUnits[0].SelectedVentilationUnitReference();
            VentilationUnitReference ventilationUnitReference_2 = airHandlingUnits[1].SelectedVentilationUnitReference();

            Assert.NotNull(ventilationUnitReference_1);
            Assert.NotNull(ventilationUnitReference_2);

            //The same product, by the identity that actually decides it.
            Assert.True(ventilationUnitReference_1.Matches(ventilationUnitReference_2));

            //...and two separate objects, so neither unit can be re-specified through the other. The
            //descriptor hands out a defensive copy on every read for exactly this reason.
            Assert.NotSame(ventilationUnitReference_1, ventilationUnitReference_2);

            adjacencyCluster.AirHandlingUnitDesignDuty(airHandlingUnits[0], out double supplyDuty_1_Lps, out _);
            adjacencyCluster.AirHandlingUnitDesignDuty(airHandlingUnits[1], out double supplyDuty_2_Lps, out _);

            Assert.NotEqual(supplyDuty_1_Lps, supplyDuty_2_Lps, 3);

            //Raising one dwelling's design moves that dwelling's duty and no other's.
            RaiseDwellingSupplyTo(adjacencyCluster, airHandlingUnits[0], supplyDuty_1_Lps + 5);

            adjacencyCluster.AirHandlingUnitDesignDuty(airHandlingUnits[1], out double supplyDuty_2_After_Lps, out _);

            Assert.Equal(supplyDuty_2_Lps, supplyDuty_2_After_Lps, 6);
        }

        // G. The plural outputs Grasshopper exposes (VentilationSystems, AirHandlingUnits,
        //    VentilationUnitSelections) - focused on the wiring itself, not the selection rule section A-F
        //    already cover. SAMAnalyticalPreparePartOIteration forwards these three properties verbatim.

        /// <summary>
        /// Iteration 1a's exact prior behaviour: no catalogue in, no unit out. VentilationSystems and
        /// AirHandlingUnits are unaffected by that - they are the generic network Iteration 1a already
        /// built - but VentilationUnitSelections stays empty and the singular outputs remain item 0 of the
        /// plural ones, so a caller that ignores the new outputs entirely sees nothing different.
        /// </summary>
        [Fact]
        public void NullDescriptors_LeaveVentilationSystemsAndAirHandlingUnitsPopulated_ButNoSelections()
        {
            PartOIterationPreparation preparation = Prepared(null);

            Assert.Single(preparation.VentilationSystems);
            Assert.Single(preparation.AirHandlingUnits);
            Assert.Empty(preparation.VentilationUnitSelections);

            Assert.Same(preparation.VentilationSystem, preparation.VentilationSystems[0]);
            Assert.Same(preparation.AirHandlingUnit, preparation.AirHandlingUnits[0]);

            Assert.Null(preparation.AirHandlingUnits[0].SelectedVentilationUnitReference());
        }

        /// <summary>
        /// A connected catalogue reaches the existing selection kernel through the same
        /// <c>PreparePartOIteration</c> call, and the selection it makes is the one
        /// <c>VentilationUnitSelections</c> reports - the same identity that ends up on the air handling
        /// unit, not a second answer computed separately for the report.
        /// </summary>
        [Fact]
        public void ConnectedDescriptors_ExposeTheSameSelectionThatWasWrittenToTheUnit()
        {
            PartOIterationPreparation preparation = Prepared(DwellingCatalogue());

            VentilationUnitSelection ventilationUnitSelection = Assert.Single(preparation.VentilationUnitSelections);

            Assert.True(ventilationUnitSelection.IsSelected);
            Assert.Equal("MVHR-25", ventilationUnitSelection.VentilationUnitReference.Model);

            VentilationUnitReference ventilationUnitReference_OnUnit = preparation.AirHandlingUnits[0].SelectedVentilationUnitReference();

            Assert.NotNull(ventilationUnitReference_OnUnit);
            Assert.True(ventilationUnitSelection.VentilationUnitReference.Matches(ventilationUnitReference_OnUnit));
        }

        /// <summary>
        /// Two dwellings each contribute their own system, their own unit and their own selection to the
        /// three plural outputs - none of the three collapses two dwellings' results into one, and each
        /// selection's duty is the duty of the dwelling it actually belongs to, not the other one's.
        /// </summary>
        [Fact]
        public void TwoDwellings_PluralOutputsCarryOneEntryPerDwelling_WithMatchingDuties()
        {
            PartOIterationPreparation preparation = Prepare(TwoDwellingModel(), DwellingCatalogue());

            Assert.Null(preparation.Refusal);
            Assert.Equal(2, preparation.VentilationSystems.Count);
            Assert.Equal(2, preparation.AirHandlingUnits.Count);
            Assert.Equal(2, preparation.VentilationUnitSelections.Count);

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            foreach (AirHandlingUnit airHandlingUnit in preparation.AirHandlingUnits)
            {
                VentilationUnitReference ventilationUnitReference = airHandlingUnit.SelectedVentilationUnitReference();
                Assert.NotNull(ventilationUnitReference);

                VentilationUnitSelection ventilationUnitSelection = preparation.VentilationUnitSelections.Find(x => ventilationUnitReference.Matches(x.VentilationUnitReference));
                Assert.NotNull(ventilationUnitSelection);

                adjacencyCluster.AirHandlingUnitDesignDuty(airHandlingUnit, out double supplyDuty_Lps, out double extractDuty_Lps);

                Assert.Equal(supplyDuty_Lps, ventilationUnitSelection.SupplyDuty_Lps, 6);
                Assert.Equal(extractDuty_Lps, ventilationUnitSelection.ExtractDuty_Lps, 6);
            }
        }

        /// <summary>
        /// The duty a selection reports is the dwelling's design duty, never the equipment's rated capacity
        /// - restated here against <see cref="PartOIterationPreparation.VentilationUnitSelections"/>
        /// specifically, since that is the new surface a Grasshopper canvas reads it through.
        /// </summary>
        [Fact]
        public void SelectionDuty_IsTheDesignDuty_NotTheSelectedProductsCapacity()
        {
            PartOIterationPreparation preparation = Prepared(DwellingCatalogue());

            VentilationUnitSelection ventilationUnitSelection = Assert.Single(preparation.VentilationUnitSelections);

            Assert.Equal(preparation.DesignSupplyDuty_Lps, ventilationUnitSelection.SupplyDuty_Lps, 6);
            Assert.Equal(preparation.DesignExtractDuty_Lps, ventilationUnitSelection.ExtractDuty_Lps, 6);

            Assert.NotEqual(ventilationUnitSelection.Descriptor.MaximumSupplyFlowRate_Lps, ventilationUnitSelection.SupplyDuty_Lps, 3);
        }

        // K. ApplyTargetedDesignAirFlow's equipment validation - Grasshopper Seam 2's analytical orchestration.
        //    The airflow rebalancing itself (targeted vs derived, Part F floor, no-partial-write) is section
        //    D/E/H's job and is unchanged; these tests are focused on the NEW behaviour: once a catalogue is
        //    offered, does the serving unit get kept, reselected or refused - and does an equipment refusal
        //    ever roll back the airflow change that already committed?
        //
        //    The selection is asserted from the CLUSTER throughout - SelectedModel/SelectedReference - and
        //    never from a unit handle taken before the call, because the write is a replacement object
        //    rather than a mutation of the model's instance. That is also what keeps the change inside the
        //    cluster it was handed, which is its own test at the end of this region.

        /// <summary>
        /// A targeted change the selected unit can still absorb leaves the selection exactly as it was -
        /// restated here against the new orchestration specifically, and against the catalogue's OTHER
        /// capable products: nothing about their existence should matter while MVHR-100 still suffices.
        /// </summary>
        [Fact]
        public void EquipmentValidation_KeptWhenSelectedProductRemainsSufficient()
        {
            PartOIterationPreparation preparation = Prepared(Catalogue());

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;
            AirHandlingUnit airHandlingUnit = Assert.Single(adjacencyCluster.GetObjects<AirHandlingUnit>());

            VentilationUnitReference ventilationUnitReference_Before = airHandlingUnit.SelectedVentilationUnitReference();
            Assert.Equal("MVHR-100", ventilationUnitReference_Before.Model);

            DwellingDesignAirFlowChange change = RaiseDutyTotalToWithCatalogue(adjacencyCluster, airHandlingUnit, 60, Catalogue());

            Assert.Equal(VentilationUnitSelectionOutcome.Kept, change.VentilationUnitSelectionOutcome);
            Assert.True(ventilationUnitReference_Before.Matches(change.VentilationUnitReference));
            Assert.True(ventilationUnitReference_Before.Matches(SelectedReference(adjacencyCluster)));
            Assert.Null(change.VentilationUnitSelectionReason);
        }

        /// <summary>
        /// A targeted change that pushes the recalculated duty past the selected product's rating escalates
        /// to the next SMALLEST capable product - 180, not 150, since a 160 l/s duty needs the smallest one
        /// that still covers it, and 150 does not.
        /// </summary>
        [Fact]
        public void EquipmentValidation_ReselectsTheSmallestCapableProductWhenExhausted()
        {
            PartOIterationPreparation preparation = Prepared(Catalogue());

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;
            AirHandlingUnit airHandlingUnit = Assert.Single(adjacencyCluster.GetObjects<AirHandlingUnit>());

            DwellingDesignAirFlowChange change = RaiseDutyTotalToWithCatalogue(adjacencyCluster, airHandlingUnit, 160, Catalogue());

            Assert.Equal(VentilationUnitSelectionOutcome.Reselected, change.VentilationUnitSelectionOutcome);
            Assert.Equal("MVHR-180", change.VentilationUnitReference?.Model);
            Assert.Equal("MVHR-180", SelectedModel(adjacencyCluster));

            //The design airflow is the engineering duty, never the selected product's rating.
            Assert.Equal(160, change.SupplyDuty_Lps, 6);
        }

        /// <summary>
        /// <b>No product in the catalogue can move the recalculated duty, and the airflow change is NOT
        /// rolled back because of it.</b> This is the existing contract
        /// <c>ATargetedChangeBeyondCapacity_ExposesExhaustionAndEscalates</c> already pins for the
        /// separately-sequenced calls; restated here for the single orchestrated call this seam adds.
        /// </summary>
        [Fact]
        public void EquipmentValidation_RefusesEquipmentButKeepsTheAirflowChangeSuccessful()
        {
            PartOIterationPreparation preparation = Prepared(Catalogue());

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;
            AirHandlingUnit airHandlingUnit = Assert.Single(adjacencyCluster.GetObjects<AirHandlingUnit>());

            DwellingDesignAirFlowChange change = RaiseDutyTotalToWithCatalogue(adjacencyCluster, airHandlingUnit, 300, Catalogue());

            //The airflow change committed - it is not what refused.
            Assert.True(change.Successful, string.Join(" ", change.Refusals));
            Assert.Equal(300, change.SupplyDuty_Lps, 6);

            Assert.Equal(VentilationUnitSelectionOutcome.Refused, change.VentilationUnitSelectionOutcome);
            Assert.False(string.IsNullOrWhiteSpace(change.VentilationUnitSelectionReason));

            //The unit keeps whatever it had before this call - an honest state, never a half-selection.
            Assert.Equal("MVHR-100", SelectedModel(adjacencyCluster));
            Assert.Equal("MVHR-100", change.VentilationUnitReference?.Model);
        }

        /// <summary>
        /// <b>No catalogue connected is backward compatible, not a new refusal.</b> A model that already
        /// carries a selected product must not have that selection invalidated, touched, or even inspected,
        /// merely because the caller did not supply a catalogue this time.
        /// </summary>
        [Fact]
        public void EquipmentValidation_UnconnectedDescriptors_LeavesTheSelectionUntouched()
        {
            PartOIterationPreparation preparation = Prepared(Catalogue());

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;
            AirHandlingUnit airHandlingUnit = Assert.Single(adjacencyCluster.GetObjects<AirHandlingUnit>());

            VentilationUnitReference ventilationUnitReference_Before = airHandlingUnit.SelectedVentilationUnitReference();

            //Well beyond every product in the catalogue - if equipment were evaluated at all, this would refuse it.
            DwellingDesignAirFlowChange change = RaiseDutyTotalToWithCatalogue(adjacencyCluster, airHandlingUnit, 300, null);

            Assert.True(change.Successful, string.Join(" ", change.Refusals));
            Assert.Equal(VentilationUnitSelectionOutcome.NotApplicable, change.VentilationUnitSelectionOutcome);
            Assert.Null(change.VentilationUnitReference);
            Assert.Null(change.VentilationUnitSelectionReason);

            //Untouched - not merely unreported.
            Assert.True(ventilationUnitReference_Before.Matches(SelectedReference(adjacencyCluster)));
        }

        /// <summary>
        /// <b>A unit nothing has ever been selected for is not "exhausted".</b> This call validates an
        /// EXISTING selection - it does not make a first one. A catalogue offered against a unit with no
        /// prior selection must leave it exactly as unselected as it was, reading NotApplicable, never
        /// silently perform an initial selection as a side effect of a targeted airflow change.
        /// </summary>
        [Fact]
        public void EquipmentValidation_NeverSelected_StaysNotApplicable_EvenWithACatalogueOffered()
        {
            //Iteration 1a: descriptors null at preparation, so no selection was ever made.
            PartOIterationPreparation preparation = Prepared(null);

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;
            AirHandlingUnit airHandlingUnit = Assert.Single(adjacencyCluster.GetObjects<AirHandlingUnit>());

            Assert.Null(airHandlingUnit.SelectedVentilationUnitReference());

            DwellingDesignAirFlowChange change = RaiseDutyTotalToWithCatalogue(adjacencyCluster, airHandlingUnit, 30, Catalogue());

            Assert.Equal(VentilationUnitSelectionOutcome.NotApplicable, change.VentilationUnitSelectionOutcome);
            Assert.Null(change.VentilationUnitReference);
            Assert.Null(change.VentilationUnitSelectionReason);

            //Still nothing selected - not a first selection made on its behalf.
            Assert.Null(SelectedReference(adjacencyCluster));

            //The unit itself is still resolved and reported, even though there was nothing to validate -
            //null here would mean "no unit resolved", which is not what happened.
            Assert.NotNull(change.AirHandlingUnit);
            Assert.Equal(airHandlingUnit.Guid, change.AirHandlingUnit.Guid);
        }

        /// <summary>
        /// <b>An unknown capacity is refused, never treated as exhausted.</b> The currently selected
        /// product is not among the descriptors THIS call was given - a filtered or narrower catalogue
        /// than the one it was originally selected from - so its adequacy is unknown, not insufficient.
        /// Falling through to reselection would let an incomplete catalogue silently downgrade a unit that
        /// remains entirely adequate for the real duty.
        /// </summary>
        [Fact]
        public void EquipmentValidation_SelectedProductNotInThisCatalogue_IsRefusedAsUnknown_NeverDowngraded()
        {
            PartOIterationPreparation preparation = Prepared(Catalogue());

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;
            AirHandlingUnit airHandlingUnit = Assert.Single(adjacencyCluster.GetObjects<AirHandlingUnit>());

            Assert.Equal("MVHR-100", airHandlingUnit.SelectedVentilationUnitReference()?.Model);

            //A catalogue that omits MVHR-100 entirely - the unit is still selected as it, but this call
            //cannot see its capacity. A duty well within 100's original rating, so a Reselected outcome
            //here would prove the defect: downgrading a unit that was never actually exhausted.
            List<VentilationUnitCapacityDescriptor> catalogueWithoutTheSelectedProduct = [Descriptor("MVHR-150", 150, 150), Descriptor("MVHR-180", 180, 180), Descriptor("MVHR-220", 220, 220)];

            DwellingDesignAirFlowChange change = RaiseDutyTotalToWithCatalogue(adjacencyCluster, airHandlingUnit, 60, catalogueWithoutTheSelectedProduct);

            Assert.True(change.Successful, string.Join(" ", change.Refusals));
            Assert.Equal(VentilationUnitSelectionOutcome.Refused, change.VentilationUnitSelectionOutcome);
            Assert.Contains("not among the ventilation unit products offered", change.VentilationUnitSelectionReason);

            //Untouched - still MVHR-100, never downgraded to a smaller product from an incomplete catalogue.
            Assert.Equal("MVHR-100", SelectedModel(adjacencyCluster));
            Assert.Equal("MVHR-100", change.VentilationUnitReference?.Model);
        }

        /// <summary>
        /// One dwelling's equipment escalation must not read, report on, or touch another dwelling's unit -
        /// the same independence <c>TwoUnits_ShareAProductAndKeepIndependentDuties</c> already proves for
        /// duty, restated here for the new equipment-outcome fields specifically.
        /// </summary>
        [Fact]
        public void EquipmentValidation_TwoDwellings_StayIndependent()
        {
            List<VentilationUnitCapacityDescriptor> descriptors = [Descriptor("MVHR-50", 50, 50)];

            PartOIterationPreparation preparation = Prepare(TwoDwellingModel(), descriptors);

            Assert.Null(preparation.Refusal);

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            List<AirHandlingUnit> airHandlingUnits = adjacencyCluster.GetObjects<AirHandlingUnit>();
            Assert.Equal(2, airHandlingUnits.Count);

            VentilationUnitReference ventilationUnitReference_2_Before = airHandlingUnits[1].SelectedVentilationUnitReference();

            //Push dwelling 1 well past the one product offered - it can only refuse, never touch dwelling 2.
            DwellingDesignAirFlowChange change = RaiseDutyTotalToWithCatalogue(adjacencyCluster, airHandlingUnits[0], 500, descriptors);

            Assert.True(change.Successful, string.Join(" ", change.Refusals));
            Assert.Equal(VentilationUnitSelectionOutcome.Refused, change.VentilationUnitSelectionOutcome);
            Assert.True(change.AirHandlingUnit.Guid == airHandlingUnits[0].Guid);

            //Dwelling 2's unit and duty are exactly as they were.
            Assert.True(ventilationUnitReference_2_Before.Matches(airHandlingUnits[1].SelectedVentilationUnitReference()));
            adjacencyCluster.AirHandlingUnitDesignDuty(airHandlingUnits[1], out double supplyDuty_2_Lps, out _);
            Assert.NotEqual(500, supplyDuty_2_Lps, 3);
        }

        /// <summary>
        /// <b>Deferred guard, re-checked for this seam.</b> Grasshopper can supply any string as
        /// <c>flowClassification_</c>; if it resolves to <see cref="FlowClassification.Undefined"/>, the
        /// existing top-of-method check already refuses it before anything else runs. Pinned here because
        /// this is the first public entry point that exposes the parameter to Grasshopper at all.
        /// </summary>
        [Fact]
        public void UndefinedFlowClassification_IsRefused()
        {
            PartOIterationPreparation preparation = Prepared(null);

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;
            Space space_Bedroom = adjacencyCluster.GetSpaces().Find(x => x.Name == name_Bedroom);

            Dictionary<Guid, double> terminals_Before = TerminalDesigns(adjacencyCluster);

            DwellingDesignAirFlowChange change = adjacencyCluster.ApplyTargetedDesignAirFlow(space_Bedroom, FlowClassification.Undefined, 999);

            Assert.False(change.Successful);
            Assert.Contains(change.Refusals, x => x.Contains("neither"));
            Assert.Equal(VentilationUnitSelectionOutcome.NotApplicable, change.VentilationUnitSelectionOutcome);
            Assert.Equal(terminals_Before, TerminalDesigns(adjacencyCluster));
        }

        /// <summary>
        /// <b>Deferred guard, re-checked for this seam.</b> A room holding a healthy terminal beside a
        /// second one whose design airflow was never established (null, not zero - see
        /// <see cref="VentilationTerminal.DesignFlowRate_Lps"/>) is exactly the externally-authored state
        /// Grasshopper can now hand this operation for the first time.
        /// <para>
        /// <b>Pinned, not newly guarded.</b> <c>IsRedistributable</c> already treats null as a real,
        /// zero-weighted quantity - deliberately, so a room's FIRST design decision through this operation
        /// is not blocked - and the sibling's calculated share is then exactly zero, never NaN and never a
        /// silently wrong nonzero number. The healthy terminal absorbs the whole change exactly as if the
        /// null one were not there. Refusing outright would break the ordinary case of designing a room for
        /// the first time; this is not the false-0/0-duty risk the deferred guard names, because that risk
        /// is about a whole system with no established terminal at all, and this transaction's OWN
        /// precondition already requires the system to be balanced and Approved Document F compliant before
        /// anything is written - which a genuinely never-sized system is not. No new production path
        /// creates a <see cref="VentilationTerminal"/> from nothing here; this only ever rewrites terminals
        /// the model already had.
        /// </para>
        /// </summary>
        [Fact]
        public void ASiblingNullTerminal_GetsAnExplicitZeroShare_NeverCorruptingTheRoomTotal()
        {
            PartOIterationPreparation preparation = Prepared(DwellingCatalogue());

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            Space space_Kitchen = adjacencyCluster.GetSpaces().Find(x => x.Name == name_Kitchen);

            VentilationTerminal ventilationTerminal_Unestablished = new("Unestablished extract", FlowClassification.Extract, null);
            ventilationTerminal_Unestablished.SetValue(VentilationTerminalParameter.PartFTerminalReference, Analytical.Query.VentilationTerminals(adjacencyCluster.VentilationTerminals(space_Kitchen), FlowClassification.Extract)[0].GetValue<PartFTerminalReference>(VentilationTerminalParameter.PartFTerminalReference));

            adjacencyCluster.AddObject(ventilationTerminal_Unestablished);
            adjacencyCluster.AddRelation(ventilationTerminal_Unestablished, space_Kitchen);
            adjacencyCluster.AddRelation(ventilationTerminal_Unestablished, Assert.Single(adjacencyCluster.GetObjects<VentilationSystem>()));

            double requirement_Kitchen_Lps = adjacencyCluster.PartFRequiredFlowRate_Lps(space_Kitchen, FlowClassification.Extract).Value;
            double design_Kitchen_Before_Lps = Design(adjacencyCluster, space_Kitchen, FlowClassification.Extract);

            Space space_Bedroom = adjacencyCluster.GetSpaces().Find(x => x.Name == name_Bedroom);

            DwellingDesignAirFlowChange change = adjacencyCluster.ApplyTargetedDesignAirFlow(space_Bedroom, FlowClassification.Supply, Design(adjacencyCluster, space_Bedroom, FlowClassification.Supply) + 4);

            Assert.True(change.Successful, string.Join(" ", change.Refusals));

            //The room total is exact and above its requirement - never NaN, never silently short.
            double design_Kitchen_After_Lps = Design(adjacencyCluster, space_Kitchen, FlowClassification.Extract);
            Assert.Equal(design_Kitchen_Before_Lps + 4, design_Kitchen_After_Lps, 6);
            Assert.True(design_Kitchen_After_Lps + 0.001 >= requirement_Kitchen_Lps);

            //The previously-unestablished terminal is now explicitly, exactly zero - not left null, and not NaN.
            VentilationTerminal ventilationTerminal_After = adjacencyCluster.GetObjects<VentilationTerminal>().Find(x => x.Guid == ventilationTerminal_Unestablished.Guid);
            Assert.NotNull(ventilationTerminal_After);
            Assert.True(ventilationTerminal_After.DesignFlowRate_Lps.HasValue);
            Assert.Equal(0, ventilationTerminal_After.DesignFlowRate_Lps.Value, 6);
        }

        /// <summary>
        /// <b>A reselection is written into the cluster it was handed, and into nothing behind it.</b>
        /// <para>
        /// <c>AnalyticalModel.AdjacencyCluster</c> hands out a copy so that a manual edit can be made and
        /// then kept or thrown away, and both halves of this seam's change have to honour that copy. The
        /// airflow half always did - the terminals are written as replacements. The equipment half did not:
        /// the selection was set on the air handling unit object <i>in place</i>, and the copy shares that
        /// object with the model it came from, so a dwelling whose plant was silently escalated on the
        /// upstream model had no way to see it. <c>SAMAnalyticalApplyTargetedDesignAirFlow</c> documents
        /// its model input as not modified, and an "all or nothing" refusal contract on top of that; a
        /// selection leaking upstream breaks both.
        /// </para>
        /// </summary>
        [Fact]
        public void EquipmentValidation_AReselection_IsNotWrittenBackOntoTheModelTheClusterCameFrom()
        {
            AnalyticalModel analyticalModel = Prepared(DwellingCatalogue()).AnalyticalModel;

            Space space = analyticalModel.AdjacencyCluster.GetSpaces().Find(x => x.Name == name_LivingRoom);

            double design_Before_Lps = Design(analyticalModel.AdjacencyCluster, space, FlowClassification.Supply);

            //Past what the selected MVHR-25 will carry, so the seam has to buy the next product up.
            double requested_Lps = design_Before_Lps + 10;

            //The copy, taken and edited exactly as the Grasshopper component takes and edits one.
            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;

            AirHandlingUnit airHandlingUnit_Before = Assert.Single(adjacencyCluster.GetObjects<AirHandlingUnit>());
            List<Guid> guids_Sections_Before = [.. airHandlingUnit_Before.GetSections_Supply().ConvertAll(x => x.Guid), .. airHandlingUnit_Before.GetSections_Extract().ConvertAll(x => x.Guid)];
            Assert.NotEmpty(guids_Sections_Before);

            DwellingDesignAirFlowChange change = adjacencyCluster.ApplyTargetedDesignAirFlow(space, FlowClassification.Supply, requested_Lps, PartFExtractAllocationStrategy.MinimumFirstCookingPriority, 0.001, DwellingCatalogue());

            Assert.True(change.Successful, string.Join(" ", change.Refusals));
            Assert.Equal(VentilationUnitSelectionOutcome.Reselected, change.VentilationUnitSelectionOutcome);

            //The copy carries both halves: the design that was asked for, and the product bought to move it.
            Assert.Equal(requested_Lps, Design(adjacencyCluster, space, FlowClassification.Supply), 6);
            Assert.Equal("MVHR-35", SelectedModel(adjacencyCluster));
            Assert.Equal("MVHR-35", change.VentilationUnitReference?.Model);

            //And the model the copy came from carries neither.
            Assert.Equal(design_Before_Lps, Design(analyticalModel.AdjacencyCluster, space, FlowClassification.Supply), 6);
            Assert.Equal("MVHR-25", SelectedModel(analyticalModel.AdjacencyCluster));

            //The unit the copy now holds is a replacement, so it has to be the SAME unit in every other
            //respect - the identity every name- and guid-based lookup resolves it through, the supply
            //temperatures Iteration 1a deliberately leaves unstated, and the whole section arrangement,
            //which lives in an equipment model of its own rather than in a parameter.
            AirHandlingUnit airHandlingUnit_After = Assert.Single(adjacencyCluster.GetObjects<AirHandlingUnit>());

            Assert.Equal(airHandlingUnit_Before.Guid, airHandlingUnit_After.Guid);
            Assert.Equal(airHandlingUnit_Before.Name, airHandlingUnit_After.Name);
            Assert.Equal(airHandlingUnit_Before.SummerSupplyTemperature, airHandlingUnit_After.SummerSupplyTemperature);
            Assert.Equal(airHandlingUnit_Before.WinterSupplyTemperature, airHandlingUnit_After.WinterSupplyTemperature);
            List<Guid> guids_Sections_After = [.. airHandlingUnit_After.GetSections_Supply().ConvertAll(x => x.Guid), .. airHandlingUnit_After.GetSections_Extract().ConvertAll(x => x.Guid)];
            Assert.Equal(guids_Sections_Before, guids_Sections_After);

            //And the relations to it survived being written over, so the dwelling's system still resolves
            //its plant and the duty the reselection was made against is still readable through it.
            Assert.True(adjacencyCluster.AirHandlingUnitDesignDuty(airHandlingUnit_After, out double supplyDuty_Lps, out _));
            Assert.True(supplyDuty_Lps > 25, string.Format("The reselection was made against a design duty of {0:0.###} l/s, which should have exceeded the MVHR-25 it replaced.", supplyDuty_Lps));
        }

        /// <summary>
        /// Moves an air handling unit's whole duty to <paramref name="target_Lps"/> through the living room
        /// of the dwelling it serves - found through the unit's own system, exactly as
        /// <see cref="RaiseDwellingSupplyTo"/> does, so this works for either the one- or two-dwelling
        /// fixture - but forwarding a catalogue through the new equipment-validation parameter, and
        /// returning the change for the caller to assert on.
        /// </summary>
        private static DwellingDesignAirFlowChange RaiseDutyTotalToWithCatalogue(AdjacencyCluster adjacencyCluster, AirHandlingUnit airHandlingUnit, double target_Lps, List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors)
        {
            adjacencyCluster.AirHandlingUnitDesignDuty(airHandlingUnit, out double supplyDuty_Lps, out _);

            foreach (VentilationSystem ventilationSystem in adjacencyCluster.VentilationSystems(airHandlingUnit))
            {
                foreach (Space space in adjacencyCluster.GetRelatedObjects<Space>(ventilationSystem) ?? [])
                {
                    if (space.Name.StartsWith(name_LivingRoom, StringComparison.Ordinal))
                    {
                        DwellingDesignAirFlowChange result = adjacencyCluster.ApplyTargetedDesignAirFlow(
                            space,
                            FlowClassification.Supply,
                            Design(adjacencyCluster, space, FlowClassification.Supply) + (target_Lps - supplyDuty_Lps),
                            ventilationUnitCapacityDescriptors: ventilationUnitCapacityDescriptors);

                        Assert.True(result.Successful, string.Join(" ", result.Refusals));

                        return result;
                    }
                }
            }

            Assert.Fail("The fixture unit serves no living room.");

            return null;
        }

        // =================================================================================================
        // L. Iteration 2B - the candidate / preflight / commit boundary.
        //    Modify.EvaluateTargetedDesignAirFlow proposes the SAME engineering change section D/E/H/K
        //    already pin, and differs from it in exactly one respect: it never touches the model it was
        //    given, and it hands back the changed one only where the selected unit can carry it. The
        //    manual seam's semantics are restated here beside it, unchanged, because the two have to
        //    diverge deliberately rather than by accident.
        // =================================================================================================

        /// <summary>
        /// <b>Accepted.</b> A candidate the selected product can carry comes back with the changed model to
        /// adopt, the targeted and derived adjustments it would make, the duties before and after, and the
        /// headroom that would be left - while the model it was evaluated against is untouched.
        /// </summary>
        [Fact]
        public void Candidate_WithinSelectedCapacity_IsAcceptedAndCarriesTheChangedModel()
        {
            AdjacencyCluster adjacencyCluster = Selected(out AirHandlingUnit airHandlingUnit, out _);

            adjacencyCluster.AirHandlingUnitDesignDuty(airHandlingUnit, out double supplyDuty_Before_Lps, out _);
            Assert.Equal(19.2, supplyDuty_Before_Lps, 3);

            string json_Before = Core.Convert.ToString(adjacencyCluster);

            Space space = adjacencyCluster.GetSpaces().Find(x => x.Name == name_LivingRoom);
            double design_Before_Lps = Design(adjacencyCluster, space, FlowClassification.Supply);

            //22.2 l/s against MVHR-25 - inside the rating, with 2.8 l/s to spare.
            DwellingDesignAirFlowCandidate candidate = adjacencyCluster.EvaluateTargetedDesignAirFlow(
                space,
                FlowClassification.Supply,
                design_Before_Lps + 3,
                ventilationUnitCapacityDescriptors: DwellingCatalogue());

            Assert.True(candidate.IsAccepted, string.Join(" ", candidate.Refusals));
            Assert.NotNull(candidate.AdjacencyCluster);
            Assert.Equal(VentilationUnitSelectionOutcome.Kept, candidate.VentilationUnitSelectionOutcome);
            Assert.Null(candidate.VentilationUnitSelectionReason);

            //What it proposes, and what that proposal derives.
            Assert.Equal(design_Before_Lps, candidate.TargetedAdjustment.Before_Lps, 6);
            Assert.Equal(design_Before_Lps + 3, candidate.TargetedAdjustment.After_Lps, 6);
            Assert.False(candidate.TargetedAdjustment.IsDerived);
            Assert.NotEmpty(candidate.DerivedAdjustments);
            Assert.All(candidate.DerivedAdjustments, x => Assert.True(x.IsDerived));

            double change_Derived_Lps = 0;
            foreach (DesignAirFlowAdjustment designAirFlowAdjustment in candidate.DerivedAdjustments)
            {
                change_Derived_Lps += designAirFlowAdjustment.Change_Lps;
            }

            Assert.Equal(3, change_Derived_Lps, 6);

            //The dwelling's duty, on both sides of the proposal.
            Assert.Equal(19.2, candidate.SupplyDuty_Before_Lps, 3);
            Assert.Equal(19.2, candidate.ExtractDuty_Before_Lps, 3);
            Assert.Equal(22.2, candidate.SupplyDuty_After_Lps, 3);
            Assert.Equal(22.2, candidate.ExtractDuty_After_Lps, 3);

            //Headroom is reported against the SELECTED product, and is not a design airflow.
            Assert.Equal(2.8, candidate.SupplyHeadroom_Lps, 3);
            Assert.Equal(2.8, candidate.ExtractHeadroom_Lps, 3);
            Assert.Equal("MVHR-25", candidate.VentilationUnitReference?.Model);

            //The model to adopt genuinely carries the change, and is still a compliant balanced design.
            Assert.Equal(design_Before_Lps + 3, Design(candidate.AdjacencyCluster, space, FlowClassification.Supply), 6);
            candidate.AdjacencyCluster.ReconcileVentilationSystemDesignDuty(candidate.VentilationSystem, out _, out _, out List<string> refusals_Compliance);
            Assert.Empty(refusals_Compliance);

            //And the model it was evaluated against is exactly as it was.
            Assert.True(Helpers.JsonEquivalence.AreEquivalent(json_Before, Core.Convert.ToString(adjacencyCluster), out string difference), difference);
        }

        /// <summary>
        /// <b>Rejected, atomically.</b> A candidate whose duty outgrows the selected product is refused -
        /// and the model it was evaluated against is unchanged terminal by terminal, not merely at the room
        /// totals. No targeted change without its derived ones, and no derived ones at all.
        /// </summary>
        [Fact]
        public void Candidate_BeyondSelectedCapacity_IsRefusedAndNothingIsWritten()
        {
            AdjacencyCluster adjacencyCluster = Selected(out AirHandlingUnit airHandlingUnit, out VentilationUnitReference ventilationUnitReference_Before);

            string json_Before = Core.Convert.ToString(adjacencyCluster);
            Dictionary<Guid, double> terminals_Before = TerminalDesigns(adjacencyCluster);

            Space space = adjacencyCluster.GetSpaces().Find(x => x.Name == name_LivingRoom);

            //29.2 l/s against MVHR-25 - 4.2 l/s past the rating.
            DwellingDesignAirFlowCandidate candidate = adjacencyCluster.EvaluateTargetedDesignAirFlow(
                space,
                FlowClassification.Supply,
                Design(adjacencyCluster, space, FlowClassification.Supply) + 10,
                ventilationUnitCapacityDescriptors: DwellingCatalogue());

            Assert.False(candidate.IsAccepted);

            //Nothing to adopt - the whole point of the boundary.
            Assert.Null(candidate.AdjacencyCluster);

            //The airflow arithmetic itself was fine; it is the equipment that rejected the candidate, and
            //the result says so specifically rather than only that something refused.
            Assert.True(candidate.Change.Successful, string.Join(" ", candidate.Change.Refusals));
            Assert.Equal(VentilationUnitSelectionOutcome.Refused, candidate.VentilationUnitSelectionOutcome);
            Assert.Contains("exhausted", candidate.VentilationUnitSelectionReason);
            Assert.Contains(candidate.Refusals, x => x.Contains("cannot carry it"));

            //It reports what the candidate WOULD have designed, and how far past the unit that went.
            Assert.Equal(29.2, candidate.SupplyDuty_After_Lps, 3);
            Assert.Equal(-4.2, candidate.SupplyHeadroom_Lps, 3);

            //Untouched: every terminal, the unit's selection, and the model whole.
            Assert.Equal(terminals_Before, TerminalDesigns(adjacencyCluster));
            Assert.True(ventilationUnitReference_Before.Matches(airHandlingUnit.SelectedVentilationUnitReference()));
            Assert.True(Helpers.JsonEquivalence.AreEquivalent(json_Before, Core.Convert.ToString(adjacencyCluster), out string difference), difference);
        }

        /// <summary>
        /// <b>A candidate never buys a bigger unit.</b> The catalogue above holds MVHR-35 and MVHR-50, both
        /// of which could move 29.2 l/s - and the candidate is refused anyway. The selected unit is the
        /// constraint an optimisation explores within, not a variable it gets to move; choosing a product
        /// remains <c>Modify.SelectVentilationUnit</c>, called deliberately.
        /// </summary>
        [Fact]
        public void Candidate_BeyondSelectedCapacity_NeverReselects_EvenWhereABiggerProductIsOffered()
        {
            AdjacencyCluster adjacencyCluster = Selected(out AirHandlingUnit airHandlingUnit, out _);

            Assert.Contains(DwellingCatalogue(), x => x.MaximumSupplyFlowRate_Lps >= 35);

            Space space = adjacencyCluster.GetSpaces().Find(x => x.Name == name_LivingRoom);

            DwellingDesignAirFlowCandidate candidate = adjacencyCluster.EvaluateTargetedDesignAirFlow(
                space,
                FlowClassification.Supply,
                Design(adjacencyCluster, space, FlowClassification.Supply) + 10,
                ventilationUnitCapacityDescriptors: DwellingCatalogue());

            Assert.False(candidate.IsAccepted);
            Assert.NotEqual(VentilationUnitSelectionOutcome.Reselected, candidate.VentilationUnitSelectionOutcome);

            //Neither the model's unit nor the one the result reports has moved off MVHR-25.
            Assert.Equal("MVHR-25", airHandlingUnit.SelectedVentilationUnitReference()?.Model);
            Assert.Equal("MVHR-25", candidate.VentilationUnitReference?.Model);
        }

        /// <summary>
        /// <b>The manual seam is unchanged, and the two now diverge deliberately.</b> The SAME proposal, on
        /// the SAME dwelling, with a catalogue offering nothing bigger: the manual edit commits the airflow
        /// change and reports the equipment gap beside it, while the candidate rejects the proposal whole.
        /// That difference is the entire content of Iteration 2B.
        /// </summary>
        [Fact]
        public void ManualEdit_KeepsItsAirflowChangeWhenEquipmentRefuses_WhereTheCandidateKeepsNothing()
        {
            //A catalogue holding ONLY the selected product, so the manual seam has nothing to escalate to
            //and refuses - the case that would look identical to a candidate rejection if the manual
            //semantics had been changed.
            List<VentilationUnitCapacityDescriptor> descriptors = [Descriptor("MVHR-25", 25, 25)];

            AdjacencyCluster adjacencyCluster_Manual = Selected(out AirHandlingUnit airHandlingUnit_Manual, out _);
            Space space_Manual = adjacencyCluster_Manual.GetSpaces().Find(x => x.Name == name_LivingRoom);
            double design_Manual_Lps = Design(adjacencyCluster_Manual, space_Manual, FlowClassification.Supply) + 10;

            DwellingDesignAirFlowChange change = adjacencyCluster_Manual.ApplyTargetedDesignAirFlow(
                space_Manual,
                FlowClassification.Supply,
                design_Manual_Lps,
                ventilationUnitCapacityDescriptors: descriptors);

            //MANUAL: the design edit stands, and the equipment gap is reported beside it - never rolled
            //back into it, and never added to the transaction's own refusals.
            Assert.True(change.Successful, string.Join(" ", change.Refusals));
            Assert.Empty(change.Refusals);
            Assert.Equal(VentilationUnitSelectionOutcome.Refused, change.VentilationUnitSelectionOutcome);
            Assert.False(string.IsNullOrWhiteSpace(change.VentilationUnitSelectionReason));
            Assert.Equal(design_Manual_Lps, Design(adjacencyCluster_Manual, space_Manual, FlowClassification.Supply), 6);
            Assert.Equal(29.2, change.SupplyDuty_Lps, 3);
            Assert.Equal("MVHR-25", airHandlingUnit_Manual.SelectedVentilationUnitReference()?.Model);

            //AUTOMATIC: the same proposal, and nothing survives it.
            AdjacencyCluster adjacencyCluster_Candidate = Selected(out _, out _);
            Space space_Candidate = adjacencyCluster_Candidate.GetSpaces().Find(x => x.Name == name_LivingRoom);
            double design_Candidate_Before_Lps = Design(adjacencyCluster_Candidate, space_Candidate, FlowClassification.Supply);

            string json_Before = Core.Convert.ToString(adjacencyCluster_Candidate);

            DwellingDesignAirFlowCandidate candidate = adjacencyCluster_Candidate.EvaluateTargetedDesignAirFlow(
                space_Candidate,
                FlowClassification.Supply,
                design_Candidate_Before_Lps + 10,
                ventilationUnitCapacityDescriptors: descriptors);

            Assert.False(candidate.IsAccepted);
            Assert.NotEmpty(candidate.Refusals);
            Assert.Equal(design_Candidate_Before_Lps, Design(adjacencyCluster_Candidate, space_Candidate, FlowClassification.Supply), 6);
            Assert.True(Helpers.JsonEquivalence.AreEquivalent(json_Before, Core.Convert.ToString(adjacencyCluster_Candidate), out string difference), difference);
        }

        /// <summary>
        /// The manual seam's OTHER equipment behaviour, restated unchanged: where the catalogue does hold a
        /// bigger product, a manual edit still escalates to it. A candidate never does - see
        /// <see cref="Candidate_BeyondSelectedCapacity_NeverReselects_EvenWhereABiggerProductIsOffered"/>.
        /// </summary>
        [Fact]
        public void ManualEdit_StillReselectsTheNextCapableProduct()
        {
            AdjacencyCluster adjacencyCluster = Selected(out _, out _);

            Space space = adjacencyCluster.GetSpaces().Find(x => x.Name == name_LivingRoom);

            DwellingDesignAirFlowChange change = adjacencyCluster.ApplyTargetedDesignAirFlow(
                space,
                FlowClassification.Supply,
                Design(adjacencyCluster, space, FlowClassification.Supply) + 10,
                ventilationUnitCapacityDescriptors: DwellingCatalogue());

            Assert.True(change.Successful, string.Join(" ", change.Refusals));
            Assert.Equal(VentilationUnitSelectionOutcome.Reselected, change.VentilationUnitSelectionOutcome);
            Assert.Equal("MVHR-35", SelectedModel(adjacencyCluster));
        }

        /// <summary>
        /// <b>An accepted candidate applies exactly what the manual seam would apply.</b> The commit is the
        /// existing engineering transaction rather than a reimplementation of it, so the same proposal
        /// through either route leaves every room's design airflow identical, targeted and derived alike.
        /// </summary>
        [Fact]
        public void AcceptedCandidate_AppliesTheSameDesignTheManualSeamWould()
        {
            AdjacencyCluster adjacencyCluster_Manual = Selected(out _, out _);
            Retarget(adjacencyCluster_Manual, name_LivingRoom, FlowClassification.Supply, 3);

            AdjacencyCluster adjacencyCluster_Candidate = Selected(out _, out _);
            Space space_Candidate = adjacencyCluster_Candidate.GetSpaces().Find(x => x.Name == name_LivingRoom);

            //Compared on the CANDIDATE's own model, before and after: the two fixtures are built
            //independently and their Approved Document F data carries their own space guids, so the two
            //routes' requirement text can only be compared with itself.
            Dictionary<string, string> requirements_Before = Requirements(adjacencyCluster_Candidate);

            DwellingDesignAirFlowCandidate candidate = adjacencyCluster_Candidate.EvaluateTargetedDesignAirFlow(
                space_Candidate,
                FlowClassification.Supply,
                Design(adjacencyCluster_Candidate, space_Candidate, FlowClassification.Supply) + 3,
                ventilationUnitCapacityDescriptors: DwellingCatalogue());

            Assert.True(candidate.IsAccepted, string.Join(" ", candidate.Refusals));

            //Room by room and side by side - the targeted change and every derived one.
            Assert.Equal(Designs(adjacencyCluster_Manual), Designs(candidate.AdjacencyCluster));

            //And no Approved Document F requirement moved to get there.
            Assert.Equal(requirements_Before, Requirements(candidate.AdjacencyCluster));
        }

        /// <summary>
        /// <b>Authority separation, on the committed model.</b> The selected product's capacity constrained
        /// the candidate and became nothing else: not an Approved Document F requirement, not a room's
        /// design airflow, and not a runtime airflow. A unit rated at 100 l/s serving a dwelling designed at
        /// 22.2 l/s leaves the dwelling designed at 22.2.
        /// </summary>
        [Fact]
        public void AcceptedCandidate_NeverTurnsCapacityIntoARequirement_ADesign_OrARuntimeAirflow()
        {
            //Catalogue() sizes the fixture flat onto MVHR-100 - headroom it must not spend.
            PartOIterationPreparation preparation = Prepared(Catalogue());

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;
            AirHandlingUnit airHandlingUnit = Assert.Single(adjacencyCluster.GetObjects<AirHandlingUnit>());
            Assert.Equal("MVHR-100", airHandlingUnit.SelectedVentilationUnitReference()?.Model);

            Space space = adjacencyCluster.GetSpaces().Find(x => x.Name == name_LivingRoom);

            Dictionary<string, string> requirements_Before = Requirements(adjacencyCluster);
            List<string> runtimeAirflows_Before = RuntimeAirflows(adjacencyCluster);

            DwellingDesignAirFlowCandidate candidate = adjacencyCluster.EvaluateTargetedDesignAirFlow(
                space,
                FlowClassification.Supply,
                Design(adjacencyCluster, space, FlowClassification.Supply) + 3,
                ventilationUnitCapacityDescriptors: Catalogue());

            Assert.True(candidate.IsAccepted, string.Join(" ", candidate.Refusals));

            //The design duty is what was asked for, never the rating that permitted it.
            Assert.Equal(22.2, candidate.SupplyDuty_After_Lps, 3);
            Assert.Equal(77.8, candidate.SupplyHeadroom_Lps, 3);
            Assert.Equal(100, candidate.VentilationUnitCapacityDescriptor.MaximumSupplyFlowRate_Lps, 6);

            //No room's design airflow, on either side, went anywhere near the capacity.
            foreach (KeyValuePair<string, double> keyValuePair in Designs(candidate.AdjacencyCluster))
            {
                Assert.True(keyValuePair.Value < 100, keyValuePair.Key);
            }

            //Every Approved Document F requirement is the same requirement, on the committed model.
            Assert.Equal(requirements_Before, Requirements(candidate.AdjacencyCluster));

            //And no runtime airflow was written - that is the preparation's job, not a design change's.
            Assert.Equal(runtimeAirflows_Before, RuntimeAirflows(candidate.AdjacencyCluster));
        }

        /// <summary>
        /// A candidate the airflow transaction itself refuses - here a design below what Approved Document F
        /// requires of the targeted room - is rejected with that refusal, and equally leaves nothing behind.
        /// The Part F floor is enforced by the existing transaction, never re-derived by the candidate.
        /// </summary>
        [Fact]
        public void Candidate_BelowThePartFFloor_IsRefusedByTheAirflowTransaction()
        {
            AdjacencyCluster adjacencyCluster = Selected(out _, out _);

            Space space = adjacencyCluster.GetSpaces().Find(x => x.Name == name_LivingRoom);

            double? requirement_Lps = adjacencyCluster.PartFRequiredFlowRate_Lps(space, FlowClassification.Supply);
            Assert.True(requirement_Lps > 1);

            string json_Before = Core.Convert.ToString(adjacencyCluster);

            DwellingDesignAirFlowCandidate candidate = adjacencyCluster.EvaluateTargetedDesignAirFlow(
                space,
                FlowClassification.Supply,
                requirement_Lps.Value - 1,
                ventilationUnitCapacityDescriptors: DwellingCatalogue());

            Assert.False(candidate.IsAccepted);
            Assert.Null(candidate.AdjacencyCluster);
            Assert.Contains(candidate.Refusals, x => x.Contains("Approved Document F requires"));

            //Equipment was never reached - the airflow change was impossible before capacity could matter.
            Assert.Equal(VentilationUnitSelectionOutcome.NotApplicable, candidate.VentilationUnitSelectionOutcome);

            Assert.True(Helpers.JsonEquivalence.AreEquivalent(json_Before, Core.Convert.ToString(adjacencyCluster), out string difference), difference);
        }

        /// <summary>
        /// No catalogue offered means equipment is no constraint on the candidate - the same
        /// backward-compatible meaning it has for a manual edit, so a caller who never selected a product
        /// can still explore a design.
        /// </summary>
        [Fact]
        public void Candidate_WithoutACatalogue_TreatsEquipmentAsNoConstraint()
        {
            AdjacencyCluster adjacencyCluster = Selected(out _, out _);

            Space space = adjacencyCluster.GetSpaces().Find(x => x.Name == name_LivingRoom);

            //Well past MVHR-25 - refused outright had a catalogue been offered.
            DwellingDesignAirFlowCandidate candidate = adjacencyCluster.EvaluateTargetedDesignAirFlow(
                space,
                FlowClassification.Supply,
                Design(adjacencyCluster, space, FlowClassification.Supply) + 10);

            Assert.True(candidate.IsAccepted, string.Join(" ", candidate.Refusals));
            Assert.Equal(VentilationUnitSelectionOutcome.NotApplicable, candidate.VentilationUnitSelectionOutcome);
            Assert.Null(candidate.VentilationUnitCapacityDescriptor);
            Assert.True(double.IsNaN(candidate.SupplyHeadroom_Lps));
        }

        /// <summary>
        /// A unit nothing has ever been selected for constrains nothing, exactly as it validates nothing for
        /// a manual edit - a candidate does not make a first selection on its behalf.
        /// </summary>
        [Fact]
        public void Candidate_WithNoProductEverSelected_IsNotConstrainedByEquipment()
        {
            //Iteration 1a: no catalogue at preparation, so nothing was ever selected.
            PartOIterationPreparation preparation = Prepared(null);

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;
            AirHandlingUnit airHandlingUnit = Assert.Single(adjacencyCluster.GetObjects<AirHandlingUnit>());
            Assert.Null(airHandlingUnit.SelectedVentilationUnitReference());

            Space space = adjacencyCluster.GetSpaces().Find(x => x.Name == name_LivingRoom);

            DwellingDesignAirFlowCandidate candidate = adjacencyCluster.EvaluateTargetedDesignAirFlow(
                space,
                FlowClassification.Supply,
                Design(adjacencyCluster, space, FlowClassification.Supply) + 3,
                ventilationUnitCapacityDescriptors: DwellingCatalogue());

            Assert.True(candidate.IsAccepted, string.Join(" ", candidate.Refusals));
            Assert.Equal(VentilationUnitSelectionOutcome.NotApplicable, candidate.VentilationUnitSelectionOutcome);

            //Still nothing selected, on the committed model as well as the original.
            Assert.Null(airHandlingUnit.SelectedVentilationUnitReference());
            Assert.Null(Assert.Single(candidate.AdjacencyCluster.GetObjects<AirHandlingUnit>()).SelectedVentilationUnitReference());
        }

        /// <summary>
        /// <b>An unknown capacity rejects the candidate rather than passing it.</b> The currently selected
        /// product is not among the products this call was given, so whether it could carry the candidate is
        /// unknown - and an optimisation must not commit a design on an unknown. The same conservatism
        /// <c>Query.IsVentilationUnitSufficient</c> already applies for a manual edit.
        /// </summary>
        [Fact]
        public void Candidate_WhereTheSelectedProductIsNotInTheCatalogue_IsRefusedAsUnknown()
        {
            AdjacencyCluster adjacencyCluster = Selected(out _, out _);

            //A catalogue that omits MVHR-25, at a duty every remaining product could carry easily.
            List<VentilationUnitCapacityDescriptor> descriptors = [Descriptor("MVHR-35", 35, 35), Descriptor("MVHR-50", 50, 50)];

            Space space = adjacencyCluster.GetSpaces().Find(x => x.Name == name_LivingRoom);

            DwellingDesignAirFlowCandidate candidate = adjacencyCluster.EvaluateTargetedDesignAirFlow(
                space,
                FlowClassification.Supply,
                Design(adjacencyCluster, space, FlowClassification.Supply) + 3,
                ventilationUnitCapacityDescriptors: descriptors);

            Assert.False(candidate.IsAccepted);
            Assert.Null(candidate.AdjacencyCluster);
            Assert.Equal(VentilationUnitSelectionOutcome.Refused, candidate.VentilationUnitSelectionOutcome);
            Assert.Contains("not among the ventilation unit products offered", candidate.VentilationUnitSelectionReason);
        }

        /// <summary>
        /// <b>Headroom is not a target.</b> A dwelling designed at 19.2 l/s behind a unit rated at 100 is
        /// left designed at 19.2 - a candidate that proposes no change reports 80.8 l/s of headroom and
        /// proposes taking none of it. This is the accepted three-flat fixture's invariant in miniature:
        /// 30/30 and 63/63 l/s stay where they are behind a 150/150 unit.
        /// </summary>
        [Fact]
        public void Candidate_DoesNotSpendAvailableHeadroom()
        {
            PartOIterationPreparation preparation = Prepared(Catalogue());

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            Space space = adjacencyCluster.GetSpaces().Find(x => x.Name == name_LivingRoom);
            double design_Before_Lps = Design(adjacencyCluster, space, FlowClassification.Supply);

            //The design it already has, proposed again - the only thing a candidate ever evaluates is what
            //it was asked to evaluate.
            DwellingDesignAirFlowCandidate candidate = adjacencyCluster.EvaluateTargetedDesignAirFlow(
                space,
                FlowClassification.Supply,
                design_Before_Lps,
                ventilationUnitCapacityDescriptors: Catalogue());

            Assert.True(candidate.IsAccepted, string.Join(" ", candidate.Refusals));
            Assert.Equal(80.8, candidate.SupplyHeadroom_Lps, 3);
            Assert.Empty(candidate.DerivedAdjustments);
            Assert.Equal(design_Before_Lps, candidate.TargetedAdjustment.After_Lps, 6);
            Assert.Equal(Designs(adjacencyCluster), Designs(candidate.AdjacencyCluster));
        }

        /// <summary>
        /// One dwelling's candidate never reads, reports on or writes another dwelling's - the isolation
        /// <c>EquipmentValidation_TwoDwellings_StayIndependent</c> pins for the manual seam, restated for
        /// the candidate boundary.
        /// </summary>
        [Fact]
        public void Candidate_TwoDwellings_StayIndependent()
        {
            List<VentilationUnitCapacityDescriptor> descriptors = [Descriptor("MVHR-50", 50, 50)];

            PartOIterationPreparation preparation = Prepare(TwoDwellingModel(), descriptors);
            Assert.Null(preparation.Refusal);

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            List<AirHandlingUnit> airHandlingUnits = adjacencyCluster.GetObjects<AirHandlingUnit>();
            Assert.Equal(2, airHandlingUnits.Count);

            adjacencyCluster.AirHandlingUnitDesignDuty(airHandlingUnits[1], out double supplyDuty_2_Before_Lps, out _);
            VentilationUnitReference ventilationUnitReference_2_Before = airHandlingUnits[1].SelectedVentilationUnitReference();

            //The living room of whichever flat unit 0 happens to serve - resolved through the unit's own
            //system rather than by name, so the test never depends on the order the units came back in.
            Space space = LivingRoom(adjacencyCluster, airHandlingUnits[0]);

            //Push that dwelling well past the only product offered.
            DwellingDesignAirFlowCandidate candidate = adjacencyCluster.EvaluateTargetedDesignAirFlow(
                space,
                FlowClassification.Supply,
                Design(adjacencyCluster, space, FlowClassification.Supply) + 100,
                ventilationUnitCapacityDescriptors: descriptors);

            Assert.False(candidate.IsAccepted);
            Assert.Equal(airHandlingUnits[0].Guid, candidate.AirHandlingUnit.Guid);

            //Dwelling 2's duty and selection are untouched by dwelling 1's rejected candidate.
            adjacencyCluster.AirHandlingUnitDesignDuty(airHandlingUnits[1], out double supplyDuty_2_After_Lps, out _);
            Assert.Equal(supplyDuty_2_Before_Lps, supplyDuty_2_After_Lps, 6);
            Assert.True(ventilationUnitReference_2_Before.Matches(airHandlingUnits[1].SelectedVentilationUnitReference()));
        }

        /// <summary>No model is not a crash, and it is not an accepted candidate either.</summary>
        [Fact]
        public void Candidate_WithoutAModel_IsRefused()
        {
            DwellingDesignAirFlowCandidate candidate = ((AdjacencyCluster)null).EvaluateTargetedDesignAirFlow(null, FlowClassification.Supply, 10);

            Assert.NotNull(candidate);
            Assert.False(candidate.IsAccepted);
            Assert.Null(candidate.AdjacencyCluster);
            Assert.NotEmpty(candidate.Refusals);
        }


        // =================================================================================================
        // M. Iteration 2B - the clamp. Modify.ResolveTargetedDesignAirFlow bisects the SAME candidate
        //    section L pins, so every answer it gives is a candidate that genuinely passed. What it adds
        //    is the answer to the question a single candidate cannot answer: not "is 40 l/s feasible"
        //    but "how much CAN this room have, against the unit already selected". The request bounds it
        //    on one side and the room's existing design bounds it on the other, so it can neither
        //    overshoot what was asked for nor move the room the other way to get there.
        // =================================================================================================

        /// <summary>
        /// <b>The ordinary case costs nothing.</b> A request the dwelling can carry is met exactly, in one
        /// evaluation, with no search and no bracket - the answer an engineer would have got by evaluating
        /// that one candidate themselves.
        /// </summary>
        [Fact]
        public void Resolution_AFeasibleRequest_IsMetExactlyInOneEvaluation()
        {
            AdjacencyCluster adjacencyCluster = Selected(out _, out _);

            Space space = adjacencyCluster.GetSpaces().Find(x => x.Name == name_LivingRoom);
            double design_Before_Lps = Design(adjacencyCluster, space, FlowClassification.Supply);

            //22.2 l/s against MVHR-25 - inside the rating, exactly as the candidate test uses it.
            DwellingDesignAirFlowResolution resolution = adjacencyCluster.ResolveTargetedDesignAirFlow(
                space,
                FlowClassification.Supply,
                design_Before_Lps + 3,
                ventilationUnitCapacityDescriptors: DwellingCatalogue());

            Assert.True(resolution.IsAccepted, string.Join(" ", resolution.Refusals));
            Assert.True(resolution.IsRequestSatisfied);
            Assert.True(resolution.IsChanged);
            Assert.Null(resolution.LimitingReason);
            Assert.Equal(1, resolution.Evaluations);

            Assert.Equal(design_Before_Lps + 3, resolution.Requested_Lps, 6);
            Assert.Equal(design_Before_Lps + 3, resolution.Achieved_Lps, 6);

            //The whole candidate contract is reachable through the answer, unchanged.
            Assert.Equal(19.2, resolution.SupplyDuty_Before_Lps, 3);
            Assert.Equal(22.2, resolution.SupplyDuty_After_Lps, 3);
            Assert.Equal(22.2, resolution.ExtractDuty_After_Lps, 3);
            Assert.Equal(2.8, resolution.SupplyHeadroom_Lps, 3);
            Assert.Equal("MVHR-25", resolution.VentilationUnitReference?.Model);
            Assert.NotEmpty(resolution.DerivedAdjustments);

            //And the model to adopt genuinely carries it.
            Assert.NotNull(resolution.AdjacencyCluster);
            Assert.Equal(design_Before_Lps + 3, Design(resolution.AdjacencyCluster, space, FlowClassification.Supply), 6);
        }

        /// <summary>
        /// <b>The whole point.</b> A request past the selected unit's rating is not refused - it is clamped
        /// to the greatest design airflow that unit will carry, and says so. The dwelling is sized at
        /// 19.2 l/s on MVHR-25, so 5.8 l/s is what the living room can actually take up.
        /// </summary>
        [Fact]
        public void Resolution_BeyondSelectedCapacity_ClampsToTheGreatestFeasibleDesign()
        {
            AdjacencyCluster adjacencyCluster = Selected(out _, out _);

            Space space = adjacencyCluster.GetSpaces().Find(x => x.Name == name_LivingRoom);
            double design_Before_Lps = Design(adjacencyCluster, space, FlowClassification.Supply);

            //+10 would design the dwelling at 29.2 l/s - 4.2 l/s past MVHR-25, and refused outright as a
            //single candidate.
            DwellingDesignAirFlowResolution resolution = adjacencyCluster.ResolveTargetedDesignAirFlow(
                space,
                FlowClassification.Supply,
                design_Before_Lps + 10,
                ventilationUnitCapacityDescriptors: DwellingCatalogue());

            //Accepted - and unmistakably NOT what was asked for.
            Assert.True(resolution.IsAccepted, string.Join(" ", resolution.Refusals));
            Assert.False(resolution.IsRequestSatisfied);
            Assert.True(resolution.IsChanged);
            Assert.Empty(resolution.Refusals);

            Assert.Equal(design_Before_Lps + 10, resolution.Requested_Lps, 6);
            Assert.Equal(design_Before_Lps + 5.8, resolution.Achieved_Lps, 2);

            //It searched, and it said what stopped it.
            Assert.True(resolution.Evaluations > 2);
            Assert.NotNull(resolution.LimitingReason);
            Assert.Contains("cannot carry", resolution.LimitingReason);

            //The dwelling now designs the unit's rating and not a litre more, on both sides.
            Assert.Equal(25, resolution.SupplyDuty_After_Lps, 2);
            Assert.Equal(25, resolution.ExtractDuty_After_Lps, 2);
            Assert.Equal(0, resolution.SupplyHeadroom_Lps, 2);

            //And the answer is a real candidate: still balanced, still Approved Document F compliant.
            Assert.NotNull(resolution.AdjacencyCluster);
            resolution.AdjacencyCluster.ReconcileVentilationSystemDesignDuty(resolution.VentilationSystem, out _, out _, out List<string> refusals_Compliance);
            Assert.Empty(refusals_Compliance);

            resolution.AdjacencyCluster.VentilationSystemDesignDuty(resolution.VentilationSystem, out double supplyDuty_Lps, out double extractDuty_Lps);
            Assert.Equal(supplyDuty_Lps, extractDuty_Lps, 6);
        }

        /// <summary>
        /// <b>The clamped answer is the GREATEST feasible one, not merely a feasible one.</b> This is what
        /// makes the search worth having, and the only thing a bisection could plausibly get wrong: a
        /// tenth of a litre more is refused by the very evaluation the answer came from.
        /// </summary>
        [Fact]
        public void Resolution_ClampedAnswer_IsTheGreatestFeasibleDesign()
        {
            AdjacencyCluster adjacencyCluster = Selected(out _, out _);

            Space space = adjacencyCluster.GetSpaces().Find(x => x.Name == name_LivingRoom);

            DwellingDesignAirFlowResolution resolution = adjacencyCluster.ResolveTargetedDesignAirFlow(
                space,
                FlowClassification.Supply,
                Design(adjacencyCluster, space, FlowClassification.Supply) + 10,
                ventilationUnitCapacityDescriptors: DwellingCatalogue());

            Assert.True(resolution.IsAccepted, string.Join(" ", resolution.Refusals));

            //Feasible where it landed...
            DwellingDesignAirFlowCandidate candidate_At = adjacencyCluster.EvaluateTargetedDesignAirFlow(
                space,
                FlowClassification.Supply,
                resolution.Achieved_Lps,
                ventilationUnitCapacityDescriptors: DwellingCatalogue());

            Assert.True(candidate_At.IsAccepted, string.Join(" ", candidate_At.Refusals));

            //...and infeasible a tenth of a litre further on, which bounds the answer to well inside the
            //0.001 l/s tolerance it was resolved to.
            DwellingDesignAirFlowCandidate candidate_Past = adjacencyCluster.EvaluateTargetedDesignAirFlow(
                space,
                FlowClassification.Supply,
                resolution.Achieved_Lps + 0.1,
                ventilationUnitCapacityDescriptors: DwellingCatalogue());

            Assert.False(candidate_Past.IsAccepted);
        }

        /// <summary>
        /// <b>A request far larger than the bracket a fixed halving budget could close still clamps to the
        /// unit's rating</b> - it does not quietly give up and answer "no change possible".
        /// <para>
        /// Bisection needs about <c>log2(width / tolerance)</c> halvings, so a
        /// request 1e18 l/s above the existing design needs roughly seventy against a 0.001 l/s tolerance.
        /// A fixed budget of sixty exits with the feasible bound still sitting on the anchor - the search
        /// returns the unchanged design, reports it as the closest feasible value, and claims it was
        /// resolved to within the tolerance. All three are wrong, and none of them are visible to the
        /// caller. The budget is now derived from the bracket and the tolerance instead.
        /// </para>
        /// </summary>
        [Fact]
        public void Resolution_AVastRequest_StillClampsToTheSelectedUnitsRating()
        {
            AdjacencyCluster adjacencyCluster = Selected(out _, out _);

            Space space = adjacencyCluster.GetSpaces().Find(x => x.Name == name_LivingRoom);
            double design_Before_Lps = Design(adjacencyCluster, space, FlowClassification.Supply);

            DwellingDesignAirFlowResolution resolution = adjacencyCluster.ResolveTargetedDesignAirFlow(
                space,
                FlowClassification.Supply,
                design_Before_Lps + 1e18,
                ventilationUnitCapacityDescriptors: DwellingCatalogue());

            Assert.True(resolution.IsAccepted, string.Join(" ", resolution.Refusals));
            Assert.False(resolution.IsRequestSatisfied);

            //The answer the fixed budget could not reach: the room really can take 5.8 l/s more, and
            //saying otherwise would send an engineer to buy a bigger unit they do not need.
            Assert.True(resolution.IsChanged);
            Assert.Equal(design_Before_Lps + 5.8, resolution.Achieved_Lps, 2);
            Assert.Equal(25, resolution.SupplyDuty_After_Lps, 2);

            //And it paid for the answer rather than for the size of the number: log2(1e18 / 0.001) is about
            //seventy halvings, plus the request and the anchor. A budget that followed the bracket without
            //bound would be the opposite defect.
            Assert.InRange(resolution.Evaluations, 3, 100);

            AssertPartFFloorsAreMet(resolution.AdjacencyCluster);
        }

        /// <summary>
        /// <b>The request is a ceiling, never a target to grow into.</b> A dwelling on a 50 l/s unit asked
        /// for 3 l/s more gets 3 l/s more - the search stops at what was asked for and leaves the remaining
        /// headroom exactly where it was, reported and unspent.
        /// </summary>
        [Fact]
        public void Resolution_NeverSpendsHeadroomTheRequestDidNotAskFor()
        {
            AdjacencyCluster adjacencyCluster = Selected(out AirHandlingUnit airHandlingUnit, out _);

            List<VentilationUnitCapacityDescriptor> descriptors = [Descriptor("MVHR-50", 50, 50)];

            //Re-selected deliberately onto a unit with a great deal of room to spare.
            adjacencyCluster.SelectVentilationUnit(airHandlingUnit, descriptors, out _, out _);

            Space space = adjacencyCluster.GetSpaces().Find(x => x.Name == name_LivingRoom);
            double design_Before_Lps = Design(adjacencyCluster, space, FlowClassification.Supply);

            DwellingDesignAirFlowResolution resolution = adjacencyCluster.ResolveTargetedDesignAirFlow(
                space,
                FlowClassification.Supply,
                design_Before_Lps + 3,
                ventilationUnitCapacityDescriptors: descriptors);

            Assert.True(resolution.IsAccepted, string.Join(" ", resolution.Refusals));
            Assert.True(resolution.IsRequestSatisfied);

            //Exactly what was asked for - not the 30.8 l/s the unit would have carried.
            Assert.Equal(design_Before_Lps + 3, resolution.Achieved_Lps, 6);
            Assert.Equal(22.2, resolution.SupplyDuty_After_Lps, 3);
            Assert.Equal(27.8, resolution.SupplyHeadroom_Lps, 3);
        }

        /// <summary>
        /// <b>The selected unit is the constraint, never a variable.</b> A request past MVHR-25 is clamped
        /// to MVHR-25 even though MVHR-35 and MVHR-50 are sitting in the same catalogue - a clamp explores
        /// within the equipment somebody chose, and does not buy bigger equipment as a side effect. The
        /// rule <c>Candidate_BeyondSelectedCapacity_NeverReselects_EvenWhereABiggerProductIsOffered</c>
        /// states for one candidate, restated for the search built on it.
        /// </summary>
        [Fact]
        public void Resolution_NeverReselectsALargerProduct_EvenWhereOneIsOffered()
        {
            AdjacencyCluster adjacencyCluster = Selected(out _, out _);

            List<string> selected_Before = SelectedModels(adjacencyCluster);

            Space space = adjacencyCluster.GetSpaces().Find(x => x.Name == name_LivingRoom);

            DwellingDesignAirFlowResolution resolution = adjacencyCluster.ResolveTargetedDesignAirFlow(
                space,
                FlowClassification.Supply,
                Design(adjacencyCluster, space, FlowClassification.Supply) + 10,
                ventilationUnitCapacityDescriptors: DwellingCatalogue());

            Assert.True(resolution.IsAccepted, string.Join(" ", resolution.Refusals));

            //Kept, on the answer and in the model it hands back - never Reselected.
            Assert.Equal(VentilationUnitSelectionOutcome.Kept, resolution.Candidate.VentilationUnitSelectionOutcome);
            Assert.Equal("MVHR-25", resolution.VentilationUnitReference?.Model);
            Assert.Equal(25, resolution.VentilationUnitCapacityDescriptor.MaximumSupplyFlowRate_Lps, 6);

            Assert.Equal(selected_Before, SelectedModels(resolution.AdjacencyCluster));
            Assert.Equal(selected_Before, SelectedModels(adjacencyCluster));
        }

        /// <summary>
        /// <b>A reduction clamps too, and on an entirely different limit.</b> Nothing about equipment is
        /// involved: the supply room can be taken down only as far as the extract side holds design
        /// headroom above its own Approved Document F floors. <c>AReductionBeyondAllAvailableHeadroom</c>
        /// refuses 30 to 10 outright; the clamp answers 28, which
        /// <c>AReductionExactlyAtAvailableHeadroom_Succeeds</c> independently proves is the boundary.
        /// </summary>
        [Fact]
        public void Resolution_AReduction_ClampsToTheOppositeSidesPartFHeadroom()
        {
            AdjacencyCluster adjacencyCluster = HeadroomFixture(out Space space_Supply, out Space space_Extract);

            //Supply 30 -> 10 needs 20 l/s off the extract side, which holds only 2 l/s above its floor.
            DwellingDesignAirFlowResolution resolution = adjacencyCluster.ResolveTargetedDesignAirFlow(space_Supply, FlowClassification.Supply, 10);

            Assert.True(resolution.IsAccepted, string.Join(" ", resolution.Refusals));
            Assert.False(resolution.IsRequestSatisfied);
            Assert.True(resolution.IsChanged);

            Assert.Equal(10, resolution.Requested_Lps, 6);
            Assert.Equal(28, resolution.Achieved_Lps, 2);

            Assert.NotNull(resolution.LimitingReason);
            Assert.Contains("headroom above what Approved Document F requires", resolution.LimitingReason);

            //The derived side went to its floor and no further, in the model handed back.
            Assert.Equal(28, Design(resolution.AdjacencyCluster, space_Extract, FlowClassification.Extract), 6);

            //Balanced to within the tolerance the whole transaction is defined against - and NOT to more
            //than that. The extract side stops dead on its own Approved Document F floor, so the last
            //sliver of supply the search can still take is the sliver the BALANCE check tolerates, and the
            //supply room lands at 27.999 rather than 28.
            //
            //That is legal, and it is a different thing entirely from the floor defect
            //Resolution_BelowTheTargetRoomsPartFFloor pins: the room holding 27.999 here is the SUPPLY
            //room, whose own Approved Document F requirement is 10 l/s. Nothing in this answer is below
            //its floor - which the assertion below states outright rather than leaving to be inferred.
            Assert.True(System.Math.Abs(resolution.SupplyDuty_After_Lps - resolution.ExtractDuty_After_Lps) <= 0.001);
            Assert.Equal(28, resolution.ExtractDuty_After_Lps, 6);

            AssertPartFFloorsAreMet(resolution.AdjacencyCluster);

            //And the model it was resolved against is untouched.
            Assert.Equal(30, Design(adjacencyCluster, space_Supply, FlowClassification.Supply), 6);
            Assert.Equal(30, Design(adjacencyCluster, space_Extract, FlowClassification.Extract), 6);
        }

        /// <summary>
        /// <b>A request below the targeted room's own Approved Document F floor clamps to EXACTLY that
        /// floor</b> - not to the last value the comparison would have tolerated below it.
        /// <para>
        /// <b>This is the case that found the floor-integrity defect.</b> A bounded search drives
        /// deliberately at the edge of every constraint, and the edge of the Part F floor was
        /// <c>design + tolerance &lt; requirement</c> - so 27.999 l/s against a 28 l/s requirement passed
        /// every check and was written, and the model persisted a room designed below what Approved
        /// Document F requires of it. Tolerance decides whether two airflows are the same number; it is
        /// not permission to record a design below the regulatory minimum. The existing authority now
        /// raises a within-tolerance request to the requirement before planning anything from it.
        /// </para>
        /// </summary>
        [Fact]
        public void Resolution_BelowTheTargetRoomsPartFFloor_ClampsToTheFloor_NeverBelowIt()
        {
            AdjacencyCluster adjacencyCluster = HeadroomFixture(out Space space_Supply, out Space space_Extract);

            //The extract room requires 28 and is designed at 30. Asking for 20 is asking for less than
            //Approved Document F allows - the supply side could easily have absorbed it.
            DwellingDesignAirFlowResolution resolution = adjacencyCluster.ResolveTargetedDesignAirFlow(space_Extract, FlowClassification.Extract, 20);

            Assert.True(resolution.IsAccepted, string.Join(" ", resolution.Refusals));
            Assert.False(resolution.IsRequestSatisfied);

            //EXACTLY the requirement - asserted to nine decimal places, because 27.999 is precisely the
            //answer this test is here to rule out.
            Assert.Equal(28, resolution.Achieved_Lps, 9);
            Assert.True(resolution.Achieved_Lps >= 28);

            //And the targeted adjustment reports the value that was actually written, not the one asked
            //for and not the one the bisection last tried.
            Assert.Equal(28, resolution.TargetedAdjustment.After_Lps, 9);
            Assert.Equal(30, resolution.TargetedAdjustment.Before_Lps, 9);
            Assert.Equal(28, Design(resolution.AdjacencyCluster, space_Extract, FlowClassification.Extract), 9);

            Assert.NotNull(resolution.LimitingReason);
            Assert.Contains("Approved Document F requires of that room", resolution.LimitingReason);

            //The balancing side was planned from 28 and not from 27.999, so the dwelling comes out square
            //rather than out by exactly the amount the floor correction moved.
            Assert.Equal(28, Design(resolution.AdjacencyCluster, space_Supply, FlowClassification.Supply), 9);
            Assert.True(System.Math.Abs(resolution.SupplyDuty_After_Lps - resolution.ExtractDuty_After_Lps) <= 0.001);

            //And no room anywhere in the answer sits below its own floor.
            AssertPartFFloorsAreMet(resolution.AdjacencyCluster);
        }

        /// <summary>
        /// <b>Where nothing at all is feasible, the search refuses and carries no model.</b> A dwelling that
        /// is not a valid design to change - here one already below an Approved Document F floor in a room
        /// nobody targeted - cannot carry its OWN design, so no value between it and the request could be
        /// valid either. The search says that in as many words rather than bisecting a bracket with no
        /// feasible point in it, and it costs two evaluations to find out.
        /// </summary>
        [Fact]
        public void Resolution_WhereTheDwellingItselfIsInvalid_RefusesAndCarriesNoModel()
        {
            AdjacencyCluster adjacencyCluster = ShortfallFixture(out Space space_Supply, out _, out _);

            Dictionary<string, double> design_Before = Designs(adjacencyCluster);

            DwellingDesignAirFlowResolution resolution = adjacencyCluster.ResolveTargetedDesignAirFlow(space_Supply, FlowClassification.Supply, 30);

            Assert.False(resolution.IsAccepted);
            Assert.Null(resolution.AdjacencyCluster);
            Assert.True(double.IsNaN(resolution.Achieved_Lps));
            Assert.False(resolution.IsRequestSatisfied);
            Assert.False(resolution.IsChanged);

            //The request, then the design as it stands - and no bisection at all.
            Assert.Equal(2, resolution.Evaluations);

            Assert.Contains(resolution.Refusals, x => x.Contains("does not currently meet Approved Document F"));
            Assert.Contains(resolution.Refusals, x => x.Contains("the dwelling's design as it already stands was rejected on the same terms"));

            Assert.Equal(design_Before, Designs(adjacencyCluster));
        }

        /// <summary>
        /// <b>The model the search was given is never touched</b> - not by the clamped answer it accepted,
        /// and not by any of the candidates it rejected on the way there. Compared as whole clusters, so
        /// this covers every terminal and every relation, not merely the room totals.
        /// </summary>
        [Fact]
        public void Resolution_LeavesTheEvaluatedModelUntouched()
        {
            AdjacencyCluster adjacencyCluster = Selected(out _, out _);

            string json_Before = Core.Convert.ToString(adjacencyCluster);
            Dictionary<Guid, double> terminals_Before = TerminalDesigns(adjacencyCluster);

            Space space = adjacencyCluster.GetSpaces().Find(x => x.Name == name_LivingRoom);

            DwellingDesignAirFlowResolution resolution = adjacencyCluster.ResolveTargetedDesignAirFlow(
                space,
                FlowClassification.Supply,
                Design(adjacencyCluster, space, FlowClassification.Supply) + 10,
                ventilationUnitCapacityDescriptors: DwellingCatalogue());

            //A clamped answer - so the search definitely rejected candidates along the way.
            Assert.True(resolution.IsAccepted, string.Join(" ", resolution.Refusals));
            Assert.False(resolution.IsRequestSatisfied);
            Assert.True(resolution.Evaluations > 2);

            Assert.Equal(terminals_Before, TerminalDesigns(adjacencyCluster));
            Assert.True(Helpers.JsonEquivalence.AreEquivalent(json_Before, Core.Convert.ToString(adjacencyCluster), out string difference), difference);
        }

        /// <summary>
        /// A request that IS the room's existing design is satisfied, in one evaluation, and changes
        /// nothing - so a caller can ask without having to check first whether asking is a no-op.
        /// </summary>
        [Fact]
        public void Resolution_AtTheRoomsExistingDesign_IsSatisfiedAndChangesNothing()
        {
            AdjacencyCluster adjacencyCluster = Selected(out _, out _);

            Space space = adjacencyCluster.GetSpaces().Find(x => x.Name == name_LivingRoom);
            double design_Before_Lps = Design(adjacencyCluster, space, FlowClassification.Supply);

            DwellingDesignAirFlowResolution resolution = adjacencyCluster.ResolveTargetedDesignAirFlow(
                space,
                FlowClassification.Supply,
                design_Before_Lps,
                ventilationUnitCapacityDescriptors: DwellingCatalogue());

            Assert.True(resolution.IsAccepted, string.Join(" ", resolution.Refusals));
            Assert.True(resolution.IsRequestSatisfied);
            Assert.False(resolution.IsChanged);
            Assert.Equal(1, resolution.Evaluations);
            Assert.Equal(design_Before_Lps, resolution.Achieved_Lps, 6);
            Assert.Empty(resolution.DerivedAdjustments);
        }

        /// <summary>
        /// No catalogue offered means equipment is no constraint on the search either - the same
        /// backward-compatible meaning it has for a candidate and for a manual edit. The request is then
        /// simply met.
        /// </summary>
        [Fact]
        public void Resolution_WithoutACatalogue_TreatsEquipmentAsNoConstraint()
        {
            AdjacencyCluster adjacencyCluster = Selected(out _, out _);

            Space space = adjacencyCluster.GetSpaces().Find(x => x.Name == name_LivingRoom);

            //Well past MVHR-25 - clamped hard had a catalogue been offered.
            DwellingDesignAirFlowResolution resolution = adjacencyCluster.ResolveTargetedDesignAirFlow(
                space,
                FlowClassification.Supply,
                Design(adjacencyCluster, space, FlowClassification.Supply) + 10);

            Assert.True(resolution.IsAccepted, string.Join(" ", resolution.Refusals));
            Assert.True(resolution.IsRequestSatisfied);
            Assert.Equal(1, resolution.Evaluations);
            Assert.Equal(VentilationUnitSelectionOutcome.NotApplicable, resolution.Candidate.VentilationUnitSelectionOutcome);
            Assert.True(double.IsNaN(resolution.SupplyHeadroom_Lps));
        }

        /// <summary>
        /// <b>Authority separation, on a clamped answer.</b> The capacity that decided where the search
        /// stopped became nothing else: not an Approved Document F requirement, and not a runtime airflow.
        /// A design that was limited BY the equipment is exactly the case where capacity would leak into
        /// the design if it were ever going to.
        /// </summary>
        [Fact]
        public void Resolution_ClampedAnswer_WritesNoRequirementAndNoRuntimeAirflow()
        {
            AdjacencyCluster adjacencyCluster = Selected(out _, out _);

            Dictionary<string, string> requirements_Before = Requirements(adjacencyCluster);
            List<string> runtimeAirflows_Before = RuntimeAirflows(adjacencyCluster);

            Space space = adjacencyCluster.GetSpaces().Find(x => x.Name == name_LivingRoom);

            DwellingDesignAirFlowResolution resolution = adjacencyCluster.ResolveTargetedDesignAirFlow(
                space,
                FlowClassification.Supply,
                Design(adjacencyCluster, space, FlowClassification.Supply) + 10,
                ventilationUnitCapacityDescriptors: DwellingCatalogue());

            Assert.True(resolution.IsAccepted, string.Join(" ", resolution.Refusals));
            Assert.False(resolution.IsRequestSatisfied);

            //Every Approved Document F requirement is the same requirement, on the model handed back.
            Assert.Equal(requirements_Before, Requirements(resolution.AdjacencyCluster));

            //And no runtime airflow was written - that is the preparation's job, not a design search's.
            Assert.Equal(runtimeAirflows_Before, RuntimeAirflows(resolution.AdjacencyCluster));

            //No room's design airflow became the rating that limited it, either.
            foreach (KeyValuePair<string, double> keyValuePair in Designs(resolution.AdjacencyCluster))
            {
                Assert.True(keyValuePair.Value < 25, keyValuePair.Key);
            }
        }

        /// <summary>
        /// One dwelling's search never reads, reports on or writes another dwelling's - the isolation
        /// <c>Candidate_TwoDwellings_StayIndependent</c> pins for one candidate, restated for the many a
        /// search evaluates.
        /// </summary>
        [Fact]
        public void Resolution_TwoDwellings_StayIndependent()
        {
            List<VentilationUnitCapacityDescriptor> descriptors = [Descriptor("MVHR-50", 50, 50)];

            PartOIterationPreparation preparation = Prepare(TwoDwellingModel(), descriptors);
            Assert.Null(preparation.Refusal);

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            List<AirHandlingUnit> airHandlingUnits = adjacencyCluster.GetObjects<AirHandlingUnit>();
            Assert.Equal(2, airHandlingUnits.Count);

            adjacencyCluster.AirHandlingUnitDesignDuty(airHandlingUnits[1], out double supplyDuty_2_Before_Lps, out _);
            VentilationUnitReference ventilationUnitReference_2_Before = airHandlingUnits[1].SelectedVentilationUnitReference();

            Space space = LivingRoom(adjacencyCluster, airHandlingUnits[0]);

            //Push that dwelling well past the only product offered, so the search really has to clamp.
            DwellingDesignAirFlowResolution resolution = adjacencyCluster.ResolveTargetedDesignAirFlow(
                space,
                FlowClassification.Supply,
                Design(adjacencyCluster, space, FlowClassification.Supply) + 100,
                ventilationUnitCapacityDescriptors: descriptors);

            Assert.True(resolution.IsAccepted, string.Join(" ", resolution.Refusals));
            Assert.False(resolution.IsRequestSatisfied);
            Assert.Equal(airHandlingUnits[0].Guid, resolution.AirHandlingUnit.Guid);
            Assert.Equal(50, resolution.SupplyDuty_After_Lps, 2);

            //Dwelling 2's duty and selection are untouched - in the model handed back, and in the original.
            List<AirHandlingUnit> airHandlingUnits_After = resolution.AdjacencyCluster.GetObjects<AirHandlingUnit>();
            AirHandlingUnit airHandlingUnit_2_After = airHandlingUnits_After.Find(x => x.Guid == airHandlingUnits[1].Guid);

            resolution.AdjacencyCluster.AirHandlingUnitDesignDuty(airHandlingUnit_2_After, out double supplyDuty_2_Resolved_Lps, out _);
            Assert.Equal(supplyDuty_2_Before_Lps, supplyDuty_2_Resolved_Lps, 6);
            Assert.True(ventilationUnitReference_2_Before.Matches(airHandlingUnit_2_After.SelectedVentilationUnitReference()));

            adjacencyCluster.AirHandlingUnitDesignDuty(airHandlingUnits[1], out double supplyDuty_2_After_Lps, out _);
            Assert.Equal(supplyDuty_2_Before_Lps, supplyDuty_2_After_Lps, 6);
        }

        /// <summary>
        /// A tolerance that cannot be compared against is refused before anything is evaluated - it decides
        /// both every Approved Document F, balance and capacity comparison beneath the search AND when the
        /// search's own bracket has closed. See <c>Query.IsValidFlowRateTolerance</c>.
        /// </summary>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(-1.0)]
        public void Resolution_AnUnusableTolerance_RefusesWithoutEvaluatingAnything(double tolerance_Lps)
        {
            AdjacencyCluster adjacencyCluster = Selected(out _, out _);

            string json_Before = Core.Convert.ToString(adjacencyCluster);

            Space space = adjacencyCluster.GetSpaces().Find(x => x.Name == name_LivingRoom);

            DwellingDesignAirFlowResolution resolution = adjacencyCluster.ResolveTargetedDesignAirFlow(
                space,
                FlowClassification.Supply,
                Design(adjacencyCluster, space, FlowClassification.Supply) + 3,
                tolerance_Lps: tolerance_Lps,
                ventilationUnitCapacityDescriptors: DwellingCatalogue());

            Assert.False(resolution.IsAccepted);
            Assert.Null(resolution.AdjacencyCluster);
            Assert.Equal(0, resolution.Evaluations);
            Assert.NotEmpty(resolution.Refusals);

            Assert.True(Helpers.JsonEquivalence.AreEquivalent(json_Before, Core.Convert.ToString(adjacencyCluster), out string difference), difference);
        }

        /// <summary>
        /// A request that is not a quantity of air is refused by the search itself, before any bracket is
        /// built - there is no value to resolve towards.
        /// </summary>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(-1.0)]
        public void Resolution_AnImpossibleRequest_IsRefusedWithoutEvaluatingAnything(double designFlowRate_Lps)
        {
            AdjacencyCluster adjacencyCluster = Selected(out _, out _);

            Space space = adjacencyCluster.GetSpaces().Find(x => x.Name == name_LivingRoom);

            DwellingDesignAirFlowResolution resolution = adjacencyCluster.ResolveTargetedDesignAirFlow(
                space,
                FlowClassification.Supply,
                designFlowRate_Lps,
                ventilationUnitCapacityDescriptors: DwellingCatalogue());

            Assert.False(resolution.IsAccepted);
            Assert.Null(resolution.AdjacencyCluster);
            Assert.Equal(0, resolution.Evaluations);
            Assert.Null(resolution.Candidate);
            Assert.NotEmpty(resolution.Refusals);
        }

        /// <summary>No model is not a crash, and it is not an accepted answer either.</summary>
        [Fact]
        public void Resolution_WithoutAModel_IsRefused()
        {
            DwellingDesignAirFlowResolution resolution = ((AdjacencyCluster)null).ResolveTargetedDesignAirFlow(null, FlowClassification.Supply, 10);

            Assert.NotNull(resolution);
            Assert.False(resolution.IsAccepted);
            Assert.Null(resolution.AdjacencyCluster);
            Assert.Null(resolution.Candidate);
            Assert.NotEmpty(resolution.Refusals);
        }

        // =================================================================================================
        // N. Floor integrity - PartFRequiredAirFlow <= DesignAirFlow, at the tolerance boundary.
        //    The lower half of the Iteration 2 invariant used to be enforced as
        //    "design + tolerance < requirement refuses", which quietly permits a design up to one whole
        //    tolerance BELOW the regulatory floor to be written. Nothing reached that edge until
        //    Iteration 2B's bounded search started driving deliberately at every constraint. The rule is
        //    now: more than a tolerance below refuses, exactly as before; within a tolerance below is
        //    raised to the requirement and written as the requirement. Pinned on both the transaction and
        //    the setter beneath it, because a floor that only holds on one route is not a floor.
        // =================================================================================================

        /// <summary>
        /// <b>The transaction.</b> A targeted design airflow a rounding bit under the room's Approved
        /// Document F requirement is written as the requirement, exactly - and the balancing consequence is
        /// planned from that value, so the dwelling comes out square.
        /// </summary>
        [Fact]
        public void NearFloorRequest_WithinTolerance_IsRaisedToThePartFRequirementExactly()
        {
            AdjacencyCluster adjacencyCluster = HeadroomFixture(out Space space_Supply, out Space space_Extract);

            //The extract room requires 28 and is designed at 30. Half a thousandth under the requirement is
            //a number the tolerance cannot tell apart from it.
            DwellingDesignAirFlowChange change = adjacencyCluster.ApplyTargetedDesignAirFlow(space_Extract, FlowClassification.Extract, 28 - 0.0005);

            Assert.True(change.Successful, string.Join(" ", change.Refusals));

            //Written as the requirement, not as asked.
            Assert.Equal(28, Design(adjacencyCluster, space_Extract, FlowClassification.Extract), 9);
            Assert.Equal(28, change.TargetedAdjustment.After_Lps, 9);

            //Balanced from the normalized value: the supply side moved to 28 too, not to 27.9995.
            Assert.Equal(28, Design(adjacencyCluster, space_Supply, FlowClassification.Supply), 9);
            Assert.Equal(change.SupplyDuty_Lps, change.ExtractDuty_Lps, 9);

            //And it said so rather than correcting the engineer silently.
            Assert.Contains(change.Notes, x => x.Contains("raised to exactly that requirement"));

            AssertPartFFloorsAreMet(adjacencyCluster);
        }

        /// <summary>
        /// <b>The boundary, from both sides.</b> Normalization is not an excuse to accept any sub-floor
        /// design: a request materially below the requirement is still refused with nothing written, and
        /// the two behaviours meet exactly one tolerance below the floor.
        /// </summary>
        [Fact]
        public void ARequestMoreThanToleranceBelowThePartFFloor_IsStillRefused()
        {
            AdjacencyCluster adjacencyCluster = HeadroomFixture(out _, out Space space_Extract);

            Dictionary<string, double> design_Before = Designs(adjacencyCluster);

            //A hundredth under - ten tolerances, a design decision rather than a rounding bit.
            DwellingDesignAirFlowChange change = adjacencyCluster.ApplyTargetedDesignAirFlow(space_Extract, FlowClassification.Extract, 28 - 0.01);

            Assert.False(change.Successful);
            Assert.Contains(change.Refusals, x => x.Contains("Approved Document F requires of that room"));
            Assert.Null(change.TargetedAdjustment);
            Assert.Empty(change.DerivedAdjustments);

            //Nothing written - the refusal is unchanged from before the floor correction.
            Assert.Equal(design_Before, Designs(adjacencyCluster));

            //Exactly one tolerance below is the last value that normalizes rather than refusing, which is
            //where the two behaviours meet. Stated here so the boundary is pinned rather than implied.
            DwellingDesignAirFlowChange change_Boundary = adjacencyCluster.ApplyTargetedDesignAirFlow(space_Extract, FlowClassification.Extract, 28 - 0.001);

            Assert.True(change_Boundary.Successful, string.Join(" ", change_Boundary.Refusals));
            Assert.Equal(28, Design(adjacencyCluster, space_Extract, FlowClassification.Extract), 9);
        }

        /// <summary>
        /// <b>The setter beneath it.</b> A direct caller of <c>Modify.SetSpaceDesignFlowRate</c> cannot
        /// persist a design below the floor either, because the floor is enforced where the write happens
        /// and not only in the transaction above it.
        /// </summary>
        [Fact]
        public void SetSpaceDesignFlowRate_WithinToleranceBelowThePartFFloor_WritesTheRequirementExactly()
        {
            AdjacencyCluster adjacencyCluster = HeadroomFixture(out _, out Space space_Extract);

            List<VentilationTerminal> ventilationTerminals = adjacencyCluster.SetSpaceDesignFlowRate(space_Extract, FlowClassification.Extract, 28 - 0.0005, out List<string> notes, out List<string> refusals);

            Assert.NotNull(ventilationTerminals);
            Assert.Empty(refusals);

            //The room total is the requirement, exactly - and so is the one terminal it was shared across.
            Assert.Equal(28, Design(adjacencyCluster, space_Extract, FlowClassification.Extract), 9);
            Assert.All(ventilationTerminals, x => Assert.True(x.DesignFlowRate_Lps.HasValue));

            Assert.Contains(notes, x => x.Contains("set to exactly that requirement"));
        }

        /// <summary>
        /// The same setter still refuses a design materially below the floor, writing nothing - the rule
        /// <c>DesignAirFlowBelowThePartFRequirement_IsRefused</c> already pins, restated at the boundary so
        /// the normalization above is known not to have widened it.
        /// </summary>
        [Fact]
        public void SetSpaceDesignFlowRate_MoreThanToleranceBelowThePartFFloor_IsStillRefused()
        {
            AdjacencyCluster adjacencyCluster = HeadroomFixture(out _, out Space space_Extract);

            Dictionary<string, double> design_Before = Designs(adjacencyCluster);

            Assert.Null(adjacencyCluster.SetSpaceDesignFlowRate(space_Extract, FlowClassification.Extract, 28 - 0.01, out _, out List<string> refusals));

            Assert.NotEmpty(refusals);
            Assert.Equal(design_Before, Designs(adjacencyCluster));
        }

        /// <summary>
        /// <b>The candidate boundary inherits the correction.</b> A candidate evaluated a rounding bit
        /// under the floor is accepted and carries a model designed AT the floor - the transaction it
        /// borrows is the same one, so the candidate and the search cannot disagree with a manual edit
        /// about where the floor is.
        /// </summary>
        [Fact]
        public void Candidate_WithinToleranceBelowThePartFFloor_CarriesAModelDesignedAtTheFloor()
        {
            AdjacencyCluster adjacencyCluster = HeadroomFixture(out Space space_Supply, out Space space_Extract);

            string json_Before = Core.Convert.ToString(adjacencyCluster);

            DwellingDesignAirFlowCandidate candidate = adjacencyCluster.EvaluateTargetedDesignAirFlow(space_Extract, FlowClassification.Extract, 28 - 0.0005);

            Assert.True(candidate.IsAccepted, string.Join(" ", candidate.Refusals));
            Assert.Equal(28, candidate.TargetedAdjustment.After_Lps, 9);
            Assert.Equal(28, Design(candidate.AdjacencyCluster, space_Extract, FlowClassification.Extract), 9);
            Assert.Equal(28, Design(candidate.AdjacencyCluster, space_Supply, FlowClassification.Supply), 9);

            AssertPartFFloorsAreMet(candidate.AdjacencyCluster);

            //Still evaluated on a copy - the correction changed what gets written, not where.
            Assert.True(Helpers.JsonEquivalence.AreEquivalent(json_Before, Core.Convert.ToString(adjacencyCluster), out string difference), difference);
        }

        /// <summary>
        /// The whole real dwelling, after a clamped search against a capacity boundary: no room on either
        /// side sits below its own Approved Document F requirement. The invariant asserted over a model the
        /// production Part F calculator sized, rather than only over the hand-built floor fixtures.
        /// </summary>
        [Fact]
        public void Resolution_ClampedAnswer_LeavesEveryRoomAtOrAboveItsPartFRequirement()
        {
            AdjacencyCluster adjacencyCluster = Selected(out _, out _);

            Space space = adjacencyCluster.GetSpaces().Find(x => x.Name == name_LivingRoom);

            DwellingDesignAirFlowResolution resolution = adjacencyCluster.ResolveTargetedDesignAirFlow(
                space,
                FlowClassification.Supply,
                Design(adjacencyCluster, space, FlowClassification.Supply) + 10,
                ventilationUnitCapacityDescriptors: DwellingCatalogue());

            Assert.True(resolution.IsAccepted, string.Join(" ", resolution.Refusals));
            Assert.False(resolution.IsRequestSatisfied);

            AssertPartFFloorsAreMet(resolution.AdjacencyCluster);
            AssertPartFFloorsAreMet(adjacencyCluster);
        }

        // =================================================================================================
        // O. Iteration 2B - the AUTOMATIC Grasshopper seam. SAMAnalytical.ResolveTargetedDesignAirFlow.
        //    Region M pins the clamp itself, on AdjacencyCluster. These pin what the COMPONENT adds on top
        //    of it and nothing else: which model comes out of an accepted answer, which comes out of a
        //    refused one, and that neither carries a design the caller did not get. An accepted answer
        //    hands out a NEW model; a refused one deliberately hands back the caller's own, unchanged,
        //    rather than manufacturing a copy of a design that was never applied.
        //
        //    Why these test library calls and not the Grasshopper component. SAM.Tests references no
        //    Grasshopper assembly, so ResolvedModel below performs the same steps, in the same order, that
        //    SAMAnalyticalResolveTargetedDesignAirFlow.SolveInstance performs - the component contributes
        //    only Grasshopper parameter reading and message levels on top. Same arrangement, and the same
        //    reason, as PartOIterationPreparationTests.
        //
        //    The manual seam it sits beside is region K's, and is deliberately untouched: an engineer
        //    STATES a value there and the equipment gap is reported beside a change that has already
        //    committed. Here nothing commits until a whole design has been found feasible, so the same
        //    request comes back clamped instead. Component_TheTwoSeamsStillAnswerDifferently pins that the
        //    two answers remain the two answers.
        // =================================================================================================

        /// <summary>
        /// <b>A request the selected unit can carry is met exactly, and the model handed out carries it.</b>
        /// The component's whole contribution on the accepted path is
        /// <c>new AnalyticalModel(input, resolution.AdjacencyCluster)</c>, so this pins that the design the
        /// search settled on is the design a downstream wire actually receives.
        /// </summary>
        [Fact]
        public void Component_AnExactRequestWithinCapacity_IsSatisfiedAndTheOutputModelCarriesIt()
        {
            AnalyticalModel analyticalModel = Prepared(DwellingCatalogue()).AnalyticalModel;

            Space space = analyticalModel.AdjacencyCluster.GetSpaces().Find(x => x.Name == name_LivingRoom);

            double design_Before_Lps = Design(analyticalModel.AdjacencyCluster, space, FlowClassification.Supply);

            //Well inside the 5.8 l/s MVHR-25 leaves the fixture flat.
            double requested_Lps = design_Before_Lps + 2;

            AnalyticalModel analyticalModel_Resolved = ResolvedModel(analyticalModel, space, FlowClassification.Supply, requested_Lps, DwellingCatalogue(), out DwellingDesignAirFlowResolution resolution);

            Assert.True(resolution.IsAccepted, string.Join(" ", resolution.Refusals));
            Assert.True(resolution.IsRequestSatisfied);
            Assert.True(resolution.IsChanged);
            Assert.Equal(requested_Lps, resolution.Achieved_Lps, 6);
            Assert.Null(resolution.LimitingReason);

            //The model that comes OUT of the component, not the resolution's own cluster.
            Assert.NotSame(analyticalModel, analyticalModel_Resolved);
            Assert.Equal(requested_Lps, Design(analyticalModel_Resolved.AdjacencyCluster, space, FlowClassification.Supply), 6);

            //And the one that went in is where it was.
            Assert.Equal(design_Before_Lps, Design(analyticalModel.AdjacencyCluster, space, FlowClassification.Supply), 6);
        }

        /// <summary>
        /// <b>A request beyond the selected unit clamps, and the unit stays selected.</b> The engineering is
        /// region M's; what is pinned here is that the clamped design - not the request, and not the unit's
        /// rating - is what reaches the output model, and that the product identity on the air handling
        /// unit in that model is the one that was already there.
        /// </summary>
        [Fact]
        public void Component_ACapacityLimitedRequest_ClampsAndKeepsTheSelectedUnit()
        {
            AnalyticalModel analyticalModel = Prepared(DwellingCatalogue()).AnalyticalModel;

            Space space = analyticalModel.AdjacencyCluster.GetSpaces().Find(x => x.Name == name_LivingRoom);

            double design_Before_Lps = Design(analyticalModel.AdjacencyCluster, space, FlowClassification.Supply);

            //Well past what MVHR-25 will carry.
            double requested_Lps = design_Before_Lps + 10;

            AnalyticalModel analyticalModel_Resolved = ResolvedModel(analyticalModel, space, FlowClassification.Supply, requested_Lps, DwellingCatalogue(), out DwellingDesignAirFlowResolution resolution);

            //Accepted and clamped - which is not a refusal, and the two outputs say so separately.
            Assert.True(resolution.IsAccepted, string.Join(" ", resolution.Refusals));
            Assert.False(resolution.IsRequestSatisfied);
            Assert.True(resolution.IsChanged);
            Assert.Empty(resolution.Refusals);

            //Moved TOWARDS the request and stopped short of it. Never past, and never the other way.
            Assert.True(resolution.Achieved_Lps > design_Before_Lps);
            Assert.True(resolution.Achieved_Lps < requested_Lps);

            //And the bound is named rather than left to be inferred.
            Assert.False(string.IsNullOrWhiteSpace(resolution.LimitingReason));

            //The clamped value is what the output model actually carries.
            Assert.Equal(resolution.Achieved_Lps, Design(analyticalModel_Resolved.AdjacencyCluster, space, FlowClassification.Supply), 6);

            //Nothing was bought. Kept, on the answer and on the unit in the model handed out.
            Assert.Equal(VentilationUnitSelectionOutcome.Kept, resolution.Candidate.VentilationUnitSelectionOutcome);
            Assert.Equal("MVHR-25", SelectedModel(analyticalModel_Resolved.AdjacencyCluster));
            Assert.Equal("MVHR-25", SelectedModel(analyticalModel.AdjacencyCluster));

            //The rating that bounded the answer is reported as a rating and stays one. The duty stops at
            //that rating within the tolerance every capacity comparison is made against - and not one
            //bisection step further, which is the whole claim "the selected unit is the bound" makes.
            Assert.Equal(25, resolution.VentilationUnitCapacityDescriptor.MaximumSupplyFlowRate_Lps, 6);
            Assert.True(resolution.SupplyDuty_After_Lps <= 25 + 0.001, string.Format("The clamped design duty is {0:0.#########} l/s against the selected MVHR-25.", resolution.SupplyDuty_After_Lps));
        }

        /// <summary>
        /// <b>The model the component was given is never touched</b> - not by a clamped answer, and not by
        /// a refused one. Compared as whole clusters, so this covers every terminal and every relation
        /// rather than only the room totals.
        /// <para>
        /// The refused half also pins the shape of the answer: no candidate model is handed back at all,
        /// so there is no half-applied design for anyone to adopt by mistake. The component returns the
        /// caller's own model in that case, which is why <c>accepted</c> and not the model wire is the
        /// thing to read.
        /// </para>
        /// </summary>
        [Fact]
        public void Component_LeavesTheInputModelUntouched_OnAClampAndOnARefusal()
        {
            AnalyticalModel analyticalModel = Prepared(DwellingCatalogue()).AnalyticalModel;

            Space space = analyticalModel.AdjacencyCluster.GetSpaces().Find(x => x.Name == name_LivingRoom);

            string json_Before = Core.Convert.ToString(analyticalModel.AdjacencyCluster);
            Dictionary<Guid, double> terminals_Before = TerminalDesigns(analyticalModel.AdjacencyCluster);

            //A clamp, so the search definitely rejected candidates on the way there.
            ResolvedModel(analyticalModel, space, FlowClassification.Supply, Design(analyticalModel.AdjacencyCluster, space, FlowClassification.Supply) + 10, DwellingCatalogue(), out DwellingDesignAirFlowResolution resolution_Clamped);

            Assert.True(resolution_Clamped.IsAccepted, string.Join(" ", resolution_Clamped.Refusals));
            Assert.False(resolution_Clamped.IsRequestSatisfied);

            Assert.Equal(terminals_Before, TerminalDesigns(analyticalModel.AdjacencyCluster));
            Assert.True(Helpers.JsonEquivalence.AreEquivalent(json_Before, Core.Convert.ToString(analyticalModel.AdjacencyCluster), out string difference_Clamped), difference_Clamped);

            //A refusal. The living room is a habitable room, so Approved Document F gives it a supply
            //terminal and no extract - there is no extract side of it to resolve towards anything.
            AnalyticalModel analyticalModel_Refused = ResolvedModel(analyticalModel, space, FlowClassification.Extract, 30, DwellingCatalogue(), out DwellingDesignAirFlowResolution resolution_Refused);

            Assert.False(resolution_Refused.IsAccepted);
            Assert.NotEmpty(resolution_Refused.Refusals);
            Assert.Null(resolution_Refused.AdjacencyCluster);
            Assert.True(double.IsNaN(resolution_Refused.Achieved_Lps));
            Assert.False(resolution_Refused.IsRequestSatisfied);
            Assert.False(resolution_Refused.IsChanged);

            //No partially modified model came out - the caller's own is what comes back.
            Assert.Same(analyticalModel, analyticalModel_Refused);

            Assert.Equal(terminals_Before, TerminalDesigns(analyticalModel.AdjacencyCluster));
            Assert.True(Helpers.JsonEquivalence.AreEquivalent(json_Before, Core.Convert.ToString(analyticalModel.AdjacencyCluster), out string difference_Refused), difference_Refused);
        }

        /// <summary>
        /// <b>The targeted and derived adjustments the component reports are the ones the output model
        /// carries</b>, and the dwelling it hands out is balanced.
        /// <para>
        /// The two lists are the whole reason design airflow can be reviewed at all - one room was chosen,
        /// the rest moved because the network required it - so a component that reported them from anywhere
        /// other than the model it also hands out would be reporting a design nobody receives.
        /// </para>
        /// </summary>
        [Fact]
        public void Component_TargetedAndDerivedAdjustments_MatchTheOutputModelAndLeaveItBalanced()
        {
            AnalyticalModel analyticalModel = Prepared(DwellingCatalogue()).AnalyticalModel;

            Space space = analyticalModel.AdjacencyCluster.GetSpaces().Find(x => x.Name == name_LivingRoom);

            AnalyticalModel analyticalModel_Resolved = ResolvedModel(analyticalModel, space, FlowClassification.Supply, Design(analyticalModel.AdjacencyCluster, space, FlowClassification.Supply) + 2, DwellingCatalogue(), out DwellingDesignAirFlowResolution resolution);

            Assert.True(resolution.IsAccepted, string.Join(" ", resolution.Refusals));

            AdjacencyCluster adjacencyCluster_Resolved = analyticalModel_Resolved.AdjacencyCluster;

            //ONE targeted room, and it is the room that was pointed at.
            Assert.NotNull(resolution.TargetedAdjustment);
            Assert.Equal(name_LivingRoom, resolution.TargetedAdjustment.SpaceName);
            Assert.Equal(resolution.TargetedAdjustment.After_Lps, Design(adjacencyCluster_Resolved, space, FlowClassification.Supply), 6);

            //Balancing the increase means moving the extract side, so there is something derived to check.
            Assert.NotEmpty(resolution.DerivedAdjustments);

            foreach (DesignAirFlowAdjustment designAirFlowAdjustment in resolution.DerivedAdjustments)
            {
                //A derived room is never the targeted one - that distinction is the point of the two lists.
                Assert.NotEqual(name_LivingRoom, designAirFlowAdjustment.SpaceName);

                Space space_Derived = adjacencyCluster_Resolved.GetSpaces().Find(x => x.Name == designAirFlowAdjustment.SpaceName);

                Assert.NotNull(space_Derived);
                Assert.Equal(designAirFlowAdjustment.After_Lps, Design(adjacencyCluster_Resolved, space_Derived, designAirFlowAdjustment.FlowClassification), 6);
            }

            //The dwelling the component hands out is square, and the duties it reports are that dwelling's.
            adjacencyCluster_Resolved.VentilationSystemDesignDuty(adjacencyCluster_Resolved.GetObject<VentilationSystem>(resolution.VentilationSystem.Guid), out double supplyDuty_Lps, out double extractDuty_Lps);

            Assert.Equal(resolution.SupplyDuty_After_Lps, supplyDuty_Lps, 6);
            Assert.Equal(resolution.ExtractDuty_After_Lps, extractDuty_Lps, 6);
            Assert.True(System.Math.Abs(supplyDuty_Lps - extractDuty_Lps) <= 0.001);

            AssertPartFFloorsAreMet(adjacencyCluster_Resolved);
        }

        /// <summary>
        /// <b>A reduction cannot be made to persist below the Approved Document F requirement</b>, however
        /// far down it is asked to go - and the component is the route somebody would most easily try it
        /// on, by wiring its own output back into it and asking for zero.
        /// <para>
        /// Two calls, exactly as a canvas would wire them: raise the room, then ask for nothing. The second
        /// answer clamps at the room's requirement, which is where Iteration 1a designed it in the first
        /// place, and every room in the model handed out is still at or above its own floor.
        /// </para>
        /// </summary>
        [Fact]
        public void Component_AReductionStopsAtThePartFFloor()
        {
            AnalyticalModel analyticalModel = Prepared(DwellingCatalogue()).AnalyticalModel;

            Space space = analyticalModel.AdjacencyCluster.GetSpaces().Find(x => x.Name == name_LivingRoom);

            double requirement_Lps = analyticalModel.AdjacencyCluster.PartFRequiredFlowRate_Lps(space, FlowClassification.Supply) ?? double.NaN;

            Assert.False(double.IsNaN(requirement_Lps));

            AnalyticalModel analyticalModel_Raised = ResolvedModel(analyticalModel, space, FlowClassification.Supply, requirement_Lps + 2, DwellingCatalogue(), out DwellingDesignAirFlowResolution resolution_Raised);

            Assert.True(resolution_Raised.IsRequestSatisfied);

            //Now ask the raised model for nothing at all.
            AnalyticalModel analyticalModel_Reduced = ResolvedModel(analyticalModel_Raised, space, FlowClassification.Supply, 0, DwellingCatalogue(), out DwellingDesignAirFlowResolution resolution_Reduced);

            Assert.True(resolution_Reduced.IsAccepted, string.Join(" ", resolution_Reduced.Refusals));
            Assert.False(resolution_Reduced.IsRequestSatisfied);

            //Exactly the requirement, and asserted to nine places because a thousandth under it is
            //precisely the answer region N exists to rule out.
            Assert.Equal(requirement_Lps, resolution_Reduced.Achieved_Lps, 9);
            Assert.True(resolution_Reduced.Achieved_Lps >= requirement_Lps);
            Assert.Equal(requirement_Lps, Design(analyticalModel_Reduced.AdjacencyCluster, space, FlowClassification.Supply), 9);

            AssertPartFFloorsAreMet(analyticalModel_Reduced.AdjacencyCluster);
        }

        /// <summary>
        /// <b>Authority separation, through the component.</b> The capacity that decided where the search
        /// stopped became nothing else in the model handed out: not an Approved Document F requirement, and
        /// not a runtime or profile airflow. A design that was limited BY the equipment is exactly the case
        /// where a capacity would leak into a design if it were ever going to.
        /// </summary>
        [Fact]
        public void Component_WritesNoPartFRequirementAndNoRuntimeAirflow()
        {
            AnalyticalModel analyticalModel = Prepared(DwellingCatalogue()).AnalyticalModel;

            Dictionary<string, string> requirements_Before = Requirements(analyticalModel.AdjacencyCluster);
            List<string> runtimeAirflows_Before = RuntimeAirflows(analyticalModel.AdjacencyCluster);

            Space space = analyticalModel.AdjacencyCluster.GetSpaces().Find(x => x.Name == name_LivingRoom);

            AnalyticalModel analyticalModel_Resolved = ResolvedModel(analyticalModel, space, FlowClassification.Supply, Design(analyticalModel.AdjacencyCluster, space, FlowClassification.Supply) + 10, DwellingCatalogue(), out DwellingDesignAirFlowResolution resolution);

            Assert.True(resolution.IsAccepted, string.Join(" ", resolution.Refusals));
            Assert.False(resolution.IsRequestSatisfied);

            AdjacencyCluster adjacencyCluster_Resolved = analyticalModel_Resolved.AdjacencyCluster;

            //The requirement is what bounded the answer from below and the capacity what bounded it from
            //above. Neither moved.
            Assert.Equal(requirements_Before, Requirements(adjacencyCluster_Resolved));
            Assert.Equal(runtimeAirflows_Before, RuntimeAirflows(adjacencyCluster_Resolved));

            //And no room took the selected product's rating as its own design airflow.
            foreach (double design_Lps in Designs(adjacencyCluster_Resolved).Values)
            {
                Assert.NotEqual(25, design_Lps, 6);
            }
        }

        /// <summary>
        /// <b>The two seams still answer differently, and neither was changed to accommodate the other.</b>
        /// The same over-capacity request, put to both on the same dwelling:
        /// <code>
        /// manual    -> applied in full, and a BIGGER product bought to carry it        (Reselected)
        /// automatic -> clamped to what the ALREADY selected product carries            (Kept)
        /// </code>
        /// <para>
        /// This is the sharpest form the distinction takes. An engineer who states a number gets that
        /// number and the plant it implies; an engineer who asks how much is available gets the answer the
        /// plant they already have will give, and no invoice. Neither is more correct - they are answers to
        /// different questions, and the reason both seams exist.
        /// </para>
        /// <para>
        /// Region K owns the manual seam's behaviour and is not restated here - only that adding the
        /// automatic one left it exactly where it was.
        /// </para>
        /// </summary>
        [Fact]
        public void Component_TheTwoSeamsStillAnswerDifferently()
        {
            AnalyticalModel analyticalModel = Prepared(DwellingCatalogue()).AnalyticalModel;

            Space space = analyticalModel.AdjacencyCluster.GetSpaces().Find(x => x.Name == name_LivingRoom);

            double requested_Lps = Design(analyticalModel.AdjacencyCluster, space, FlowClassification.Supply) + 10;

            //The manual seam, called exactly as SAMAnalytical.ApplyTargetedDesignAirFlow calls it - on a
            //copy of the SAME model the resolver is given below, which is the point: the two seams are
            //compared on one dwelling, and neither call's edits reach the other or the model they share.
            AdjacencyCluster adjacencyCluster_Manual = analyticalModel.AdjacencyCluster;

            DwellingDesignAirFlowChange change = adjacencyCluster_Manual.ApplyTargetedDesignAirFlow(space, FlowClassification.Supply, requested_Lps, PartFExtractAllocationStrategy.MinimumFirstCookingPriority, 0.001, DwellingCatalogue());

            Assert.True(change.Successful, string.Join(" ", change.Refusals));
            Assert.Equal(requested_Lps, Design(adjacencyCluster_Manual, space, FlowClassification.Supply), 6);

            //The request was applied in full, so the selected MVHR-25 no longer covers it - and the manual
            //seam buys the smallest product that does.
            Assert.Equal(VentilationUnitSelectionOutcome.Reselected, change.VentilationUnitSelectionOutcome);
            Assert.NotEqual("MVHR-25", SelectedModel(adjacencyCluster_Manual));

            //The automatic seam, on the same untouched model - and it is untouched, reselection included.
            Assert.Equal("MVHR-25", SelectedModel(analyticalModel.AdjacencyCluster));

            AnalyticalModel analyticalModel_Resolved = ResolvedModel(analyticalModel, space, FlowClassification.Supply, requested_Lps, DwellingCatalogue(), out DwellingDesignAirFlowResolution resolution);

            Assert.True(resolution.IsAccepted, string.Join(" ", resolution.Refusals));
            Assert.False(resolution.IsRequestSatisfied);
            Assert.True(resolution.Achieved_Lps < requested_Lps);
            Assert.Equal(VentilationUnitSelectionOutcome.Kept, resolution.Candidate.VentilationUnitSelectionOutcome);
            Assert.Equal(resolution.Achieved_Lps, Design(analyticalModel_Resolved.AdjacencyCluster, space, FlowClassification.Supply), 6);

            //Nothing was bought, on either model - the same catalogue that let the manual seam reselect was
            //offered here and changed nothing.
            Assert.Equal("MVHR-25", SelectedModel(analyticalModel_Resolved.AdjacencyCluster));
            Assert.Equal("MVHR-25", SelectedModel(analyticalModel.AdjacencyCluster));
        }
        // =================================================================================================
        // Fixtures
        // =================================================================================================

        // ---- The real catalogue's own products, on a real dwelling ---------------------------------------
        //
        // Every other test in this file selects from fixture products, because the RULE is what they are
        // about and SAM.Analytical owns the rule and never the product list. These three are the Approved
        // Document O closeout case, on the two products SAM_Systems now ships: the Nuaire MRXBOX at
        // 150/150 l/s and the Nuaire XBOXER XBC15 at 190/190 l/s.
        //
        // The identities and capacities are stated here rather than read from a file, because
        // SAM.Analytical does not reference SAM.Analytical.Systems and must not start. That these figures
        // are what the shipped catalogue actually says is SAM_Systems' own assertion, in
        // VentilationUnitCatalogueTests.

        /// <summary>
        /// <b>The real 150 -> 190 l/s manufacturer boundary, on a model.</b> One physical dwelling grown
        /// through the whole ladder:
        /// <code>
        /// duty  19.2  MRXBOX selected - the smallest capable of the two
        /// duty 150    MRXBOX exactly on its rating, still sufficient, nothing to re-select
        /// duty 160    MRXBOX EXHAUSTED -> re-selecting from the current design escalates to XBC15
        /// duty 190    XBC15 exactly on its rating, and re-selecting keeps it rather than reaching higher
        /// duty 191    nothing offered can move it: refused, and the unit keeps its previous selection
        /// </code>
        /// <para>
        /// The Approved Document F requirement is never reset and never consulted by any of it - what grew
        /// is the design. Asserted at the end, over the whole model.
        /// </para>
        /// </summary>
        [Fact]
        public void TheRealLadder_EscalatesFromMRXBOXToXBC15_AndThenRefuses()
        {
            AdjacencyCluster adjacencyCluster = SelectedFromRealLadder(out AirHandlingUnit airHandlingUnit, out _);

            Dictionary<string, string> requirements_Before = Requirements(adjacencyCluster);

            //---- 150 l/s: exactly on the MRXBOX rating. Sufficient, and re-selecting stays put.
            RaiseDutyTotalTo(adjacencyCluster, airHandlingUnit, 150);

            Assert.True(adjacencyCluster.IsVentilationUnitSufficient(airHandlingUnit, RealLadder(), out string reason_150), reason_150);
            Assert.Equal(model_MRXBOX, adjacencyCluster.SelectVentilationUnit(airHandlingUnit, RealLadder(), out _, out _).VentilationUnitReference?.Model);
            Assert.Equal(model_MRXBOX, SelectedModel(adjacencyCluster));

            //---- 160 l/s: the MRXBOX is exhausted and the XBC15 is what answers. THE new rung.
            RaiseDutyTotalTo(adjacencyCluster, airHandlingUnit, 160);

            Assert.False(adjacencyCluster.IsVentilationUnitSufficient(airHandlingUnit, RealLadder(), out string reason_160));
            Assert.False(string.IsNullOrWhiteSpace(reason_160));

            VentilationUnitSelection ventilationUnitSelection_160 = adjacencyCluster.SelectVentilationUnit(airHandlingUnit, RealLadder(), out _, out _);

            Assert.True(ventilationUnitSelection_160.IsSelected);
            Assert.Equal(model_XBC15, ventilationUnitSelection_160.VentilationUnitReference.Model);
            Assert.Equal(model_XBC15, SelectedModel(adjacencyCluster));

            //The duty it was selected against is the DESIGN duty, and the headroom is what is left over -
            //neither is the capacity, and 160 was never written back as one.
            Assert.Equal(160, ventilationUnitSelection_160.SupplyDuty_Lps, 6);
            Assert.Equal(30, ventilationUnitSelection_160.SupplyHeadroom_Lps, 6);

            //---- 190 l/s: exactly on the XBC15's rating. Still sufficient, still not escalated.
            RaiseDutyTotalTo(adjacencyCluster, airHandlingUnit, 190);

            Assert.True(adjacencyCluster.IsVentilationUnitSufficient(airHandlingUnit, RealLadder(), out string reason_190), reason_190);
            Assert.Equal(model_XBC15, adjacencyCluster.SelectVentilationUnit(airHandlingUnit, RealLadder(), out _, out _).VentilationUnitReference?.Model);

            //---- 191 l/s: past everything the two real products can do.
            RaiseDutyTotalTo(adjacencyCluster, airHandlingUnit, 191);

            List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors_Real = RealLadder().FindAll(x => x.VentilationUnitReference.Manufacturer == "Nuaire");

            VentilationUnitSelection ventilationUnitSelection_191 = adjacencyCluster.SelectVentilationUnit(airHandlingUnit, ventilationUnitCapacityDescriptors_Real, out _, out List<string> refusals_191);

            Assert.False(ventilationUnitSelection_191.IsSelected);
            Assert.Null(ventilationUnitSelection_191.Descriptor);
            Assert.NotEmpty(refusals_191);
            Assert.Contains("190", ventilationUnitSelection_191.Reason);

            //Nothing was written on the refusal: the unit keeps the selection it already had, which is an
            //honest state, rather than being left half-selected or downgraded.
            Assert.Equal(model_XBC15, SelectedModel(adjacencyCluster));

            //And through all of it, Approved Document F never moved.
            Assert.Equal(requirements_Before, Requirements(adjacencyCluster));
        }

        /// <summary>
        /// <b>Selecting XBC15 explicitly changes equipment facts and nothing else.</b>
        /// <para>
        /// The automatic rule would choose the MRXBOX at this dwelling's duty, because it is the smaller of
        /// the two capable products. Writing the XBC15's identity onto the unit instead - which is what an
        /// engineer specifying a particular product does, and all that
        /// <c>AirHandlingUnitParameter.VentilationUnitReference</c> holds - moves the identity and the
        /// capacity that identity resolves to in the catalogue. It moves nothing else:
        /// </para>
        /// <code>
        /// PartFRequiredAirFlow != DesignAirFlow != SelectedEquipmentCapacity != OperatingAirFlow
        /// </code>
        /// </summary>
        [Fact]
        public void ExplicitlySelectingXBC15_ChangesOnlyTheEquipmentFacts()
        {
            AdjacencyCluster adjacencyCluster = SelectedFromRealLadder(out AirHandlingUnit airHandlingUnit, out VentilationUnitReference ventilationUnitReference_Automatic);

            //The premise: automatic selection chose the smaller product, and would choose it again.
            Assert.Equal(model_MRXBOX, ventilationUnitReference_Automatic.Model);
            Assert.Equal(150, airHandlingUnit.SelectedVentilationUnitCapacityDescriptor(RealLadder()).MaximumSupplyFlowRate_Lps, 6);

            Dictionary<string, string> requirements_Before = Requirements(adjacencyCluster);
            Dictionary<string, double> designs_Before = Designs(adjacencyCluster);
            List<string> runtimeAirflows_Before = RuntimeAirflows(adjacencyCluster);
            List<string> airMovements_Before = (adjacencyCluster.GetObjects<SpaceAirMovement>() ?? []).ConvertAll(x => string.Format("{0}|{1}", x.Name, x.AirFlow));

            adjacencyCluster.AirHandlingUnitDesignDuty(airHandlingUnit, out double supplyDuty_Before_Lps, out double extractDuty_Before_Lps);

            //---- The explicit selection. A copy, added over the model's own instance, exactly as
            //     Modify.SelectVentilationUnit writes one - the identity, and only the identity.
            AirHandlingUnit airHandlingUnit_XBC15 = new(airHandlingUnit);
            airHandlingUnit_XBC15.SetValue(AirHandlingUnitParameter.VentilationUnitReference, XBC15Reference());

            adjacencyCluster.AddObject(airHandlingUnit_XBC15);

            //---- What changed: the identity, and the capacity it resolves to.
            Assert.Equal(model_XBC15, SelectedModel(adjacencyCluster));

            AirHandlingUnit airHandlingUnit_After = Assert.Single(adjacencyCluster.GetObjects<AirHandlingUnit>());

            Assert.Equal(190, airHandlingUnit_After.SelectedVentilationUnitCapacityDescriptor(RealLadder()).MaximumSupplyFlowRate_Lps, 6);
            Assert.Equal(190, airHandlingUnit_After.SelectedVentilationUnitCapacityDescriptor(RealLadder()).MaximumExtractFlowRate_Lps, 6);

            //---- What did not: every other authority in the model.
            Assert.Equal(requirements_Before, Requirements(adjacencyCluster));
            Assert.Equal(designs_Before, Designs(adjacencyCluster));
            Assert.Equal(runtimeAirflows_Before, RuntimeAirflows(adjacencyCluster));
            Assert.Equal(airMovements_Before, (adjacencyCluster.GetObjects<SpaceAirMovement>() ?? []).ConvertAll(x => string.Format("{0}|{1}", x.Name, x.AirFlow)));

            adjacencyCluster.AirHandlingUnitDesignDuty(airHandlingUnit_After, out double supplyDuty_After_Lps, out double extractDuty_After_Lps);

            Assert.Equal(supplyDuty_Before_Lps, supplyDuty_After_Lps, 6);
            Assert.Equal(extractDuty_Before_Lps, extractDuty_After_Lps, 6);

            //The capacity is a ceiling with room to spare, and the design duty is nowhere near it. A
            //190 l/s unit does not make this a 190 l/s dwelling.
            Assert.True(supplyDuty_After_Lps < 190);
            Assert.True(adjacencyCluster.IsVentilationUnitSufficient(airHandlingUnit_After, RealLadder(), out string reason), reason);
        }

        /// <summary>
        /// The capacity of an explicitly chosen product is <b>looked up by identity, never stored</b> - so a
        /// model carrying the XBC15 selection says which product it is and nothing about what that product
        /// can do. Offer a catalogue without it and the ceiling is unknown, which is a reportable state and
        /// not a silent downgrade to the product that is there.
        /// </summary>
        [Fact]
        public void TheXBC15Selection_StoresIdentityAndLooksTheCapacityUp()
        {
            AdjacencyCluster adjacencyCluster = SelectedFromRealLadder(out AirHandlingUnit airHandlingUnit, out _);

            AirHandlingUnit airHandlingUnit_XBC15 = new(airHandlingUnit);
            airHandlingUnit_XBC15.SetValue(AirHandlingUnitParameter.VentilationUnitReference, XBC15Reference());

            adjacencyCluster.AddObject(airHandlingUnit_XBC15);

            AirHandlingUnit airHandlingUnit_After = Assert.Single(adjacencyCluster.GetObjects<AirHandlingUnit>());

            //The stored identity survives serialization with its three identity fields, and no capacity
            //travels with it.
            VentilationUnitReference ventilationUnitReference = airHandlingUnit_After.SelectedVentilationUnitReference();

            Assert.NotNull(ventilationUnitReference);

            AirHandlingUnit airHandlingUnit_RoundTripped = new(airHandlingUnit_After.ToJsonObject());

            Assert.True(ventilationUnitReference.Matches(airHandlingUnit_RoundTripped.SelectedVentilationUnitReference()));
            Assert.Equal(model_XBC15, airHandlingUnit_RoundTripped.SelectedVentilationUnitReference().Model);
            Assert.True(string.IsNullOrEmpty(airHandlingUnit_RoundTripped.SelectedVentilationUnitReference().Reference));

            //Offered the real ladder, the ceiling resolves to 190.
            Assert.Equal(190, airHandlingUnit_After.SelectedVentilationUnitCapacityDescriptor(RealLadder()).MaximumSupplyFlowRate_Lps, 6);

            //Offered a catalogue that does not hold it, the ceiling is UNKNOWN - not the MRXBOX's 150, and
            //not unlimited. Reported as a refusal with a reason rather than as a pass.
            List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors_WithoutXBC15 = RealLadder().FindAll(x => x.VentilationUnitReference.Model != model_XBC15);

            Assert.Null(airHandlingUnit_After.SelectedVentilationUnitCapacityDescriptor(ventilationUnitCapacityDescriptors_WithoutXBC15));
            Assert.False(adjacencyCluster.IsVentilationUnitSufficient(airHandlingUnit_After, ventilationUnitCapacityDescriptors_WithoutXBC15, out string reason));
            Assert.False(string.IsNullOrWhiteSpace(reason));
        }

        // =================================================================================================
        // P. Manual authority - Modify.AssignVentilationUnit. The engineer states the product, and the
        //    statement is held: no rule runs, no airflow moves, and nothing is "corrected".
        // =================================================================================================

        /// <summary>
        /// An explicit assignment writes the identity and <b>moves no airflow of any kind</b> - not the
        /// Approved Document F requirement, not the design airflow, and not the runtime fields a simulation
        /// reads. Fitting a different box is not a statement about any of them.
        /// </summary>
        [Fact]
        public void AnExplicitAssignment_WritesTheIdentityAndMovesNoAirflow()
        {
            AdjacencyCluster adjacencyCluster = SelectedFromRealLadder(out AirHandlingUnit airHandlingUnit, out _);

            Dictionary<string, string> requirements_Before = Requirements(adjacencyCluster);
            Dictionary<string, double> designs_Before = Designs(adjacencyCluster);
            List<string> runtimeAirflows_Before = RuntimeAirflows(adjacencyCluster);

            Assert.True(adjacencyCluster.AssignVentilationUnit(airHandlingUnit, XBC15Reference(), out List<string> notes, out List<string> refusals));

            Assert.Empty(refusals);
            Assert.NotEmpty(notes);

            //The identity moved.
            Assert.Equal(model_XBC15, SelectedModel(adjacencyCluster));

            //Nothing else did.
            Assert.Equal(requirements_Before, Requirements(adjacencyCluster));
            Assert.Equal(designs_Before, Designs(adjacencyCluster));
            Assert.Equal(runtimeAirflows_Before, RuntimeAirflows(adjacencyCluster));

            AssertPartFFloorsAreMet(adjacencyCluster);
        }

        /// <summary>
        /// A manual assignment may deliberately choose a <b>larger</b> capable product than the rule would.
        /// Automatic selection answers MRXBOX at this duty; an engineer who assigns the XBC15 has specified
        /// a bigger box for reasons this model knows nothing about - procurement, standardisation, a duct
        /// run - and nothing here reverts it to the smallest capable product.
        /// </summary>
        [Fact]
        public void AnExplicitAssignment_MayDeliberatelyChooseALargerCapableProduct()
        {
            AdjacencyCluster adjacencyCluster = SelectedFromRealLadder(out AirHandlingUnit airHandlingUnit, out _);

            //The rule, asked the same question, would still say MRXBOX.
            adjacencyCluster.AirHandlingUnitDesignDuty(airHandlingUnit, out double supplyDuty_Lps, out double extractDuty_Lps);
            Assert.Equal(model_MRXBOX, RealLadder().SelectSmallestCapableVentilationUnit(supplyDuty_Lps, extractDuty_Lps).VentilationUnitReference.Model);

            Assert.True(adjacencyCluster.AssignVentilationUnit(airHandlingUnit, XBC15Reference(), out _, out _));

            //And the oversized choice stands, with its own ceiling resolved from the catalogue.
            Assert.Equal(model_XBC15, SelectedModel(adjacencyCluster));

            AirHandlingUnit airHandlingUnit_After = Assert.Single(adjacencyCluster.GetObjects<AirHandlingUnit>());

            Assert.Equal(190, airHandlingUnit_After.SelectedVentilationUnitCapacityDescriptor(RealLadder()).MaximumSupplyFlowRate_Lps, 6);
            Assert.True(adjacencyCluster.IsVentilationUnitSufficient(airHandlingUnit_After, RealLadder(), out _));
        }

        /// <summary>
        /// <b>An insufficient manual assignment is held, and reported.</b> A 175 l/s dwelling with the
        /// 150 l/s MRXBOX assigned stays assigned as the MRXBOX: the design airflow is not reduced to fit
        /// the box, the XBC15 is not substituted, and the shortfall is reported rather than hidden. The
        /// engineer authored a design that does not work and is told so.
        /// </summary>
        [Fact]
        public void AnExplicitAssignment_ToAnInsufficientProduct_IsHeldAndReported()
        {
            AdjacencyCluster adjacencyCluster = SelectedFromRealLadder(out AirHandlingUnit airHandlingUnit, out _);

            RaiseDutyTotalTo(adjacencyCluster, airHandlingUnit, 175);

            Dictionary<string, string> requirements_Before = Requirements(adjacencyCluster);
            Dictionary<string, double> designs_Before = Designs(adjacencyCluster);

            Assert.True(adjacencyCluster.AssignVentilationUnit(airHandlingUnit, MRXBOXReference(), out _, out List<string> refusals));
            Assert.Empty(refusals);

            AirHandlingUnit airHandlingUnit_After = Assert.Single(adjacencyCluster.GetObjects<AirHandlingUnit>());

            //Still the MRXBOX, and still rated at 150 - the identity is the engineer's and the capacity is
            //the catalogue's.
            Assert.Equal(model_MRXBOX, SelectedModel(adjacencyCluster));
            Assert.Equal(150, airHandlingUnit_After.SelectedVentilationUnitCapacityDescriptor(RealLadder()).MaximumSupplyFlowRate_Lps, 6);

            //Reported insufficient, by name.
            Assert.False(adjacencyCluster.IsVentilationUnitSufficient(airHandlingUnit_After, RealLadder(), out string reason));
            Assert.False(string.IsNullOrWhiteSpace(reason));

            //And the design was not quietly cut back to 150 to make the box fit.
            adjacencyCluster.AirHandlingUnitDesignDuty(airHandlingUnit_After, out double supplyDuty_Lps, out double extractDuty_Lps);

            Assert.Equal(175, supplyDuty_Lps, 6);
            Assert.Equal(175, extractDuty_Lps, 6);
            Assert.Equal(requirements_Before, Requirements(adjacencyCluster));
            Assert.Equal(designs_Before, Designs(adjacencyCluster));

            //A capable alternative EXISTS - which is what makes a suggestion possible - and naming it
            //changed nothing, because a suggestion is a value and not a write.
            Assert.Equal(model_XBC15, RealLadder().SelectSmallestCapableVentilationUnit(supplyDuty_Lps, extractDuty_Lps).VentilationUnitReference.Model);
            Assert.Equal(model_MRXBOX, SelectedModel(adjacencyCluster));
        }

        /// <summary>
        /// Assigning one dwelling's product changes that dwelling and no other. There is no cross-dwelling
        /// equipment decision for an override to leak through.
        /// </summary>
        [Fact]
        public void AnExplicitAssignment_ChangesOneDwellingAndLeavesTheOthers()
        {
            PartOIterationPreparation preparation = Prepare(TwoDwellingModel(), DwellingCatalogue());

            Assert.Null(preparation.Refusal);

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            List<AirHandlingUnit> airHandlingUnits = adjacencyCluster.GetObjects<AirHandlingUnit>();
            airHandlingUnits.Sort((x, y) => StringComparer.Ordinal.Compare(x.Name, y.Name));

            Assert.Equal(2, airHandlingUnits.Count);

            string name_Assigned = airHandlingUnits[0].Name;
            string model_Other_Before = airHandlingUnits[1].SelectedVentilationUnitReference()?.Model;

            Assert.False(string.IsNullOrWhiteSpace(model_Other_Before));

            Dictionary<string, double> designs_Before = Designs(adjacencyCluster);

            Assert.True(adjacencyCluster.AssignVentilationUnit(airHandlingUnits[0], XBC15Reference(), out _, out _));

            List<AirHandlingUnit> airHandlingUnits_After = adjacencyCluster.GetObjects<AirHandlingUnit>();

            Assert.Equal(model_XBC15, airHandlingUnits_After.Find(x => x.Name == name_Assigned).SelectedVentilationUnitReference()?.Model);
            Assert.Equal(model_Other_Before, airHandlingUnits_After.Find(x => x.Name != name_Assigned).SelectedVentilationUnitReference()?.Model);

            Assert.Equal(designs_Before, Designs(adjacencyCluster));
        }

        /// <summary>
        /// <b>The contract Iteration 2B rests on, at the level it actually holds.</b> An assigned identity
        /// survives a re-preparation offered no catalogue - which is exactly how every 2B round re-prepares.
        /// No rule runs, and the deliberately oversized XBC15 is not downgraded to the MRXBOX the rule would
        /// pick at this duty.
        /// </summary>
        [Fact]
        public void AnExplicitAssignment_SurvivesRePreparationWithNoCatalogue()
        {
            PartOIterationPreparation preparation = Prepared(RealLadder());

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            AirHandlingUnit airHandlingUnit = Assert.Single(adjacencyCluster.GetObjects<AirHandlingUnit>());

            Assert.Equal(model_MRXBOX, airHandlingUnit.SelectedVentilationUnitReference()?.Model);

            Assert.True(adjacencyCluster.AssignVentilationUnit(airHandlingUnit, XBC15Reference(), out _, out _));

            //Re-prepared with NO catalogue - null is how "select nothing, keep what is there" is spelled,
            //and what OptimisePartOTM59 passes on every round.
            PartOIterationPreparation preparation_Again = Prepare(new AnalyticalModel(preparation.AnalyticalModel, adjacencyCluster), null);

            Assert.Null(preparation_Again.Refusal);
            Assert.Equal(model_XBC15, SelectedModel(preparation_Again.AnalyticalModel.AdjacencyCluster));
        }

        /// <summary>
        /// An assigned identity survives serialization, so an authored assignment is still authored after
        /// the project has been saved and reopened - and the capacity still comes from the catalogue rather
        /// than from the file.
        /// </summary>
        [Fact]
        public void AnExplicitAssignment_SurvivesTheJsonRoundTrip()
        {
            AdjacencyCluster adjacencyCluster = SelectedFromRealLadder(out AirHandlingUnit airHandlingUnit, out _);

            Assert.True(adjacencyCluster.AssignVentilationUnit(airHandlingUnit, XBC15Reference(), out _, out _));

            AirHandlingUnit airHandlingUnit_RoundTripped = new(Assert.Single(adjacencyCluster.GetObjects<AirHandlingUnit>()).ToJsonObject());

            Assert.Equal(model_XBC15, airHandlingUnit_RoundTripped.SelectedVentilationUnitReference()?.Model);
            Assert.Equal(190, airHandlingUnit_RoundTripped.SelectedVentilationUnitCapacityDescriptor(RealLadder()).MaximumSupplyFlowRate_Lps, 6);
        }

        /// <summary>
        /// Assigning onto a unit the model does not contain refuses and writes nothing - the same rule, and
        /// the same reason, as the automatic path. A selection nothing can resolve is worse than none.
        /// </summary>
        [Fact]
        public void AnExplicitAssignment_OntoAUnitOutsideTheModel_RefusesAndWritesNothing()
        {
            AdjacencyCluster adjacencyCluster = SelectedFromRealLadder(out AirHandlingUnit airHandlingUnit, out _);

            Assert.False(adjacencyCluster.AssignVentilationUnit(Analytical.Create.AirHandlingUnit(airHandlingUnit.Name), XBC15Reference(), out _, out List<string> refusals));

            Assert.NotEmpty(refusals);
            Assert.Single(adjacencyCluster.GetObjects<AirHandlingUnit>());
            Assert.Equal(model_MRXBOX, SelectedModel(adjacencyCluster));
        }

        /// <summary>
        /// A product that identifies nothing is refused, and the unit's existing selection is left exactly
        /// where it was. This is not how a dwelling comes to have no product - that is a unit nothing has
        /// been assigned to yet, which is reported as such rather than invented.
        /// </summary>
        [Fact]
        public void AnIdentitylessProduct_IsRefusedAndTheExistingSelectionSurvives()
        {
            AdjacencyCluster adjacencyCluster = SelectedFromRealLadder(out AirHandlingUnit airHandlingUnit, out _);

            Assert.False(adjacencyCluster.AssignVentilationUnit(airHandlingUnit, new VentilationUnitReference(), out _, out List<string> refusals));
            Assert.NotEmpty(refusals);

            Assert.False(adjacencyCluster.AssignVentilationUnit(airHandlingUnit, null, out _, out refusals));
            Assert.NotEmpty(refusals);

            Assert.Equal(model_MRXBOX, SelectedModel(adjacencyCluster));
        }

        /// <summary>
        /// A manually assigned product is <b>indistinguishable</b> from an automatically selected one to
        /// everything downstream: the same parameter, read by the same query, resolved against the same
        /// catalogue. That is what lets a manual assignment act as a ceiling in Iteration 2B without 2B
        /// needing to know a human chose it - and it is why no second "how was this chosen" flag is stored
        /// on the unit.
        /// </summary>
        [Fact]
        public void AnAssignedProduct_IsIndistinguishableFromASelectedOne()
        {
            AdjacencyCluster adjacencyCluster_Selected = SelectedFromRealLadder(out _, out _);

            AdjacencyCluster adjacencyCluster_Assigned = SelectedFromRealLadder(out AirHandlingUnit airHandlingUnit, out _);

            Assert.True(adjacencyCluster_Assigned.AssignVentilationUnit(airHandlingUnit, MRXBOXReference(), out _, out _));

            Assert.True(SelectedReference(adjacencyCluster_Selected).Matches(SelectedReference(adjacencyCluster_Assigned)));
        }

        /// <summary>The MRXBOX's identity as the catalogue states it - the hybrid combination, not a bare model.</summary>
        private static VentilationUnitReference MRXBOXReference()
        {
            return new VentilationUnitReference("Nuaire", model_MRXBOX, "MR-ECO-COOL-V");
        }

        /// <summary>
        /// The two products <c>SAM_Systems</c> ships, at their published capacities and declared ranks.
        /// <para>
        /// Stated rather than read: <c>SAM.Analytical</c> does not reference <c>SAM.Analytical.Systems</c>
        /// and adding a manufacturer must never need a change here. That these figures are what the shipped
        /// catalogue says is asserted in <c>SAM_Systems</c>' own <c>VentilationUnitCatalogueTests</c>.
        /// </para>
        /// <para>
        /// A 500 l/s fixture product sits behind them so that a test which proves nothing escalated past the
        /// real boundary is proving a choice rather than the absence of an alternative.
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

        /// <summary>The XBC15's identity, as the catalogue states it - no reference, no cooling module.</summary>
        private static VentilationUnitReference XBC15Reference()
        {
            return new VentilationUnitReference("Nuaire", model_XBC15, null);
        }

        /// <summary>
        /// The prepared fixture dwelling with a product selected from the REAL ladder. At the fixture's
        /// 19.2 l/s duty the smaller of the two real products answers, which is the premise the escalation
        /// and explicit-selection tests below both start from.
        /// </summary>
        private static AdjacencyCluster SelectedFromRealLadder(out AirHandlingUnit airHandlingUnit, out VentilationUnitReference ventilationUnitReference)
        {
            PartOIterationPreparation preparation = Prepared(RealLadder());

            AdjacencyCluster result = preparation.AnalyticalModel.AdjacencyCluster;

            airHandlingUnit = Assert.Single(result.GetObjects<AirHandlingUnit>());

            ventilationUnitReference = airHandlingUnit.SelectedVentilationUnitReference();

            Assert.NotNull(ventilationUnitReference);
            Assert.Equal(model_MRXBOX, ventilationUnitReference.Model);

            return result;
        }

        /// <summary>
        /// The catalogue the <b>pure selection rule</b> is exercised over, at the sizes Iteration 2's
        /// worked example uses. Deliberately not a real manufacturer: <c>SAM.Analytical</c> owns the rule
        /// and never the product list.
        /// </summary>
        private static List<VentilationUnitCapacityDescriptor> Catalogue()
        {
            return
            [
                Descriptor("MVHR-100", 100, 100),
                Descriptor("MVHR-150", 150, 150),
                Descriptor("MVHR-180", 180, 180),
                Descriptor("MVHR-220", 220, 220),
            ];
        }

        /// <summary>
        /// The catalogue the <b>model</b> tests select from, at the sizes a real domestic unit is rated at.
        /// <para>
        /// The fixture flat comes out of the real <see cref="PartFCalculator"/> at 19.2 l/s, and a
        /// residential mechanical ventilation with heat recovery unit is rated in tens of litres per second
        /// rather than hundreds. Selecting against a catalogue the fixture dwelling could never fill would
        /// make every model test answer "the smallest one" whatever it was measuring.
        /// </para>
        /// </summary>
        private static List<VentilationUnitCapacityDescriptor> DwellingCatalogue()
        {
            return
            [
                Descriptor("MVHR-15", 15, 15),
                Descriptor("MVHR-25", 25, 25),
                Descriptor("MVHR-35", 35, 35),
                Descriptor("MVHR-50", 50, 50),
            ];
        }

        private static VentilationUnitCapacityDescriptor Descriptor(string model, double maximumSupply_Lps, double maximumExtract_Lps, int rank = 0)
        {
            return new VentilationUnitCapacityDescriptor(new VentilationUnitReference("Test Fixture", model, null), maximumSupply_Lps, maximumExtract_Lps, rank);
        }

        /// <summary>The whole Iteration 1a chain, with or without a product catalogue behind it.</summary>
        private static PartOIterationPreparation Prepared(List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors)
        {
            PartOIterationPreparation result = Prepare(Model(), ventilationUnitCapacityDescriptors);

            Assert.Null(result.Refusal);
            Assert.NotNull(result.AnalyticalModel);

            return result;
        }

        /// <summary>The production preparation, called as <c>SAMAnalytical.PreparePartOIteration</c> calls it.</summary>
        private static PartOIterationPreparation Prepare(AnalyticalModel analyticalModel, List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors)
        {
            List<Zone> zones = analyticalModel.GetZones();

            Assert.NotEmpty(zones);

            Dictionary<Guid, string> dictionary = [];
            foreach (Zone zone in zones)
            {
                dictionary[zone.Guid] = "MVRE";
            }

            return analyticalModel.PreparePartOIteration(PartOIteration.BasePassive, null, dictionary, ventilationUnitCapacityDescriptors);
        }

        /// <summary>
        /// A prepared dwelling with a product already selected, handed back as the cluster the tests work
        /// on directly. The fixture flat is sized at 19.2 l/s, so it lands on MVHR-25 with 5.8 l/s of
        /// headroom - which is what the capacity tests then push past.
        /// </summary>
        private static AdjacencyCluster Selected(out AirHandlingUnit airHandlingUnit, out VentilationUnitReference ventilationUnitReference)
        {
            PartOIterationPreparation preparation = Prepared(DwellingCatalogue());

            AdjacencyCluster result = preparation.AnalyticalModel.AdjacencyCluster;

            airHandlingUnit = Assert.Single(result.GetObjects<AirHandlingUnit>());

            ventilationUnitReference = airHandlingUnit.SelectedVentilationUnitReference();

            Assert.NotNull(ventilationUnitReference);
            Assert.Equal("MVHR-25", ventilationUnitReference.Model);

            return result;
        }

        /// <summary>
        /// One flat with the two shapes that matter: habitable rooms Approved Document F gives a supply
        /// terminal and no extract, and wet rooms it gives an extract terminal and no supply. Sized by the
        /// real <see cref="PartFCalculator"/>, so the requirements under test are the ones production makes.
        /// </summary>
        private static AnalyticalModel Model()
        {
            AdjacencyCluster adjacencyCluster = new();

            AddDwelling(adjacencyCluster, 0, 0);

            return Sized(adjacencyCluster, 1);
        }

        /// <summary>
        /// Two flats of deliberately different size, each its own dwelling zone with its own internal
        /// partitions and no partition between them. <see cref="PartFCalculator"/> sizes each independently,
        /// which is the premise the isolation tests rest on.
        /// </summary>
        private static AnalyticalModel TwoDwellingModel()
        {
            AdjacencyCluster adjacencyCluster = new();

            AddDwelling(adjacencyCluster, 1, 0);
            AddDwelling(adjacencyCluster, 2, 100);

            return Sized(adjacencyCluster, 2);
        }

        /// <summary>
        /// One flat's rooms and its internal partitions. <paramref name="index"/> of 0 names the rooms
        /// plainly for the single-dwelling fixture; 1 and 2 suffix them, because two dwellings in one model
        /// need distinguishable room names.
        /// <para>
        /// Flat 2 is larger, so the two dwellings have genuinely different duties and select different
        /// products. <paramref name="x"/> separates the two flats' partition geometry, so neither dwelling
        /// is defined by walls coincident with the other's.
        /// </para>
        /// </summary>
        private static void AddDwelling(AdjacencyCluster adjacencyCluster, int index, double x)
        {
            double scale = index == 2 ? 2.0 : 1.0;

            Dictionary<string, double> dictionary = new()
            {
                { Name(name_LivingRoom, index), 30.0 * scale },
                { Name(name_Bedroom, index), 16.0 * scale },
                { Name(name_Kitchen, index), 12.0 },
                { Name(name_Bathroom, index), 6.0 },
            };

            foreach (KeyValuePair<string, double> keyValuePair in dictionary)
            {
                Space space = new(keyValuePair.Key);

                space.SetValue(SpaceParameter.Area, keyValuePair.Value);
                space.SetValue(SpaceParameter.Volume, keyValuePair.Value * 2.5);

                InternalCondition internalCondition = new(keyValuePair.Key + " IC");

                internalCondition.SetValue(InternalConditionParameter.VentilationSystemTypeName, "MVRE");

                space.InternalCondition = internalCondition;

                adjacencyCluster.AddObject(space);
            }

            //A flat, not a bag of loose rooms: every room opens off the living room, so the dwelling has a
            //transfer air network for the supplied air to reach the extracted rooms through. No partition
            //crosses between the two flats, which is what keeps them separate dwellings.
            foreach (string name in new[] { name_Bedroom, name_Kitchen, name_Bathroom })
            {
                Helpers.DwellingPartitions.Partition(adjacencyCluster, Name(name_LivingRoom, index), Name(name, index), x);

                x += 10;
            }
        }

        /// <summary>
        /// Zones each dwelling and runs the real Part F calculation over the zone category.
        /// <para>
        /// <b>Zoned before the calculation, and sized through the category overload.</b>
        /// <c>PartFCalculator.Calculate()</c> with no category sizes the whole model as ONE dwelling, which
        /// spreads a single supply total across both flats' habitable rooms and leaves each flat
        /// individually unbalanced - the preparation then refuses, correctly, and the fixture would be
        /// testing nothing. <c>Calculate(zoneCategoryName)</c> is what sizes each dwelling zone on its own.
        /// </para>
        /// </summary>
        private static AnalyticalModel Sized(AdjacencyCluster adjacencyCluster, int dwellings)
        {
            for (int i = 0; i < dwellings; i++)
            {
                int index = dwellings == 1 ? 0 : i + 1;

                Zone zone = new(string.Format("Flat {0}", i + 1));
                zone.SetValue(ZoneParameter.IsDwelling, true);
                zone.SetValue(ZoneParameter.ZoneCategory, name_ZoneCategory);

                adjacencyCluster.AddObject(zone);

                //Related exactly as a real model's zoning relates a dwelling zone to its rooms - both
                //PartFCalculator and Modify.PrepareBaseMVHR partition by this relation.
                foreach (Space space in adjacencyCluster.GetSpaces())
                {
                    if (index == 0 || space.Name.EndsWith(string.Format(" {0}", index), StringComparison.Ordinal))
                    {
                        adjacencyCluster.AddRelation(zone, space);
                    }
                }
            }

            AnalyticalModel analyticalModel = new("Part O Iteration 2 Dwelling", null, null, null, adjacencyCluster, null, new ProfileLibrary("Part O Iteration 2 Fixture"));

            PartFCalculator partFCalculator = Analytical.Query.DefaultPartFCalculator();

            Assert.NotNull(partFCalculator);

            partFCalculator.AdjacencyCluster = analyticalModel.AdjacencyCluster;

            Assert.True(partFCalculator.Calculate(name_ZoneCategory), "The Part F calculation did not run, so every test resting on it would be meaningless.");

            return new AnalyticalModel(analyticalModel, partFCalculator.AdjacencyCluster);
        }

        private static string Name(string name, int index)
        {
            return index == 0 ? name : string.Format("{0} {1}", name, index);
        }

        /// <summary>
        /// Moves one dwelling's supply duty to <paramref name="target_Lps"/> through the living room of
        /// the dwelling <paramref name="airHandlingUnit"/> serves - found through the unit's own system,
        /// so the caller never has to know which flat it got.
        /// </summary>
        private static void RaiseDwellingSupplyTo(AdjacencyCluster adjacencyCluster, AirHandlingUnit airHandlingUnit, double target_Lps)
        {
            adjacencyCluster.AirHandlingUnitDesignDuty(airHandlingUnit, out double supplyDuty_Lps, out _);

            foreach (VentilationSystem ventilationSystem in adjacencyCluster.VentilationSystems(airHandlingUnit))
            {
                foreach (Space space in adjacencyCluster.GetRelatedObjects<Space>(ventilationSystem) ?? [])
                {
                    if (space.Name.StartsWith(name_LivingRoom, StringComparison.Ordinal))
                    {
                        Retarget(adjacencyCluster, space.Name, FlowClassification.Supply, target_Lps - supplyDuty_Lps);

                        adjacencyCluster.AirHandlingUnitDesignDuty(airHandlingUnit, out double supplyDuty_After_Lps, out _);

                        Assert.Equal(target_Lps, supplyDuty_After_Lps, 6);

                        return;
                    }
                }
            }

            Assert.Fail("The fixture unit serves no living room.");
        }

        /// <summary>
        /// The living room of the dwelling <paramref name="airHandlingUnit"/> serves, resolved through the
        /// unit's own ventilation system - so a test never has to know which flat a unit came back as.
        /// </summary>
        private static Space LivingRoom(AdjacencyCluster adjacencyCluster, AirHandlingUnit airHandlingUnit)
        {
            foreach (VentilationSystem ventilationSystem in adjacencyCluster.VentilationSystems(airHandlingUnit))
            {
                foreach (Space space in adjacencyCluster.GetRelatedObjects<Space>(ventilationSystem) ?? [])
                {
                    if (space.Name.StartsWith(name_LivingRoom, StringComparison.Ordinal))
                    {
                        return space;
                    }
                }
            }

            Assert.Fail("The fixture unit serves no living room.");

            return null;
        }

        /// <summary>The models every air handling unit in the cluster is currently selected as, in order.</summary>
        private static List<string> SelectedModels(AdjacencyCluster adjacencyCluster)
        {
            List<string> result = [];

            foreach (AirHandlingUnit airHandlingUnit in adjacencyCluster.GetObjects<AirHandlingUnit>() ?? [])
            {
                result.Add(string.Format("{0}: {1}", airHandlingUnit.Name, airHandlingUnit.SelectedVentilationUnitReference()?.Model ?? "-"));
            }

            result.Sort(StringComparer.Ordinal);

            return result;
        }

        /// <summary>The same model with one room's design airflow raised, ready to be re-prepared.</summary>
        private static AnalyticalModel Changed(PartOIterationPreparation preparation, string name_Space, FlowClassification flowClassification, double increase_Lps)
        {
            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            Space space = adjacencyCluster.GetSpaces().Find(x => x.Name == name_Space);

            Assert.NotNull(adjacencyCluster.SetSpaceDesignFlowRate(space, flowClassification, Design(adjacencyCluster, space, flowClassification) + increase_Lps, out _, out List<string> refusals));
            Assert.Empty(refusals);

            return new AnalyticalModel(preparation.AnalyticalModel, adjacencyCluster);
        }

        /// <summary>
        /// One Approved Document O optimisation step on the fixture dwelling, applied the way the
        /// architecture intends: <b>one</b> targeted room, with the balancing consequence derived.
        /// </summary>
        private static AnalyticalModel Retargeted(PartOIterationPreparation preparation, string name_Space, FlowClassification flowClassification, double increase_Lps)
        {
            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            Retarget(adjacencyCluster, name_Space, flowClassification, increase_Lps);

            return new AnalyticalModel(preparation.AnalyticalModel, adjacencyCluster);
        }

        /// <summary>
        /// Moves the dwelling's whole duty to <paramref name="target_Lps"/> by targeting one habitable room
        /// and letting the extract side follow, so the fixture the capacity tests push on stays a design a
        /// balanced unit could actually serve.
        /// </summary>
        private static void RaiseDutyTotalTo(AdjacencyCluster adjacencyCluster, AirHandlingUnit airHandlingUnit, double target_Lps)
        {
            adjacencyCluster.AirHandlingUnitDesignDuty(airHandlingUnit, out double supplyDuty_Lps, out _);

            Retarget(adjacencyCluster, name_LivingRoom, FlowClassification.Supply, target_Lps - supplyDuty_Lps);

            adjacencyCluster.AirHandlingUnitDesignDuty(airHandlingUnit, out double supplyDuty_After_Lps, out double extractDuty_After_Lps);

            Assert.Equal(target_Lps, supplyDuty_After_Lps, 6);
            Assert.Equal(target_Lps, extractDuty_After_Lps, 6);
        }

        /// <summary>
        /// Adds <paramref name="increase_Lps"/> to one targeted room and rebalances the dwelling, asserting
        /// the transaction was accepted.
        /// </summary>
        private static DwellingDesignAirFlowChange Retarget(AdjacencyCluster adjacencyCluster, string name_Space, FlowClassification flowClassification, double increase_Lps)
        {
            Space space = adjacencyCluster.GetSpaces().Find(x => x.Name == name_Space);

            Assert.NotNull(space);

            DwellingDesignAirFlowChange result = adjacencyCluster.ApplyTargetedDesignAirFlow(space, flowClassification, Design(adjacencyCluster, space, flowClassification) + increase_Lps);

            Assert.True(result.Successful, string.Join(" ", result.Refusals));

            return result;
        }

        /// <summary>One room's design airflow in one direction, summed across its terminals.</summary>
        private static double Design(AdjacencyCluster adjacencyCluster, Space space, FlowClassification flowClassification)
        {
            return adjacencyCluster.VentilationTerminals(space).VentilationTerminalDesignDuty_Lps(flowClassification) ?? 0;
        }

        /// <summary>
        /// The lower half of the Iteration 2 invariant, asserted over a whole model: every room that
        /// carries design terminals of a direction, and has an Approved Document F requirement for that
        /// direction, is designed at or above it.
        /// <para>
        /// <b>Asserted with no tolerance at all, deliberately.</b> A tolerance here is exactly the mistake
        /// this helper exists to catch - it is what let a room be persisted 0.001 l/s below its regulatory
        /// floor and read as compliant. A design that cannot be told apart from the floor is now WRITTEN as
        /// the floor, so the comparison has nothing left to forgive.
        /// </para>
        /// <para>
        /// Rooms with no terminal of a direction are skipped: they carry no design airflow to check, and
        /// whether one should exist at all is <c>Modify.RealizePartFVentilationTerminals</c>'s question.
        /// </para>
        /// </summary>
        private static void AssertPartFFloorsAreMet(AdjacencyCluster adjacencyCluster)
        {
            Assert.NotNull(adjacencyCluster);

            foreach (Space space in adjacencyCluster.GetSpaces() ?? [])
            {
                foreach (FlowClassification flowClassification in new[] { FlowClassification.Supply, FlowClassification.Extract })
                {
                    List<VentilationTerminal> ventilationTerminals = Analytical.Query.VentilationTerminals(adjacencyCluster.VentilationTerminals(space), flowClassification) ?? [];
                    if (ventilationTerminals.Count == 0)
                    {
                        continue;
                    }

                    double? requirement_Lps = adjacencyCluster.PartFRequiredFlowRate_Lps(space, flowClassification);
                    if (!requirement_Lps.HasValue)
                    {
                        continue;
                    }

                    double design_Lps = Design(adjacencyCluster, space, flowClassification);

                    Assert.True(
                        design_Lps >= requirement_Lps.Value,
                        string.Format("Space '{0}' is designed at {1:0.#########} l/s of {2} against the {3:0.#########} l/s Approved Document F requires of it.", space.Name, design_Lps, flowClassification, requirement_Lps.Value));
                }
            }
        }

        /// <summary>
        /// Every design terminal in the model by guid, so "nothing at all was written" can be asserted
        /// value by value rather than only at the room totals - including terminals belonging to another
        /// ventilation system, which a room total would never show.
        /// </summary>
        private static Dictionary<Guid, double> TerminalDesigns(AdjacencyCluster adjacencyCluster)
        {
            Dictionary<Guid, double> result = [];

            foreach (VentilationTerminal ventilationTerminal in adjacencyCluster.GetObjects<VentilationTerminal>() ?? [])
            {
                result[ventilationTerminal.Guid] = ventilationTerminal.DesignFlowRate_Lps ?? double.NaN;
            }

            return result;
        }

        /// <summary>
        /// A minimal one-system dwelling stated directly: 30 l/s of supply against a 10 l/s requirement
        /// (20 l/s of headroom), and 30 l/s of extract against a 28 l/s requirement (2 l/s of headroom).
        /// <para>
        /// <b>Hand-built on purpose.</b> A dwelling the real <see cref="PartFCalculator"/> sized has equal
        /// requirement totals on the two sides and equal design totals, so its removable headroom is
        /// symmetric and a reduction can always be balanced. Asymmetric headroom is the only condition
        /// under which the allocator's shortfall refusal is reachable at all, so the fixture states it
        /// rather than pretending a Part F run produced it.
        /// </para>
        /// </summary>
        private static AdjacencyCluster HeadroomFixture(out Space space_Supply, out Space space_Extract)
        {
            AdjacencyCluster adjacencyCluster = new();

            space_Supply = Room(adjacencyCluster, "Supply Room", PartFTerminalRole.Supply, 10);
            space_Extract = Room(adjacencyCluster, "Extract Room", PartFTerminalRole.GeneralExtract, 28);

            VentilationSystem ventilationSystem = new("Fixture", new VentilationSystemType("Fixture MVHR", "Fixture"));

            adjacencyCluster.AddObject(ventilationSystem);

            Terminal(adjacencyCluster, ventilationSystem, space_Supply, FlowClassification.Supply, 30);
            Terminal(adjacencyCluster, ventilationSystem, space_Extract, FlowClassification.Extract, 30);

            adjacencyCluster.AddRelation(ventilationSystem, space_Supply);
            adjacencyCluster.AddRelation(ventilationSystem, space_Extract);

            return adjacencyCluster;
        }

        /// <summary>
        /// A dwelling that <b>balances globally while one room is already below its Approved Document F
        /// floor</b> - the exact case from the PR #79 review.
        /// <code>
        /// Bathroom extract:  requirement 10, design  5     &lt;- illegal, and nobody is targeting it
        /// Kitchen  extract:  requirement 10, design 15     &lt;- offsetting surplus
        /// Living   supply:   requirement 20, design 20
        ///                    extract total 20 == supply total 20
        /// </code>
        /// <para>
        /// Built by hand because the real <see cref="PartFCalculator"/> would never produce it: the point
        /// is precisely that such a model can reach the API from elsewhere, and that balance alone does not
        /// prove it legal.
        /// </para>
        /// </summary>
        private static AdjacencyCluster ShortfallFixture(out Space space_Supply, out Space space_Bathroom, out Space space_Kitchen)
        {
            AdjacencyCluster adjacencyCluster = new();

            space_Supply = Room(adjacencyCluster, "Living Room", PartFTerminalRole.Supply, 20);
            space_Bathroom = Room(adjacencyCluster, "Bathroom", PartFTerminalRole.GeneralExtract, 10);
            space_Kitchen = Room(adjacencyCluster, "Kitchen", PartFTerminalRole.LocalKitchenExtract, 10);

            VentilationSystem ventilationSystem = new("Fixture", new VentilationSystemType("Fixture MVHR", "Fixture"));

            adjacencyCluster.AddObject(ventilationSystem);

            Terminal(adjacencyCluster, ventilationSystem, space_Supply, FlowClassification.Supply, 20);
            Terminal(adjacencyCluster, ventilationSystem, space_Bathroom, FlowClassification.Extract, 5);
            Terminal(adjacencyCluster, ventilationSystem, space_Kitchen, FlowClassification.Extract, 15);

            adjacencyCluster.AddRelation(ventilationSystem, space_Supply);
            adjacencyCluster.AddRelation(ventilationSystem, space_Bathroom);
            adjacencyCluster.AddRelation(ventilationSystem, space_Kitchen);

            return adjacencyCluster;
        }

        /// <summary>One room of <see cref="HeadroomFixture"/>, carrying a stated Approved Document F requirement.</summary>
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

        /// <summary>One design terminal of <see cref="HeadroomFixture"/>, related to its room and its system.</summary>
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

            foreach (Space space in adjacencyCluster.GetSpaces())
            {
                foreach (FlowClassification flowClassification in new[] { FlowClassification.Supply, FlowClassification.Extract })
                {
                    result[string.Format("{0} {1}", space.Name, flowClassification)] = System.Math.Round(Design(adjacencyCluster, space, flowClassification), 6);
                }
            }

            return result;
        }

        /// <summary>
        /// Every room's Approved Document F requirement on both sides - the values that must be identical
        /// before and after any design transaction.
        /// </summary>
        private static Dictionary<string, string> Requirements(AdjacencyCluster adjacencyCluster)
        {
            Dictionary<string, string> result = [];

            foreach (Space space in adjacencyCluster.GetSpaces())
            {
                PartFSpaceData partFSpaceData = space.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData);

                result[space.Name] = partFSpaceData is null ? "-" : Core.Convert.ToString(partFSpaceData);
            }

            return result;
        }

        /// <summary>
        /// The transfer air [l/s] one space passes on to the rest of the dwelling.
        /// <para>
        /// Space-to-space only: a movement naming the air handling unit is the system's supply or extract,
        /// not the dwelling's internal transfer air. Both ends are <c>ObjectReference</c> text rather than
        /// plain names, and <c>SpaceAirMovement.AirFlow</c> is m3/s - the conversion to l/s happens here so
        /// the assertions read in the units the design is stated in.
        /// </para>
        /// </summary>
        private static double TransferOut(PartOIterationPreparation preparation, string name_Space)
        {
            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            Space space = adjacencyCluster.GetSpaces().Find(x => x.Name == name_Space);

            Assert.NotNull(space);

            string reference_Space = new Core.ObjectReference(space).ToString();

            List<string> references_Space = [];
            foreach (Space space_Other in adjacencyCluster.GetSpaces())
            {
                references_Space.Add(new Core.ObjectReference(space_Other).ToString());
            }

            double result = 0;

            foreach (SpaceAirMovement spaceAirMovement in adjacencyCluster.GetObjects<SpaceAirMovement>() ?? [])
            {
                if (spaceAirMovement.From == reference_Space && references_Space.Contains(spaceAirMovement.To))
                {
                    result += spaceAirMovement.AirFlow * 1000.0;
                }
            }

            return result;
        }

        /// <summary>Every space's Approved Document F data, as text, so two models can be compared whole.</summary>
        private static List<string> Requirements(PartOIterationPreparation preparation)
        {
            List<string> result = [];

            foreach (Space space in preparation.AnalyticalModel.AdjacencyCluster.GetSpaces())
            {
                PartFSpaceData partFSpaceData = space.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData);

                result.Add(string.Format("{0}: {1}", space.Name, partFSpaceData is null ? "-" : Core.Convert.ToString(partFSpaceData)));
            }

            result.Sort(StringComparer.Ordinal);

            return result;
        }

        /// <summary>
        /// Every space's runtime airflow state - the fields a simulation actually reads, and the ones
        /// Iteration 2 must not touch.
        /// <para>
        /// The values rather than the whole serialized internal condition: <c>ApplyPartFVentilationRates</c>
        /// builds a fresh <c>InternalCondition</c> on every run, so two preparations of the same model
        /// differ by a guid that says nothing about whether an airflow moved.
        /// </para>
        /// </summary>
        private static List<string> RuntimeAirflows(PartOIterationPreparation preparation)
        {
            return RuntimeAirflows(preparation.AnalyticalModel.AdjacencyCluster);
        }

        /// <summary>
        /// The same, asked of a cluster directly - a design transaction hands one of those back, and a
        /// preparation is not the only thing whose runtime airflows have to be provably untouched.
        /// </summary>
        private static List<string> RuntimeAirflows(AdjacencyCluster adjacencyCluster)
        {
            List<string> result = [];

            foreach (Space space in adjacencyCluster.GetSpaces())
            {
                InternalCondition internalCondition = space.InternalCondition;

                result.Add(string.Format(
                    "{0}: supply {1}, exhaust {2}, ventilation profile {3}",
                    space.Name,
                    Value(internalCondition, InternalConditionParameter.SupplyAirFlow),
                    Value(internalCondition, InternalConditionParameter.ExhaustAirFlow),
                    internalCondition?.GetValue<string>(InternalConditionParameter.VentilationProfileName) ?? "-"));
            }

            result.Sort(StringComparer.Ordinal);

            return result;
        }

        private static string Value(InternalCondition internalCondition, InternalConditionParameter internalConditionParameter)
        {
            return internalCondition is not null && internalCondition.TryGetValue(internalConditionParameter, out double value) ? value.ToString("0.######") : "-";
        }

        private static int AirMovementCount(PartOIterationPreparation preparation)
        {
            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            return (adjacencyCluster.GetObjects<SpaceAirMovement>() ?? []).Count + (adjacencyCluster.GetObjects<AirHandlingUnitAirMovement>() ?? []).Count;
        }

        /// <summary>
        /// <b>The automatic Grasshopper seam, called as <c>SAMAnalytical.ResolveTargetedDesignAirFlow</c>
        /// calls it</b> - the same two steps, in the same order, that its <c>SolveInstance</c> performs once
        /// its parameters have been read:
        /// <code>
        /// resolution = analyticalModel.AdjacencyCluster.ResolveTargetedDesignAirFlow(...)
        /// out        = accepted ? new AnalyticalModel(analyticalModel, resolution.AdjacencyCluster)
        ///                       : analyticalModel
        /// </code>
        /// <para>
        /// Reading <c>AnalyticalModel.AdjacencyCluster</c> hands out a copy, and the resolver evaluates
        /// every candidate on copies of its own, so <paramref name="analyticalModel"/> is unreachable from
        /// here - which is the property region O's isolation test then proves rather than assumes.
        /// </para>
        /// </summary>
        private static AnalyticalModel ResolvedModel(AnalyticalModel analyticalModel, Space space, FlowClassification flowClassification, double designFlowRate_Lps, List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors, out DwellingDesignAirFlowResolution resolution)
        {
            resolution = analyticalModel.AdjacencyCluster.ResolveTargetedDesignAirFlow(space, flowClassification, designFlowRate_Lps, PartFExtractAllocationStrategy.MinimumFirstCookingPriority, 0.001, ventilationUnitCapacityDescriptors);

            return resolution.IsAccepted ? new AnalyticalModel(analyticalModel, resolution.AdjacencyCluster) : analyticalModel;
        }

        /// <summary>
        /// The model the cluster's one air handling unit is currently selected as. See
        /// <see cref="SelectedModels"/> for the whole-model form.
        /// </summary>
        private static string SelectedModel(AdjacencyCluster adjacencyCluster)
        {
            return SelectedReference(adjacencyCluster)?.Model;
        }

        /// <summary>
        /// The product reference the one air handling unit in <paramref name="adjacencyCluster"/> carries,
        /// read <b>from the cluster</b> rather than from a unit handle taken before the call.
        /// <para>
        /// The distinction is load-bearing. <c>Modify.SelectVentilationUnit</c> writes the selection onto a
        /// guid-preserving replacement unit and adds that over the model's instance, so a handle taken
        /// earlier is no longer a probe of what the model now says - reading one would let a leaked or a
        /// missing selection both pass unnoticed. See
        /// <see cref="EquipmentValidation_AReselection_IsNotWrittenBackOntoTheModelTheClusterCameFrom"/>.
        /// </para>
        /// </summary>
        private static VentilationUnitReference SelectedReference(AdjacencyCluster adjacencyCluster)
        {
            return Assert.Single(adjacencyCluster.GetObjects<AirHandlingUnit>()).SelectedVentilationUnitReference();
        }
    }
}
