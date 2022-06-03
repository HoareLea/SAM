using System;
using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static Planar.ISAMGeometry2D Convert(this Plane plane, IBoundable3D geometry)
        {
            if (plane == null || geometry == null)
                return null;

            return Convert(plane, geometry as dynamic);
        }

        public static Planar.Polycurve2D Convert(this Plane plane, Polycurve3D polycurve3D)
        {
            if (plane == null || polycurve3D == null)
                return null;

            List<ICurve3D> curve3Ds = polycurve3D.GetCurves();

            if (curve3Ds.TrueForAll(x => x is Segment3D))
                return new Planar.Polycurve2D(curve3Ds.ConvertAll(x => Convert(plane, (Segment3D)x)));

            throw new NotImplementedException();
        }

        public static Polycurve3D Convert(this Plane plane, Planar.Polycurve2D polycurve2D)
        {
            if (plane == null || polycurve2D == null)
                return null;

            List<Planar.ICurve2D> curve2Ds = polycurve2D.GetCurves();

            if (curve2Ds.TrueForAll(x => x is Planar.Segment2D))
                return new Polycurve3D(curve2Ds.ConvertAll(x => Convert(plane, (Planar.Segment2D)x)));

            throw new NotImplementedException();
        }

        public static ISAMGeometry3D Convert(this Plane plane, Planar.ISAMGeometry2D geometry)
        {
            if (plane == null || geometry == null)
                return null;

            return Convert(plane, geometry as dynamic);
        }

        public static PolycurveLoop3D Convert(this Plane plane, Planar.PolycurveLoop2D polycurveLoop2D)
        {
            if (plane == null || polycurveLoop2D == null)
                return null;

            List<Planar.ICurve2D> curve2Ds = polycurveLoop2D.GetCurves();

            if (curve2Ds.TrueForAll(x => x is Planar.Segment2D))
                return new PolycurveLoop3D(curve2Ds.ConvertAll(x => Convert(plane, (Planar.Segment2D)x)));

            throw new NotImplementedException();
        }

        public static ICurve3D Convert(this Plane plane, Planar.ICurve2D curve2D)
        {
            if (plane == null || curve2D == null)
                return null;

            return Convert(plane, curve2D as dynamic);
        }

        public static Point3D Convert(this Plane plane, Planar.Point2D point2D)
        {
            if (plane == null || point2D == null)
                return null;

            Vector3D axisX = plane.AxisX;
            Vector3D axisY = plane.AxisY;

            Vector3D u = new Vector3D(axisY.X * point2D.Y, axisY.Y * point2D.Y, axisY.Z * point2D.Y);
            Vector3D v = new Vector3D(axisX.X * point2D.X, axisX.Y * point2D.X, axisX.Z * point2D.X);

            Point3D origin = plane.Origin;

            return new Point3D(origin.X + u.X + v.X, origin.Y + u.Y + v.Y, origin.Z + u.Z + v.Z);
        }

        public static Planar.Point2D Convert(this Plane plane, Point3D point3D)
        {
            if (plane == null || point3D == null)
                return null;

            Vector3D axisX = plane.AxisX;
            Vector3D axisY = plane.AxisY;
            Point3D origin = plane.Origin;

            Vector3D vector3D = new Vector3D(point3D.X - origin.X, point3D.Y - origin.Y, point3D.Z - origin.Z);
            return new Planar.Point2D(axisX.DotProduct(vector3D), axisY.DotProduct(vector3D));
        }

        public static Planar.Vector2D Convert(this Plane plane, Vector3D vector3D)
        {
            if (plane == null || vector3D == null)
                return null;

            Vector3D axisX = plane.AxisX;
            Vector3D axisY = plane.AxisY;

            //Vector3D vector3D_Result = new Vector3D(vector3D.X - origin.X, vector3D.Y - origin.Y, vector3D.Z - origin.Z);
            return new Planar.Vector2D(axisX.DotProduct(vector3D), axisY.DotProduct(vector3D));
        }

        public static Vector3D Convert(this Plane plane, Planar.Vector2D vector2D)
        {
            if (plane == null || vector2D == null)
                return null;

            Vector3D axisX = plane.AxisX;
            Vector3D axisY = plane.AxisY;

            Vector3D u = new Vector3D(axisY.X * vector2D.Y, axisY.Y * vector2D.Y, axisY.Z * vector2D.Y);
            Vector3D v = new Vector3D(axisX.X * vector2D.X, axisX.Y * vector2D.X, axisX.Z * vector2D.X);

            return new Vector3D(u.X + v.X, u.Y + v.Y, u.Z + v.Z);
        }

        public static Polygon3D Convert(this Plane plane, Planar.Polygon2D polygon2D)
        {
            if (plane == null || polygon2D == null)
                return null;

            //return new Polygon3D(Convert(polygon2D.Points));
            return new Polygon3D(plane, polygon2D.Points);
        }

        public static Rectangle3D Convert(this Plane plane, Planar.Rectangle2D rectangle2D)
        {
            if (plane == null || rectangle2D == null)
                return null;

            return new Rectangle3D(plane, rectangle2D);
            //return new Polygon3D(plane, rectangle2D.GetPoints());
        }

        public static Planar.Polygon2D Convert(this Plane plane, Polygon3D polygon3D)
        {
            if (plane == null || polygon3D == null)
                return null;

            List<Point3D> point3Ds = polygon3D.GetPoints();
            if (point3Ds == null)
                return null;

            return new Planar.Polygon2D(Convert(plane, point3Ds));
        }

        public static Planar.Polyline2D Convert(this Plane plane, Polyline3D polyline3D)
        {
            if (plane == null || polyline3D == null)
                return null;

            List<Point3D> point3Ds = polyline3D.GetPoints();
            if (point3Ds == null)
                return null;

            return new Planar.Polyline2D(Convert(plane, point3Ds));
        }

        public static Polyline3D Convert(this Plane plane, Planar.Polyline2D polyline2D)
        {
            if (plane == null || polyline2D == null)
                return null;

            List<Planar.Point2D> point2Ds = polyline2D.GetPoints();
            if (point2Ds == null)
                return null;

            return new Polyline3D(Convert(plane, point2Ds));
        }

        public static Planar.Triangle2D Convert(this Plane plane, Triangle3D triangle3D)
        {
            if (plane == null || triangle3D == null)
                return null;

            List<Point3D> point3Ds = triangle3D.GetPoints();
            if (point3Ds == null)
                return null;

            return new Planar.Triangle2D(Convert(plane, point3Ds[0]), Convert(plane, point3Ds[1]), Convert(plane, point3Ds[2]));
        }

        public static Triangle3D Convert(this Plane plane, Planar.Triangle2D triangle2D)
        {
            if (plane == null || triangle2D == null)
                return null;

            List<Planar.Point2D> point2Ds = triangle2D.GetPoints();
            if (point2Ds == null)
                return null;

            return new Triangle3D(Convert(plane, point2Ds[0]), Convert(plane, point2Ds[1]), Convert(plane, point2Ds[2]));
        }

        public static Planar.Segment2D Convert(this Plane plane, Segment3D segment3D)
        {
            if (plane == null || segment3D == null)
                return null;

            return new Planar.Segment2D(Convert(plane, segment3D[0]), Convert(plane, segment3D[1]));
        }

        public static Segment3D Convert(this Plane plane, Planar.Segment2D segment2D)
        {
            if (plane == null || segment2D == null)
                return null;

            return new Segment3D(Convert(plane, segment2D[0]), Convert(plane, segment2D[1]));
        }

        public static Planar.Line2D Convert(this Plane plane, Line3D line3D)
        {
            if (plane == null || line3D == null)
                return null;

            return new Planar.Line2D(Convert(plane, line3D.Origin), Convert(plane, line3D.Direction));
        }

        public static Line3D Convert(this Plane plane, Planar.Line2D line2D)
        {
            if (plane == null || line2D == null)
                return null;

            return new Line3D(Convert(plane, line2D.Origin), Convert(plane, line2D.Direction));
        }

        public static IClosedPlanar3D Convert(this Plane plane, Planar.IClosed2D closed2D)
        {
            if (plane == null || closed2D == null)
                return null;

            return Convert(plane, closed2D as dynamic);
        }

        public static Planar.Face2D Convert(this Plane plane, Face3D face3D)
        {
            if (plane == null)
                return null;

            IClosedPlanar3D closedPlanar3D_External = face3D?.GetExternalEdge3D();
            if (closedPlanar3D_External == null)
                return null;

            Planar.IClosed2D closed2D_external = Convert(plane, closedPlanar3D_External);

            List<Planar.IClosed2D> closed2Ds_internal = new List<Planar.IClosed2D>();
            List<IClosedPlanar3D> closedPlanar3Ds_Internal = face3D.GetInternalEdge3Ds();
            if (closedPlanar3Ds_Internal != null && closedPlanar3Ds_Internal.Count > 0)
                closedPlanar3Ds_Internal.ForEach(x => closed2Ds_internal.Add(Convert(plane, x)));

            return Planar.Face2D.Create(closed2D_external, closed2Ds_internal);
        }

        public static Face3D Convert(this Plane plane, Planar.Face2D face2D)
        {
            if (plane == null || face2D == null)
                return null;

            return new Face3D(plane, face2D);
        }

        public static Planar.Rectangle2D Convert(this Plane plane, Rectangle3D rectangle3D)
        {
            if (plane == null)
            {
                return null;
            }

            Vector3D vector3D_Width = rectangle3D.WidthDirection * rectangle3D.Width;
            Vector3D vector3D_Height = rectangle3D.HeightDirection * rectangle3D.Height;

            Planar.Vector2D vector2D_Height = plane.Convert(vector3D_Height);

            return new Planar.Rectangle2D(plane.Convert(rectangle3D.Origin), plane.Convert(vector3D_Width).Length, vector2D_Height.Length, vector2D_Height.Unit);

        }

        public static Planar.IClosed2D Convert(this Plane plane, IClosed3D closed3D)
        {
            if (plane == null || closed3D == null)
                return null;

            return Convert(plane, closed3D as dynamic);
        }

        public static List<Planar.Point2D> Convert(this Plane plane, IEnumerable<Point3D> point3Ds)
        {
            if (plane == null || point3Ds == null)
                return null;

            List<Planar.Point2D> point2Ds = new List<Planar.Point2D>();
            foreach (Point3D point3D in point3Ds)
                point2Ds.Add(Convert(plane, point3D));

            return point2Ds;
        }

        public static List<Point3D> Convert(this Plane plane, IEnumerable<Planar.Point2D> point2Ds)
        {
            if (plane == null || point2Ds == null)
                return null;

            List<Point3D> point3Ds = new List<Point3D>();
            foreach (Planar.Point2D point2D in point2Ds)
                point3Ds.Add(Convert(plane, point2D));

            return point3Ds;
        }

        public static Point3D Convert(this Sphere sphere, Planar.Point2D point2D)
        {
            if (sphere == null || point2D == null)
            {
                return null;
            }

            return new Point3D(System.Math.Sin(point2D.X) * System.Math.Cos(point2D.Y), System.Math.Sin(point2D.X) * System.Math.Sin(point2D.Y), System.Math.Cos(point2D.X));
        }
    }
}