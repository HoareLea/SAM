// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static bool On(this ISegmentable3D segmentable3D, Point3D point3D, double tolerance = Core.Tolerance.Distance)
        {
            if (segmentable3D == null || point3D == null)
                return false;

            return On(segmentable3D.GetSegments(), point3D, tolerance);
        }

        public static bool On(this IEnumerable<Segment3D> segment3Ds, Point3D point3D, double tolerance = Core.Tolerance.Distance)
        {
            if (segment3Ds == null || point3D == null)
                return false;

            foreach (Segment3D segment3D in segment3Ds)
            {
                if (segment3D == null)
                    continue;

                if (segment3D.On(point3D, tolerance))
                    return true;
            }

            return false;
        }

        public static bool On(this IEnumerable<ISegmentable3D> segmentable3Ds, Point3D point3D, double tolerance = Core.Tolerance.Distance)
        {
            if (segmentable3Ds == null || point3D == null)
                return false;

            foreach (ISegmentable3D segmentable3D in segmentable3Ds)
                if (On(segmentable3D, point3D, tolerance))
                    return true;

            return false;
        }

        public static bool On(this Plane plane, ISegmentable3D segmentable3D, double tolerance = Core.Tolerance.Distance)
        {
            if (plane == null || segmentable3D == null)
                return false;

            List<Point3D> point3Ds = segmentable3D.GetPoints();
            if (point3Ds == null || point3Ds.Count == 0)
                return false;

            foreach (Point3D point3D in point3Ds)
            {
                if (point3D == null)
                    continue;

                if (!plane.On(point3D, tolerance))
                    return false;
            }

            return true;
        }

        public static bool On(this Plane plane, IPlanar3D planar3D, double tolerance = Core.Tolerance.Distance)
        {
            if (plane == null || planar3D == null)
            {
                return false;
            }

            Plane plane_Planar3D = planar3D.GetPlane();
            if (plane_Planar3D == null)
            {
                return false;
            }

            return Coplanar(plane, plane_Planar3D, tolerance) && plane.On(plane_Planar3D.Origin, tolerance);
        }

        /// <summary>
        /// Determines whether a 3D point lies on any of the 12 edges of a 3D bounding box within distance tolerance.
        /// </summary>
        /// <param name="boundingBox3D">The bounding box.</param>
        /// <param name="point3D">The 3D point.</param>
        /// <param name="tolerance">Distance tolerance.</param>
        /// <returns>True if on edge; otherwise, false.</returns>
        public static bool On(this BoundingBox3D boundingBox3D, Point3D point3D, double tolerance = Core.Tolerance.Distance)
        {
            if (boundingBox3D == null || point3D == null)
            {
                return false;
            }

            double minX = boundingBox3D.MinX;
            double minY = boundingBox3D.MinY;
            double minZ = boundingBox3D.MinZ;
            double maxX = boundingBox3D.MaxX;
            double maxY = boundingBox3D.MaxY;
            double maxZ = boundingBox3D.MaxZ;

            if (double.IsNaN(minX) || double.IsNaN(maxX) || double.IsNaN(minY) || double.IsNaN(maxY) || double.IsNaN(minZ) || double.IsNaN(maxZ))
            {
                return false;
            }

            bool inX = point3D.X >= minX - tolerance && point3D.X <= maxX + tolerance;
            bool inY = point3D.Y >= minY - tolerance && point3D.Y <= maxY + tolerance;
            bool inZ = point3D.Z >= minZ - tolerance && point3D.Z <= maxZ + tolerance;

            if (!inX || !inY || !inZ)
            {
                return false;
            }

            bool onX = System.Math.Abs(point3D.X - minX) <= tolerance || System.Math.Abs(point3D.X - maxX) <= tolerance;
            bool onY = System.Math.Abs(point3D.Y - minY) <= tolerance || System.Math.Abs(point3D.Y - maxY) <= tolerance;
            bool onZ = System.Math.Abs(point3D.Z - minZ) <= tolerance || System.Math.Abs(point3D.Z - maxZ) <= tolerance;

            return (onX && onY) || (onX && onZ) || (onY && onZ);
        }
    }
}
