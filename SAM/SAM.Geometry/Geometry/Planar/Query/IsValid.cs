using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static bool IsValid(this Segment2D segment2D)
        {
            if (segment2D == null)
                return false;

            if (!IsValid(segment2D[0]))
                return false;

            if (!IsValid(segment2D[1]))
                return false;

            return true;
        }

        public static bool IsValid(this Point2D point2D)
        {
            if (point2D == null)
                return false;

            if (point2D.IsNaN())
                return false;

            return true;
        }

        public static bool IsValid(this Vector2D vector2D)
        {
            if (vector2D == null)
                return false;

            if (vector2D.IsNaN())
                return false;

            return true;
        }

        public static bool IsValid(this IClosed2D closed2D)
        {
            if(closed2D == null)
            {
                return false;
            }

            return IsValid(closed2D as dynamic);
        }

        public static bool IsValid(this Polygon2D polygon2D)
        {
            if(polygon2D == null)
            {
                return false;
            }

            List<Point2D> point2Ds = polygon2D.GetPoints();
            if(point2Ds == null || point2Ds.Count < 3)
            {
                return false;
            }

            foreach(Point2D point2D in point2Ds)
            {
                if(!IsValid( point2D))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsValid(this Rectangle2D rectangle2D)
        {
            if(rectangle2D == null)
            {
                return false;
            }

            if(!IsValid(rectangle2D.Origin))
            {
                return false;
            }

            if(double.IsNaN(rectangle2D.Width))
            {
                return false;
            }

            if (double.IsNaN(rectangle2D.Height))
            {
                return false;
            }

            if (!IsValid(rectangle2D.HeightDirection))
            {
                return false;
            }

            return true;
        }

        public static bool IsValid(this BoundingBox2D boundingBox2D)
        {
            if (boundingBox2D == null)
            {
                return false;
            }

            if (!IsValid(boundingBox2D.Min))
            {
                return false;
            }

            if (!IsValid(boundingBox2D.Max))
            {
                return false;
            }

            return true;
        }

        public static bool IsValid(this Circle2D circle2D)
        {
            if (circle2D == null)
            {
                return false;
            }

            if (!IsValid(circle2D.Center))
            {
                return false;
            }

            if (double.IsNaN(circle2D.Radius))
            {
                return false;
            }

            return true;
        }

        public static bool IsValid(this Ellipse2D ellipse2D)
        {
            if (ellipse2D == null)
            {
                return false;
            }

            if (!IsValid(ellipse2D.Center))
            {
                return false;
            }

            if (double.IsNaN(ellipse2D.Width))
            {
                return false;
            }

            if (double.IsNaN(ellipse2D.Height))
            {
                return false;
            }

            if (!IsValid(ellipse2D.HeightDirection))
            {
                return false;
            }

            return true;
        }

        public static bool IsValid(this Face2D face2D)
        {
            if(face2D == null)
            {
                return false;
            }

            if(!IsValid(face2D.ExternalEdge2D))
            {
                return false;
            }

            List<IClosed2D> closed2Ds = face2D.InternalEdge2Ds;
            if(closed2Ds != null && closed2Ds.Count != 0)
            {
                foreach(IClosed2D closed2D in closed2Ds)
                {
                    if(!IsValid(closed2D))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static bool IsValid(this Line2D line2D)
        {
            if(line2D  == null)
            {
                return false;
            }

            if(!IsValid( line2D.Origin))
            {
                return false;
            }

            if(!IsValid(line2D.Direction))
            {
                return false;
            }

            return true;
        }

        public static bool IsValid(this ICurve2D curve2D)
        {
            if(curve2D == null)
            {
                return false;
            }

            return IsValid(curve2D as dynamic);
        }

        public static bool IsValid(this Polycurve2D polycurve2D)
        {
            if(polycurve2D == null)
            {
                return false;
            }

            List<ICurve2D> curve2Ds = polycurve2D.GetCurves();
            if(curve2Ds == null || curve2Ds.Count == 0)
            {
                return false;
            }

            foreach(ICurve2D curve2D in curve2Ds)
            {
                if(!IsValid(curve2D))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsValid(this PolycurveLoop2D polycurveLoop2D)
        {
            if(polycurveLoop2D == null)
            {
                return false;
            }

            List<ICurve2D> curve2Ds = polycurveLoop2D.GetCurves();
            if(curve2Ds == null || curve2Ds.Count == 0)
            {
                return false;
            }

            return curve2Ds.TrueForAll(x => x.IsValid());
        }

        public static bool IsValid(this Polyline2D polyline2D)
        {
            if (polyline2D == null)
            {
                return false;
            }

            List<Segment2D> segment2D = polyline2D.GetSegments();
            if (segment2D == null || segment2D.Count == 0)
            {
                return false;
            }

            return segment2D.TrueForAll(x => x.IsValid());
        }

        public static bool IsValid(this Triangle2D triangle2D)
        {
            List<Point2D> point2Ds = triangle2D?.GetPoints();
            if(point2Ds == null || point2Ds.Count != 3)
            {
                return false;
            }

            return point2Ds.TrueForAll(x => IsValid(x));
        }
    }
}