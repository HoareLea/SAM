// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

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

        public static BoundingBox3D Transform(this BoundingBox3D boundingBox3D, Transform3D transform3D)
        {
            return Transform(boundingBox3D, transform3D?.Matrix4D);
        }

        public static List<Point3D> Transform(this IEnumerable<Point3D> point3Ds, Transform3D transform3D)
        {
            return Transform(point3Ds, transform3D?.Matrix4D);
        }

        public static Vector3D Transform(this Vector3D vector3D, Transform3D transform3D)
        {
            return Transform(vector3D, transform3D.Matrix4D);
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

        public static Extrusion Transform(this Extrusion extrusion, Transform3D transform3D)
        {
            return Transform(extrusion, transform3D?.Matrix4D);
        }

        public static Shell Transform(this Shell shell, Transform3D transform3D)
        {
            return Transform(shell, transform3D?.Matrix4D);
        }

        public static Rectangle3D Transform(this Rectangle3D rectangle3D, Transform3D transform3D)
        {
            return Transform(rectangle3D, transform3D?.Matrix4D);
        }

        public static Circle3D Transform(this Circle3D circle3D, Transform3D transform3D)
        {
            return Transform(circle3D, transform3D?.Matrix4D);
        }

        public static Sphere Transform(this Sphere sphere, Transform3D transform3D)
        {
            return Transform(sphere, transform3D?.Matrix4D);
        }

        public static CoordinateSystem3D Transform(this CoordinateSystem3D coordinateSystem3D, Transform3D transform3D)
        {
            return Transform(coordinateSystem3D, transform3D?.Matrix4D);
        }

        public static Mesh3D Transform(this Mesh3D mesh3D, Transform3D transform3D)
        {
            return Transform(mesh3D, transform3D?.Matrix4D);
        }

        public static Circle3D Transform(this Circle3D circle3D, Matrix4D matrix4D)
        {
            if (matrix4D == null)
            {
                return null;
            }

            Plane plane = circle3D.GetPlane();
            if (plane == null)
            {
                return null;
            }

            return new Circle3D(plane.Transform(matrix4D), circle3D.Radius);
        }

        public static Sphere Transform(this Sphere sphere, Matrix4D matrix4D)
        {
            if (matrix4D == null)
            {
                return null;
            }

            Point3D point3D = sphere.Origin;
            if (point3D == null)
            {
                return null;
            }

            return new Sphere(point3D.Transform(matrix4D), sphere.Radius);
        }

        public static Ellipse3D Transform(this Ellipse3D ellipse3D, Transform3D transform3D)
        {
            return Transform(ellipse3D, transform3D?.Matrix4D);
        }

        public static Ellipse3D Transform(this Ellipse3D ellipse3D, Matrix4D matrix4D)
        {
            if (matrix4D == null)
            {
                return null;
            }

            return new Ellipse3D(ellipse3D.GetPlane().Transform(matrix4D), ellipse3D.Ellipse2D);
        }

        public static Extrusion Transform(this Extrusion extrusion, Matrix4D matrix4D)
        {
            if (matrix4D == null)
            {
                return null;
            }

            Face3D face3D = extrusion?.Face3D;
            if (face3D == null)
            {
                return null;
            }

            Vector3D vector3D = extrusion.Vector;
            if (vector3D == null)
            {
                return null;
            }


            return new Extrusion(Transform(face3D, matrix4D), Transform(vector3D, matrix4D));
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

        public static Rectangle3D Transform(this Rectangle3D rectangle3D, Matrix4D matrix4D)
        {
            if (matrix4D == null || rectangle3D == null)
                return null;

            Plane plane = rectangle3D.GetPlane();

            plane = plane.Transform(matrix4D);

            Vector3D vector3D_Width = rectangle3D.WidthDirection * rectangle3D.Width;
            Vector3D vector3D_Height = rectangle3D.HeightDirection * rectangle3D.Height;

            Planar.Vector2D vector2D_Width = plane.Convert(vector3D_Width.Transform(matrix4D));
            Planar.Vector2D vector2D_Height = plane.Convert(vector3D_Height.Transform(matrix4D));

            Planar.Point2D origin = plane.Convert(rectangle3D.Origin.Transform(matrix4D));

            return new Rectangle3D(plane, new Planar.Rectangle2D(origin, vector2D_Width.Length, vector2D_Height.Length, vector2D_Height.Unit));
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

            IClosedPlanar3D externalEdge = face3D?.GetExternalEdge3D();
            if (externalEdge == null)
                return null;

            externalEdge = Transform(externalEdge as dynamic, matrix4D);
            if (externalEdge == null)
                return null;

            List<IClosedPlanar3D> internalEdges = face3D.GetInternalEdge3Ds();
            if (internalEdges != null && internalEdges.Count > 0)
                internalEdges.ConvertAll(x => Transform(x as dynamic, matrix4D));

            Plane plane = externalEdge.GetPlane();

            return Face3D.Create(plane, plane.Convert(externalEdge), internalEdges?.ConvertAll(x => plane.Convert(plane.Project(x))));
        }

        public static Shell Transform(this Shell shell, Matrix4D matrix4D)
        {
            if (matrix4D == null)
                return null;

            List<Face3D> face3Ds = shell?.Face3Ds;
            if (face3Ds == null)
                return null;

            face3Ds = face3Ds.ConvertAll(x => x.Transform(matrix4D));

            return new Shell(face3Ds);
        }

        public static Vector3D Transform(this Vector3D vector3D, Matrix4D matrix4D)
        {
            if (vector3D == null || matrix4D == null)
                return null;

            Matrix matrix = matrix4D * vector3D.GetArgumentedMatrix();

            return Create.Vector3D(matrix);
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

        public static BoundingBox3D Transform(this BoundingBox3D boundingBox3D, Matrix4D matrix4D)
        {
            if (boundingBox3D == null || matrix4D == null)
            {
                return null;
            }

            double minX = boundingBox3D.MinX;
            double minY = boundingBox3D.MinY;
            double minZ = boundingBox3D.MinZ;
            double maxX = boundingBox3D.MaxX;
            double maxY = boundingBox3D.MaxY;
            double maxZ = boundingBox3D.MaxZ;

            if (double.IsNaN(minX) || double.IsNaN(maxX) || double.IsNaN(minY) || double.IsNaN(maxY) || double.IsNaN(minZ) || double.IsNaN(maxZ))
            {
                return null;
            }

            double newMinX = matrix4D[0, 3];
            double newMaxX = matrix4D[0, 3];
            double newMinY = matrix4D[1, 3];
            double newMaxY = matrix4D[1, 3];
            double newMinZ = matrix4D[2, 3];
            double newMaxZ = matrix4D[2, 3];

            // X row
            double a0 = matrix4D[0, 0] * minX;
            double b0 = matrix4D[0, 0] * maxX;
            newMinX += System.Math.Min(a0, b0);
            newMaxX += System.Math.Max(a0, b0);

            double a1 = matrix4D[0, 1] * minY;
            double b1 = matrix4D[0, 1] * maxY;
            newMinX += System.Math.Min(a1, b1);
            newMaxX += System.Math.Max(a1, b1);

            double a2 = matrix4D[0, 2] * minZ;
            double b2 = matrix4D[0, 2] * maxZ;
            newMinX += System.Math.Min(a2, b2);
            newMaxX += System.Math.Max(a2, b2);

            // Y row
            a0 = matrix4D[1, 0] * minX;
            b0 = matrix4D[1, 0] * maxX;
            newMinY += System.Math.Min(a0, b0);
            newMaxY += System.Math.Max(a0, b0);

            a1 = matrix4D[1, 1] * minY;
            b1 = matrix4D[1, 1] * maxY;
            newMinY += System.Math.Min(a1, b1);
            newMaxY += System.Math.Max(a1, b1);

            a2 = matrix4D[1, 2] * minZ;
            b2 = matrix4D[1, 2] * maxZ;
            newMinY += System.Math.Min(a2, b2);
            newMaxY += System.Math.Max(a2, b2);

            // Z row
            a0 = matrix4D[2, 0] * minX;
            b0 = matrix4D[2, 0] * maxX;
            newMinZ += System.Math.Min(a0, b0);
            newMaxZ += System.Math.Max(a0, b0);

            a1 = matrix4D[2, 1] * minY;
            b1 = matrix4D[2, 1] * maxY;
            newMinZ += System.Math.Min(a1, b1);
            newMaxZ += System.Math.Max(a1, b1);

            a2 = matrix4D[2, 2] * minZ;
            b2 = matrix4D[2, 2] * maxZ;
            newMinZ += System.Math.Min(a2, b2);
            newMaxZ += System.Math.Max(a2, b2);

            return new BoundingBox3D(new Point3D(newMinX, newMinY, newMinZ), new Point3D(newMaxX, newMaxY, newMaxZ));
        }

        public static Point3D Transform(this Point3D point3D, Matrix4D matrix4D)
        {
            if (point3D == null || matrix4D == null)
                return null;

            Matrix matrix = matrix4D * point3D.GetArgumentedMatrix();

            return Create.Point3D(matrix);
        }

        public static CoordinateSystem3D Transform(this CoordinateSystem3D coordinateSystem3D, Matrix4D matrix4D)
        {
            if (coordinateSystem3D == null || matrix4D == null)
            {
                return null;
            }

            Point3D origin = coordinateSystem3D.Origin.Transform(matrix4D);
            Vector3D axisX = coordinateSystem3D.AxisX.Transform(matrix4D);
            Vector3D axisY = coordinateSystem3D.AxisY.Transform(matrix4D);
            Vector3D axisZ = coordinateSystem3D.AxisZ.Transform(matrix4D);

            return new CoordinateSystem3D(origin, axisX, axisY, axisZ);
        }

        public static Mesh3D Transform(this Mesh3D mesh3D, Matrix4D matrix4D)
        {
            if (mesh3D == null || matrix4D == null)
                return null;

            List<Point3D> points = mesh3D.GetPoints()?.ConvertAll(x => x.Transform(matrix4D));

            List<System.Tuple<int, int, int>> indexes = new List<System.Tuple<int, int, int>>();
            for (int i = 0; i < mesh3D.TrianglesCount; i++)
                indexes.Add(mesh3D.GetTriangleIndexes(i));

            return new Mesh3D(points, indexes);
        }
    }
}
