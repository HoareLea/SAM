// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SAM.Core;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static List<Point2D> ConvexHull(this IEnumerable<Point2D> point2Ds)
        {
            if (point2Ds == null)
                return null;

            List<Point2D> points;
            if (point2Ds is List<Point2D> listInput)
            {
                points = new List<Point2D>(listInput.Count);
                for (int i = 0; i < listInput.Count; i++)
                {
                    Point2D p = listInput[i];
                    if (p != null) points.Add(p);
                }
            }
            else
            {
                points = new List<Point2D>(point2Ds.CountOrDefault());
                foreach (Point2D p in point2Ds)
                {
                    if (p != null) points.Add(p);
                }
            }

            int count = points.Count;
            if (count <= 3)
            {
                if (count == 0)
                    return [];
                if (count == 1)
                    return [points[0]];
                if (count == 2)
                {
                    if (points[0].X == points[1].X && points[0].Y == points[1].Y)
                        return [points[0]];
                    return [points[0], points[1]];
                }
                if (count == 3)
                {
                    double cp = CrossProduct(points[0], points[1], points[2]);
                    if (cp == 0)
                    {
                        points.Sort(ConvexHullComparer.Instance);
                        if (points[0].X == points[2].X && points[0].Y == points[2].Y)
                            return [points[0]];
                        return [points[0], points[2]];
                    }
                }
            }

            // Akl-Toussaint heuristic pre-filtering for large point clouds
            if (count >= 16)
            {
                points = FilterAklToussaint(points);
                count = points.Count;
            }

            // Sort points lexicographically by (X, Y)
            points.Sort(ConvexHullComparer.Instance);

            // Andrew's Monotone Chain Hull construction
            Point2D[] hull = new Point2D[2 * count];
            int k = 0;

            // Lower hull scan (left to right)
            for (int i = 0; i < count; i++)
            {
                Point2D p = points[i];
                while (k >= 2 && CrossProduct(hull[k - 2], hull[k - 1], p) >= 0)
                {
                    k--;
                }
                hull[k++] = p;
            }

            // Upper hull scan (right to left)
            int t = k + 1;
            for (int i = count - 2; i >= 0; i--)
            {
                Point2D p = points[i];
                while (k >= t && CrossProduct(hull[k - 2], hull[k - 1], p) >= 0)
                {
                    k--;
                }
                hull[k++] = p;
            }

            k--; // Remove duplicate endpoint

            if (k <= 0)
                return [];

            List<Point2D> result = new List<Point2D>(k);
            for (int i = 0; i < k; i++)
            {
                result.Add(hull[i]);
            }
            return result;
        }

        public static List<Point2D> ConvexHull(this IEnumerable<Segment2D> segment2Ds)
        {
            if (segment2Ds == null)
                return null;

            List<Point2D> point2Ds = new List<Point2D>(segment2Ds.CountOrDefault() * 2);

            foreach (Segment2D segment2D in segment2Ds)
            {
                if (segment2D == null) continue;
                Point2D p0 = segment2D[0];
                Point2D p1 = segment2D[1];
                if (p0 != null) point2Ds.Add(p0);
                if (p1 != null) point2Ds.Add(p1);
            }

            return ConvexHull(point2Ds);
        }

        public static List<Point2D> ConvexHull(this ISegmentable2D segmentable2D)
        {
            if (segmentable2D == null)
                return null;

            List<Point2D> points = segmentable2D.GetPoints();
            return ConvexHull(points);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double CrossProduct(Point2D p1, Point2D p2, Point2D p3)
        {
            return (p2.X - p1.X) * (p3.Y - p1.Y) - (p2.Y - p1.Y) * (p3.X - p1.X);
        }

        private static List<Point2D> FilterAklToussaint(List<Point2D> points)
        {
            int n = points.Count;
            Point2D minX = points[0], maxX = points[0];
            Point2D minY = points[0], maxY = points[0];

            for (int i = 1; i < n; i++)
            {
                Point2D p = points[i];
                if (p.X < minX.X || (p.X == minX.X && p.Y < minX.Y)) minX = p;
                if (p.X > maxX.X || (p.X == maxX.X && p.Y > maxX.Y)) maxX = p;
                if (p.Y < minY.Y || (p.Y == minY.Y && p.X < minY.X)) minY = p;
                if (p.Y > maxY.Y || (p.Y == maxY.Y && p.X > maxY.X)) maxY = p;
            }

            // Collect unique extreme points in CCW order around bounding quad
            List<Point2D> quad = new List<Point2D>(4);
            AddIfUnique(quad, minX);
            AddIfUnique(quad, minY);
            AddIfUnique(quad, maxX);
            AddIfUnique(quad, maxY);

            int qCount = quad.Count;
            if (qCount < 3)
                return points; // Extreme points are collinear or identical

            // Ensure quad vertices are CCW
            double areaSum = 0;
            for (int i = 0; i < qCount; i++)
            {
                Point2D p1 = quad[i];
                Point2D p2 = quad[(i + 1) % qCount];
                areaSum += (p2.X - p1.X) * (p2.Y + p1.Y);
            }
            if (areaSum > 0) // Clockwise
            {
                quad.Reverse();
            }

            double[] baseX = new double[qCount];
            double[] baseY = new double[qCount];
            double[] edgeX = new double[qCount];
            double[] edgeY = new double[qCount];
            for (int i = 0; i < qCount; i++)
            {
                Point2D a = quad[i];
                Point2D b = quad[(i + 1) % qCount];
                baseX[i] = a.X;
                baseY[i] = a.Y;
                edgeX[i] = b.X - a.X;
                edgeY[i] = b.Y - a.Y;
            }

            List<Point2D> filtered = new List<Point2D>(n / 4 + 8);
            for (int i = 0; i < n; i++)
            {
                Point2D p = points[i];

                // A point is discarded only when it lies strictly left of every edge of
                // the CCW extreme polygon (i.e. strictly interior). Extreme points and
                // any point on an edge yield a cross product <= 0 and are kept, so no
                // separate extreme-point check is needed.
                bool strictlyInside = true;
                for (int e = 0; e < qCount; e++)
                {
                    if (edgeX[e] * (p.Y - baseY[e]) - edgeY[e] * (p.X - baseX[e]) <= 0)
                    {
                        strictlyInside = false;
                        break;
                    }
                }

                if (!strictlyInside)
                {
                    filtered.Add(p);
                }
            }

            return filtered;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void AddIfUnique(List<Point2D> list, Point2D p)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].X == p.X && list[i].Y == p.Y) return;
            }
            list.Add(p);
        }
    }
}
