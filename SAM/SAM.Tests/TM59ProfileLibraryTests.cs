// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// The six TM59-specific "flat schedule" profiles (TM59_No Occupancy/Lighting/Gains,
    /// TM59_CommCorridor_lighting/heatgain, TM59_HIU_pipework_heatgain) were semantic duplicates of
    /// SAM's canonical Constant/OFF Gain profiles - same [[0,23,1.0]]/[[0,23,0.0]] all-day shape, just
    /// under TM59-local names and GUIDs. This locks the replacement: Constant/OFF are now shipped
    /// INSIDE the TM59 profile bundle itself (same Guid/Category/Values as the main library's, so a
    /// model that already has one is never given a duplicate), every TM59 InternalCondition profile
    /// reference still resolves, and the TM59 bundle is self-sufficient without the main
    /// SAM_ProfileLibrary ever having been injected - which matters because
    /// MapInternalConditionsByTM59 (SAM_UI) injects only DefaultProfileLibrary_TM59().
    /// </summary>
    public class TM59ProfileLibraryTests
    {
        [Fact]
        public void Every_TM59_InternalCondition_Profile_Reference_Resolves_From_The_TM59_Bundle_Alone()
        {
            ProfileLibrary profileLibrary = TM59TestData.ProfileLibrary;

            foreach (InternalCondition internalCondition in TM59TestData.InternalConditionLibrary.GetInternalConditions())
            {
                Dictionary<ProfileType, string> profileNames = internalCondition.GetProfileTypeDictionary();
                Dictionary<ProfileType, Profile> resolved = internalCondition.GetProfileDictionary(profileLibrary, true);

                Assert.Equal(profileNames.Count, resolved.Count);

                foreach (KeyValuePair<ProfileType, string> keyValuePair in profileNames)
                {
                    Assert.True(resolved.ContainsKey(keyValuePair.Key),
                        $"'{internalCondition.Name}' references '{keyValuePair.Value}' ({keyValuePair.Key}) which did not resolve from the TM59 profile bundle alone.");
                }
            }
        }

        [Fact]
        public void Constant_Resolves_For_Lighting_And_EquipmentSensible_Via_ProfileGroup_Gain()
        {
            ProfileLibrary profileLibrary = TM59TestData.ProfileLibrary;

            Profile lighting = profileLibrary.GetProfile("Constant", ProfileType.Lighting, true);
            Assert.NotNull(lighting);
            Assert.Equal(ProfileGroup.Gain, lighting.ProfileGroup);

            Profile equipmentSensible = profileLibrary.GetProfile("Constant", ProfileType.EquipmentSensible, true);
            Assert.NotNull(equipmentSensible);
            Assert.Equal(ProfileGroup.Gain, equipmentSensible.ProfileGroup);
        }

        [Fact]
        public void OFF_Resolves_For_Occupancy_Lighting_And_EquipmentSensible()
        {
            ProfileLibrary profileLibrary = TM59TestData.ProfileLibrary;

            Assert.NotNull(profileLibrary.GetProfile("OFF", ProfileType.Occupancy, true));
            Assert.NotNull(profileLibrary.GetProfile("OFF", ProfileType.Lighting, true));
            Assert.NotNull(profileLibrary.GetProfile("OFF", ProfileType.EquipmentSensible, true));
        }

        [Fact]
        public void SAM_Tas_Style_Resolution_Finds_Constant_For_Communal_Corridor_And_HIU_Conditions()
        {
            // Mirrors exactly the call SAM_Tas's own UpdateProfile/UpdateThermostatProfile helpers make:
            // InternalCondition.GetProfile(ProfileType, ProfileLibrary) with its default
            // includeProfileGroup:true - see SAM_Tas's Modify\UpdateZone.cs.
            ProfileLibrary profileLibrary = TM59TestData.ProfileLibrary;

            InternalCondition corridor = TM59TestData.InternalConditionLibrary
                .GetInternalConditions("TM59_Communal Corridor (including pipework gains)").First();

            Profile corridorLighting = corridor.GetProfile(ProfileType.Lighting, profileLibrary);
            Assert.Equal("Constant", corridorLighting?.Name);

            Profile corridorEquipment = corridor.GetProfile(ProfileType.EquipmentSensible, profileLibrary);
            Assert.Equal("Constant", corridorEquipment?.Name);

            InternalCondition hiu = TM59TestData.InternalConditionLibrary.GetInternalConditions("TM59_Cupboard with HIU").First();
            Profile hiuEquipment = hiu.GetProfile(ProfileType.EquipmentSensible, profileLibrary);
            Assert.Equal("Constant", hiuEquipment?.Name);

            // The unique continuous 16C heating profile (kept, renamed from TM59_HTG_1to24_16 to the
            // canonical unprefixed HTG_1to24_16 naming convention already used by e.g. HTG_1to24_21).
            InternalCondition bathroom = TM59TestData.InternalConditionLibrary.GetInternalConditions("TM59_Bathroom").First();
            Profile heating = bathroom.GetProfile(ProfileType.Heating, profileLibrary);
            Assert.Equal("HTG_1to24_16", heating?.Name);
        }

        [Fact]
        public void Model_Already_Containing_Constant_Does_Not_Receive_A_Duplicate_From_The_TM59_Bundle()
        {
            ProfileLibrary mainProfileLibrary = SAM.Core.Create.IJSAMObject<ProfileLibrary>(Fixtures.ReadAllText("SAM_ProfileLibrary.JSON"));
            Profile mainConstant = mainProfileLibrary.GetProfile("Constant", ProfileGroup.Gain);
            Assert.NotNull(mainConstant);

            AnalyticalModel analyticalModel = new AnalyticalModel(Guid.NewGuid(), "Test Model");
            Assert.True(analyticalModel.AddProfile(mainConstant, false));

            int countAfterFirstAdd = analyticalModel.ProfileLibrary.GetProfiles().Count;

            Profile tm59Constant = TM59TestData.ProfileLibrary.GetProfile("Constant", ProfileGroup.Gain);
            Assert.NotNull(tm59Constant);

            // Same Category::Name (and Guid) as the one already in the model - AddProfile(override:false)
            // must not add a second copy.
            Assert.False(analyticalModel.AddProfile(tm59Constant, false));
            Assert.Equal(countAfterFirstAdd, analyticalModel.ProfileLibrary.GetProfiles().Count);
        }

        [Fact]
        public void TM59_Bundle_Works_In_A_Fresh_Model_Without_The_Default_Profile_Library_Injected()
        {
            // Mirrors MapInternalConditionsByTM59.cs (SAM_UI): only Query.DefaultProfileLibrary_TM59()'s
            // profiles are ever added to the model - the main SAM_ProfileLibrary is never injected there.
            AnalyticalModel analyticalModel = new AnalyticalModel(Guid.NewGuid(), "Fresh TM59-Only Model");
            foreach (Profile profile in TM59TestData.ProfileLibrary.GetProfiles())
            {
                analyticalModel.AddProfile(profile, false);
            }

            InternalCondition corridor = TM59TestData.InternalConditionLibrary
                .GetInternalConditions("TM59_Communal Corridor (including pipework gains)").First();

            Profile resolved = corridor.GetProfile(ProfileType.Lighting, analyticalModel.ProfileLibrary);
            Assert.Equal("Constant", resolved?.Name);
        }
    }
}
