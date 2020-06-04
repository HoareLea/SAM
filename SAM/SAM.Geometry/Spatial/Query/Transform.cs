using SAM.Math;
using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static Point3D Transform(this Point3D point3D, Transform3D transform3D)
        {
            return Transform(point3D, transform3D?.Matrix4D);
        }

        public static Point3D Transform(this Point3D point3D, Matrix4D matrix4D)
        {
            if (point3D == null || matrix4D == null)
                return null;

            Matrix matrix = matrix4D * point3D.GetArgumentedMatrix();

            return Create.Point3D(matrix);
        }

        public static List<Point3D> Transform(this IEnumerable<Point3D> point3Ds, Transform3D transform3D)
        {
            return Transform(point3Ds, transform3D?.Matrix4D);
        }

        public static List<Point3D> Transform(this IEnumerable<Point3D> point3Ds, Matrix4D matrix4D)
        {
            if (point3Ds == null || matrix4D == null)
                return null;

            List<Point3D> result = new List<Point3D>();
            foreach (Point3D point3D in point3Ds)
                result.Add(point3D.Transform(matrix4D));

            return result;
        }

        public static Vector3D Transform(this Vector3D vector3D, Transform3D transform3D)
        {
            return Transform(vector3D, transform3D.Matrix4D);
        }

        public static Vector3D Transform(this Vector3D vector3D, Matrix4D matrix4D)
        {
            if (vector3D == null || matrix4D == null)
                return null;

            Matrix matrix = matrix4D * vector3D.GetArgumentedMatrix();

            return Create.Vector3D(matrix);
        }

        public static Polygon3D Transform(this Polygon3D polygon3D, Transform3D transform3D)
        {
            return Transform(polygon3D, transform3D.Matrix4D);
        }

        public static Polyline3D Transform(this Polyline3D polyline3D, Transform3D transform3D)
        {
            return Transform(polyline3D, transform3D.Matrix4D);
        }

        public static Triangle3D Transform(this Triangle3D triangle3D, Transform3D transform3D)
        {
            return Transform(triangle3D, transform3D.Matrix4D);
        }

        public static Plane Transform(this Plane plane, Transform3D transform3D)
        {
            return Transform(plane, transform3D.Matrix4D);
        }

        public static Segment3D Transform(this Segment3D segment3D, Transform3D transform3D)
        {
            return Transform(segment3D, transform3D.Matrix4D);
        }

        public static Face3D Transform(this Face3D face3D, Transform3D transform3D)
        {
            return Transform(face3D, transform3D?.Matrix4D);
        }

        public static Polygon3D Transform(this Polygon3D polygon3D, Matrix4D matrix4D)
        {
            if (matrix4D == null)
                return null;

            List<Point3D> point3Ds = polygon3D?.GetPoints()?.Transform(matrix4D);
            if (point3Ds == null)
                return null;

            return new Polygon3D(point3Ds);
        }

        public static Polyline3D Transform(this Polyline3D polyline3D, Matrix4D matrix4D)
        {
            if (matrix4D == null)
                return null;

            List<Point3D> point3Ds = polyline3D?.GetPoints()?.Transform(matrix4D);
            if (point3Ds == null)
                return null;

            return new Polyline3D(point3Ds);
        }

        public static Triangle3D Transform(this Triangle3D triangle3D, Matrix4D matrix4D)
        {
            if (matrix4D == null)
                return null;

            List<Point3D> point3Ds = triangle3D?.GetPoints()?.Transform(matrix4D);
            if (point3Ds == null)
                return null;

            return new Triangle3D(point3Ds[0], point3Ds[1], point3Ds[2]);
        }

        public static Plane Transform(this Plane plane, Matrix4D matrix4D)
        {
            if (plane == null || matrix4D == null)
                return null;

            Point3D origin = plane.Origin.Transform(matrix4D);
            Vector3D axisX = plane.AxisX.Transform(matrix4D);
            Vector3D axisY = plane.AxisY.Transform(matrix4D);

            return new Plane(origin, axisX, axisY);
        }

        public static Segment3D Transform(this Segment3D segment3D, Matrix4D matrix4D)
        {
            if (segment3D == null || matrix4D == null)
                return null;

            return new Segment3D(segment3D[0].Transform(matrix4D), segment3D[1].Transform(matrix4D));
        }

        public static Face3D Transform(this Face3D face3D, Matrix4D matrix4D)
        {
            if (matrix4D == null)
                return null;

            IClosedPlanar3D externalEdge = face3D?.GetExternalEdge();
            if (externalEdge == null)
                return null;

            externalEdge = Transform(externalEdge as dynamic, matrix4D);
            if (externalEdge == null)
                return null;

            List<IClosedPlanar3D> internalEdges = face3D.GetInternalEdges();
            if (internalEdges != null && internalEdges.Count > 0)
                internalEdges.ConvertAll(x => Transform(x as dynamic, matrix4D));

            Plane plane = externalEdge.GetPlane();

            return Face3D.Create(plane, plane.Convert(externalEdge), internalEdges?.ConvertAll(x => plane.Convert(plane.Project(x))));
        }
    }
}