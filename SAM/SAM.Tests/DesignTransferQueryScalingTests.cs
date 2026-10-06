// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace SAM.Tests
{
    /// <summary>
    /// That <see cref="Analytical.Query.DesignTransferSpaceAirMovements"/> and <see cref="Analytical.Query.DesignTransferFlowRate_Lps"/>
    /// resolve a model's design transfer routes in O(spaces + movements), not O(spaces &#215; movements),
    /// asserted <b>structurally</b> - see <c>PartFIndexScalingTests</c> for why allocated bytes is the right
    /// measure here and why no assertion in this class carries a time limit.
    /// <para>
    /// <b>The defect shape this guards against.</b> Both queries used to resolve a <see cref="SpaceAirMovement"/>
    /// endpoint through <c>AdjacencyCluster.AirMovementEndpoint</c>, which resolves an <c>ObjectReference</c> by
    /// scanning every object of the endpoint's runtime type rather than through an indexed lookup. A caller
    /// resolving many routes - a floor-plan overlay reading one route per adjacency, exactly what
    /// <c>DesignAirFlowFloorPlanOverlay.BuildTransferMarks</c> does - paid that scan again for every route, so
    /// the whole read was quadratic in the number of spaces. Independently measured before the fix: roughly
    /// 22.7 MB at 500 spaces, 86.4 MB at 1000, 336 MB at 2000 and 1.324 GB at 4000 - a growth ratio near 4 at
    /// every doubling, where a linear read produces a ratio near 2.
    /// </para>
    /// </summary>
    public class DesignTransferQueryScalingTests
    {
        private readonly ITestOutputHelper output;

        public DesignTransferQueryScalingTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        // ---- 1. Correctness survives at every size, so the scaling numbers are not measuring nothing -----

        /// <summary>
        /// Every adjacent pair in the chain reports exactly the movement written between them, whichever
        /// order the two guids are asked in - at a size small enough to eyeball and one large enough that a
        /// broken index would have to get very lucky to still agree by chance.
        /// </summary>
        [Theory]
        [InlineData(50)]
        [InlineData(500)]
        [InlineData(5000)]
        public void EveryAdjacentPair_ReportsExactlyTheMovementWritten(int roomCount)
        {
            (AdjacencyCluster adjacencyCluster, List<Space> spaces) = Chain(roomCount);

            Dictionary<(Guid, Guid), Analytical.Query.DesignTransferAirMovement> dictionary = adjacencyCluster.DesignTransferSpaceAirMovements();

            Assert.Equal(roomCount - 1, dictionary.Count);

            for (int i = 1; i < spaces.Count; i++)
            {
                double? flow_Lps = adjacencyCluster.DesignTransferFlowRate_Lps(spaces[i - 1].Guid, spaces[i].Guid, out Guid guid_From, out Guid guid_To, dictionary);

                Assert.NotNull(flow_Lps);
                Assert.Equal(30.0, flow_Lps.Value, 6);
                Assert.Equal(spaces[i - 1].Guid, guid_From);
                Assert.Equal(spaces[i].Guid, guid_To);

                //Order-independent, exactly as the un-indexed convenience overload already guarantees.
                double? flow_Lps_Reversed = adjacencyCluster.DesignTransferFlowRate_Lps(spaces[i].Guid, spaces[i - 1].Guid, out Guid guid_From_Reversed, out Guid guid_To_Reversed, dictionary);

                Assert.Equal(flow_Lps, flow_Lps_Reversed);
                Assert.Equal(guid_From, guid_From_Reversed);
                Assert.Equal(guid_To, guid_To_Reversed);
            }
        }

        // ---- 2. Allocation grows linearly with the model, not quadratically ------------------------------

        /// <summary>
        /// Resolving every adjacency's flow rate through one shared dictionary - exactly what
        /// <c>DesignAirFlowFloorPlanOverlay.BuildTransferMarks</c> does - at the four sizes the review
        /// measured directly: 500, 1000, 2000 and 4000 spaces. The pre-fix query roughly quadrupled its
        /// allocation at every doubling; this asserts it now stays near double.
        /// </summary>
        [Fact]
        public void ResolvingEveryRoute_AllocatesLinearlyWithTheModel()
        {
            int[] counts = [500, 1000, 2000, 4000];

            List<long> allocated = [];

            foreach (int count in counts)
            {
                (AdjacencyCluster adjacencyCluster, _) = Chain(count);

                //Warmed, so the first size measured is not paying for the JIT of everything below it.
                ResolveEveryRoute(adjacencyCluster);

                allocated.Add(Allocated(() => ResolveEveryRoute(adjacencyCluster)));
            }

            for (int i = 0; i < counts.Length; i++)
            {
                output.WriteLine("spaces={0,5}  allocated={1,14:N0} bytes", counts[i], allocated[i]);
            }

            for (int i = 1; i < counts.Length; i++)
            {
                double ratio = (double)allocated[i] / allocated[i - 1];

                output.WriteLine("{0} -> {1}: x{2:0.00}", counts[i - 1], counts[i], ratio);

                Assert.True(
                    ratio < 2.6,
                    string.Format(
                        "Doubling the chain from {0} to {1} spaces multiplied the design transfer query's allocation by {2:0.00}. Linear work sits near 2 and quadratic work near 4, so an endpoint is being resolved by a whole-model scan again.",
                        counts[i - 1],
                        counts[i],
                        ratio));
            }
        }

        /// <summary>Every adjacency's flow rate, resolved through one shared dictionary - the read a floor-plan overlay performs once per route.</summary>
        private static void ResolveEveryRoute(AdjacencyCluster adjacencyCluster)
        {
            Dictionary<(Guid, Guid), Analytical.Query.DesignTransferAirMovement> dictionary = adjacencyCluster.DesignTransferSpaceAirMovements();

            foreach (Analytical.Query.DesignTransferAirMovement designTransferAirMovement in dictionary.Values)
            {
                adjacencyCluster.DesignTransferFlowRate_Lps(designTransferAirMovement.FromGuid, designTransferAirMovement.ToGuid, out _, out _, dictionary);
            }
        }

        // ---- 3. Timings, reported and asserted on by nothing -----------------------------------------------

        /// <summary>
        /// Local wall clock, for the report. <b>Evidence, not a contract</b> - this asserts nothing about
        /// time, and a slow machine does not make it fail.
        /// </summary>
        [Fact]
        [Trait("Category", "Benchmark")]
        public void Benchmark()
        {
            output.WriteLine("{0,6} {1,14}", "spaces", "resolve (ms)");

            foreach (int count in new[] { 500, 1000, 2000, 4000, 5000 })
            {
                (AdjacencyCluster adjacencyCluster, _) = Chain(count);

                ResolveEveryRoute(adjacencyCluster);

                Stopwatch stopwatch = Stopwatch.StartNew();
                ResolveEveryRoute(adjacencyCluster);
                stopwatch.Stop();

                output.WriteLine("{0,6} {1,14:0.0}", count, stopwatch.Elapsed.TotalMilliseconds);
            }
        }

        // ---- Fixture ----------------------------------------------------------------------------------------

        /// <summary>
        /// A chain of <paramref name="roomCount"/> spaces, each with one <see cref="SpaceAirMovement"/> design
        /// transfer route into the next. No geometry at all - the query under test reads neither panels nor
        /// apertures - and the spaces are returned in the exact order they were created rather than re-found by
        /// name, so the fixture itself has no O(n) scan to confound what is being measured.
        /// </summary>
        private static (AdjacencyCluster AdjacencyCluster, List<Space> Spaces) Chain(int roomCount)
        {
            AdjacencyCluster adjacencyCluster = new();

            List<Space> spaces = [];

            for (int i = 0; i < roomCount; i++)
            {
                Space space = new(string.Format("Room {0:00000}", i));

                space.SetValue(SpaceParameter.Area, 10.0);
                space.SetValue(SpaceParameter.Volume, 25.0);

                adjacencyCluster.AddObject(space);
                spaces.Add(space);
            }

            for (int i = 1; i < roomCount; i++)
            {
                adjacencyCluster.AddObject(new SpaceAirMovement(
                    string.Format("Transfer {0:00000}", i),
                    0.03,
                    new ObjectReference(spaces[i - 1]).ToString(),
                    new ObjectReference(spaces[i]).ToString()));
            }

            return (adjacencyCluster, spaces);
        }

        private static long Allocated(Action action)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            long before = GC.GetAllocatedBytesForCurrentThread();

            action();

            return GC.GetAllocatedBytesForCurrentThread() - before;
        }
    }
}
