// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        /// <summary>
        /// Check if given point is above plane
        /// </summary>
        /// <param name="plane"></param>
        /// <param name="point3D"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static bool Above(this Plane plane, Point3D point3D, double tolerance = 0)
        {
            if (point3D == null || plane == null)
                return false;

            Vector3D normal = plane.InternalNormal;
            Point3D origin = plane.InternalOrigin;
            if (normal == null || origin == null)
                return false;

            return (normal.X * (point3D.X - origin.X)) + (normal.Y * (point3D.Y - origin.Y)) + (normal.Z * (point3D.Z - origin.Z)) > 0 + tolerance;
        }

        public static bool Above(this Plane plane, Face3D face3D, double tolerance)
        {
            if (face3D == null)
            {
                return false;
            }

            ISegmentable3D segmentable3D = face3D.GetExternalEdge3D() as ISegmentable3D;
            if (segmentable3D == null)
            {
                throw new System.NotImplementedException();
            }


            List<Point3D> point3Ds = segmentable3D.GetPoints();
            if (point3Ds == null || point3Ds.Count == 0)
            {
                return false;
            }

            return point3Ds.TrueForAll(x => plane.Above(x, tolerance));
        }

        public static bool Above(this Plane plane, Shell shell, double tolerance)
        {
            if (plane == null || shell == null)
            {
                return false;
            }

            BoundingBox3D boundingBox3D = shell.GetBoundingBox();

            if (boundingBox3D == null)
            {
                return false;
            }

            if (Above(plane, boundingBox3D, tolerance))
            {
                return true;
            }

            return shell.Boundaries.TrueForAll(x => plane.Above(x.Item1, tolerance) && plane.Above(x.Item2, tolerance));
        }

        public static bool Above(this Plane plane, BoundingBox3D boundingBox3D, double tolerance)
        {
            if (plane == null || boundingBox3D == null)
            {
                return false;
            }

            Vector3D normal = plane.InternalNormal;
            Point3D origin = plane.InternalOrigin;
            if (normal == null || origin == null)
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

            double minProj = (((normal.X >= 0 ? minX : maxX) - origin.X) * normal.X)
                           + (((normal.Y >= 0 ? minY : maxY) - origin.Y) * normal.Y)
                           + (((normal.Z >= 0 ? minZ : maxZ) - origin.Z) * normal.Z);

            return minProj > tolerance;
        }
    }
}
