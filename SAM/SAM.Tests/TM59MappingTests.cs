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
    /// End-to-end TM59 mapping: the supplied UI example, flat/bedroom-count composition, occupancy,
    /// and the manual-review outcomes for ambiguous or unsupported cases.
    /// </summary>
    public class TM59MappingTests
    {
        private static Space NewSpace(string name, double? area = null)
        {
            Space space = new Space(Guid.NewGuid(), name, null);
            if (area.HasValue)
                space.SetValue(SpaceParameter.Area, area.Value);

            return space;
        }

        private static string? ResolveName(TM59InternalConditionResolver resolver, Space space, List<Space>? flat)
        {
            return resolver.Resolve(space, flat).InternalCondition?.Name;
        }

        [Fact]
        public void Supplied_Example_Resolves_As_Expected()
        {
            TM59InternalConditionResolver resolver = TM59TestData.NewResolver();

            // Corridor (no flat/zone at all)
            Assert.Equal("TM59_Communal Corridor (including pipework gains)",
                ResolveName(resolver, NewSpace("Corridor1"), null));

            // Flat 1: Studio + its own bathroom
            Space bathroom2 = NewSpace("Bathroom2");
            Space studio10 = NewSpace("Studio 10");
            List<Space> flat1 = new List<Space> { bathroom2, studio10 };
            Assert.Equal("TM59_Bathroom", ResolveName(resolver, bathroom2, flat1));
            Assert.Equal("Studio", ResolveName(resolver, studio10, flat1));

            // Flat 2: one bedroom -> Double, one ensuite, one kitchen -> 1 Bed Apt. Kitchen
            Space bedroom23 = NewSpace("Bedroom 23");
            Space ensuite5 = NewSpace("Ensuite5");
            Space kitchen4 = NewSpace("Kitchen4");
            List<Space> flat2 = new List<Space> { bedroom23, ensuite5, kitchen4 };
            Assert.Equal("Double Bedroom", ResolveName(resolver, bedroom23, flat2));
            Assert.Equal("TM59_Bathroom", ResolveName(resolver, ensuite5, flat2));
            Assert.Equal("1 Bed Apt. Kitchen", ResolveName(resolver, kitchen4, flat2));

            // Flat 3: same shape as Flat 2
            Space bedroom26 = NewSpace("Bedroom26");
            Space ensuite8 = NewSpace("Ensuite8");
            Space kitchen7 = NewSpace("Kitchen7");
            List<Space> flat3 = new List<Space> { bedroom26, ensuite8, kitchen7 };
            Assert.Equal("Double Bedroom", ResolveName(resolver, bedroom26, flat3));
            Assert.Equal("TM59_Bathroom", ResolveName(resolver, ensuite8, flat3));
            Assert.Equal("1 Bed Apt. Kitchen", ResolveName(resolver, kitchen7, flat3));
        }

        [Fact]
        public void OneBed_Flat_Combined_LivingKitchen()
        {
            TM59InternalConditionResolver resolver = TM59TestData.NewResolver();
            Space bedroom = NewSpace("Bedroom 1");
            Space livingKitchen = NewSpace("Living Kitchen");
            List<Space> flat = new List<Space> { bedroom, livingKitchen };

            TM59InternalConditionResult result = resolver.Resolve(livingKitchen, flat);
            Assert.Equal("1 Bed Apt. Living Room/Kitchen", result.InternalCondition?.Name);
            Assert.Equal(2, result.Occupancy);
        }

        [Fact]
        public void TwoBed_Flat_Separate_Living_And_Kitchen()
        {
            TM59InternalConditionResolver resolver = TM59TestData.NewResolver();
            Space bedroom1 = NewSpace("Bedroom 1");
            Space bedroom2 = NewSpace("Bedroom 2");
            Space living = NewSpace("Living Room");
            Space kitchen = NewSpace("Kitchen");
            List<Space> flat = new List<Space> { bedroom1, bedroom2, living, kitchen };

            Assert.Equal("2 Bed Apt. Living Room", ResolveName(resolver, living, flat));
            Assert.Equal("2 Bed Apt. Kitchen", ResolveName(resolver, kitchen, flat));

            TM59InternalConditionResult livingResult = resolver.Resolve(living, flat);
            Assert.Equal(3, livingResult.Occupancy);
        }

        [Fact]
        public void ThreeBed_Flat_Combined_LivingKitchen()
        {
            TM59InternalConditionResolver resolver = TM59TestData.NewResolver();
            Space bedroom1 = NewSpace("Bedroom 1");
            Space bedroom2 = NewSpace("Bedroom 2");
            Space bedroom3 = NewSpace("Bedroom 3");
            Space livingKitchen = NewSpace("Living Kitchen");
            List<Space> flat = new List<Space> { bedroom1, bedroom2, bedroom3, livingKitchen };

            TM59InternalConditionResult result = resolver.Resolve(livingKitchen, flat);
            Assert.Equal("3 Bed Apt. Living Room/Kitchen", result.InternalCondition?.Name);
            Assert.Equal(4, result.Occupancy);
        }

        [Fact]
        public void FourBed_Flat_Is_Manual_Review_Not_Clamped_To_ThreeBed()
        {
            TM59InternalConditionResolver resolver = TM59TestData.NewResolver();
            List<Space> bedrooms = Enumerable.Range(1, 4).Select(i => NewSpace($"Bedroom {i}")).ToList();
            Space livingKitchen = NewSpace("Living Kitchen");
            List<Space> flat = new List<Space>(bedrooms) { livingKitchen };

            TM59InternalConditionResult result = resolver.Resolve(livingKitchen, flat);

            Assert.Null(result.InternalCondition);
            Assert.Contains("4 bedrooms", result.Diagnostic);
        }

        [Fact]
        public void ZoneLess_Corridor_Resolves_Without_A_Zone()
        {
            AdjacencyCluster adjacencyCluster = new AdjacencyCluster();
            Space corridor = NewSpace("Corridor 5");
            adjacencyCluster.AddObject(corridor);

            TM59InternalConditionResolver resolver = TM59TestData.NewResolver();
            TM59InternalConditionResult result = resolver.Resolve(adjacencyCluster, corridor, "Flats");

            Assert.Equal("TM59_Communal Corridor (including pipework gains)", result.InternalCondition?.Name);
        }

        [Fact]
        public void Null_AdjacencyCluster_Does_Not_Resolve_A_Habitable_Space_As_A_SoleSpace_Flat()
        {
            // A null AdjacencyCluster must be treated exactly like "no zone found" for habitable
            // classifications - never as license to resolve the space alone as if it were a one-space
            // flat, which would otherwise silently promote a lone bedroom to Double Bedroom and a lone
            // combined living/kitchen to Studio with no zone at all to justify either assumption.
            TM59InternalConditionResolver resolver = TM59TestData.NewResolver();

            Space bedroom = NewSpace("Bedroom 1");
            TM59InternalConditionResult bedroomResult = resolver.Resolve(null, bedroom, "Flats");
            Assert.Null(bedroomResult.InternalCondition);
            Assert.False(string.IsNullOrWhiteSpace(bedroomResult.Diagnostic));

            Space livingKitchen = NewSpace("Living Kitchen");
            TM59InternalConditionResult livingKitchenResult = resolver.Resolve(null, livingKitchen, "Flats");
            Assert.Null(livingKitchenResult.InternalCondition);
            Assert.False(string.IsNullOrWhiteSpace(livingKitchenResult.Diagnostic));
        }

        [Fact]
        public void Null_AdjacencyCluster_Still_Resolves_NonHabitable_Spaces()
        {
            // Non-habitable spaces need no zone/flat context at all - unaffected by the fix above.
            TM59InternalConditionResolver resolver = TM59TestData.NewResolver();
            TM59InternalConditionResult result = resolver.Resolve(null, NewSpace("Corridor1"), "Flats");
            Assert.Equal("TM59_Communal Corridor (including pipework gains)", result.InternalCondition?.Name);
        }

        [Fact]
        public void Bathroom_Ensuite_Corridor_Do_Not_Increase_Bedroom_Count()
        {
            TM59InternalConditionResolver resolver = TM59TestData.NewResolver();
            Space bedroom1 = NewSpace("Bedroom 1");
            Space bedroom2 = NewSpace("Bedroom 2");
            Space bathroom = NewSpace("Bathroom");
            Space ensuite = NewSpace("Ensuite");
            Space kitchen = NewSpace("Kitchen");
            List<Space> flat = new List<Space> { bedroom1, bedroom2, bathroom, ensuite, kitchen };

            TM59InternalConditionResult result = resolver.Resolve(kitchen, flat);
            Assert.Equal(2, result.BedroomCount);
            Assert.Equal("2 Bed Apt. Kitchen", result.InternalCondition?.Name);
        }

        [Fact]
        public void OneDouble_RestSingle_Inference_When_No_Keywords_Or_Area()
        {
            TM59InternalConditionResolver resolver = TM59TestData.NewResolver();
            Space bedroomA = NewSpace("Bedroom A");
            Space bedroomB = NewSpace("Bedroom B");
            Space bedroomC = NewSpace("Bedroom C");
            List<Space> flat = new List<Space> { bedroomA, bedroomB, bedroomC };

            List<string?> resolved = flat.Select(x => ResolveName(resolver, x, flat)).ToList();

            Assert.Single(resolved, x => x == "Double Bedroom");
            Assert.Equal(2, resolved.Count(x => x == "Single Bedroom"));

            // Deterministic: stable name-ordering picks "Bedroom A" as the main/double bedroom.
            Assert.Equal("Double Bedroom", ResolveName(resolver, bedroomA, flat));
        }

        [Fact]
        public void Explicit_Single_Double_Keywords_Override_Area_Based_Inference()
        {
            TM59InternalConditionResolver resolver = TM59TestData.NewResolver();
            // Smaller room is explicitly "Master" (double); larger room has no keyword.
            Space master = NewSpace("Master Bedroom", area: 8.0);
            Space plain = NewSpace("Bedroom 2", area: 20.0);
            List<Space> flat = new List<Space> { master, plain };

            Assert.Equal("Double Bedroom", ResolveName(resolver, master, flat));
            Assert.Equal("Single Bedroom", ResolveName(resolver, plain, flat));
        }

        [Fact]
        public void Largest_Bedroom_By_Area_Becomes_Double_When_No_Keyword_Present()
        {
            TM59InternalConditionResolver resolver = TM59TestData.NewResolver();
            Space small = NewSpace("Bedroom 1", area: 9.0);
            Space large = NewSpace("Bedroom 2", area: 16.0);
            List<Space> flat = new List<Space> { small, large };

            Assert.Equal("Double Bedroom", ResolveName(resolver, large, flat));
            Assert.Equal("Single Bedroom", ResolveName(resolver, small, flat));
        }

        [Fact]
        public void Occupancy_Table_Matches_The_Documented_Values()
        {
            TM59InternalConditionResolver resolver = TM59TestData.NewResolver();

            Space studioSpace = NewSpace("Studio");
            Assert.Equal(2, resolver.Resolve(studioSpace, new List<Space> { studioSpace }).Occupancy);

            // Explicit "master" keyword removes any dependency on the area/name tie-break, so the
            // Double/Single occupancy figures are asserted unambiguously.
            Space masterBedroom = NewSpace("Master Bedroom");
            Space singleBedroomSpace = NewSpace("Bedroom 2");
            List<Space> twoBedFlat = new List<Space> { masterBedroom, singleBedroomSpace };
            Assert.Equal(2, resolver.Resolve(masterBedroom, twoBedFlat).Occupancy);
            Assert.Equal(1, resolver.Resolve(singleBedroomSpace, twoBedFlat).Occupancy);

            Space bathroom = NewSpace("Bathroom");
            Assert.Equal(0, resolver.Resolve(bathroom, null).Occupancy);
        }

        [Fact]
        public void Integration_Through_Real_AdjacencyCluster_And_UpdateZone()
        {
            AdjacencyCluster adjacencyCluster = new AdjacencyCluster();

            Space bedroom = NewSpace("Bedroom 23");
            Space ensuite = NewSpace("Ensuite5");
            Space kitchen = NewSpace("Kitchen4");

            adjacencyCluster.AddObject(bedroom);
            adjacencyCluster.AddObject(ensuite);
            adjacencyCluster.AddObject(kitchen);
            adjacencyCluster.UpdateZone("Flat 2", "Flats", bedroom, ensuite, kitchen);

            TM59InternalConditionResolver resolver = TM59TestData.NewResolver();

            Assert.Equal("Double Bedroom", resolver.Resolve(adjacencyCluster, bedroom, "Flats").InternalCondition?.Name);
            Assert.Equal("TM59_Bathroom", resolver.Resolve(adjacencyCluster, ensuite, "Flats").InternalCondition?.Name);
            Assert.Equal("1 Bed Apt. Kitchen", resolver.Resolve(adjacencyCluster, kitchen, "Flats").InternalCondition?.Name);
        }

        [Fact]
        public void Integration_Corridor_With_No_Zone_Still_Resolves()
        {
            AdjacencyCluster adjacencyCluster = new AdjacencyCluster();
            Space corridor = NewSpace("Corridor1");
            adjacencyCluster.AddObject(corridor);
            // Deliberately not added to any zone.

            TM59InternalConditionResolver resolver = TM59TestData.NewResolver();
            TM59InternalConditionResult result = resolver.Resolve(adjacencyCluster, corridor, "Flats");

            Assert.Equal("TM59_Communal Corridor (including pipework gains)", result.InternalCondition?.Name);
        }

        [Fact]
        public void Integration_TwoBed_Flat_Through_Real_AdjacencyCluster()
        {
            AdjacencyCluster adjacencyCluster = new AdjacencyCluster();

            Space bedroom1 = NewSpace("Bedroom 1");
            Space bedroom2 = NewSpace("Bedroom 2");
            Space living = NewSpace("Living Room");
            Space kitchen = NewSpace("Kitchen");

            foreach (Space space in new[] { bedroom1, bedroom2, living, kitchen })
                adjacencyCluster.AddObject(space);

            adjacencyCluster.UpdateZone("Flat 9", "Flats", bedroom1, bedroom2, living, kitchen);

            TM59InternalConditionResolver resolver = TM59TestData.NewResolver();
            Assert.Equal("2 Bed Apt. Living Room", resolver.Resolve(adjacencyCluster, living, "Flats").InternalCondition?.Name);
            Assert.Equal("2 Bed Apt. Kitchen", resolver.Resolve(adjacencyCluster, kitchen, "Flats").InternalCondition?.Name);
        }

        // --- Partial bedroom-area data (requirement 2): some, but not all, bedrooms in the flat have
        // a valid Area. A missing Area must never be treated as zero, so this must fall back to the
        // same stable name/Guid ordering as the no-area case, not silently favour the one with data. ---

        [Fact]
        public void Partial_Bedroom_Area_Data_Falls_Back_To_Stable_Ordering_And_Diagnostic()
        {
            TM59InternalConditionResolver resolver = TM59TestData.NewResolver();
            Space bedroomA = NewSpace("Bedroom A", area: 12.0);
            Space bedroomB = NewSpace("Bedroom B"); // no area at all
            List<Space> flat = new List<Space> { bedroomA, bedroomB };

            TM59InternalConditionResult resultA = resolver.Resolve(bedroomA, flat);
            TM59InternalConditionResult resultB = resolver.Resolve(bedroomB, flat);

            // Stable name ordering picks "Bedroom A", regardless of it being the one WITH area data.
            Assert.Equal("Double Bedroom", resultA.InternalCondition?.Name);
            Assert.Equal("Single Bedroom", resultB.InternalCondition?.Name);
            Assert.Contains("Partial bedroom area data", resultA.Diagnostic);
            Assert.Contains("Partial bedroom area data", resultB.Diagnostic);
        }

        [Fact]
        public void Exact_Tie_Bedroom_Area_Resolves_By_Name_Guid_TieBreak_Without_Throwing()
        {
            TM59InternalConditionResolver resolver = TM59TestData.NewResolver();
            Space bedroomA = NewSpace("Bedroom A", area: 12.0);
            Space bedroomB = NewSpace("Bedroom B", area: 12.0);
            List<Space> flat = new List<Space> { bedroomA, bedroomB };

            TM59InternalConditionResult resultA = resolver.Resolve(bedroomA, flat);
            TM59InternalConditionResult resultB = resolver.Resolve(bedroomB, flat);

            // Both bedrooms have valid (equal) Area, so this is the area-based branch - no diagnostic -
            // and the tie is broken deterministically by name ordering, not by throwing or flip-flopping.
            Assert.Equal("Double Bedroom", resultA.InternalCondition?.Name);
            Assert.Equal("Single Bedroom", resultB.InternalCondition?.Name);
            Assert.Null(resultA.Diagnostic);
            Assert.Null(resultB.Diagnostic);
        }

        [Fact]
        public void Explicit_Bedroom_Keyword_Wins_Over_Inferred_Bedrooms_Which_Both_Default_To_Single()
        {
            // One explicit "Master Bedroom" (Double) alongside two plain, area-only "Bedroom N" spaces:
            // the explicit keyword wins regardless of area, and since only one Double is possible per
            // flat, the other two are both Single - their relative area does not change that outcome.
            TM59InternalConditionResolver resolver = TM59TestData.NewResolver();
            Space master = NewSpace("Master Bedroom", area: 8.0);
            Space bedroom2 = NewSpace("Bedroom 2", area: 20.0);
            Space bedroom3 = NewSpace("Bedroom 3", area: 10.0);
            List<Space> flat = new List<Space> { master, bedroom2, bedroom3 };

            Assert.Equal("Double Bedroom", ResolveName(resolver, master, flat));
            Assert.Equal("Single Bedroom", ResolveName(resolver, bedroom2, flat));
            Assert.Equal("Single Bedroom", ResolveName(resolver, bedroom3, flat));
        }

        // --- TM59Manager.TM59Occupancy(InternalCondition): stateless, name-based table lookup used by
        // SAM_UI post-dialog callers (and to clear stale Occupancy when remapping to non-habitable). ---

        [Fact]
        public void TM59Occupancy_InternalCondition_Matches_Documented_Table_For_Every_Library_Condition()
        {
            Dictionary<string, int> expected = new Dictionary<string, int>
            {
                ["Studio"] = 2,
                ["1 Bed Apt. Living Room/Kitchen"] = 2,
                ["1 Bed Apt. Living Room"] = 2,
                ["1 Bed Apt. Kitchen"] = 2,
                ["2 Bed Apt. Living Room/Kitchen"] = 3,
                ["2 Bed Apt. Living Room"] = 3,
                ["2 Bed Apt. Kitchen"] = 3,
                ["3 Bed Apt. Living Room/Kitchen"] = 4,
                ["3 Bed Apt. Living Room"] = 4,
                ["3 Bed Apt. Kitchen"] = 4,
                ["Double Bedroom"] = 2,
                ["Single Bedroom"] = 1,
                ["TM59_Bathroom"] = 0,
                ["TM59_Internal Corridor"] = 0,
                ["TM59_Communal Corridor (including pipework gains)"] = 0,
                ["TM59_Stairs"] = 0,
                ["TM59_Cupboard/riser/lift/void"] = 0,
                ["TM59_Cupboard with HIU"] = 0,
                ["TM59_Riser Communal pipework"] = 0,
            };

            List<InternalCondition> conditions = TM59TestData.InternalConditionLibrary.GetInternalConditions();
            Assert.Equal(19, conditions.Count);

            foreach (InternalCondition condition in conditions)
            {
                Assert.True(expected.TryGetValue(condition.Name, out int expectedOccupancy),
                    $"Unexpected condition '{condition.Name}' not in the documented occupancy table.");
                Assert.Equal(expectedOccupancy, TM59Manager.TM59Occupancy(condition));
            }
        }

        [Fact]
        public void TM59Occupancy_InternalCondition_Null_Or_Unknown_Name_Returns_Zero()
        {
            Assert.Equal(0, TM59Manager.TM59Occupancy((InternalCondition)null));
            Assert.Equal(0, TM59Manager.TM59Occupancy(new InternalCondition("Unknown")));
        }

        [Fact]
        public void TM59Occupancy_InternalCondition_Lookup_Is_Purely_Name_Based_Not_Space_State()
        {
            // Proves the table SAM_UI relies on to zero out stale Occupancy/AreaPerPerson when
            // remapping a former bedroom to a non-habitable condition is stateless: it depends only
            // on the resolved condition's Name, never on whatever the Space previously held.
            InternalCondition corridor = TM59TestData.InternalConditionLibrary
                .GetInternalConditions("TM59_Communal Corridor (including pipework gains)").First();

            Assert.Equal(0, TM59Manager.TM59Occupancy(corridor));
        }
    }
}
