// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Tests.Helpers;
using System;
using System.Collections.Generic;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// The corrected Studio rule: a lone space in a single-space zone is Studio only via an explicit
    /// match (all three TM59 roles) or the narrowly-scoped zero-bedroom combined-space rule - never
    /// merely because it is the only space in the zone (the old, removed behaviour).
    /// </summary>
    public class TM59ClassificationTests
    {
        private static Space NewSpace(string name) => new Space(Guid.NewGuid(), name, null);

        [Fact]
        public void Lone_Bedroom_Only_Space_Classifies_As_Bedroom_Not_Studio()
        {
            TM59InternalConditionResolver resolver = TM59TestData.NewResolver();
            Space space = NewSpace("Bedroom 1");

            TM59InternalConditionResult result = resolver.Resolve(space, new List<Space> { space });

            Assert.Equal(TM59SpaceClassification.Bedroom, result.Classification);
            Assert.Equal("Double Bedroom", result.InternalCondition?.Name);
        }

        [Fact]
        public void Lone_LivingRoom_Only_Space_Is_Manual_Review_Not_Studio()
        {
            TM59InternalConditionResolver resolver = TM59TestData.NewResolver();
            Space space = NewSpace("Living Room");

            TM59InternalConditionResult result = resolver.Resolve(space, new List<Space> { space });

            Assert.Null(result.InternalCondition);
            Assert.False(string.IsNullOrWhiteSpace(result.Diagnostic));
        }

        [Fact]
        public void Lone_Space_Matching_All_Three_Roles_Is_Studio_Explicit_Match()
        {
            TM59InternalConditionResolver resolver = TM59TestData.NewResolver();
            Space space = NewSpace("Studio 10");

            TM59InternalConditionResult result = resolver.Resolve(space, new List<Space> { space });

            Assert.Equal(TM59SpaceClassification.Studio, result.Classification);
            Assert.Equal("Studio", result.InternalCondition?.Name);
        }

        [Fact]
        public void Lone_CombinedLivingKitchen_Space_With_No_Other_Bedroom_Is_Studio_ZeroBedroom_Rule()
        {
            TM59InternalConditionResolver resolver = TM59TestData.NewResolver();
            Space space = NewSpace("Kitchen Living Room");

            TM59InternalConditionResult result = resolver.Resolve(space, new List<Space> { space });

            Assert.Equal(TM59SpaceClassification.Studio, result.Classification);
            Assert.Equal("Studio", result.InternalCondition?.Name);
        }

        [Fact]
        public void CombinedLivingKitchen_Space_Alongside_A_Separate_Bedroom_Is_Not_Studio()
        {
            TM59InternalConditionResolver resolver = TM59TestData.NewResolver();
            Space livingKitchen = NewSpace("Kitchen Living Room");
            Space bedroom = NewSpace("Bedroom 1");
            List<Space> flat = new List<Space> { livingKitchen, bedroom };

            TM59InternalConditionResult result = resolver.Resolve(livingKitchen, flat);

            Assert.Equal(TM59SpaceClassification.LivingRoomKitchen, result.Classification);
            Assert.Equal("1 Bed Apt. Living Room/Kitchen", result.InternalCondition?.Name);
        }

        [Fact]
        public void CombinedLivingKitchen_Space_Alongside_An_Unrecognized_Room_Is_Not_Auto_Promoted_To_Studio()
        {
            // "Study" matches no TM59 keyword at all, so it classifies Undefined rather than Bedroom -
            // but an unrecognized name might genuinely be a bedroom the matcher failed to recognize, so
            // this must NOT be silently treated as "no other habitable space, therefore Studio". Only a
            // POSITIVELY confirmed non-habitable other space (bathroom, corridor, ...) should still
            // allow the zero-bedroom Studio shortcut - see CombinedLivingKitchen_Space_Alongside_A_
            // ConfirmedNonHabitable_Space_Is_Still_Studio for that case.
            TM59InternalConditionResolver resolver = TM59TestData.NewResolver();
            Space livingKitchen = NewSpace("Kitchen Living Room");
            Space study = NewSpace("Study");
            List<Space> flat = new List<Space> { livingKitchen, study };

            TM59InternalConditionResult result = resolver.Resolve(livingKitchen, flat);

            Assert.Equal(TM59SpaceClassification.LivingRoomKitchen, result.Classification);
            Assert.Null(result.InternalCondition);
            Assert.Contains("no identifiable bedroom", result.Diagnostic);
        }

        [Fact]
        public void CombinedLivingKitchen_Space_Alongside_A_ConfirmedNonHabitable_Space_Is_Still_Studio()
        {
            TM59InternalConditionResolver resolver = TM59TestData.NewResolver();
            Space livingKitchen = NewSpace("Kitchen Living Room");
            Space bathroom = NewSpace("Bathroom");
            List<Space> flat = new List<Space> { livingKitchen, bathroom };

            TM59InternalConditionResult result = resolver.Resolve(livingKitchen, flat);

            Assert.Equal(TM59SpaceClassification.Studio, result.Classification);
            Assert.Equal("Studio", result.InternalCondition?.Name);
        }
    }
}
