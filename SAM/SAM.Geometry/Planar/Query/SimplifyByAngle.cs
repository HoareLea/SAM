using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static List<Point2D> SimplifyByAngle(this IEnumerable<Point2D> point2Ds, bool closed, double tolerane = Core.Tolerance.Angle)
        {
            if (point2Ds == null)
                return null;

            List<Point2D> result = new List<Point2D>(point2Ds);

            int start = 0;
            int end = closed ? result.Count : result.Count - 2;
            while (start < end)
            {
                Point2D first = result[start];
                Point2D second = result[(start + 1) % result.Count];
                Point2D third = result[(start + 2) % result.Count];

                if (second.SmallestAngle(first, third) <= tolerane)
                {
                    result.RemoveAt((start + 1) % result.Count);
                    end--;
                }
                else
                {
                    start++;
                }
            }

            return result;
        }

        public static Polygon2D SimplifyByAngle(this Polygon2D polygon2D, double tolerane = Core.Tolerance.Angle)
        {
            if (polygon2D == null)
                return null;

            List<Point2D> point2Ds = polygon2D.Points;
            if (point2Ds == null || point2Ds.Count < 3)
                return new Polygon2D(polygon2D);

            point2Ds = SimplifyByAngle(point2Ds, true, tolerane);
            if (point2Ds == null || point2Ds.Count < 3)
                return new Polygon2D(polygon2D);

            return new Polygon2D(point2Ds);
        }

        public static Polyline2D SimplifyByAngle(this Polyline2D polyline2D, double tolerane = Core.Tolerance.Angle)
        {
            if (polyline2D == null)
                return null;

            List<Point2D> point2Ds = polyline2D.Points;
            if (point2Ds == null || point2Ds.Count < 3)
                return new Polyline2D(polyline2D);

            point2Ds = SimplifyByAngle(point2Ds, true, tolerane);
            if (point2Ds == null || point2Ds.Count < 3)
                return new Polyline2D(polyline2D);

            return new Polyline2D(point2Ds, polyline2D.IsClosed());
        }

        public static Face2D SimplifyByAngle(this Face2D face2D, double tolerane_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (face2D == null)
            {
                return null;
            }

            IClosed2D externalEdge = face2D.ExternalEdge2D;
            if(externalEdge is Polygon2D)
            {
                externalEdge = SimplifyByAngle((Polygon2D)externalEdge, tolerane_Angle);
            }

            List<IClosed2D> internalEdge2Ds = face2D.InternalEdge2Ds;
            if (internalEdge2Ds != null && internalEdge2Ds.Count!= 0)
            {
                for(int i=0; i < internalEdge2Ds.Count; i++)
                {
                    if(internalEdge2Ds[i] is Polygon2D)
                    {
                        internalEdge2Ds[i] = SimplifyByAngle((Polygon2D)internalEdge2Ds[i], tolerane_Angle);
                    }
                }
            }

            return Face2D.Create(externalEdge, internalEdge2Ds, EdgeOrientationMethod.Undefined, tolerance_Distance);
        }
    }
}