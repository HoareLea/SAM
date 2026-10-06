// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020-2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Analytical.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace SAM.Tests
{
    /// <summary>
    /// How <c>Modify.SetSpaceDesignFlowRate</c> finds the room it is about to write - and why the cheaper way
    /// of finding it is the <b>only</b> one that is safe on a write path.
    ///
    /// <para><b>Why this one was left alone the first time</b></para>
    /// <para>
    /// The Part F scaling work replaced <c>GetSpaces().Find(...)</c> with a <see cref="PartFIndex"/> snapshot
    /// at six read-only sites and deliberately did not touch this one, because it is a <b>write</b>: it
    /// replaces the room's terminals in the cluster. A snapshot shared across a loop of writes would answer
    /// with the model as it stood before them, so a second write to a room would compute its proportional
    /// split from the duties the first write had already replaced - silently wrong, and invisible.
    /// </para>
    ///
    /// <para><b>What changed, and why it is not a snapshot</b></para>
    /// <para>
    /// <c>AdjacencyCluster.GetObject&lt;Space&gt;(guid)</c> is a lookup in the cluster's <i>live</i> object
    /// dictionary, taken fresh on every call. There is nothing to go stale: it sees every write made before
    /// it, exactly as re-running <c>GetSpaces().Find</c> did. The requirement is then read with
    /// <c>Query.PartFRequiredFlowRate_Lps(Space, FlowClassification)</c> - the shared reader both the
    /// one-space query and <c>PartFIndex</c> end at - over the instance just resolved, instead of resolving
    /// the very same instance a second time through the whole model.
    /// </para>
    ///
    /// <para><b>Section 1 is the equivalence proof the change rests on</b></para>
    /// <para>
    /// It compares the two resolutions <b>directly, by reference</b>, over every space of a model at the
    /// sizes real projects reach and over the edges that are not spaces at all. Section 2 pins the write-path
    /// behaviour that a snapshot would have broken, and section 3 measures the cost.
    /// </para>
    /// </summary>
    public class SetSpaceDesignFlowRateLookupTests
    {
        private readonly ITestOutputHelper _output;

        public SetSpaceDesignFlowRateLookupTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // -------------------------------------------------------------------------------------------------
        // 1. GetObject<Space>(guid) is GetSpaces().Find(guid), exactly
        // -------------------------------------------------------------------------------------------------

        /// <summary>
        /// For every space of the model, the O(1) authority returns the <b>same object</b> the linear search
        /// returns. Reference equality, not equivalence: the write path replaces this instance's terminals,
        /// so answering with a different instance of the same room would be a different fact.
        /// </summary>
        [Theory]
        [InlineData(100)]
        [InlineData(500)]
        [InlineData(1000)]
        [InlineData(5000)]
        public void TheO1Authority_ReturnsTheSameInstanceTheLinearSearchReturns(int count)
        {
            AdjacencyCluster adjacencyCluster = PartFIndexTests.Model(count);

            List<Space> spaces = adjacencyCluster.GetSpaces();

            Assert.Equal(count, spaces.Count);

            foreach (Space space in spaces)
            {
                Assert.Same(Find_Oracle(adjacencyCluster, space.Guid), adjacencyCluster.GetObject<Space>(space.Guid));
            }
        }

        /// <summary>
        /// And it answers <b>null</b> for exactly what the linear search answers null for: a guid belonging to
        /// no object, the empty guid, and a guid belonging to an object of another type.
        /// <para>
        /// The last of those is the one worth stating. <c>GetSpaces()</c> is the space buckets flattened, so a
        /// zone's guid is not in it; <c>GetObject&lt;Space&gt;</c> reaches the zone's bucket only through the
        /// type filter, which refuses it for the same reason. A resolution that answered a zone here would
        /// make <c>SetSpaceDesignFlowRate</c> refuse a real room with the wrong sentence, or worse.
        /// </para>
        /// </summary>
        [Fact]
        public void TheO1Authority_AnswersNullForExactlyWhatTheLinearSearchAnswersNullFor()
        {
            AdjacencyCluster adjacencyCluster = PartFIndexTests.Model(50);

            List<Guid> guids = [Guid.NewGuid(), Guid.Empty, adjacencyCluster.GetZones()[0].Guid];

            foreach (Guid guid in guids)
            {
                Assert.Null(Find_Oracle(adjacencyCluster, guid));
                Assert.Null(adjacencyCluster.GetObject<Space>(guid));
            }
        }

        /// <summary>
        /// The equivalence survives writing. After the terminals of a room have been replaced, both
        /// resolutions still answer with the model's current instance of every room - the O(1) one is a live
        /// dictionary read, not a picture taken earlier.
        /// </summary>
        [Fact]
        public void TheO1Authority_StillMatchesTheLinearSearchAfterAWrite()
        {
            AdjacencyCluster adjacencyCluster = Realized(60);

            Space space = Supplied(adjacencyCluster);

            Assert.NotNull(adjacencyCluster.SetSpaceDesignFlowRate(space, FlowClassification.Supply, Design(adjacencyCluster, space, FlowClassification.Supply) + 5, out _, out List<string> refusals));
            Assert.Empty(refusals);

            foreach (Space space_Temp in adjacencyCluster.GetSpaces())
            {
                Assert.Same(Find_Oracle(adjacencyCluster, space_Temp.Guid), adjacencyCluster.GetObject<Space>(space_Temp.Guid));
            }
        }

        // -------------------------------------------------------------------------------------------------
        // 2. The write path sees its own writes
        // -------------------------------------------------------------------------------------------------

        /// <summary>
        /// <b>The test a shared snapshot would fail.</b> Three writes to one room, each reported as starting
        /// from what the previous one left - and the room really carries the last figure afterwards.
        /// <para>
        /// A resolution taken once before the loop would have computed the second and third splits from the
        /// duties the first write replaced, and reported each change as though it started from the original
        /// design. That is exactly the failure this method was excluded from the earlier indexing work to
        /// avoid, so it is pinned rather than argued.
        /// </para>
        /// </summary>
        [Fact]
        public void RepeatedWritesToOneRoom_EachSeeTheOneBefore()
        {
            AdjacencyCluster adjacencyCluster = Realized(40);

            Space space = Supplied(adjacencyCluster);

            double design = Design(adjacencyCluster, space, FlowClassification.Supply);

            for (int i = 0; i < 3; i++)
            {
                double design_Before = Design(adjacencyCluster, space, FlowClassification.Supply);

                Assert.NotNull(adjacencyCluster.SetSpaceDesignFlowRate(space, FlowClassification.Supply, design_Before + 4, out List<string> notes, out List<string> refusals));

                Assert.Empty(refusals);

                //The note states the figure it started from, and that figure is what the write before left.
                Assert.Contains(string.Format("from {0:0.###} l/s to {1:0.###} l/s", design_Before, design_Before + 4), Assert.Single(notes), StringComparison.Ordinal);

                Assert.Equal(design_Before + 4, Design(adjacencyCluster, space, FlowClassification.Supply), 9);
            }

            Assert.Equal(design + 12, Design(adjacencyCluster, space, FlowClassification.Supply), 9);
        }

        /// <summary>
        /// A whole sweep of the model: every room written once, then every room read back. Nothing a later
        /// write did overwrote an earlier one, and no room was resolved to a version of itself from before
        /// the sweep started.
        /// </summary>
        [Fact]
        public void AWriteSweepOfTheModel_LeavesEveryRoomCarryingWhatItWasGiven()
        {
            AdjacencyCluster adjacencyCluster = Realized(200);

            Dictionary<Guid, double> dictionary_Expected = [];

            foreach (Space space in adjacencyCluster.GetSpaces())
            {
                double design = Design(adjacencyCluster, space, FlowClassification.Supply);
                if (double.IsNaN(design))
                {
                    continue;
                }

                if (adjacencyCluster.SetSpaceDesignFlowRate(space, FlowClassification.Supply, design + 3, out _, out List<string> refusals) is null)
                {
                    //A refusal is a legitimate answer for a room whose requirement is above the target; it is
                    //not what this test is about, and the room is simply not one of the expectations.
                    Assert.NotEmpty(refusals);

                    continue;
                }

                dictionary_Expected[space.Guid] = design + 3;
            }

            Assert.True(dictionary_Expected.Count > 50, string.Format("Only {0} rooms were written, which is too few for the sweep to have exercised anything.", dictionary_Expected.Count));

            foreach (KeyValuePair<Guid, double> keyValuePair in dictionary_Expected)
            {
                Assert.Equal(keyValuePair.Value, Design(adjacencyCluster, adjacencyCluster.GetObject<Space>(keyValuePair.Key), FlowClassification.Supply), 9);
            }
        }

        /// <summary>
        /// The regulatory floor is read <b>live</b>, not remembered. Recalculating Approved Document F onto a
        /// room between two writes moves the floor the second write is judged against, and the second write
        /// refuses against the NEW figure.
        /// <para>
        /// This is what "no stale snapshot" has to mean for the requirement as well as for the room: the
        /// shared reader is handed the instance just resolved out of the cluster, and that instance carries
        /// whatever <c>PartFSpaceData</c> the model now holds.
        /// </para>
        /// </summary>
        [Fact]
        public void TheRegulatoryFloor_IsReadFromTheModelAsItStandsAtEachWrite()
        {
            AdjacencyCluster adjacencyCluster = Realized(30);

            Space space = Supplied(adjacencyCluster);

            //Comfortably above whatever the fixture sized it at, so the first write is accepted.
            Assert.NotNull(adjacencyCluster.SetSpaceDesignFlowRate(space, FlowClassification.Supply, 40, out _, out List<string> refusals_First));
            Assert.Empty(refusals_First);

            //Approved Document F recalculated: the room now requires far more than the design it carries.
            Space space_Resized = new(adjacencyCluster.GetObject<Space>(space.Guid));

            PartFSpaceData partFSpaceData = space_Resized.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData);

            foreach (PartFVentilationTerminalRequirement partFVentilationTerminalRequirement in partFSpaceData.Terminals)
            {
                if (!partFVentilationTerminalRequirement.IsExtract)
                {
                    partFVentilationTerminalRequirement.ContinuousDesignFlowRate_Lps = 500;
                }
            }

            space_Resized.SetValue(SpaceParameter.PartFSpaceData, partFSpaceData);

            adjacencyCluster.AddObject(space_Resized);

            //The same call, refused now - against the requirement the model currently states.
            Assert.Null(adjacencyCluster.SetSpaceDesignFlowRate(space, FlowClassification.Supply, 40, out _, out List<string> refusals_Second));

            Assert.Contains("500", Assert.Single(refusals_Second), StringComparison.Ordinal);
        }

        /// <summary>
        /// A room the model does not hold is still refused, and with the same sentence. The resolution
        /// answered null before and answers null now.
        /// </summary>
        [Fact]
        public void ARoomTheModelDoesNotHold_IsStillRefused()
        {
            AdjacencyCluster adjacencyCluster = Realized(20);

            Assert.Null(adjacencyCluster.SetSpaceDesignFlowRate(new Space("Somebody else's room"), FlowClassification.Supply, 25, out _, out List<string> refusals));

            Assert.Contains("is not in the model", Assert.Single(refusals), StringComparison.Ordinal);
        }

        /// <summary>
        /// A caller holding a <b>stale copy</b> of a room writes to the model's current instance, exactly as
        /// before - the reason the resolution exists at all.
        /// </summary>
        [Fact]
        public void AStaleCopyOfARoom_WritesToTheInstanceTheModelHolds()
        {
            AdjacencyCluster adjacencyCluster = Realized(30);

            Space space = Supplied(adjacencyCluster);

            double design = Design(adjacencyCluster, space, FlowClassification.Supply);

            //Renamed, and carrying no Approved Document F data at all - so a write that trusted it would
            //report the wrong room and judge against no floor.
            Space space_Stale = new(space.Guid, new Space("A room from before the rates were applied"));

            Assert.NotNull(adjacencyCluster.SetSpaceDesignFlowRate(space_Stale, FlowClassification.Supply, design + 6, out List<string> notes, out List<string> refusals));

            Assert.Empty(refusals);
            Assert.Contains(string.Format("Space '{0}'", space.Name), Assert.Single(notes), StringComparison.Ordinal);
            Assert.Equal(design + 6, Design(adjacencyCluster, space, FlowClassification.Supply), 9);
        }

        // -------------------------------------------------------------------------------------------------
        // 3. The cost of a sweep of writes
        // -------------------------------------------------------------------------------------------------

        /// <summary>
        /// Doubling the model roughly doubles what a whole-model write sweep allocates. Each write used to
        /// rebuild the model's space list <b>twice</b> - once to resolve the room and once inside the
        /// requirement query it then asked about the very same room - so the sweep was quadratic.
        /// </summary>
        [Fact]
        public void AWriteSweep_AllocatesLinearlyWithTheModel()
        {
            int[] counts = [400, 800, 1600];

            List<long> allocated = [];
            List<long> allocated_Oracle = [];

            foreach (int count in counts)
            {
                allocated.Add(Allocated(() => Sweep(Realized(count))));
                allocated_Oracle.Add(Allocated(() => Sweep_Oracle(Realized(count))));
            }

            for (int i = 0; i < counts.Length; i++)
            {
                _output.WriteLine("n={0,5}  write sweep={1,14:N0} bytes  the two resolutions it replaced={2,16:N0} bytes", counts[i], allocated[i], allocated_Oracle[i]);
            }

            for (int i = 1; i < counts.Length; i++)
            {
                double ratio = (double)allocated[i] / allocated[i - 1];
                double ratio_Oracle = (double)allocated_Oracle[i] / allocated_Oracle[i - 1];

                _output.WriteLine("{0} -> {1}: write sweep x{2:0.00}, the resolutions it replaced x{3:0.00}", counts[i - 1], counts[i], ratio, ratio_Oracle);

                Assert.True(
                    ratio < 2.6,
                    string.Format("Doubling the model from {0} to {1} rooms multiplied the write sweep's allocation by {2:0.00}. Linear work sits near 2 and quadratic work near 4, so a write is walking the whole model again.", counts[i - 1], counts[i], ratio));
            }
        }

        /// <summary>
        /// Local wall clock at the sizes the real projects reach, for the report. Asserts nothing about time.
        /// </summary>
        [Fact]
        [Trait("Category", "Benchmark")]
        public void Benchmark()
        {
            _output.WriteLine("{0,6} {1,26} {2,20}", "rooms", "the two resolutions (ms)", "write sweep (ms)");

            foreach (int count in new[] { 100, 500, 1000, 5000 })
            {
                //Warmed, so the first size measured is not paying for the JIT of every method below it.
                Sweep_Oracle(Realized(50));
                Sweep(Realized(50));

                AdjacencyCluster adjacencyCluster_Oracle = Realized(count);

                Stopwatch stopwatch = Stopwatch.StartNew();
                Sweep_Oracle(adjacencyCluster_Oracle);
                stopwatch.Stop();
                double elapsed_Oracle = stopwatch.Elapsed.TotalMilliseconds;

                AdjacencyCluster adjacencyCluster = Realized(count);

                stopwatch.Restart();
                Sweep(adjacencyCluster);
                stopwatch.Stop();

                _output.WriteLine("{0,6} {1,26:0.0} {2,20:0.0}", count, elapsed_Oracle, stopwatch.Elapsed.TotalMilliseconds);
            }
        }

        // ---- Fixture --------------------------------------------------------------------------------------

        /// <summary>
        /// The resolution <b>exactly as it was written</b>: the model's whole space list rebuilt, then walked.
        /// The oracle every equivalence assertion above compares against.
        /// </summary>
        private static Space Find_Oracle(AdjacencyCluster adjacencyCluster, Guid guid)
        {
            return (adjacencyCluster.GetSpaces() ?? []).Find(x => x is not null && x.Guid == guid);
        }

        /// <summary>Every room of the model written once, on the supply side.</summary>
        private static void Sweep(AdjacencyCluster adjacencyCluster)
        {
            foreach (Space space in adjacencyCluster.GetSpaces())
            {
                adjacencyCluster.SetSpaceDesignFlowRate(space, FlowClassification.Supply, 400, out _, out _);
            }
        }

        /// <summary>
        /// The <b>two</b> whole-model resolutions each write used to make, and nothing else - the room, and
        /// then the same room again inside the requirement query. Measured on its own so the before and after
        /// are comparable without the write itself standing in the way.
        /// </summary>
        private static void Sweep_Oracle(AdjacencyCluster adjacencyCluster)
        {
            foreach (Space space in adjacencyCluster.GetSpaces())
            {
                Space space_Cluster = Find_Oracle(adjacencyCluster, space.Guid);
                if (space_Cluster is null)
                {
                    continue;
                }

                Analytical.Query.PartFRequiredFlowRate_Lps(adjacencyCluster, space_Cluster, FlowClassification.Supply);
            }
        }

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
        /// The Part F fixture with its requirements realized as design terminals, which is what a design
        /// airflow is written across.
        /// </summary>
        private static AdjacencyCluster Realized(int count)
        {
            AdjacencyCluster result = PartFIndexTests.Model(count);

            result.RealizePartFVentilationTerminals(null, out _, out _);

            return result;
        }

        /// <summary>The first room of the model that carries a design supply terminal to write to.</summary>
        private static Space Supplied(AdjacencyCluster adjacencyCluster)
        {
            foreach (Space space in adjacencyCluster.GetSpaces())
            {
                if (!double.IsNaN(Design(adjacencyCluster, space, FlowClassification.Supply)))
                {
                    return space;
                }
            }

            Assert.Fail("The fixture realized no design supply terminal at all, so there is nothing to write to.");

            return null;
        }

        /// <summary>
        /// A room's current design airflow on one side, or NaN where it carries no terminal of that direction.
        /// </summary>
        private static double Design(AdjacencyCluster adjacencyCluster, Space space, FlowClassification flowClassification)
        {
            List<VentilationTerminal> ventilationTerminals = Analytical.Query.VentilationTerminals(adjacencyCluster.VentilationTerminals(space), flowClassification) ?? [];

            return ventilationTerminals.Count == 0 ? double.NaN : ventilationTerminals.VentilationTerminalDesignDuty_Lps(flowClassification) ?? 0;
        }
    }
}
