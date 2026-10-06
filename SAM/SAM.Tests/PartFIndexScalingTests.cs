// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020-2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace SAM.Tests
{
    /// <summary>
    /// That a bulk Approved Document F aggregation scales, asserted <b>structurally</b>.
    ///
    /// <para><b>No test here has a time limit, and that is deliberate</b></para>
    /// <para>
    /// "Must finish in under 500 ms" is not a statement about this code - it is a statement about the
    /// machine, the build configuration and whatever else the runner happened to be doing, and it fails on a
    /// loaded CI box for reasons no reviewer can act on. What these assert instead is <b>work</b>: how many
    /// times a bulk traversal resolves a space, how many snapshots it builds, and how the memory it churns
    /// grows as the model doubles. Those are properties of the algorithm, and they are the same on every
    /// machine.
    /// </para>
    /// <para>
    /// The wall clock does appear once, in <see cref="Benchmark"/>, which asserts nothing at all and exists
    /// to print numbers a person can read. It is marked as a benchmark so it can be excluded from a run.
    /// </para>
    ///
    /// <para><b>Why allocated bytes is the right shape measurement here</b></para>
    /// <para>
    /// The defect this work removed was <c>AdjacencyCluster.GetSpaces()</c> rebuilding the model's whole
    /// space list inside a loop over that same list. A rebuilt list is an allocation of <c>O(spaces)</c>, so
    /// doing it per space allocates <c>O(spaces²)</c> bytes and doing it once allocates <c>O(spaces)</c>.
    /// <c>GC.GetAllocatedBytesForCurrentThread</c> counts exactly that, deterministically, without any
    /// dependence on how fast the machine is or when a collection happens to run - so a doubling of the model
    /// that roughly doubles the bytes is linear, and one that roughly quadruples them is not.
    /// </para>
    /// </summary>
    public class PartFIndexScalingTests
    {
        private readonly ITestOutputHelper _output;

        public PartFIndexScalingTests(ITestOutputHelper output)
        {
            _output = output;
        }

        /// <summary>
        /// An index that reports what a bulk caller actually asked of it - the direct operation count the
        /// scaling claim rests on. It changes no answer: every method calls its base.
        /// </summary>
        private sealed class CountingPartFIndex : PartFIndex
        {
            internal static int Constructions;

            internal int Resolutions;

            internal int Requirements;

            internal CountingPartFIndex(AdjacencyCluster adjacencyCluster)
                : base(adjacencyCluster)
            {
                Constructions++;
            }

            public override Space Space(Guid guid)
            {
                Resolutions++;

                return base.Space(guid);
            }

            public override double? PartFRequiredFlowRate_Lps(Space space, FlowClassification flowClassification)
            {
                Requirements++;

                return base.PartFRequiredFlowRate_Lps(space, flowClassification);
            }
        }

        /// <summary>
        /// The aggregation every bulk caller performs: the dwelling scope, then a supply and an extract
        /// requirement for each of its rooms.
        /// </summary>
        private static double Aggregate(PartFIndex partFIndex, List<Zone> zones)
        {
            double result = 0;

            foreach (Space space in partFIndex.Spaces_Zones(zones))
            {
                result += partFIndex.PartFRequiredFlowRate_Lps(space, FlowClassification.Supply) ?? 0;
                result += partFIndex.PartFRequiredFlowRate_Lps(space, FlowClassification.Extract) ?? 0;
            }

            return result;
        }

        private static double Aggregate_Oracle(AdjacencyCluster adjacencyCluster, List<Zone> zones)
        {
            double result = 0;

            //Exactly what the callers did before the index existed: resolve the scope against the model, and
            //ask the one-space query twice per room.
            Dictionary<Guid, Space> dictionary = [];
            foreach (Space space in adjacencyCluster.GetSpaces() ?? [])
            {
                dictionary[space.Guid] = space;
            }

            HashSet<Guid> guids = [];

            foreach (Zone zone in zones)
            {
                foreach (Space space in adjacencyCluster.GetRelatedObjects<Space>(zone) ?? [])
                {
                    if (!guids.Add(space.Guid) || !dictionary.TryGetValue(space.Guid, out Space space_Current))
                    {
                        continue;
                    }

                    result += Analytical.Query.PartFRequiredFlowRate_Lps(adjacencyCluster, space_Current, FlowClassification.Supply) ?? 0;
                    result += Analytical.Query.PartFRequiredFlowRate_Lps(adjacencyCluster, space_Current, FlowClassification.Extract) ?? 0;
                }
            }

            return result;
        }

        // ---- 1. The bulk answer is the oracle's answer, at every size -----------------------------------

        /// <summary>
        /// The whole-model aggregation through one index equals the aggregation through the one-space query,
        /// exactly, at every size the real projects reach.
        /// </summary>
        [Theory]
        [InlineData(50)]
        [InlineData(200)]
        [InlineData(500)]
        [InlineData(1000)]
        [InlineData(5000)]
        public void BulkAggregation_EqualsTheOracleAggregation(int count)
        {
            AdjacencyCluster adjacencyCluster = PartFIndexTests.Model(count);

            List<Zone> zones = adjacencyCluster.GetZones();

            double expected = Aggregate_Oracle(adjacencyCluster, zones);

            Assert.Equal(expected, Aggregate(new PartFIndex(adjacencyCluster), zones));

            //Not a vacuous comparison: the fixture really does size rooms.
            Assert.True(System.Math.Abs(expected) > count / 2.0, string.Format("The aggregate over {0} spaces was {1}, which is too small for the comparison to have exercised anything.", count, expected));
        }

        // ---- 2. One snapshot, and a linear number of resolutions ----------------------------------------

        /// <summary>
        /// <b>Built once, and asked a linear number of questions.</b> One index serves the whole traversal:
        /// exactly two requirements per room in scope, exactly one identity resolution behind each of them,
        /// and no second snapshot built anywhere.
        /// <para>
        /// This is the assertion that would fail if anybody put the index construction back inside the loop,
        /// however fast the machine happened to be.
        /// </para>
        /// </summary>
        [Theory]
        [InlineData(50)]
        [InlineData(200)]
        [InlineData(500)]
        [InlineData(1000)]
        [InlineData(5000)]
        public void BulkAggregation_BuildsOneSnapshotAndResolvesLinearly(int count)
        {
            AdjacencyCluster adjacencyCluster = PartFIndexTests.Model(count);

            List<Zone> zones = adjacencyCluster.GetZones();

            CountingPartFIndex.Constructions = 0;

            CountingPartFIndex partFIndex = new(adjacencyCluster);

            Aggregate(partFIndex, zones);

            Assert.Equal(1, CountingPartFIndex.Constructions);

            //Two directions per room of the scope, and nothing else.
            Assert.Equal(count * 2, partFIndex.Requirements);

            //One resolution to place each room of the scope, and one behind each requirement. Linear in the
            //scope and independent of how many spaces the model holds.
            Assert.Equal(count * 3, partFIndex.Resolutions);
        }

        // ---- 3. The memory the traversal churns grows linearly, not quadratically -----------------------

        private static long Allocated(Action action)
        {
            //Warmed first, so the measurement is the work and not the JIT.
            action();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            long before = GC.GetAllocatedBytesForCurrentThread();

            action();

            return GC.GetAllocatedBytesForCurrentThread() - before;
        }

        /// <summary>
        /// Doubling the model roughly doubles the memory a bulk aggregation churns. A traversal that rebuilt
        /// the model's space list per room would roughly quadruple it, which is what the numbers printed
        /// beside the assertion show the one-space query still doing.
        /// <para>
        /// <b>2.6, over two doublings.</b> Linear work sits at 2 and quadratic work at 4, so the margin is
        /// wide in both directions - this cannot pass by accident and it cannot fail because a runner was
        /// busy.
        /// </para>
        /// </summary>
        [Fact]
        public void BulkAggregation_AllocationGrowsLinearlyWithTheModel()
        {
            int[] counts = [400, 800, 1600];

            List<long> allocated_Index = [];
            List<long> allocated_Oracle = [];

            foreach (int count in counts)
            {
                AdjacencyCluster adjacencyCluster = PartFIndexTests.Model(count);

                List<Zone> zones = adjacencyCluster.GetZones();

                allocated_Index.Add(Allocated(() => Aggregate(new PartFIndex(adjacencyCluster), zones)));
                allocated_Oracle.Add(Allocated(() => Aggregate_Oracle(adjacencyCluster, zones)));
            }

            for (int i = 0; i < counts.Length; i++)
            {
                _output.WriteLine("n={0,5}  index={1,12:N0} bytes  one-space query={2,14:N0} bytes", counts[i], allocated_Index[i], allocated_Oracle[i]);
            }

            for (int i = 1; i < counts.Length; i++)
            {
                double ratio_Index = (double)allocated_Index[i] / allocated_Index[i - 1];
                double ratio_Oracle = (double)allocated_Oracle[i] / allocated_Oracle[i - 1];

                _output.WriteLine("{0} -> {1}: index x{2:0.00}, one-space query x{3:0.00}", counts[i - 1], counts[i], ratio_Index, ratio_Oracle);

                Assert.True(
                    ratio_Index < 2.6,
                    string.Format("Doubling the model from {0} to {1} spaces multiplied the bulk aggregation's allocation by {2:0.00}. Linear work sits near 2 and quadratic work near 4, so something is traversing the whole model per space again.", counts[i - 1], counts[i], ratio_Index));
            }
        }

        // ---- 4. Timings, reported and asserted on by nothing ---------------------------------------------

        /// <summary>
        /// Local wall clock, for the report. <b>Evidence, not a contract</b> - this asserts nothing about
        /// time, and a slow machine does not make it fail.
        /// </summary>
        [Fact]
        [Trait("Category", "Benchmark")]
        public void Benchmark()
        {
            _output.WriteLine("{0,6} {1,14} {2,14}", "spaces", "one-space (ms)", "index (ms)");

            foreach (int count in new[] { 50, 200, 500, 1000, 5000 })
            {
                AdjacencyCluster adjacencyCluster = PartFIndexTests.Model(count);

                List<Zone> zones = adjacencyCluster.GetZones();

                //Warmed, so the first size measured is not paying for the JIT of every method below it.
                Aggregate_Oracle(adjacencyCluster, zones);
                Aggregate(new PartFIndex(adjacencyCluster), zones);

                Stopwatch stopwatch = Stopwatch.StartNew();
                double oracle = Aggregate_Oracle(adjacencyCluster, zones);
                stopwatch.Stop();
                double elapsed_Oracle = stopwatch.Elapsed.TotalMilliseconds;

                stopwatch.Restart();
                double index = Aggregate(new PartFIndex(adjacencyCluster), zones);
                stopwatch.Stop();

                Assert.Equal(oracle, index);

                _output.WriteLine("{0,6} {1,14:0.0} {2,14:0.0}", count, elapsed_Oracle, stopwatch.Elapsed.TotalMilliseconds);
            }
        }
    }
}
