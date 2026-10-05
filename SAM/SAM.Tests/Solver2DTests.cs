// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020-2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Geometry.Planar;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace SAM.Tests
{
    /// <summary>
    /// Behaviour locks for the shared 2D placement engine <see cref="Solver2D"/>, which places the
    /// floor-plan space labels, the Mollier chart labels and the Part F airflow tags.
    /// <para>
    /// Written for the hardening pass that made it the common annotation engine, and covering one thing
    /// each: the two null paths, the placement order, the result type, and the deterministic work budget
    /// that replaced a wall-clock one. The determinism tests are the load-bearing ones - a saved drawing
    /// that redraws with its labels somewhere else is not a saved drawing.
    /// </para>
    /// </summary>
    public class Solver2DTests
    {
        private readonly ITestOutputHelper testOutputHelper;

        public Solver2DTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        // --- The two null paths ---------------------------------------------------------------------

        /// <summary>
        /// A null obstacle list means "nothing to avoid". The constructor accepts one, so the solve has to
        /// as well; it used to throw a NullReferenceException out of obstacles2D.Find on the first
        /// candidate of the first item. This is the path a consumer with no obstacles takes.
        /// </summary>
        [Fact]
        public void Solve_NullObstacleList_PlacesInsteadOfThrowing()
        {
            Solver2D solver2D = new Solver2D(Area(), null);
            solver2D.Add(Data(new Point2D(0, 0), "only"));

            List<Solver2DResult> solver2DResults = solver2D.Solve();

            Assert.NotNull(solver2DResults);
            Assert.Single(solver2DResults);
            Assert.Equal(Solver2DResultType.Solved, solver2DResults[0].ResultType);
            Assert.NotNull(solver2DResults[0].Closed2D<Rectangle2D>());
        }

        /// <summary>
        /// An item nothing could be found for carries no footprint, so the items after it must be solved
        /// against the ones that WERE placed and must not see the gap. Exercises the small non-grid path
        /// (256 items or fewer) - a Mollier chart or a small floor plan - where the overlap test reads the
        /// results collected so far and so meets the unplaced entry.
        /// </summary>
        [Fact]
        public void Solve_ItemLeftUnplaced_LaterItemsStillSolve()
        {
            Solver2D solver2D = new Solver2D(Area(), new List<IClosed2D>());

            //Unplaceable by construction: its centre is required to land in a square 40 m away, and the
            //sweep only reaches 4.5 m.
            Solver2DData solver2DData_Unplaceable = Data(new Point2D(0, 0), "unplaceable");
            solver2DData_Unplaceable.Solver2DSettings.LimitArea = new Rectangle2D(new Point2D(40, 40), 1, 1);

            solver2D.Add(solver2DData_Unplaceable);
            solver2D.Add(Data(new Point2D(0, 0), "after"));

            List<Solver2DResult> solver2DResults = solver2D.Solve();

            Assert.Equal(2, solver2DResults.Count);

            Assert.Equal(Solver2DResultType.Unplaced, solver2DResults[0].ResultType);
            Assert.Null(solver2DResults[0].Closed2D<Rectangle2D>());

            Assert.Equal(Solver2DResultType.Solved, solver2DResults[1].ResultType);
            Assert.NotNull(solver2DResults[1].Closed2D<Rectangle2D>());
        }

        // --- Deterministic order -------------------------------------------------------------------

        /// <summary>
        /// Items of equal priority are placed in the order they were added. Twenty of them, because
        /// List&lt;T&gt;.Sort switches from a stable insertion sort to an unstable introsort above sixteen -
        /// which is exactly why sorting on priority alone reordered equal-priority labels and made a
        /// redraw move them.
        /// </summary>
        [Fact]
        public void Solve_EqualPriority_PlacesInInsertionOrder()
        {
            Solver2D solver2D = new Solver2D(Area(), new List<IClosed2D>());

            List<string> tags = new List<string>();
            for (int i = 0; i < 20; i++)
            {
                string tag = string.Format("tag {0}", i);

                tags.Add(tag);
                solver2D.Add(Data(new Point2D(0, 0), tag));
            }

            List<Solver2DResult> solver2DResults = solver2D.Solve();

            Assert.Equal(tags, solver2DResults.ConvertAll(x => x.Tag as string));

            //Placed first, so it keeps the anchor and everything else moves around it.
            Point2D point2D_Centroid = solver2DResults[0].Closed2D<Rectangle2D>().GetCentroid();
            Assert.Equal(0, point2D_Centroid.X, 6);
            Assert.Equal(0, point2D_Centroid.Y, 6);
        }

        /// <summary>Priority still wins, and only ties fall back to insertion order.</summary>
        [Fact]
        public void Solve_LowerPriorityAddedLast_IsStillPlacedFirst()
        {
            Solver2D solver2D = new Solver2D(Area(), new List<IClosed2D>());

            Solver2DData solver2DData_Second = Data(new Point2D(0, 0), "high");
            solver2DData_Second.Priority = 10;

            Solver2DData solver2DData_First = Data(new Point2D(0, 0), "low");
            solver2DData_First.Priority = 1;

            solver2D.Add(solver2DData_Second);
            solver2D.Add(solver2DData_First);

            List<Solver2DResult> solver2DResults = solver2D.Solve();

            Assert.Equal("low", solver2DResults[0].Tag as string);
            Assert.Equal("high", solver2DResults[1].Tag as string);

            //The one placed first keeps the anchor.
            Assert.Equal(0, solver2DResults[0].Closed2D<Rectangle2D>().GetCentroid().X, 6);
        }

        /// <summary>
        /// The same input solved twice returns the same geometry, both from a second instance and from a
        /// second call on the same instance - the latter used to re-sort the item list in place, so call
        /// two started from the order call one left behind.
        /// </summary>
        [Fact]
        public void Solve_IdenticalInput_ReturnsIdenticalPlacement()
        {
            List<Solver2DResult> solver2DResults_1 = Solver2D_Crowded().Solve();

            Solver2D solver2D = Solver2D_Crowded();
            List<Solver2DResult> solver2DResults_2 = solver2D.Solve();
            List<Solver2DResult> solver2DResults_3 = solver2D.Solve();

            AssertSamePlacement(solver2DResults_1, solver2DResults_2);
            AssertSamePlacement(solver2DResults_1, solver2DResults_3);
        }

        /// <summary>
        /// The work the solve does is a function of its input, which is what makes the budget - and so the
        /// layout it produces - independent of the machine. A stopwatch budget could not promise this.
        /// </summary>
        [Fact]
        public void Solve_IdenticalInput_ConsumesIdenticalWork()
        {
            Solver2D solver2D_1 = Solver2D_Crowded();
            Solver2D solver2D_2 = Solver2D_Crowded();

            solver2D_1.Solve();
            solver2D_2.Solve();

            Assert.True(solver2D_1.WorkUnits > 0);
            Assert.Equal(solver2D_1.WorkUnits, solver2D_2.WorkUnits);
        }

        // --- Result type ----------------------------------------------------------------------------

        /// <summary>
        /// Once the budget is spent the remaining items are dropped at their anchor untested, and that has
        /// to be visible: the geometry is not null, it can overlap what is already there, and a caller
        /// reading only Closed2D cannot tell it from a solved placement. Fallback is how it can.
        /// </summary>
        [Fact]
        public void Solve_BudgetExhausted_ReportsFallbackAndNotSolved()
        {
            Solver2D solver2D = new Solver2D(Area(), new List<IClosed2D>());
            solver2D.WorkBudget = 1;

            for (int i = 0; i < 6; i++)
            {
                solver2D.Add(Data(new Point2D(0, 0), string.Format("tag {0}", i)));
            }

            List<Solver2DResult> solver2DResults = solver2D.Solve();

            Assert.Equal(Solver2DResultType.Solved, solver2DResults[0].ResultType);

            List<Solver2DResult> solver2DResults_Fallback = solver2DResults.FindAll(x => x.ResultType == Solver2DResultType.Fallback);
            Assert.NotEmpty(solver2DResults_Fallback);

            Rectangle2D rectangle2D_Solved = solver2DResults[0].Closed2D<Rectangle2D>();

            foreach (Solver2DResult solver2DResult in solver2DResults_Fallback)
            {
                Rectangle2D rectangle2D = solver2DResult.Closed2D<Rectangle2D>();

                //Geometry, so a consumer that only checks for null would draw it as though it were solved.
                Assert.NotNull(rectangle2D);

                //At the anchor, and therefore on top of the item that was solved there.
                Assert.Equal(0, rectangle2D.GetCentroid().X, 6);
                Assert.Equal(0, rectangle2D.GetCentroid().Y, 6);
                Assert.True(rectangle2D.InRange(rectangle2D_Solved));
            }
        }

        /// <summary>A non-positive budget removes the cap, so nothing falls back.</summary>
        [Fact]
        public void Solve_NoBudget_NeverFallsBack()
        {
            Solver2D solver2D = Solver2D_Crowded();
            solver2D.WorkBudget = 0;

            List<Solver2DResult> solver2DResults = solver2D.Solve();

            Assert.DoesNotContain(Solver2DResultType.Fallback, solver2DResults.ConvertAll(x => x.ResultType));
        }

        /// <summary>
        /// The result type a caller reads is never the default one: a defaulted value that said "solved"
        /// would be the confusion the type was added to remove.
        /// </summary>
        [Fact]
        public void Solve_EveryResult_CarriesAnExplicitResultType()
        {
            List<Solver2DResult> solver2DResults = Solver2D_Crowded().Solve();

            Assert.DoesNotContain(Solver2DResultType.Undefined, solver2DResults.ConvertAll(x => x.ResultType));
        }

        /// <summary>
        /// The constructor that predates the result type still works, deriving it from the geometry. It is
        /// deliberately not what Solver2D uses, because a fallback rectangle would come out of it as solved.
        /// </summary>
        [Fact]
        public void Solver2DResult_WithoutResultType_DerivesItFromTheGeometry()
        {
            Solver2DData solver2DData = Data(new Point2D(0, 0), "tag");

            Assert.Equal(Solver2DResultType.Solved, new Solver2DResult(solver2DData, new Rectangle2D(1, 1)).ResultType);
            Assert.Equal(Solver2DResultType.Unplaced, new Solver2DResult(solver2DData, null).ResultType);
        }

        // --- LimitArea semantics --------------------------------------------------------------------

        /// <summary>
        /// LimitArea constrains the CENTROID only, and the rectangle may overhang it. Locked because the
        /// name reads as though it constrained the whole rectangle, and because all three consumers depend
        /// on the looser meaning: an ensuite cannot contain a whole text box, and requiring it to would
        /// leave the smallest rooms unlabelled.
        /// </summary>
        [Fact]
        public void Solve_LimitArea_ConstrainsTheCentroidAndNotTheWholeRectangle()
        {
            Solver2D solver2D = new Solver2D(Area(), new List<IClosed2D>());

            //A limit area 0.8 m across cannot contain the 4 m by 1 m label, only its centre.
            Rectangle2D rectangle2D_Limit = new Rectangle2D(new Point2D(-0.4, -0.4), 0.8, 0.8);

            Solver2DData solver2DData = new Solver2DData(new Rectangle2D(new Point2D(-2, -0.5), 4, 1), new Point2D(0, 0));
            solver2DData.Tag = "wide";
            solver2DData.Solver2DSettings = new Solver2DSettings()
            {
                StartingDistance = 0,
                ShiftDistance = 0.5,
                IterationCount = 10,
                LimitArea = rectangle2D_Limit,
            };

            solver2D.Add(solver2DData);

            List<Solver2DResult> solver2DResults = solver2D.Solve();

            Rectangle2D rectangle2D = solver2DResults[0].Closed2D<Rectangle2D>();

            Assert.Equal(Solver2DResultType.Solved, solver2DResults[0].ResultType);
            Assert.NotNull(rectangle2D);

            //Centre inside the limit area...
            Assert.True(rectangle2D_Limit.Inside(rectangle2D.GetCentroid()));

            //...and the rectangle itself sticking well outside it.
            Assert.False(rectangle2D_Limit.Inside(rectangle2D));
        }

        // --- The Mollier chart's shape of input -----------------------------------------------------

        /// <summary>
        /// The Mollier chart's own shape of input, which has no test of its own because its adapter lives
        /// above OxyPlot in SAM_UI: point labels at their default priority, curve labels anchored to a
        /// Polyline2D at priorities 2 to 4, and a circle obstacle per point. Locks the three things the
        /// hardening pass could have disturbed there - that priority still decides the order, that equal
        /// priority now follows the order the chart added them in, and that obstacles are still avoided -
        /// and it is the only cover the Polyline2D anchor branch has.
        /// </summary>
        [Fact]
        public void Solve_MollierShapedInput_KeepsPriorityOrderAvoidsObstaclesAndRepeats()
        {
            List<Solver2DResult> solver2DResults_1 = Solver2D_Mollier(out List<IClosed2D> obstacle2Ds).Solve();
            List<Solver2DResult> solver2DResults_2 = Solver2D_Mollier(out List<IClosed2D> _).Solve();

            //Point labels first - their priority is the default int.MinValue - in the order they were added,
            //then the curve labels by priority. This is the order the chart draws them in.
            List<object> tags_Expected = new List<object>();
            for (int i = 0; i < 20; i++)
            {
                tags_Expected.Add(string.Format("point {0}", i));
            }

            tags_Expected.Add("curve 2");
            tags_Expected.Add("curve 3");
            tags_Expected.Add("curve 4");

            Assert.Equal(tags_Expected, solver2DResults_1.ConvertAll(x => x.Tag));

            AssertSamePlacement(solver2DResults_1, solver2DResults_2);

            //A label the solver accepted never sits on an obstacle.
            foreach (Solver2DResult solver2DResult in solver2DResults_1)
            {
                Rectangle2D rectangle2D = solver2DResult.Closed2D<Rectangle2D>();
                if (solver2DResult.ResultType != Solver2DResultType.Solved || rectangle2D == null)
                {
                    continue;
                }

                Assert.DoesNotContain(true, obstacle2Ds.ConvertAll(x => x.InRange(rectangle2D)));
            }

            //And the curve labels, which take the Polyline2D branch, were actually placed.
            Assert.All(solver2DResults_1.GetRange(20, 3), x => Assert.Equal(Solver2DResultType.Solved, x.ResultType));
        }

        // --- Budget calibration ---------------------------------------------------------------------

        /// <summary>
        /// A healthy plan-sized solve stays an order of magnitude inside <see cref="Solver2D.DefaultWorkBudget"/>,
        /// so the cap never changes a real drawing's layout. This is the measurement the default is
        /// calibrated against: 5 000 labels arranged as a floor plan's spaces are, each with a limit area
        /// and the floor plan's own IterationCount of 100. It cost 9 900 units when the budget was set.
        /// </summary>
        [Fact]
        public void Solve_HealthyPlanSizedInput_StaysWellInsideTheDefaultBudget()
        {
            Solver2D solver2D = new Solver2D(Area(2000), new List<IClosed2D>());

            for (int i = 0; i < 5000; i++)
            {
                //Laid out on a 5 m grid, as rooms are - each label has room to place at or near its anchor.
                Point2D point2D = new Point2D((i % 50) * 5, (i / 50) * 5);

                Solver2DData solver2DData = new Solver2DData(new Rectangle2D(new Point2D(point2D.X - 1, point2D.Y - 0.15), 2, 0.3), point2D);
                solver2DData.Tag = i;
                solver2DData.Solver2DSettings = new Solver2DSettings()
                {
                    StartingDistance = 0,
                    ShiftDistance = 0.04,
                    IterationCount = 100,
                    LimitArea = new Rectangle2D(new Point2D(point2D.X - 2, point2D.Y - 2), 4, 4),
                };

                solver2D.Add(solver2DData);
            }

            List<Solver2DResult> solver2DResults = solver2D.Solve();

            testOutputHelper.WriteLine(string.Format("5000 labels: {0} work units, budget {1}", solver2D.WorkUnits, Solver2D.DefaultWorkBudget));

            Assert.All(solver2DResults, x => Assert.Equal(Solver2DResultType.Solved, x.ResultType));
            Assert.True(solver2D.WorkUnits < Solver2D.DefaultWorkBudget / 10, string.Format("{0} work units is not an order of magnitude inside the {1} default budget", solver2D.WorkUnits, Solver2D.DefaultWorkBudget));
        }

        /// <summary>
        /// An expensive layout is still stopped by the budget, and the items it stops are reported as
        /// Fallback rather than passed off as placed. The budget is the backstop for whatever input costs
        /// more than foreseen, so it has to keep working even though the layouts it was calibrated on no
        /// longer reach it.
        /// <para>
        /// This used to be 400 labels sharing one anchor, which cost 620 263 units and was the reason the
        /// budget existed. Since SAM_UI #58 that input costs 1 336 units (see
        /// <see cref="Solve_CoincidentAnchors_SolveWithoutTheBudget"/>) and would never reach a budget of
        /// 20 000, so the test would have stopped testing the budget. A tight cluster of mixed-width labels
        /// is still the most expensive input there is - its cost grows with the square of the labels that
        /// find room in it - so it is what exercises the mechanism now.
        /// </para>
        /// </summary>
        [Fact]
        public void Solve_DegenerateLayout_IsStoppedByTheBudgetAndSaysSo()
        {
            Solver2D solver2D = new Solver2D(Infinite(), new List<IClosed2D>());

            //A small explicit budget so the test measures the mechanism rather than spending the default.
            solver2D.WorkBudget = 20000;

            solver2D.AddRange(Labels_Cluster(400, 2));

            List<Solver2DResult> solver2DResults = solver2D.Solve();

            testOutputHelper.WriteLine(string.Format("400 clustered labels: {0} work units", solver2D.WorkUnits));

            Assert.Contains(Solver2DResultType.Fallback, solver2DResults.ConvertAll(x => x.ResultType));

            //Bounded: once over budget every remaining item costs nothing, so the overrun is one item's worth.
            Assert.True(solver2D.WorkUnits < 200000, string.Format("{0} work units overran the 20000 budget by more than one item's search", solver2D.WorkUnits));
        }

        /// <summary>
        /// The grid path, above 256 items, orders and places identically twice over as well. It shares the
        /// ordering fix with the linear path but not the overlap test, so it is worth its own lock.
        /// </summary>
        [Fact]
        public void Solve_GridPath_IsDeterministic()
        {
            List<Solver2DResult> solver2DResults_1 = Solver2D_Grid().Solve();
            List<Solver2DResult> solver2DResults_2 = Solver2D_Grid().Solve();

            Assert.Equal(300, solver2DResults_1.Count);
            AssertSamePlacement(solver2DResults_1, solver2DResults_2);
        }

        // --- Clustered anchors (SAM_UI #58) ------------------------------------------------------------
        //
        // The search skips candidates whose rejection it already knows, and the spatial index returns only
        // the placed rectangles that can overlap. Both are meant to change nothing but the cost. The oracle
        // test proves that against an independent transcription of the search; the others lock the cost.

        /// <summary>
        /// Every input solves to exactly what the plain search - every candidate tested, every placed
        /// rectangle scanned, nothing skipped - places. The reference is a transcription of the search as it
        /// stood before the clustered-anchor work, kept deliberately naive, and both sides run without a
        /// budget so the comparison is of the search itself.
        /// <para>
        /// The inputs are the ones the skips were built for and the ones that could trip them up: a pile of
        /// identical labels, a pile of mixed widths, anchors a hair apart, a tight cluster, identical labels
        /// whose limit areas are equal but separate objects, a run of unplaceable labels long enough to switch
        /// the search to its single-ring mode, an outlier label far larger than the grid's cells, labels
        /// turned off the axes, and the Mollier chart's points, curve labels and obstacles.
        /// </para>
        /// </summary>
        [Theory]
        [InlineData("pile")]
        [InlineData("mixed pile")]
        [InlineData("near-coincident")]
        [InlineData("cluster")]
        [InlineData("equal limit areas")]
        [InlineData("unplaceable run")]
        [InlineData("outlier")]
        [InlineData("rotated")]
        [InlineData("mollier")]
        [InlineData("priorities")]
        [InlineData("no grid")]
        public void Solve_PlacesExactlyWhatThePlainSearchPlaces(string name)
        {
            List<IClosed2D> obstacle2Ds = new List<IClosed2D>();
            IClosed2D area = Infinite();
            List<Solver2DData> solver2DDatas = Labels_Oracle(name, ref area, obstacle2Ds);

            Solver2D solver2D = new Solver2D(area, obstacle2Ds);
            solver2D.WorkBudget = 0;
            solver2D.AddRange(solver2DDatas);

            List<Solver2DResult> solver2DResults = solver2D.Solve();
            List<Solver2DResult> solver2DResults_Reference = new ReferenceSolver2D(area, obstacle2Ds).Solve(solver2DDatas);

            testOutputHelper.WriteLine(string.Format("{0}: {1} items, {2} work units", name, solver2DDatas.Count, solver2D.WorkUnits));

            AssertSamePlacement(solver2DResults_Reference, solver2DResults);
        }

        /// <summary>
        /// Identical labels sharing one anchor - the calibration case the budget was set by, at the floor
        /// plan's IterationCount of 100 - solve within the default budget, with nothing falling back. It cost
        /// 620 263 units before SAM_UI #58, past the budget, and the last 164 of the 400 labels were dropped at
        /// the anchor; it costs 1 336 now. The cost also stops growing with the label count once the search
        /// has filled its reach: 1 600 labels cost what 400 do.
        /// </summary>
        [Fact]
        public void Solve_CoincidentAnchors_SolveWithoutTheBudget()
        {
            Solver2D solver2D_400 = Solver2D_Coincident(400);
            List<Solver2DResult> solver2DResults_400 = solver2D_400.Solve();

            Solver2D solver2D_1600 = Solver2D_Coincident(1600);
            List<Solver2DResult> solver2DResults_1600 = solver2D_1600.Solve();

            testOutputHelper.WriteLine(string.Format("400 coincident labels: {0} work units; 1600: {1}", solver2D_400.WorkUnits, solver2D_1600.WorkUnits));

            Assert.DoesNotContain(Solver2DResultType.Fallback, solver2DResults_400.ConvertAll(x => x.ResultType));
            Assert.DoesNotContain(Solver2DResultType.Fallback, solver2DResults_1600.ConvertAll(x => x.ResultType));

            //1 336 measured; the bound leaves room for a change of grid, not for the square coming back.
            Assert.True(solver2D_400.WorkUnits < 5000, string.Format("{0} work units for 400 coincident labels", solver2D_400.WorkUnits));
            Assert.True(solver2D_1600.WorkUnits < 2 * solver2D_400.WorkUnits, string.Format("{0} work units for 1600 coincident labels against {1} for 400", solver2D_1600.WorkUnits, solver2D_400.WorkUnits));

            //The first 400 of the 1 600 are the same labels in the same order, so they land in the same places.
            AssertSamePlacement(solver2DResults_400, solver2DResults_1600.GetRange(0, 400));
        }

        /// <summary>
        /// The floor plan's own shape of pile - labels of mixed widths, each with its room as limit area - on
        /// anchors that coincide, nearly coincide, or crowd a 4 m cluster, all solve within the default budget
        /// with nothing falling back. Before SAM_UI #58 each of them spent the whole budget and dropped most of
        /// its labels at their anchors; uncapped they cost 0.8 to 6.2 million units.
        /// </summary>
        [Theory]
        [InlineData("own rooms", 400, 30000)]
        [InlineData("near-coincident", 400, 150000)]
        [InlineData("cluster", 400, 150000)]
        public void Solve_ClusteredFloorPlanLabels_SolveWithoutTheBudget(string name, int count, long workUnits_Max)
        {
            Solver2D solver2D = new Solver2D(Infinite(), new List<IClosed2D>());
            solver2D.AddRange(Labels_Clustered(name, count));

            List<Solver2DResult> solver2DResults = solver2D.Solve();

            testOutputHelper.WriteLine(string.Format("{0} x{1}: {2} work units, {3} solved, {4} unplaced", name, count, solver2D.WorkUnits, solver2DResults.FindAll(x => x.ResultType == Solver2DResultType.Solved).Count, solver2DResults.FindAll(x => x.ResultType == Solver2DResultType.Unplaced).Count));

            Assert.DoesNotContain(Solver2DResultType.Fallback, solver2DResults.ConvertAll(x => x.ResultType));
            Assert.True(solver2D.WorkUnits < workUnits_Max, string.Format("{0} work units against a bound of {1}", solver2D.WorkUnits, workUnits_Max));
        }

        /// <summary>
        /// Four times the labels in the same near-coincident pile cost barely more: once the search has filled
        /// its reach, a further label fails within its first ring and the cost stops growing.
        /// </summary>
        [Fact]
        public void Solve_NearCoincidentAnchors_CostStopsGrowingWithTheLabelCount()
        {
            Solver2D solver2D_400 = new Solver2D(Infinite(), new List<IClosed2D>());
            solver2D_400.AddRange(Labels_Clustered("near-coincident", 400));
            solver2D_400.Solve();

            Solver2D solver2D_1600 = new Solver2D(Infinite(), new List<IClosed2D>());
            solver2D_1600.AddRange(Labels_Clustered("near-coincident", 1600));
            List<Solver2DResult> solver2DResults_1600 = solver2D_1600.Solve();

            testOutputHelper.WriteLine(string.Format("near-coincident: 400 labels {0} work units, 1600 labels {1}", solver2D_400.WorkUnits, solver2D_1600.WorkUnits));

            Assert.DoesNotContain(Solver2DResultType.Fallback, solver2DResults_1600.ConvertAll(x => x.ResultType));
            Assert.True(solver2D_1600.WorkUnits < 1.5 * solver2D_400.WorkUnits, string.Format("{0} work units for 1600 labels against {1} for 400", solver2D_1600.WorkUnits, solver2D_400.WorkUnits));
        }

        /// <summary>
        /// A clustered solve repeats exactly - positions, result types and the work it took - both from a
        /// fresh instance and from a second call on the same one. The second call matters here: the solve
        /// keeps state across items (what each search has already ruled out), and none of it may leak into
        /// the next call.
        /// </summary>
        [Fact]
        public void Solve_ClusteredInput_IsDeterministic()
        {
            Solver2D solver2D_1 = new Solver2D(Infinite(), new List<IClosed2D>());
            solver2D_1.AddRange(Labels_Clustered("near-coincident", 300));
            List<Solver2DResult> solver2DResults_1 = solver2D_1.Solve();
            long workUnits_1 = solver2D_1.WorkUnits;

            Solver2D solver2D_2 = new Solver2D(Infinite(), new List<IClosed2D>());
            solver2D_2.AddRange(Labels_Clustered("near-coincident", 300));
            List<Solver2DResult> solver2DResults_2 = solver2D_2.Solve();
            List<Solver2DResult> solver2DResults_3 = solver2D_2.Solve();

            AssertSamePlacement(solver2DResults_1, solver2DResults_2);
            AssertSamePlacement(solver2DResults_1, solver2DResults_3);
            Assert.Equal(workUnits_1, solver2D_2.WorkUnits);
        }

        /// <summary>
        /// A healthy floor plan's labels land exactly where they did before the clustered-anchor work: every
        /// position of a 2 000-label plan, built the way the floor plan builds its labels (mixed widths, a room
        /// as each label's limit area, ShiftDistance a hundredth of the room's reach), hashes to the value
        /// recorded from the solver as it was. The same for the 5 000-label plan the budget is calibrated on.
        /// </summary>
        [Fact]
        public void Solve_HealthyPlans_PlaceExactlyAsBefore()
        {
            Solver2D solver2D_FloorPlan = new Solver2D(Infinite(), new List<IClosed2D>());
            solver2D_FloorPlan.AddRange(Labels_HealthyFloorPlan());
            List<Solver2DResult> solver2DResults_FloorPlan = solver2D_FloorPlan.Solve();

            Solver2D solver2D_Grid = Solver2D_HealthyGrid();
            List<Solver2DResult> solver2DResults_Grid = solver2D_Grid.Solve();

            testOutputHelper.WriteLine(string.Format("2000-label floor plan: {0} work units; 5000-label grid: {1}", solver2D_FloorPlan.WorkUnits, solver2D_Grid.WorkUnits));

            Assert.All(solver2DResults_FloorPlan, x => Assert.Equal(Solver2DResultType.Solved, x.ResultType));
            Assert.All(solver2DResults_Grid, x => Assert.Equal(Solver2DResultType.Solved, x.ResultType));

            //Recorded from Solver2D at sow/2026-Q3 64a735f2, before SAM_UI #58.
            Assert.Equal("D3695E2C874C3279", Fingerprint(solver2DResults_FloorPlan));
            Assert.Equal("8C81DF1132F81649", Fingerprint(solver2DResults_Grid));
        }

        // --- Helpers ---------------------------------------------------------------------------------

        private static Rectangle2D Area(double size = 100)
        {
            return new Rectangle2D(new Point2D(-size, -size), size * 3, size * 3);
        }

        /// <summary>The floor plan's own area: unbounded, so labels may extend beyond the rooms.</summary>
        private static Rectangle2D Infinite()
        {
            return new Rectangle2D(new Point2D(double.MinValue / 2, double.MinValue / 2), double.MaxValue, double.MaxValue);
        }

        private static Face2D Room(double x, double y, double width, double height)
        {
            return new Face2D(new Polygon2D(new List<Point2D>()
            {
                new Point2D(x - (width / 2), y - (height / 2)),
                new Point2D(x + (width / 2), y - (height / 2)),
                new Point2D(x + (width / 2), y + (height / 2)),
                new Point2D(x - (width / 2), y + (height / 2)),
            }));
        }

        /// <summary>
        /// A space label as the floor plan builds it (GeometryObjectModel): the rectangle off-centre from its
        /// anchor (Solve re-centres it), the room as limit area, and ShiftDistance a hundredth of the room's
        /// reach from the anchor.
        /// </summary>
        private static Solver2DData Label(Point2D point2D, double width, double height, Face2D face2D, object tag, int iterationCount = 100)
        {
            double distance_Farthest = 0;
            foreach (Point2D point2D_Room in ((Polygon2D)face2D.ExternalEdge2D).Points)
            {
                distance_Farthest = System.Math.Max(distance_Farthest, point2D_Room.Distance(point2D));
            }

            Solver2DData result = new Solver2DData(new Rectangle2D(new Point2D(point2D.X + (width / 2), point2D.Y + (height / 2)), width, height), point2D);
            result.Tag = tag;
            result.Solver2DSettings = new Solver2DSettings()
            {
                StartingDistance = 0,
                IterationCount = iterationCount,
                ShiftDistance = distance_Farthest / iterationCount,
                LimitArea = face2D,
            };

            return result;
        }

        /// <summary>Label widths of 1.2 m to 6 m, as room names come out at 0.4 m text height.</summary>
        private static double LabelWidth(int index)
        {
            return 0.4 * 0.6 * (5 + ((index * 13) % 21));
        }

        /// <summary>A deterministic spread in [-1, 1].</summary>
        private static double Spread(int index, double factor)
        {
            double value = index * factor;
            return (2 * (value - System.Math.Floor(value))) - 1;
        }

        /// <summary>Identical labels on one anchor at the floor plan's IterationCount of 100.</summary>
        private static Solver2D Solver2D_Coincident(int count)
        {
            Solver2D result = new Solver2D(Area(), new List<IClosed2D>());

            for (int i = 0; i < count; i++)
            {
                Solver2DData solver2DData = Data(new Point2D(0, 0), i);
                solver2DData.Solver2DSettings.IterationCount = 100;

                result.Add(solver2DData);
            }

            return result;
        }

        /// <summary>
        /// Mixed-width floor-plan labels piled on one point: "own rooms" each with a 10 m room of its own,
        /// "near-coincident" jittered by 4 mm inside one shared 40 m room, "cluster" spread over a 4 m square.
        /// </summary>
        private static List<Solver2DData> Labels_Clustered(string name, int count)
        {
            List<Solver2DData> result = new List<Solver2DData>();

            Face2D face2D_Shared = Room(0, 0, 40, 40);
            for (int i = 0; i < count; i++)
            {
                switch (name)
                {
                    case "own rooms":
                        result.Add(Label(new Point2D(0, 0), LabelWidth(i), 0.4, Room(0, 0, 10, 10), i));
                        break;

                    case "near-coincident":
                        result.Add(Label(new Point2D(0.004 * Spread(i, 0.6180339887), 0.004 * Spread(i, 0.7548776662)), LabelWidth(i), 0.4, face2D_Shared, i));
                        break;

                    case "cluster":
                        result.Add(Label(new Point2D(2 * Spread(i, 0.6180339887), 2 * Spread(i, 0.7548776662)), LabelWidth(i), 0.4, face2D_Shared, i));
                        break;

                    default:
                        throw new System.ArgumentException(name);
                }
            }

            return result;
        }

        private static List<Solver2DData> Labels_Cluster(int count, double halfSize)
        {
            List<Solver2DData> result = new List<Solver2DData>();

            Face2D face2D = Room(0, 0, 40, 40);
            for (int i = 0; i < count; i++)
            {
                result.Add(Label(new Point2D(halfSize * Spread(i, 0.6180339887), halfSize * Spread(i, 0.7548776662)), LabelWidth(i), 0.4, face2D, i));
            }

            return result;
        }

        /// <summary>
        /// A healthy floor plan: 2 000 rooms of 3 m to 8 m on a 9 m grid, each labelled at its centre.
        /// </summary>
        private static List<Solver2DData> Labels_HealthyFloorPlan()
        {
            List<Solver2DData> result = new List<Solver2DData>();

            for (int i = 0; i < 2000; i++)
            {
                double x = (i % 40) * 9;
                double y = (i / 40) * 9;

                result.Add(Label(new Point2D(x, y), LabelWidth(i), 0.4, Room(x, y, 3 + ((i * 7) % 6), 3 + ((i * 5) % 6)), i));
            }

            return result;
        }

        /// <summary>The 5 000-label plan the default budget is calibrated on.</summary>
        private static Solver2D Solver2D_HealthyGrid()
        {
            Solver2D result = new Solver2D(Area(2000), new List<IClosed2D>());

            for (int i = 0; i < 5000; i++)
            {
                Point2D point2D = new Point2D((i % 50) * 5, (i / 50) * 5);

                Solver2DData solver2DData = new Solver2DData(new Rectangle2D(new Point2D(point2D.X - 1, point2D.Y - 0.15), 2, 0.3), point2D);
                solver2DData.Tag = i;
                solver2DData.Solver2DSettings = new Solver2DSettings()
                {
                    StartingDistance = 0,
                    ShiftDistance = 0.04,
                    IterationCount = 100,
                    LimitArea = new Rectangle2D(new Point2D(point2D.X - 2, point2D.Y - 2), 4, 4),
                };

                result.Add(solver2DData);
            }

            return result;
        }

        /// <summary>The inputs of <see cref="Solve_PlacesExactlyWhatThePlainSearchPlaces"/>, kept small enough for the plain search.</summary>
        private static List<Solver2DData> Labels_Oracle(string name, ref IClosed2D area, List<IClosed2D> obstacle2Ds)
        {
            List<Solver2DData> result = new List<Solver2DData>();

            switch (name)
            {
                case "pile":
                    area = Area();
                    for (int i = 0; i < 80; i++)
                    {
                        Solver2DData solver2DData = Data(new Point2D(0, 0), i);
                        solver2DData.Solver2DSettings.IterationCount = 40;
                        result.Add(solver2DData);
                    }
                    break;

                case "mixed pile":
                    Face2D face2D_Pile = Room(0, 0, 16, 16);
                    for (int i = 0; i < 80; i++)
                    {
                        result.Add(Label(new Point2D(0, 0), LabelWidth(i), 0.4, face2D_Pile, i, 40));
                    }
                    break;

                case "near-coincident":
                    Face2D face2D_Near = Room(0, 0, 16, 16);
                    for (int i = 0; i < 80; i++)
                    {
                        result.Add(Label(new Point2D(0.004 * Spread(i, 0.6180339887), 0.004 * Spread(i, 0.7548776662)), LabelWidth(i), 0.4, face2D_Near, i, 40));
                    }
                    break;

                case "cluster":
                    Face2D face2D_Cluster = Room(0, 0, 16, 16);
                    for (int i = 0; i < 80; i++)
                    {
                        result.Add(Label(new Point2D(2 * Spread(i, 0.6180339887), 2 * Spread(i, 0.7548776662)), LabelWidth(i), 0.4, face2D_Cluster, i, 40));
                    }
                    break;

                case "equal limit areas":
                    //Equal geometry, separate objects: the searches must not be treated as one.
                    for (int i = 0; i < 60; i++)
                    {
                        result.Add(Label(new Point2D(0, 0), 2, 0.4, Room(0, 0, 12, 12), i, 40));
                    }
                    break;

                case "unplaceable run":
                    //Forty labels whose rooms lie out of reach, then forty that can place: the run switches
                    //the search to a single ring, and a success switches it back.
                    for (int i = 0; i < 40; i++)
                    {
                        Solver2DData solver2DData = Data(new Point2D(0, 0), i);
                        solver2DData.Solver2DSettings.LimitArea = new Rectangle2D(new Point2D(40, 40), 1, 1);
                        result.Add(solver2DData);
                    }

                    for (int i = 40; i < 80; i++)
                    {
                        result.Add(Data(new Point2D(0, 0), i));
                    }

                    for (int i = 80; i < 120; i++)
                    {
                        Solver2DData solver2DData = Data(new Point2D(0, 0), i);
                        solver2DData.Solver2DSettings.LimitArea = new Rectangle2D(new Point2D(40, 40), 1, 1);
                        result.Add(solver2DData);
                    }
                    break;

                case "outlier":
                    //One label hundreds of times the size of the rest: far more cells than the grid spreads one
                    //rectangle over.
                    Face2D face2D_Outlier = Room(0, 0, 16, 16);
                    for (int i = 0; i < 60; i++)
                    {
                        double width = i == 10 ? 400 : LabelWidth(i);
                        result.Add(Label(new Point2D(Spread(i, 0.6180339887), Spread(i, 0.7548776662)), width, i == 10 ? 30 : 0.4, face2D_Outlier, i, 40));
                    }
                    break;

                case "rotated":
                    //Every third label turned off the axes, where a bounding box is not the rectangle.
                    area = Area();
                    for (int i = 0; i < 60; i++)
                    {
                        Point2D point2D = new Point2D(0.5 * Spread(i, 0.6180339887), 0.5 * Spread(i, 0.7548776662));
                        Vector2D heightDirection = i % 3 == 0 ? new Vector2D(0.6, 0.8) : new Vector2D(0, 1);

                        Solver2DData solver2DData = new Solver2DData(new Rectangle2D(point2D, 1.5, 0.5, heightDirection), point2D);
                        solver2DData.Tag = i;
                        solver2DData.Solver2DSettings = new Solver2DSettings()
                        {
                            StartingDistance = 0,
                            ShiftDistance = 0.2,
                            IterationCount = 30,
                        };

                        result.Add(solver2DData);
                    }
                    break;

                case "mollier":
                    Solver2D_Mollier(out List<IClosed2D> obstacle2Ds_Mollier, out List<Solver2DData> solver2DDatas_Mollier, out area);
                    obstacle2Ds.AddRange(obstacle2Ds_Mollier);
                    result.AddRange(solver2DDatas_Mollier);
                    break;

                case "priorities":
                    //Priorities out of insertion order, with ties, around one anchor and against obstacles.
                    area = Area();
                    obstacle2Ds.Add(new Circle2D(new Point2D(1.5, 0), 0.5));
                    obstacle2Ds.Add(new Rectangle2D(new Point2D(-3, -0.5), 1, 1));
                    for (int i = 0; i < 60; i++)
                    {
                        Solver2DData solver2DData = Data(new Point2D(0, 0), i);
                        solver2DData.Priority = (i * 7) % 5;
                        solver2DData.Solver2DSettings.IterationCount = 30;
                        result.Add(solver2DData);
                    }
                    break;

                case "no grid":
                    //Labels too small to size the spatial index by - under the distance tolerance - so the
                    //solve takes the linear scan it keeps for exactly this input.
                    area = Area();
                    for (int i = 0; i < 40; i++)
                    {
                        Point2D point2D = new Point2D(1e-7 * (i % 3), 0);

                        Solver2DData solver2DData = new Solver2DData(new Rectangle2D(point2D, 4e-7, 2e-7), point2D);
                        solver2DData.Tag = i;
                        solver2DData.Solver2DSettings = new Solver2DSettings()
                        {
                            StartingDistance = 0,
                            ShiftDistance = 1e-7,
                            IterationCount = 20,
                        };

                        result.Add(solver2DData);
                    }
                    break;

                default:
                    throw new System.ArgumentException(name);
            }

            return result;
        }

        /// <summary>
        /// A hash of every result's tag, type and rectangle (to 1e-9), so a test can lock a large layout to
        /// the exact positions it had.
        /// </summary>
        private static string Fingerprint(List<Solver2DResult> solver2DResults)
        {
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
            foreach (Solver2DResult solver2DResult in solver2DResults)
            {
                Rectangle2D rectangle2D = solver2DResult.Closed2D<Rectangle2D>();

                stringBuilder.Append(solver2DResult.Tag).Append(';').Append(solver2DResult.ResultType).Append(';');
                if (rectangle2D != null)
                {
                    stringBuilder.Append(Round(rectangle2D.Origin.X)).Append(',').Append(Round(rectangle2D.Origin.Y)).Append(',').Append(Round(rectangle2D.Width)).Append(',').Append(Round(rectangle2D.Height));
                }

                stringBuilder.Append('\n');
            }

            byte[] hash = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(stringBuilder.ToString()));

            return System.Convert.ToHexString(hash).Substring(0, 16);
        }

        private static string Round(double value)
        {
            return System.Math.Round(value, 9).ToString("R", System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// The search with nothing clever in it: every candidate tested, every placed rectangle scanned, no
        /// budget. A transcription of Solver2D.Solve as it stood at sow/2026-Q3 64a735f2, before SAM_UI #58,
        /// minus the budget and the spatial index (which only ever narrowed the same scan). It is the oracle
        /// the optimised search must agree with, so it must stay this plain.
        /// </summary>
        private sealed class ReferenceSolver2D
        {
            private readonly IClosed2D area;
            private readonly List<IClosed2D> obstacle2Ds;

            public ReferenceSolver2D(IClosed2D area, List<IClosed2D> obstacle2Ds)
            {
                this.area = area;
                this.obstacle2Ds = obstacle2Ds;
            }

            public List<Solver2DResult> Solve(List<Solver2DData> solver2DDatas)
            {
                List<int> indexes = Enumerable.Range(0, solver2DDatas.Count).ToList();
                indexes.Sort((x, y) =>
                {
                    int compare = solver2DDatas[x].Priority.CompareTo(solver2DDatas[y].Priority);
                    return compare != 0 ? compare : x.CompareTo(y);
                });

                List<Vector2D> offsets = new List<Vector2D>();
                foreach (double angle in new double[] { 0, 90, 180, 270, 45, 135, 225, 315 })
                {
                    double radians = System.Math.PI * angle / 180;
                    offsets.Add(new Vector2D(System.Math.Sin(radians), System.Math.Cos(radians)));
                }

                List<Solver2DResult> result = new List<Solver2DResult>();
                List<Rectangle2D> placed = new List<Rectangle2D>();
                int consecutiveUnplaced = 0;

                foreach (int index in indexes)
                {
                    Solver2DData solver2DData = solver2DDatas[index];
                    Rectangle2D rectangle2D = solver2DData.Closed2D<Rectangle2D>();
                    Solver2DSettings solver2DSettings = solver2DData.Solver2DSettings;

                    double iterationCount = solver2DSettings.ShiftDistance > 0 ? solver2DSettings.IterationCount : 1;
                    if (consecutiveUnplaced >= 32)
                    {
                        iterationCount = 1;
                    }

                    Rectangle2D? rectangle2D_Result = null;
                    if (solver2DData.Geometry2D<ISAMGeometry2D>() is Point2D point2D)
                    {
                        Rectangle2D rectangle2D_Centred = rectangle2D.GetMoved(new Vector2D(rectangle2D.GetCentroid(), point2D));
                        for (int i = 0; i < iterationCount && rectangle2D_Result == null; i++)
                        {
                            foreach (Vector2D offset in offsets)
                            {
                                Rectangle2D rectangle2D_Candidate = rectangle2D_Centred.GetMoved(offset * (solver2DSettings.StartingDistance + (i * solver2DSettings.ShiftDistance)));
                                if (Accepts(rectangle2D_Candidate, solver2DSettings, placed))
                                {
                                    rectangle2D_Result = rectangle2D_Candidate;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        Polyline2D polyline2D = solver2DData.Geometry2D<Polyline2D>();
                        Point2D point2D_Closest = polyline2D.Closest(rectangle2D.GetCentroid());
                        double distanceToCenter = point2D_Closest.Distance(rectangle2D.GetCentroid());

                        for (int i = 0; i < iterationCount && rectangle2D_Result == null; i++)
                        {
                            for (int j = -1; j <= 1; j += 2)
                            {
                                double x = point2D_Closest.X + i * j * solver2DSettings.ShiftDistance;
                                double y = Y(polyline2D, x);
                                if (double.IsNaN(y))
                                {
                                    continue;
                                }

                                Point2D point2D_New = new Point2D(x, y);
                                List<Segment2D> segment2Ds = polyline2D.ClosestSegment2Ds(point2D_New);
                                if (segment2Ds == null)
                                {
                                    continue;
                                }

                                Segment2D segment2D = segment2Ds[0];
                                bool clockwise = segment2D.Direction.GetPerpendicular().Y < 0;

                                Rectangle2D rectangle2D_Candidate = Query.MoveToSegment2D(rectangle2D, segment2D, point2D_New, distanceToCenter, clockwise);
                                if (rectangle2D_Candidate != null && System.Math.Abs(rectangle2D.Width - rectangle2D_Candidate.Width) >= SAM.Core.Tolerance.MacroDistance)
                                {
                                    rectangle2D_Candidate = new Rectangle2D(rectangle2D_Candidate.Origin, -rectangle2D_Candidate.Height, rectangle2D_Candidate.Width, rectangle2D_Candidate.WidthDirection);
                                }

                                if (rectangle2D_Candidate != null && Accepts(rectangle2D_Candidate, solver2DSettings, placed))
                                {
                                    rectangle2D_Result = rectangle2D_Candidate;
                                    break;
                                }
                            }
                        }
                    }

                    result.Add(new Solver2DResult(solver2DData, rectangle2D_Result, rectangle2D_Result == null ? Solver2DResultType.Unplaced : Solver2DResultType.Solved));

                    if (rectangle2D_Result == null)
                    {
                        consecutiveUnplaced++;
                    }
                    else
                    {
                        consecutiveUnplaced = 0;
                        placed.Add(rectangle2D_Result);
                    }
                }

                return result;
            }

            private bool Accepts(Rectangle2D rectangle2D, Solver2DSettings solver2DSettings, List<Rectangle2D> placed)
            {
                if (!area.Inside(rectangle2D))
                {
                    return false;
                }

                if (obstacle2Ds != null && obstacle2Ds.Exists(x => x.InRange(rectangle2D)))
                {
                    return false;
                }

                if (placed.Exists(x => x.InRange(rectangle2D) || rectangle2D.InRange(x)))
                {
                    return false;
                }

                return solver2DSettings.LimitArea == null || solver2DSettings.LimitArea.Inside(rectangle2D.GetCentroid());
            }

            private static double Y(Polyline2D polyline2D, double x)
            {
                Segment2D? segment2D = polyline2D.Segment2Ds().Find(s => s.Min.X <= x && x <= s.Max.X);
                if (segment2D == null)
                {
                    return double.NaN;
                }

                List<Point2D> point2Ds = segment2D.GetPoints();
                if (point2Ds == null || point2Ds.Count < 2)
                {
                    return double.NaN;
                }

                SAM.Math.LinearEquation linearEquation = SAM.Math.Create.LinearEquation(point2Ds[0].X, point2Ds[0].Y, point2Ds[1].X, point2Ds[1].Y);

                return linearEquation == null ? double.NaN : linearEquation.Evaluate(x);
            }
        }

        private static Solver2DData Data(Point2D point2D, object tag)
        {
            Solver2DData result = new Solver2DData(new Rectangle2D(new Point2D(point2D.X - 1, point2D.Y - 0.5), 2, 1), point2D);

            result.Tag = tag;
            result.Solver2DSettings = new Solver2DSettings()
            {
                StartingDistance = 0,
                ShiftDistance = 0.5,
                IterationCount = 10,
            };

            return result;
        }

        /// <summary>Ten items on one anchor, so every one after the first has to be displaced.</summary>
        private static Solver2D Solver2D_Crowded()
        {
            Solver2D result = new Solver2D(Area(), new List<IClosed2D>());

            for (int i = 0; i < 10; i++)
            {
                result.Add(Data(new Point2D(0, 0), string.Format("tag {0}", i)));
            }

            return result;
        }

        /// <summary>
        /// A Mollier chart's shape of input: twenty point labels at the default priority with a circle
        /// obstacle each, and three curve labels anchored to a polyline at priorities 2, 3 and 4.
        /// </summary>
        private static Solver2D Solver2D_Mollier(out List<IClosed2D> obstacle2Ds)
        {
            Solver2D_Mollier(out obstacle2Ds, out List<Solver2DData> solver2DDatas, out IClosed2D area);

            Solver2D result = new Solver2D(area, obstacle2Ds);
            result.AddRange(solver2DDatas);

            return result;
        }

        private static void Solver2D_Mollier(out List<IClosed2D> obstacle2Ds, out List<Solver2DData> solver2DDatas, out IClosed2D area)
        {
            obstacle2Ds = new List<IClosed2D>();

            solver2DDatas = new List<Solver2DData>();

            for (int i = 0; i < 20; i++)
            {
                Point2D point2D = new Point2D(i * 2.5, (i % 3) * 1.5);

                obstacle2Ds.Add(new Circle2D(point2D, 0.14));

                Solver2DData solver2DData = new Solver2DData(new Rectangle2D(new Point2D(point2D.X - 0.6, point2D.Y + 0.4), 1.2, 0.3), point2D);
                solver2DData.Tag = string.Format("point {0}", i);
                solver2DData.Solver2DSettings = new Solver2DSettings()
                {
                    StartingDistance = 0.2,
                    ShiftDistance = 0.1,
                    IterationCount = 10,
                };

                solver2DDatas.Add(solver2DData);
            }

            Polyline2D polyline2D = new Polyline2D(new List<Segment2D>
            {
                new Segment2D(new Point2D(0, -3), new Point2D(25, -1)),
                new Segment2D(new Point2D(25, -1), new Point2D(50, -3)),
            });

            for (int priority = 2; priority <= 4; priority++)
            {
                Solver2DData solver2DData = new Solver2DData(new Rectangle2D(new Point2D(24, -0.6), 2, 0.3), polyline2D);
                solver2DData.Tag = string.Format("curve {0}", priority);
                solver2DData.Priority = priority;
                solver2DData.Solver2DSettings = new Solver2DSettings()
                {
                    StartingDistance = 0,
                    ShiftDistance = 0.5,
                    IterationCount = 100,
                };

                solver2DDatas.Add(solver2DData);
            }

            area = new Rectangle2D(new BoundingBox2D(new Point2D(-10, -10), new Point2D(60, 20)));
        }

        /// <summary>Three hundred items, which is over the threshold where the spatial index is built.</summary>
        private static Solver2D Solver2D_Grid()
        {
            Solver2D result = new Solver2D(Area(500), new List<IClosed2D>());

            for (int i = 0; i < 300; i++)
            {
                //Deliberately crowded in pairs, so placement order matters and a reordering would show.
                result.Add(Data(new Point2D((i / 2) * 2.5, 0), i));
            }

            return result;
        }

        private static void AssertSamePlacement(List<Solver2DResult> solver2DResults_1, List<Solver2DResult> solver2DResults_2)
        {
            Assert.Equal(solver2DResults_1.Count, solver2DResults_2.Count);

            for (int i = 0; i < solver2DResults_1.Count; i++)
            {
                Assert.Equal(solver2DResults_1[i].Tag, solver2DResults_2[i].Tag);
                Assert.Equal(solver2DResults_1[i].ResultType, solver2DResults_2[i].ResultType);

                Rectangle2D rectangle2D_1 = solver2DResults_1[i].Closed2D<Rectangle2D>();
                Rectangle2D rectangle2D_2 = solver2DResults_2[i].Closed2D<Rectangle2D>();

                if (rectangle2D_1 == null || rectangle2D_2 == null)
                {
                    Assert.Null(rectangle2D_1);
                    Assert.Null(rectangle2D_2);
                    continue;
                }

                Assert.Equal(rectangle2D_1.Origin.X, rectangle2D_2.Origin.X, 9);
                Assert.Equal(rectangle2D_1.Origin.Y, rectangle2D_2.Origin.Y, 9);
                Assert.Equal(rectangle2D_1.Width, rectangle2D_2.Width, 9);
                Assert.Equal(rectangle2D_1.Height, rectangle2D_2.Height, 9);
            }
        }
    }
}
