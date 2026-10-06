// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        /// <summary>
        /// Split Segment2Ds
        /// </summary>
        /// <returns>List Segment2D</returns>
        /// <param name="segment2Ds">Segments2Ds</param>
        /// <param name="tolerance">tolerance</param>
        public static List<Segment2D> Split(this IEnumerable<Segment2D> segment2Ds, double tolerance = Core.Tolerance.Distance)
        {
            if (segment2Ds == null)
                return null;

            List<Tuple<BoundingBox2D, Segment2D>> tuples = new List<Tuple<BoundingBox2D, Segment2D>>();
            // Deduplicated intersection endpoints. Modify.Add over this list used to rescan
            // every accepted point for each new endpoint and each intersection found by the
            // pair loop below, and the Find at the intersection lookup did the same again -
            // all three are proximity queries on the same set, so they share one grid. The
            // exact Modify.Add / AlmostEquals rules are re-evaluated per candidate, and the
            // grid returns points in insertion order, so identical duplicates resolve to the
            // same stored instance as before.
            Point2DGrid point2Ds = new Point2DGrid(tolerance);
            foreach (Segment2D segment2D in segment2Ds)
            {
                if (segment2D == null || segment2D.GetLength() < tolerance)
                {
                    continue;
                }

                tuples.Add(new Tuple<BoundingBox2D, Segment2D>(segment2D.GetBoundingBox(), segment2D));
                AddIfAbsent(point2Ds, segment2D[0], tolerance);
                AddIfAbsent(point2Ds, segment2D[1], tolerance);
            }

            int count = tuples.Count();

            // Broad phase for the pair sweep below. The sweep only ever accepts a pair whose
            // boxes satisfy boundingBox2D_1.InRange(boundingBox2D_2, tolerance), and the grid
            // returns a superset of the boxes that can satisfy it: each box is registered in
            // the cells it covers once inflated by the tolerance, and a query walks the cells
            // its own box covers, so two boxes within tolerance of each other always share a
            // cell. Boxes that cannot be quantised are held aside by the grid and offered to
            // every query. InRange is still evaluated on every candidate, and the exact
            // predicates (On, Intersection, AlmostSimilar) still decide every pair, so the
            // accepted set is unchanged - only pairs InRange would have rejected disappear.
            //
            // The sweep additionally requires, for each i, that every candidate j is processed
            // exactly once and in ascending original j order; SweepCandidates is where that is
            // established and why.
            //
            // Below 32 segments the index costs more than the pairs it removes, so the sweep
            // runs over the whole tail, which is the pre-existing code path exactly.
            BoundingBox2DGrid grid_Sweep = null;
            int[] stamps = null;
            if (count >= 32)
            {
                grid_Sweep = new BoundingBox2DGrid(tolerance, BoundingBox2DGrid.CellSizeHint(tuples.ConvertAll(x => x.Item1)));
                foreach (Tuple<BoundingBox2D, Segment2D> tuple in tuples)
                {
                    grid_Sweep.Add(tuple.Item1);
                }

                stamps = new int[count];
                for (int i = 0; i < count; i++)
                {
                    stamps[i] = -1;
                }
            }

            List<int> indexes = new List<int>();

            List<List<Point2D>> point2DsList = Enumerable.Repeat<List<Point2D>>(null, count).ToList();
            for (int i = 0; i < count - 1; i++)
            {
                BoundingBox2D boundingBox2D_1 = tuples[i].Item1;
                Segment2D segment2D_1 = tuples[i].Item2;

                SweepCandidates(grid_Sweep, boundingBox2D_1, i, count, stamps, indexes);

                foreach (int j in indexes)
                {
                    BoundingBox2D boundingBox2D_2 = tuples[j].Item1;
                    if (!boundingBox2D_1.InRange(boundingBox2D_2, tolerance))
                    {
                        continue;
                    }

                    Segment2D segment2D_2 = tuples[j].Item2;
                    if (segment2D_1.AlmostSimilar(segment2D_2, tolerance))
                    {
                        continue;
                    }

                    Point2D point2D_Closest1;
                    Point2D point2D_Closest2;

                    List<Point2D> point2Ds_Intersection = new List<Point2D>();

                    if (segment2D_1.On(segment2D_2[0], tolerance))
                        point2Ds_Intersection.Add(segment2D_2[0]);

                    if (segment2D_2.On(segment2D_1[0], tolerance))
                        point2Ds_Intersection.Add(segment2D_1[0]);

                    if (segment2D_1.On(segment2D_2[1], tolerance))
                        point2Ds_Intersection.Add(segment2D_2[1]);

                    if (segment2D_2.On(segment2D_1[1], tolerance))
                        point2Ds_Intersection.Add(segment2D_1[1]);

                    if (point2Ds_Intersection.Count == 0)
                    {
                        Point2D point2D_Intersection = segment2D_1.Intersection(segment2D_2, out point2D_Closest1, out point2D_Closest2, tolerance);
                        if (point2D_Intersection == null || point2D_Intersection.IsNaN())
                            continue;

                        if (point2D_Closest1 != null && point2D_Closest2 != null)
                            if (point2D_Closest1.Distance(point2D_Closest2) > tolerance)
                                continue;

                        point2Ds_Intersection.Add(point2D_Intersection);
                    }

                    if (point2Ds_Intersection == null || point2Ds_Intersection.Count == 0)
                    {
                        continue;
                    }

                    foreach (Point2D point2D_Intersection in point2Ds_Intersection)
                    {
                        Point2D point2D_Intersection_Temp = FindAlmostEqual(point2Ds, point2D_Intersection, tolerance);
                        if (point2D_Intersection_Temp == null)
                        {
                            point2D_Intersection_Temp = point2D_Intersection;
                            AddIfAbsent(point2Ds, point2D_Intersection_Temp, tolerance);
                        }

                        if (point2D_Intersection_Temp.Distance(segment2D_1.Start) > tolerance && point2D_Intersection_Temp.Distance(segment2D_1.End) > tolerance)
                        {
                            if (point2DsList[i] == null)
                            {
                                point2DsList[i] = new List<Point2D>();
                            }

                            Modify.Add(point2DsList[i], point2D_Intersection_Temp, tolerance);
                        }

                        if (point2D_Intersection_Temp.Distance(segment2D_2.Start) > tolerance && point2D_Intersection_Temp.Distance(segment2D_2.End) > tolerance)
                        {
                            if (point2DsList[j] == null)
                            {
                                point2DsList[j] = new List<Point2D>();
                            }

                            Modify.Add(point2DsList[j], point2D_Intersection_Temp, tolerance);
                        }
                    }
                }
            }

            List<Segment2D> result = new List<Segment2D>();
            // Both result scans below - the AlmostSimilar dedup and the endpoint-pair dedup -
            // can only match a kept segment whose bounding box agrees with the candidate's box
            // to within tolerance, so the kept set is indexed on that box and only those
            // candidates reach the exact predicates.
            BoundingBox2DGrid grid_Result = new BoundingBox2DGrid(tolerance, BoundingBox2DGrid.CellSizeHint(tuples.ConvertAll(x => x.Item1)));

            // AlmostSimilar answers true for the same instance before it looks at the tolerance,
            // so an input segment repeated by reference is similar to itself whatever the
            // tolerance is - including a negative one, where BoundsInRange rejects every pair.
            // The spatial index cannot carry that, so identity is tracked beside it.
            HashSet<object> instances = new HashSet<object>(ReferenceComparer.Instance);

            for (int i = 0; i < count; i++)
            {
                Segment2D segment2D_Temp = tuples[i].Item2;

                if (instances.Contains(segment2D_Temp))
                {
                    continue;
                }

                bool similar = false;
                BoundingBox2D boundingBox2D_Temp = segment2D_Temp.GetBoundingBox();
                foreach (int index in grid_Result.Candidates(boundingBox2D_Temp))
                {
                    // Cheap necessary condition first: bounds must agree within tolerance.
                    if (!BoundsInRange(boundingBox2D_Temp, grid_Result[index], tolerance))
                    {
                        continue;
                    }

                    if (result[index].AlmostSimilar(segment2D_Temp, tolerance))
                    {
                        similar = true;
                        break;
                    }
                }

                if (similar)
                {
                    continue;
                }

                List<Point2D> point2Ds_Temp = point2DsList[i];
                if (point2Ds_Temp == null || point2Ds_Temp.Count == 0)
                {
                    grid_Result.Add(boundingBox2D_Temp);
                    instances.Add(segment2D_Temp);
                    result.Add(segment2D_Temp);
                    continue;
                }

                Modify.Add(point2Ds_Temp, segment2D_Temp[0], tolerance);
                Modify.Add(point2Ds_Temp, segment2D_Temp[1], tolerance);

                Modify.SortByDistance(point2Ds_Temp, segment2D_Temp[0]);

                for (int j = 0; j < point2Ds_Temp.Count - 1; j++)
                {
                    Point2D point2D_1 = point2Ds_Temp[j];
                    Point2D point2D_2 = point2Ds_Temp[j + 1];

                    Segment2D segment2D = null;
                    BoundingBox2D boundingBox2D_Piece = null;
                    foreach (int index in grid_Result.Candidates(boundingBox2D_Piece = new BoundingBox2D(point2D_1, point2D_2)))
                    {
                        // Cheap necessary condition first: bounds must agree within tolerance.
                        if (!BoundsInRange(boundingBox2D_Piece, grid_Result[index], tolerance))
                        {
                            continue;
                        }

                        Segment2D segment2D_Temp_2 = result[index];
                        if ((segment2D_Temp_2[0].AlmostEquals(point2D_1, tolerance) && segment2D_Temp_2[1].AlmostEquals(point2D_2, tolerance)) || (segment2D_Temp_2[1].AlmostEquals(point2D_1, tolerance) && segment2D_Temp_2[0].AlmostEquals(point2D_2, tolerance)))
                        {
                            segment2D = segment2D_Temp_2;
                            break;
                        }
                    }

                    if (segment2D != null)
                        continue;

                    Segment2D segment2D_Piece = new Segment2D(point2D_1, point2D_2);
                    result.Add(segment2D_Piece);
                    grid_Result.Add(boundingBox2D_Piece);
                    instances.Add(segment2D_Piece);
                }
            }

            return result;
        }

        /// <summary>
        /// Fills <paramref name="indexes"/> with the tuple indices after <paramref name="index"/>
        /// whose bounding box could be InRange of <paramref name="boundingBox2D"/>. With no grid
        /// it is the whole tail - the original inner loop.
        /// </summary>
        /// <remarks>
        /// The invariant this method exists to hold, and the one the whole broad phase rests on:
        /// <para>
        /// <em>for each i, every candidate j appears exactly once, and in ascending original j
        /// order.</em>
        /// </para>
        /// Both halves are load bearing, and neither is free.
        /// <para>
        /// Exactly once: a box is registered in every cell it covers, so the raw candidate stream
        /// repeats a box once per cell it shares with the query. Visiting one pair twice would run
        /// Modify.Add twice over point2DsList, and at a negative tolerance Modify.Add accepts a
        /// point it has already stored - so the repeats have to be collapsed, not merely tolerated.
        /// The stamp array does it in one pass without allocating.
        /// </para>
        /// <para>
        /// Ascending: point2DsList[i] accumulates intersections in the order the pairs are
        /// visited, and Modify.SortByDistance later orders the pieces by distance rather than by
        /// arrival - but which Point2D instance ends up stored, and so which instances the output
        /// pieces share, still depends on arrival order. BoundingBox2DGrid.Candidates sorts before
        /// returning, so the stream is already ascending and the filtering below preserves it.
        /// </para>
        /// </remarks>
        private static void SweepCandidates(BoundingBox2DGrid boundingBox2DGrid, BoundingBox2D boundingBox2D, int index, int count, int[] stamps, List<int> indexes)
        {
            indexes.Clear();

            if (boundingBox2DGrid == null)
            {
                for (int i = index + 1; i < count; i++)
                {
                    indexes.Add(i);
                }

                return;
            }

            foreach (int index_Candidate in boundingBox2DGrid.Candidates(boundingBox2D))
            {
                if (index_Candidate <= index || stamps[index_Candidate] == index)
                {
                    continue;
                }

                stamps[index_Candidate] = index;
                indexes.Add(index_Candidate);
            }
        }

        private static bool BoundsInRange(BoundingBox2D boundingBox2D_1, BoundingBox2D boundingBox2D_2, double tolerance)
        {
            // Necessary condition for both dedup predicates above: a match requires every
            // bounding-box bound to agree within tolerance. It only skips pairs the exact
            // predicates would always reject. NaN tolerance passes everything through, so the
            // exact predicates keep their historical NaN behaviour.
            if (boundingBox2D_1 == null || boundingBox2D_2 == null || double.IsNaN(tolerance))
            {
                return true;
            }

            return System.Math.Abs(boundingBox2D_1.Min.X - boundingBox2D_2.Min.X) <= tolerance
                && System.Math.Abs(boundingBox2D_1.Min.Y - boundingBox2D_2.Min.Y) <= tolerance
                && System.Math.Abs(boundingBox2D_1.Max.X - boundingBox2D_2.Max.X) <= tolerance
                && System.Math.Abs(boundingBox2D_1.Max.Y - boundingBox2D_2.Max.Y) <= tolerance;
        }

        private static bool AddIfAbsent(Point2DGrid point2DGrid, Point2D point2D, double tolerance)
        {
            // Same acceptance rule as Modify.Add: an existing point within Euclidean distance
            // rejects the new one (the axis checks there are redundant prefilters). Candidates
            // arrive in insertion order, so the decision matches the list scan exactly.
            if (point2D == null)
            {
                return false;
            }

            foreach (int index in point2DGrid.Candidates(point2D))
            {
                Point2D point2D_Temp = point2DGrid[index];
                if (point2D_Temp == null)
                {
                    continue;
                }

                if (point2D_Temp.Distance(point2D) <= tolerance)
                {
                    return false;
                }
            }

            point2DGrid.Add(point2D);
            return true;
        }

        private static Point2D FindAlmostEqual(Point2DGrid point2DGrid, Point2D point2D, double tolerance)
        {
            // First point in insertion order passing the strict axis AlmostEquals test - the
            // same match List.Find(AlmostEquals) returned.
            foreach (int index in point2DGrid.Candidates(point2D))
            {
                Point2D point2D_Temp = point2DGrid[index];
                if (point2D_Temp != null && point2D.AlmostEquals(point2D_Temp, tolerance))
                {
                    return point2D_Temp;
                }
            }

            return null;
        }

        public static List<Segment2D> Split(this IEnumerable<ISegmentable2D> segmentable2Ds, double tolerance = Core.Tolerance.Distance)
        {
            if (segmentable2Ds == null)
                return null;

            List<Segment2D> segment2Ds = new List<Segment2D>();
            foreach (ISegmentable2D segmentable2D in segmentable2Ds)
            {
                if (segmentable2D == null)
                    continue;

                List<Segment2D> segment2Ds_Temp = segmentable2D.GetSegments();
                if (segment2Ds_Temp == null)
                    continue;

                segment2Ds.AddRange(segment2Ds_Temp);
            }

            return Split(segment2Ds, tolerance);
        }

        public static List<Segment2D> Split(this ISegmentable2D segmentable2D, IEnumerable<Segment2D> segment2Ds, double tolerance = Core.Tolerance.Distance)
        {
            if (segmentable2D == null || segment2Ds == null)
                return null;

            if (segment2Ds.Count() == 0)
                return new List<Segment2D>();

            List<Segment2D> result = new List<Segment2D>();
            foreach (Segment2D segment2D in segment2Ds)
            {
                List<Point2D> point2Ds = segmentable2D.Intersections(segment2D, tolerance);
                if (point2Ds == null || point2Ds.Count == 0)
                {
                    result.Add(new Segment2D(segment2D));
                    continue;
                }

                if (point2Ds.Find(x => x.AlmostEquals(segment2D[0])) == null)
                    point2Ds.Add(segment2D[0]);

                if (point2Ds.Find(x => x.AlmostEquals(segment2D[1])) == null)
                    point2Ds.Add(segment2D[1]);

                Modify.SortByDistance(point2Ds, segment2D[0]);

                List<Segment2D> segment2Ds_Temp = Create.Segment2Ds(point2Ds, false);
                if (segment2Ds_Temp != null && segment2Ds_Temp.Count != 0)
                    result.AddRange(segment2Ds_Temp);
            }

            return result;
        }

        /// <summary>
        /// Method splits face2Ds by intersection. Does not fully work if any of face2Ds are similar or overlaping
        /// </summary>
        /// <param name="face2Ds">Face2Ds</param>
        /// <param name="tolerance">Tolerance</param>
        /// <returns>Face2Ds</returns>
        public static List<Face2D> Split(this IEnumerable<Face2D> face2Ds, double tolerance = Core.Tolerance.Distance)
        {
            if (face2Ds == null)
                return null;

            if (face2Ds.Count() <= 1)
                return new List<Face2D>(face2Ds);

            List<Face2D> face2Ds_Temp = new List<Face2D>(face2Ds);
            face2Ds_Temp.RemoveSimilar(tolerance);

            MultiPolygon multiPolygon = face2Ds_Temp.ToNTS(tolerance);
            if (multiPolygon == null || multiPolygon.IsEmpty)
                return null;

            List<Polygon> polygons = multiPolygon.ToNTS_Polygons(tolerance);
            if (polygons == null)
                return null;

            return polygons.ConvertAll(x => x.ToSAM(tolerance));
        }

        public static List<Face2D> Split(this Face2D face2D, IEnumerable<ISegmentable2D> segmentable2Ds, double tolerance_Snap = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (face2D == null || segmentable2Ds == null)
            {
                return null;
            }

            BoundingBox2D boundingBox2D = face2D.GetBoundingBox();
            if (boundingBox2D == null)
            {
                return null;
            }

            List<ISegmentable2D> segmentable2Ds_All = new List<ISegmentable2D>();
            foreach (ISegmentable2D segmentable2D in segmentable2Ds)
            {
                BoundingBox2D boundingBox2D_Segmentable2D = segmentable2D?.GetBoundingBox();
                if (boundingBox2D_Segmentable2D == null)
                {
                    continue;
                }

                if (!boundingBox2D.InRange(boundingBox2D_Segmentable2D, tolerance))
                {
                    continue;
                }

                segmentable2Ds_All.Add(segmentable2D);
            }

            if (segmentable2Ds_All == null || segmentable2Ds_All.Count == 0)
            {
                return null;
            }

            IClosed2D externalEdge = face2D.ExternalEdge2D;

            ISegmentable2D segmentable2D_ExternalEdge = externalEdge as ISegmentable2D;
            if (segmentable2D_ExternalEdge == null)
            {
                return null;
            }

            segmentable2Ds_All.Add(segmentable2D_ExternalEdge);

            List<IClosed2D> internalEdges = face2D.InternalEdge2Ds;
            if (internalEdges != null && internalEdges.Count != 0)
            {
                foreach (IClosed2D internalEdge in internalEdges)
                {
                    ISegmentable2D segmentable2D_InternalEdge = internalEdge as ISegmentable2D;
                    if (segmentable2D_InternalEdge == null)
                    {
                        continue;
                    }

                    segmentable2Ds_All.Add(segmentable2D_InternalEdge);
                }
            }

            List<Segment2D> segment2Ds = segmentable2Ds_All.Split(tolerance);
            segment2Ds = segment2Ds.Snap(true, tolerance_Snap);

            List<Polygon2D> polygon2Ds = Create.Polygon2Ds(segment2Ds, tolerance);
            if (polygon2Ds == null || polygon2Ds.Count == 0)
            {
                return null;
            }

            List<Tuple<Polygon2D, Point2D>> tuples = polygon2Ds.ConvertAll(x => new Tuple<Polygon2D, Point2D>(x, x.InternalPoint2D()));

            tuples = tuples.FindAll(x => externalEdge.Inside(x.Item2, tolerance));

            List<Face2D> result = new List<Face2D>();
            tuples.Sort((x, y) => y.Item1.GetArea().CompareTo(x.Item1.GetArea()));
            while (tuples.Count > 0)
            {
                Polygon2D polygon2D_External = tuples[0].Item1;
                tuples.RemoveAt(0);

                List<Polygon2D> polygon2Ds_Internal = tuples.FindAll(x => polygon2D_External.Inside(x.Item2, tolerance)).ConvertAll(x => x.Item1);
                if (polygon2Ds_Internal.Count != 0)
                {
                    tuples.RemoveAll(x => polygon2Ds_Internal.Contains(x.Item1));
                }

                Face2D face2D_Split = Create.Face2D(polygon2D_External, polygon2Ds_Internal);
                if (face2D_Split != null)
                {
                    Point2D point2D_Internal = face2D_Split?.GetInternalPoint2D(tolerance);
                    if (face2D.Inside(point2D_Internal, tolerance))
                    {
                        result.Add(face2D_Split);
                    }
                }
            }

            return result;

            //List<Tuple<Polygon2D, Point2D>> tuples_Internal = null;
            //if (internalEdges != null && internalEdges.Count != 0)
            //{
            //    tuples_Internal = tuples.FindAll(x => internalEdges.Find(y => y.Inside(x.Item2, tolerance)) != null);
            //}

            //tuples_Internal?.ForEach(x => tuples.Remove(x));

            //if(tuples.Count == 0)
            //{
            //    return null;
            //}

            //List<Face2D> result = new List<Face2D>();
            //foreach(Tuple<Polygon2D, Point2D> tuple in tuples)
            //{
            //    Face2D face2D_Split = Create.Face2D(tuple.Item1, tuples_Internal?.ConvertAll(x => x.Item1));
            //    if(face2D_Split != null)
            //    {
            //        result.Add(face2D_Split);
            //    }
            //}
            //return result;
        }

        public static List<Polyline2D> Split(this Polyline2D polyline2D, Point2D point2D, double tolerance = Core.Tolerance.Distance)
        {
            if (polyline2D == null || point2D == null)
                return null;

            Point2D point2D_Temp = polyline2D.InsertClosest(point2D, tolerance);
            if (point2D_Temp == null)
                return null;

            int index = polyline2D.IndexOfClosestPoint2D(point2D_Temp);

            List<Point2D> point2Ds = polyline2D.Points;

            if (index == 0 || index == point2Ds.Count - 1)
                return new List<Polyline2D>() { new Polyline2D(point2Ds) };

            List<Polyline2D> result = new List<Polyline2D>();
            result.Add(new Polyline2D(point2Ds.GetRange(0, index + 1)));
            result.Add(new Polyline2D(point2Ds.GetRange(index, point2Ds.Count - index)));

            return result;
        }

        /// <summary>
        /// Generates additional points between two points where point2D_1 is first point on list and point2D_2 is last point.
        /// </summary>
        /// <param name="point2D_1">First point</param>
        /// <param name="point2D_2">Last point</param>
        /// <param name="count">Number of additional points</param>
        /// <returns></returns>
        public static IEnumerable<Point2D> Split(this Point2D point2D_1, Point2D point2D_2, int count)
        {
            if (point2D_1 == null || point2D_2 == null)
                return null;

            if (count <= 0)
                return null;

            if (count == 1)
                return new Point2D[] { point2D_1, point2D_2 };

            Vector2D vector2D = new Vector2D(point2D_1, point2D_2);
            double length_Split = vector2D.Length / count;
            vector2D = vector2D.Unit * length_Split;

            Point2D[] result = new Point2D[count + 1];

            result[0] = new Point2D(point2D_1);
            for (int i = 0; i < count; i++)
                result[i + 1] = result[i].GetMoved(vector2D);

            result[count] = new Point2D(point2D_2);

            return result;
        }

        /// <summary>
        /// Split one polygon2D into the count number of polygon2Ds. Split based on smallest Rectangle2D described on given polygon2D and spliting longest edge.
        /// </summary>
        /// <param name="polygon2D">Polygon2D to be splitted</param>
        /// <param name="count">Number of output Polygon2Ds</param>
        /// <param name="alignment">Split Alignment</param>
        /// <param name="tolerance">Tolerance</param>
        /// <returns>Polygon2Ds</returns>
        public static List<Polygon2D> Split(this Polygon2D polygon2D, int count, Alignment alignment, double tolerance = Core.Tolerance.Distance)
        {
            Rectangle2D rectangle2D = Create.Rectangle2D(polygon2D?.Points);
            if (rectangle2D == null)
                return null;

            Vector2D direction = Direction(alignment);
            if (direction == null)
                return null;

            double angle_Height = System.Math.Min(rectangle2D.HeightDirection.Angle(direction), rectangle2D.HeightDirection.Angle(direction.GetNegated()));
            double angle_Width = System.Math.Min(rectangle2D.WidthDirection.Angle(direction), rectangle2D.WidthDirection.Angle(direction.GetNegated()));

            Segment2D segment2D = null;
            Vector2D vector2D = null;

            if (angle_Height < angle_Width)
            {
                vector2D = rectangle2D.WidthDirection;
                Vector2D vector_Width = rectangle2D.WidthDirection * (rectangle2D.Width / 2);
                Point2D point2D_Start = rectangle2D.Origin.GetMoved(vector_Width);
                Point2D point2D_End = point2D_Start.GetMoved(rectangle2D.HeightDirection * rectangle2D.Height);

                segment2D = new Segment2D(point2D_Start, point2D_End);
            }
            else
            {
                vector2D = rectangle2D.HeightDirection;
                Vector2D vector_Height = rectangle2D.HeightDirection * (rectangle2D.Height / 2);
                Point2D point2D_Start = rectangle2D.Origin.GetMoved(vector_Height);
                Point2D point2D_End = point2D_Start.GetMoved(rectangle2D.WidthDirection * rectangle2D.Width);

                segment2D = new Segment2D(point2D_Start, point2D_End);
            }

            vector2D = vector2D * (rectangle2D.GetDiagonals()[0].GetLength() / 2);
            Vector2D vector2D_Negate = vector2D.GetNegated();

            IEnumerable<Point2D> point2Ds = Split(segment2D.Start, segment2D.End, count);

            if (point2Ds == null)
                return null;

            int aCount = point2Ds.Count();

            if (aCount <= 2)
                return new List<Polygon2D>() { new Polygon2D(polygon2D) };

            List<Polygon2D> result = new List<Polygon2D>();

            List<Segment2D> segment2Ds = new List<Segment2D>();
            for (int i = 1; i < aCount - 1; i++)
            {
                Point2D point2D = point2Ds.ElementAt(i);
                segment2Ds.Add(new Segment2D(point2D.GetMoved(vector2D), point2D.GetMoved(vector2D_Negate)));
            }

            segment2Ds.AddRange(polygon2D.GetSegments());

            if (segment2Ds == null || segment2Ds.Count < 4)
                return new List<Polygon2D>() { new Polygon2D(polygon2D) };

            segment2Ds = Split(segment2Ds, tolerance);
            if (segment2Ds == null || segment2Ds.Count == 0)
                return null;

            List<Segment2D> segment2Ds_Inside = new List<Segment2D>();
            foreach (Segment2D segment2D_Temp in segment2Ds)
            {
                Point2D point2D = segment2D_Temp.Mid();

                if (!polygon2D.Inside(point2D, tolerance) && !polygon2D.On(point2D, tolerance))
                    continue;

                segment2Ds_Inside.Add(segment2D_Temp);
            }

            return Create.Polygon2Ds(segment2Ds_Inside, tolerance);
        }

        public static List<Tuple<Face2D, T>> Split<T>(this Face2D face2D, IEnumerable<Tuple<Face2D, T>> tuples, double tolerance = Core.Tolerance.Distance)
        {
            if (face2D == null || tuples == null || tuples.Count() == 0)
            {
                return null;
            }

            List<Face2D> face2Ds_Temp = new List<Face2D>() { face2D };

            List<Tuple<Face2D, T>> result = new List<Tuple<Face2D, T>>();
            foreach (Tuple<Face2D, T> tuple in tuples)
            {
                List<Face2D> face2Ds_Intersection = new List<Face2D>();
                foreach (Face2D face2D_Temp in face2Ds_Temp)
                {
                    List<Face2D> face2Ds_Intersection_Temp = face2D_Temp.Intersection(tuple.Item1);
                    face2Ds_Intersection_Temp?.RemoveAll(x => x == null || x.GetArea() <= tolerance);
                    if (face2Ds_Intersection_Temp == null || face2Ds_Intersection_Temp.Count == 0)
                    {
                        continue;
                    }

                    face2Ds_Intersection.AddRange(face2Ds_Intersection_Temp);
                }

                result.AddRange(face2Ds_Intersection.ConvertAll(x => new Tuple<Face2D, T>(x, tuple.Item2)));

                List<Face2D> face2Ds_Difference = new List<Face2D>();
                foreach (Face2D face2D_Temp in face2Ds_Temp)
                {
                    List<Face2D> face2Ds_Difference_Temp = face2D_Temp.Difference(face2Ds_Intersection);
                    face2Ds_Difference_Temp?.RemoveAll(x => x == null || x.GetArea() <= tolerance);
                    if (face2Ds_Difference_Temp == null || face2Ds_Difference_Temp.Count == 0)
                    {
                        continue;
                    }

                    face2Ds_Difference.AddRange(face2Ds_Difference_Temp);
                }

                face2Ds_Temp = face2Ds_Difference;
                if (face2Ds_Temp == null || face2Ds_Temp.Count == 0)
                    break;
            }

            if (face2Ds_Temp != null && face2Ds_Temp.Count != 0)
            {
                int index = -1;
                while (index < face2Ds_Temp.Count)
                {
                    index++;

                    for (int i = 0; i < result.Count; i++)
                    {
                        List<Face2D> face2Ds_Union = result[i].Item1.Union(face2Ds_Temp[index]);
                        if (face2Ds_Union == null || face2Ds_Union.Count != 1)
                        {
                            continue;
                        }

                        face2Ds_Temp.RemoveAt(index);
                        result[i] = new Tuple<Face2D, T>(face2Ds_Union[0], result[i].Item2);
                        index = -1;
                        break;
                    }
                }

                if (face2Ds_Temp != null && face2Ds_Temp.Count != 0)
                {
                    List<Tuple<Face2D, T>> tuples_Temp_Sorted = new List<Tuple<Face2D, T>>(result);
                    tuples_Temp_Sorted.Sort((x, y) => y.Item1.GetArea().CompareTo(x.Item1.GetArea()));

                    face2Ds_Temp = face2Ds_Temp.Union(tolerance);

                    foreach (Face2D face2D_Temp in face2Ds_Temp)
                    {
                        result.Add(new Tuple<Face2D, T>(face2D_Temp, tuples_Temp_Sorted[0].Item2));
                    }
                }

            }

            return result;

        }

        public static Polyline2D Split(this Segment2D segment2D, double distance, AlignmentPoint alignmentPoint = AlignmentPoint.Start, double tolerance = Core.Tolerance.Distance)
        {
            if (segment2D == null || double.IsNaN(distance) || alignmentPoint == AlignmentPoint.Undefined || distance <= tolerance)
            {
                return null;
            }

            double length = segment2D.GetLength();
            if (length < distance)
            {
                return new Polyline2D(segment2D.GetPoints(), false);
            }

            Point2D point2D_Start = alignmentPoint == AlignmentPoint.End ? segment2D.GetEnd() : segment2D.GetStart();

            Vector2D vector2D = segment2D.Direction;
            Vector2D vector2D_Temp = null;

            List<Point2D> point2Ds = new List<Point2D>();

            if (alignmentPoint == AlignmentPoint.End)
            {
                vector2D.Negate();
            }
            else if (alignmentPoint == AlignmentPoint.Mid)
            {
                vector2D_Temp = vector2D * ((length % distance) / 2);
                if (vector2D_Temp.Length > tolerance)
                {
                    point2D_Start = point2D_Start.GetMoved(vector2D_Temp) as Point2D;
                    point2Ds.Add(point2D_Start);
                }
            }

            vector2D = vector2D * distance;

            int count = System.Convert.ToInt32(System.Math.Floor(length / distance));
            vector2D_Temp = new Vector2D(vector2D);

            for (int i = 0; i < count; i++)
            {
                point2Ds.Add(point2D_Start.GetMoved(vector2D_Temp) as Point2D);
                vector2D_Temp += vector2D;
            }

            if (point2Ds.Count == 0)
            {
                return null;
            }

            if (alignmentPoint == AlignmentPoint.End)
            {
                point2Ds.Reverse();
            }

            if (!AlmostSimilar(point2Ds.First(), segment2D.GetStart(), tolerance))
            {
                point2Ds.Insert(0, segment2D.GetStart());
            }

            if (!AlmostSimilar(point2Ds.Last(), segment2D.GetEnd(), tolerance))
            {
                point2Ds.Add(segment2D.GetEnd());
            }

            return new Polyline2D(point2Ds, false);


        }
    }
}
