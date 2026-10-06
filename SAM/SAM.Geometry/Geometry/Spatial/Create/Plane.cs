// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Create
    {

        public static Plane Plane(this Point3D point3D_1, Point3D point3D_2, Point3D point3D_3)
        {
            if (point3D_1 == null || point3D_2 == null || point3D_3 == null)
            {
                return null;
            }


            Vector3D normal = Query.Normal(point3D_1, point3D_2, point3D_3);
            if (normal == null || !normal.IsValid())
            {
                return null;
            }

            Point3D centroid = new Point3D((point3D_1.X + point3D_2.X + point3D_3.X) / 3.0, (point3D_1.Y + point3D_2.Y + point3D_3.Y) / 3.0, (point3D_1.Z + point3D_2.Z + point3D_3.Z) / 3.0);
            return new Plane(centroid, normal);
        }

        public static Plane Plane(this IEnumerable<Point3D> point3Ds, double tolerance = Core.Tolerance.Distance)
        {
            Vector3D normal = Query.Normal(point3Ds, tolerance);
            if (normal == null || !normal.IsValid())
                return null;

            return new Plane(point3Ds.Average(), normal);
        }

        public static Plane Plane(Point3D origin, Vector3D axisX, Vector3D axisY)
        {
            if (origin == null || axisX == null || axisY == null)
                return null;

            return new Plane(origin, axisX, axisY);
        }

        public static Plane Plane(double elevation)
        {
            return new Plane(new Point3D(0, 0, elevation), Spatial.Vector3D.WorldZ);
        }

        public static Plane Plane(double value, int dimensionIndex)
        {
            switch (dimensionIndex)
            {
                case 0:
                    return new Plane(new Point3D(value, 0, 0), Spatial.Vector3D.WorldX);
                case 1:
                    return new Plane(new Point3D(0, value, 0), Spatial.Vector3D.WorldY);
                case 2:
                    return new Plane(new Point3D(0, 0, value), Spatial.Vector3D.WorldZ);
            }

            return null;
        }
    }
}
