// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Geometry.Planar;
using System;
using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static ISAMGeometry2D Convert(this Plane plane, IBoundable3D boundable3D)
        {
            if (plane == null || boundable3D == null)
                return null;

            return Convert(plane, boundable3D as dynamic);
        }

        public static Polycurve2D Convert(this Plane plane, Polycurve3D polycurve3D)
        {
            if (plane == null || polycurve3D == null)
                return null;

            List<ICurve3D> curve3Ds = polycurve3D.GetCurves();

            if (curve3Ds.TrueForAll(x => x is Segment3D))
                return new Polycurve2D(curve3Ds.ConvertAll(x => Convert(plane, (Segment3D)x)));

            throw new NotImplementedException();
        }

        public static Polycurve3D Convert(this Plane plane, Polycurve2D polycurve2D)
        {
            if (plane == null || polycurve2D == null)
                return null;

            List<ICurve2D> curve2Ds = polycurve2D.GetCurves();

            if (curve2Ds.TrueForAll(x => x is Segment2D))
                return new Polycurve3D(curve2Ds.ConvertAll(x => Convert(plane, (Segment2D)x)));

            throw new NotImplementedException();
        }

        public static ISAMGeometry3D Convert(this Plane plane, ISAMGeometry2D sAMGeometry2D)
        {
            if (plane == null || sAMGeometry2D == null)
                return null;

            return Convert(plane, sAMGeometry2D as dynamic);
        }

        public static PolycurveLoop3D Convert(this Plane plane, PolycurveLoop2D polycurveLoop2D)
        {
            if (plane == null || polycurveLoop2D == null)
                return null;

            List<ICurve2D> curve2Ds = polycurveLoop2D.GetCurves();

            if (curve2Ds.TrueForAll(x => x is Segment2D))
                return new PolycurveLoop3D(curve2Ds.ConvertAll(x => Convert(plane, (Segment2D)x)));

            throw new NotImplementedException();
        }

        public static ICurve3D Convert(this Plane plane, ICurve2D curve2D)
        {
            if (plane == null || curve2D == null)
                return null;

            return Convert(plane, curve2D as dynamic);
        }

        public static Point3D Convert(this Plane plane, Point2D point2D)
        {
            if (plane == null || point2D == null)
                return null;

            Vector3D axisX = plane.AxisX;
            Vector3D axisY = plane.InternalAxisY;
            Point3D origin = plane.InternalOrigin;

            double px = point2D.X;
            double py = point2D.Y;

            return new Point3D(origin.X + axisX.X * px + axisY.X * py, origin.Y + axisX.Y * px + axisY.Y * py, origin.Z + axisX.Z * px + axisY.Z * py);
        }

        public static Point2D Convert(this Plane plane, Point3D point3D)
        {
            if (plane == null || point3D == null)
                return null;

            Vector3D axisX = plane.AxisX;
            Vector3D axisY = plane.InternalAxisY;
            Point3D origin = plane.InternalOrigin;

            double dx = point3D.X - origin.X;
            double dy = point3D.Y - origin.Y;
            double dz = point3D.Z - origin.Z;

            return new Point2D(axisX.X * dx + axisX.Y * dy + axisX.Z * dz, axisY.X * dx + axisY.Y * dy + axisY.Z * dz);
        }

        public static Vector2D Convert(this Plane plane, Vector3D vector3D)
        {
            if (plane == null || vector3D == null)
                return null;

            Vector3D axisX = plane.AxisX;
            Vector3D axisY = plane.InternalAxisY;

            double vx = vector3D.X;
            double vy = vector3D.Y;
            double vz = vector3D.Z;

            return new Vector2D(axisX.X * vx + axisX.Y * vy + axisX.Z * vz, axisY.X * vx + axisY.Y * vy + axisY.Z * vz);
        }

        public static Vector3D Convert(this Plane plane, Vector2D vector2D)
        {
            if (plane == null || vector2D == null)
                return null;

            Vector3D axisX = plane.AxisX;
            Vector3D axisY = plane.InternalAxisY;

            double vx = vector2D.X;
            double vy = vector2D.Y;

            return new Vector3D(axisX.X * vx + axisY.X * vy, axisX.Y * vx + axisY.Y * vy, axisX.Z * vx + axisY.Z * vy);
        }

        public static Polygon3D Convert(this Plane plane, Polygon2D polygon2D)
        {
            if (plane == null || polygon2D == null)
                return null;

            //return new Polygon3D(Convert(polygon2D.Points));
            return new Polygon3D(plane, polygon2D.Points);
        }

        public static SAMGeometry3DGroup Convert(this Plane plane, SAMGeometry2DGroup sAMGeometry2Ds)
        {
            if (plane == null || sAMGeometry2Ds == null)
            {
                return null;
            }

            List<ISAMGeometry3D> sAMGeometry3D = new List<ISAMGeometry3D>();
            foreach (ISAMGeometry2D sAMGeometry2D in sAMGeometry2Ds)
            {
                ISAMGeometry3D sAMGeometry3D_Temp = Convert(plane, sAMGeometry2D);
                sAMGeometry3D.Add(sAMGeometry3D_Temp);
            }

            return new SAMGeometry3DGroup(sAMGeometry3D);
        }

        public static Rectangle3D Convert(this Plane plane, Rectangle2D rectangle2D)
        {
            if (plane == null || rectangle2D == null)
                return null;

            return new Rectangle3D(plane, rectangle2D);
            //return new Polygon3D(plane, rectangle2D.GetPoints());
        }

        public static Polygon2D Convert(this Plane plane, Polygon3D polygon3D)
        {
            if (plane == null || polygon3D == null)
                return null;

            List<Point3D> point3Ds = polygon3D.GetPoints();
            if (point3Ds == null || point3Ds.Count < 3)
                return null;

            return new Polygon2D(Convert(plane, point3Ds));
        }

        public static Polyline2D Convert(this Plane plane, Polyline3D polyline3D)
        {
            if (plane == null || polyline3D == null)
                return null;

            List<Point3D> point3Ds = polyline3D.GetPoints();
            if (point3Ds == null)
                return null;

            return new Polyline2D(Convert(plane, point3Ds));
        }

        public static Polyline3D Convert(this Plane plane, Polyline2D polyline2D)
        {
            if (plane == null || polyline2D == null)
                return null;

            List<Point2D> point2Ds = polyline2D.GetPoints();
            if (point2Ds == null)
                return null;

            return new Polyline3D(Convert(plane, point2Ds));
        }

        public static Triangle2D Convert(this Plane plane, Triangle3D triangle3D)
        {
            if (plane == null || triangle3D == null)
                return null;

            List<Point3D> point3Ds = triangle3D.GetPoints();
            if (point3Ds == null)
                return null;

            return new Triangle2D(Convert(plane, point3Ds[0]), Convert(plane, point3Ds[1]), Convert(plane, point3Ds[2]));
        }

        public static Triangle3D Convert(this Plane plane, Triangle2D triangle2D)
        {
            if (plane == null || triangle2D == null)
                return null;

            List<Point2D> point2Ds = triangle2D.GetPoints();
            if (point2Ds == null)
                return null;

            return new Triangle3D(Convert(plane, point2Ds[0]), Convert(plane, point2Ds[1]), Convert(plane, point2Ds[2]));
        }

        public static Segment2D Convert(this Plane plane, Segment3D segment3D)
        {
            if (plane == null || segment3D == null)
                return null;

            return new Segment2D(Convert(plane, segment3D[0]), Convert(plane, segment3D[1]));
        }

        public static Segment3D Convert(this Plane plane, Segment2D segment2D)
        {
            if (plane == null || segment2D == null)
                return null;

            return new Segment3D(Convert(plane, segment2D[0]), Convert(plane, segment2D[1]));
        }

        public static Line2D Convert(this Plane plane, Line3D line3D)
        {
            if (plane == null || line3D == null)
                return null;

            return new Line2D(Convert(plane, line3D.Origin), Convert(plane, line3D.Direction));
        }

        public static Line3D Convert(this Plane plane, Line2D line2D)
        {
            if (plane == null || line2D == null)
                return null;

            return new Line3D(Convert(plane, line2D.Origin), Convert(plane, line2D.Direction));
        }

        public static IClosedPlanar3D Convert(this Plane plane, IClosed2D closed2D)
        {
            if (plane == null || closed2D == null)
                return null;

            return Convert(plane, closed2D as dynamic);
        }

        public static Face2D Convert(this Plane plane, Face3D face3D)
        {
            if (plane == null)
            {
                return null;
            }

            IClosedPlanar3D closedPlanar3D_External = face3D?.GetExternalEdge3D();
            if (closedPlanar3D_External == null)
            {
                return null;
            }

            IClosed2D closed2D_external = Convert(plane, closedPlanar3D_External);
            if (closed2D_external == null)
            {
                return null;
            }

            List<IClosed2D> closed2Ds_internal = new List<IClosed2D>();
            List<IClosedPlanar3D> closedPlanar3Ds_Internal = face3D.GetInternalEdge3Ds();
            if (closedPlanar3Ds_Internal != null && closedPlanar3Ds_Internal.Count > 0)
                closedPlanar3Ds_Internal.ForEach(x => closed2Ds_internal.Add(Convert(plane, x)));

            return Planar.Face2D.Create(closed2D_external, closed2Ds_internal);
        }

        public static Face3D Convert(this Plane plane, Face2D face2D)
        {
            if (plane == null || face2D == null)
                return null;

            return new Face3D(plane, face2D);
        }

        public static Rectangle2D Convert(this Plane plane, Rectangle3D rectangle3D)
        {
            if (plane == null)
            {
                return null;
            }

            Vector3D vector3D_Width = rectangle3D.WidthDirection * rectangle3D.Width;
            Vector3D vector3D_Height = rectangle3D.HeightDirection * rectangle3D.Height;

            Vector2D vector2D_Height = plane.Convert(vector3D_Height);

            return new Rectangle2D(plane.Convert(rectangle3D.Origin), plane.Convert(vector3D_Width).Length, vector2D_Height.Length, vector2D_Height.Unit);

        }

        public static IClosed2D Convert(this Plane plane, IClosed3D closed3D)
        {
            if (plane == null || closed3D == null)
                return null;

            return Convert(plane, closed3D as dynamic);
        }

        public static List<Point2D> Convert(this Plane plane, IEnumerable<Point3D> point3Ds)
        {
            if (plane == null || point3Ds == null)
                return null;

            Vector3D axisX = plane.AxisX;
            Vector3D axisY = plane.InternalAxisY;
            Point3D origin = plane.InternalOrigin;

            if (axisX == null || axisY == null || origin == null)
                return null;

            double ox = origin.X, oy = origin.Y, oz = origin.Z;
            double xx = axisX.X, xy = axisX.Y, xz = axisX.Z;
            double yx = axisY.X, yy = axisY.Y, yz = axisY.Z;

            List<Point2D> point2Ds = new List<Point2D>();
            foreach (Point3D point3D in point3Ds)
            {
                if (point3D == null)
                {
                    point2Ds.Add(null);
                    continue;
                }

                double dx = point3D.X - ox;
                double dy = point3D.Y - oy;
                double dz = point3D.Z - oz;

                point2Ds.Add(new Point2D(xx * dx + xy * dy + xz * dz, yx * dx + yy * dy + yz * dz));
            }

            return point2Ds;
        }

        public static List<Point3D> Convert(this Plane plane, IEnumerable<Point2D> point2Ds)
        {
            if (plane == null || point2Ds == null)
                return null;

            Vector3D axisX = plane.AxisX;
            Vector3D axisY = plane.InternalAxisY;
            Point3D origin = plane.InternalOrigin;

            if (axisX == null || axisY == null || origin == null)
                return null;

            double ox = origin.X, oy = origin.Y, oz = origin.Z;
            double xx = axisX.X, xy = axisX.Y, xz = axisX.Z;
            double yx = axisY.X, yy = axisY.Y, yz = axisY.Z;

            List<Point3D> point3Ds = new List<Point3D>();
            foreach (Point2D point2D in point2Ds)
            {
                if (point2D == null)
                {
                    point3Ds.Add(null);
                    continue;
                }

                double px = point2D.X;
                double py = point2D.Y;

                point3Ds.Add(new Point3D(ox + xx * px + yx * py, oy + xy * px + yy * py, oz + xz * px + yz * py));
            }

            return point3Ds;
        }

        public static Point3D Convert(this Sphere sphere, Point2D point2D)
        {
            if (sphere == null || point2D == null)
            {
                return null;
            }

            Point3D origin = sphere.Origin;

            Vector3D vector3D = new Vector3D(System.Math.Sin(point2D.X) * System.Math.Cos(point2D.Y), System.Math.Sin(point2D.X) * System.Math.Sin(point2D.Y), System.Math.Cos(point2D.X));
            vector3D.Scale(sphere.Radius);

            return origin.GetMoved(vector3D) as Point3D;
        }

        public static Point3D Convert(this CoordinateSystem3D coordinateSystem3D_From, CoordinateSystem3D coordinateSystem3D_To, Point3D point3D)
        {
            if (coordinateSystem3D_From == null || coordinateSystem3D_To == null || point3D == null)
            {
                return null;
            }

            Transform3D transform3D = Transform3D.GetCoordinateSystem3DToCoordinateSystem3D(coordinateSystem3D_From, coordinateSystem3D_To);
            if (transform3D == null)
            {
                return null;
            }

            return point3D.Transform(transform3D);
        }

        public static List<Point3D> Convert(this CoordinateSystem3D coordinateSystem3D_From, CoordinateSystem3D coordinateSystem3D_To, IEnumerable<Point3D> point3Ds)
        {
            if (coordinateSystem3D_From == null || coordinateSystem3D_To == null || point3Ds == null)
            {
                return null;
            }

            List<Point3D> result = new List<Point3D>();
            foreach (Point3D point3D in point3Ds)
            {
                result.Add(Convert(coordinateSystem3D_From, coordinateSystem3D_To, point3D));
            }

            return result;
        }

        public static Point3D Convert(this CoordinateSystem3D coordinateSystem3D_From, CoordinateSystem3D coordinateSystem3D_To, Vector3D vector3D)
        {
            if (coordinateSystem3D_From == null || coordinateSystem3D_To == null || vector3D == null)
            {
                return null;
            }

            Transform3D transform3D = Transform3D.GetCoordinateSystem3DToCoordinateSystem3D(coordinateSystem3D_From, coordinateSystem3D_To);
            if (transform3D == null)
            {
                return null;
            }

            return vector3D.Transform(transform3D);
        }

        public static Plane Convert(this CoordinateSystem3D coordinateSystem3D_From, CoordinateSystem3D coordinateSystem3D_To, Plane plane)
        {
            if (coordinateSystem3D_From == null || coordinateSystem3D_To == null || plane == null)
            {
                return null;
            }

            Point3D origin = Convert(coordinateSystem3D_From, coordinateSystem3D_To, plane.Origin);
            Vector3D axisX = Convert(coordinateSystem3D_From, coordinateSystem3D_To, plane.AxisX);
            Vector3D axisY = Convert(coordinateSystem3D_From, coordinateSystem3D_To, plane.AxisY);

            return new Plane(origin, axisX, axisY);
        }

        public static Polygon3D Convert(this CoordinateSystem3D coordinateSystem3D_From, CoordinateSystem3D coordinateSystem3D_To, Polygon3D polygon3D)
        {
            if (coordinateSystem3D_From == null || coordinateSystem3D_To == null || polygon3D == null)
            {
                return null;
            }

            Plane plane = Convert(coordinateSystem3D_From, coordinateSystem3D_To, polygon3D.GetPlane());
            if (plane == null)
            {
                return null;
            }

            List<Point3D> point3Ds = Convert(coordinateSystem3D_From, coordinateSystem3D_To, polygon3D.GetPoints());
            if (point3Ds == null)
            {
                return null;
            }

            return new Polygon3D(plane, point3Ds.ConvertAll(x => Convert(plane, x)));
        }

        public static Rectangle3D Convert(this CoordinateSystem3D coordinateSystem3D_From, CoordinateSystem3D coordinateSystem3D_To, Rectangle3D rectangle3D)
        {
            if (coordinateSystem3D_From == null || coordinateSystem3D_To == null || rectangle3D == null)
            {
                return null;
            }

            Transform3D transform3D = Transform3D.GetCoordinateSystem3DToCoordinateSystem3D(coordinateSystem3D_From, coordinateSystem3D_To);

            return rectangle3D.Transform(transform3D);
        }

        public static Triangle3D Convert(this CoordinateSystem3D coordinateSystem3D_From, CoordinateSystem3D coordinateSystem3D_To, Triangle3D triangle3D)
        {
            if (coordinateSystem3D_From == null || coordinateSystem3D_To == null || triangle3D == null)
            {
                return null;
            }

            Point3D point3D_1 = Convert(coordinateSystem3D_From, coordinateSystem3D_To, triangle3D[0]);
            Point3D point3D_2 = Convert(coordinateSystem3D_From, coordinateSystem3D_To, triangle3D[1]);
            Point3D point3D_3 = Convert(coordinateSystem3D_From, coordinateSystem3D_To, triangle3D[2]);

            return new Triangle3D(point3D_1, point3D_2, point3D_3);
        }

        public static Point3D Convert(CoordinateSystem3D coordinateSystem3D, Point3D point3D)
        {
            return Convert(CoordinateSystem3D.World, coordinateSystem3D, point3D);
        }

    }
}
