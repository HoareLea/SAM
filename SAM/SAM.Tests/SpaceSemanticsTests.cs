// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Core;
using SAM.Geometry.Spatial;
using SAM.Tests.Helpers;
using System.Collections.Generic;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// Tests for the shared semantic space classification layer consumed by Approved Document F,
    /// Approved Document O, CIBSE TM59 and the SAM_UI internal condition mapping.
    /// </summary>
    /// <remarks>
    /// The classification vocabulary under test is the real shipped resource
    /// SAM_SpaceUseTextMap.JSON, not a fixture, so these tests also lock the shipped synonyms against
    /// accidental edits that would reintroduce an ambiguous or over-broad alias.
    /// </remarks>
    public class SpaceSemanticsTests
    {
        private const string textMapFileName = "SAM_SpaceUseTextMap.JSON";

        // ------------------------------------------------------------------
        // No unrestricted substring matching
        // ------------------------------------------------------------------

        /// <summary>
        /// The defect the shared layer exists to remove. TextMap.GetSortedKeys scores tokens with a
        /// bidirectional Contains, so "Server Room" scored against the "living room" and "shower room"
        /// aliases on the shared token "room" alone and resolved to whichever candidate sorted first.
        /// An alias must only ever match as a whole token or whole contiguous phrase.
        /// </summary>
        [Fact]
        public void ServerRoom_IsNotClassifiedAsLiving()
        {
            SpaceSemantics spaceSemantics = Resolve("Server Room");

            Assert.Equal(SpaceUse.Undefined, spaceSemantics.SpaceUse);
            Assert.NotEqual(SpaceUse.LivingRoom, spaceSemantics.SpaceUse);
            Assert.Equal(SpaceSemanticsSource.Unclassified, spaceSemantics.Source);
            Assert.False(string.IsNullOrWhiteSpace(spaceSemantics.Diagnostic));
        }

        /// <summary>
        /// The same guarantee reached through the Part F rule set, which is how the defect actually
        /// presented: a server room must never be given a habitable room's supply terminal.
        /// </summary>
        [Fact]
        public void ServerRoom_IsNotGivenAPartFCategory()
        {
            PartFData partFData = Analytical.Create.PartFData(Fixtures.GetPath("SAM_PartFSpaceRulesUKDwellingsMVHR.json"));

            Assert.Null(partFData.GetPartFCategory("Server Room"));
        }

        /// <summary>
        /// Names that share a single generic token with an alias but are not that room must not resolve
        /// to it. Each of these previously scored against a "* room" alias on "room" alone.
        /// </summary>
        [Theory]
        [InlineData("Server Room")]
        [InlineData("Meeting Room")]
        [InlineData("Comms Room")]
        [InlineData("Room 12")]
        public void GenericRoomNames_AreNotClassified(string name)
        {
            Assert.Equal(SpaceUse.Undefined, Resolve(name).SpaceUse);
        }

        // ------------------------------------------------------------------
        // The SAM design convention table
        // ------------------------------------------------------------------

        /// <summary>
        /// A studio is habitable, bedroom-equivalent and a cooking space, and takes the supply role
        /// only. It is deliberately NOT a wet room: SAM assigns one terminal role per space, so the
        /// cooking function is carried by IsCookingSpace and reported rather than given an extract.
        /// </summary>
        [Fact]
        public void Studio_IsHabitableBedroomEquivalentCookingSupplyOnly()
        {
            SpaceSemantics spaceSemantics = Resolve("Studio");

            Assert.Equal(SpaceUse.Studio, spaceSemantics.SpaceUse);
            Assert.True(spaceSemantics.IsHabitable);
            Assert.True(spaceSemantics.IsBedroomEquivalent);
            Assert.True(spaceSemantics.IsCookingSpace);
            Assert.True(spaceSemantics.HasSupplyRole);
            Assert.False(spaceSemantics.HasExtractRole);
            Assert.False(spaceSemantics.IsWetRoom);
        }

        /// <summary>
        /// An open plan living kitchen is habitable and a cooking space but NOT bedroom-equivalent, and
        /// takes the supply role only. Approved Document F, Volume 1, Appendix A makes it habitable
        /// because it is not solely a kitchen.
        /// </summary>
        [Fact]
        public void LivingKitchen_IsHabitableCookingSupplyOnlyAndNotBedroomEquivalent()
        {
            SpaceSemantics spaceSemantics = Resolve("Living Kitchen");

            Assert.Equal(SpaceUse.LivingRoomKitchen, spaceSemantics.SpaceUse);
            Assert.True(spaceSemantics.IsHabitable);
            Assert.False(spaceSemantics.IsBedroomEquivalent);
            Assert.True(spaceSemantics.IsCookingSpace);
            Assert.True(spaceSemantics.HasSupplyRole);
            Assert.False(spaceSemantics.HasExtractRole);
        }

        /// <summary>A bathroom is not habitable, is a wet room, and takes the extract role.</summary>
        [Fact]
        public void Bathroom_IsNotHabitableIsWetRoomAndTakesExtract()
        {
            SpaceSemantics spaceSemantics = Resolve("Bathroom");

            Assert.Equal(SpaceUse.Bathroom, spaceSemantics.SpaceUse);
            Assert.False(spaceSemantics.IsHabitable);
            Assert.True(spaceSemantics.IsWetRoom);
            Assert.True(spaceSemantics.HasExtractRole);
            Assert.False(spaceSemantics.HasSupplyRole);
        }

        /// <summary>
        /// A room that is solely a kitchen is a wet room and NOT habitable, per Appendix A. This is the
        /// distinction that makes the living-kitchen case above defensible rather than arbitrary.
        /// </summary>
        [Fact]
        public void Kitchen_IsAWetRoomAndNotHabitable()
        {
            SpaceSemantics spaceSemantics = Resolve("Kitchen");

            Assert.Equal(SpaceUse.Kitchen, spaceSemantics.SpaceUse);
            Assert.False(spaceSemantics.IsHabitable);
            Assert.True(spaceSemantics.IsWetRoom);
            Assert.True(spaceSemantics.IsCookingSpace);
            Assert.True(spaceSemantics.HasExtractRole);
        }

        /// <summary>Communal circulation belongs to no dwelling, so it must be excluded from one.</summary>
        [Fact]
        public void CommunalCirculation_IsNotADwellingSpace()
        {
            SpaceSemantics spaceSemantics = Resolve("Communal Corridor");

            Assert.Equal(SpaceUse.CommunalCirculation, spaceSemantics.SpaceUse);
            Assert.False(spaceSemantics.IsDwellingSpace);
            Assert.True(spaceSemantics.IsCommunal);
            Assert.True(spaceSemantics.IsCirculation);
        }

        /// <summary>Circulation inside a dwelling is part of that dwelling, unlike communal circulation.</summary>
        [Fact]
        public void Circulation_IsADwellingSpace()
        {
            SpaceSemantics spaceSemantics = Resolve("Hall");

            Assert.Equal(SpaceUse.Circulation, spaceSemantics.SpaceUse);
            Assert.True(spaceSemantics.IsDwellingSpace);
            Assert.False(spaceSemantics.IsCommunal);
        }

        /// <summary>
        /// An unclassified space must not be assumed to sit outside the dwelling: an unrecognised name is
        /// a reporting problem, not evidence that the room is communal.
        /// </summary>
        [Fact]
        public void UnclassifiedSpace_IsStillTreatedAsPartOfTheDwelling()
        {
            Assert.True(Resolve("Server Room").IsDwellingSpace);
        }

        /// <summary>
        /// A longer phrase beats a shorter one, so a communal corridor is not read as dwelling
        /// circulation and a lift lobby is not read as a dwelling lobby.
        /// </summary>
        [Theory]
        [InlineData("Communal Corridor", SpaceUse.CommunalCirculation)]
        [InlineData("Corridor", SpaceUse.Circulation)]
        [InlineData("Lift Lobby", SpaceUse.CommunalCirculation)]
        [InlineData("Lobby", SpaceUse.Circulation)]
        [InlineData("Living Kitchen", SpaceUse.LivingRoomKitchen)]
        [InlineData("Living Room", SpaceUse.LivingRoom)]
        [InlineData("Ensuite WC", SpaceUse.SanitaryAccommodation)]
        [InlineData("Ensuite", SpaceUse.Ensuite)]
        public void LongerPhrase_BeatsShorterPhrase(string name, SpaceUse expected)
        {
            Assert.Equal(expected, Resolve(name).SpaceUse);
        }

        /// <summary>Trailing room numbers and separators must not defeat recognition.</summary>
        [Theory]
        [InlineData("Bedroom 1", SpaceUse.Bedroom)]
        [InlineData("Bedroom_2", SpaceUse.Bedroom)]
        [InlineData("Kitchen-4", SpaceUse.Kitchen)]
        [InlineData("  BATHROOM  ", SpaceUse.Bathroom)]
        [InlineData("Studio 1_0", SpaceUse.Studio)]
        public void NormalisationHandlesCaseNumbersAndSeparators(string name, SpaceUse expected)
        {
            Assert.Equal(expected, Resolve(name).SpaceUse);
        }

        // ------------------------------------------------------------------
        // Resolution priority
        // ------------------------------------------------------------------

        /// <summary>An explicit override wins over anything the name would otherwise match.</summary>
        [Fact]
        public void UserOverride_WinsOverNameMatching()
        {
            Space space = Space("Bedroom 1");
            space.SetValue(SpaceParameter.SpaceUseOverride, SpaceUse.PlantRoom.ToString());

            SpaceSemantics spaceSemantics = Resolver().Resolve(space);

            Assert.Equal(SpaceUse.PlantRoom, spaceSemantics.SpaceUse);
            Assert.Equal(SpaceSemanticsSource.UserOverride, spaceSemantics.Source);
        }

        /// <summary>An override lets an otherwise unrecognisable name be classified deliberately.</summary>
        [Fact]
        public void UserOverride_ClassifiesAnUnrecognisedName()
        {
            Space space = Space("Server Room");
            space.SetValue(SpaceParameter.SpaceUseOverride, "PlantRoom");

            Assert.Equal(SpaceUse.PlantRoom, Resolver().Resolve(space).SpaceUse);
        }

        /// <summary>
        /// An override naming nothing real must be reported, not silently ignored - otherwise the space
        /// falls through to name matching and appears to have been classified correctly.
        /// </summary>
        [Fact]
        public void UnrecognisedUserOverride_IsReportedAndDoesNotFallThrough()
        {
            Space space = Space("Bedroom 1");
            space.SetValue(SpaceParameter.SpaceUseOverride, "NotASpaceUse");

            SpaceSemantics spaceSemantics = Resolver().Resolve(space);

            Assert.Equal(SpaceUse.Undefined, spaceSemantics.SpaceUse);
            Assert.Equal(SpaceSemanticsSource.Unclassified, spaceSemantics.Source);
            Assert.Contains("NotASpaceUse", spaceSemantics.Diagnostic);
        }

        /// <summary>
        /// An InternalCondition must not silently override a space name that resolves to something
        /// different.
        /// <para>
        /// Regression, found by validating against the real SAM_zoningAM model: that model carries the
        /// TM59 "Studio" condition on spaces named Bathroom_2, Ensuite_5 and Corridor_1. Trusting the
        /// condition over the name turned each into a habitable supply space, removed the only extract in
        /// the flat, and left supply and extract unbalanced. An InternalCondition records a thermal
        /// condition, not a room use, and is routinely assigned in bulk.
        /// </para>
        /// </summary>
        [Theory]
        [InlineData("Bathroom_2", SpaceUse.Bathroom)]
        [InlineData("Ensuite_5", SpaceUse.Ensuite)]
        [InlineData("Corridor_1", SpaceUse.Circulation)]
        [InlineData("WC", SpaceUse.SanitaryAccommodation)]
        public void ConflictingInternalCondition_DoesNotOverrideTheSpaceName(string name, SpaceUse expected)
        {
            Space space = Space(name);
            space.InternalCondition = new InternalCondition("Studio");

            SpaceSemantics spaceSemantics = Resolver().Resolve(space);

            Assert.Equal(expected, spaceSemantics.SpaceUse);
            Assert.NotEqual(SpaceUse.Studio, spaceSemantics.SpaceUse);
        }

        /// <summary>The conflict is reported, never resolved silently.</summary>
        [Fact]
        public void ConflictingInternalCondition_IsReported()
        {
            Space space = Space("Bathroom_2");
            space.InternalCondition = new InternalCondition("Studio");

            SpaceSemantics spaceSemantics = Resolver().Resolve(space);

            Assert.True(spaceSemantics.HasSourceConflict);
            Assert.False(string.IsNullOrWhiteSpace(spaceSemantics.Diagnostic));
            Assert.Contains("CONFLICT", spaceSemantics.Diagnostic);
            Assert.Contains("Studio", spaceSemantics.Diagnostic);
            Assert.Contains("Bathroom", spaceSemantics.Diagnostic);
        }

        /// <summary>
        /// Both source values survive a conflict. The higher-priority result is used, but neither source is
        /// overwritten, so SAM_UI can show the engineer exactly what each source said.
        /// </summary>
        [Fact]
        public void ConflictingInternalCondition_PreservesBothSourceValues()
        {
            Space space = Space("Bathroom_2");
            space.InternalCondition = new InternalCondition("Studio");

            SpaceSemantics spaceSemantics = Resolver().Resolve(space);

            //The resolved answer is the name-derived one...
            Assert.Equal(SpaceUse.Bathroom, spaceSemantics.SpaceUse);

            //...and both underlying source values are still readable.
            Assert.Equal(SpaceUse.Bathroom, spaceSemantics.SpaceUse_Name);
            Assert.Equal(SpaceUse.Studio, spaceSemantics.SpaceUse_InternalCondition);
        }

        /// <summary>Where the two sources agree, both are recorded and no conflict is flagged.</summary>
        [Fact]
        public void AgreeingSources_AreBothRecordedWithoutConflict()
        {
            Space space = Space("Bedroom 1");
            space.InternalCondition = new InternalCondition("Double Bedroom");

            SpaceSemantics spaceSemantics = Resolver().Resolve(space);

            Assert.False(spaceSemantics.HasSourceConflict);
            Assert.Equal(SpaceUse.Bedroom, spaceSemantics.SpaceUse_Name);
            Assert.Equal(SpaceUse.Bedroom, spaceSemantics.SpaceUse_InternalCondition);
        }

        // ------------------------------------------------------------------
        // Compatible refinements are not conflicts
        // ------------------------------------------------------------------

        /// <summary>
        /// Circulation (in-dwelling) and Communal Circulation are not a disagreement: Communal
        /// Circulation is Circulation, just outside the dwelling boundary rather than inside it. The
        /// more specific value wins and no conflict is reported.
        /// </summary>
        [Fact]
        public void CirculationAndCommunalCirculation_AreCompatibleNotConflicting()
        {
            Space space = Space("Corridor_1");
            space.InternalCondition = new InternalCondition("Communal Corridor");

            SpaceSemantics spaceSemantics = Resolver().Resolve(space);

            Assert.Equal(SpaceUse.CommunalCirculation, spaceSemantics.SpaceUse);
            Assert.False(spaceSemantics.HasSourceConflict);
            Assert.Equal(SpaceUse.Circulation, spaceSemantics.SpaceUse_Name);
            Assert.Equal(SpaceUse.CommunalCirculation, spaceSemantics.SpaceUse_InternalCondition);
        }

        /// <summary>
        /// The pairing resolves the same way whichever source produced the more specific value - it is
        /// the specificity that matters, not which source (name vs internal condition) said it.
        /// </summary>
        [Fact]
        public void CirculationAndCommunalCirculation_ResolveToTheMoreSpecificValue_RegardlessOfSource()
        {
            Space space = Space("Communal Corridor");
            space.InternalCondition = new InternalCondition("Hall");

            SpaceSemantics spaceSemantics = Resolver().Resolve(space);

            Assert.Equal(SpaceUse.CommunalCirculation, spaceSemantics.SpaceUse);
            Assert.False(spaceSemantics.HasSourceConflict);
        }

        /// <summary>
        /// An ensuite is a bathroom accessed from a bedroom - the same room said more precisely - so a
        /// space named Ensuite carrying the TM59_Bathroom condition is not a disagreement.
        /// <para>
        /// This is the pairing the TM59 condition split produces: those spaces previously carried one
        /// combined "TM59_Bathroom/internal corridors" condition, whose name resolved to Circulation and
        /// so conflicted with every bathroom and ensuite that held it.
        /// </para>
        /// </summary>
        [Fact]
        public void BathroomAndEnsuite_AreCompatibleNotConflicting()
        {
            Space space = Space("Ensuite5");
            space.InternalCondition = new InternalCondition("TM59_Bathroom");

            SpaceSemantics spaceSemantics = Resolver().Resolve(space);

            Assert.Equal(SpaceUse.Ensuite, spaceSemantics.SpaceUse);
            Assert.False(spaceSemantics.HasSourceConflict);
        }

        /// <summary>
        /// A space named Bathroom carrying the split TM59_Bathroom condition agrees outright - the
        /// straightforward case the split exists to produce.
        /// </summary>
        [Fact]
        public void BathroomSpace_WithTheSplitBathroomCondition_HasNoConflict()
        {
            Space space = Space("Bathroom_2");
            space.InternalCondition = new InternalCondition("TM59_Bathroom");

            SpaceSemantics spaceSemantics = Resolver().Resolve(space);

            Assert.Equal(SpaceUse.Bathroom, spaceSemantics.SpaceUse);
            Assert.False(spaceSemantics.HasSourceConflict);
        }

        /// <summary>
        /// Sanitary accommodation is NOT a refinement of Bathroom: Approved Document F Table 1.2 gives
        /// them different extract rates, so silently treating one as the other would change a
        /// calculated rate. It must stay a reported conflict.
        /// </summary>
        [Fact]
        public void BathroomAndSanitaryAccommodation_RemainAGenuineConflict()
        {
            Space space = Space("WC");
            space.InternalCondition = new InternalCondition("TM59_Bathroom");

            SpaceSemantics spaceSemantics = Resolver().Resolve(space);

            Assert.Equal(SpaceUse.SanitaryAccommodation, spaceSemantics.SpaceUse);
            Assert.True(spaceSemantics.HasSourceConflict);
        }

        /// <summary>
        /// Bathroom versus Studio is a genuinely incompatible pair - neither is a refinement of the
        /// other - so it must remain a reported conflict.
        /// </summary>
        [Fact]
        public void BathroomAndStudio_RemainAGenuineConflict()
        {
            Space space = Space("Bathroom_2");
            space.InternalCondition = new InternalCondition("Studio");

            SpaceSemantics spaceSemantics = Resolver().Resolve(space);

            Assert.True(spaceSemantics.HasSourceConflict);
            Assert.Equal(SpaceUse.Bathroom, spaceSemantics.SpaceUse);
        }

        /// <summary>
        /// An explicit override still wins outright even where the name and internal condition would
        /// otherwise resolve as a compatible refinement rather than a conflict - the override check runs
        /// first and returns immediately, before either source is even matched.
        /// </summary>
        [Fact]
        public void UserOverride_WinsEvenOverACompatibleRefinementPair()
        {
            Space space = Space("Corridor_1");
            space.InternalCondition = new InternalCondition("Communal Corridor");
            space.SetValue(SpaceParameter.SpaceUseOverride, SpaceUse.PlantRoom.ToString());

            SpaceSemantics spaceSemantics = Resolver().Resolve(space);

            Assert.Equal(SpaceUse.PlantRoom, spaceSemantics.SpaceUse);
            Assert.Equal(SpaceSemanticsSource.UserOverride, spaceSemantics.Source);
        }

        /// <summary>
        /// Where only the InternalCondition resolved, the name source is recorded as Undefined rather than
        /// being conflated with the winning value.
        /// </summary>
        [Fact]
        public void InternalConditionOnly_RecordsAnUndefinedNameSource()
        {
            Space space = Space("Zone A 14");
            space.InternalCondition = new InternalCondition("Double Bedroom");

            SpaceSemantics spaceSemantics = Resolver().Resolve(space);

            Assert.False(spaceSemantics.HasSourceConflict);
            Assert.Equal(SpaceUse.Undefined, spaceSemantics.SpaceUse_Name);
            Assert.Equal(SpaceUse.Bedroom, spaceSemantics.SpaceUse_InternalCondition);
        }

        /// <summary>The conflict flag and both source values survive serialisation.</summary>
        [Fact]
        public void SourceConflict_RoundTripsThroughJson()
        {
            Space space = Space("Bathroom_2");
            space.InternalCondition = new InternalCondition("Studio");

            SpaceSemantics spaceSemantics = Resolver().Resolve(space);
            SpaceSemantics spaceSemantics_RoundTrip = new(spaceSemantics.ToJsonObject());

            Assert.True(spaceSemantics_RoundTrip.HasSourceConflict);
            Assert.Equal(SpaceUse.Bathroom, spaceSemantics_RoundTrip.SpaceUse_Name);
            Assert.Equal(SpaceUse.Studio, spaceSemantics_RoundTrip.SpaceUse_InternalCondition);
        }

        /// <summary>
        /// Where the space name means nothing, the InternalCondition does classify the space - that is the
        /// case an explicit mapping is genuinely useful for.
        /// </summary>
        [Fact]
        public void InternalCondition_ClassifiesASpaceWhoseNameMeansNothing()
        {
            Space space = Space("Zone A 14");
            space.InternalCondition = new InternalCondition("Double Bedroom");

            SpaceSemantics spaceSemantics = Resolver().Resolve(space);

            Assert.Equal(SpaceUse.Bedroom, spaceSemantics.SpaceUse);
            Assert.Equal(SpaceSemanticsSource.InternalCondition, spaceSemantics.Source);
        }

        /// <summary>An agreeing InternalCondition simply confirms the name, with no conflict reported.</summary>
        [Fact]
        public void AgreeingInternalCondition_RaisesNoConflict()
        {
            Space space = Space("Bedroom 1");
            space.InternalCondition = new InternalCondition("Double Bedroom");

            SpaceSemantics spaceSemantics = Resolver().Resolve(space);

            Assert.Equal(SpaceUse.Bedroom, spaceSemantics.SpaceUse);
            Assert.True(string.IsNullOrWhiteSpace(spaceSemantics.Diagnostic));
        }

        /// <summary>An explicit override still beats a conflicting name and InternalCondition together.</summary>
        [Fact]
        public void UserOverride_BeatsBothTheNameAndTheInternalCondition()
        {
            Space space = Space("Bathroom_2");
            space.InternalCondition = new InternalCondition("Studio");
            space.SetValue(SpaceParameter.SpaceUseOverride, "UtilityRoom");

            Assert.Equal(SpaceUse.UtilityRoom, Resolver().Resolve(space).SpaceUse);
        }

        /// <summary>An exact synonym is reported as such, so the match can be traced.</summary>
        [Fact]
        public void ExactSynonym_IsReportedAsTheSource()
        {
            Assert.Equal(SpaceSemanticsSource.ExactSynonym, Resolve("Bathroom").Source);
        }

        /// <summary>A name containing an alias as a phrase is reported as a phrase match.</summary>
        [Fact]
        public void PhraseMatch_IsReportedAsTheSource()
        {
            Assert.Equal(SpaceSemanticsSource.PhraseMatch, Resolve("Main Bathroom Upper").Source);
        }

        /// <summary>
        /// Two space uses matching at the same phrase length AND the same character length is a genuine
        /// conflict that the matcher cannot rank. It must be reported, never resolved by whichever
        /// happened to sort first. "Plant Store" ties "plant" (PlantRoom) against "store" (Storage):
        /// both one token, both five characters.
        /// </summary>
        [Fact]
        public void AmbiguousMatch_IsReportedRatherThanGuessed()
        {
            SpaceSemantics spaceSemantics = Resolve("Plant Store");

            Assert.Equal(SpaceUse.Undefined, spaceSemantics.SpaceUse);
            Assert.Equal(SpaceSemanticsSource.Unclassified, spaceSemantics.Source);
            Assert.False(string.IsNullOrWhiteSpace(spaceSemantics.Diagnostic));
            Assert.Contains("PlantRoom", spaceSemantics.Diagnostic);
            Assert.Contains("Storage", spaceSemantics.Diagnostic);
        }

        /// <summary>
        /// Where two aliases tie on phrase length, the longer alias wins - the matcher's documented
        /// tie-break, inherited from the TM59 implementation. "Double Kitchen" therefore resolves to
        /// Kitchen: the kitchen noun (7 characters) outranks the bedroom-size modifier "double" (6).
        /// This is deterministic, not a guess, and is asserted so the tie-break cannot change silently.
        /// </summary>
        [Fact]
        public void TiedPhraseLength_IsBrokenByTheLongerAlias()
        {
            SpaceSemantics spaceSemantics = Resolve("Double Kitchen");

            Assert.Equal(SpaceUse.Kitchen, spaceSemantics.SpaceUse);
            Assert.NotEqual(SpaceSemanticsSource.Unclassified, spaceSemantics.Source);
        }

        /// <summary>Classification must be deterministic: the same name always gives the same answer.</summary>
        [Theory]
        [InlineData("Bedroom 1")]
        [InlineData("Living Kitchen")]
        [InlineData("Server Room")]
        [InlineData("Communal Corridor")]
        public void Classification_IsDeterministic(string name)
        {
            SpaceUse first = Resolve(name).SpaceUse;

            for (int i = 0; i < 25; i++)
            {
                //A fresh resolver each time, so a cache hit cannot be what makes this look stable.
                Assert.Equal(first, Resolve(name).SpaceUse);
            }
        }

        /// <summary>
        /// The cache must not outlive the name it was computed from. A Space can only be "renamed" by
        /// constructing a new one with the same Guid, and a stale classification must not then be
        /// returned forever.
        /// </summary>
        [Fact]
        public void Cache_IsInvalidatedWhenTheNameChanges()
        {
            SpaceSemanticsResolver spaceSemanticsResolver = Resolver();

            Space space = Space("Bedroom 1");
            Assert.Equal(SpaceUse.Bedroom, spaceSemanticsResolver.Resolve(space).SpaceUse);

            Space space_Renamed = new(space, "Bathroom", new Point3D(0, 0, 1.5));
            Assert.Equal(SpaceUse.Bathroom, spaceSemanticsResolver.Resolve(space_Renamed).SpaceUse);
        }

        /// <summary>An override applied after a first resolution must invalidate the cached answer too.</summary>
        [Fact]
        public void Cache_IsInvalidatedWhenAnOverrideIsApplied()
        {
            SpaceSemanticsResolver spaceSemanticsResolver = Resolver();

            Space space = Space("Bedroom 1");
            Assert.Equal(SpaceUse.Bedroom, spaceSemanticsResolver.Resolve(space).SpaceUse);

            space.SetValue(SpaceParameter.SpaceUseOverride, "PlantRoom");
            Assert.Equal(SpaceUse.PlantRoom, spaceSemanticsResolver.Resolve(space).SpaceUse);
        }

        // ------------------------------------------------------------------
        // Round trip and cross-standard mapping
        // ------------------------------------------------------------------

        /// <summary>Semantics must survive serialisation so a mapping can be persisted and reused.</summary>
        [Fact]
        public void SpaceSemantics_RoundTripsThroughJson()
        {
            SpaceSemantics spaceSemantics = Resolve("Studio");

            SpaceSemantics spaceSemantics_RoundTrip = new(spaceSemantics.ToJsonObject());

            Assert.Equal(spaceSemantics.SpaceUse, spaceSemantics_RoundTrip.SpaceUse);
            Assert.Equal(spaceSemantics.Source, spaceSemantics_RoundTrip.Source);
            Assert.Equal(spaceSemantics.IsHabitable, spaceSemantics_RoundTrip.IsHabitable);
            Assert.Equal(spaceSemantics.IsBedroomEquivalent, spaceSemantics_RoundTrip.IsBedroomEquivalent);
            Assert.Equal(spaceSemantics.IsCookingSpace, spaceSemantics_RoundTrip.IsCookingSpace);
            Assert.Equal(spaceSemantics.HasSupplyRole, spaceSemantics_RoundTrip.HasSupplyRole);
            Assert.Equal(spaceSemantics.HasExtractRole, spaceSemantics_RoundTrip.HasExtractRole);
        }

        /// <summary>
        /// The habitable space uses map one-to-one onto the TM59 classifications, so Part F, Part O and
        /// TM59 agree on the rooms that all three assess.
        /// </summary>
        [Theory]
        [InlineData(SpaceUse.Bedroom, TM59SpaceClassification.Bedroom)]
        [InlineData(SpaceUse.LivingRoom, TM59SpaceClassification.LivingRoom)]
        [InlineData(SpaceUse.Kitchen, TM59SpaceClassification.Kitchen)]
        [InlineData(SpaceUse.LivingRoomKitchen, TM59SpaceClassification.LivingRoomKitchen)]
        [InlineData(SpaceUse.Studio, TM59SpaceClassification.Studio)]
        public void HabitableSpaceUses_RoundTripWithTM59(SpaceUse spaceUse, TM59SpaceClassification expected)
        {
            Assert.Equal(expected, spaceUse.TM59SpaceClassification());
            Assert.Equal(spaceUse, expected.SpaceUse());
        }

        /// <summary>
        /// Every space use TM59 regards as non-habitable collapses to NonHabitable, and that direction is
        /// lossy on purpose: TM59 cannot distinguish a bathroom from a corridor, which is exactly why the
        /// TM59 classification must not be allowed to drive Part F wet room rates.
        /// </summary>
        [Theory]
        [InlineData(SpaceUse.Bathroom)]
        [InlineData(SpaceUse.Ensuite)]
        [InlineData(SpaceUse.UtilityRoom)]
        [InlineData(SpaceUse.SanitaryAccommodation)]
        [InlineData(SpaceUse.Circulation)]
        [InlineData(SpaceUse.CommunalCirculation)]
        [InlineData(SpaceUse.Storage)]
        [InlineData(SpaceUse.PlantRoom)]
        public void NonHabitableSpaceUses_CollapseToTM59NonHabitable(SpaceUse spaceUse)
        {
            Assert.Equal(TM59SpaceClassification.NonHabitable, spaceUse.TM59SpaceClassification());
            Assert.Equal(SpaceUse.Undefined, TM59SpaceClassification.NonHabitable.SpaceUse());
        }

        // ------------------------------------------------------------------
        // The shipped vocabulary itself
        // ------------------------------------------------------------------

        /// <summary>The shared vocabulary must be shipped and must define every space use it claims to.</summary>
        [Fact]
        public void SharedVocabulary_IsShippedAndCoversEverySpaceUse()
        {
            TextMap textMap = TextMap();
            Assert.NotNull(textMap);

            List<string> keys = [.. textMap.Keys];

            foreach (SpaceUse spaceUse in System.Enum.GetValues(typeof(SpaceUse)))
            {
                if (spaceUse == SpaceUse.Undefined)
                {
                    continue;
                }

                Assert.Contains(spaceUse.ToString(), keys);
            }
        }

        /// <summary>
        /// A bare generic token must never be an alias: that is what let "room" bridge "Server Room" to
        /// "living room". Locks the shipped vocabulary against reintroducing one.
        /// </summary>
        [Theory]
        [InlineData("room")]
        [InlineData("space")]
        [InlineData("area")]
        [InlineData("zone")]
        public void SharedVocabulary_ContainsNoBareGenericToken(string token)
        {
            TextMap textMap = TextMap();

            foreach (string key in textMap.Keys)
            {
                Assert.DoesNotContain(token, textMap.GetValues(key));
            }
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private static TextMap TextMap()
        {
            return Core.Create.IJSAMObject<TextMap>(Fixtures.ReadAllText(textMapFileName));
        }

        private static SpaceSemanticsResolver Resolver()
        {
            return new SpaceSemanticsResolver(TextMap());
        }

        private static Space Space(string name)
        {
            return new Space(name, new Point3D(0, 0, 1.5));
        }

        private static SpaceSemantics Resolve(string name)
        {
            return Resolver().Resolve(Space(name));
        }
    }
}
