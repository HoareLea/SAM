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
    /// Tests for explicit dwelling identification when Part F is calculated per zone.
    /// </summary>
    /// <remarks>
    /// A zone category alone cannot identify a dwelling. The example model SAM_zoningAM.sam proves the
    /// point: its shared corridor is a zone in the same "Flats" category as the flats it serves, so a
    /// category-only filter sizes the corridor as a dwelling and produces meaningless rates for it. The
    /// fixtures here mirror that model's zoning, space names, areas and volumes. The .sam file itself is
    /// not used as a fixture - it lives outside the repository and is a binary - and is never written to.
    /// </remarks>
    public class PartFDwellingFilterTests
    {
        private const string dataFileName = "SAM_PartFSpaceRulesUKDwellingsMVHR.json";

        private const string zoneCategoryName = "Flats";

        private const double tolerance = 1e-6;

        // ------------------------------------------------------------------
        // Explicit dwelling identification
        // ------------------------------------------------------------------

        /// <summary>Zones explicitly marked as dwellings are each sized as their own dwelling.</summary>
        [Fact]
        public void ExplicitlyIdentifiedFlats_AreEachSizedAsADwelling()
        {
            PartFCalculator partFCalculator = Calculate(Marked());

            List<string> names = [.. partFCalculator.DwellingResults.ConvertAll(x => x.Name).OrderBy(x => x)];

            Assert.Equal(["Flat 1", "Flat 2", "Flat 3"], names);
        }

        /// <summary>
        /// A shared corridor explicitly marked Is Dwelling = false is never sized as a dwelling, and its
        /// spaces are left without ventilation properties.
        /// </summary>
        [Fact]
        public void ExplicitlyExcludedCorridor_IsNotSizedAsADwelling()
        {
            PartFCalculator partFCalculator = Calculate(Marked());

            Assert.DoesNotContain("Corridor", partFCalculator.DwellingResults.ConvertAll(x => x.Name));
            Assert.Contains("Corridor", partFCalculator.ExcludedZoneNames);
            Assert.Null(SpaceData(partFCalculator, "Corridor_1"));
        }

        /// <summary>
        /// A flow rate from an EARLIER calculation must not survive into a later one that no longer
        /// sizes that space. The corridor's space never having PartFSpaceData in a fresh model (the test
        /// above) does not exercise this, because circulation takes no Part F terminal at all and so is
        /// never populated regardless of inclusion - it exercises the exact regression this guards: size
        /// Flat 1 as a dwelling, giving Studio 1_0 a real, non-null flow rate, then decide Flat 1 is
        /// actually not a dwelling (e.g. a landlord unit) and recalculate on that same, now-populated
        /// cluster; Studio 1_0's stale rate from when it WAS sized must be gone, not just absent from
        /// this run's own writes.
        /// </summary>
        [Fact]
        public void ZoneExcludedAfterAnEarlierRunSizedIt_LosesItsStaleFlowRate()
        {
            PartFCalculator partFCalculator = Calculate(Marked());

            PartFSpaceData spaceData_BeforeExclusion = SpaceData(partFCalculator, "Studio 1_0");
            Assert.NotNull(spaceData_BeforeExclusion);
            Assert.True(spaceData_BeforeExclusion.ContinuousDesignFlowRate_Lps > 0);

            AdjacencyCluster adjacencyCluster = partFCalculator.AdjacencyCluster;
            Zone flat1 = adjacencyCluster.GetZones().Find(x => x.Name == "Flat 1");
            flat1.SetValue(ZoneParameter.IsDwelling, false);
            adjacencyCluster.AddObject(flat1);

            PartFCalculator partFCalculator_Reexcluded = Calculate(adjacencyCluster);

            Assert.Null(SpaceData(partFCalculator_Reexcluded, "Studio 1_0"));
        }

        /// <summary>The excluded corridor is reported, so its exclusion is visible rather than silent.</summary>
        [Fact]
        public void ExplicitlyExcludedCorridor_IsReported()
        {
            PartFCalculator partFCalculator = Calculate(Marked());

            Assert.Contains(partFCalculator.Remarks, x => x.Contains("Is Dwelling = false") && x.Contains("Corridor"));
        }

        /// <summary>
        /// The corridor must not appear as a dwelling with no bedroom, no habitable room and no wet room -
        /// which is exactly what category-only filtering produced.
        /// </summary>
        [Fact]
        public void ExplicitlyExcludedCorridor_RaisesNoEmptyDwellingWarnings()
        {
            PartFCalculator partFCalculator = Calculate(Marked());

            Assert.DoesNotContain(partFCalculator.Warnings, x => x.StartsWith("Corridor:"));
        }

        /// <summary>A zone explicitly marked false is never processed, even if it is the only zone.</summary>
        [Fact]
        public void OnlyZoneMarkedFalse_IsNeverProcessedAsADwelling()
        {
            AdjacencyCluster adjacencyCluster = new();
            AddZone(adjacencyCluster, "Corridor", false, AddSpace(adjacencyCluster, "Corridor_1", 366, 1464, 0));

            PartFCalculator partFCalculator = Calculate(adjacencyCluster);

            Assert.Empty(partFCalculator.DwellingResults);
            Assert.Contains(partFCalculator.Warnings, x => x.Contains("No zone in category 'Flats' is marked Is Dwelling = true"));
        }

        // ------------------------------------------------------------------
        // Mixed marking
        // ------------------------------------------------------------------

        /// <summary>
        /// Where some zones carry the flag and others do not, only an explicit true is treated as a
        /// dwelling. An unmarked zone is neither silently included nor silently dropped: it is reported.
        /// </summary>
        [Fact]
        public void MixedMarking_ProcessesOnlyExplicitTrueAndReportsTheUnmarked()
        {
            AdjacencyCluster adjacencyCluster = new();

            AddZone(adjacencyCluster, "Flat 1", true,
                AddSpace(adjacencyCluster, "Studio 1_0", 75, 300, 0),
                AddSpace(adjacencyCluster, "Bathroom_2", 25, 100, 1));

            AddZone(adjacencyCluster, "Corridor", false,
                AddSpace(adjacencyCluster, "Corridor_1", 366, 1464, 2));

            //No Is Dwelling parameter at all.
            AddZone(adjacencyCluster, "Flat 2", null,
                AddSpace(adjacencyCluster, "Bedroom 2_3", 105, 420, 3),
                AddSpace(adjacencyCluster, "Kitchen_4", 75, 300, 4));

            PartFCalculator partFCalculator = Calculate(adjacencyCluster);

            Assert.Equal(["Flat 1"], partFCalculator.DwellingResults.ConvertAll(x => x.Name));

            Assert.Contains("Flat 2", partFCalculator.ExcludedZoneNames);
            Assert.Contains("Corridor", partFCalculator.ExcludedZoneNames);

            Assert.Contains(partFCalculator.Warnings, x => x.Contains("no Is Dwelling parameter while others do") && x.Contains("Flat 2"));

            Assert.Null(SpaceData(partFCalculator, "Bedroom 2_3"));
            Assert.NotNull(SpaceData(partFCalculator, "Studio 1_0"));
        }

        // ------------------------------------------------------------------
        // Backward compatibility with unmarked models
        // ------------------------------------------------------------------

        /// <summary>
        /// A legacy model where no zone carries the flag keeps the previous category-based behaviour, so
        /// existing definitions continue to work.
        /// </summary>
        [Fact]
        public void LegacyModelWithNoDwellingFlags_KeepsCategoryBasedBehaviour()
        {
            PartFCalculator partFCalculator = Calculate(Unmarked());

            List<string> names = [.. partFCalculator.DwellingResults.ConvertAll(x => x.Name).OrderBy(x => x)];

            Assert.Equal(["Corridor", "Flat 1", "Flat 2", "Flat 3"], names);
        }

        /// <summary>
        /// That fallback must be reported: a corridor sized as a dwelling is a modelling problem, and the
        /// warning is how the engineer finds out.
        /// </summary>
        [Fact]
        public void LegacyModelWithNoDwellingFlags_RaisesAClearWarning()
        {
            PartFCalculator partFCalculator = Calculate(Unmarked());

            Assert.Contains(partFCalculator.Warnings, x => x.Contains("has an Is Dwelling parameter") && x.Contains("Set Is Dwelling on each zone"));
        }

        /// <summary>
        /// Even under the legacy fallback, a communal corridor recognised by name is excluded from the
        /// dwelling it was wrongly placed in, so it cannot contribute floor area or flow rates.
        /// </summary>
        [Fact]
        public void CommunalCorridorInsideADwelling_IsExcludedFromIt()
        {
            AdjacencyCluster adjacencyCluster = new();

            AddZone(adjacencyCluster, "Flat 1", true,
                AddSpace(adjacencyCluster, "Bedroom 1", 14, 35, 0),
                AddSpace(adjacencyCluster, "Bathroom", 6, 15, 1),
                AddSpace(adjacencyCluster, "Communal Corridor", 300, 750, 2));

            PartFCalculator partFCalculator = Calculate(adjacencyCluster);

            PartFDwellingResult dwellingResult = Assert.Single(partFCalculator.DwellingResults);

            Assert.Contains("Communal Corridor", dwellingResult.NonDwellingSpaceNames);

            //14 + 6 = 20 m2. The 300 m2 corridor must not have been counted.
            Assert.Equal(20, dwellingResult.InternalFloorArea_M2, tolerance);
            Assert.Contains(partFCalculator.Warnings, x => x.Contains("not part of any dwelling"));
        }

        // ------------------------------------------------------------------
        // Category selection
        // ------------------------------------------------------------------

        /// <summary>An invalid category is reported with the categories that are present.</summary>
        [Fact]
        public void InvalidCategory_IsReportedWithTheCategoriesPresent()
        {
            PartFCalculator partFCalculator = new(DataFile()) { AdjacencyCluster = Marked() };

            Assert.True(partFCalculator.Calculate("NotACategory"));

            Assert.Empty(partFCalculator.DwellingResults);
            Assert.Contains(partFCalculator.Warnings, x => x.Contains("No zone belongs to zone category 'NotACategory'") && x.Contains("Flats"));
        }

        /// <summary>
        /// An empty category means single house mode: the whole model is one dwelling, no dwelling zone
        /// or flag is needed, and no warning is raised merely because the input was left empty.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void EmptyCategory_IsSingleHouseModeWithoutWarning(string zoneCategoryName_Empty)
        {
            AdjacencyCluster adjacencyCluster = new();
            AddSpace(adjacencyCluster, "Bedroom 1", 14, 35, 0);
            AddSpace(adjacencyCluster, "Living Room", 28, 70, 1);
            AddSpace(adjacencyCluster, "Kitchen", 16, 40, 2);
            AddSpace(adjacencyCluster, "Bathroom", 6, 15, 3);

            PartFCalculator partFCalculator = new(DataFile()) { AdjacencyCluster = adjacencyCluster };

            Assert.True(partFCalculator.Calculate(zoneCategoryName_Empty));

            PartFDwellingResult dwellingResult = Assert.Single(partFCalculator.DwellingResults);

            Assert.Null(dwellingResult.Name);
            Assert.Empty(partFCalculator.Warnings);
        }

        /// <summary>
        /// Single house mode must not require a dwelling flag, and must not warn about one being absent -
        /// there is no zone involved at all.
        /// </summary>
        [Fact]
        public void SingleHouseMode_NeedsNoZonesAndRaisesNoDwellingFlagWarning()
        {
            AdjacencyCluster adjacencyCluster = new();
            AddSpace(adjacencyCluster, "Bedroom 1", 14, 35, 0);
            AddSpace(adjacencyCluster, "Living Room", 28, 70, 1);
            AddSpace(adjacencyCluster, "Kitchen", 16, 40, 2);
            AddSpace(adjacencyCluster, "Bathroom", 6, 15, 3);

            PartFCalculator partFCalculator = new(DataFile()) { AdjacencyCluster = adjacencyCluster };

            Assert.True(partFCalculator.Calculate());

            Assert.Empty(adjacencyCluster.GetZones() ?? []);
            Assert.DoesNotContain(partFCalculator.Warnings, x => x.Contains("Is Dwelling"));
        }

        // ------------------------------------------------------------------
        // Space membership
        // ------------------------------------------------------------------

        /// <summary>A space in two dwelling zones is sized twice and must be reported.</summary>
        [Fact]
        public void SpaceInMultipleDwellingZones_IsReported()
        {
            AdjacencyCluster adjacencyCluster = Marked();

            Space space = adjacencyCluster.GetSpaces().Find(x => x.Name == "Kitchen_4");
            Zone zone = adjacencyCluster.GetZones().Find(x => x.Name == "Flat 3");
            adjacencyCluster.AddRelation(zone, space);

            PartFCalculator partFCalculator = Calculate(adjacencyCluster);

            Assert.Contains(partFCalculator.Warnings, x => x.Contains("Kitchen_4") && x.Contains("more than one dwelling zone"));
        }

        /// <summary>Spaces outside every dwelling zone are reported and left unsized.</summary>
        [Fact]
        public void SpacesOutsideEveryDwellingZone_AreReportedAndLeftUnsized()
        {
            AdjacencyCluster adjacencyCluster = Marked();
            AddSpace(adjacencyCluster, "Landlord Store", 12, 30, 20);

            PartFCalculator partFCalculator = Calculate(adjacencyCluster);

            Assert.Contains(partFCalculator.Warnings, x => x.Contains("do not belong to any dwelling zone") && x.Contains("Landlord Store"));
            Assert.Null(SpaceData(partFCalculator, "Landlord Store"));
        }

        /// <summary>
        /// The spaces of an explicitly excluded corridor zone are not silently forgotten, but their
        /// exclusion is expected - SelectDwellingZones already reported it as a zone-level Remark - so
        /// they must not also raise the generic "outside every dwelling zone" Warning, which would report
        /// the same, expected exclusion a second time as if something needed fixing.
        /// </summary>
        [Fact]
        public void SpacesOfAnExcludedZone_AreReportedAsARemarkNotAWarning()
        {
            PartFCalculator partFCalculator = Calculate(Marked());

            Assert.DoesNotContain(partFCalculator.Warnings, x => x.Contains("Corridor_1"));
            Assert.Contains(partFCalculator.Remarks, x => x.Contains("Corridor") && x.Contains("Corridor_1") && x.Contains("Is Dwelling is set to No"));
        }

        /// <summary>
        /// Minimal, single-zone-pair reproduction of the same rule: Is Dwelling = false is an expected
        /// exclusion, not a problem to warn about.
        /// </summary>
        [Fact]
        public void ExplicitlyExcludedZone_RaisesNoUnzonedSpaceWarning()
        {
            AdjacencyCluster adjacencyCluster = new();
            AddZone(adjacencyCluster, "Flat 1", true,
                AddSpace(adjacencyCluster, "Bedroom 1", 14, 35, 0),
                AddSpace(adjacencyCluster, "Bathroom", 6, 15, 1));
            AddZone(adjacencyCluster, "Corridor", false, AddSpace(adjacencyCluster, "Corridor_1", 30, 90, 2));

            PartFCalculator partFCalculator = Calculate(adjacencyCluster);

            Assert.DoesNotContain(partFCalculator.Warnings, x => x.Contains("Corridor_1"));
            Assert.Contains(partFCalculator.Remarks, x => x.Contains("Corridor") && x.Contains("Is Dwelling is set to No"));
        }

        // ------------------------------------------------------------------
        // Isolation between dwellings
        // ------------------------------------------------------------------

        /// <summary>Each dwelling is sized independently: no bedroom count or area leaks between flats.</summary>
        [Fact]
        public void EachDwelling_IsCalculatedIndependently()
        {
            PartFCalculator partFCalculator = Calculate(Marked());

            PartFDwellingResult dwellingResult_2 = Dwelling(partFCalculator, "Flat 2");
            PartFDwellingResult dwellingResult_3 = Dwelling(partFCalculator, "Flat 3");

            //Flat 2 and Flat 3 are identical, so they must size identically.
            Assert.Equal(dwellingResult_2.ContinuousDesignSystemRate_Lps, dwellingResult_3.ContinuousDesignSystemRate_Lps, tolerance);
            Assert.Equal(dwellingResult_2.InternalFloorArea_M2, dwellingResult_3.InternalFloorArea_M2, tolerance);

            //Flat 1 is a studio flat and must not share Flat 2's floor area.
            Assert.NotEqual(Dwelling(partFCalculator, "Flat 1").InternalFloorArea_M2, dwellingResult_2.InternalFloorArea_M2);
        }

        /// <summary>Changing one flat must not alter another - no cross contamination.</summary>
        [Fact]
        public void ChangingOneDwelling_DoesNotAffectAnother()
        {
            double before = Dwelling(Calculate(Marked()), "Flat 3").ContinuousDesignSystemRate_Lps;

            AdjacencyCluster adjacencyCluster = Marked();
            Zone zone = adjacencyCluster.GetZones().Find(x => x.Name == "Flat 2");
            adjacencyCluster.AddRelation(zone, AddSpace(adjacencyCluster, "Bedroom 2_9", 200, 800, 9));

            double after = Dwelling(Calculate(adjacencyCluster), "Flat 3").ContinuousDesignSystemRate_Lps;

            Assert.Equal(before, after, tolerance);
        }

        /// <summary>Every dwelling balances at both the design and the background condition.</summary>
        [Fact]
        public void EveryDwelling_BalancesAtBothConditions()
        {
            PartFCalculator partFCalculator = Calculate(Marked());

            foreach (PartFDwellingResult dwellingResult in partFCalculator.DwellingResults)
            {
                if (dwellingResult.TotalExtract_Lps == 0)
                {
                    //A dwelling with no wet room cannot balance; it is reported instead.
                    continue;
                }

                Assert.Equal(dwellingResult.TotalSupply_Lps, dwellingResult.TotalExtract_Lps, tolerance);
                Assert.Equal(dwellingResult.TotalSetbackSupply_Lps, dwellingResult.TotalSetbackExtract_Lps, tolerance);
            }
        }

        // ------------------------------------------------------------------
        // Stale internal conditions must not remove a wet room's extract
        // ------------------------------------------------------------------

        /// <summary>
        /// Regression against the real SAM_zoningAM model, which carries the TM59 "Studio" condition on
        /// spaces named Bathroom_2 and Ensuite_5. Trusting the condition over the space name turned each
        /// wet room into a habitable supply space, so the flat lost its only extract and supply no longer
        /// balanced extract. The room name must win, and the dwelling must still balance.
        /// </summary>
        [Fact]
        public void StaleStudioInternalConditions_DoNotRemoveTheWetRoomExtract()
        {
            AdjacencyCluster adjacencyCluster = new();

            Space space_Studio = AddSpace(adjacencyCluster, "Studio 1_0", 75, 300, 0);
            Space space_Bathroom = AddSpace(adjacencyCluster, "Bathroom_2", 25, 100, 1);

            //Exactly what the example model holds: the TM59 Studio condition on both spaces.
            space_Studio.InternalCondition = new InternalCondition("Studio");
            space_Bathroom.InternalCondition = new InternalCondition("Studio");
            adjacencyCluster.AddObject(space_Studio);
            adjacencyCluster.AddObject(space_Bathroom);

            AddZone(adjacencyCluster, "Flat 1", true, space_Studio, space_Bathroom);

            PartFCalculator partFCalculator = Calculate(adjacencyCluster);

            PartFDwellingResult dwellingResult = Dwelling(partFCalculator, "Flat 1");

            //The bathroom keeps its Table 1.2 minimum and takes the general extract. The studio's own local
            //kitchen extract carries the 13 l/s kitchen minimum, so the per-room high-rate minimum total
            //is 21 l/s.
            Assert.Equal(21, dwellingResult.WetRoomMinimumTotal_Lps, tolerance);
            Assert.True(SpaceData(partFCalculator, "Bathroom_2")!.ContinuousDesignFlowRate_Lps > 0);
            Assert.Equal(Analytical.Enums.PartFVentilationType.extract, SpaceData(partFCalculator, "Bathroom_2")!.PartFVentilationType);
            Assert.Equal(Analytical.Enums.PartFVentilationType.supply, SpaceData(partFCalculator, "Studio 1_0")!.PartFVentilationType);

            //And the dwelling balances at both conditions, which it did not before the fix.
            Assert.Equal(dwellingResult.TotalSupply_Lps, dwellingResult.TotalExtract_Lps, tolerance);
            Assert.Equal(dwellingResult.TotalSetbackSupply_Lps, dwellingResult.TotalSetbackExtract_Lps, tolerance);
            Assert.Equal(30, dwellingResult.ContinuousDesignSystemRate_Lps, tolerance);
        }

        // ------------------------------------------------------------------
        // The supplied model is never modified
        // ------------------------------------------------------------------

        /// <summary>The input AnalyticalModel must be left untouched; an updated copy is returned.</summary>
        [Theory]
        [InlineData(null)]
        [InlineData(zoneCategoryName)]
        public void Calculate_DoesNotModifyTheSuppliedModel(string zoneCategoryName_Input)
        {
            AdjacencyCluster adjacencyCluster = Marked();

            PartFCalculator partFCalculator = new(DataFile()) { AdjacencyCluster = adjacencyCluster };
            Assert.True(partFCalculator.Calculate(zoneCategoryName_Input));

            foreach (Space space in adjacencyCluster.GetSpaces())
            {
                Assert.Null(space.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData));
                Assert.Null(space.GetValue<SpaceSemantics>(SpaceParameter.SpaceSemantics));
            }
        }

        /// <summary>No rate produced in zoned mode may be NaN or infinite.</summary>
        [Fact]
        public void ZonedMode_NeverProducesNaNOrInfiniteRates()
        {
            PartFCalculator partFCalculator = Calculate(Marked());

            foreach (Space space in partFCalculator.AdjacencyCluster.GetSpaces())
            {
                PartFSpaceData partFSpaceData = space.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData);
                if (partFSpaceData is null)
                {
                    continue;
                }

                Assert.False(double.IsNaN(partFSpaceData.ContinuousDesignFlowRate_Lps ?? 0));
                Assert.False(double.IsInfinity(partFSpaceData.ContinuousDesignFlowRate_Lps ?? 0));
                Assert.False(double.IsNaN(partFSpaceData.SetbackFlowRate_Lps ?? 0));
                Assert.False(double.IsInfinity(partFSpaceData.SetbackFlowRate_Lps ?? 0));
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
            PartFDwellingResult result = partFCalculator.DwellingResults.Find(x => x.Name == name);
            Assert.NotNull(result);
            return result;
        }

        private static PartFSpaceData SpaceData(PartFCalculator partFCalculator, string name)
        {
            return partFCalculator.AdjacencyCluster.GetSpaces().Find(x => x.Name == name)
                ?.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData);
        }

        private static PartFCalculator Calculate(AdjacencyCluster adjacencyCluster)
        {
            PartFCalculator result = new(DataFile()) { AdjacencyCluster = adjacencyCluster };

            Assert.True(result.Calculate(zoneCategoryName));

            return result;
        }

        /// <summary>The zoning of SAM_zoningAM.sam, with the dwellings and the corridor marked explicitly.</summary>
        private static AdjacencyCluster Marked()
        {
            return Cluster(true);
        }

        /// <summary>The same zoning as a legacy model: no zone carries an Is Dwelling parameter.</summary>
        private static AdjacencyCluster Unmarked()
        {
            return Cluster(false);
        }

        private static AdjacencyCluster Cluster(bool marked)
        {
            AdjacencyCluster result = new();

            AddZone(result, "Flat 1", marked ? true : null,
                AddSpace(result, "Studio 1_0", 75, 300, 0),
                AddSpace(result, "Bathroom_2", 25, 100, 2));

            AddZone(result, "Corridor", marked ? false : null,
                AddSpace(result, "Corridor_1", 366, 1464, 1));

            AddZone(result, "Flat 2", marked ? true : null,
                AddSpace(result, "Bedroom 2_3", 105, 420, 3),
                AddSpace(result, "Kitchen_4", 75, 300, 4),
                AddSpace(result, "Ensuite_5", 30, 120, 5));

            AddZone(result, "Flat 3", marked ? true : null,
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

        private static void AddZone(AdjacencyCluster adjacencyCluster, string name, bool? isDwelling, params Space[] spaces)
        {
            Zone zone = new(name);
            zone.SetValue(ZoneParameter.ZoneCategory, zoneCategoryName);

            if (isDwelling.HasValue)
            {
                zone.SetValue(ZoneParameter.IsDwelling, isDwelling.Value);
            }

            adjacencyCluster.AddObject(zone);

            foreach (Space space in spaces)
            {
                adjacencyCluster.AddRelation(zone, space);
            }
        }
    }
}
