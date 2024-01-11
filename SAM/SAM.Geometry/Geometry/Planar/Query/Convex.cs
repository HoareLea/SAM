using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static bool Convex(this IEnumerable<Point2D> point2Ds)
        {
            if (point2Ds == null || point2Ds.Count() < 3)
            {
                return false;
            }

            return !Concave(point2Ds);
        }

        public static bool Convex<T>(this T segmentable2D) where T : ISegmentable2D, IClosed2D
        {
            if (segmentable2D == null)
            {
                return false;
            }

            return Convex(segmentable2D.GetPoints());
        }

        public static bool Convex(this Face2D face2D, bool externalEdge = true, bool internalEdges = true)
        {
            if(face2D == null)
            {
                return false;
            }

            if(externalEdge)
            {
                IClosed2D closed2D = face2D.ExternalEdge2D;
                if(closed2D == null)
                {
                    return false;
                }

                ISegmentable2D segmentable2D = closed2D as ISegmentable2D;
                if(segmentable2D == null)
                {
                    throw new System.NotImplementedException();
                }

                if(!Convex(segmentable2D.GetPoints()))
                {
                    return false;
                }
            }

            if (internalEdges)
            {
                List<IClosed2D> closed2Ds = face2D.InternalEdge2Ds;
                if (closed2Ds == null || closed2Ds.Count == 0)
                {
                    return true;
                }

                foreach(IClosed2D closed2D in closed2Ds)
                {
                    ISegmentable2D segmentable2D = closed2D as ISegmentable2D;
                    if (segmentable2D == null)
                    {
                        throw new System.NotImplementedException();
                    }

                    if (!Convex(segmentable2D.GetPoints()))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}