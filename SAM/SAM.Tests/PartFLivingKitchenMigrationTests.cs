// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Analytical.Enums;
using SAM.Geometry.Spatial;
using SAM.Tests.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// Migration regression tests for the deliberate change of LivingKitchen from an EXTRACT role to a
    /// SUPPLY role in the Part F workflow, and for the backward compatibility of the surrounding data
    /// model.
    /// </summary>
    /// <remarks>
    /// MIGRATION NOTE: LivingKitchen spaces are now treated as habitable supply spaces rather than extract
    /// spaces in the Part F workflow. Existing workflows that relied on LivingKitchen as an extract
    /// terminal should be reviewed.
    /// <para>
    /// The change follows Approved Document F, Volume 1 (2021 edition) Appendix A (page 36): a habitable
    /// room is one used for dwelling purposes but not <i>solely</i> a kitchen, so an open-plan
    /// living-kitchen is habitable and takes mechanical supply under paragraph 1.67. The cooking function
    /// it still contains is carried by IsCookingSpace and reported through the local kitchen extract
    /// warning, not by giving the space an extract terminal.
    /// </para>
    /// </remarks>
    public class PartFLivingKitchenMigrationTests
    {
        private const string dataFileName = "SAM_PartFSpaceRulesUKDwellingsMVHR.json";

        private const double tolerance = 1e-6;

        // ------------------------------------------------------------------
        // 1. Legacy Part F JSON without SpaceUse still loads
        // ------------------------------------------------------------------

        /// <summary>
        /// A rule set written before the shared vocabulary existed carries no SpaceUse on its categories.
        /// It must still load, and its categories must still classify rooms by their own synonyms.
        /// </summary>
        [Fact]
        public void LegacyRuleSetWithoutSpaceUse_StillLoadsAndClassifies()
        {
            PartFData partFData = LegacyData();

            Assert.NotEmpty(partFData.PartFCategories);

            //Every legacy category has no SpaceUse at all.
            Assert.All(partFData.PartFCategories.Values, x => Assert.Equal(SpaceUse.Undefined, x.SpaceUse));

            //And each is still reachable by its own synonym, through the deterministic matcher.
            PartFCategory? partFCategory = partFData.GetPartFCategory("Legacy Sleeping");
            Assert.NotNull(partFCategory);
            Assert.Equal("LegacySleeping", partFCategory!.Name);
            Assert.Equal(PartFVentilationType.supply, partFCategory.PartFVentilationType);
        }

        /// <summary>A legacy rule set still produces a complete, balanced calculation.</summary>
        [Fact]
        public void LegacyRuleSetWithoutSpaceUse_StillCalculates()
        {
            PartFCalculator partFCalculator = Calculate(LegacyData(),
                ("Legacy Sleeping", 20, 50),
                ("Legacy Extract", 6, 15));

            PartFDwellingResult dwellingResult = Assert.Single(partFCalculator.DwellingResults);

            Assert.True(dwellingResult.ContinuousDesignSystemRate_Lps > 0);
            Assert.Equal(dwellingResult.TotalSupply_Lps, dwellingResult.TotalExtract_Lps, tolerance);
            Assert.Equal(dwellingResult.TotalSetbackSupply_Lps, dwellingResult.TotalSetbackExtract_Lps, tolerance);
        }

        /// <summary>
        /// A legacy rule set that also omits the setback factor and the one-habitable-room rate falls back
        /// to the documented defaults rather than to zero.
        /// </summary>
        [Fact]
        public void LegacyRuleSetWithoutNewKeys_UsesTheDocumentedDefaults()
        {
            PartFData partFData = LegacyData();

            Assert.Equal(PartFData.DefaultSetbackFlowRateFactor, partFData.SetbackFlowRateFactor, tolerance);
            Assert.Equal(PartFData.DefaultOneHabitableRoomRate_Lps, partFData.OneHabitableRoomRate_Lps, tolerance);
        }

        /// <summary>
        /// A rule set written by the interim build, which used BackgroundFlowRateFactor, still applies its
        /// value - the key is accepted for backward-compatible deserialization only.
        /// </summary>
        [Fact]
        public void RuleSetWithLegacyBackgroundFactorKey_IsStillHonoured()
        {
            string path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "SAM_PartF_LegacyBackgroundKey.json");

            System.IO.File.WriteAllText(path, """
            {
              "WholeDwellingRates_Lps": { "1": 19, "2": 25, "BackgroundFlowRateFactor": 0.45 },
              "Categories": []
            }
            """);

            try
            {
                Assert.Equal(0.45, Analytical.Create.PartFData(path).SetbackFlowRateFactor, tolerance);
            }
            finally
            {
                System.IO.File.Delete(path);
            }
        }

        // ------------------------------------------------------------------
        // 2 and 3. Existing serialized PartFSpaceData and CalculatedFlowRate_Lps
        // ------------------------------------------------------------------

        /// <summary>
        /// A PartFSpaceData serialised by an earlier SAM build carries only CalculatedFlowRate_Lps, and
        /// that value has always been the sizing rate. It must read back as the continuous design rate.
        /// </summary>
        [Fact]
        public void LegacySerialisedPartFSpaceData_IsStillReadable()
        {
            JsonObject jsonObject = new()
            {
                ["_type"] = "SAM.Analytical.PartFSpaceData,SAM.Analytical",
                ["Name"] = "LivingKitchen",
                ["CalculatedFlowRate_Lps"] = 27.5,
                ["MinFlowRate_Lps"] = 13.0,
                ["IsTerminalSpace"] = true,
                ["IsBedroom"] = false,
                ["IsCookingSpace"] = true,
                ["IncludeInFloorAreaCheck"] = true,
                ["ScaleSupplyWithVolume"] = false,
                ["ScaleExtractAboveMinimum"] = true,
                ["PartFType"] = "WetRoom",
                ["PartFVentilationType"] = "extract",
            };

            PartFSpaceData partFSpaceData = new();
            Assert.True(partFSpaceData.FromJsonObject(jsonObject));

            //Every legacy field survives, including the OLD extract role - deserialising an old model must
            //reproduce what that model said, not silently re-role it.
            Assert.Equal("LivingKitchen", partFSpaceData.Name);
            Assert.Equal(27.5, partFSpaceData.ContinuousDesignFlowRate_Lps!.Value, tolerance);
            Assert.Equal(27.5, partFSpaceData.CalculatedFlowRate_Lps!.Value, tolerance);
            Assert.Equal(13, partFSpaceData.MinFlowRate_Lps!.Value, tolerance);
            Assert.Equal(PartFVentilationType.extract, partFSpaceData.PartFVentilationType);
            Assert.Equal(PartFType.WetRoom, partFSpaceData.PartFType);
            Assert.True(partFSpaceData.IsCookingSpace);

            //A legacy model has no setback value; it must be null, not zero or NaN.
            Assert.Null(partFSpaceData.SetbackFlowRate_Lps);
        }

        /// <summary>
        /// CalculatedFlowRate_Lps keeps its meaning as the continuous design flow rate. This is the
        /// property SAM_Tas_Grasshopper and SAM_Systems read, so its meaning must not drift.
        /// </summary>
        [Fact]
        public void CalculatedFlowRate_StillMeansContinuousDesignFlow()
        {
            PartFCalculator partFCalculator = Calculate(DataFile(),
                ("Living Kitchen", 30, 75),
                ("Bedroom 1", 14, 35),
                ("Bathroom", 6, 15));

            foreach (Space space in partFCalculator.AdjacencyCluster.GetSpaces())
            {
                PartFSpaceData? partFSpaceData = space.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData);
                if (partFSpaceData is null)
                {
                    continue;
                }

                Assert.Equal(partFSpaceData.ContinuousDesignFlowRate_Lps, partFSpaceData.CalculatedFlowRate_Lps);
                Assert.NotEqual(partFSpaceData.SetbackFlowRate_Lps, partFSpaceData.CalculatedFlowRate_Lps);
            }
        }

        /// <summary>
        /// A model written now is still readable by an earlier SAM build: both the new and the legacy key
        /// are written, and the legacy key holds the continuous design rate.
        /// </summary>
        [Fact]
        public void NewlySerialisedPartFSpaceData_IsReadableByAnOlderBuild()
        {
            PartFSpaceData partFSpaceData = new()
            {
                ContinuousDesignFlowRate_Lps = 41,
                SetbackFlowRate_Lps = 12.3,
            };

            JsonObject jsonObject = partFSpaceData.ToJsonObject();

            Assert.Equal(41, jsonObject["ContinuousDesignFlowRate_Lps"]!.GetValue<double>(), tolerance);
            Assert.Equal(41, jsonObject["CalculatedFlowRate_Lps"]!.GetValue<double>(), tolerance);
            Assert.Equal(12.3, jsonObject["SetbackFlowRate_Lps"]!.GetValue<double>(), tolerance);
        }

        // ------------------------------------------------------------------
        // 4. LivingKitchen resolves as habitable, cooking, supply, not extract
        // ------------------------------------------------------------------

        /// <summary>The shared semantic classification of an open-plan living-kitchen.</summary>
        [Fact]
        public void LivingKitchen_SharedSemantics_AreHabitableCookingSupplyOnly()
        {
            SpaceSemantics spaceSemantics = SpaceUse.LivingRoomKitchen.SpaceSemantics();

            Assert.True(spaceSemantics.IsHabitable);
            Assert.True(spaceSemantics.IsCookingSpace);
            Assert.True(spaceSemantics.HasSupplyRole);
            Assert.False(spaceSemantics.HasExtractRole);
            Assert.False(spaceSemantics.IsWetRoom);
            Assert.False(spaceSemantics.IsBedroomEquivalent);
        }

        /// <summary>The shipped Part F rule set agrees: habitable, cooking, supply, no extract minimum.</summary>
        [Fact]
        public void LivingKitchen_PartFCategory_IsHabitableSupply()
        {
            PartFCategory? partFCategory = DataFile().GetPartFCategory("Living Kitchen");

            Assert.NotNull(partFCategory);
            Assert.Equal(SpaceUse.LivingRoomKitchen, partFCategory!.SpaceUse);
            Assert.Equal(PartFType.Habitable, partFCategory.PartFType);
            Assert.Equal(PartFVentilationType.supply, partFCategory.PartFVentilationType);
            Assert.True(partFCategory.IsCookingSpace);
            Assert.False(partFCategory.IsBedroom);
            Assert.True(partFCategory.MinFlowRate_Lps is null || partFCategory.MinFlowRate_Lps.Value == 0);
        }

        /// <summary>
        /// A living-kitchen does NOT count as a bedroom. Only a studio, which also combines sleeping, is
        /// bedroom-equivalent.
        /// </summary>
        [Fact]
        public void LivingKitchen_DoesNotCountAsABedroom()
        {
            PartFDwellingResult dwellingResult = Dwelling(Calculate(DataFile(),
                ("Living Kitchen", 30, 75),
                ("Bedroom 1", 14, 35),
                ("Bathroom", 6, 15)));

            //Two habitable rooms (living-kitchen + bedroom), one bedroom.
            Assert.Equal(2, dwellingResult.HabitableRoomCount);
            Assert.Equal(1, dwellingResult.BedroomCount);
        }

        // ------------------------------------------------------------------
        // 5. Downstream supply/extract grouping
        // ------------------------------------------------------------------

        /// <summary>
        /// The grouping downstream consumers key off - PartFSpaceData.PartFVentilationType - reports
        /// LivingKitchen as supply. SAM_Systems' UpdateAirSystem and SystemEnergyCentre read exactly this
        /// property to group spaces onto supply and extract systems.
        /// </summary>
        [Fact]
        public void DownstreamGrouping_TreatsLivingKitchenAsSupply()
        {
            PartFCalculator partFCalculator = Calculate(DataFile(),
                ("Living Kitchen", 30, 75),
                ("Bedroom 1", 14, 35),
                ("Bathroom", 6, 15),
                ("Ensuite", 4, 10),
                ("WC", 2, 5),
                ("Utility Room", 5, 12),
                ("Kitchen", 0.0001, 0.0001));

            Dictionary<string, PartFVentilationType> grouping = partFCalculator.AdjacencyCluster.GetSpaces()
                .Where(x => x.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData) is not null)
                .ToDictionary(x => x.Name, x => x.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData)!.PartFVentilationType);

            //Supply side.
            Assert.Equal(PartFVentilationType.supply, grouping["Living Kitchen"]);
            Assert.Equal(PartFVentilationType.supply, grouping["Bedroom 1"]);

            //Extract side - every configured wet room stays extract.
            Assert.Equal(PartFVentilationType.extract, grouping["Bathroom"]);
            Assert.Equal(PartFVentilationType.extract, grouping["Ensuite"]);
            Assert.Equal(PartFVentilationType.extract, grouping["WC"]);
            Assert.Equal(PartFVentilationType.extract, grouping["Utility Room"]);
            Assert.Equal(PartFVentilationType.extract, grouping["Kitchen"]);
        }

        /// <summary>
        /// The living-kitchen's PRIMARY terminal is still supply, and the scalar rate a downstream system
        /// build reads is still that supply rate, unchanged in meaning. Its local kitchen extract is a
        /// second terminal on the same space, visible through the terminal collection.
        /// </summary>
        [Fact]
        public void LivingKitchen_CarriesASupplyRateAsItsPrimaryTerminal()
        {
            PartFCalculator partFCalculator = Calculate(DataFile(),
                ("Living Kitchen", 30, 75),
                ("Bedroom 1", 14, 35),
                ("Bathroom", 6, 15));

            PartFSpaceData partFSpaceData = partFCalculator.AdjacencyCluster.GetSpaces()
                .Find(x => x.Name == "Living Kitchen")!
                .GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData)!;

            //Unchanged for a consumer written before terminal-level sizing.
            Assert.Equal(PartFVentilationType.supply, partFSpaceData.PartFVentilationType);
            Assert.True(partFSpaceData.ContinuousDesignFlowRate_Lps > 0);
            Assert.True(partFSpaceData.IsTerminalSpace);
            Assert.Equal(PartFTerminalRole.Supply, partFSpaceData.PrimaryTerminal()!.TerminalRole);
            Assert.Equal(partFSpaceData.ContinuousSupplyFlowRate_Lps, partFSpaceData.ContinuousDesignFlowRate_Lps);

            //And the local kitchen extract paragraph 1.17a requires of the same room is now represented,
            //where before it could not be. It is reachable through the terminal collection and through the
            //aggregate, but deliberately not through the legacy scalar above.
            Assert.Equal(2, partFSpaceData.Terminals.Count);
            Assert.True(partFSpaceData.LocalKitchenExtractFlowRate_Lps > 0);

            //Extract is now shared between the living kitchen's local extract and the bathroom.
            PartFDwellingResult dwellingResult = Assert.Single(partFCalculator.DwellingResults);

            double rate_Bathroom = partFCalculator.AdjacencyCluster.GetSpaces()
                .Find(x => x.Name == "Bathroom")!
                .GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData)!.ContinuousDesignFlowRate_Lps!.Value;

            Assert.Equal(dwellingResult.TotalExtract_Lps, rate_Bathroom + partFSpaceData.LocalKitchenExtractFlowRate_Lps!.Value, tolerance);
        }

        // ------------------------------------------------------------------
        // 6. Wet rooms remain extract
        // ------------------------------------------------------------------

        /// <summary>Every configured wet room keeps its extract role and its Table 1.2 minimum.</summary>
        [Theory]
        [InlineData("Bathroom", 8)]
        [InlineData("Ensuite", 8)]
        [InlineData("Utility Room", 8)]
        [InlineData("WC", 6)]
        [InlineData("Kitchen", 13)]
        public void WetRoom_RemainsExtractWithItsMinimum(string name, double expected_Lps)
        {
            PartFCategory? partFCategory = DataFile().GetPartFCategory(name);

            Assert.NotNull(partFCategory);
            Assert.Equal(PartFVentilationType.extract, partFCategory!.PartFVentilationType);
            Assert.Equal(PartFType.WetRoom, partFCategory.PartFType);
            Assert.True(partFCategory.IsTerminalSpace);
            Assert.Equal(expected_Lps, partFCategory.MinFlowRate_Lps!.Value, tolerance);
        }

        // ------------------------------------------------------------------
        // 7. The migration note is shipped with the rule set
        // ------------------------------------------------------------------

        /// <summary>
        /// The rule set carries a migration note, so the behaviour change is discoverable from the data
        /// file itself and not only from the release notes.
        /// </summary>
        [Fact]
        public void RuleSet_CarriesTheLivingKitchenMigrationNote()
        {
            string json = Fixtures.ReadAllText(dataFileName);

            JsonObject? jsonObject = JsonNode.Parse(json) as JsonObject;
            Assert.NotNull(jsonObject);

            JsonArray? migrationNotes = jsonObject!["MigrationNotes"] as JsonArray;
            Assert.NotNull(migrationNotes);

            List<string> notes = [.. migrationNotes!.Select(x => x?.GetValue<string>() ?? string.Empty)];

            Assert.Contains(notes, x =>
                x.Contains("LivingKitchen") &&
                x.Contains("habitable supply spaces") &&
                x.Contains("should be reviewed"));
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private static PartFData DataFile()
        {
            return Analytical.Create.PartFData(Fixtures.GetPath(dataFileName));
        }

        /// <summary>
        /// A rule set in the pre-shared-vocabulary shape: categories with synonyms but no SpaceUse, and no
        /// setback or one-habitable-room keys.
        /// </summary>
        private static PartFData LegacyData()
        {
            return new PartFData
            {
                WholeDwellingRates_Lps = new Dictionary<int, double> { { 1, 19 }, { 2, 25 } },
                PartFCategories = new Dictionary<string, PartFCategory>
                {
                    ["LegacySleeping"] = new PartFCategory("LegacySleeping", PartFType.Habitable, PartFVentilationType.supply,
                        true, null, true, true, true, false, "RoomVolume", ["legacy sleeping"]),
                    ["LegacyExtract"] = new PartFCategory("LegacyExtract", PartFType.WetRoom, PartFVentilationType.extract,
                        false, 8, true, true, false, true, "RoomVolume", ["legacy extract"]),
                },
            };
        }

        private static PartFDwellingResult Dwelling(PartFCalculator partFCalculator)
        {
            return Assert.Single(partFCalculator.DwellingResults);
        }

        private static PartFCalculator Calculate(PartFData partFData, params (string Name, double Area_M2, double Volume_M3)[] spaces)
        {
            AdjacencyCluster adjacencyCluster = new();

            for (int i = 0; i < spaces.Length; i++)
            {
                Space space = new(spaces[i].Name, new Point3D(i * 10, 0, 1.5));
                space.SetValue(SpaceParameter.Area, spaces[i].Area_M2);
                space.SetValue(SpaceParameter.Volume, spaces[i].Volume_M3);
                adjacencyCluster.AddObject(space);
            }

            PartFCalculator result = new(partFData) { AdjacencyCluster = adjacencyCluster };

            Assert.True(result.Calculate());

            return result;
        }
    }
}
