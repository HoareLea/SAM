// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;
using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    /// <summary>
    /// Operation-local uniform grid over <see cref="Point2D"/> instances, for proximity
    /// searches that used to rescan a growing or repeatedly enumerated point list.
    /// <para>
    /// The grid is a broad-phase index only: it never decides geometric equivalence.
    /// <see cref="Candidates"/> returns the indices of every point that could possibly lie
    /// within the grid tolerance of the query point (plus a margin of one cell against
    /// floating-point cell-boundary rounding), in ascending insertion order, so an exact
    /// predicate evaluated over the candidates sees the same matches in the same order as a
    /// full scan of the same points.
    /// </para>
    /// <para>
    /// Points that cannot be quantised into cells - null, non-finite, or far enough from the
    /// origin that their cell indexes leave <see cref="long"/> range - are held aside and
    /// offered to every query. A null, non-finite, out-of-range or otherwise degenerate query
    /// falls back to the full index. Between the two, no point can become unreachable from a
    /// query that should find it, whatever its coordinates. The index is mutable:
    /// <see cref="Replace"/> keeps it synchronised when a caller moves a point, which
    /// workflows that average endpoints in place require - including moving a point into or
    /// out of the unplaceable set.
    /// </para>
    /// </summary>
    internal sealed class Point2DGrid
    {
        /// <summary>
        /// Largest cell index magnitude that converts to <see cref="long"/> with a defined
        /// result, and still leaves room for the one-cell query padding. Out-of-range
        /// double-to-integer conversion is unspecified in C# - runtimes variously wrap or
        /// saturate - so indexes are range-checked as doubles before any cast. Checking after
        /// the cast cannot work: a saturated value is indistinguishable from a real one.
        /// </summary>
        private const double MaximumCellIndex = 9.0e18;

        private readonly double tolerance;
        private readonly double cellSize;
        private readonly List<Point2D> point2Ds;
        private readonly Dictionary<Tuple<long, long>, List<int>> dictionary;
        private readonly List<int> unbounded;

        public Point2DGrid(double tolerance)
        {
            this.tolerance = tolerance;
            cellSize = tolerance > 0 ? tolerance : Core.Tolerance.Distance;

            point2Ds = new List<Point2D>();
            dictionary = new Dictionary<Tuple<long, long>, List<int>>();
            unbounded = new List<int>();
        }

        public int Count
        {
            get
            {
                return point2Ds.Count;
            }
        }

        public Point2D this[int index]
        {
            get
            {
                return point2Ds[index];
            }
        }

        public int Add(Point2D point2D)
        {
            int index = point2Ds.Count;
            point2Ds.Add(point2D);

            Tuple<long, long> key = Key(point2D, cellSize);
            if (key == null)
            {
                unbounded.Add(index);
                return index;
            }

            if (!dictionary.TryGetValue(key, out List<int> list))
            {
                list = new List<int>();
                dictionary[key] = list;
            }

            list.Add(index);
            return index;
        }

        public void Replace(int index, Point2D point2D)
        {
            Point2D point2D_Old = point2Ds[index];

            Tuple<long, long> key_Old = Key(point2D_Old, cellSize);
            if (key_Old == null)
            {
                unbounded.Remove(index);
            }
            else if (dictionary.TryGetValue(key_Old, out List<int> list_Old))
            {
                list_Old.Remove(index);
            }

            point2Ds[index] = point2D;

            Tuple<long, long> key = Key(point2D, cellSize);
            if (key == null)
            {
                unbounded.Add(index);
                return;
            }

            if (!dictionary.TryGetValue(key, out List<int> list))
            {
                list = new List<int>();
                dictionary[key] = list;
            }

            list.Add(index);
        }

        /// <summary>
        /// Indices of every point that could lie within the grid tolerance of
        /// <paramref name="point2D"/>, in ascending insertion order. Degenerate queries
        /// (null, NaN or infinite coordinates, non-finite tolerance) return the whole index.
        /// </summary>
        public List<int> Candidates(Point2D point2D)
        {
            if (!IsFinite(point2D) || double.IsNaN(tolerance) || double.IsInfinity(tolerance))
            {
                return All();
            }

            double minX = point2D.X - tolerance;
            double maxX = point2D.X + tolerance;
            double minY = point2D.Y - tolerance;
            double maxY = point2D.Y + tolerance;

            if (!IsFinite(minX) || !IsFinite(maxX) || !IsFinite(minY) || !IsFinite(maxY))
            {
                return All();
            }

            double index_MinX = System.Math.Floor(minX / cellSize);
            double index_MaxX = System.Math.Floor(maxX / cellSize);
            double index_MinY = System.Math.Floor(minY / cellSize);
            double index_MaxY = System.Math.Floor(maxY / cellSize);

            // A query whose own cell indexes leave long range cannot be walked, and the points
            // it is looking for could not be placed either. Offer the whole index and let the
            // exact predicate decide, exactly as the list scan this replaced did.
            if (!IsCellIndex(index_MinX) || !IsCellIndex(index_MaxX) || !IsCellIndex(index_MinY) || !IsCellIndex(index_MaxY))
            {
                return All();
            }

            // One cell of padding around the tolerance box: cell assignment involves a
            // floating-point division, so a point legitimately within tolerance can land one
            // cell outside the mathematically exact range at a cell boundary.
            long kx1 = (long)index_MinX - 1;
            long kx2 = (long)index_MaxX + 1;
            long ky1 = (long)index_MinY - 1;
            long ky2 = (long)index_MaxY + 1;

            List<int> result = new List<int>(unbounded);
            for (long kx = kx1; kx <= kx2; kx++)
            {
                for (long ky = ky1; ky <= ky2; ky++)
                {
                    if (dictionary.TryGetValue(new Tuple<long, long>(kx, ky), out List<int> list))
                    {
                        result.AddRange(list);
                    }
                }
            }

            result.Sort();
            return result;
        }

        private List<int> All()
        {
            List<int> result = new List<int>(point2Ds.Count);
            for (int i = 0; i < point2Ds.Count; i++)
            {
                result.Add(i);
            }

            return result;
        }

        /// <summary>
        /// Cell key for a point, or null when the point cannot be quantised - it is null, has
        /// non-finite coordinates, or sits so far from the origin that its cell indexes leave
        /// long range. Null sends the point to the unbounded set, which every query is offered,
        /// so a point that cannot be placed is still always found.
        /// </summary>
        private static Tuple<long, long> Key(Point2D point2D, double cellSize)
        {
            if (!IsFinite(point2D))
            {
                return null;
            }

            double index_X = System.Math.Floor(point2D.X / cellSize);
            double index_Y = System.Math.Floor(point2D.Y / cellSize);

            if (!IsCellIndex(index_X) || !IsCellIndex(index_Y))
            {
                return null;
            }

            return new Tuple<long, long>((long)index_X, (long)index_Y);
        }

        /// <summary>
        /// Whether a cell index is finite and small enough to cast, and to survive the +/-1
        /// query padding, without leaving <see cref="long"/> range. Written so NaN fails.
        /// </summary>
        private static bool IsCellIndex(double value)
        {
            return System.Math.Abs(value) <= MaximumCellIndex;
        }

        private static bool IsFinite(Point2D point2D)
        {
            if (point2D == null)
            {
                return false;
            }

            return IsFinite(point2D.X) && IsFinite(point2D.Y);
        }

        private static bool IsFinite(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }
    }
}
