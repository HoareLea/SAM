// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static bool Intersect(this Plane plane, BoundingBox3D boundingBox3D, double tolerance = Core.Tolerance.Distance)
        {
            if (plane == null || boundingBox3D == null)
            {
                return false;
            }

            Vector3D normal = plane.Normal;
            Point3D origin = plane.Origin;
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

            double centerX = (minX + maxX) * 0.5;
            double centerY = (minY + maxY) * 0.5;
            double centerZ = (minZ + maxZ) * 0.5;

            double dist = (normal.X * (centerX - origin.X)) + (normal.Y * (centerY - origin.Y)) + (normal.Z * (centerZ - origin.Z));
            double hx = (maxX - minX) * 0.5;
            double hy = (maxY - minY) * 0.5;
            double hz = (maxZ - minZ) * 0.5;

            double r = (System.Math.Abs(normal.X) * hx) + (System.Math.Abs(normal.Y) * hy) + (System.Math.Abs(normal.Z) * hz);
            return System.Math.Abs(dist) <= (r + tolerance);
        }

        public static bool Intersect(this Face3D face3D, Point3D point3D, Vector3D vector3D, double tolerance = Core.Tolerance.Distance)
        {
            if (face3D == null || point3D == null || vector3D == null)
            {
                return false;
            }

            PlanarIntersectionResult planarIntersectionResult = Create.PlanarIntersectionResult(face3D, point3D, vector3D, tolerance);
            if (planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
            {
                return false;
            }

            Point3D point3D_Intersection = planarIntersectionResult.GetGeometry3Ds<Point3D>()?.FirstOrDefault();

            return point3D_Intersection != null && point3D_Intersection.IsValid();
        }
    }
}
