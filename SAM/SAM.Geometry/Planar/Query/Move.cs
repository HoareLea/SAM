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

            BoundingBox2D result = new BoundingBox2D(boundingBox2D);
            result.Move(vector2D);

            return result;
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

            Line2D result = new Line2D(line2D);
            result.Move(vector2D);

            return result;
        }

        public static Polycurve2D Move(this Polycurve2D polycurve2D, Vector2D vector2D)
        {
            if (vector2D == null || polycurve2D == null)
            {
                return null;
            }

            Polycurve2D result = new Polycurve2D(polycurve2D);
            result.Move(vector2D);

            return result;
        }

        public static PolycurveLoop2D Move(this PolycurveLoop2D polycurveLoop2D, Vector2D vector2D)
        {
            if (vector2D == null || polycurveLoop2D == null)
            {
                return null;
            }

            PolycurveLoop2D result = new PolycurveLoop2D(polycurveLoop2D);
            result.Move(vector2D);

            return result;
        }

        public static Polyline2D Move(this Polyline2D polyline2D, Vector2D vector2D)
        {
            if (vector2D == null || polyline2D == null)
            {
                return null;
            }

            Polyline2D result = new Polyline2D(polyline2D);
            result.Move(vector2D);

            return result;
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

            Segment2D result = new Segment2D(segment2D);
            result.Move(vector2D);

            return result;
        }

        public static Triangle2D Move(this Triangle2D triangle2D, Vector2D vector2D)
        {
            if (vector2D == null || triangle2D == null)
            {
                return null;
            }

            Triangle2D result = new Triangle2D(triangle2D);
            result.Move(vector2D);

            return result;
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