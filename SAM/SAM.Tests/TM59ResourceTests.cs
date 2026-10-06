// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Core;
using SAM.Tests.Helpers;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// Data-integrity tests for the TM59 resource files: deserialization, TextMap/Library
    /// consistency, and the SAM_Tas compatibility lock on the 6 legacy TextMap rows.
    /// </summary>
    public class TM59ResourceTests
    {
        // The 6 rows that predate this change and are consumed by SAM_Tas (RoomUse, Convert.ToTM59,
        // OverheatingCalculator, Tas.SAP). Must never change - appending new rows is safe, editing
        // these is not.
        private static readonly Dictionary<string, string[]> LegacyRows = new Dictionary<string, string[]>
        {
            ["Living"] = new[] { "studio", "sitting", "lounge", "living", "lvg", "lvng", "liv", "dining" },
            ["Sleeping"] = new[] { "bed", "twin", "double", "dbl", "bedroom", "sleep", "studio" },
            ["Cooking"] = new[] { "kitchen", "k'chn", "kit", "kitchenette", "cafe", "ktch", "studio" },
            ["1"] = new[] { "twin", "twin bed", "single", "single bed", "sgl", "sgl bed", "1-bed", "1 bed" },
            ["2"] = new[] { "king", "queen", "dbl", "dbl bed", "double", "double bed", "2 bed", "2-bed", "studio" },
            ["3"] = new[] { "3-bed", "3 bed" },
        };

        private static readonly string[] NewNonHabitableConditionNames = new[]
        {
            "TM59_Bathroom",
            "TM59_Internal Corridor",
            "TM59_Communal Corridor (including pipework gains)",
            "TM59_Stairs",
            "TM59_Cupboard/riser/lift/void",
            "TM59_Cupboard with HIU",
            "TM59_Riser Communal pipework",
        };

        [Fact]
        public void TextMap_Deserializes_And_RoundTrips()
        {
            string json = Fixtures.ReadAllText("SAM_InternalConditionTextMap_TM59.JSON");
            TextMap textMap = RoundTrip.FromJson<TextMap>(json);

            Assert.NotNull(textMap);
            Assert.NotNull(textMap.Keys);
        }

        [Fact]
        public void InternalConditionLibrary_Deserializes_And_RoundTrips()
        {
            string json = Fixtures.ReadAllText("SAM_InternalConditionLibrary_TM59.JSON");
            InternalConditionLibrary library = RoundTrip.FromJson<InternalConditionLibrary>(json);

            Assert.NotNull(library);
        }

        [Fact]
        public void ProfileLibrary_Deserializes_And_RoundTrips()
        {
            string json = Fixtures.ReadAllText("SAM_ProfileLibrary_TM59.JSON");
            ProfileLibrary library = RoundTrip.FromJson<ProfileLibrary>(json);

            Assert.NotNull(library);
        }

        [Fact]
        public void Library_Has_No_Duplicate_Condition_Names()
        {
            List<InternalCondition> internalConditions = TM59TestData.InternalConditionLibrary.GetInternalConditions();
            Assert.NotNull(internalConditions);

            List<string> names = internalConditions!.Select(x => x.Name).ToList();
            Assert.Equal(names.Distinct().Count(), names.Count);
        }

        [Fact]
        public void Library_Contains_All_Nineteen_Expected_Condition_Names()
        {
            List<string> expected = new List<string>
            {
                "Studio",
                "1 Bed Apt. Living Room/Kitchen", "1 Bed Apt. Living Room", "1 Bed Apt. Kitchen",
                "2 Bed Apt. Living Room/Kitchen", "2 Bed Apt. Living Room", "2 Bed Apt. Kitchen",
                "3 Bed Apt. Living Room/Kitchen", "3 Bed Apt. Living Room", "3 Bed Apt. Kitchen",
                "Double Bedroom", "Single Bedroom",
            };
            expected.AddRange(NewNonHabitableConditionNames);

            List<string> actual = TM59TestData.InternalConditionLibrary.GetInternalConditions()!.Select(x => x.Name).ToList();

            foreach (string name in expected)
                Assert.Contains(name, actual);

            Assert.Equal(19, actual.Count);
        }

        [Fact]
        public void Every_NonRole_TextMap_Key_Resolves_To_Exactly_One_Library_Condition()
        {
            HashSet<string> roleOrCountKeys = new HashSet<string> { "Living", "Sleeping", "Cooking", "1", "2", "3" };

            foreach (string key in TM59TestData.TextMap.Keys!)
            {
                if (roleOrCountKeys.Contains(key))
                    continue;

                List<InternalCondition>? matches = TM59TestData.InternalConditionLibrary.GetInternalConditions(key);
                Assert.True(matches != null && matches.Count == 1, $"TextMap key '{key}' should resolve to exactly one InternalCondition (found {matches?.Count ?? 0}).");
            }
        }

        [Fact]
        public void Every_Profile_Referenced_By_Every_TM59_Condition_Exists_In_TM59_ProfileLibrary()
        {
            List<string> profileParameterNames = new List<string>
            {
                "Occupancy Profile Name", "Equipment Sensible Profile Name", "Equipment Latent Profile Name",
                "Lighting Profile Name", "Infiltration Profile Name", "Heating Profile Name", "Cooling Profile Name",
                "Humidification Profile Name", "Dehumidification Profile Name", "Pollutant Profile Name",
            };

            List<string> profileNames = TM59TestData.ProfileLibrary.GetObjects()!.Select(x => x.Name).ToList();

            foreach (InternalCondition internalCondition in TM59TestData.InternalConditionLibrary.GetInternalConditions()!)
            {
                foreach (string parameterName in profileParameterNames)
                {
                    if (!internalCondition.TryGetValue(parameterName, out string? profileName) || string.IsNullOrWhiteSpace(profileName))
                        continue;

                    Assert.True(profileNames.Contains(profileName),
                        $"Condition '{internalCondition.Name}' references profile '{profileName}' ({parameterName}) which is missing from SAM_ProfileLibrary_TM59.JSON.");
                }
            }
        }

        [Fact]
        public void No_New_Keyword_Is_Duplicated_Across_Two_New_Condition_Rows()
        {
            HashSet<string> legacyKeys = new HashSet<string>(LegacyRows.Keys);

            Dictionary<string, string> aliasToKey = new Dictionary<string, string>();
            foreach (string key in TM59TestData.TextMap.Keys!)
            {
                if (legacyKeys.Contains(key))
                    continue;

                foreach (string alias in TM59TestData.TextMap.GetValues(key)!)
                {
                    if (aliasToKey.TryGetValue(alias, out string? existingKey))
                        Assert.Fail($"Keyword '{alias}' is duplicated under both '{existingKey}' and '{key}'.");

                    aliasToKey[alias] = key;
                }
            }
        }

        [Fact]
        public void Legacy_SixRows_Are_Unchanged()
        {
            foreach (KeyValuePair<string, string[]> row in LegacyRows)
            {
                List<string>? values = TM59TestData.TextMap.GetValues(row.Key);
                Assert.NotNull(values);
                Assert.Equal(row.Value.OrderBy(x => x), values!.OrderBy(x => x));
            }
        }

        [Fact]
        public void New_NonHabitable_Condition_Names_Do_Not_Read_As_Habitable_Roles()
        {
            // Locks the SAM_Tas contract: RoomUse.cs relies on TM59SpaceApplications, which reads the
            // condition Name against the Living/Sleeping/Cooking rows. The new names must never match.
            foreach (string name in NewNonHabitableConditionNames)
            {
                Assert.False(TM59Manager.IsSleeping(name, TM59TestData.TextMap), $"'{name}' unexpectedly reads as Sleeping.");
                Assert.False(TM59Manager.IsLiving(name, TM59TestData.TextMap), $"'{name}' unexpectedly reads as Living.");
                Assert.False(TM59Manager.IsCooking(name, TM59TestData.TextMap), $"'{name}' unexpectedly reads as Cooking.");
            }
        }
    }
}
