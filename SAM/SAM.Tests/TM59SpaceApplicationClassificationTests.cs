// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Tests.Helpers;
using System.Collections.Generic;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// Fixes two related TM59 InternalCondition space-application defects: the Flat1 report's
    /// <c>Kitchen_4</c> / <c>Kitchen_7</c> rows showing "Sleeping, Cooking" rather than "Cooking" alone, and
    /// a naturally ventilated apartment "Living Room" reading as Sleeping - which would wrongly subject it
    /// to the bedroom night-time Criterion 2.
    /// <para>
    /// <b>Defect 1 - the apartment bedroom-count qualifier.</b> <c>TM59InternalConditionResolver</c> names a
    /// multi-bedroom apartment's kitchen/living-room condition <c>"{bedroomCount} Bed Apt. ..."</c> (e.g.
    /// <c>"1 Bed Apt. Kitchen"</c>) - the apartment's bedroom COUNT is part of the condition's own name, and
    /// "Bed" is independently a literal "Sleeping" keyword. <b>Fixed</b> by
    /// <c>TM59Manager.RoleMatchName</c>, which strips a leading <c>"N Bed Apt."</c> qualifier before an
    /// InternalCondition's name is matched, applied only to the InternalCondition-based overloads.
    /// </para>
    /// <para>
    /// <b>Defect 2 - the "room"/"bedroom" substring collision.</b> Once the apartment prefix is stripped, a
    /// bare <c>"Living Room"</c> condition still read as Sleeping, because the old matching primitive,
    /// <c>TextMap.GetSortedKeys</c>, does <c>value.Contains(token) || token.Contains(value)</c> - so the
    /// token "room" matched the "Sleeping" alias "bedroom" as an accidental substring, even though "room" is
    /// not "bedroom". <b>Fixed</b> by routing InternalCondition role matching
    /// (<c>TM59Manager.IsRole</c>) through <c>Query.TM59TextMapMatches</c> instead - the same deterministic,
    /// whole-token/whole-phrase matcher <c>TM59InternalConditionResolver</c> already uses for Space
    /// classification, reused rather than special-cased. It requires an alias's tokens to appear as a
    /// contiguous, exact-equality sequence in the name, so "room" can never match the alias "bedroom".
    /// </para>
    /// <para>
    /// <b>Scope.</b> Both fixes apply only to InternalCondition-based classification
    /// (<c>IsSleeping</c>/<c>IsLiving</c>/<c>IsCooking</c>/<c>TM59SpaceApplications</c> taking an
    /// <c>InternalCondition</c>, and therefore every caller of them - including SAM_Tas's
    /// <c>RoomUse.cs</c>/<c>ToSAP.cs</c>/<c>OverheatingCalculator.cs</c>). Space-name classification
    /// (<c>IsSleeping(Space, TextMap)</c> and its Living/Cooking siblings) is untouched.
    /// </para>
    /// </summary>
    public class TM59SpaceApplicationClassificationTests
    {
        [Theory]
        [InlineData("1 Bed Apt. Kitchen")]
        [InlineData("2 Bed Apt. Kitchen")]
        [InlineData("3 Bed Apt. Kitchen")]
        public void ApartmentKitchenCondition_ClassifiesAsCookingOnly_NotSleeping(string internalConditionName)
        {
            InternalCondition internalCondition = new(internalConditionName);

            List<TM59SpaceApplication> tM59SpaceApplications = TM59Manager.TM59SpaceApplications(internalCondition, TM59TestData.TextMap);

            Assert.Contains(TM59SpaceApplication.Cooking, tM59SpaceApplications);
            Assert.DoesNotContain(TM59SpaceApplication.Sleeping, tM59SpaceApplications);
            Assert.DoesNotContain(TM59SpaceApplication.Living, tM59SpaceApplications);
        }

        /// <summary>
        /// The defect this class is named for: a bare apartment Living Room (no Kitchen) must classify as
        /// Living alone. Before the whole-token fix, "room" (its own token here) matched the "Sleeping"
        /// alias "bedroom" as a substring, so this space was wrongly routed as a bedroom and subjected to
        /// Criterion 2 - see <c>TM59AssessmentCalculatorTests.ApartmentLivingRoomCondition_IsNotRoutedAsABedroom_SoCriterion2IsNotApplicable</c>
        /// for the end-to-end consequence.
        /// </summary>
        [Theory]
        [InlineData("1 Bed Apt. Living Room")]
        [InlineData("2 Bed Apt. Living Room")]
        [InlineData("3 Bed Apt. Living Room")]
        public void ApartmentLivingRoomCondition_ClassifiesAsLivingOnly_NotSleeping(string internalConditionName)
        {
            InternalCondition internalCondition = new(internalConditionName);

            List<TM59SpaceApplication> tM59SpaceApplications = TM59Manager.TM59SpaceApplications(internalCondition, TM59TestData.TextMap);

            Assert.Contains(TM59SpaceApplication.Living, tM59SpaceApplications);
            Assert.DoesNotContain(TM59SpaceApplication.Sleeping, tM59SpaceApplications);
            Assert.DoesNotContain(TM59SpaceApplication.Cooking, tM59SpaceApplications);
        }

        [Theory]
        [InlineData("1 Bed Apt. Living Room/Kitchen")]
        [InlineData("2 Bed Apt. Living Room/Kitchen")]
        [InlineData("3 Bed Apt. Living Room/Kitchen")]
        public void ApartmentLivingRoomKitchenCondition_ClassifiesAsLivingAndCooking_NotSleeping(string internalConditionName)
        {
            InternalCondition internalCondition = new(internalConditionName);

            List<TM59SpaceApplication> tM59SpaceApplications = TM59Manager.TM59SpaceApplications(internalCondition, TM59TestData.TextMap);

            Assert.Contains(TM59SpaceApplication.Living, tM59SpaceApplications);
            Assert.Contains(TM59SpaceApplication.Cooking, tM59SpaceApplications);
            Assert.DoesNotContain(TM59SpaceApplication.Sleeping, tM59SpaceApplications);
        }

        /// <summary>
        /// A single-word condition name with no apartment-size qualifier - "Double Bedroom", "Single
        /// Bedroom", "Studio" - is unaffected by either fix: the prefix strip only ever matches
        /// "N Bed Apt. ...", and "bedroom"/"double"/"single" are genuine whole-token matches on their own,
        /// not substring artifacts.
        /// </summary>
        [Theory]
        [InlineData("Double Bedroom")]
        [InlineData("Single Bedroom")]
        public void BedroomCondition_ClassifiesAsSleepingOnly(string internalConditionName)
        {
            InternalCondition internalCondition = new(internalConditionName);

            List<TM59SpaceApplication> tM59SpaceApplications = TM59Manager.TM59SpaceApplications(internalCondition, TM59TestData.TextMap);

            Assert.Contains(TM59SpaceApplication.Sleeping, tM59SpaceApplications);
            Assert.DoesNotContain(TM59SpaceApplication.Cooking, tM59SpaceApplications);
            Assert.DoesNotContain(TM59SpaceApplication.Living, tM59SpaceApplications);
        }

        /// <summary>"Studio" classifies as all three roles, exactly as before either fix - "studio" is a genuine, single-token alias shared by all three keyword lists.</summary>
        [Fact]
        public void StudioCondition_ClassifiesAsAllThreeRoles_Unchanged()
        {
            InternalCondition internalCondition = new("Studio");

            List<TM59SpaceApplication> tM59SpaceApplications = TM59Manager.TM59SpaceApplications(internalCondition, TM59TestData.TextMap);

            Assert.Contains(TM59SpaceApplication.Sleeping, tM59SpaceApplications);
            Assert.Contains(TM59SpaceApplication.Living, tM59SpaceApplications);
            Assert.Contains(TM59SpaceApplication.Cooking, tM59SpaceApplications);
        }

        /// <summary>
        /// The fix is scoped to InternalCondition classification only. A Space literally named
        /// "1 Bed Apt. Kitchen" (not a realistic room name, but the boundary the fix must respect) still
        /// reads as Sleeping through the unmodified <c>IsSleeping(Space, TextMap)</c> path - proving neither
        /// the apartment-prefix strip nor the whole-token matcher was applied to Space-name matching, which
        /// callers elsewhere rely on exactly as it was.
        /// </summary>
        [Fact]
        public void SpaceNameClassification_StillReadsTheRawBedToken_NeitherFixTouchesIt()
        {
            Space space = new("1 Bed Apt. Kitchen");

            Assert.True(TM59Manager.IsSleeping(space, TM59TestData.TextMap));
            Assert.True(TM59Manager.IsCooking(space, TM59TestData.TextMap));
        }
    }
}
