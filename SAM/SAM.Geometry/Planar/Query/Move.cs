using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static Point2D Move(this Point2D point2D, Vector2D vector2D)
        {
            if(point2D == null || vector2D == null)
            {
                return null;
            }
            
            Point2D result = new Point2D(point2D);
            result.Move(vector2D);
            return result;
        }

        public static Vector2D Move(this Vector2D baseVector2D, Vector2D vector2D)
        {
            if(baseVector2D == null || vector2D == null)
            {
                return null;
            }

            return new Vector2D(baseVector2D[0] + vector2D[0], baseVector2D[1] + vector2D[1]);
        }

        public static Polygon2D Move(this Polygon2D polygon2D, Vector2D vector2D)
        {
            if(vector2D == null || polygon2D == null)
            {
                return null;
            }

            Polygon2D result = new Polygon2D(polygon2D);
            result.Move(vector2D);

            return result;
        }

        public static BoundingBox2D Move(this BoundingBox2D boundingBox2D, Vector2D vector2D)
        {
            if (vector2D == null || boundingBox2D == null)
            {
                return null;
            }

            return new BoundingBox2D(boundingBox2D.Min.GetMoved(vector2D), boundingBox2D.Max.GetMoved(vector2D));
        }

        public static Circle2D Move(this Circle2D circle2D, Vector2D vector2D)
        {
            if (vector2D == null || circle2D == null)
            {
                return null;
            }

            Circle2D result = new Circle2D(circle2D);
            result.Move(vector2D);

            return result;
        }

        public static Ellipse2D Move(this Ellipse2D ellipse2D, Vector2D vector2D)
        {
            if (vector2D == null || ellipse2D == null)
            {
                return null;
            }

            Ellipse2D result = new Ellipse2D(ellipse2D);
            result.Move(vector2D);

            return result;
        }

        public static Face2D Move(this Face2D face2D, Vector2D vector2D)
        {
            if (vector2D == null || face2D == null)
            {
                return null;
            }

            Face2D result = new Face2D(face2D);
            result.Move(vector2D);

            return result;
        }

        public static Line2D Move(this Line2D line2D, Vector2D vector2D)
        {
            if (vector2D == null || line2D == null)
            {
                return null;
            }

            return new Line2D(line2D.Origin.GetMoved(vector2D), line2D.Direction);
        }

        public static Polycurve2D Move(this Polycurve2D polycurve2D, Vector2D vector2D)
        {
            if (vector2D == null || polycurve2D == null)
            {
                return null;
            }

            return new Polycurve2D(polycurve2D.GetCurves().ConvertAll(x => x.Move(vector2D)));
        }

        public static PolycurveLoop2D Move(this PolycurveLoop2D polycurveLoop2D, Vector2D vector2D)
        {
            if (vector2D == null || polycurveLoop2D == null)
            {
                return null;
            }

            return new PolycurveLoop2D(polycurveLoop2D.GetCurves().ConvertAll(x => x.Move(vector2D)));
        }

        public static Polyline2D Move(this Polyline2D polyline2D, Vector2D vector2D)
        {
            if (vector2D == null || polyline2D == null)
            {
                return null;
            }

            return new Polyline2D(polyline2D.Points.ConvertAll(x => x.GetMoved(vector2D)), polyline2D.IsClosed());
        }

        public static Rectangle2D Move(this Rectangle2D rectangle2D, Vector2D vector2D)
        {
            if (vector2D == null || rectangle2D == null)
            {
                return null;
            }

            Rectangle2D result = new Rectangle2D(rectangle2D);
            result.Move(vector2D);

            return result;
        }

        public static Segment2D Move(this Segment2D segment2D, Vector2D vector2D)
        {
            if (vector2D == null || segment2D == null)
            {
                return null;
            }

            return new Segment2D(segment2D[0].GetMoved(vector2D), segment2D[1].GetMoved(vector2D));
        }

        public static Triangle2D Move(this Triangle2D triangle2D, Vector2D vector2D)
        {
            if (vector2D == null || triangle2D == null)
            {
                return null;
            }

            List<Point2D> point2Ds = triangle2D.GetPoints();

            return new Triangle2D(point2Ds[0].GetMoved(vector2D), point2Ds[1].GetMoved(vector2D), point2Ds[2].GetMoved(vector2D));
        }

        public static T Move<T>(this T sAMGeometry2D, Vector2D vector2D) where T: ISAMGeometry2D
        {
            if (vector2D == null || sAMGeometry2D == null)
            {
                return default;
            }

            return Query.Move(sAMGeometry2D as dynamic, vector2D);
        }
    }
}