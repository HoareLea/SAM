using QuickGraph;
using SAM.Geometry.Planar;
using System.Collections.Generic;

namespace SAM.Geometry
{
    public static partial class Create
    {
        public static UndirectedGraph<Point2D, Edge<Point2D>> UndirectedGraph(this IEnumerable<ISegmentable2D> segmentable2Ds)
        {
            if (segmentable2Ds == null)
                return null;

            List<Edge<Point2D>> edges = new List<Edge<Point2D>>();
            foreach (ISegmentable2D segmentable2D in segmentable2Ds)
            {
                List<Segment2D> segment2Ds = segmentable2D?.GetSegments();
                if (segment2Ds != null)
                {
                    foreach (Segment2D segment2D in segment2Ds)
                    {
                        if (segment2D != null)
                        {
                            edges.Add(new Edge<Point2D>(segment2D[0], segment2D[1]));
                        }
                    }
                }
            }

            return edges.ToUndirectedGraph<Point2D, Edge<Point2D>>(); ;
        }
    }
}