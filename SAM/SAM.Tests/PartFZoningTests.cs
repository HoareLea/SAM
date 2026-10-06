// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Geometry.Spatial;
using SAM.Tests.Helpers;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// Dwelling grouping tests for <see cref="PartFCalculator"/>, covering the single house workflow
    /// (no zones) and the multi-flat workflow (one zone per flat) against Approved Document F,
    /// Volume 1: Dwellings, 2021 edition, for use in England.
    /// </summary>
    /// <remarks>
    /// The fixtures mirror the zoning structure of the example model SAM_zoningAM.sam: a zone
    /// category named "Flats" holding zones "Flat 1", "Corridor", "Flat 2" and "Flat 3", with the
    /// same space names, areas and volumes. The .sam file itself is not used as a test fixture -
    /// it lives outside the repository, is a 128 KB binary, and would make the suite depend on a
    /// path that does not exist on a build agent. Rebuilding the same structure in memory keeps the
    /// tests self-contained while exercising the real arrangement, including the shared corridor
    /// that sits inside the "Flats" category.
    /// </remarks>
    public class PartFZoningTests
    {
        private const string dataFileName = "SAM_PartFSpaceRulesUKDwellingsMVHR.json";

        private const string zoneCategoryName = "Flats";

        private const double tolerance = 1e-6;

        // ------------------------------------------------------------------
        // Single house - no zones, empty zone category
        // ------------------------------------------------------------------

        /// <summary>
        /// A single house needs no zones. Leaving the zone category empty sizes the whole model as
        /// one dwelling and is not itself a problem, so it must not raise a warning.
        /// </summary>
        [Fact]
        public void SingleHouse_EmptyZoneCategory_SizesWholeModelAsOneDwellingWithoutWarning()
        {
            PartFCalculator partFCalculator = House();

            Assert.True(partFCalculator.Calculate((string)null));

            PartFDwellingResult dwellingResult = Assert.Single(partFCalculator.DwellingResults);

            Assert.Null(dwellingResult.Name);
            Assert.Empty(partFCalculator.Warnings);
            Assert.Equal(8, dwellingResult.SpaceNames.Count);
        }

        /// <summary>Blank and whitespace zone category names behave the same as an empty one.</summary>
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void SingleHouse_BlankZoneCategory_IsTreatedAsSingleDwelling(string zoneCategoryName_Blank)
        {
            PartFCalculator partFCalculator = House();

            Assert.True(partFCalculator.Calculate(zoneCategoryName_Blank));

            Assert.Single(partFCalculator.DwellingResults);
            Assert.Empty(partFCalculator.Warnings);
        }

        /// <summary>
        /// ADF F Vol 1 (2021) Table 1.3 and paragraph 1.24 (page 10) applied once across the whole
        /// house: 3 bedrooms (31 l/s) against 118 m2 (35.4 l/s), so the area based rate governs.
        /// </summary>
        [Fact]
        public void SingleHouse_BedroomCountFloorAreaAndRate_AreCalculatedOnceForTheWholeHouse()
        {
            PartFCalculator partFCalculator = House();
            Assert.True(partFCalculator.Calculate((string)null));

            PartFDwellingResult dwellingResult = Assert.Single(partFCalculator.DwellingResults);

            Assert.Equal(3, dwellingResult.BedroomCount);
            Assert.Equal(118, dwellingResult.InternalFloorArea_M2, tolerance);
            Assert.Equal(31, dwellingResult.BedroomBasedRate_Lps, tolerance);
            Assert.Equal(35.4, dwellingResult.AreaBasedRate_Lps, tolerance);
            Assert.Equal(35.4, dwellingResult.WholeDwellingRate_Lps, tolerance);
            Assert.Equal(35.4, dwellingResult.FinalSystemRate_Lps, tolerance);
        }

        /// <summary>
        /// ADF F Vol 1 (2021) paragraphs 1.67 and 1.69 (page 16): across the single house, supply
        /// goes to the habitable rooms and extract to the wet rooms, and both totals balance.
        /// </summary>
        [Fact]
        public void SingleHouse_RoomRates_AreAssignedAndBalanced()
        {
            PartFCalculator partFCalculator = House();
            Assert.True(partFCalculator.Calculate((string)null));

            Dictionary<string, double> rates = Rates(partFCalculator);
            PartFDwellingResult dwellingResult = partFCalculator.DwellingResults[0];

            Assert.Equal(dwellingResult.FinalSystemRate_Lps, dwellingResult.TotalSupply_Lps, tolerance);
            Assert.Equal(dwellingResult.FinalSystemRate_Lps, dwellingResult.TotalExtract_Lps, tolerance);

            Assert.True(rates["Kitchen"] >= 13);
            Assert.True(rates["Bathroom"] >= 8);
            Assert.True(rates["WC"] >= 6);
            Assert.Equal(0, rates["Hall"], tolerance);
            Assert.True(rates["Bedroom 1"] > 0);
        }

        // ------------------------------------------------------------------
        // Multiple flats - one zone per flat, matching SAM_zoningAM.sam
        // ------------------------------------------------------------------

        /// <summary>
        /// Each zone of the selected category is one dwelling. The example model holds three flats
        /// plus a shared corridor zone that sits in the same category.
        /// </summary>
        [Fact]
        public void Flats_EachZoneInTheCategory_IsSizedAsASeparateDwelling()
        {
            PartFCalculator partFCalculator = Flats();
            Assert.True(partFCalculator.Calculate(zoneCategoryName));

            List<string> names = partFCalculator.DwellingResults.ConvertAll(x => x.Name);
            names.Sort();

            Assert.Equal(new List<string> { "Corridor", "Flat 1", "Flat 2", "Flat 3" }, names);
        }

        /// <summary>
        /// ADF F Vol 1 (2021) paragraph 1.24 and Table 1.3 (page 10) applied per flat. Flat 2 has one
        /// bedroom (19 l/s) and 210 m2 (63 l/s), so the area based rate governs at 63 l/s.
        /// </summary>
        [Fact]
        public void Flats_BedroomCountAndFloorArea_AreCalculatedIndependentlyPerFlat()
        {
            PartFCalculator partFCalculator = Flats();
            Assert.True(partFCalculator.Calculate(zoneCategoryName));

            PartFDwellingResult flat2 = Dwelling(partFCalculator, "Flat 2");

            Assert.Equal(1, flat2.BedroomCount);
            Assert.Equal(210, flat2.InternalFloorArea_M2, tolerance);

            //Only the bedroom is habitable here - the kitchen and the ensuite are not - so Table 1.3
            //note 1 sets the room based rate at 13 l/s. The floor area rate of 63 l/s governs regardless.
            Assert.Equal(1, flat2.HabitableRoomCount);
            Assert.True(flat2.OneHabitableRoomRuleApplied);
            Assert.Equal(13, flat2.BedroomOrHabitableRate_Lps, tolerance);

            //BedroomBasedRate_Lps is the plain Table 1.3 one-bedroom figure (19 l/s), NOT the note 1
            //rate it happens to have been overridden by. The two must read differently here, or a
            //regression collapsing them back into one number (as BedroomOrHabitableRate_Lps did before
            //this fix) would report this dwelling's bedroom-table rate as 13, not the real 19.
            Assert.Equal(19, flat2.BedroomBasedRate_Lps, tolerance);
            Assert.NotEqual(flat2.BedroomBasedRate_Lps, flat2.BedroomOrHabitableRate_Lps);

            Assert.Equal(63, flat2.AreaBasedRate_Lps, tolerance);
            Assert.Equal(63, flat2.FinalSystemRate_Lps, tolerance);
        }

        /// <summary>
        /// ADF F Vol 1 (2021) Table 1.2, paragraph 1.67 and paragraph 1.69: within one flat, supply
        /// goes to the bedroom and extract is shared between the kitchen and ensuite above their
        /// minimums, both totals matching the flat's own system rate.
        /// </summary>
        [Fact]
        public void Flats_SupplyAndExtract_AreBalancedWithinEachFlat()
        {
            PartFCalculator partFCalculator = Flats();
            Assert.True(partFCalculator.Calculate(zoneCategoryName));

            Dictionary<string, double> rates = Rates(partFCalculator);
            PartFDwellingResult flat2 = Dwelling(partFCalculator, "Flat 2");

            Assert.Equal(63, rates["Bedroom 2_3"], tolerance);

            //Minimum-first, cooking-priority: kitchen 13 + ensuite 8 = 21 l/s of Table 1.2 minimums, and
            //the remaining 42 l/s all goes to the kitchen, which holds the cooking function. The previous
            //volume-weighted split gave the kitchen 43 and the ensuite 20.
            Assert.Equal(55, rates["Kitchen_4"], tolerance);   // 13 + 42
            Assert.Equal(8, rates["Ensuite_5"], tolerance);    // its Table 1.2 minimum

            Assert.Equal(63, flat2.TotalSupply_Lps, tolerance);
            Assert.Equal(63, flat2.TotalExtract_Lps, tolerance);
        }

        /// <summary>
        /// Flats with different room mixes must not influence one another. Enlarging Flat 3 leaves
        /// every Flat 2 figure untouched.
        /// </summary>
        [Fact]
        public void Flats_ChangingOneFlat_DoesNotAffectAnother()
        {
            PartFCalculator partFCalculator_Base = Flats();
            Assert.True(partFCalculator_Base.Calculate(zoneCategoryName));
            PartFDwellingResult flat2_Base = Dwelling(partFCalculator_Base, "Flat 2");

            //Give Flat 3 a second bedroom and far more floor area.
            AdjacencyCluster adjacencyCluster = FlatsCluster();
            Zone zone_Flat3 = adjacencyCluster.GetZones().Find(x => x.Name == "Flat 3");
            Space space = new("Bedroom 4_9", new Point3D(90, 0, 1.5));
            space.SetValue(SpaceParameter.Area, 400.0);
            space.SetValue(SpaceParameter.Volume, 1600.0);
            adjacencyCluster.AddObject(space);
            adjacencyCluster.AddRelation(zone_Flat3, space);

            PartFCalculator partFCalculator = new(DataFile()) { AdjacencyCluster = adjacencyCluster };
            Assert.True(partFCalculator.Calculate(zoneCategoryName));

            PartFDwellingResult flat2 = Dwelling(partFCalculator, "Flat 2");
            PartFDwellingResult flat3 = Dwelling(partFCalculator, "Flat 3");

            Assert.Equal(flat2_Base.BedroomCount, flat2.BedroomCount);
            Assert.Equal(flat2_Base.InternalFloorArea_M2, flat2.InternalFloorArea_M2, tolerance);
            Assert.Equal(flat2_Base.FinalSystemRate_Lps, flat2.FinalSystemRate_Lps, tolerance);

            Assert.Equal(2, flat3.BedroomCount);
            Assert.Equal(610, flat3.InternalFloorArea_M2, tolerance);
            Assert.NotEqual(flat2.FinalSystemRate_Lps, flat3.FinalSystemRate_Lps);
        }

        /// <summary>
        /// Spaces outside every selected dwelling zone are reported and left alone. Here the shared
        /// stair is in no zone at all.
        /// </summary>
        [Fact]
        public void Flats_SpacesOutsideEveryDwellingZone_AreReportedAndLeftUnsized()
        {
            AdjacencyCluster adjacencyCluster = FlatsCluster();
            Space space = new("Landlord Store", new Point3D(100, 0, 1.5));
            space.SetValue(SpaceParameter.Area, 12.0);
            space.SetValue(SpaceParameter.Volume, 30.0);
            adjacencyCluster.AddObject(space);

            PartFCalculator partFCalculator = new(DataFile()) { AdjacencyCluster = adjacencyCluster };
            Assert.True(partFCalculator.Calculate(zoneCategoryName));

            Assert.Contains(partFCalculator.Warnings, x => x.Contains("do not belong to any dwelling zone") && x.Contains("Landlord Store"));

            Space space_Result = partFCalculator.AdjacencyCluster.GetSpaces().Find(x => x.Name == "Landlord Store");
            Assert.Null(space_Result.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData));
        }

        /// <summary>A space put into two dwelling zones is sized twice and must be reported.</summary>
        [Fact]
        public void Flats_SpaceInTwoDwellingZones_IsReported()
        {
            AdjacencyCluster adjacencyCluster = FlatsCluster();
            Zone zone_Flat3 = adjacencyCluster.GetZones().Find(x => x.Name == "Flat 3");
            Space space = adjacencyCluster.GetSpaces().Find(x => x.Name == "Kitchen_4");
            adjacencyCluster.AddRelation(zone_Flat3, space);

            PartFCalculator partFCalculator = new(DataFile()) { AdjacencyCluster = adjacencyCluster };
            Assert.True(partFCalculator.Calculate(zoneCategoryName));

            Assert.Contains(partFCalculator.Warnings, x => x.Contains("Kitchen_4") && x.Contains("more than one dwelling zone"));
        }

        /// <summary>A dwelling zone holding no spaces is reported and skipped.</summary>
        [Fact]
        public void Flats_DwellingZoneWithNoSpaces_IsReportedAndSkipped()
        {
            AdjacencyCluster adjacencyCluster = FlatsCluster();
            Zone zone = new("Flat 4");
            zone.SetValue(ZoneParameter.ZoneCategory, zoneCategoryName);
            adjacencyCluster.AddObject(zone);

            PartFCalculator partFCalculator = new(DataFile()) { AdjacencyCluster = adjacencyCluster };
            Assert.True(partFCalculator.Calculate(zoneCategoryName));

            Assert.Contains(partFCalculator.Warnings, x => x.Contains("Flat 4") && x.Contains("contains no spaces"));
            Assert.DoesNotContain(partFCalculator.DwellingResults, x => x.Name == "Flat 4");
        }

        /// <summary>An unknown zone category is reported, and the categories present are listed.</summary>
        [Fact]
        public void Flats_UnknownZoneCategory_IsReportedWithTheCategoriesPresent()
        {
            PartFCalculator partFCalculator = Flats();
            Assert.True(partFCalculator.Calculate("Apartments"));

            string warning = Assert.Single(partFCalculator.Warnings);
            Assert.Contains("No zone belongs to zone category 'Apartments'", warning);
            Assert.Contains("Flats", warning);
            Assert.Empty(partFCalculator.DwellingResults);
        }

        /// <summary>Zone category matching is case sensitive, and the message says so.</summary>
        [Fact]
        public void Flats_ZoneCategoryWithWrongCase_IsReportedAsACaseMismatch()
        {
            PartFCalculator partFCalculator = Flats();
            Assert.True(partFCalculator.Calculate("flats"));

            string warning = Assert.Single(partFCalculator.Warnings);
            Assert.Contains("case sensitive", warning);
            Assert.Contains("'Flats'", warning);
        }

        /// <summary>A zone category supplied against a model with no zones at all is reported.</summary>
        [Fact]
        public void Flats_ModelWithoutZones_IsReported()
        {
            PartFCalculator partFCalculator = House();
            Assert.True(partFCalculator.Calculate(zoneCategoryName));

            Assert.Contains(partFCalculator.Warnings, x => x.Contains("contains no zones"));
            Assert.Empty(partFCalculator.DwellingResults);
        }

        /// <summary>
        /// The shared corridor of the example model sits inside the "Flats" category, so it is
        /// sized as a dwelling. It has no bedroom, no habitable room and no wet room, and all three
        /// must be reported rather than passing silently.
        /// </summary>
        [Fact]
        public void Flats_SharedCorridorZoneInsideTheCategory_IsReportedAsHavingNoDwellingRooms()
        {
            PartFCalculator partFCalculator = Flats();
            Assert.True(partFCalculator.Calculate(zoneCategoryName));

            PartFDwellingResult corridor = Dwelling(partFCalculator, "Corridor");

            Assert.Contains(corridor.Warnings, x => x.Contains("no space was classified as a bedroom", System.StringComparison.OrdinalIgnoreCase));
            Assert.Contains(corridor.Warnings, x => x.Contains("habitable room"));
            Assert.Contains(corridor.Warnings, x => x.Contains("extract terminal"));
            Assert.Equal(0, corridor.TotalSupply_Lps, tolerance);
            Assert.Equal(0, corridor.TotalExtract_Lps, tolerance);
        }

        /// <summary>
        /// The example model names Flat 1's main room "Studio 1_0". A studio combines sleeping,
        /// living and cooking, so it counts as one bedroom (ADF F Vol 1 Table 1.3, page 10) and
        /// takes supply as a habitable room (paragraph 1.67, page 16).
        /// </summary>
        [Fact]
        public void Flats_StudioFlat_CountsAsOneBedroomAndTakesSupply()
        {
            PartFCalculator partFCalculator = Flats();
            Assert.True(partFCalculator.Calculate(zoneCategoryName));

            PartFDwellingResult flat1 = Dwelling(partFCalculator, "Flat 1");
            Dictionary<string, double> rates = Rates(partFCalculator);

            Assert.Empty(flat1.UnclassifiedSpaceNames);
            Assert.Equal(1, flat1.BedroomCount);
            Assert.Equal(100, flat1.InternalFloorArea_M2, tolerance);   // 75 studio + 25 bathroom
            Assert.Equal(30, flat1.FinalSystemRate_Lps, tolerance);     // 0.3 x 100 beats 19 l/s

            //The scalar rate is the PRIMARY terminal's, unchanged in meaning: supply for the studio, which
            //is a habitable room, and extract for the bathroom, which is a wet room.
            Assert.Equal(30, rates["Studio 1_0"], tolerance);
            Assert.Equal(8, rates["Bathroom_2"], tolerance);

            Assert.Equal(30, flat1.TotalSupply_Lps, tolerance);
            Assert.Equal(30, flat1.TotalExtract_Lps, tolerance);
        }

        /// <summary>
        /// The studio's local kitchen extract is now modelled as a terminal of its own, so Flat 1 carries
        /// the ADF F Vol 1 (2021) paragraph 1.17a and Table 1.2 kitchen requirement explicitly instead of
        /// reporting it as an unrepresentable gap. The bathroom's extract is still general extract, held
        /// separately, and still does not satisfy paragraph 1.17a.
        /// </summary>
        [Fact]
        public void Flats_StudioFlat_TakesItsOwnLocalKitchenExtractTerminal()
        {
            PartFCalculator partFCalculator = Flats();
            Assert.True(partFCalculator.Calculate(zoneCategoryName));

            PartFDwellingResult flat1 = Dwelling(partFCalculator, "Flat 1");

            PartFVentilationTerminalRequirement terminal = Assert.Single(flat1.ComplianceResult.LocalKitchenExtractTerminals);

            Assert.Equal("Studio 1_0", terminal.SpaceName);
            Assert.Equal(13, terminal.MinimumRequiredFlowRate_Lps.Value, tolerance);

            //Minimum-first, cooking-priority: kitchen 13 + bathroom 8 = 21 l/s of minimums, and the
            //remaining 9 l/s goes to the cooking space.
            Assert.Equal(22, terminal.ContinuousDesignFlowRate_Lps.Value, tolerance);

            Assert.Contains(flat1.ComplianceResult.SupplyTerminals, x => x.SpaceName == "Studio 1_0");
            Assert.Contains(flat1.ComplianceResult.GeneralExtractTerminals, x => x.SpaceName == "Bathroom_2");

            Assert.True(flat1.TotalExtract_Lps > 0);
            Assert.DoesNotContain(flat1.Warnings, x => x.Contains("ENGINEERING CHECK REQUIRED"));
        }

        /// <summary>The supplied model is never modified, in either grouping mode.</summary>
        [Theory]
        [InlineData(null)]
        [InlineData(zoneCategoryName)]
        public void Calculate_DoesNotModifyTheSuppliedModel(string zoneCategoryName_Input)
        {
            AdjacencyCluster adjacencyCluster = FlatsCluster();

            PartFCalculator partFCalculator = new(DataFile()) { AdjacencyCluster = adjacencyCluster };
            Assert.True(partFCalculator.Calculate(zoneCategoryName_Input));

            Assert.NotSame(adjacencyCluster, partFCalculator.AdjacencyCluster);
            Assert.All(adjacencyCluster.GetSpaces(), x => Assert.Null(x.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData)));
        }

        /// <summary>No calculated rate may ever be NaN or infinite, in either grouping mode.</summary>
        [Theory]
        [InlineData(null)]
        [InlineData(zoneCategoryName)]
        public void Calculate_NeverProducesNaNOrInfiniteRates(string zoneCategoryName_Input)
        {
            PartFCalculator partFCalculator = Flats();
            Assert.True(partFCalculator.Calculate(zoneCategoryName_Input));

            foreach (double rate in Rates(partFCalculator).Values)
            {
                Assert.False(double.IsNaN(rate));
                Assert.False(double.IsInfinity(rate));
            }

            foreach (PartFDwellingResult dwellingResult in partFCalculator.DwellingResults)
            {
                Assert.False(double.IsNaN(dwellingResult.FinalSystemRate_Lps));
                Assert.False(double.IsInfinity(dwellingResult.FinalSystemRate_Lps));
                Assert.False(double.IsNaN(dwellingResult.TotalSupply_Lps));
                Assert.False(double.IsNaN(dwellingResult.TotalExtract_Lps));
            }
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private static PartFData DataFile()
        {
            return Analytical.Create.PartFData(Fixtures.GetPath(dataFileName));
        }

        private static PartFDwellingResult Dwelling(PartFCalculator partFCalculator, string name)
        {
            PartFDwellingResult? result = partFCalculator.DwellingResults.Find(x => x.Name == name);
            Assert.NotNull(result);
            return result!;
        }

        private static Dictionary<string, double> Rates(PartFCalculator partFCalculator)
        {
            return partFCalculator.AdjacencyCluster.GetSpaces().ToDictionary(
                x => x.Name,
                x => x.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData)?.CalculatedFlowRate_Lps ?? 0);
        }

        /// <summary>A single house with no zones at all: 3 bedrooms, 118 m2.</summary>
        private static PartFCalculator House()
        {
            AdjacencyCluster adjacencyCluster = new();

            AddSpace(adjacencyCluster, "Bedroom 1", 14, 35, 0);
            AddSpace(adjacencyCluster, "Bedroom 2", 12, 30, 1);
            AddSpace(adjacencyCluster, "Bedroom 3", 10, 25, 2);
            AddSpace(adjacencyCluster, "Living Room", 28, 70, 3);
            AddSpace(adjacencyCluster, "Kitchen", 16, 40, 4);
            AddSpace(adjacencyCluster, "Bathroom", 6, 15, 5);
            AddSpace(adjacencyCluster, "WC", 3, 7.5, 6);
            AddSpace(adjacencyCluster, "Hall", 29, 72.5, 7);

            return new PartFCalculator(DataFile()) { AdjacencyCluster = adjacencyCluster };
        }

        private static PartFCalculator Flats()
        {
            return new PartFCalculator(DataFile()) { AdjacencyCluster = FlatsCluster() };
        }

        /// <summary>
        /// The zoning arrangement of SAM_zoningAM.sam: zone category "Flats" holding "Flat 1",
        /// "Corridor", "Flat 2" and "Flat 3", with that model's space names, areas and volumes.
        /// </summary>
        private static AdjacencyCluster FlatsCluster()
        {
            AdjacencyCluster result = new();

            AddZone(result, "Flat 1",
                AddSpace(result, "Studio 1_0", 75, 300, 0),
                AddSpace(result, "Bathroom_2", 25, 100, 2));

            AddZone(result, "Corridor",
                AddSpace(result, "Corridor_1", 366, 1464, 1));

            AddZone(result, "Flat 2",
                AddSpace(result, "Bedroom 2_3", 105, 420, 3),
                AddSpace(result, "Kitchen_4", 75, 300, 4),
                AddSpace(result, "Ensuite_5", 30, 120, 5));

            AddZone(result, "Flat 3",
                AddSpace(result, "Bedroom 2_6", 105, 420, 6),
                AddSpace(result, "Kitchen_7", 75, 300, 7),
                AddSpace(result, "Ensuite_8", 30, 120, 8));

            return result;
        }

        private static Space AddSpace(AdjacencyCluster adjacencyCluster, string name, double area_M2, double volume_M3, int index)
        {
            Space result = new(name, new Point3D(index * 10, 0, 1.5));
            result.SetValue(SpaceParameter.Area, area_M2);
            result.SetValue(SpaceParameter.Volume, volume_M3);
            adjacencyCluster.AddObject(result);
            return result;
        }

        private static void AddZone(AdjacencyCluster adjacencyCluster, string name, params Space[] spaces)
        {
            Zone zone = new(name);
            zone.SetValue(ZoneParameter.ZoneCategory, zoneCategoryName);
            adjacencyCluster.AddObject(zone);

            foreach (Space space in spaces)
            {
                adjacencyCluster.AddRelation(zone, space);
            }
        }
    }
}
