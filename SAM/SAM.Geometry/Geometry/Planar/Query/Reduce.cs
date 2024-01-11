using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static Polyline2D Reduce(this Polyline2D polyline2D, double minDistance)
        {
            if (polyline2D == null)
                return null;

            List<Point2D> point2Ds = Reduce(polyline2D.GetPoints(), minDistance);
            if(point2Ds == null || point2Ds.Count == 0)
            {
                return new Polyline2D(polyline2D);
            }

            return new Polyline2D(point2Ds);
        }

        public static Polygon2D Reduce(this Polygon2D polygon2D, double minDistance)
        {
            if (polygon2D == null)
                return null;

            List<Point2D> point2Ds = polygon2D.GetPoints();
            if(point2Ds.Count <= 3)
            {
                return new Polygon2D(polygon2D);
            }

            point2Ds = Reduce(point2Ds, minDistance);
            if (point2Ds == null || point2Ds.Count < 3)
            {
                return new Polygon2D(polygon2D);
            }

            return new Polygon2D(point2Ds);
        }

        public static Face2D Reduce(this Face2D face2D, double minDistance)
        {
            if(face2D == null)
            {
                return null;
            }

            IClosed2D externalEdge2D = Reduce(face2D.ExternalEdge2D, minDistance);

            List<IClosed2D> internalEdge2Ds = face2D.InternalEdge2Ds;
            if(internalEdge2Ds != null && internalEdge2Ds.Count != 0)
            {
                internalEdge2Ds = internalEdge2Ds.ConvertAll(x => Reduce(x, minDistance));
            }

            return Create.Face2D(externalEdge2D, internalEdge2Ds, EdgeOrientationMethod.Undefined);
        }

        public static IClosed2D Reduce(IClosed2D closed2D, double minDistance)
        {
            if(closed2D == null)
            {
                return null;
            }

            if(closed2D is Polygon2D)
            {
                return Reduce((Polygon2D)closed2D, minDistance);
            }
            
            if(closed2D is Face2D)
            {
                return Reduce((Face2D)closed2D, minDistance);
            }

            return closed2D.Clone() as IClosed2D;
        }

        private static List<Point2D> Reduce(this IEnumerable<Point2D> point2Ds, double minDistance)
        {
            if (point2Ds == null)
                return null;

            List<Point2D> result = point2Ds.ToList();
            if (result == null)
            {
                return null;
            }

            List<int> indexes = null;
            do
            {
                indexes = new List<int>();

                if (result.Count > 2)
                {
                    for (int i = 1; i < result.Count; i = i + 2)
                    {
                        if (result[i - 1].Distance(result[i]) < minDistance)
                            indexes.Add(i);
                    }

                    if (indexes != null && indexes.Count > 0)
                    {
                        indexes.Reverse();
                        indexes.ForEach(x => result.RemoveAt(x));
                    }
                }

            } while (indexes != null && indexes.Count > 0);

            return result;
        }
    }
}