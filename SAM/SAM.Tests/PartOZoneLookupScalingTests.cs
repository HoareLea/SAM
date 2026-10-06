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
    /// How the Approved Document O paths find a <b>zone</b> in the design model - and that finding one no
    /// longer rebuilds the model's zone list, or copies its whole cluster.
    ///
    /// <para><b>Two sites, one shape of defect</b></para>
    /// <list type="number">
    /// <item><c>OverheatingScenarioMap.Add</c> resolved its scenario's design zone with
    /// <c>analyticalModel_Design.GetZones().Find(...)</c> and then read that zone's spaces off
    /// <c>analyticalModel_Design.AdjacencyCluster</c> - <b>once per scenario</b>. There is one scenario per
    /// dwelling, so a block resolved its zones quadratically; and worse, the <c>AdjacencyCluster</c> property
    /// hands out a <i>new shallow copy of the entire cluster</i> on every read, so a five hundred dwelling
    /// block copied the cluster five hundred times.</item>
    /// <item><c>TM59AssessmentCalculator.Spaces</c> did the same per zone of the requested scope, and
    /// additionally de-duplicated the rooms it gathered with a linear <c>Find</c> over everything gathered so
    /// far - quadratic in the rooms assessed.</item>
    /// </list>
    ///
    /// <para><b>What replaced them</b></para>
    /// <para>
    /// One <c>Dictionary&lt;Guid, Zone&gt;</c> and one cluster reference per map or per call, and a
    /// <c>HashSet&lt;Guid&gt;</c> beside the gathered list. All three are request scoped and hold identity
    /// only. The cluster copy is shallow, so a hoisted one shares every <c>Zone</c> and <c>Space</c> instance
    /// with the model and there is nothing for it to disagree with.
    /// </para>
    ///
    /// <para><b>What these assert</b></para>
    /// <para>
    /// The same answers as the per-zone resolution, and work that grows linearly. Allocated bytes, never
    /// milliseconds - <see cref="Benchmark"/> aside, which asserts nothing.
    /// </para>
    /// </summary>
    public class PartOZoneLookupScalingTests
    {
        private readonly ITestOutputHelper _output;

        public PartOZoneLookupScalingTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // -------------------------------------------------------------------------------------------------
        // 1. OverheatingScenarioMap
        // -------------------------------------------------------------------------------------------------

        /// <summary>
        /// Every scenario governs exactly the simulated rooms the per-scenario resolution gave it, in the
        /// same order, at every size.
        /// </summary>
        [Theory]
        [InlineData(20)]
        [InlineData(100)]
        [InlineData(500)]
        public void EachScenario_GovernsExactlyTheRoomsThePerScenarioResolutionGaveIt(int dwellings)
        {
            AnalyticalModel analyticalModel = Model(dwellings);

            List<OverheatingScenario> overheatingScenarios = Scenarios(analyticalModel);

            SimulationSpaceMap simulationSpaceMap = SimulationSpaceMap.Identity(analyticalModel.GetSpaces());

            OverheatingScenarioMap overheatingScenarioMap = new(overheatingScenarios, analyticalModel, simulationSpaceMap);

            Assert.Empty(overheatingScenarioMap.Refusals);
            Assert.Equal(dwellings, overheatingScenarioMap.OverheatingScenarios.Count);

            foreach (OverheatingScenario overheatingScenario in overheatingScenarios)
            {
                List<Space> spaces_Expected = Spaces_Oracle(analyticalModel, overheatingScenario.ZoneGuid);

                List<Space> spaces = overheatingScenarioMap.Spaces(overheatingScenario);

                Assert.Equal(spaces_Expected.Count, spaces.Count);

                for (int i = 0; i < spaces_Expected.Count; i++)
                {
                    Assert.Equal(spaces_Expected[i].Guid, spaces[i].Guid);
                }
            }
        }

        /// <summary>
        /// A scenario naming a zone the design model does not hold is still refused, with the same sentence -
        /// the index answers null exactly where the linear search did.
        /// </summary>
        [Fact]
        public void AScenarioNamingAZoneTheModelDoesNotHold_IsStillRefused()
        {
            AnalyticalModel analyticalModel = Model(4);

            List<OverheatingScenario> overheatingScenarios = Scenarios(analyticalModel);

            overheatingScenarios.Add(new OverheatingScenario(PartOAssessmentScope.Dwelling, Guid.NewGuid(), PartOIteration.BasePassive));

            OverheatingScenarioMap overheatingScenarioMap = new(overheatingScenarios, analyticalModel, SimulationSpaceMap.Identity(analyticalModel.GetSpaces()));

            Assert.Contains("names a zone the design model does not hold", Assert.Single(overheatingScenarioMap.Refusals), StringComparison.Ordinal);

            //And the four real scenarios are unaffected.
            Assert.Equal(4, overheatingScenarioMap.OverheatingScenarios.Count);
        }

        /// <summary>
        /// Doubling the block roughly doubles what building the map allocates. The per-scenario resolution it
        /// replaced - the zone list rebuilt and the cluster copied, once per scenario - roughly quadruples,
        /// and that ratio is measured beside it rather than asserted from memory.
        /// </summary>
        [Fact]
        public void BuildingTheScenarioMap_AllocatesLinearlyWithTheBlock()
        {
            int[] counts = [100, 200, 400];

            List<long> allocated = [];
            List<long> allocated_Oracle = [];

            foreach (int count in counts)
            {
                AnalyticalModel analyticalModel = Model(count);

                List<OverheatingScenario> overheatingScenarios = Scenarios(analyticalModel);

                List<Space> spaces = analyticalModel.GetSpaces();

                allocated.Add(Allocated(() => new OverheatingScenarioMap(overheatingScenarios, analyticalModel, SimulationSpaceMap.Identity(spaces))));
                allocated_Oracle.Add(Allocated(() =>
                {
                    foreach (OverheatingScenario overheatingScenario in overheatingScenarios)
                    {
                        Spaces_Oracle(analyticalModel, overheatingScenario.ZoneGuid);
                    }
                }));
            }

            for (int i = 0; i < counts.Length; i++)
            {
                _output.WriteLine("dwellings={0,5}  scenario map={1,14:N0} bytes  per-scenario resolution alone={2,16:N0} bytes", counts[i], allocated[i], allocated_Oracle[i]);
            }

            for (int i = 1; i < counts.Length; i++)
            {
                double ratio = (double)allocated[i] / allocated[i - 1];
                double ratio_Oracle = (double)allocated_Oracle[i] / allocated_Oracle[i - 1];

                _output.WriteLine("{0} -> {1}: scenario map x{2:0.00}, per-scenario resolution x{3:0.00}", counts[i - 1], counts[i], ratio, ratio_Oracle);

                Assert.True(
                    ratio < 2.6,
                    string.Format("Doubling the block from {0} to {1} dwellings multiplied the scenario map's allocation by {2:0.00}. Linear work sits near 2 and quadratic work near 4, so something is walking - or copying - the whole model per scenario again.", counts[i - 1], counts[i], ratio));
            }
        }

        // -------------------------------------------------------------------------------------------------
        // 2. TM59AssessmentCalculator.Spaces
        // -------------------------------------------------------------------------------------------------

        /// <summary>
        /// The scope a set of design zones resolves to is exactly what the per-zone resolution and the linear
        /// de-duplication produced - same rooms, same order, one entry each.
        /// </summary>
        [Theory]
        [InlineData(20)]
        [InlineData(100)]
        [InlineData(500)]
        public void TheAssessmentScope_IsExactlyWhatThePerZoneResolutionGathered(int dwellings)
        {
            AnalyticalModel analyticalModel = Model(dwellings);

            List<Zone> zones = analyticalModel.GetZones();

            TM59AssessmentCalculator tM59AssessmentCalculator = new(analyticalModel, analyticalModel, SimulationSpaceMap.Identity(analyticalModel.GetSpaces()));

            List<Space> spaces_Expected = Scope_Oracle(analyticalModel, zones);

            List<Space> spaces = tM59AssessmentCalculator.Spaces(null, zones);

            Assert.Empty(tM59AssessmentCalculator.AssociationRefusals);

            Assert.Equal(spaces_Expected.Count, spaces.Count);

            for (int i = 0; i < spaces_Expected.Count; i++)
            {
                Assert.Equal(spaces_Expected[i].Guid, spaces[i].Guid);
            }
        }

        /// <summary>
        /// Two zones sharing a room still yield that room once, and asking for a zone and one of its rooms
        /// does not return the room twice. The de-duplication rule is unchanged; only how the question is
        /// asked is.
        /// </summary>
        [Fact]
        public void ARoomInTwoRequestedZones_IsGatheredOnce()
        {
            AnalyticalModel analyticalModel = Model(3);

            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;

            List<Zone> zones = adjacencyCluster.GetZones();

            //A second zone over the first dwelling's rooms, so the two overlap completely.
            Zone zone_Overlapping = new("Overlapping");
            adjacencyCluster.AddObject(zone_Overlapping);

            List<Space> spaces_First = adjacencyCluster.GetRelatedObjects<Space>(zones[0]);

            foreach (Space space in spaces_First)
            {
                adjacencyCluster.AddRelation(zone_Overlapping, space);
            }

            AnalyticalModel analyticalModel_Temp = new(analyticalModel, adjacencyCluster);

            TM59AssessmentCalculator tM59AssessmentCalculator = new(analyticalModel_Temp, analyticalModel_Temp, SimulationSpaceMap.Identity(analyticalModel_Temp.GetSpaces()));

            //The dwelling zone, the overlapping zone, and one of the shared rooms named individually.
            List<Space> spaces = tM59AssessmentCalculator.Spaces([spaces_First[0]], [zones[0], zone_Overlapping]);

            Assert.Equal(spaces_First.Count, spaces.Count);

            HashSet<Guid> guids = [];

            foreach (Space space in spaces)
            {
                Assert.True(guids.Add(space.Guid), string.Format("Space '{0}' was gathered more than once.", space.Name));
            }
        }

        /// <summary>
        /// A requested zone the design model does not hold is still refused, with the same sentence, and the
        /// rest of the scope survives.
        /// </summary>
        [Fact]
        public void ARequestedZoneTheModelDoesNotHold_IsStillRefused()
        {
            AnalyticalModel analyticalModel = Model(3);

            List<Zone> zones = analyticalModel.GetZones();

            TM59AssessmentCalculator tM59AssessmentCalculator = new(analyticalModel, analyticalModel, SimulationSpaceMap.Identity(analyticalModel.GetSpaces()));

            List<Space> spaces = tM59AssessmentCalculator.Spaces(null, [zones[0], new Zone("A zone from another model")]);

            Assert.Contains("is not in the design model", Assert.Single(tM59AssessmentCalculator.AssociationRefusals), StringComparison.Ordinal);

            Assert.Equal(Spaces_Oracle(analyticalModel, zones[0].Guid).Count, spaces.Count);
        }

        /// <summary>
        /// Doubling the block roughly doubles what resolving the whole assessment scope allocates.
        /// </summary>
        [Fact]
        public void ResolvingTheAssessmentScope_AllocatesLinearlyWithTheBlock()
        {
            int[] counts = [100, 200, 400];

            List<long> allocated = [];
            List<long> allocated_Oracle = [];

            foreach (int count in counts)
            {
                AnalyticalModel analyticalModel = Model(count);

                List<Zone> zones = analyticalModel.GetZones();

                TM59AssessmentCalculator tM59AssessmentCalculator = new(analyticalModel, analyticalModel, SimulationSpaceMap.Identity(analyticalModel.GetSpaces()));

                allocated.Add(Allocated(() => tM59AssessmentCalculator.Spaces(null, zones)));
                allocated_Oracle.Add(Allocated(() => Scope_Oracle(analyticalModel, zones)));
            }

            for (int i = 0; i < counts.Length; i++)
            {
                _output.WriteLine("dwellings={0,5}  scope={1,14:N0} bytes  per-zone resolution + linear de-duplication={2,16:N0} bytes", counts[i], allocated[i], allocated_Oracle[i]);
            }

            for (int i = 1; i < counts.Length; i++)
            {
                double ratio = (double)allocated[i] / allocated[i - 1];
                double ratio_Oracle = (double)allocated_Oracle[i] / allocated_Oracle[i - 1];

                _output.WriteLine("{0} -> {1}: scope x{2:0.00}, the resolution it replaced x{3:0.00}", counts[i - 1], counts[i], ratio, ratio_Oracle);

                Assert.True(
                    ratio < 2.6,
                    string.Format("Doubling the block from {0} to {1} dwellings multiplied the scope resolution's allocation by {2:0.00}, which is not linear.", counts[i - 1], counts[i], ratio));
            }
        }

        // -------------------------------------------------------------------------------------------------
        // 3. Timings, asserted on by nothing
        // -------------------------------------------------------------------------------------------------

        /// <summary>
        /// Local wall clock at the sizes the real projects reach - 1,000 dwellings of five rooms is a five
        /// thousand space project. <b>Evidence, not a contract.</b>
        /// </summary>
        [Fact]
        [Trait("Category", "Benchmark")]
        public void Benchmark()
        {
            _output.WriteLine("{0,10} {1,7} {2,18} {3,14} {4,16} {5,12}", "dwellings", "spaces", "scenarios (ms)", "was (ms)", "scope (ms)", "was (ms)");

            foreach (int count in new[] { 20, 100, 200, 1000 })
            {
                AnalyticalModel analyticalModel = Model(count);

                List<OverheatingScenario> overheatingScenarios = Scenarios(analyticalModel);
                List<Zone> zones = analyticalModel.GetZones();
                List<Space> spaces = analyticalModel.GetSpaces();

                SimulationSpaceMap simulationSpaceMap = SimulationSpaceMap.Identity(spaces);

                TM59AssessmentCalculator tM59AssessmentCalculator = new(analyticalModel, analyticalModel, simulationSpaceMap);

                //Warmed, so the first size measured is not paying for the JIT of every method below it.
                new OverheatingScenarioMap(overheatingScenarios, analyticalModel, simulationSpaceMap);
                tM59AssessmentCalculator.Spaces(null, zones);

                Stopwatch stopwatch = Stopwatch.StartNew();
                new OverheatingScenarioMap(overheatingScenarios, analyticalModel, simulationSpaceMap);
                stopwatch.Stop();
                double elapsed_Map = stopwatch.Elapsed.TotalMilliseconds;

                stopwatch.Restart();
                foreach (OverheatingScenario overheatingScenario in overheatingScenarios)
                {
                    Spaces_Oracle(analyticalModel, overheatingScenario.ZoneGuid);
                }
                stopwatch.Stop();
                double elapsed_Map_Oracle = stopwatch.Elapsed.TotalMilliseconds;

                stopwatch.Restart();
                tM59AssessmentCalculator.Spaces(null, zones);
                stopwatch.Stop();
                double elapsed_Scope = stopwatch.Elapsed.TotalMilliseconds;

                stopwatch.Restart();
                Scope_Oracle(analyticalModel, zones);
                stopwatch.Stop();

                _output.WriteLine("{0,10} {1,7} {2,18:0.0} {3,14:0.0} {4,16:0.0} {5,12:0.0}", count, spaces.Count, elapsed_Map, elapsed_Map_Oracle, elapsed_Scope, stopwatch.Elapsed.TotalMilliseconds);
            }
        }

        // ---- Fixture --------------------------------------------------------------------------------------

        /// <summary>
        /// The zone resolution and the relation read <b>exactly as they were written</b> in
        /// <c>OverheatingScenarioMap.Add</c>: the model's whole zone list rebuilt and walked, and its whole
        /// cluster copied, per scenario.
        /// </summary>
        private static List<Space> Spaces_Oracle(AnalyticalModel analyticalModel, Guid guid_Zone)
        {
            Zone zone = analyticalModel.GetZones()?.Find(x => x != null && x.Guid == guid_Zone);

            return zone == null ? [] : analyticalModel.AdjacencyCluster.GetRelatedObjects<Space>(zone) ?? [];
        }

        /// <summary>
        /// The scope gathering <b>exactly as it was written</b> in <c>TM59AssessmentCalculator.Spaces</c>:
        /// the zone resolved per zone, and each room de-duplicated by a linear search of everything gathered
        /// so far.
        /// </summary>
        private static List<Space> Scope_Oracle(AnalyticalModel analyticalModel, IEnumerable<Zone> zones_Design)
        {
            List<Space> result = [];

            foreach (Zone zone_Design in zones_Design ?? [])
            {
                Zone zone = analyticalModel.GetZones()?.Find(x => x != null && x.Guid == zone_Design.Guid);
                if (zone == null)
                {
                    continue;
                }

                foreach (Space space in analyticalModel.AdjacencyCluster.GetRelatedObjects<Space>(zone) ?? [])
                {
                    if (result.Find(x => x != null && x.Guid == space.Guid) == null)
                    {
                        result.Add(space);
                    }
                }
            }

            return result;
        }

        /// <summary>One scenario per dwelling zone, which is what a Part O run states.</summary>
        private static List<OverheatingScenario> Scenarios(AnalyticalModel analyticalModel)
        {
            List<OverheatingScenario> result = [];

            foreach (Zone zone in analyticalModel.GetZones())
            {
                result.Add(new OverheatingScenario(PartOAssessmentScope.Dwelling, zone.Guid, PartOIteration.BasePassive));
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

        /// <summary>A block of <paramref name="dwellings"/> flats, five rooms each.</summary>
        private static AnalyticalModel Model(int dwellings)
        {
            return new AnalyticalModel("Block", null, null, null, PartFIndexTests.Model(dwellings * 5));
        }
    }
}
