// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Analytical.Enums;
using System.Collections.Generic;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// <b>Approved Document O Iteration 2 - the equipment preselection, and the three things it must never
    /// become.</b>
    /// <para>
    /// <see cref="PartOEquipmentSelection"/> answers one question: which products was a selection allowed
    /// to offer, and who decided. It must never become
    /// </para>
    /// <list type="number">
    /// <item>an <b>assignment</b> - <c>AllowedVentilationUnitReferences != SelectedVentilationUnitReference</c>;</item>
    /// <item>a <b>capability</b> - the pool holds identities and no maximum airflow, ever;</item>
    /// <item>a <b>suggestion of a fallback</b> - an empty pool refuses and is never widened back to the
    /// whole catalogue behind the engineer's back.</item>
    /// </list>
    /// <para>
    /// The selection rule itself is not retested here - it belongs to
    /// <see cref="PartOVentilationUnitSelectionTests"/> and is deliberately reached through
    /// <c>Query.SelectSmallestCapableVentilationUnit</c> rather than reimplemented, so a pool can only ever
    /// change <i>what was offered</i> and never <i>how the offer was judged</i>.
    /// </para>
    /// <para>
    /// <b>The two real products are named here on purpose.</b> Unlike the fixture catalogues elsewhere,
    /// these tests pin the shipped 150 / 190 l/s ladder that the native Iteration 2 acceptance walks, so
    /// the capacities are the ones an engineer will see. The catalogue file itself stays SAM_Systems'
    /// business; only the two capacities are restated.
    /// </para>
    /// </summary>
    public class PartOEquipmentSelectionTests
    {
        /// <summary>The shipped Nuaire hybrid unit's model, 150/150 l/s.</summary>
        private const string model_MRXBOX = "MRXBOXAB-ECO5-AECV";

        /// <summary>The shipped Nuaire XBOXER commercial unit's model, 190/190 l/s.</summary>
        private const string model_XBC15 = "XBC15";

        /// <summary>A product no test ever expects to see chosen - a tripwire for a silent widening.</summary>
        private const string model_Decoy = "Never Selected";

        // =================================================================================================
        // A. Automatic - all catalogue products
        // =================================================================================================

        /// <summary>
        /// The historic default, restated as a mode: every selectable product is a candidate, and the real
        /// ladder answers 150 -> MRXBOX, 160 -> XBC15, 191 -> nothing.
        /// </summary>
        [Theory]
        [InlineData(150, model_MRXBOX)]
        [InlineData(160, model_XBC15)]
        [InlineData(190, model_XBC15)]
        public void AutomaticAllProducts_WalksTheRealLadder(double duty_Lps, string model_Expected)
        {
            PartOEquipmentSelection partOEquipmentSelection = new(PartOEquipmentSelectionMode.AutomaticAllProducts);

            VentilationUnitSelection ventilationUnitSelection = Select(partOEquipmentSelection, duty_Lps);

            Assert.True(ventilationUnitSelection.IsSelected);
            Assert.Equal(model_Expected, ventilationUnitSelection.VentilationUnitReference.Model);
        }

        /// <summary>
        /// Above the largest real product nothing is selected, and the refusal says so rather than handing
        /// back the biggest thing available. An undersized unit is not an answer.
        /// </summary>
        [Fact]
        public void AutomaticAllProducts_AboveTheLargestProduct_Refuses()
        {
            PartOEquipmentSelection partOEquipmentSelection = new(PartOEquipmentSelectionMode.AutomaticAllProducts);

            VentilationUnitSelection ventilationUnitSelection = Select(partOEquipmentSelection, 191, RealLadder(decoy: false));

            Assert.False(ventilationUnitSelection.IsSelected);
            Assert.Null(ventilationUnitSelection.VentilationUnitReference);
            Assert.Contains("190", ventilationUnitSelection.Reason);
        }

        /// <summary>The whole catalogue is offered, decoy included - this mode narrows nothing.</summary>
        [Fact]
        public void AutomaticAllProducts_NarrowsNothing()
        {
            PartOEquipmentSelection partOEquipmentSelection = new(PartOEquipmentSelectionMode.AutomaticAllProducts);

            Assert.Equal(3, partOEquipmentSelection.CandidateDescriptors(RealLadder()).Count);
        }

        // =================================================================================================
        // B. Automatic - selected pool
        // =================================================================================================

        /// <summary>
        /// A pool of both real products behaves exactly as the whole catalogue does at these duties: the
        /// pool bounds what is offered and changes nothing about how the offer is judged.
        /// </summary>
        [Theory]
        [InlineData(150, model_MRXBOX)]
        [InlineData(160, model_XBC15)]
        public void APoolOfBothProducts_StillSelectsTheSmallestCapable(double duty_Lps, string model_Expected)
        {
            PartOEquipmentSelection partOEquipmentSelection = Pool(MRXBOXReference(), XBC15Reference());

            VentilationUnitSelection ventilationUnitSelection = Select(partOEquipmentSelection, duty_Lps);

            Assert.True(ventilationUnitSelection.IsSelected);
            Assert.Equal(model_Expected, ventilationUnitSelection.VentilationUnitReference.Model);
        }

        /// <summary>
        /// A pool of the LARGER product only puts the larger product on a duty the smaller could have
        /// served. That is the point of a pool: the engineer has said the small one is not available, and
        /// "smallest capable" now means smallest capable <i>of what is permitted</i>.
        /// </summary>
        [Fact]
        public void APoolOfTheLargerProductOnly_SelectsIt_EvenWhereTheSmallerWouldHaveServed()
        {
            PartOEquipmentSelection partOEquipmentSelection = Pool(XBC15Reference());

            VentilationUnitSelection ventilationUnitSelection = Select(partOEquipmentSelection, 150);

            Assert.True(ventilationUnitSelection.IsSelected);
            Assert.Equal(model_XBC15, ventilationUnitSelection.VentilationUnitReference.Model);
            Assert.Equal(190, ventilationUnitSelection.Descriptor.MaximumSupplyFlowRate_Lps);

            //And the design duty is untouched by the larger box - 40 l/s of headroom is headroom, not a
            //design airflow.
            Assert.Equal(150, ventilationUnitSelection.SupplyDuty_Lps);
            Assert.Equal(40, ventilationUnitSelection.SupplyHeadroom_Lps);
        }

        /// <summary>
        /// A pool of the SMALLER product only, against a duty it cannot meet, refuses - and does not reach
        /// past the pool for the product that could. The refusal states the shortfall against the pool's
        /// own largest capacity, so the engineer is told what is actually wrong.
        /// </summary>
        [Fact]
        public void APoolOfTheSmallerProductOnly_RefusesADutyItCannotMeet()
        {
            PartOEquipmentSelection partOEquipmentSelection = Pool(MRXBOXReference());

            VentilationUnitSelection ventilationUnitSelection = Select(partOEquipmentSelection, 160);

            Assert.False(ventilationUnitSelection.IsSelected);
            Assert.Null(ventilationUnitSelection.VentilationUnitReference);

            //The pool's ceiling, not the catalogue's.
            Assert.Contains("150", ventilationUnitSelection.Reason);
            Assert.DoesNotContain(model_XBC15, ventilationUnitSelection.Reason);
        }

        /// <summary>
        /// <b>An empty pool refuses, and SAM never falls back to the complete catalogue.</b> The engineer
        /// asked for a choice from what they permitted and permitted nothing; quietly selecting from
        /// everything would be the single most dangerous thing this feature could do, because the answer
        /// would look exactly like a correct one.
        /// </summary>
        [Fact]
        public void AnEmptyPool_RefusesExplicitly_AndNeverWidensToTheCatalogue()
        {
            PartOEquipmentSelection partOEquipmentSelection = new(PartOEquipmentSelectionMode.AutomaticSelectedPool);

            Assert.Empty(partOEquipmentSelection.CandidateDescriptors(RealLadder()));

            VentilationUnitSelection ventilationUnitSelection = Select(partOEquipmentSelection, 30);

            Assert.False(ventilationUnitSelection.IsSelected);
            Assert.Null(ventilationUnitSelection.VentilationUnitReference);
            Assert.Contains("No ventilation unit product was offered", ventilationUnitSelection.Reason);
        }

        /// <summary>
        /// An empty pool is not silently treated as "no equipment selection" either: the candidate set is
        /// an empty list, which refuses, and never null, which would mean "run no rule".
        /// </summary>
        [Fact]
        public void AnEmptyPool_IsAnEmptyCandidateSet_AndNotTheAbsenceOfOne()
        {
            PartOEquipmentSelection partOEquipmentSelection = new(PartOEquipmentSelectionMode.AutomaticSelectedPool);

            Assert.NotNull(partOEquipmentSelection.CandidateDescriptors(RealLadder()));
        }

        /// <summary>
        /// The order the engineer ticked the products in cannot change the answer. Candidates are ordered
        /// by capacity and rank downstream, so this is a property of the design rather than a coincidence -
        /// pinned because a pool is the first thing in this chain a human authors by hand.
        /// </summary>
        [Fact]
        public void PoolOrderDoesNotAffectSelection()
        {
            VentilationUnitSelection ventilationUnitSelection_Forward = Select(Pool(MRXBOXReference(), XBC15Reference()), 150);
            VentilationUnitSelection ventilationUnitSelection_Reversed = Select(Pool(XBC15Reference(), MRXBOXReference()), 150);

            Assert.Equal(model_MRXBOX, ventilationUnitSelection_Forward.VentilationUnitReference.Model);
            Assert.Equal(model_MRXBOX, ventilationUnitSelection_Reversed.VentilationUnitReference.Model);

            Assert.Equal(
                ventilationUnitSelection_Forward.Descriptor.MaximumSupplyFlowRate_Lps,
                ventilationUnitSelection_Reversed.Descriptor.MaximumSupplyFlowRate_Lps);
        }

        /// <summary>
        /// An excluded product is never selected, whatever its capacity - here a 500 l/s decoy that would
        /// win any "biggest" or "nearest" rule and that no pooled selection may ever reach.
        /// </summary>
        [Fact]
        public void AnExcludedProduct_IsNeverSelected()
        {
            PartOEquipmentSelection partOEquipmentSelection = Pool(MRXBOXReference(), XBC15Reference());

            List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors = partOEquipmentSelection.CandidateDescriptors(RealLadder());

            Assert.Equal(2, ventilationUnitCapacityDescriptors.Count);
            Assert.DoesNotContain(ventilationUnitCapacityDescriptors, x => x.VentilationUnitReference.Model == model_Decoy);

            //And on a duty only the decoy could serve, the pooled selection refuses rather than reaching it.
            Assert.False(Select(partOEquipmentSelection, 400).IsSelected);
        }

        /// <summary>
        /// A pool naming a product the current catalogue does not hold contributes no candidate - it cannot
        /// conjure a descriptor, because a capacity is only ever a catalogue fact.
        /// </summary>
        [Fact]
        public void APooledProductTheCatalogueDoesNotHold_ContributesNoCandidate()
        {
            PartOEquipmentSelection partOEquipmentSelection = Pool(new VentilationUnitReference("Nuaire", "XBC-Imaginary", null));

            Assert.Empty(partOEquipmentSelection.CandidateDescriptors(RealLadder()));
        }

        // =================================================================================================
        // C. Manual per dwelling - no rule runs at all
        // =================================================================================================

        /// <summary>
        /// Manual mode offers <b>no</b> candidate set. Null is what <c>Modify.PreparePartOIteration</c>
        /// already reads as "no catalogue, so select nothing and leave every existing identity alone" -
        /// which is precisely what manual authority means, reached without a new code path.
        /// </summary>
        [Fact]
        public void ManualMode_RunsNoSelectionRule()
        {
            PartOEquipmentSelection partOEquipmentSelection = new(PartOEquipmentSelectionMode.ManualPerDwelling);

            Assert.Null(partOEquipmentSelection.CandidateDescriptors(RealLadder()));
            Assert.False(partOEquipmentSelection.IsAutomatic);
        }

        /// <summary>A pool does not make manual mode automatic.</summary>
        [Fact]
        public void ManualMode_WithAPool_StillRunsNoSelectionRule()
        {
            Assert.Null(Pool(PartOEquipmentSelectionMode.ManualPerDwelling, MRXBOXReference()).CandidateDescriptors(RealLadder()));
        }

        /// <summary>
        /// With no preselection made, a manual picker offers the whole catalogue. An empty picker would make
        /// the mode unusable, and there is no selection rule here for the width to bias - which is exactly
        /// why this differs from <see cref="AnEmptyPool_RefusesExplicitly_AndNeverWidensToTheCatalogue"/>.
        /// </summary>
        [Fact]
        public void ManualMode_WithNoPreselection_OffersTheWholeCatalogue()
        {
            PartOEquipmentSelection partOEquipmentSelection = new(PartOEquipmentSelectionMode.ManualPerDwelling);

            Assert.Equal(3, partOEquipmentSelection.AllowedDescriptors(RealLadder()).Count);
        }

        /// <summary>With a preselection made, a manual picker offers exactly it.</summary>
        [Fact]
        public void ManualMode_WithAPreselection_OffersExactlyIt()
        {
            List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors = Pool(PartOEquipmentSelectionMode.ManualPerDwelling, XBC15Reference()).AllowedDescriptors(RealLadder());

            Assert.Equal(model_XBC15, Assert.Single(ventilationUnitCapacityDescriptors).VentilationUnitReference.Model);
        }

        /// <summary>
        /// The pooled AUTOMATIC mode with an empty pool offers a picker nothing, because there the engineer
        /// did say "only what I permit". The two empty pools mean different things and are read differently.
        /// </summary>
        [Fact]
        public void AnEmptyPoolInAutomaticMode_OffersAPickerNothing()
        {
            Assert.Empty(new PartOEquipmentSelection(PartOEquipmentSelectionMode.AutomaticSelectedPool).AllowedDescriptors(RealLadder()));
        }

        // =================================================================================================
        // D. Convert to Manual - an authority change and nothing else
        // =================================================================================================

        /// <summary>
        /// Converting to manual changes the authority and preserves the pool exactly. It reads no dwelling,
        /// so there is nothing here that could reselect, resize or "improve" an assignment.
        /// </summary>
        [Fact]
        public void Manual_ChangesTheAuthorityAndPreservesThePool()
        {
            PartOEquipmentSelection partOEquipmentSelection = Pool(MRXBOXReference(), XBC15Reference());

            PartOEquipmentSelection partOEquipmentSelection_Manual = partOEquipmentSelection.Manual();

            Assert.Equal(PartOEquipmentSelectionMode.ManualPerDwelling, partOEquipmentSelection_Manual.Mode);
            Assert.Equal([model_MRXBOX, model_XBC15], Models(partOEquipmentSelection_Manual.AllowedVentilationUnitReferences));

            //The original is untouched - converting produces a new statement rather than mutating one.
            Assert.Equal(PartOEquipmentSelectionMode.AutomaticSelectedPool, partOEquipmentSelection.Mode);
        }

        /// <summary>
        /// Converting from "all products" preserves an EMPTY pool, which manual mode then reads as the whole
        /// catalogue. The engineer who never narrowed anything gets every product in the picker, which is
        /// the only answer that matches what they were looking at a moment earlier.
        /// </summary>
        [Fact]
        public void ManualFromAllProducts_LeavesEveryProductPickable()
        {
            PartOEquipmentSelection partOEquipmentSelection = new PartOEquipmentSelection(PartOEquipmentSelectionMode.AutomaticAllProducts).Manual();

            Assert.False(partOEquipmentSelection.HasAllowedVentilationUnitReferences);
            Assert.Equal(3, partOEquipmentSelection.AllowedDescriptors(RealLadder()).Count);
        }

        // =================================================================================================
        // E. A pool is not an assignment, and holds no capability
        // =================================================================================================

        /// <summary>
        /// The serialised pool holds identities and <b>no maximum airflow of any kind</b>. A capacity in
        /// here would be a second answer to "what can this product move", going stale the day the catalogue
        /// is corrected, sitting in a saved project beside numbers meaning design duty and regulatory
        /// requirement.
        /// </summary>
        [Fact]
        public void ThePoolStoresIdentitiesAndNeverCapacities()
        {
            string text = Pool(MRXBOXReference(), XBC15Reference()).ToJsonObject().ToJsonString();

            Assert.Contains(model_MRXBOX, text);
            Assert.Contains(model_XBC15, text);

            //By field name rather than by digits: the serialised object carries a guid, and a guid's hex
            //could contain "150" or "190" by chance, which would make a digit assertion pass or fail for
            //reasons that have nothing to do with capacity.
            Assert.DoesNotContain("Maximum", text);
            Assert.DoesNotContain("FlowRate", text);
            Assert.DoesNotContain("Rank", text);
        }

        /// <summary>
        /// The pool is copied on the way in and on the way out, so nothing can rename a permitted product
        /// through a list that looked like a read.
        /// </summary>
        [Fact]
        public void ThePoolCannotBeEditedThroughTheListItHandsBack()
        {
            PartOEquipmentSelection partOEquipmentSelection = Pool(MRXBOXReference());

            List<VentilationUnitReference> ventilationUnitReferences = partOEquipmentSelection.AllowedVentilationUnitReferences;
            ventilationUnitReferences[0].Model = "Tampered";
            ventilationUnitReferences.Add(XBC15Reference());

            Assert.Equal([model_MRXBOX], Models(partOEquipmentSelection.AllowedVentilationUnitReferences));
        }

        /// <summary>
        /// <see cref="PartOEquipmentSelection.IsAllowed"/> is about permission and never about assignment:
        /// it answers for a product, not for a dwelling, and an un-narrowed pool permits everything except
        /// under the pooled automatic mode, where narrowing to nothing was the engineer's own instruction.
        /// </summary>
        [Fact]
        public void IsAllowed_AnswersPermissionAndNotAssignment()
        {
            Assert.True(new PartOEquipmentSelection(PartOEquipmentSelectionMode.AutomaticAllProducts).IsAllowed(XBC15Reference()));
            Assert.True(new PartOEquipmentSelection(PartOEquipmentSelectionMode.ManualPerDwelling).IsAllowed(XBC15Reference()));
            Assert.False(new PartOEquipmentSelection(PartOEquipmentSelectionMode.AutomaticSelectedPool).IsAllowed(XBC15Reference()));

            Assert.True(Pool(XBC15Reference()).IsAllowed(XBC15Reference()));
            Assert.False(Pool(XBC15Reference()).IsAllowed(MRXBOXReference()));
            Assert.False(Pool(XBC15Reference()).IsAllowed(null));
        }

        /// <summary>
        /// Permission is by identity, and identity includes the reference that distinguishes a combination -
        /// the MRXBOX is permitted as the hybrid combination the catalogue ships, not as a bare model name.
        /// </summary>
        [Fact]
        public void PermissionIsByFullIdentity()
        {
            Assert.False(Pool(MRXBOXReference()).IsAllowed(new VentilationUnitReference("Nuaire", model_MRXBOX, null)));
            Assert.True(Pool(MRXBOXReference()).IsAllowed(MRXBOXReference()));
        }

        /// <summary>A product with no identity is never permitted and is never stored in a pool.</summary>
        [Fact]
        public void AnIdentitylessProduct_IsNeverPooled()
        {
            Assert.False(new PartOEquipmentSelection(PartOEquipmentSelectionMode.AutomaticSelectedPool, [new VentilationUnitReference()]).HasAllowedVentilationUnitReferences);
        }

        // =================================================================================================
        // E2. Matches - the comparison a prepared run's reuse turns on
        // =================================================================================================

        /// <summary>
        /// Two statements of the same authority over the same permitted products match. This is what lets
        /// Prepare &amp; Run reuse an already-prepared iteration instead of preparing it again.
        /// </summary>
        [Fact]
        public void TheSameAuthorityOverTheSameProducts_Matches()
        {
            Assert.True(Pool(MRXBOXReference(), XBC15Reference()).Matches(Pool(MRXBOXReference(), XBC15Reference())));
            Assert.True(new PartOEquipmentSelection().Matches(new PartOEquipmentSelection()));
        }

        /// <summary>
        /// <b>Order carries no meaning.</b> A pool ticked the other way round is the same pool - selection
        /// already ignores order, so refusing a reuse over it would repeat a whole preparation for nothing.
        /// </summary>
        [Fact]
        public void APoolInAnotherOrder_StillMatches()
        {
            Assert.True(Pool(MRXBOXReference(), XBC15Reference()).Matches(Pool(XBC15Reference(), MRXBOXReference())));
        }

        /// <summary>
        /// <b>A different authority does not match, and this is the load-bearing case.</b> Reusing a
        /// preparation made under one mode for a request that has since changed it would simulate the
        /// products the OLD configuration chose while the dialog reported the new one - a wrong answer that
        /// looks exactly like a right one.
        /// </summary>
        [Fact]
        public void ADifferentAuthority_DoesNotMatch()
        {
            PartOEquipmentSelection partOEquipmentSelection = Pool(MRXBOXReference(), XBC15Reference());

            Assert.False(partOEquipmentSelection.Matches(partOEquipmentSelection.Manual()));
            Assert.False(partOEquipmentSelection.Matches(new PartOEquipmentSelection(PartOEquipmentSelectionMode.AutomaticAllProducts, [MRXBOXReference(), XBC15Reference()])));
        }

        /// <summary>A different pool does not match, in either direction and at either size.</summary>
        [Fact]
        public void ADifferentPool_DoesNotMatch()
        {
            Assert.False(Pool(MRXBOXReference()).Matches(Pool(XBC15Reference())));
            Assert.False(Pool(MRXBOXReference()).Matches(Pool(MRXBOXReference(), XBC15Reference())));
            Assert.False(Pool(MRXBOXReference(), XBC15Reference()).Matches(Pool(MRXBOXReference())));

            //By full identity, so the hybrid combination is not the bare model.
            Assert.False(Pool(MRXBOXReference()).Matches(Pool(new VentilationUnitReference("Nuaire", model_MRXBOX, null))));
        }

        /// <summary>Nothing matches null - there is no statement there to agree with.</summary>
        [Fact]
        public void NullMatchesNothing()
        {
            Assert.False(new PartOEquipmentSelection().Matches(null));
        }

        /// <summary>
        /// A round trip through JSON still matches what it came from - so a configuration read back off a
        /// saved project is recognised as the same one, rather than forcing a re-preparation.
        /// </summary>
        [Fact]
        public void AJsonRoundTrip_StillMatches()
        {
            PartOEquipmentSelection partOEquipmentSelection = Pool(MRXBOXReference(), XBC15Reference());

            Assert.True(partOEquipmentSelection.Matches(new PartOEquipmentSelection(partOEquipmentSelection.ToJsonObject())));
        }

        // =================================================================================================
        // F. Persistence - project scoped, and absent means the historic default
        // =================================================================================================

        /// <summary>The configuration survives its own JSON round trip.</summary>
        [Fact]
        public void TheConfiguration_SurvivesItsJsonRoundTrip()
        {
            PartOEquipmentSelection read = new(Pool(MRXBOXReference(), XBC15Reference()).ToJsonObject());

            Assert.Equal(PartOEquipmentSelectionMode.AutomaticSelectedPool, read.Mode);
            Assert.Equal([model_MRXBOX, model_XBC15], Models(read.AllowedVentilationUnitReferences));
        }

        /// <summary>
        /// And survives being stamped on a project and read back off it - which is what makes the mode and
        /// the pool outlive both the Part O dialog and the project being closed. It rides in the model's own
        /// parameters beside <see cref="PartOIsolationContext"/>, so it cannot leak into another project.
        /// </summary>
        [Fact]
        public void TheConfiguration_SurvivesTheProjectsJsonRoundTrip()
        {
            AnalyticalModel analyticalModel = new("Fixture", null, null, null, new AdjacencyCluster());

            analyticalModel.SetValue(AnalyticalModelParameter.PartOEquipmentSelection, Pool(PartOEquipmentSelectionMode.ManualPerDwelling, XBC15Reference()));

            AnalyticalModel analyticalModel_Read = new(analyticalModel.ToJsonObject());

            PartOEquipmentSelection read = analyticalModel_Read.GetValue<PartOEquipmentSelection>(AnalyticalModelParameter.PartOEquipmentSelection);

            Assert.NotNull(read);
            Assert.Equal(PartOEquipmentSelectionMode.ManualPerDwelling, read.Mode);
            Assert.Equal([model_XBC15], Models(read.AllowedVentilationUnitReferences));
        }

        /// <summary>
        /// A project that has never said anything reads as the historic default - automatic over the whole
        /// catalogue - so nothing needs migrating and an old project behaves today exactly as it did.
        /// </summary>
        [Fact]
        public void AProjectThatSaysNothing_ReadsAsTheHistoricDefault()
        {
            AnalyticalModel analyticalModel = new("Fixture", null, null, null, new AdjacencyCluster());

            Assert.Null(analyticalModel.GetValue<PartOEquipmentSelection>(AnalyticalModelParameter.PartOEquipmentSelection));

            //Which is what the default-constructed statement says, and it narrows nothing.
            PartOEquipmentSelection partOEquipmentSelection = new();

            Assert.Equal(PartOEquipmentSelectionMode.AutomaticAllProducts, partOEquipmentSelection.Mode);
            Assert.False(partOEquipmentSelection.HasAllowedVentilationUnitReferences);
            Assert.Equal(3, partOEquipmentSelection.CandidateDescriptors(RealLadder()).Count);
        }

        /// <summary>
        /// A mode written by a newer SAM, or corrupted, reads as the historic default rather than refusing.
        /// This is project configuration: a project whose preference cannot be read has no preference, and
        /// that is a safe answer because the default narrows nothing and hides nothing.
        /// </summary>
        [Fact]
        public void AnUnreadableMode_ReadsAsTheHistoricDefault()
        {
            System.Text.Json.Nodes.JsonObject jsonObject = Pool(PartOEquipmentSelectionMode.ManualPerDwelling, XBC15Reference()).ToJsonObject();
            jsonObject["Mode"] = "SomethingAFutureVersionInvented";

            PartOEquipmentSelection read = new(jsonObject);

            Assert.Equal(PartOEquipmentSelectionMode.AutomaticAllProducts, read.Mode);

            //The pool it could read is still read - one unreadable field does not discard the rest.
            Assert.Equal([model_XBC15], Models(read.AllowedVentilationUnitReferences));
        }

        /// <summary>The copy constructor is a copy, not a share.</summary>
        [Fact]
        public void TheCopyConstructor_CopiesThePool()
        {
            PartOEquipmentSelection partOEquipmentSelection = Pool(MRXBOXReference());

            PartOEquipmentSelection partOEquipmentSelection_Copy = new(partOEquipmentSelection)
            {
                Mode = PartOEquipmentSelectionMode.ManualPerDwelling,
            };

            Assert.Equal(PartOEquipmentSelectionMode.AutomaticSelectedPool, partOEquipmentSelection.Mode);
            Assert.Equal([model_MRXBOX], Models(partOEquipmentSelection_Copy.AllowedVentilationUnitReferences));
        }

        // =================================================================================================
        // Fixtures
        // =================================================================================================

        /// <summary>
        /// The two shipped products at their shipped capacities and ranks, plus a 500 l/s decoy that no test
        /// expects to be chosen. The decoy is the tripwire: any rule that reached past a pool, or preferred
        /// the biggest or the nearest product, would select it.
        /// </summary>
        private static List<VentilationUnitCapacityDescriptor> RealLadder(bool decoy = true)
        {
            List<VentilationUnitCapacityDescriptor> result =
            [
                new VentilationUnitCapacityDescriptor(MRXBOXReference(), 150, 150, 10),
                new VentilationUnitCapacityDescriptor(XBC15Reference(), 190, 190, 20),
            ];

            if (decoy)
            {
                result.Add(new VentilationUnitCapacityDescriptor(new VentilationUnitReference("Test Fixture", model_Decoy, null), 500, 500, 99));
            }

            return result;
        }

        private static VentilationUnitReference MRXBOXReference()
        {
            return new VentilationUnitReference("Nuaire", model_MRXBOX, "MR-ECO-COOL-V");
        }

        private static VentilationUnitReference XBC15Reference()
        {
            return new VentilationUnitReference("Nuaire", model_XBC15, null);
        }

        private static PartOEquipmentSelection Pool(params VentilationUnitReference[] ventilationUnitReferences)
        {
            return Pool(PartOEquipmentSelectionMode.AutomaticSelectedPool, ventilationUnitReferences);
        }

        private static PartOEquipmentSelection Pool(PartOEquipmentSelectionMode partOEquipmentSelectionMode, params VentilationUnitReference[] ventilationUnitReferences)
        {
            return new PartOEquipmentSelection(partOEquipmentSelectionMode, ventilationUnitReferences);
        }

        /// <summary>
        /// The production selection rule, over exactly the candidates the configuration permits. Balanced
        /// duty, because both shipped products are balanced heat-recovery cores.
        /// </summary>
        private static VentilationUnitSelection Select(PartOEquipmentSelection partOEquipmentSelection, double duty_Lps, List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors = null)
        {
            return partOEquipmentSelection
                .CandidateDescriptors(ventilationUnitCapacityDescriptors ?? RealLadder())
                .SelectSmallestCapableVentilationUnit(duty_Lps, duty_Lps);
        }

        private static List<string> Models(List<VentilationUnitReference> ventilationUnitReferences)
        {
            List<string> result = ventilationUnitReferences.ConvertAll(x => x.Model);

            result.Sort(System.StringComparer.Ordinal);

            return result;
        }
    }
}
