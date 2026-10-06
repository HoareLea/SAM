// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020-2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Analytical.Enums;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// <see cref="PartFIndex"/> against the authority it accelerates.
    ///
    /// <para><b>The oracle is the existing one-space query</b></para>
    /// <para>
    /// <c>Query.PartFRequiredFlowRate_Lps(AdjacencyCluster, Space, FlowClassification)</c> is what every
    /// caller asked before this index existed, and it is what these tests compare against - not a restated
    /// expectation. A comparison against a number written into the test would only prove the test and the
    /// index agree; comparing against the query proves the index did not change what Approved Document F
    /// requires of any room in any model this suite can build.
    /// </para>
    /// <para>
    /// Compared with <c>Assert.Equal</c> on <c>double?</c> - <b>exact equality, not a tolerance</b>. The
    /// index performs no arithmetic of its own: it finds the same space and calls the same reader, so
    /// anything but a bit-identical answer is a defect rather than a rounding difference.
    /// </para>
    ///
    /// <para><b>What the edge cases here pin, and why they are pinned rather than chosen</b></para>
    /// <para>
    /// Several of them - a space the model does not carry, a rate of NaN, a direction that is neither supply
    /// nor extract - have a defined answer in the one-space query that is not obviously the only reasonable
    /// one. Each is asserted against that query rather than against a preference, so this file records what
    /// SAM does and refuses to let the index quietly do something else.
    /// </para>
    /// </summary>
    public class PartFIndexTests
    {
        // ---- Fixtures ----------------------------------------------------------------------------------

        /// <summary>
        /// A synthetic dwelling model of <paramref name="count"/> spaces, deliberately mixed so a sweep over
        /// it exercises every answer the query has: sized supply rooms, sized extract rooms, rooms sized on
        /// both sides, rooms carrying no Approved Document F data at all, a rate of exactly zero, a negative
        /// rate and a NaN.
        /// </summary>
        internal static AdjacencyCluster Model(int count, int spacesPerDwelling = 5)
        {
            AdjacencyCluster adjacencyCluster = new();

            List<Space> spaces = [];

            for (int i = 0; i < count; i++)
            {
                Space space = new(string.Format("Space {0}", i), new Point3D(i * 10, 0, 1.5));
                space.SetValue(SpaceParameter.Area, 12.0 + (i % 7));
                space.SetValue(SpaceParameter.Volume, 30.0 + (i % 11));

                //Every seventh room carries no Part F data at all - a corridor, a store, a plant room. The
                //query answers null for those, which is not a requirement of zero, and a bulk sweep has to
                //keep answering null.
                if (i % 7 != 6)
                {
                    space.SetValue(SpaceParameter.PartFSpaceData, PartFSpaceData(i));
                }

                adjacencyCluster.AddObject(space);
                spaces.Add(space);
            }

            for (int i = 0; i < count; i += spacesPerDwelling)
            {
                Zone zone = new(string.Format("Flat {0}", i / spacesPerDwelling));
                zone.SetValue(ZoneParameter.ZoneCategory, "Flats");
                zone.SetValue(ZoneParameter.IsDwelling, true);

                adjacencyCluster.AddObject(zone);

                for (int j = i; j < System.Math.Min(i + spacesPerDwelling, count); j++)
                {
                    adjacencyCluster.AddRelation(zone, spaces[j]);
                }
            }

            return adjacencyCluster;
        }

        private static PartFSpaceData PartFSpaceData(int i)
        {
            PartFSpaceData result = new();

            //Parenthesised deliberately: a switch expression binds tighter than %, so "i % 6 switch {...}"
            //is "i % (6 switch {...})" and every rate below would be silently ignored.
            double rate = (i % 6) switch
            {
                0 => 8.5,
                1 => 13.0,
                2 => 0.0,           //sized at exactly zero: an answer of 0, never null.
                3 => -4.0,          //a negative rate is not manufactured here; it is what the model says.
                4 => double.NaN,    //the query maps NaN to null. Pinned, not assumed.
                _ => 6.25,
            };

            if (i % 3 != 1)
            {
                result.Terminals.Add(new PartFVentilationTerminalRequirement(string.Format("Supply {0}", i), Guid.NewGuid(), PartFTerminalRole.Supply)
                {
                    ContinuousDesignFlowRate_Lps = rate,
                    IsRequired = true,
                });
            }

            if (i % 3 != 0)
            {
                result.Terminals.Add(new PartFVentilationTerminalRequirement(string.Format("Extract {0}", i), Guid.NewGuid(), PartFTerminalRole.GeneralExtract)
                {
                    ContinuousDesignFlowRate_Lps = rate + 1.5,
                    IsRequired = true,
                });
            }

            //A subdivided room: two supply terminals whose rates sum to the room requirement.
            if (i % 13 == 5)
            {
                result.Terminals.Add(new PartFVentilationTerminalRequirement(string.Format("Supply B {0}", i), Guid.NewGuid(), PartFTerminalRole.Supply)
                {
                    ContinuousDesignFlowRate_Lps = 3.75,
                    IsRequired = true,
                });
            }

            return result;
        }

        private static Space Space(AdjacencyCluster adjacencyCluster, string name)
        {
            return (adjacencyCluster.GetSpaces() ?? []).Find(x => x.Name == name);
        }

        // ---- Oracle equivalence ------------------------------------------------------------------------

        /// <summary>
        /// Every space of the model, both directions, at every size the real projects reach: the index and
        /// the one-space query give the same answer, exactly.
        /// </summary>
        [Theory]
        [InlineData(50)]
        [InlineData(200)]
        [InlineData(500)]
        [InlineData(1000)]
        [InlineData(5000)]
        public void PartFIndex_AnswersExactlyWhatTheOneSpaceQueryAnswers(int count)
        {
            AdjacencyCluster adjacencyCluster = Model(count);

            PartFIndex partFIndex = new(adjacencyCluster);

            List<Space> spaces = adjacencyCluster.GetSpaces();

            Assert.Equal(count, spaces.Count);
            Assert.Equal(count, partFIndex.Count);

            int count_Sized = 0;

            foreach (Space space in spaces)
            {
                foreach (FlowClassification flowClassification in new[] { FlowClassification.Supply, FlowClassification.Extract })
                {
                    double? expected = Analytical.Query.PartFRequiredFlowRate_Lps(adjacencyCluster, space, flowClassification);

                    Assert.Equal(expected, partFIndex.PartFRequiredFlowRate_Lps(space, flowClassification));

                    if (expected.HasValue)
                    {
                        count_Sized++;
                    }
                }
            }

            //The sweep is only evidence if it actually reached sized rooms, unsized rooms and both
            //directions. A fixture that had quietly stopped carrying Part F data would pass every assertion
            //above by comparing null with null.
            Assert.True(count_Sized > count, string.Format("The fixture produced only {0} sized answers over {1} spaces, so the comparison proved nothing.", count_Sized, count));
        }

        /// <summary>
        /// The same sweep asked through the index a second time gives the same answers - nothing about the
        /// first traversal changed what the second one reads.
        /// </summary>
        [Fact]
        public void PartFIndex_IsReadOnly_ASecondSweepAnswersTheSame()
        {
            AdjacencyCluster adjacencyCluster = Model(200);

            PartFIndex partFIndex = new(adjacencyCluster);

            List<double?> first = [];
            List<double?> second = [];

            foreach (Space space in partFIndex.Spaces)
            {
                first.Add(partFIndex.PartFRequiredFlowRate_Lps(space, FlowClassification.Supply));
                first.Add(partFIndex.PartFRequiredFlowRate_Lps(space, FlowClassification.Extract));
            }

            foreach (Space space in partFIndex.Spaces)
            {
                second.Add(partFIndex.PartFRequiredFlowRate_Lps(space, FlowClassification.Supply));
                second.Add(partFIndex.PartFRequiredFlowRate_Lps(space, FlowClassification.Extract));
            }

            Assert.Equal(first, second);

            //And the model was not touched: the one-space query still says the same thing.
            foreach (Space space in adjacencyCluster.GetSpaces())
            {
                Assert.Equal(
                    Analytical.Query.PartFRequiredFlowRate_Lps(adjacencyCluster, space, FlowClassification.Supply),
                    partFIndex.PartFRequiredFlowRate_Lps(space, FlowClassification.Supply));
            }
        }

        /// <summary>
        /// <b>No rate is held.</b> A requirement rewritten on the model after the index was built is read
        /// through the index immediately - because the index resolves an identity and then reads the space,
        /// rather than remembering what the space said.
        /// <para>
        /// This is the assertion that separates an index from a cache, and it is the one that would fail
        /// first if anybody ever memoised a rate on it.
        /// </para>
        /// </summary>
        [Fact]
        public void PartFIndex_HoldsNoRate_ARewrittenRequirementIsSeenImmediately()
        {
            AdjacencyCluster adjacencyCluster = Model(20);

            PartFIndex partFIndex = new(adjacencyCluster);

            Space space = Space(adjacencyCluster, "Space 0");

            Assert.Equal(8.5, partFIndex.PartFRequiredFlowRate_Lps(space, FlowClassification.Supply));

            PartFSpaceData partFSpaceData = space.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData);
            partFSpaceData.Terminals.Find(x => x.TerminalRole == PartFTerminalRole.Supply).ContinuousDesignFlowRate_Lps = 21.0;
            space.SetValue(SpaceParameter.PartFSpaceData, partFSpaceData);

            Assert.Equal(21.0, partFIndex.PartFRequiredFlowRate_Lps(space, FlowClassification.Supply));

            //And still the same thing the one-space query says.
            Assert.Equal(
                Analytical.Query.PartFRequiredFlowRate_Lps(adjacencyCluster, space, FlowClassification.Supply),
                partFIndex.PartFRequiredFlowRate_Lps(space, FlowClassification.Supply));
        }

        // ---- Edge cases, each pinned against the one-space query ---------------------------------------

        /// <summary>No model at all answers nothing, and never falls back onto the caller's own space.</summary>
        [Fact]
        public void NullModel_AnswersNothing_NotTheHandedInSpace()
        {
            AdjacencyCluster adjacencyCluster = Model(5);

            Space space = Space(adjacencyCluster, "Space 0");

            Assert.NotNull(space.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData));

            PartFIndex partFIndex = new(null);

            Assert.Equal(0, partFIndex.Count);
            Assert.Null(partFIndex.AdjacencyCluster);

            Assert.Null(Analytical.Query.PartFRequiredFlowRate_Lps(null, space, FlowClassification.Supply));
            Assert.Null(partFIndex.PartFRequiredFlowRate_Lps(space, FlowClassification.Supply));

            Assert.Empty(partFIndex.Spaces);
            Assert.Empty(partFIndex.Spaces_Zones([new Zone("Flat 0")]));
        }

        /// <summary>A null space answers nothing on both forms.</summary>
        [Fact]
        public void NullSpace_AnswersNothing()
        {
            AdjacencyCluster adjacencyCluster = Model(5);

            PartFIndex partFIndex = new(adjacencyCluster);

            Assert.Null(Analytical.Query.PartFRequiredFlowRate_Lps(adjacencyCluster, null, FlowClassification.Supply));
            Assert.Null(partFIndex.PartFRequiredFlowRate_Lps(null, FlowClassification.Supply));
            Assert.Null(partFIndex.Space((Space)null));
        }

        /// <summary>
        /// A model with no spaces at all. The index is empty and answers exactly what the query answers.
        /// </summary>
        [Fact]
        public void EmptyModel_IsEmptyAndAgreesWithTheQuery()
        {
            AdjacencyCluster adjacencyCluster = new();

            Space space = new("Detached", new Point3D(0, 0, 0));
            space.SetValue(SpaceParameter.PartFSpaceData, PartFSpaceData(0));

            PartFIndex partFIndex = new(adjacencyCluster);

            Assert.Equal(0, partFIndex.Count);
            Assert.Empty(partFIndex.Spaces);
            Assert.Null(partFIndex.Space(space.Guid));

            //GetSpaces() on an empty cluster returns null, the query falls back to the handed-in space, and
            //so must the index. Pinned as the query's behaviour, not chosen here.
            Assert.Equal(
                Analytical.Query.PartFRequiredFlowRate_Lps(adjacencyCluster, space, FlowClassification.Supply),
                partFIndex.PartFRequiredFlowRate_Lps(space, FlowClassification.Supply));
        }

        /// <summary>
        /// A space the model does not carry is answered from the instance handed in - the query's behaviour,
        /// pinned here so the index cannot silently start refusing it.
        /// </summary>
        [Fact]
        public void SpaceNotInTheModel_IsAnsweredFromTheInstanceHandedIn()
        {
            AdjacencyCluster adjacencyCluster = Model(10);

            Space space_Detached = new("Somebody else's room", new Point3D(999, 0, 0));
            space_Detached.SetValue(SpaceParameter.PartFSpaceData, PartFSpaceData(0));

            PartFIndex partFIndex = new(adjacencyCluster);

            Assert.Null(partFIndex.Space(space_Detached.Guid));
            Assert.Same(space_Detached, partFIndex.Space(space_Detached));

            Assert.Equal(
                Analytical.Query.PartFRequiredFlowRate_Lps(adjacencyCluster, space_Detached, FlowClassification.Supply),
                partFIndex.PartFRequiredFlowRate_Lps(space_Detached, FlowClassification.Supply));

            Assert.Equal(8.5, partFIndex.PartFRequiredFlowRate_Lps(space_Detached, FlowClassification.Supply));
        }

        /// <summary>
        /// A stale copy of a space the model DOES carry is answered from the model's instance, not the
        /// copy's. This is the whole reason the resolution exists, and it is the property the index had to
        /// preserve while making it cheap.
        /// </summary>
        [Fact]
        public void StaleCopyOfAModelSpace_IsAnsweredFromTheModel()
        {
            AdjacencyCluster adjacencyCluster = Model(10);

            Space space_Cluster = Space(adjacencyCluster, "Space 0");

            //Same identity, different Part F data - a caller holding a space from before the rates were
            //applied.
            Space space_Stale = new(space_Cluster);

            PartFSpaceData partFSpaceData_Stale = new();
            partFSpaceData_Stale.Terminals.Add(new PartFVentilationTerminalRequirement("Stale", Guid.NewGuid(), PartFTerminalRole.Supply)
            {
                ContinuousDesignFlowRate_Lps = 999.0,
            });

            space_Stale.SetValue(SpaceParameter.PartFSpaceData, partFSpaceData_Stale);

            Assert.Equal(space_Cluster.Guid, space_Stale.Guid);

            PartFIndex partFIndex = new(adjacencyCluster);

            Assert.Same(space_Cluster, partFIndex.Space(space_Stale));

            Assert.Equal(
                Analytical.Query.PartFRequiredFlowRate_Lps(adjacencyCluster, space_Stale, FlowClassification.Supply),
                partFIndex.PartFRequiredFlowRate_Lps(space_Stale, FlowClassification.Supply));

            Assert.Equal(8.5, partFIndex.PartFRequiredFlowRate_Lps(space_Stale, FlowClassification.Supply));
        }

        /// <summary>A space carrying no Approved Document F data answers null - not zero.</summary>
        [Fact]
        public void UnsizedSpace_AnswersNull_NotZero()
        {
            AdjacencyCluster adjacencyCluster = Model(10);

            Space space = Space(adjacencyCluster, "Space 6");

            Assert.Null(space.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData));

            PartFIndex partFIndex = new(adjacencyCluster);

            Assert.Null(Analytical.Query.PartFRequiredFlowRate_Lps(adjacencyCluster, space, FlowClassification.Supply));
            Assert.Null(partFIndex.PartFRequiredFlowRate_Lps(space, FlowClassification.Supply));
        }

        /// <summary>
        /// Zero and a negative rate, each answered exactly as the one-space query answers it: zero is a
        /// rate and never null, and a negative rate is passed through as the model states it rather than
        /// clamped. Neither is invented here - both are read off the same fixture the query reads.
        /// </summary>
        [Theory]
        [InlineData("Space 2", 0.0)]
        [InlineData("Space 3", -4.0)]
        public void ZeroAndNegativeRates_AreAnsweredExactlyAsTheQueryAnswersThem(string name, double expected)
        {
            AdjacencyCluster adjacencyCluster = Model(10);

            Space space = Space(adjacencyCluster, name);

            PartFIndex partFIndex = new(adjacencyCluster);

            Assert.Equal(expected, Analytical.Query.PartFRequiredFlowRate_Lps(adjacencyCluster, space, FlowClassification.Supply));
            Assert.Equal(expected, partFIndex.PartFRequiredFlowRate_Lps(space, FlowClassification.Supply));
        }

        /// <summary>
        /// A room whose Approved Document F data holds NaN answers null, not NaN - the query's mapping, and
        /// the index has to keep making it. Asserted with the raw data checked first, so this cannot pass
        /// because the fixture quietly stopped carrying a NaN.
        /// </summary>
        [Fact]
        public void NaNRate_AnswersNull()
        {
            AdjacencyCluster adjacencyCluster = Model(10);

            Space space = Space(adjacencyCluster, "Space 4");

            PartFSpaceData partFSpaceData = space.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData);

            Assert.True(double.IsNaN(partFSpaceData.ContinuousExtractFlowRate_Lps.Value));

            PartFIndex partFIndex = new(adjacencyCluster);

            Assert.Null(Analytical.Query.PartFRequiredFlowRate_Lps(adjacencyCluster, space, FlowClassification.Extract));
            Assert.Null(partFIndex.PartFRequiredFlowRate_Lps(space, FlowClassification.Extract));
        }

        /// <summary>
        /// A direction that is neither supply nor extract answers null on both forms. Approved Document F
        /// sizes two directions and there is no third to report.
        /// </summary>
        [Fact]
        public void UndefinedDirection_AnswersNull()
        {
            AdjacencyCluster adjacencyCluster = Model(10);

            Space space = Space(adjacencyCluster, "Space 0");

            PartFIndex partFIndex = new(adjacencyCluster);

            Assert.Null(Analytical.Query.PartFRequiredFlowRate_Lps(adjacencyCluster, space, FlowClassification.Undefined));
            Assert.Null(partFIndex.PartFRequiredFlowRate_Lps(space, FlowClassification.Undefined));
        }

        /// <summary>
        /// A subdivided room's requirement is the room's whole requirement, summed across its terminals -
        /// the same sum the one-space query produces, reached the same way.
        /// </summary>
        [Fact]
        public void SubdividedRoom_SumsToTheSameRoomRequirement()
        {
            AdjacencyCluster adjacencyCluster = Model(20);

            Space space = Space(adjacencyCluster, "Space 5");

            PartFIndex partFIndex = new(adjacencyCluster);

            //Space 5: one 6.25 l/s supply terminal plus the 3.75 l/s subdivision.
            Assert.Equal(10.0, partFIndex.PartFRequiredFlowRate_Lps(space, FlowClassification.Supply));

            Assert.Equal(
                Analytical.Query.PartFRequiredFlowRate_Lps(adjacencyCluster, space, FlowClassification.Supply),
                partFIndex.PartFRequiredFlowRate_Lps(space, FlowClassification.Supply));
        }

        // ---- Dwelling scope ----------------------------------------------------------------------------

        /// <summary>
        /// A dwelling scope resolved through the index is the model's current space instances, once each,
        /// in zone then relation order.
        /// </summary>
        [Fact]
        public void Spaces_Zones_ResolvesTheModelsOwnInstancesInOrder()
        {
            AdjacencyCluster adjacencyCluster = Model(20);

            List<Zone> zones = adjacencyCluster.GetZones();
            zones.Sort((x, y) => string.CompareOrdinal(x.Name, y.Name));

            PartFIndex partFIndex = new(adjacencyCluster);

            List<Space> spaces = partFIndex.Spaces_Zones(zones);

            Assert.Equal(20, spaces.Count);

            foreach (Space space in spaces)
            {
                Assert.Same(partFIndex.Space(space.Guid), space);
            }

            //Zone order, then relation order.
            Assert.Equal("Space 0", spaces[0].Name);
            Assert.Equal("Space 4", spaces[4].Name);
            Assert.Equal("Space 5", spaces[5].Name);
        }

        /// <summary>
        /// Two zones that are the same dwelling by every visible property - same name, same category, both
        /// marked - and that share rooms. The shared rooms appear once, and identity decides that, not name.
        /// </summary>
        [Fact]
        public void EquivalentDwellingZonesSharingRooms_ResolveEachRoomOnce()
        {
            AdjacencyCluster adjacencyCluster = Model(10, 10);

            List<Space> spaces = adjacencyCluster.GetSpaces();

            Zone zone_Duplicate = new("Flat 0");
            zone_Duplicate.SetValue(ZoneParameter.ZoneCategory, "Flats");
            zone_Duplicate.SetValue(ZoneParameter.IsDwelling, true);

            adjacencyCluster.AddObject(zone_Duplicate);

            foreach (Space space in spaces)
            {
                adjacencyCluster.AddRelation(zone_Duplicate, space);
            }

            List<Zone> zones = adjacencyCluster.GetZones();

            Assert.Equal(2, zones.Count);
            Assert.Equal(2, Analytical.Query.PartFDwellingZones(zones).Count);

            PartFIndex partFIndex = new(adjacencyCluster);

            //Both zones name the same ten rooms. Ten spaces come back, not twenty.
            Assert.Equal(10, partFIndex.Spaces_Zones(zones).Count);
        }

        /// <summary>
        /// A zone naming a space the model no longer carries. That space is dropped rather than returned as
        /// a stale instance - a scope is what the model has, not what a relation remembers.
        /// </summary>
        [Fact]
        public void ZoneNamingARemovedSpace_DropsIt()
        {
            AdjacencyCluster adjacencyCluster = Model(10, 10);

            Space space_Removed = Space(adjacencyCluster, "Space 3");

            Assert.True(adjacencyCluster.RemoveObject<Space>(space_Removed.Guid));

            PartFIndex partFIndex = new(adjacencyCluster);

            List<Space> spaces = partFIndex.Spaces_Zones(adjacencyCluster.GetZones());

            Assert.Equal(9, spaces.Count);
            Assert.DoesNotContain(spaces, x => x.Guid == space_Removed.Guid);
        }

        /// <summary>An empty scope resolves to nothing, and is not read as "the whole model".</summary>
        [Fact]
        public void EmptyScope_ResolvesToNothing()
        {
            AdjacencyCluster adjacencyCluster = Model(10);

            PartFIndex partFIndex = new(adjacencyCluster);

            Assert.Empty(partFIndex.Spaces_Zones([]));
            Assert.Empty(partFIndex.Spaces_Zones(null));
            Assert.Empty(partFIndex.Spaces_Zones([null]));
        }

        /// <summary>
        /// The index reports the model's spaces in the cluster's own order, so a caller that used to hold
        /// <c>GetSpaces()</c> can hold this instead without its results reordering.
        /// </summary>
        [Fact]
        public void Spaces_MatchesGetSpaces()
        {
            AdjacencyCluster adjacencyCluster = Model(100);

            PartFIndex partFIndex = new(adjacencyCluster);

            List<Space> spaces_Cluster = adjacencyCluster.GetSpaces();
            List<Space> spaces_Index = partFIndex.Spaces;

            Assert.Equal(spaces_Cluster.Count, spaces_Index.Count);

            for (int i = 0; i < spaces_Cluster.Count; i++)
            {
                Assert.Same(spaces_Cluster[i], spaces_Index[i]);
            }

            //A fresh list every time, so a caller cannot mutate the index through it.
            Assert.NotSame(partFIndex.Spaces, partFIndex.Spaces);
        }
    }
}
