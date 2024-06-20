using SAM.Geometry.Spatial;
using SAM.Math;
using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static Point2D Transform(this Point2D point2D, ITransform2D transform2D)
        {
            if(point2D == null || transform2D == null)
            {
                return null;
            }
            
            if(transform2D is Transform2D)
            {
                return Transform(point2D, (Transform2D)transform2D);
            }

            if(transform2D is TransformGroup2D)
            {
                Point2D result = point2D;
                foreach(ITransform2D transform2D_Temp in (TransformGroup2D)transform2D)
                {
                    result = Transform(result, transform2D_Temp);
                }

                return result;
            }

            return null;
        }

        public static Vector2D Transform(this Vector2D vector2D, ITransform2D transform2D)
        {
            if (transform2D is Transform2D)
            {
                return Transform(vector2D, (Transform2D)transform2D);
            }

            if (transform2D is TransformGroup2D)
            {
                Vector2D result = vector2D;
                foreach (ITransform2D transform2D_Temp in (TransformGroup2D)transform2D)
                {
                    result = Transform(result, transform2D_Temp);
                }

                return result;
            }

            return null;
        }

        public static List<Point2D> Transform(this IEnumerable<Point2D> point2Ds, ITransform2D transform2D)
        {
            if(point2Ds == null || transform2D == null)
            {
                return null;
            }

            List<Point2D> result = new List<Point2D>();
            foreach(Point2D point2D in point2Ds)
            {
                result.Add(Transform(point2D, transform2D));
            }

            return result;
        }

        public static Triangle2D Transform(this Triangle2D triangle2D, ITransform2D transform2D)
        {
            if(triangle2D == null || transform2D == null)
            {
                return null;
            }
            
            List<Point2D> point2Ds = Transform(triangle2D.GetPoints(), transform2D);
            if(point2Ds == null || point2Ds.Count < 3)
            {
                return null;
            }

            return new Triangle2D(point2Ds[0], point2Ds[1], point2Ds[2]);
        }

        public static Segment2D Transform(this Segment2D segment2D, ITransform2D transform2D)
        {
            if (segment2D == null || transform2D == null)
            {
                return null;
            }

            List<Point2D> point2Ds = Transform(segment2D.GetPoints(), transform2D);
            if (point2Ds == null || point2Ds.Count < 2)
            {
                return null;
            }

            return new Segment2D(point2Ds[0], point2Ds[1]);
        }

        public static Rectangle2D Transform(this Rectangle2D rectangle2D, ITransform2D transform2D)
        {
            if (transform2D == null || rectangle2D == null)
            {
                return null;
            }

            Vector2D vector3D_Width = rectangle2D.WidthDirection * rectangle2D.Width;
            Vector2D vector3D_Height = rectangle2D.HeightDirection * rectangle2D.Height;

            Vector2D vector2D_Width = vector3D_Width.GetTransformed(transform2D) as Vector2D;
            Vector2D vector2D_Height = vector3D_Height.GetTransformed(transform2D) as Vector2D;

            Point2D origin = rectangle2D.Origin.GetTransformed(transform2D) as Point2D;

            return new Rectangle2D(origin, vector2D_Width.Length, vector2D_Height.Length, vector2D_Height.Unit);
        }

        public static Polyline2D Transform(this Polyline2D polyline2D, ITransform2D transform2D)
        {
            if (polyline2D == null || transform2D == null)
            {
                return null;
            }

            List<Point2D> point2Ds = Transform(polyline2D.GetPoints(), transform2D);
            if (point2Ds == null || point2Ds.Count  < 2)
            {
                return null;
            }

            return new Polyline2D(point2Ds);
        }

        public static BoundingBox2D Transform(this BoundingBox2D boundingBox2D, ITransform2D transform2D)
        {
            if(boundingBox2D == null || transform2D == null)
            {
                return null;
            }

            return new BoundingBox2D(Transform(boundingBox2D.Max, transform2D), Transform(boundingBox2D.Min, transform2D));
        }

        public static Circle2D Transform(this Circle2D circle2D, ITransform2D transform2D)
        {
            if (circle2D == null || transform2D == null)
            {
                return null;
            }

            Vector2D vector2D = new Vector2D(1, 1) * circle2D.Radius;
            vector2D.Transform(transform2D);

            return new Circle2D(circle2D.Center.GetTransformed(transform2D) as Point2D, vector2D.Length);
        }

        public static Polygon2D Transform(this Polygon2D polygon2D, ITransform2D transform2D)
        {
            if (transform2D == null || polygon2D == null)
            {
                return null;
            }

            List<Point2D> point2Ds = polygon2D?.Points?.Transform(transform2D);
            if (point2Ds == null)
            {
                return null;
            }

            return new Polygon2D(point2Ds);
        }

        public static CoordinateSystem2D Transform(this CoordinateSystem2D coordinateSystem2D, ITransform2D transform2D)
        {
            if (coordinateSystem2D == null || transform2D == null)
            {
                return null;
            }

            Point2D origin = coordinateSystem2D.Origin.GetTransformed(transform2D) as Point2D;
            Vector2D axisX = coordinateSystem2D.AxisX.GetTransformed(transform2D) as Vector2D;
            Vector2D axisY = coordinateSystem2D.AxisY.GetTransformed(transform2D) as Vector2D;

            return new CoordinateSystem2D(origin, axisX, axisY);
        }

        public static Face2D Transform(this Face2D face2D, ITransform2D transform2D)
        {
            if (transform2D == null || face2D == null)
            {
                return null;
            }

            IClosed2D externalEdge = face2D?.ExternalEdge2D;
            if (externalEdge == null)
            {
                return null;
            }

            externalEdge = Transform(externalEdge as dynamic, transform2D);
            if (externalEdge == null)
            {
                return null;
            }

            List<IClosed2D> internalEdges = face2D.InternalEdge2Ds;
            if (internalEdges != null && internalEdges.Count > 0)
            {
                internalEdges.ConvertAll(x => Transform(x as dynamic, transform2D));
            }

            return Face2D.Create(externalEdge, internalEdges);
        }



        public static Point2D Transform(this Point2D point2D, Transform2D transform2D)
        {
            return Transform(point2D, transform2D?.Matrix3D);
        }

        public static BoundingBox2D Transform(this BoundingBox2D boundingBox2D, Transform2D transform2D)
        {
            return Transform(boundingBox2D, transform2D?.Matrix3D);
        }

        public static List<Point2D> Transform(this IEnumerable<Point2D> point2Ds, Transform2D transform2D)
        {
            return Transform(point2Ds, transform2D?.Matrix3D);
        }

        public static Vector2D Transform(this Vector2D vector2D, Transform2D transform2D)
        {
            return Transform(vector2D, transform2D.Matrix3D);
        }

        public static Polygon2D Transform(this Polygon2D polygon2D, Transform2D transform2D)
        {
            return Transform(polygon2D, transform2D.Matrix3D);
        }

        public static Polyline2D Transform(this Polyline2D polyline2D, Transform2D transform2D)
        {
            return Transform(polyline2D, transform2D.Matrix3D);
        }

        public static Triangle2D Transform(this Triangle2D triangle2D, Transform2D transform2D)
        {
            return Transform(triangle2D, transform2D.Matrix3D);
        }

        public static Segment2D Transform(this Segment2D segment2D, Transform2D transform2D)
        {
            return Transform(segment2D, transform2D.Matrix3D);
        }

        public static Face2D Transform(this Face2D face2D, Transform2D transform2D)
        {
            return Transform(face2D, transform2D?.Matrix3D);
        }

        public static Rectangle2D Transform(this Rectangle2D rectangle2D, Transform2D transform2D)
        {
            return Transform(rectangle2D, transform2D?.Matrix3D);
        }

        public static Circle2D Transform(this Circle2D circle2D, Transform2D transform2D)
        {
            return Transform(circle2D, transform2D?.Matrix3D);
        }

        public static Circle2D Transform(this Circle2D circle2D, Matrix3D matrix3D)
        {
            if (matrix3D == null)
            {
                return null;
            }

            return new Circle2D(circle2D.Center.Transform(matrix3D), circle2D.Radius);
        }

        public static CoordinateSystem2D Transform(this CoordinateSystem2D coordinateSystem2D, Transform2D transform2D)
        {
            return Transform(coordinateSystem2D, transform2D?.Matrix3D);
        }


        public static Point2D Transform(this Point2D point2D, Matrix3D matrix3D)
        {
            if (point2D == null || matrix3D == null)
                return null;

            Matrix matrix = matrix3D * point2D.GetArgumentedMatrix();

            return Create.Point2D(matrix);
        }

        public static Vector2D Transform(this Vector2D vector2D, Matrix3D matrix3D)
        {
            if (vector2D == null || matrix3D == null)
                return null;

            Matrix matrix = matrix3D * vector2D.GetArgumentedMatrix();

            return Create.Vector2D(matrix);
        }

        public static BoundingBox2D Transform(this BoundingBox2D boundingBox2D, Matrix3D matrix3D)
        {
            if (boundingBox2D == null || matrix3D == null)
                return null;

            return new BoundingBox2D(boundingBox2D.Min.Transform(matrix3D), boundingBox2D.Max.Transform(matrix3D));
        }

        public static List<Point2D> Transform(this IEnumerable<Point2D> point2Ds, Matrix3D matrix3D)
        {
            if (point2Ds == null || matrix3D == null)
                return null;

            List<Point2D> result = new List<Point2D>();
            foreach (Point2D point2D in point2Ds)
                result.Add(point2D.Transform(matrix3D));

            return result;
        }

        public static Face2D Transform(this Face2D face2D, Matrix3D matrix3D)
        {
            if (matrix3D == null)
                return null;

            IClosed2D externalEdge = face2D?.ExternalEdge2D;
            if (externalEdge == null)
                return null;

            externalEdge = Transform(externalEdge as dynamic, matrix3D);
            if (externalEdge == null)
                return null;

            List<IClosed2D> internalEdges = face2D.InternalEdge2Ds;
            if (internalEdges != null && internalEdges.Count > 0)
                internalEdges.ConvertAll(x => Transform(x as dynamic, matrix3D));

            return Face2D.Create(externalEdge, internalEdges);
        }

        public static Segment2D Transform(this Segment2D segment2D, Matrix3D matrix3D)
        {
            if (segment2D == null || matrix3D == null)
                return null;

            return new Segment2D(segment2D[0].Transform(matrix3D), segment2D[1].Transform(matrix3D));
        }

        public static Rectangle2D Transform(this Rectangle2D rectangle2D, Matrix3D matrix3D)
        {
            if (matrix3D == null || rectangle2D == null)
            {
                return null;
            }

            Vector2D vector3D_Width = rectangle2D.WidthDirection * rectangle2D.Width;
            Vector2D vector3D_Height = rectangle2D.HeightDirection * rectangle2D.Height;

            Vector2D vector2D_Width = vector3D_Width.Transform(matrix3D);
            Vector2D vector2D_Height = vector3D_Height.Transform(matrix3D);

            Point2D origin = rectangle2D.Origin.Transform(matrix3D);

            return new Rectangle2D(origin, vector2D_Width.Length, vector2D_Height.Length, vector2D_Height.Unit);
        }

        public static Triangle2D Transform(this Triangle2D triangle2D, Matrix3D matrix3D)
        {
            if (matrix3D == null)
            {
                return null;
            }

            List<Point2D> point2Ds = triangle2D?.GetPoints()?.Transform(matrix3D);
            if (point2Ds == null)
            {
                return null;
            }

            return new Triangle2D(point2Ds[0], point2Ds[1], point2Ds[2]);
        }

        public static Polygon2D Transform(this Polygon2D polygon2D, Matrix3D matrix3D)
        {
            if (matrix3D == null)
                return null;

            List<Point2D> point2Ds = polygon2D?.Points?.Transform(matrix3D);
            if (point2Ds == null)
                return null;

            return new Polygon2D(point2Ds);
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

        public static Polyline2D Transform(this Polyline2D polyline3D, Matrix3D matrix4D)
        {
            if (matrix4D == null)
                return null;

            List<Point2D> point2Ds = polyline3D?.GetPoints()?.Transform(matrix4D);
            if (point2Ds == null)
                return null;

            return new Polyline2D(point2Ds);
        }

        public static CoordinateSystem2D Transform(this CoordinateSystem2D coordinateSystem2D, Matrix3D matrix4D)
        {
            if (coordinateSystem2D == null || matrix4D == null)
            {
                return null;
            }

            Point2D origin = coordinateSystem2D.Origin.Transform(matrix4D);
            Vector2D axisX = coordinateSystem2D.AxisX.Transform(matrix4D);
            Vector2D axisY = coordinateSystem2D.AxisY.Transform(matrix4D);

            return new CoordinateSystem2D(origin, axisX, axisY);
        }
    }
}