// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020-2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Core;
using SAM.Weather;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Nodes;
using Xunit;
using Xunit.Abstractions;

namespace SAM.Tests
{
    /// <summary>
    /// How a TM52/TM59 assessment finds the room it is about to assess - and that finding it no longer costs
    /// a walk of the whole model.
    ///
    /// <para><b>The defect</b></para>
    /// <para>
    /// Both <c>TMOverheatingCalculator.Calculate_TM52</c> and <c>Calculate_TM59</c> resolve every room they
    /// are handed against the model before reading it, and they are right to: the caller is given the
    /// simulation space instances <c>SimulationSpaceMap</c> retained, and those predate
    /// <c>TM59AssessmentCalculator.RestoreDesignInternalConditions</c>, so the instance the model now holds is
    /// the one carrying the restored design internal condition. Classifying the caller's instance would pick
    /// the wrong TM59 result type.
    /// </para>
    /// <para>
    /// The resolution was <c>GetSpaces().Find(...)</c>, taken <b>inside</b> the loop over the rooms.
    /// <c>AdjacencyCluster.GetSpaces()</c> rebuilds the model's whole space list on every call, and the TM52
    /// path went through <c>AnalyticalModel.GetSpaces()</c>, which additionally <i>copies every space in the
    /// model</i>. Assessing five thousand rooms therefore built five thousand space lists - twenty five
    /// million <c>Space</c> copies on the TM52 path - before a single hourly value had been read.
    /// </para>
    ///
    /// <para><b>What replaced it, and what these tests are for</b></para>
    /// <para>
    /// One <c>Dictionary&lt;Guid, Space&gt;</c> per call, built from the single <c>GetSpaces()</c> the loop
    /// used to make per room, dropped when the call returns. It is an index and not a cache: it holds
    /// identity and nothing else, no series, no classification and no criterion, and nothing survives between
    /// two calls. The tests below split into two halves - the resolution <b>rule</b> is unchanged (section 1),
    /// and the <b>work</b> is now linear in the rooms assessed (section 2).
    /// </para>
    ///
    /// <para><b>Nothing here has a time limit</b></para>
    /// <para>
    /// Section 2 asserts allocated bytes, which is a property of the algorithm and identical on every
    /// machine: a rebuilt space list is an allocation of <c>O(spaces)</c>, so making one per room allocates
    /// <c>O(spaces²)</c> and making one per call allocates <c>O(spaces)</c>. The wall clock appears once, in
    /// <see cref="Benchmark"/>, which asserts nothing at all.
    /// </para>
    /// </summary>
    public class TM59SpaceResolutionScalingTests
    {
        private const string key_ResultantTemperature = "Resultant Temperature";

        private const string key_OccupancySensibleGain = "Occupancy Sensible Gain";

        private readonly ITestOutputHelper _output;

        public TM59SpaceResolutionScalingTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // -------------------------------------------------------------------------------------------------
        // 1. The resolution rule is unchanged
        // -------------------------------------------------------------------------------------------------

        /// <summary>
        /// A room the caller holds a <b>stale copy</b> of is still assessed from the instance the model
        /// carries.
        /// <para>
        /// This is the whole reason the resolution exists, and it is what a cheaper "just use the space you
        /// were given" would have broken. The stale copy here carries a different name and no hourly series
        /// at all; the result is named for the model's room and is produced from the model's series.
        /// </para>
        /// </summary>
        [Fact]
        public void AStaleCopyOfARoom_IsAssessedFromTheInstanceTheModelHolds()
        {
            AnalyticalModel analyticalModel = Model(3);

            Space space_Model = analyticalModel.GetSpaces()[1];

            //A copy with the same guid and nothing else in common: renamed, and carrying neither series.
            Space space_Stale = new(space_Model.Guid, new Space("Somebody else's room"));

            Assert.Equal(space_Model.Guid, space_Stale.Guid);
            Assert.False(Core.Query.TryGetValue(space_Stale, key_ResultantTemperature, out JsonArray _));

            List<TM59ExtendedResult> results = Calculator(analyticalModel).Calculate_TM59([space_Stale]);

            TM59ExtendedResult result = Assert.Single(results);

            Assert.Equal(space_Model.Name, result.Name);
            Assert.Equal(space_Model.Guid.ToString(), result.Reference);
        }

        /// <summary>
        /// The same, on the TM52 path - which resolved through <c>AnalyticalModel.GetSpaces()</c> and so was
        /// the more expensive of the two.
        /// </summary>
        [Fact]
        public void AStaleCopyOfARoom_IsAssessedFromTheInstanceTheModelHolds_TM52()
        {
            AnalyticalModel analyticalModel = Model(3);

            Space space_Model = analyticalModel.GetSpaces()[2];

            Space space_Stale = new(space_Model.Guid, new Space("Somebody else's room"));

            TM52ExtendedResult result = Assert.Single(Calculator(analyticalModel).Calculate_TM52([space_Stale], int.MinValue, int.MaxValue));

            Assert.Equal(space_Model.Name, result.Name);
            Assert.Equal(space_Model.Guid.ToString(), result.Reference);
        }

        /// <summary>
        /// A room that is not in the model at all is left out, and every other room in the request still
        /// produces its result. <c>Find</c> answered null for it and so does the index.
        /// </summary>
        [Fact]
        public void ARoomTheModelDoesNotHold_IsLeftOutAndTheRestAreAssessed()
        {
            AnalyticalModel analyticalModel = Model(4);

            List<Space> spaces = analyticalModel.GetSpaces();

            List<Space> spaces_Requested = [spaces[0], new Space("A room from another model"), spaces[3]];

            List<TM59ExtendedResult> results = Calculator(analyticalModel).Calculate_TM59(spaces_Requested);

            Assert.Equal(2, results.Count);
            Assert.Equal(spaces[0].Name, results[0].Name);
            Assert.Equal(spaces[3].Name, results[1].Name);
        }

        /// <summary>
        /// Results come back in the order the CALLER asked for the rooms, not the order the model happens to
        /// hold them in - a property of the loop, which the index does not touch and must not be allowed to.
        /// </summary>
        [Fact]
        public void TheResults_FollowTheOrderTheRoomsWereAskedFor()
        {
            AnalyticalModel analyticalModel = Model(5);

            List<Space> spaces = analyticalModel.GetSpaces();

            List<Space> spaces_Requested = [spaces[4], spaces[0], spaces[2]];

            List<TM59ExtendedResult> results = Calculator(analyticalModel).Calculate_TM59(spaces_Requested);

            Assert.Equal(3, results.Count);
            Assert.Equal(spaces[4].Name, results[0].Name);
            Assert.Equal(spaces[0].Name, results[1].Name);
            Assert.Equal(spaces[2].Name, results[2].Name);
        }

        /// <summary>
        /// A room asked for twice is answered twice. The resolution never de-duplicated and the index does
        /// not start: which rooms an assessment covers is the caller's statement, and silently collapsing a
        /// repeat would change the size of the answer.
        /// </summary>
        [Fact]
        public void ARoomAskedForTwice_IsAnsweredTwice()
        {
            AnalyticalModel analyticalModel = Model(2);

            Space space = analyticalModel.GetSpaces()[0];

            List<TM59ExtendedResult> results = Calculator(analyticalModel).Calculate_TM59([space, space]);

            Assert.Equal(2, results.Count);
            Assert.Equal(results[0].Reference, results[1].Reference);
        }

        /// <summary>
        /// The whole assessment is what the per-room resolution produced, at every size the real projects
        /// reach - same rooms, same order, same references, same criterion type.
        /// <para>
        /// <see cref="Resolve_Oracle"/> is the resolution exactly as it was written before this change:
        /// <c>GetSpaces().Find(...)</c>, once per room. Comparing the assessment against the rooms that
        /// resolution picks is what makes this an equivalence test rather than a restatement of the new code.
        /// </para>
        /// </summary>
        [Theory]
        [InlineData(100)]
        [InlineData(500)]
        [InlineData(1000)]
        public void TheAssessment_CoversExactlyTheRoomsThePerRoomResolutionPicked(int count)
        {
            AnalyticalModel analyticalModel = Model(count);

            List<Space> spaces_Requested = Requested(analyticalModel);

            List<Space> spaces_Expected = Resolve_Oracle(analyticalModel, spaces_Requested);

            List<TM59ExtendedResult> results = Calculator(analyticalModel).Calculate_TM59(spaces_Requested);

            Assert.Equal(spaces_Expected.Count, results.Count);

            for (int i = 0; i < spaces_Expected.Count; i++)
            {
                Assert.Equal(spaces_Expected[i].Name, results[i].Name);
                Assert.Equal(spaces_Expected[i].Guid.ToString(), results[i].Reference);
                Assert.IsType<TM59CorridorExtendedResult>(results[i]);
            }

            //Not a vacuous comparison: the request really did include rooms the model does not hold, and they
            //really were dropped.
            Assert.True(spaces_Expected.Count < spaces_Requested.Count, "The fixture asked only for rooms the model holds, so the dropping of an unresolved room was never exercised.");
        }

        // -------------------------------------------------------------------------------------------------
        // 2. The work is linear in the rooms assessed
        // -------------------------------------------------------------------------------------------------

        /// <summary>
        /// Doubling the model roughly doubles what a whole-model TM59 assessment allocates. The per-room
        /// resolution it replaced roughly quadruples, and that ratio is printed beside it from
        /// <see cref="Resolve_Oracle"/> - the code as it stood - so the comparison is measured and not
        /// asserted from memory.
        /// <para>
        /// <b>2.6, over two doublings.</b> Linear work sits at 2 and quadratic work at 4, so this cannot pass
        /// by accident and cannot fail because a runner was busy.
        /// </para>
        /// </summary>
        [Fact]
        public void AWholeModelAssessment_AllocatesLinearlyWithTheModel()
        {
            int[] counts = [400, 800, 1600];

            List<long> allocated_Assessment = [];
            List<long> allocated_Oracle = [];

            foreach (int count in counts)
            {
                AnalyticalModel analyticalModel = Model(count);

                List<Space> spaces = analyticalModel.GetSpaces();

                allocated_Assessment.Add(Allocated(() => Calculator(analyticalModel).Calculate_TM59(spaces)));
                allocated_Oracle.Add(Allocated(() => Resolve_Oracle(analyticalModel, spaces)));
            }

            for (int i = 0; i < counts.Length; i++)
            {
                _output.WriteLine("n={0,5}  whole assessment={1,14:N0} bytes  per-room resolution alone={2,16:N0} bytes", counts[i], allocated_Assessment[i], allocated_Oracle[i]);
            }

            for (int i = 1; i < counts.Length; i++)
            {
                double ratio_Assessment = (double)allocated_Assessment[i] / allocated_Assessment[i - 1];
                double ratio_Oracle = (double)allocated_Oracle[i] / allocated_Oracle[i - 1];

                _output.WriteLine("{0} -> {1}: assessment x{2:0.00}, per-room resolution x{3:0.00}", counts[i - 1], counts[i], ratio_Assessment, ratio_Oracle);

                Assert.True(
                    ratio_Assessment < 2.6,
                    string.Format("Doubling the model from {0} to {1} rooms multiplied the assessment's allocation by {2:0.00}. Linear work sits near 2 and quadratic work near 4, so something is walking the whole model per room again.", counts[i - 1], counts[i], ratio_Assessment));
            }
        }

        /// <summary>
        /// And on the TM52 path, which is the one that copied every space in the model per room.
        /// </summary>
        [Fact]
        public void AWholeModelTM52Assessment_AllocatesLinearlyWithTheModel()
        {
            int[] counts = [400, 800, 1600];

            List<long> allocated = [];

            foreach (int count in counts)
            {
                AnalyticalModel analyticalModel = Model(count);

                List<Space> spaces = analyticalModel.GetSpaces();

                allocated.Add(Allocated(() => Calculator(analyticalModel).Calculate_TM52(spaces, int.MinValue, int.MaxValue)));
            }

            for (int i = 0; i < counts.Length; i++)
            {
                _output.WriteLine("n={0,5}  TM52 assessment={1,14:N0} bytes", counts[i], allocated[i]);
            }

            for (int i = 1; i < counts.Length; i++)
            {
                double ratio = (double)allocated[i] / allocated[i - 1];

                _output.WriteLine("{0} -> {1}: TM52 assessment x{2:0.00}", counts[i - 1], counts[i], ratio);

                Assert.True(
                    ratio < 2.6,
                    string.Format("Doubling the model from {0} to {1} rooms multiplied the TM52 assessment's allocation by {2:0.00}, which is not linear.", counts[i - 1], counts[i], ratio));
            }
        }

        /// <summary>
        /// The model is indexed <b>once per call and never between calls</b>. Two assessments of one model
        /// allocate about twice one - not about one, which is what a cache held on the calculator would show.
        /// <para>
        /// Stated as a bound rather than an equality because the two calls allocate their own result objects
        /// too; what is being refused is the possibility that the second call is materially cheaper than the
        /// first.
        /// </para>
        /// </summary>
        [Fact]
        public void TwoAssessmentsOfOneModel_CostAboutTwiceOne()
        {
            AnalyticalModel analyticalModel = Model(400);

            List<Space> spaces = analyticalModel.GetSpaces();

            TMOverheatingCalculator tMOverheatingCalculator = Calculator(analyticalModel);

            long allocated_One = Allocated(() => tMOverheatingCalculator.Calculate_TM59(spaces));
            long allocated_Two = Allocated(() =>
            {
                tMOverheatingCalculator.Calculate_TM59(spaces);
                tMOverheatingCalculator.Calculate_TM59(spaces);
            });

            _output.WriteLine("one={0:N0} bytes, two={1:N0} bytes, ratio x{2:0.00}", allocated_One, allocated_Two, (double)allocated_Two / allocated_One);

            Assert.True(
                allocated_Two > allocated_One * 1.8,
                string.Format("Two assessments allocated {0:N0} bytes against {1:N0} for one. The second one is not paying its own way, which means something is being remembered between calls.", allocated_Two, allocated_One));
        }

        // -------------------------------------------------------------------------------------------------
        // 3. Timings, asserted on by nothing
        // -------------------------------------------------------------------------------------------------

        /// <summary>
        /// Local wall clock at the sizes the real projects reach, for the report. <b>Evidence, not a
        /// contract</b> - a slow machine does not make it fail.
        /// </summary>
        [Fact]
        [Trait("Category", "Benchmark")]
        public void Benchmark()
        {
            _output.WriteLine("{0,6} {1,22} {2,20}", "rooms", "per-room resolve (ms)", "whole TM59 (ms)");

            foreach (int count in new[] { 100, 500, 1000, 5000 })
            {
                AnalyticalModel analyticalModel = Model(count);

                List<Space> spaces = analyticalModel.GetSpaces();

                //Warmed, so the first size measured is not paying for the JIT of every method below it.
                Resolve_Oracle(analyticalModel, spaces);
                Calculator(analyticalModel).Calculate_TM59(spaces);

                Stopwatch stopwatch = Stopwatch.StartNew();
                Resolve_Oracle(analyticalModel, spaces);
                stopwatch.Stop();
                double elapsed_Oracle = stopwatch.Elapsed.TotalMilliseconds;

                stopwatch.Restart();
                List<TM59ExtendedResult> results = Calculator(analyticalModel).Calculate_TM59(spaces);
                stopwatch.Stop();

                Assert.Equal(count, results.Count);

                _output.WriteLine("{0,6} {1,22:0.0} {2,20:0.0}", count, elapsed_Oracle, stopwatch.Elapsed.TotalMilliseconds);
            }
        }

        // ---- Fixture --------------------------------------------------------------------------------------

        /// <summary>
        /// The resolution <b>exactly as it was written</b> before this change - the model's whole space list
        /// rebuilt, and walked, once per room. Kept as the oracle both halves of this file compare against:
        /// section 1 for the rooms it picks, section 2 for what it costs to pick them.
        /// </summary>
        private static List<Space> Resolve_Oracle(AnalyticalModel analyticalModel, IEnumerable<Space> spaces)
        {
            List<Space> result = [];

            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;

            foreach (Space space in spaces)
            {
                Space space_Temp = adjacencyCluster?.GetSpaces()?.Find(x => x.Guid == space.Guid);
                if (space_Temp == null)
                {
                    continue;
                }

                result.Add(space_Temp);
            }

            return result;
        }

        /// <summary>
        /// A request that is not simply "every room of the model": one room in ten is a stale copy the model
        /// still holds, and one in seventeen belongs to no model at all - so both the "resolved to the
        /// model's instance" and the "dropped" branches are exercised at every size.
        /// </summary>
        private static List<Space> Requested(AnalyticalModel analyticalModel)
        {
            List<Space> result = [];

            List<Space> spaces = analyticalModel.GetSpaces();

            for (int i = 0; i < spaces.Count; i++)
            {
                result.Add(i % 17 == 16
                    ? new Space(string.Format("Not in this model {0}", i))
                    : i % 10 == 9 ? new Space(spaces[i].Guid, new Space("A stale copy")) : spaces[i]);
            }

            return result;
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
        /// A calculator with an explicit EMPTY <c>TextMap</c>, so every room classifies to no TM59
        /// application and the criterion chosen is deterministic without depending on a shipped resource file
        /// being installed on the machine running the tests. Following
        /// <c>TMOverheatingCalculatorTests.Calculator</c>.
        /// </summary>
        private static TMOverheatingCalculator Calculator(AnalyticalModel analyticalModel)
        {
            return new TMOverheatingCalculator(analyticalModel) { TextMap = Core.Create.TextMap("TM59") };
        }

        /// <summary>
        /// A block of <paramref name="count"/> rooms, each carrying the two hourly series an assessment reads,
        /// stored exactly as the TSD converter stores them.
        /// </summary>
        private static AnalyticalModel Model(int count)
        {
            AdjacencyCluster adjacencyCluster = new();

            for (int i = 1; i <= count; i++)
            {
                Space space = new(string.Format("Bedroom {0:00000}", i));

                ParameterSet parameterSet = new("SAM.Analytical.Tas.dll");

                parameterSet.Add(key_ResultantTemperature, Values([21.0, 24.5, 27.5, 29.0]));
                parameterSet.Add(key_OccupancySensibleGain, Values([0, 80.0, 80.0, 0]));

                space.Add(parameterSet);

                adjacencyCluster.AddObject(space);
            }

            AnalyticalModel result = new("Block", null, null, null, adjacencyCluster);

            result.SetValue(AnalyticalModelParameter.WeatherData, new WeatherData("Test", "Test", 51.5, -0.1, 0, WeatherYear()));

            return result;
        }

        /// <summary>
        /// A full year of flat 20 C dry-bulb hours - the comfort band is a running mean of these, and a bare
        /// <c>WeatherYear(2018)</c> carries no days at all. Following <c>TMOverheatingCalculatorTests</c>.
        /// </summary>
        private static WeatherYear WeatherYear()
        {
            WeatherYear result = new(2018);

            for (int day = 0; day < 365; day++)
            {
                for (int hour = 0; hour < 24; hour++)
                {
                    result.Add(day, hour, new Dictionary<string, double> { { WeatherDataType.DryBulbTemperature.ToString(), 20.0 } });
                }
            }

            return result;
        }

        private static JsonArray Values(IEnumerable<double> values)
        {
            JsonArray result = [];

            foreach (double value in values)
            {
                result.Add(value);
            }

            return result;
        }
    }
}
