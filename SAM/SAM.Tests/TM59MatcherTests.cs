// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Tests.Helpers;
using System;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// Deterministic keyword matching: normalization, whole-token/phrase matching, and
    /// specific-beats-generic ranking, exercised through TM59InternalConditionResolver.Classify
    /// and Resolve against the real shipped TM59 TextMap/Library.
    /// </summary>
    public class TM59MatcherTests
    {
        private static Space NewSpace(string name) => new Space(Guid.NewGuid(), name, null);

        [Theory]
        [InlineData("Kitchen4", "kitchen")]
        [InlineData("Bedroom 23", "bedroom")]
        [InlineData("Ensuite5", "ensuite")]
        [InlineData("En-suite 5", "en suite")]
        [InlineData("Corridor1", "corridor")]
        [InlineData("Corridor", "corridor")]
        public void TM59NormalizedName_Strips_Trailing_Room_Numbers_And_Collapses_Punctuation(string input, string expected)
        {
            Assert.Equal(expected, Query.TM59NormalizedName(input));
        }

        [Theory]
        [InlineData("Corridor1", TM59SpaceClassification.NonHabitable)]
        [InlineData("Bathroom2", TM59SpaceClassification.NonHabitable)]
        [InlineData("Ensuite5", TM59SpaceClassification.NonHabitable)]
        [InlineData("En-suite 5", TM59SpaceClassification.NonHabitable)]
        [InlineData("WC 3", TM59SpaceClassification.NonHabitable)]
        [InlineData("Studio 10", TM59SpaceClassification.Studio)]
        [InlineData("Bedroom 3", TM59SpaceClassification.Bedroom)]
        [InlineData("Bedroom 4", TM59SpaceClassification.Bedroom)]
        [InlineData("Bedroom 13", TM59SpaceClassification.Bedroom)]
        [InlineData("Bedroom 23", TM59SpaceClassification.Bedroom)]
        [InlineData("Bedroom Space", TM59SpaceClassification.Bedroom)]
        [InlineData("Master Bedroom", TM59SpaceClassification.Bedroom)]
        [InlineData("Internal Corridor 2", TM59SpaceClassification.NonHabitable)]
        [InlineData("Corridor 2", TM59SpaceClassification.NonHabitable)]
        [InlineData("Communal Riser 1", TM59SpaceClassification.NonHabitable)]
        [InlineData("Riser 1", TM59SpaceClassification.NonHabitable)]
        [InlineData("Plant Room 3", TM59SpaceClassification.Undefined)]
        public void Classify_Matches_Expected_Classification(string name, TM59SpaceClassification expected)
        {
            TM59InternalConditionResolver resolver = TM59TestData.NewResolver();
            Assert.Equal(expected, resolver.Classify(NewSpace(name)));
        }

        [Fact]
        public void InternalCorridor_And_Corridor_Resolve_To_Different_NonHabitable_Conditions()
        {
            TM59InternalConditionResolver resolver = TM59TestData.NewResolver();

            TM59InternalConditionResult internalCorridor = resolver.Resolve(NewSpace("Internal Corridor 2"), null);
            TM59InternalConditionResult corridor = resolver.Resolve(NewSpace("Corridor 2"), null);

            Assert.Equal("TM59_Internal Corridor", internalCorridor.InternalCondition?.Name);
            Assert.Equal("TM59_Communal Corridor (including pipework gains)", corridor.InternalCondition?.Name);
        }

        [Fact]
        public void CommunalRiser_And_Riser_Resolve_To_Different_NonHabitable_Conditions()
        {
            TM59InternalConditionResolver resolver = TM59TestData.NewResolver();

            TM59InternalConditionResult communalRiser = resolver.Resolve(NewSpace("Communal Riser 1"), null);
            TM59InternalConditionResult riser = resolver.Resolve(NewSpace("Riser 1"), null);

            Assert.Equal("TM59_Riser Communal pipework", communalRiser.InternalCondition?.Name);
            Assert.Equal("TM59_Cupboard/riser/lift/void", riser.InternalCondition?.Name);
        }

        [Fact]
        public void Bathroom_And_Ensuite_Resolve_To_The_Bathroom_Condition()
        {
            TM59InternalConditionResolver resolver = TM59TestData.NewResolver();

            foreach (string name in new[] { "Bathroom2", "Ensuite5", "En-suite 5" })
            {
                TM59InternalConditionResult result = resolver.Resolve(NewSpace(name), null);
                Assert.Equal("TM59_Bathroom", result.InternalCondition?.Name);
            }
        }

        [Fact]
        public void Corridor1_Resolves_To_Communal_Corridor()
        {
            TM59InternalConditionResolver resolver = TM59TestData.NewResolver();
            TM59InternalConditionResult result = resolver.Resolve(NewSpace("Corridor1"), null);

            Assert.Equal("TM59_Communal Corridor (including pipework gains)", result.InternalCondition?.Name);
        }

        [Fact]
        public void Unknown_Room_Type_Returns_Manual_Review_Without_Throwing()
        {
            TM59InternalConditionResolver resolver = TM59TestData.NewResolver();
            TM59InternalConditionResult result = resolver.Resolve(NewSpace("Plant Room 3"), null);

            Assert.Null(result.InternalCondition);
            Assert.False(string.IsNullOrWhiteSpace(result.Diagnostic));
        }

        [Fact]
        public void Generic_Space_Keyword_Never_Beats_A_Specific_Bedroom_Match()
        {
            // "space" is deliberately not a keyword for TM59_Cupboard/riser/lift/void, precisely so a
            // name like "Bedroom Space" is not misclassified as an unconditioned void.
            TM59InternalConditionResolver resolver = TM59TestData.NewResolver();
            Assert.Equal(TM59SpaceClassification.Bedroom, resolver.Classify(NewSpace("Bedroom Space")));
        }

        // --- Keyword precedence: a generic single-token non-habitable term (cupboard/store) must not
        // override a habitable role match of equal or greater specificity. ---

        [Fact]
        public void Living_Room_Cupboard_Remains_Habitable_Living_Room()
        {
            TM59InternalConditionResolver resolver = TM59TestData.NewResolver();
            Assert.Equal(TM59SpaceClassification.LivingRoom, resolver.Classify(NewSpace("Living Room Cupboard")));
        }

        [Fact]
        public void Kitchen_Store_Remains_Kitchen()
        {
            TM59InternalConditionResolver resolver = TM59TestData.NewResolver();
            Assert.Equal(TM59SpaceClassification.Kitchen, resolver.Classify(NewSpace("Kitchen Store")));
        }

        [Fact]
        public void Bedroom_Cupboard_Remains_Bedroom()
        {
            // Same principle as Living Room Cupboard / Kitchen Store, for the Bedroom role specifically.
            TM59InternalConditionResolver resolver = TM59TestData.NewResolver();
            Assert.Equal(TM59SpaceClassification.Bedroom, resolver.Classify(NewSpace("Bedroom Cupboard")));
        }

        [Fact]
        public void HIU_Cupboard_Maps_To_The_HIU_Condition()
        {
            // "HIU Cupboard" and "Cupboard HIU" are the same two words in the opposite order - the
            // matcher must recognise both, not just the order the alias happens to be written in.
            TM59InternalConditionResolver resolver = TM59TestData.NewResolver();

            TM59InternalConditionResult hiuCupboard = resolver.Resolve(NewSpace("HIU Cupboard"), null);
            TM59InternalConditionResult cupboardHiu = resolver.Resolve(NewSpace("Cupboard HIU"), null);

            Assert.Equal("TM59_Cupboard with HIU", hiuCupboard.InternalCondition?.Name);
            Assert.Equal("TM59_Cupboard with HIU", cupboardHiu.InternalCondition?.Name);
        }

        [Fact]
        public void Communal_Riser_Maps_To_Riser_Communal_Pipework()
        {
            TM59InternalConditionResolver resolver = TM59TestData.NewResolver();
            TM59InternalConditionResult result = resolver.Resolve(NewSpace("Communal Riser"), null);
            Assert.Equal("TM59_Riser Communal pipework", result.InternalCondition?.Name);
        }

        // --- Circulation semantics: internal vs communal vs stairs, and the deliberately-ambiguous case. ---

        [Fact]
        public void Stair_Landing_Maps_To_Stairs_Not_Bathroom_Internal_Corridors()
        {
            TM59InternalConditionResolver resolver = TM59TestData.NewResolver();
            TM59InternalConditionResult result = resolver.Resolve(NewSpace("Stair Landing"), null);
            Assert.Equal("TM59_Stairs", result.InternalCondition?.Name);
        }

        [Theory]
        [InlineData("Lift Lobby")]
        [InlineData("Communal Lobby")]
        [InlineData("Common Lobby")]
        public void Communal_Lobby_Variants_Map_To_Communal_Corridor(string name)
        {
            TM59InternalConditionResolver resolver = TM59TestData.NewResolver();
            TM59InternalConditionResult result = resolver.Resolve(NewSpace(name), null);
            Assert.Equal("TM59_Communal Corridor (including pipework gains)", result.InternalCondition?.Name);
        }

        [Fact]
        public void Internal_Landing_Maps_To_Internal_Corridor()
        {
            TM59InternalConditionResolver resolver = TM59TestData.NewResolver();
            TM59InternalConditionResult result = resolver.Resolve(NewSpace("Internal Landing"), null);
            Assert.Equal("TM59_Internal Corridor", result.InternalCondition?.Name);
        }

        [Fact]
        public void Stair_Lobby_Is_Manual_Review_Not_Resolved_By_Character_Count()
        {
            // "stair" (TM59_Stairs) and "lobby" (TM59_Communal Corridor) are both a single, equally
            // specific token - a genuine conflict the matcher must not silently guess at.
            TM59InternalConditionResolver resolver = TM59TestData.NewResolver();
            TM59InternalConditionResult result = resolver.Resolve(NewSpace("Stair Lobby"), null);

            Assert.Null(result.InternalCondition);
            Assert.False(string.IsNullOrWhiteSpace(result.Diagnostic));
        }

        [Fact]
        public void Plant_Room_Remains_Unresolved_Manual_Review()
        {
            // "plant"/"plant room" is deliberately not a keyword anywhere - plant rooms can have very
            // different gains and must not be silently folded into the generic unconditioned condition.
            TM59InternalConditionResolver resolver = TM59TestData.NewResolver();
            TM59InternalConditionResult result = resolver.Resolve(NewSpace("Plant Room"), null);

            Assert.Null(result.InternalCondition);
            Assert.Equal(TM59SpaceClassification.Undefined, result.Classification);
        }

        // --- Explicit "twin" bedroom-size keyword (requirement 4). ---

        // --- A bare bedroom-size modifier (single/double/twin/master/...) must not beat a genuine
        // non-habitable noun of equal or greater specificity - review finding on the PR that
        // introduced the 3-tier precedence above. ---

        [Fact]
        public void Master_Bathroom_Resolves_As_Bathroom_Not_Bedroom()
        {
            TM59InternalConditionResolver resolver = TM59TestData.NewResolver();
            TM59InternalConditionResult result = resolver.Resolve(NewSpace("Master Bathroom"), null);
            Assert.Equal("TM59_Bathroom", result.InternalCondition?.Name);
        }

        [Fact]
        public void Double_Bathroom_Resolves_As_Bathroom_Not_Bedroom()
        {
            // "double" is simultaneously a bedroom-size keyword AND a legacy Sleeping-role alias (kept
            // for SAM_Tas's own IsSleeping compatibility) - that overlap must not let it win a tie
            // against the distinct, genuine "bathroom" noun via either path.
            TM59InternalConditionResolver resolver = TM59TestData.NewResolver();
            TM59InternalConditionResult result = resolver.Resolve(NewSpace("Double Bathroom"), null);
            Assert.Equal("TM59_Bathroom", result.InternalCondition?.Name);
        }

        [Fact]
        public void Twin_Ensuite_Resolves_As_Bathroom_Not_Bedroom()
        {
            // Same overlap as Double Bathroom: "twin" is also a legacy Sleeping alias.
            TM59InternalConditionResolver resolver = TM59TestData.NewResolver();
            TM59InternalConditionResult result = resolver.Resolve(NewSpace("Twin Ensuite"), null);
            Assert.Equal("TM59_Bathroom", result.InternalCondition?.Name);
        }

        [Fact]
        public void Double_Kitchen_Resolves_As_Kitchen_Not_Bedroom()
        {
            // "double" is simultaneously a bedroom-size keyword AND a legacy Sleeping-role alias - it
            // must not beat the genuine Cooking noun "kitchen" either directly (tier 1) or by falling
            // through to tier 3's Sleeping-first priority cascade.
            TM59InternalConditionResolver resolver = TM59TestData.NewResolver();
            Assert.Equal(TM59SpaceClassification.Kitchen, resolver.Classify(NewSpace("Double Kitchen")));
        }

        [Fact]
        public void Master_Living_Room_Resolves_As_LivingRoom_Not_Bedroom()
        {
            // "master" is a bedroom-size keyword but NOT a legacy Sleeping-role alias on its own, so
            // this exercises the tier-1-vs-Living comparison directly rather than the spent-alias path.
            TM59InternalConditionResolver resolver = TM59TestData.NewResolver();
            Assert.Equal(TM59SpaceClassification.LivingRoom, resolver.Classify(NewSpace("Master Living Room")));
        }

        [Fact]
        public void Master_Bedroom_Still_Resolves_As_Bedroom_Not_Bathroom()
        {
            // Regression guard: the fix above must not affect names with no competing non-habitable
            // noun at all - "master bedroom" is a 2-token bedroom-size phrase with nothing to lose to.
            TM59InternalConditionResolver resolver = TM59TestData.NewResolver();
            Space space = NewSpace("Master Bedroom");
            TM59InternalConditionResult result = resolver.Resolve(space, new System.Collections.Generic.List<Space> { space });
            Assert.Equal("Double Bedroom", result.InternalCondition?.Name);
        }

        [Theory]
        [InlineData("Twin Bedroom")]
        [InlineData("Twin Bed")]
        [InlineData("Twin")]
        public void Twin_Names_Resolve_As_Explicit_Single_Bedroom_Even_As_The_Sole_Bedroom(string name)
        {
            // The TM59 "sole bedroom in the flat is always Double" convention only applies when there
            // is no explicit size keyword - an explicitly-named Twin/Single bedroom must override it
            // even when it is the only bedroom in the flat. "Twin" bare is also simultaneously a
            // legacy Sleeping-role alias at the same (1-token) specificity as the Single Bedroom
            // keyword - the explicit bedroom-size reading must win that tie too.
            TM59InternalConditionResolver resolver = TM59TestData.NewResolver();
            Space space = NewSpace(name);

            TM59InternalConditionResult result = resolver.Resolve(space, new System.Collections.Generic.List<Space> { space });

            Assert.Equal("Single Bedroom", result.InternalCondition?.Name);
            Assert.Equal(1, result.Occupancy);
        }
    }
}
