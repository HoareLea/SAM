// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Modify
    {
        public static void RemoveAlmostSimilar_NTS<T>(this List<T> geometries, double tolerance = Core.Tolerance.Distance) where T : NetTopologySuite.Geometries.Geometry
        {
            if (geometries == null)
                return;

            int count = geometries.Count;

            if (double.IsNaN(tolerance))
            {
                // Historical behaviour: Query.AlmostSimilar rejects with `distance > tolerance`,
                // which is false for every pair when tolerance is NaN, so the exhaustive scan
                // marked every later geometry for removal. The bucket pass below would only
                // reach neighbouring buckets and silently change that, so the original scan is
                // kept for this input. NaN is not reinterpreted as a finite tolerance.
                HashSet<int> indexes_HashSet_NaN = new HashSet<int>();
                for (int i = 0; i < count - 1; i++)
                {
                    if (indexes_HashSet_NaN.Contains(i))
                        continue;

                    for (int j = i + 1; j < count; j++)
                    {
                        if (indexes_HashSet_NaN.Contains(j))
                            continue;

                        if (Query.AlmostSimilar(geometries[i], geometries[j], tolerance))
                            indexes_HashSet_NaN.Add(j);
                    }
                }

                List<int> indexes_List_NaN = indexes_HashSet_NaN.ToList();
                indexes_List_NaN.Sort();
                indexes_List_NaN.Reverse();

                indexes_List_NaN.ForEach(x => geometries.RemoveAt(x));
                return;
            }

            // AlmostSimilar demands that every coordinate of one geometry lie within tolerance
            // of the other, in both directions. That forces each envelope bound of the two to
            // agree to within tolerance, so bucketing on the quantised envelope and probing the
            // neighbouring buckets finds every possible partner while skipping the pairs the
            // exact test was always going to reject. Without it this was an O(n^2) sweep of NTS
            // point-to-geometry distance calls.
            Envelope[] envelopes = new Envelope[count];
            for (int i = 0; i < count; i++)
            {
                NetTopologySuite.Geometries.Geometry geometry = geometries[i];
                envelopes[i] = geometry == null || geometry.IsEmpty ? null : geometry.EnvelopeInternal;
            }

            double cellSize = tolerance > 0 ? tolerance : Core.Tolerance.Distance;

            Dictionary<EnvelopeKey, List<int>> dictionary = new Dictionary<EnvelopeKey, List<int>>();
            List<int> unbounded = new List<int>();
            for (int i = 0; i < count; i++)
            {
                if (envelopes[i] == null)
                {
                    unbounded.Add(i);
                    continue;
                }

                EnvelopeKey key = EnvelopeKey.Create(envelopes[i], cellSize);
                if (!dictionary.TryGetValue(key, out List<int> list))
                {
                    list = new List<int>();
                    dictionary[key] = list;
                }

                list.Add(i);
            }

            HashSet<int> indexes_HashSet = new HashSet<int>();
            List<int> candidates = new List<int>();
            for (int i = 0; i < count - 1; i++)
            {
                if (indexes_HashSet.Contains(i))
                    continue;

                NetTopologySuite.Geometries.Geometry geometry_1 = geometries[i];

                candidates.Clear();
                if (envelopes[i] == null)
                {
                    // No usable envelope - keep the original exhaustive behaviour for this one.
                    for (int j = i + 1; j < count; j++)
                        candidates.Add(j);
                }
                else
                {
                    candidates.AddRange(unbounded);
                    EnvelopeKey.Neighbours(envelopes[i], cellSize, dictionary, candidates);
                    candidates.Sort();
                }

                foreach (int j in candidates)
                {
                    if (j <= i || indexes_HashSet.Contains(j))
                        continue;

                    NetTopologySuite.Geometries.Geometry geometry_2 = geometries[j];

                    if (Query.AlmostSimilar(geometry_1, geometry_2, tolerance))
                        indexes_HashSet.Add(j);
                }
            }

            List<int> indexes_List = indexes_HashSet.ToList();
            indexes_List.Sort();
            indexes_List.Reverse();

            indexes_List.ForEach(x => geometries.RemoveAt(x));
        }

        /// <summary>
        /// An envelope quantised to a grid of <c>tolerance</c>-sized cells on each of its four
        /// bounds. Two envelopes whose bounds all agree to within tolerance land either in the
        /// same cell or in an adjacent one on each axis, so probing the 3^4 neighbourhood of a
        /// key is enough to reach every geometry the exact test could accept.
        /// </summary>
        private readonly struct EnvelopeKey : System.IEquatable<EnvelopeKey>
        {
            private readonly long minX;
            private readonly long minY;
            private readonly long maxX;
            private readonly long maxY;

            private EnvelopeKey(long minX, long minY, long maxX, long maxY)
            {
                this.minX = minX;
                this.minY = minY;
                this.maxX = maxX;
                this.maxY = maxY;
            }

            public static EnvelopeKey Create(Envelope envelope, double cellSize)
            {
                return new EnvelopeKey(
                    (long)System.Math.Floor(envelope.MinX / cellSize),
                    (long)System.Math.Floor(envelope.MinY / cellSize),
                    (long)System.Math.Floor(envelope.MaxX / cellSize),
                    (long)System.Math.Floor(envelope.MaxY / cellSize));
            }

            public static void Neighbours(Envelope envelope, double cellSize, Dictionary<EnvelopeKey, List<int>> dictionary, List<int> indexes)
            {
                EnvelopeKey key = Create(envelope, cellSize);

                for (long dMinX = -1; dMinX <= 1; dMinX++)
                {
                    for (long dMinY = -1; dMinY <= 1; dMinY++)
                    {
                        for (long dMaxX = -1; dMaxX <= 1; dMaxX++)
                        {
                            for (long dMaxY = -1; dMaxY <= 1; dMaxY++)
                            {
                                EnvelopeKey key_Temp = new EnvelopeKey(key.minX + dMinX, key.minY + dMinY, key.maxX + dMaxX, key.maxY + dMaxY);
                                if (dictionary.TryGetValue(key_Temp, out List<int> list))
                                    indexes.AddRange(list);
                            }
                        }
                    }
                }
            }

            public bool Equals(EnvelopeKey other)
            {
                return minX == other.minX && minY == other.minY && maxX == other.maxX && maxY == other.maxY;
            }

            public override bool Equals(object obj)
            {
                return obj is EnvelopeKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;
                    hash = (hash * 31) ^ minX.GetHashCode();
                    hash = (hash * 31) ^ minY.GetHashCode();
                    hash = (hash * 31) ^ maxX.GetHashCode();
                    hash = (hash * 31) ^ maxY.GetHashCode();
                    return hash;
                }
            }
        }

        /// <summary>
        /// Removes segments from segment2Ds list which are similar to segmentable2D segments
        /// </summary>
        public static void RemoveAlmostSimilar(this ISegmentable2D segmentable2D, List<Segment2D> segment2Ds, double tolerance = Core.Tolerance.Distance)
        {
            if (segmentable2D == null || segment2Ds == null || segment2Ds.Count() == 0)
                return;

            List<Segment2D> segment2Ds_Segmentable = segmentable2D.GetSegments();

            HashSet<int> indexes = new HashSet<int>();
            for (int i = 0; i < segment2Ds.Count; i++)
            {
                foreach (Segment2D segment2D_Segmentable in segment2Ds_Segmentable)
                {
                    if (!segment2Ds[i].AlmostSimilar(segment2D_Segmentable, tolerance))
                        continue;

                    indexes.Add(i);
                    break;
                }
            }

            if (indexes.Count == 0)
                return;

            List<int> indexes_List = indexes.ToList();
            indexes_List.Sort((x, y) => y.CompareTo(x));

            indexes_List.ForEach(x => segment2Ds.RemoveAt(x));
        }

        /// <summary>
        /// Removes segments from segment2Ds list which are similar to segmentable2D segments
        /// </summary>
        public static void RemoveAlmostSimilar<T>(List<T> segmentable2Ds, double tolerance = Core.Tolerance.Distance) where T : ISegmentable2D
        {
            if (segmentable2Ds == null)
                return;

            if (segmentable2Ds.Count < 32)
            {
                // Small inputs: the original exhaustive scan, without the index setup cost.
                List<T> result_Small = new List<T>();
                foreach (T segmentable2D in segmentable2Ds)
                    if (result_Small.Find(x => Query.AlmostSimilar(x, segmentable2D, tolerance)) == null)
                        result_Small.Add(segmentable2D);

                segmentable2Ds.Clear();
                segmentable2Ds.AddRange(result_Small);
                return;
            }

            // AlmostSimilar demands that every point of each geometry lie on the other within
            // tolerance, in both directions, so the two bounding boxes must agree to within
            // tolerance on every bound - a necessary condition the kept set is indexed on.
            // The old code scanned the whole result with Query.AlmostSimilar for every new
            // geometry; the grid narrows that to the box-compatible kept geometries. The exact
            // predicate still decides every candidate, and the first occurrence still wins.
            BoundingBox2D[] boundingBox2Ds = new BoundingBox2D[segmentable2Ds.Count];
            for (int i = 0; i < segmentable2Ds.Count; i++)
            {
                boundingBox2Ds[i] = BoundingBox(segmentable2Ds[i]);
            }

            BoundingBox2DGrid grid = new BoundingBox2DGrid(tolerance, BoundingBox2DGrid.CellSizeHint(boundingBox2Ds));

            // Query.AlmostSimilar answers true for the same instance before it looks at the
            // tolerance, so an instance repeated in the input is similar to itself whatever the
            // tolerance is - including a negative one, where BoundsInRange rejects every pair,
            // identical bounds included. The spatial index cannot carry that; identity is
            // tracked beside it so the exhaustive scan's result is preserved exactly.
            HashSet<object> instances = new HashSet<object>(ReferenceComparer.Instance);

            List<T> result = new List<T>();
            for (int i = 0; i < segmentable2Ds.Count; i++)
            {
                T segmentable2D = segmentable2Ds[i];

                if (instances.Contains(segmentable2D))
                {
                    continue;
                }

                bool similar = false;
                foreach (int index in grid.Candidates(boundingBox2Ds[i]))
                {
                    // Cheap necessary condition first: bounds must agree within tolerance.
                    // It only skips pairs the exact predicate would always reject.
                    if (!BoundsInRange(boundingBox2Ds[i], grid[index], tolerance))
                    {
                        continue;
                    }

                    if (Query.AlmostSimilar(result[index], segmentable2D, tolerance))
                    {
                        similar = true;
                        break;
                    }
                }

                if (similar)
                {
                    continue;
                }

                grid.Add(boundingBox2Ds[i]);
                instances.Add(segmentable2D);
                result.Add(segmentable2D);
            }

            segmentable2Ds.Clear();
            segmentable2Ds.AddRange(result);
        }

        private static bool BoundsInRange(BoundingBox2D boundingBox2D_1, BoundingBox2D boundingBox2D_2, double tolerance)
        {
            // Necessary condition for Query.AlmostSimilar(ISegmentable2D, ...): every point of
            // each geometry lies on the other within tolerance, so all four bounding-box bounds
            // must agree within tolerance. NaN tolerance passes everything through - the exact
            // predicate keeps its historical NaN behaviour (reference equality still matches).
            if (boundingBox2D_1 == null || boundingBox2D_2 == null || double.IsNaN(tolerance))
            {
                return true;
            }

            return System.Math.Abs(boundingBox2D_1.Min.X - boundingBox2D_2.Min.X) <= tolerance
                && System.Math.Abs(boundingBox2D_1.Min.Y - boundingBox2D_2.Min.Y) <= tolerance
                && System.Math.Abs(boundingBox2D_1.Max.X - boundingBox2D_2.Max.X) <= tolerance
                && System.Math.Abs(boundingBox2D_1.Max.Y - boundingBox2D_2.Max.Y) <= tolerance;
        }

        private static BoundingBox2D BoundingBox(ISegmentable2D segmentable2D)
        {
            List<Point2D> point2Ds = segmentable2D?.GetPoints();
            if (point2Ds == null || point2Ds.Count == 0)
            {
                return null;
            }

            double minX = double.MaxValue;
            double minY = double.MaxValue;
            double maxX = double.MinValue;
            double maxY = double.MinValue;

            bool found = false;
            foreach (Point2D point2D in point2Ds)
            {
                if (point2D == null)
                {
                    continue;
                }

                minX = System.Math.Min(minX, point2D.X);
                minY = System.Math.Min(minY, point2D.Y);
                maxX = System.Math.Max(maxX, point2D.X);
                maxY = System.Math.Max(maxY, point2D.Y);
                found = true;
            }

            return found ? new BoundingBox2D(new Point2D(minX, minY), new Point2D(maxX, maxY)) : null;
        }
    }
}
