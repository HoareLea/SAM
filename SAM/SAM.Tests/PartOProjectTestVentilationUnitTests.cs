// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Analytical.Enums;
using System.Collections.Generic;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// <b>Approved Document O - the optional project test ventilation unit.</b>
    /// <para>
    /// A made-up product, stated in one project, so that "what would a 165 l/s unit do here" can be
    /// answered without editing shipped manufacturer data or inventing a fictional Nuaire entry. These
    /// tests pin the four things that make it safe rather than convenient:
    /// </para>
    /// <list type="number">
    /// <item>it is <b>never</b> a candidate under
    /// <see cref="PartOEquipmentSelectionMode.AutomaticAllProducts"/>, so that mode still means the
    /// shipped manufacturer catalogue and every historic answer is unmoved;</item>
    /// <item>it takes part in a selected pool only where the engineer explicitly ticked it, and is judged
    /// there by the ordinary rule - no special case, no preference;</item>
    /// <item>it <b>persists in the project</b>, because a dwelling assigned to it stores only an identity
    /// and the capacity behind that identity has to be readable again after a reopen;</item>
    /// <item>it persists <b>no wider</b> than the project, so one project's what-if cannot size the
    /// next project's dwellings.</item>
    /// </list>
    /// <para>
    /// The selection rule itself is not reimplemented here: every selection goes through
    /// <c>Query.SelectSmallestCapableVentilationUnit</c>, exactly as
    /// <see cref="PartOEquipmentSelectionTests"/> does, so a test product can only ever change <i>what was
    /// offered</i> and never <i>how the offer was judged</i>.
    /// </para>
    /// </summary>
    public class PartOProjectTestVentilationUnitTests
    {
        /// <summary>The shipped Nuaire hybrid unit's model, 150/150 l/s.</summary>
        private const string model_MRXBOX = "MRXBOXAB-ECO5-AECV";

        /// <summary>The shipped Nuaire XBOXER commercial unit's model, 190/190 l/s.</summary>
        private const string model_XBC15 = "XBC15";

        /// <summary>The what-if the native acceptance script types in: 165 supply, 170 extract.</summary>
        private const string name_Test = "Test unit";

        // =================================================================================================
        // A. What it states, and what it refuses to state
        // =================================================================================================

        /// <summary>
        /// A named product with two positive capacities is usable, and its identity is the synthetic one -
        /// manufacturer "Project test", model the name. That identity is what a dwelling stores, so it has
        /// to be derivable rather than typed, and it has to be unmistakable for manufacturer data.
        /// </summary>
        [Fact]
        public void ANamedProductWithTwoCapacities_IsUsable_AndIdentifiesItselfAsAProjectTest()
        {
            PartOProjectTestVentilationUnit partOProjectTestVentilationUnit = TestUnit();

            Assert.True(partOProjectTestVentilationUnit.IsValid);
            Assert.Null(partOProjectTestVentilationUnit.Refusal);

            VentilationUnitReference ventilationUnitReference = partOProjectTestVentilationUnit.VentilationUnitReference;

            Assert.Equal("Project test", ventilationUnitReference.Manufacturer);
            Assert.Equal(name_Test, ventilationUnitReference.Model);

            //No manufacturer is implied anywhere in what an engineer reads.
            Assert.Contains("Project test", ventilationUnitReference.ToString());
            Assert.DoesNotContain("Nuaire", ventilationUnitReference.ToString());
        }

        /// <summary>
        /// An unnamed, zero, negative, NaN or infinite capacity states no product. Each is refused with a
        /// sentence rather than silently treated as a very small unit - a half-finished entry that selected
        /// nothing would look identical to a catalogue that offered nothing.
        /// </summary>
        [Theory]
        [InlineData(null, 165, 170)]
        [InlineData("", 165, 170)]
        [InlineData("   ", 165, 170)]
        [InlineData(name_Test, 0, 170)]
        [InlineData(name_Test, 165, 0)]
        [InlineData(name_Test, -165, 170)]
        [InlineData(name_Test, 165, double.NaN)]
        [InlineData(name_Test, double.PositiveInfinity, 170)]
        public void AnIncompleteStatement_IsRefusedAndOffersNoIdentity(string name, double maximumSupply_Lps, double maximumExtract_Lps)
        {
            PartOProjectTestVentilationUnit partOProjectTestVentilationUnit = new(name, maximumSupply_Lps, maximumExtract_Lps);

            Assert.False(partOProjectTestVentilationUnit.IsValid);
            Assert.NotNull(partOProjectTestVentilationUnit.Refusal);

            //No identity, so nothing can be assigned to it, and no descriptor, so nothing can be sized
            //against it.
            Assert.Null(partOProjectTestVentilationUnit.VentilationUnitReference);
            Assert.Empty(partOProjectTestVentilationUnit.CapacityDescriptors());
        }

        /// <summary>
        /// The capacity crosses into a selection through the ONE existing mapping - template, then
        /// descriptor - so a test product is judged by the same code a manufacturer product is. It carries
        /// no performance table, because a what-if has no published fan data and inventing some would later
        /// read as measurement.
        /// </summary>
        [Fact]
        public void TheCapacity_CrossesThroughTheOneExistingCatalogueSeam()
        {
            VentilationUnitTemplate ventilationUnitTemplate = TestUnit().VentilationUnitTemplate();

            Assert.NotNull(ventilationUnitTemplate);
            Assert.True(ventilationUnitTemplate.IsValid);
            Assert.True(ventilationUnitTemplate.HasSelectionCapacity);
            Assert.Null(ventilationUnitTemplate.PerformanceTable);

            //And it says in words that these are not manufacturer figures.
            Assert.Contains("Not manufacturer data", ventilationUnitTemplate.Source);

            VentilationUnitCapacityDescriptor ventilationUnitCapacityDescriptor = Assert.Single(TestUnit().CapacityDescriptors());

            Assert.Equal(165, ventilationUnitCapacityDescriptor.MaximumSupplyFlowRate_Lps);
            Assert.Equal(170, ventilationUnitCapacityDescriptor.MaximumExtractFlowRate_Lps);
        }

        /// <summary>
        /// A test product ranks far beyond any shipped product, so one that happens to be the same size as
        /// a real one <b>loses</b> the tie rather than making the catalogue ambiguous. A what-if must never
        /// turn a working selection into a refusal, and must never quietly outrank real equipment.
        /// </summary>
        [Fact]
        public void ATestProductTheSameSizeAsARealOne_LosesTheTie_AndRefusesNothing()
        {
            //Same 150/150 as the shipped MRXBOX, pooled alongside it.
            PartOProjectTestVentilationUnit partOProjectTestVentilationUnit = new(name_Test, 150, 150);

            PartOEquipmentSelection partOEquipmentSelection = Pool(PartOEquipmentSelectionMode.AutomaticSelectedPool, MRXBOXReference(), partOProjectTestVentilationUnit.VentilationUnitReference);

            VentilationUnitSelection ventilationUnitSelection = Select(partOEquipmentSelection, partOProjectTestVentilationUnit, 150);

            Assert.True(ventilationUnitSelection.IsSelected);
            Assert.Equal(model_MRXBOX, ventilationUnitSelection.VentilationUnitReference.Model);
        }

        // =================================================================================================
        // B. Automatic - all catalogue products: the test product NEVER takes part
        // =================================================================================================

        /// <summary>
        /// <b>The single most important test in this file.</b> "Automatic - all catalogue products" means
        /// the shipped manufacturer catalogue, and a project test product left enabled must not join it.
        /// If it did, a project would select a made-up unit because a capacity happened to still be typed
        /// in a box, and the answer would look exactly like a correct one.
        /// </summary>
        [Fact]
        public void UnderAutomaticAllProducts_TheTestProduct_IsNeverACandidate()
        {
            PartOEquipmentSelection partOEquipmentSelection = new(PartOEquipmentSelectionMode.AutomaticAllProducts);

            List<VentilationUnitCapacityDescriptor> candidates = partOEquipmentSelection.CandidateDescriptors(RealLadder(), TestUnit().CapacityDescriptors());

            Assert.Equal(2, candidates.Count);
            Assert.DoesNotContain(candidates, x => x.VentilationUnitReference.Manufacturer == PartOProjectTestVentilationUnit.Manufacturer);
        }

        /// <summary>
        /// And the historic ladder is unmoved by a test product that would otherwise have served: a 160
        /// l/s duty still gets the XBC15, not the 165 l/s what-if that a widened candidate set would have
        /// preferred as smaller. This is the behavioural statement, not just the list.
        /// </summary>
        [Fact]
        public void UnderAutomaticAllProducts_TheLadderIsUnmovedByATestProduct()
        {
            PartOEquipmentSelection partOEquipmentSelection = new(PartOEquipmentSelectionMode.AutomaticAllProducts);

            VentilationUnitSelection ventilationUnitSelection = Select(partOEquipmentSelection, TestUnit(), 160);

            Assert.True(ventilationUnitSelection.IsSelected);
            Assert.Equal(model_XBC15, ventilationUnitSelection.VentilationUnitReference.Model);
        }

        /// <summary>
        /// The manual picker under an all-products project is likewise not offered it: those rows are an
        /// automatic rule's results, and the rule was never offered it either.
        /// </summary>
        [Fact]
        public void UnderAutomaticAllProducts_TheTestProduct_IsNotOfferedToAPicker()
        {
            PartOEquipmentSelection partOEquipmentSelection = new(PartOEquipmentSelectionMode.AutomaticAllProducts);

            Assert.Equal(2, partOEquipmentSelection.AllowedDescriptors(RealLadder(), TestUnit().CapacityDescriptors()).Count);
        }

        // =================================================================================================
        // C. Automatic - selected pool: only where explicitly included
        // =================================================================================================

        /// <summary>
        /// A pool holding ONLY the test product selects it for a duty it can meet. This is the whole point
        /// of the feature: 160/160 against a 165/170 what-if.
        /// </summary>
        [Fact]
        public void APoolOfTheTestProductOnly_SelectsIt()
        {
            PartOProjectTestVentilationUnit partOProjectTestVentilationUnit = TestUnit();

            PartOEquipmentSelection partOEquipmentSelection = Pool(PartOEquipmentSelectionMode.AutomaticSelectedPool, partOProjectTestVentilationUnit.VentilationUnitReference);

            VentilationUnitSelection ventilationUnitSelection = Select(partOEquipmentSelection, partOProjectTestVentilationUnit, 160);

            Assert.True(ventilationUnitSelection.IsSelected);
            Assert.Equal(name_Test, ventilationUnitSelection.VentilationUnitReference.Model);
            Assert.Equal(PartOProjectTestVentilationUnit.Manufacturer, ventilationUnitSelection.VentilationUnitReference.Manufacturer);

            //Capability, not duty: 160 stays the design duty and the rest is headroom.
            Assert.Equal(165, ventilationUnitSelection.Descriptor.MaximumSupplyFlowRate_Lps);
            Assert.Equal(170, ventilationUnitSelection.Descriptor.MaximumExtractFlowRate_Lps);
            Assert.Equal(160, ventilationUnitSelection.SupplyDuty_Lps);
        }

        /// <summary>
        /// A pool that does NOT tick the test product never sees it, even though the project states one -
        /// the pool is the explicit act, and a stated test product is not a permitted one.
        /// </summary>
        [Fact]
        public void APoolThatDoesNotTickTheTestProduct_NeverSelectsIt()
        {
            PartOEquipmentSelection partOEquipmentSelection = Pool(PartOEquipmentSelectionMode.AutomaticSelectedPool, XBC15Reference());

            List<VentilationUnitCapacityDescriptor> candidates = partOEquipmentSelection.CandidateDescriptors(RealLadder(), TestUnit().CapacityDescriptors());

            VentilationUnitCapacityDescriptor ventilationUnitCapacityDescriptor = Assert.Single(candidates);

            Assert.Equal(model_XBC15, ventilationUnitCapacityDescriptor.VentilationUnitReference.Model);
        }

        /// <summary>
        /// A duty beyond the test product's own capacity refuses explicitly and does not reach past the
        /// pool for the shipped product that could have served it. The refusal states the pool's ceiling,
        /// so the engineer is told what is actually wrong with their what-if.
        /// </summary>
        [Fact]
        public void ADutyBeyondTheTestProduct_RefusesRatherThanReachingForAShippedUnit()
        {
            PartOProjectTestVentilationUnit partOProjectTestVentilationUnit = TestUnit();

            PartOEquipmentSelection partOEquipmentSelection = Pool(PartOEquipmentSelectionMode.AutomaticSelectedPool, partOProjectTestVentilationUnit.VentilationUnitReference);

            VentilationUnitSelection ventilationUnitSelection = Select(partOEquipmentSelection, partOProjectTestVentilationUnit, 180);

            Assert.False(ventilationUnitSelection.IsSelected);
            Assert.Null(ventilationUnitSelection.VentilationUnitReference);

            //The what-if's own ceiling, and never the XBC15 that would have served 180.
            Assert.Contains("165", ventilationUnitSelection.Reason);
            Assert.DoesNotContain(model_XBC15, ventilationUnitSelection.Reason);
        }

        /// <summary>
        /// The two sides are judged independently, as they are for a manufacturer product: 168 extract is
        /// within 170 while 168 supply is not within 165, and one figure covering both would have approved
        /// air the supply fan cannot move.
        /// </summary>
        [Fact]
        public void TheTwoSides_AreJudgedIndependently()
        {
            PartOProjectTestVentilationUnit partOProjectTestVentilationUnit = TestUnit();

            PartOEquipmentSelection partOEquipmentSelection = Pool(PartOEquipmentSelectionMode.AutomaticSelectedPool, partOProjectTestVentilationUnit.VentilationUnitReference);

            List<VentilationUnitCapacityDescriptor> candidates = partOEquipmentSelection.CandidateDescriptors(RealLadder(), partOProjectTestVentilationUnit.CapacityDescriptors());

            Assert.True(candidates.SelectSmallestCapableVentilationUnit(160, 168).IsSelected);
            Assert.False(candidates.SelectSmallestCapableVentilationUnit(168, 160).IsSelected);
        }

        // =================================================================================================
        // D. Manual per dwelling - the picker may offer it
        // =================================================================================================

        /// <summary>
        /// Under manual authority with no preselection the picker offers everything, and "everything" now
        /// includes the project's own test product. Picking by hand is the engineer's decision, and a test
        /// capacity they stated is one of the things they may decide on.
        /// </summary>
        [Fact]
        public void UnderManualAuthority_ThePickerOffersTheTestProduct()
        {
            PartOEquipmentSelection partOEquipmentSelection = new(PartOEquipmentSelectionMode.ManualPerDwelling);

            List<VentilationUnitCapacityDescriptor> allowed = partOEquipmentSelection.AllowedDescriptors(RealLadder(), TestUnit().CapacityDescriptors());

            Assert.Equal(3, allowed.Count);
            Assert.Contains(allowed, x => x.VentilationUnitReference.Model == name_Test);

            //And no rule is run for it to bias.
            Assert.Null(partOEquipmentSelection.CandidateDescriptors(RealLadder(), TestUnit().CapacityDescriptors()));
        }

        /// <summary>
        /// Under manual authority WITH a preselection, the pool still decides - a narrowed manual project
        /// that did not tick the test product is not offered it.
        /// </summary>
        [Fact]
        public void UnderManualAuthorityWithAPool_TheTestProductStillHasToBeTicked()
        {
            PartOEquipmentSelection partOEquipmentSelection = Pool(PartOEquipmentSelectionMode.ManualPerDwelling, MRXBOXReference());

            List<VentilationUnitCapacityDescriptor> allowed = partOEquipmentSelection.AllowedDescriptors(RealLadder(), TestUnit().CapacityDescriptors());

            VentilationUnitCapacityDescriptor ventilationUnitCapacityDescriptor = Assert.Single(allowed);

            Assert.Equal(model_MRXBOX, ventilationUnitCapacityDescriptor.VentilationUnitReference.Model);
        }

        // =================================================================================================
        // E. Persistence - in the project, and no wider
        // =================================================================================================

        /// <summary>The statement survives its own JSON round trip, capacities and identity intact.</summary>
        [Fact]
        public void TheTestProduct_SurvivesItsJsonRoundTrip()
        {
            PartOProjectTestVentilationUnit read = new(TestUnit().ToJsonObject());

            Assert.Equal(name_Test, read.Name);
            Assert.Equal(165, read.MaximumSupplyFlowRate_Lps);
            Assert.Equal(170, read.MaximumExtractFlowRate_Lps);
            Assert.True(read.Matches(TestUnit()));
        }

        /// <summary>
        /// And survives being stamped on a project and read back off it - <b>which is the reason it is
        /// persisted at all</b>. A dwelling assigned to it stores only the identity, so if the capacity did
        /// not come back the reopened project would report a sound dwelling as "capacity unknown" and
        /// Iteration 2B would lose the ceiling it stops at.
        /// </summary>
        [Fact]
        public void TheTestProduct_SurvivesTheProjectsJsonRoundTrip_SoAnAssignmentStillResolves()
        {
            AnalyticalModel analyticalModel = new("Fixture", null, null, null, new AdjacencyCluster());

            analyticalModel.SetValue(AnalyticalModelParameter.PartOProjectTestVentilationUnit, TestUnit());

            AnalyticalModel analyticalModel_Read = new(analyticalModel.ToJsonObject());

            PartOProjectTestVentilationUnit read = analyticalModel_Read.GetValue<PartOProjectTestVentilationUnit>(AnalyticalModelParameter.PartOProjectTestVentilationUnit);

            Assert.NotNull(read);
            Assert.True(read.IsValid);

            //The identity a saved assignment holds still resolves a capacity out of the reopened project.
            VentilationUnitCapacityDescriptor ventilationUnitCapacityDescriptor = Assert.Single(read.CapacityDescriptors());

            Assert.True(TestUnit().VentilationUnitReference.Matches(ventilationUnitCapacityDescriptor.VentilationUnitReference));
            Assert.Equal(165, ventilationUnitCapacityDescriptor.MaximumSupplyFlowRate_Lps);
            Assert.Equal(170, ventilationUnitCapacityDescriptor.MaximumExtractFlowRate_Lps);
        }

        /// <summary>
        /// <b>One project's what-if cannot size another project's dwellings.</b> It rides in the model's own
        /// parameters, so a second project that never stated one reads nothing - which is exactly what an
        /// application setting could not have promised.
        /// </summary>
        [Fact]
        public void OneProjectsTestProduct_CannotAppearInAnother()
        {
            AnalyticalModel analyticalModel_A = new("Project A", null, null, null, new AdjacencyCluster());
            analyticalModel_A.SetValue(AnalyticalModelParameter.PartOProjectTestVentilationUnit, TestUnit());

            AnalyticalModel analyticalModel_B = new("Project B", null, null, null, new AdjacencyCluster());

            Assert.NotNull(analyticalModel_A.GetValue<PartOProjectTestVentilationUnit>(AnalyticalModelParameter.PartOProjectTestVentilationUnit));
            Assert.Null(analyticalModel_B.GetValue<PartOProjectTestVentilationUnit>(AnalyticalModelParameter.PartOProjectTestVentilationUnit));
        }

        /// <summary>
        /// A project that has never stated one behaves exactly as it did before this feature existed: the
        /// empty contribution changes no candidate set and no answer. There is nothing to migrate.
        /// </summary>
        [Theory]
        [InlineData(150, model_MRXBOX)]
        [InlineData(160, model_XBC15)]
        [InlineData(190, model_XBC15)]
        public void AProjectWithNoTestProduct_IsUnchangedHistoricBehaviour(double duty_Lps, string model_Expected)
        {
            AnalyticalModel analyticalModel = new("Fixture", null, null, null, new AdjacencyCluster());

            PartOProjectTestVentilationUnit partOProjectTestVentilationUnit = analyticalModel.GetValue<PartOProjectTestVentilationUnit>(AnalyticalModelParameter.PartOProjectTestVentilationUnit);

            Assert.Null(partOProjectTestVentilationUnit);

            PartOEquipmentSelection partOEquipmentSelection = new();

            //The two-argument overload with nothing to add is the one-argument overload.
            Assert.Equal(
                partOEquipmentSelection.CandidateDescriptors(RealLadder()).Count,
                partOEquipmentSelection.CandidateDescriptors(RealLadder(), partOProjectTestVentilationUnit.CapacityDescriptors()).Count);

            VentilationUnitSelection ventilationUnitSelection = Select(partOEquipmentSelection, partOProjectTestVentilationUnit, duty_Lps);

            Assert.True(ventilationUnitSelection.IsSelected);
            Assert.Equal(model_Expected, ventilationUnitSelection.VentilationUnitReference.Model);
        }

        /// <summary>
        /// <see cref="PartOProjectTestVentilationUnit.Matches"/> compares the identity <b>and</b> the two
        /// capacities, unlike <see cref="PartOEquipmentSelection.Matches"/> which holds no capacities at
        /// all. Re-rating a what-if from 165 to 175 changes what an Iteration 2B ceiling is, so a
        /// preparation made under the old rating must not be reused for the new one.
        /// </summary>
        [Fact]
        public void ARerating_IsADifferentStatement_SoAPreparationCannotBeReused()
        {
            Assert.True(TestUnit().Matches(TestUnit()));

            Assert.False(TestUnit().Matches(new PartOProjectTestVentilationUnit(name_Test, 175, 170)));
            Assert.False(TestUnit().Matches(new PartOProjectTestVentilationUnit(name_Test, 165, 180)));
            Assert.False(TestUnit().Matches(new PartOProjectTestVentilationUnit("Another unit", 165, 170)));
            Assert.False(TestUnit().Matches(null));
        }

        /// <summary>The copy constructor is a copy, not a share.</summary>
        [Fact]
        public void TheCopyConstructor_CopiesTheStatement()
        {
            PartOProjectTestVentilationUnit partOProjectTestVentilationUnit_Copy = new(TestUnit())
            {
                MaximumSupplyFlowRate_Lps = 175,
            };

            Assert.Equal(165, TestUnit().MaximumSupplyFlowRate_Lps);
            Assert.Equal(175, partOProjectTestVentilationUnit_Copy.MaximumSupplyFlowRate_Lps);
            Assert.Equal(name_Test, partOProjectTestVentilationUnit_Copy.Name);
        }

        /// <summary>
        /// The shipped catalogue is not touched by any of this. The test product's capability is composed
        /// with the catalogue at the point of use and never written into it, so the two lists handed in
        /// come back out unchanged.
        /// </summary>
        [Fact]
        public void TheShippedCatalogue_IsNeverModified()
        {
            List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors = RealLadder();

            List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors_ProjectTest = TestUnit().CapacityDescriptors();

            PartOEquipmentSelection partOEquipmentSelection = new(PartOEquipmentSelectionMode.ManualPerDwelling);

            partOEquipmentSelection.AllowedDescriptors(ventilationUnitCapacityDescriptors, ventilationUnitCapacityDescriptors_ProjectTest);
            partOEquipmentSelection.CandidateDescriptors(ventilationUnitCapacityDescriptors, ventilationUnitCapacityDescriptors_ProjectTest);

            Assert.Equal(2, ventilationUnitCapacityDescriptors.Count);
            Assert.DoesNotContain(ventilationUnitCapacityDescriptors, x => x.VentilationUnitReference.Manufacturer == PartOProjectTestVentilationUnit.Manufacturer);
            Assert.Single(ventilationUnitCapacityDescriptors_ProjectTest);
        }

        // =================================================================================================
        // Fixtures
        // =================================================================================================

        /// <summary>The what-if the native acceptance script types in: 165 supply, 170 extract.</summary>
        private static PartOProjectTestVentilationUnit TestUnit()
        {
            return new PartOProjectTestVentilationUnit(name_Test, 165, 170);
        }

        /// <summary>
        /// The two shipped products at their shipped capacities and ranks. No decoy here - the test
        /// product IS the tripwire in this file, and any rule that widened "all catalogue products" would
        /// select it.
        /// </summary>
        private static List<VentilationUnitCapacityDescriptor> RealLadder()
        {
            return
            [
                new VentilationUnitCapacityDescriptor(MRXBOXReference(), 150, 150, 10),
                new VentilationUnitCapacityDescriptor(XBC15Reference(), 190, 190, 20),
            ];
        }

        private static VentilationUnitReference MRXBOXReference()
        {
            return new VentilationUnitReference("Nuaire", model_MRXBOX, "MR-ECO-COOL-V");
        }

        private static VentilationUnitReference XBC15Reference()
        {
            return new VentilationUnitReference("Nuaire", model_XBC15, null);
        }

        private static PartOEquipmentSelection Pool(PartOEquipmentSelectionMode partOEquipmentSelectionMode, params VentilationUnitReference[] ventilationUnitReferences)
        {
            return new PartOEquipmentSelection(partOEquipmentSelectionMode, ventilationUnitReferences);
        }

        /// <summary>
        /// The production selection rule, over exactly the candidates the configuration permits once the
        /// project's test product has been offered or withheld by the mode. Balanced duty, matching the
        /// shipped heat-recovery cores.
        /// </summary>
        private static VentilationUnitSelection Select(PartOEquipmentSelection partOEquipmentSelection, PartOProjectTestVentilationUnit partOProjectTestVentilationUnit, double duty_Lps)
        {
            return partOEquipmentSelection
                .CandidateDescriptors(RealLadder(), partOProjectTestVentilationUnit.CapacityDescriptors())
                .SelectSmallestCapableVentilationUnit(duty_Lps, duty_Lps);
        }
    }
}
