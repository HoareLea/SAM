// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public class Solver2D
    {
        /// <summary>
        /// Default value of <see cref="WorkBudget"/>: the number of geometric comparisons a whole solve
        /// may make before the remaining items are dropped at their anchors.
        /// <para>
        /// This replaced a 10 000 ms wall-clock budget. The behaviour it bounds is the same, but a
        /// drawing's layout must not depend on how fast the machine that drew it is: with a stopwatch, the
        /// same saved view solved on a loaded laptop and on a build server could return different
        /// positions, and there is no way for either to know that happened. A count of comparisons is
        /// derived only from the input, so an identical input always produces an identical layout.
        /// </para>
        /// <para>
        /// Calibrated by measuring <see cref="WorkUnits"/> on the shapes the two existing consumers
        /// produce, at the floor plan's own <c>IterationCount</c> of 100 with a <c>LimitArea</c> per label:
        /// </para>
        /// <list type="bullet">
        /// <item>healthy plan, 5 000 space labels on a room-sized grid: <b>9 900</b> units, 0.4 s;</item>
        /// <item>healthy plan, 2 000 labels: <b>3 960</b> units - the cost is linear in the label count
        /// while each label places near its anchor, so a 10 000-label plan is around 20 000;</item>
        /// <item>degenerate collapse, 400 labels sharing one anchor: <b>620 263</b> units, 14.6 s - each
        /// label places, but only after spiralling out past the pile already there, and the cost grows
        /// with the square of the count.</item>
        /// </list>
        /// <para>
        /// So this sits more than an order of magnitude above the healthy case - which therefore never
        /// reaches it, and a test locks that - and bites into the degenerate one at roughly the point in
        /// time the 10 000 ms stopwatch used to. Time per unit is not constant (a candidate near a large
        /// pile costs more than one in open space), so the equivalence with the old budget is approximate
        /// by construction; that is the price of the layout not depending on the machine, and it is worth
        /// paying.
        /// </para>
        /// <para>
        /// The degenerate collapse no longer reaches it (SAM_UI #58). The search now skips the candidates it
        /// already knows are blocked and the grid returns only the rectangles that can overlap, without
        /// changing a single position, and the same inputs measure: the 400 labels sharing one anchor
        /// <b>1 336</b> units; 400 labels of mixed widths sharing one anchor <b>16 607</b>; 400 and 1 600
        /// labels jittered around one anchor about <b>90 000</b> to <b>107 000</b>; 1 600 labels in a 4 m
        /// cluster <b>191 951</b>; the healthy plan of 5 000 <b>5 000</b>. What remains in a pile grows with
        /// the square of the number of labels that actually find room in it - each still has to get past
        /// every label already there once - and that number is capped by how far the search reaches, not by
        /// how many labels there are. The budget stays where it is, as the backstop for whatever input is
        /// not foreseen here.
        /// </para>
        /// </summary>
        public const long DefaultWorkBudget = 500000;

        // How far two axis-aligned boxes must overlap on both axes before isKnownBlocked treats them as
        // certainly InRange: a thousand times the InRange tolerance, so rounding never decides it.
        private const double knownBlockedMargin = Core.Tolerance.MacroDistance;

        private List<Solver2DData> solver2DDatas;
        private List<IClosed2D> obstacles2D;
        private IClosed2D area;
        private long workBudget = DefaultWorkBudget;
        private long workUnits = 0;

        public Solver2D(IClosed2D area, List<IClosed2D> obstacles2D)
        {
            this.area = area;
            this.obstacles2D = obstacles2D;
        }

        /// <summary>
        /// Geometric comparisons the next <see cref="Solve"/> may make before it stops searching and drops
        /// each remaining item at its anchor as a <see cref="Solver2DResultType.Fallback"/>. Defaults to
        /// <see cref="DefaultWorkBudget"/>.
        /// <para>
        /// Deliberately a count and not a duration - see <see cref="DefaultWorkBudget"/>. A non-positive
        /// value removes the budget entirely, which leaves only the degenerate-layout backstop bounding the
        /// solve; use it only where the input size is known.
        /// </para>
        /// </summary>
        public long WorkBudget
        {
            get
            {
                return workBudget;
            }

            set
            {
                workBudget = value;
            }
        }

        /// <summary>
        /// Geometric comparisons the last <see cref="Solve"/> made. Deterministic for a given input, which
        /// is what makes <see cref="WorkBudget"/> testable and lets a consumer log how close a real model
        /// comes to it.
        /// <para>
        /// One unit per candidate position tested and one per obstacle or placed rectangle it is compared
        /// with. A candidate whose rejection is already known - from a rectangle that blocked this item a ring
        /// earlier, or from an earlier item with the identical search - is not tested and costs no unit; the
        /// check that knows it is a handful of arithmetic comparisons, at most 8 per candidate.
        /// </para>
        /// </summary>
        public long WorkUnits
        {
            get
            {
                return workUnits;
            }
        }


        public bool Add(Solver2DData solver2DData)
        {
            if (solver2DData == null || solver2DData.Geometry2D<ISAMGeometry2D>() == null || solver2DData.Closed2D<IClosed2D>() == null)
            {
                return false;
            }
            if (solver2DDatas == null)
            {
                solver2DDatas = new List<Solver2DData>();
            }

            solver2DDatas.Add(solver2DData);
            return true;
        }
        public bool AddRange(List<Solver2DData> solver2DDatas)
        {
            if (solver2DDatas == null)
            {
                return false;
            }

            solver2DDatas.ForEach(x => Add(x));
            return true;
        }

        public List<Solver2DResult> Solve()
        {
            if (solver2DDatas == null || solver2DDatas.Count == 0)
            {
                return null;
            }

            List<Solver2DResult> result = new List<Solver2DResult>();

            workUnits = 0;

            // Placement order, lowest Priority first, then the order the caller added the items in. It used
            // to be solver2DDatas.Sort(...) on Priority alone, which is List<T>.Sort - an unstable introsort
            // - so items of EQUAL priority were placed in an arbitrary order that varied with the number of
            // items. Placement order decides the layout (each item avoids the ones already placed), so two
            // solves of one saved drawing could return different positions. Every consumer here leaves
            // Priority at its default, i.e. all items are equal, so this was the normal case rather than an
            // edge one. Ordering by index as the tiebreak makes the comparison total, which removes the
            // dependency on the sort's stability altogether. Note the field itself is no longer re-ordered,
            // so a second Solve() of the same instance starts from the same order as the first.
            List<Solver2DData> solver2DDatas_Ordered = ordered();

            // Spatial index over already-placed rectangles. Without it Solve() is ~O(N^2): every one of
            // the up-to IterationCount*8 candidate positions per label linearly scans every previously
            // placed label (see intersect), which is ~150 s on a ~10k-label floor plan. The grid returns
            // a superset of potential overlaps - all placed rectangles sharing a cell with the candidate's
            // bounding box expanded by MacroDistance, far beyond the InRange tolerance (see RectangleGrid) -
            // and the exact InRange test in intersect is unchanged, so placement results are identical to
            // the linear scan.
            //
            // Built for every input size. It used to be built only above 256 items, on the reasoning that
            // small inputs should keep the original path; but the results do not depend on the path, and
            // the linear scan is exactly what made a SMALL pile slow - 200 labels sharing one anchor cost
            // 4.4 million comparisons (41 s) below the threshold, against 620 000 for 400 labels above it
            // (SAM_UI #58). The linear scan is kept only for the input the grid cannot be sized for.
            RectangleGrid grid = RectangleGrid.Create(solver2DDatas_Ordered);
            List<PlacedRectangle2D> placedRectangle2Ds = grid == null ? new List<PlacedRectangle2D>() : null;

            // Candidates already known to be rejected, keyed by everything that decides a candidate
            // sequence - see CandidateSequence. Two items with identical geometry and search settings
            // test the identical sequence of rectangles; everything that rejected one for the first item
            // (the area, the obstacles, the limit area, the rectangles placed so far) still rejects it
            // for the second, because none of them changes during a solve except the set of placed
            // rectangles, which only grows. The first item's ACCEPTED candidate is rejected too - it is
            // now occupied by the first item. So the second item can start where the first one stopped,
            // with an identical result. Without this, N labels sharing one anchor re-walk the same pile
            // N times: 620 263 comparisons for 400 of them, 1 336 with it (SAM_UI #58).
            Dictionary<CandidateSequence, int> rejectedCandidateCounts = new Dictionary<CandidateSequence, int>();

            // Per item: the placed rectangle that last blocked each of the 8 search directions - see
            // isKnownBlocked. Reused across items; cleared for each.
            PlacedRectangle2D[] blockers = new PlacedRectangle2D[8];

            // Degenerate-layout backstop. Each label that cannot be placed first runs its full
            // IterationCount * 8 candidate sweep before giving up; when a whole batch is unplaceable (e.g. a
            // floor-plan section taken at the wrong elevation collapses every space to a sliver, so no label
            // centre fits its LimitArea) that is an O(N * IterationCount) blow-up - a ~2-minute hang on a 10k
            // -label plan. A long run of consecutive failures means the layout is degenerate, so once it is
            // hit we stop sweeping and give each remaining label a single anchor attempt. The counter resets
            // on any successful placement, so a normal plan with the odd unplaceable label is unaffected.
            const int maxConsecutiveUnplaced = 32;
            int consecutiveUnplaced = 0;

            // The 8 search directions are identical for every label, so build them once rather than per label.
            List<Vector2D> offsets = generateOffsets();

            foreach (Solver2DData solver2DData in solver2DDatas_Ordered)
            {
                Rectangle2D rectangle2D = solver2DData.Closed2D<Rectangle2D>();
                Solver2DSettings solver2DSettings = solver2DData.Solver2DSettings;
                if (rectangle2D == null)
                {
                    throw new System.NotImplementedException();
                }
                Rectangle2D resultRectangle2D = null;

                ISAMGeometry2D sAMGeometry2D = solver2DData.Geometry2D<ISAMGeometry2D>();
                // With a non-positive ShiftDistance the candidate offset (StartingDistance + i * ShiftDistance)
                // does not grow with i, so every iteration tests the same positions - one pass is enough and
                // repeating it is pure cost. Guards a degenerate caller from an IterationCount-fold blow-up.
                double iterationCount = solver2DSettings.ShiftDistance > 0 ? solver2DSettings.IterationCount : 1;

                // Degenerate layout already detected (see maxConsecutiveUnplaced): skip the full sweep and
                // make a single anchor attempt for the rest, so the whole solve stays bounded.
                if (consecutiveUnplaced >= maxConsecutiveUnplaced)
                {
                    iterationCount = 1;
                }

                // Hard safety cap on the whole solve. The consecutive-unplaced backstop only catches the case
                // where labels *fail* to place; a degenerate layout can also be slow while every label
                // *succeeds* - e.g. when all anchors collapse onto the same point, each label still places but
                // only after spiralling out past a growing pile of already-placed rectangles (O(N^2)). This
                // budget bounds the solve regardless of the mechanism: once exceeded, the remaining labels
                // skip the search and are placed AT their anchor (visible, possibly overlapping) rather than
                // dropped, because a consumer blanks an unplaced (null) label and tags would vanish. Such a
                // position was never tested, so it is reported as Fallback and never as Solved.
                //
                // Counted in geometric comparisons rather than elapsed time - see WorkBudget. A normal solve
                // of either real consumer never approaches it.
                bool overBudget = isOverBudget();

                if (sAMGeometry2D is Point2D)
                {
                    Point2D point2D = (Point2D)sAMGeometry2D;
                    Rectangle2D rectangle2DWithGivenPointInCenter = rectangle2D.GetMoved(new Vector2D(rectangle2D.GetCentroid(), point2D));

                    if (overBudget)
                    {
                        resultRectangle2D = rectangle2DWithGivenPointInCenter;
                    }
                    else
                    {
                        // Candidates are numbered in the order they are tested, ring by ring and direction by
                        // direction, so a count of them is a position in the sequence; see
                        // rejectedCandidateCounts for why an item may start part-way through it.
                        CandidateSequence candidateSequence = new CandidateSequence(rectangle2DWithGivenPointInCenter, solver2DSettings);
                        rejectedCandidateCounts.TryGetValue(candidateSequence, out int candidateIndex_Start);

                        int rejectedCandidateCount = candidateIndex_Start;
                        PlacedRectangle2D placedRectangle2D_Candidate = new PlacedRectangle2D(rectangle2DWithGivenPointInCenter);
                        System.Array.Clear(blockers, 0, blockers.Length);

                        int candidateIndex = 0;
                        for (int i = 0; i < iterationCount; i++)
                        {
                            if (resultRectangle2D != null) break;

                            for (int j = 0; j < offsets.Count; j++, candidateIndex++)
                            {
                                if (candidateIndex < candidateIndex_Start)
                                {
                                    continue;
                                }

                                Vector2D scaledOffset = offsets[j] * (solver2DSettings.StartingDistance + (i * solver2DSettings.ShiftDistance));

                                if (isKnownBlocked(placedRectangle2D_Candidate, scaledOffset, j, blockers))
                                {
                                    rejectedCandidateCount = candidateIndex + 1;
                                    continue;
                                }

                                Rectangle2D rectangleTemp = rectangle2DWithGivenPointInCenter.GetMoved(scaledOffset);

                                workUnits++;

                                PlacedRectangle2D blocker = null;
                                if (area.Inside(rectangleTemp) && !intersect(rectangleTemp, placedRectangle2Ds, grid, out blocker))
                                {
                                    rejectedCandidateCount = candidateIndex + 1;
                                    if (solver2DSettings.LimitArea != null && !solver2DSettings.LimitArea.Inside(rectangleTemp.GetCentroid()))
                                    {
                                        continue;
                                    }
                                    resultRectangle2D = rectangleTemp;
                                    break;
                                }

                                rejectedCandidateCount = candidateIndex + 1;
                                if (blocker != null)
                                {
                                    blockers[j] = blocker;
                                }

                                // Re-checked WITHIN this label's own sweep, not only before it started. The
                                // outer overBudget snapshot bounds every OTHER label; on its own it does nothing
                                // for the single expensive label that is spending the budget right now - a large
                                // IterationCount against a crowded obstacle set can burn millions of comparisons
                                // in one label's foreach before the outer loop gets another chance to look. Once
                                // spent mid-sweep, this label falls back to its OWN anchor - the untested
                                // rectangle2DWithGivenPointInCenter, exactly as a label that started already over
                                // budget uses, and exactly what Fallback promises a caller: at the anchor, never
                                // at whatever arbitrary spiralled-out candidate happened to be under test when
                                // the budget ran out.
                                if (!overBudget && isOverBudget())
                                {
                                    overBudget = true;
                                    resultRectangle2D = rectangle2DWithGivenPointInCenter;
                                    break;
                                }
                            }
                        }

                        // Only a sweep that ran its course records what it learned. One cut short by the budget
                        // falls back without having tested what it skipped, so it proves nothing about it.
                        if (!overBudget)
                        {
                            rejectedCandidateCounts[candidateSequence] = rejectedCandidateCount;
                        }
                    }
                }
                else if (sAMGeometry2D is Polyline2D)
                {
                    Polyline2D polyline2D = (Polyline2D)sAMGeometry2D;
                    List<Segment2D> segment2Ds = polyline2D.GetSegments();
                    Point2D point = polyline2D.Closest(rectangle2D.GetCentroid());
                    double distanceToCenter = point.Distance(rectangle2D.GetCentroid());

                    if (overBudget)
                    {
                        resultRectangle2D = rectangle2D;
                    }

                    for (int i = 0; !overBudget && i < iterationCount; i++)
                    {
                        if (resultRectangle2D != null) break;

                        for (int j = -1; j <= 1; j += 2)
                        {
                            double xNew = point.X + i * j * solver2DSettings.ShiftDistance;
                            double yNew = getY(polyline2D, xNew);
                            if (double.IsNaN(yNew))
                            {
                                continue;
                            }
                            Point2D newPoint = new Point2D(xNew, yNew);

                            List<Segment2D> segments = polyline2D.ClosestSegment2Ds(newPoint);
                            if (segments == null) continue;

                            Segment2D segment = segments[0];
                            bool clockwise = segment.Direction.GetPerpendicular().Y < 0;


                            Rectangle2D calculatedRectangle = Query.MoveToSegment2D(rectangle2D, segment, newPoint, distanceToCenter, clockwise);
                            Rectangle2D rectangleTemp = fix(Query.MoveToSegment2D(rectangle2D, segment, newPoint, distanceToCenter, clockwise), rectangle2D);

                            workUnits++;

                            if (area.Inside(rectangleTemp) && !intersect(rectangleTemp, placedRectangle2Ds, grid, out _))
                            {
                                if (solver2DSettings.LimitArea != null && !solver2DSettings.LimitArea.Inside(rectangleTemp.GetCentroid()))
                                {
                                    continue;
                                }
                                resultRectangle2D = rectangleTemp;
                                break;
                            }

                            // Same re-check as the Point2D branch above, and the same anchor-only Fallback
                            // contract: the untested rectangle2D at its original position, never the arbitrary
                            // segment-relative candidate under test when the budget ran out.
                            if (!overBudget && isOverBudget())
                            {
                                overBudget = true;
                                resultRectangle2D = rectangle2D;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    throw new System.NotImplementedException();
                }

                // Geometry that was tested against the area, the obstacles, the rectangles already placed and
                // the limit area is Solved; the untested anchor the budget forces is Fallback; nothing at all
                // is Unplaced. The three are not interchangeable to a consumer, which is the whole point of
                // reporting them - a Fallback rectangle may sit on top of anything.
                Solver2DResultType solver2DResultType = resultRectangle2D == null
                    ? Solver2DResultType.Unplaced
                    : (overBudget ? Solver2DResultType.Fallback : Solver2DResultType.Solved);

                result.Add(new Solver2DResult(solver2DData, resultRectangle2D, solver2DResultType));

                // Track consecutive failures for the degenerate-layout backstop above; any success resets it.
                if (resultRectangle2D == null)
                {
                    consecutiveUnplaced++;
                }
                else
                {
                    consecutiveUnplaced = 0;
                }

                // Mirror the placed rectangle into the spatial index (or the linear list) for subsequent
                // labels' overlap tests. Unplaced labels (null) carry no footprint, so they are not added.
                if (resultRectangle2D != null)
                {
                    PlacedRectangle2D placedRectangle2D = new PlacedRectangle2D(resultRectangle2D);

                    grid?.Add(placedRectangle2D);
                    placedRectangle2Ds?.Add(placedRectangle2D);
                }
            }

            return result;
        }


        private double getY(Polyline2D polyLine2D, double x)
        {
            List<Segment2D> polyLine2DSegments = polyLine2D.Segment2Ds();
            Segment2D resultSegment = null;

            foreach (Segment2D segment in polyLine2DSegments)
            {
                if (segment.Min.X <= x && x <= segment.Max.X)
                {
                    resultSegment = segment;
                    break;
                }
            }
            if (resultSegment == null) return double.NaN;

            List<Point2D> points = resultSegment.GetPoints();
            if (points == null || points.Count < 2) return double.NaN;

            Math.LinearEquation linearEquation = Math.Create.LinearEquation(points[0].X, points[0].Y, points[1].X, points[1].Y);
            if (linearEquation == null) return double.NaN;

            return linearEquation.Evaluate(x);
        }

        /// <summary>
        /// Generates unit vectors in 8 directions (angles: 0, 45, 90, 135...)
        /// </summary>
        /// <returns>List of offsets</returns>        
        private List<Vector2D> generateOffsets()
        {
            List<Vector2D> offsets = new List<Vector2D>();

            double offsetAngle = 90;
            for (double angle = 0; angle < 360; angle += offsetAngle)
            {
                double radians = System.Math.PI * angle / 180;
                double offsetX = System.Math.Sin(radians);
                double offsetY = System.Math.Cos(radians);

                offsets.Add(new Vector2D(offsetX, offsetY));
            }

            for (double angle = 45; angle < 360; angle += offsetAngle)
            {
                double radians = System.Math.PI * angle / 180; ;
                double offsetX = System.Math.Sin(radians);
                double offsetY = System.Math.Cos(radians);

                offsets.Add(new Vector2D(offsetX, offsetY));
            }

            return offsets;
        }
        private Rectangle2D fix(Rectangle2D calculatedRectangle, Rectangle2D defaultRectangle)
        {
            if (calculatedRectangle == null || defaultRectangle == null)
            {
                return calculatedRectangle;
            }
            if (System.Math.Abs(defaultRectangle.Width - calculatedRectangle.Width) < Core.Tolerance.MacroDistance)
            {
                return calculatedRectangle;
            }

            Rectangle2D result = new Rectangle2D(calculatedRectangle.Origin, -calculatedRectangle.Height, calculatedRectangle.Width, calculatedRectangle.WidthDirection);
            return result;
        }
        /// <summary>
        /// Placement order: <see cref="Solver2DData.Priority"/> ascending, then the order the items were
        /// added. Sorting a list of indices rather than the items makes the comparison total, so the result
        /// does not depend on <see cref="List{T}.Sort"/> being stable - it is not.
        /// </summary>
        private List<Solver2DData> ordered()
        {
            List<int> indexes = new List<int>(solver2DDatas.Count);
            for (int i = 0; i < solver2DDatas.Count; i++)
            {
                indexes.Add(i);
            }

            indexes.Sort((x, y) =>
            {
                int compare = solver2DDatas[x].Priority.CompareTo(solver2DDatas[y].Priority);

                return compare != 0 ? compare : x.CompareTo(y);
            });

            List<Solver2DData> result = new List<Solver2DData>(indexes.Count);
            foreach (int index in indexes)
            {
                result.Add(solver2DDatas[index]);
            }

            return result;
        }

        /// <summary>
        /// Whether the solve has spent its <see cref="WorkBudget"/>. A non-positive budget means unlimited.
        /// </summary>
        private bool isOverBudget()
        {
            return workBudget > 0 && workUnits > workBudget;
        }

        /// <summary>
        /// Whether the candidate - the item's rectangle centred on its anchor, moved by offset - is certainly
        /// rejected because it deeply overlaps a placed rectangle that already blocked this item in one of the
        /// 8 directions. When it is, the candidate is skipped without being built or tested, and without a
        /// work unit: its outcome is already known.
        /// <para>
        /// This is what takes the square out of a pile of labels (SAM_UI #58). The candidates walk outward
        /// along 8 rays in steps of ShiftDistance, which is usually much shorter than a label, so a ray that
        /// runs into a placed label keeps running into the SAME label for several rings before it clears it.
        /// Every one of those rings used to be a full test. Now the label that blocked the previous ring is
        /// checked first, by arithmetic on two boxes, and only a candidate that may have cleared it is tested.
        /// The other 7 directions' blockers are checked too - with StartingDistance 0, ring 0 is the same
        /// position in every direction, and the rays of a pile cross the same labels. Bounded at 8 box
        /// comparisons per candidate, so it cannot become hidden work of its own.
        /// </para>
        /// <para>
        /// Exact, never an approximation: two axis-aligned rectangles that overlap by more than a margin on
        /// both axes are InRange - either a corner of one lies inside the other, or they cross and their edges
        /// intersect - so intersect would have returned true for this candidate, and skipping it rejects
        /// exactly what testing it would have rejected. The margin (well above the InRange tolerance and any
        /// rounding in moving a rectangle) keeps the near-touching cases on the full test. Only axis-aligned
        /// rectangles qualify, because only for those is the bounding box the rectangle; all three consumers
        /// place axis-aligned labels, and anything else is simply tested as before.
        /// </para>
        /// </summary>
        private static bool isKnownBlocked(PlacedRectangle2D placedRectangle2D_Candidate, Vector2D offset, int direction, PlacedRectangle2D[] blockers)
        {
            if (!placedRectangle2D_Candidate.AxisAligned)
            {
                return false;
            }

            double minX = placedRectangle2D_Candidate.MinX + offset.X;
            double minY = placedRectangle2D_Candidate.MinY + offset.Y;
            double maxX = placedRectangle2D_Candidate.MaxX + offset.X;
            double maxY = placedRectangle2D_Candidate.MaxY + offset.Y;

            for (int i = 0; i < blockers.Length; i++)
            {
                // This direction's own blocker first: along a ray it is by far the most likely one.
                int index = (direction + i) % blockers.Length;

                PlacedRectangle2D blocker = blockers[index];
                if (blocker == null || !blocker.AxisAligned)
                {
                    continue;
                }

                if (System.Math.Min(maxX, blocker.MaxX) - System.Math.Max(minX, blocker.MinX) > knownBlockedMargin &&
                    System.Math.Min(maxY, blocker.MaxY) - System.Math.Max(minY, blocker.MinY) > knownBlockedMargin)
                {
                    blockers[direction] = blocker;
                    return true;
                }
            }

            return false;
        }

        private bool intersect(Rectangle2D rectangle2D, List<PlacedRectangle2D> placedRectangle2Ds, RectangleGrid grid, out PlacedRectangle2D blocker)
        {
            // The placed rectangle that rejected rectangle2D, for isKnownBlocked; null when nothing did, or
            // when an obstacle did - obstacles are not necessarily rectangles.
            blocker = null;

            // A null obstacle list is a legitimate "nothing to avoid" - Solver2D's own constructor accepts
            // one, and the caller that has no obstacles has no reason to allocate an empty list to say so.
            // It used to throw here.
            if (obstacles2D != null)
            {
                foreach (IClosed2D obstacle2D in obstacles2D)
                {
                    workUnits++;

                    if (obstacle2D.InRange(rectangle2D) == true)
                    {
                        return true;
                    }
                }
            }

            // Only the placed rectangles near rectangle2D can overlap it; the grid yields that set. Without a
            // grid every placed rectangle is tested. Either way the InRange test is the same, so the outcome
            // is identical. Items the solver could not place carry no footprint and are in neither.
            IEnumerable<PlacedRectangle2D> placedRectangle2Ds_Near = grid != null ? grid.Query(rectangle2D) : placedRectangle2Ds;

            foreach (PlacedRectangle2D placedRectangle2D in placedRectangle2Ds_Near)
            {
                workUnits++;

                Rectangle2D placed = placedRectangle2D.Rectangle2D;
                if (placed.InRange(rectangle2D) == true || rectangle2D.InRange(placed) == true)
                {
                    blocker = placedRectangle2D;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// A placed rectangle together with its bounding box and whether it is axis-aligned, worked out once
        /// when it is placed rather than on every comparison. Compared by reference.
        /// </summary>
        private sealed class PlacedRectangle2D
        {
            public PlacedRectangle2D(Rectangle2D rectangle2D)
            {
                Rectangle2D = rectangle2D;

                BoundingBox2D boundingBox2D = rectangle2D.GetBoundingBox();
                MinX = boundingBox2D.Min.X;
                MinY = boundingBox2D.Min.Y;
                MaxX = boundingBox2D.Max.X;
                MaxY = boundingBox2D.Max.Y;

                // Exactly axis-aligned only; a rectangle turned by any angle at all is left to the full test.
                Vector2D heightDirection = rectangle2D.HeightDirection;
                AxisAligned = heightDirection != null && (heightDirection.X == 0 || heightDirection.Y == 0);
            }

            public Rectangle2D Rectangle2D { get; }

            public double MinX { get; }

            public double MinY { get; }

            public double MaxX { get; }

            public double MaxY { get; }

            public bool AxisAligned { get; }
        }

        /// <summary>
        /// Everything that decides the sequence of candidate rectangles a Point2D item tests: its rectangle
        /// centred on its anchor, the search distances and the limit area, which is compared by reference.
        /// The area, the obstacles and the 8 directions are the same for every item of a solve. Two items with
        /// equal values test identical candidates in identical order - see rejectedCandidateCounts in Solve.
        /// IterationCount is deliberately left out: it only decides how far along the sequence an item goes,
        /// not what the sequence is.
        /// </summary>
        private struct CandidateSequence : System.IEquatable<CandidateSequence>
        {
            private readonly double originX;
            private readonly double originY;
            private readonly double width;
            private readonly double height;
            private readonly double heightDirectionX;
            private readonly double heightDirectionY;
            private readonly double startingDistance;
            private readonly double shiftDistance;
            private readonly IClosed2D limitArea;

            public CandidateSequence(Rectangle2D rectangle2D, Solver2DSettings solver2DSettings)
            {
                Point2D origin = rectangle2D.Origin;
                Vector2D heightDirection = rectangle2D.HeightDirection;

                originX = origin.X;
                originY = origin.Y;
                width = rectangle2D.Width;
                height = rectangle2D.Height;
                heightDirectionX = heightDirection == null ? double.NaN : heightDirection.X;
                heightDirectionY = heightDirection == null ? double.NaN : heightDirection.Y;
                startingDistance = solver2DSettings.StartingDistance;
                shiftDistance = solver2DSettings.ShiftDistance;
                limitArea = solver2DSettings.LimitArea;
            }

            public bool Equals(CandidateSequence other)
            {
                return originX.Equals(other.originX) &&
                    originY.Equals(other.originY) &&
                    width.Equals(other.width) &&
                    height.Equals(other.height) &&
                    heightDirectionX.Equals(other.heightDirectionX) &&
                    heightDirectionY.Equals(other.heightDirectionY) &&
                    startingDistance.Equals(other.startingDistance) &&
                    shiftDistance.Equals(other.shiftDistance) &&
                    ReferenceEquals(limitArea, other.limitArea);
            }

            public override bool Equals(object obj)
            {
                return obj is CandidateSequence candidateSequence && Equals(candidateSequence);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int result = originX.GetHashCode();
                    result = (result * 397) ^ originY.GetHashCode();
                    result = (result * 397) ^ width.GetHashCode();
                    result = (result * 397) ^ height.GetHashCode();
                    result = (result * 397) ^ heightDirectionX.GetHashCode();
                    result = (result * 397) ^ heightDirectionY.GetHashCode();
                    result = (result * 397) ^ startingDistance.GetHashCode();
                    result = (result * 397) ^ shiftDistance.GetHashCode();
                    result = (result * 397) ^ (limitArea == null ? 0 : System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(limitArea));
                    return result;
                }
            }
        }

        // Uniform-grid spatial index over placed label rectangles, keyed by the cells their bounding boxes
        // cover. A placed rectangle is inserted into every cell its box, expanded by the InRange tolerance,
        // overlaps; a query returns every rectangle in the cells the query box, expanded by the much larger
        // MacroDistance, overlaps. Two rectangles can only be InRange if their boxes come within the InRange
        // tolerance of each other, so the two expanded boxes of such a pair overlap and therefore share at
        // least one cell - the index never drops a real overlap, only skips the far-apart ones the linear
        // scan would have tested and rejected. Cell size only affects speed, not correctness.
        //
        // There used to be a one-cell halo around every query as well. The expanded boxes already make it
        // redundant, and with cells as large as the largest label it multiplied what every query returned:
        // in a pile of labels it was ~45 rectangles per query, each one a full InRange test (SAM_UI #58).
        private sealed class RectangleGrid
        {
            private const long maxCellsPerRectangle = 1024;

            private readonly double cellSize;
            private readonly Dictionary<long, List<PlacedRectangle2D>> cells = new Dictionary<long, List<PlacedRectangle2D>>();

            // Every placed rectangle, and the outliers too large to spread over cells - see Add and Query.
            private readonly List<PlacedRectangle2D> all = new List<PlacedRectangle2D>();
            private readonly List<PlacedRectangle2D> large = new List<PlacedRectangle2D>();

            // Reused across Query calls to de-duplicate the rectangles a query box's cells share, without
            // allocating a HashSet on every call. Query is enumerated fully and sequentially by the solver
            // (one query finishes before the next starts), so a single shared scratch set is safe here.
            private readonly HashSet<PlacedRectangle2D> querySeen = new HashSet<PlacedRectangle2D>();

            private RectangleGrid(double cellSize)
            {
                this.cellSize = cellSize;
            }

            /// <summary>
            /// Sized by the labels' SHORT side: the median short side, but no smaller than a sixteenth of the
            /// median long side, so a typical label covers a few cells and never thousands. It used to be the
            /// LONGEST side of the largest label. Labels are long and thin - a 6 m name at 0.4 m text height -
            /// so a cell that size held a stack of fifteen labels, all of which every nearby query returned.
            /// </summary>
            public static RectangleGrid Create(List<Solver2DData> solver2DDatas)
            {
                List<double> shortSides = new List<double>();
                List<double> longSides = new List<double>();
                foreach (Solver2DData solver2DData in solver2DDatas)
                {
                    Rectangle2D rectangle2D = solver2DData?.Closed2D<Rectangle2D>();
                    BoundingBox2D boundingBox2D = rectangle2D?.GetBoundingBox();
                    if (boundingBox2D == null)
                    {
                        continue;
                    }

                    shortSides.Add(System.Math.Min(boundingBox2D.Width, boundingBox2D.Height));
                    longSides.Add(System.Math.Max(boundingBox2D.Width, boundingBox2D.Height));
                }

                if (shortSides.Count == 0)
                {
                    return null;
                }

                shortSides.Sort();
                longSides.Sort();

                double cellSize = System.Math.Max(shortSides[shortSides.Count / 2], longSides[longSides.Count / 2] / 16);

                // No usable footprint to size the grid by - let the caller fall back to the linear scan.
                return cellSize > Core.Tolerance.Distance ? new RectangleGrid(cellSize) : null;
            }

            public void Add(PlacedRectangle2D placedRectangle2D)
            {
                if (!range(placedRectangle2D.Rectangle2D, Core.Tolerance.Distance, out long minX, out long minY, out long maxX, out long maxY))
                {
                    return;
                }

                all.Add(placedRectangle2D);

                // A rectangle far larger than the cells - an outlier the median did not size for - is not
                // spread over thousands of cells; it is kept aside and returned by every query instead.
                if (isLarge(minX, minY, maxX, maxY))
                {
                    large.Add(placedRectangle2D);
                    return;
                }

                for (long x = minX; x <= maxX; x++)
                {
                    for (long y = minY; y <= maxY; y++)
                    {
                        long key = (x << 32) ^ (y & 0xffffffffL);
                        if (!cells.TryGetValue(key, out List<PlacedRectangle2D> list))
                        {
                            list = new List<PlacedRectangle2D>();
                            cells[key] = list;
                        }

                        list.Add(placedRectangle2D);
                    }
                }
            }

            public IEnumerable<PlacedRectangle2D> Query(Rectangle2D rectangle2D)
            {
                if (!range(rectangle2D, Core.Tolerance.MacroDistance, out long minX, out long minY, out long maxX, out long maxY))
                {
                    yield break;
                }

                // A query box far larger than the cells would visit thousands of them; every placed
                // rectangle is a smaller and still complete answer.
                if (isLarge(minX, minY, maxX, maxY))
                {
                    foreach (PlacedRectangle2D placed in all)
                    {
                        yield return placed;
                    }

                    yield break;
                }

                querySeen.Clear();

                foreach (PlacedRectangle2D placed in large)
                {
                    querySeen.Add(placed);
                    yield return placed;
                }

                for (long x = minX; x <= maxX; x++)
                {
                    for (long y = minY; y <= maxY; y++)
                    {
                        if (cells.TryGetValue((x << 32) ^ (y & 0xffffffffL), out List<PlacedRectangle2D> list))
                        {
                            foreach (PlacedRectangle2D placed in list)
                            {
                                if (querySeen.Add(placed))
                                {
                                    yield return placed;
                                }
                            }
                        }
                    }
                }
            }

            private static bool isLarge(long minX, long minY, long maxX, long maxY)
            {
                // In double, so an absurd extent cannot overflow into a small count.
                return ((double)maxX - minX + 1) * ((double)maxY - minY + 1) > maxCellsPerRectangle;
            }

            private bool range(Rectangle2D rectangle2D, double offset, out long minX, out long minY, out long maxX, out long maxY)
            {
                minX = minY = maxX = maxY = 0;

                BoundingBox2D boundingBox2D = rectangle2D?.GetBoundingBox(offset);
                if (boundingBox2D == null)
                {
                    return false;
                }

                Point2D min = boundingBox2D.Min;
                Point2D max = boundingBox2D.Max;
                minX = (long)System.Math.Floor(min.X / cellSize);
                minY = (long)System.Math.Floor(min.Y / cellSize);
                maxX = (long)System.Math.Floor(max.X / cellSize);
                maxY = (long)System.Math.Floor(max.Y / cellSize);
                return true;
            }
        }

    }
}
