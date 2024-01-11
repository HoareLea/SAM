using QuickGraph;
using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Create
    {
        public static AdjacencyGraph<Point2D, Edge<Point2D>> AdjacencyGraph(this IEnumerable<ISegmentable2D> segmentable2Ds, double tolerance = Core.Tolerance.Distance)
        {
            if (segmentable2Ds == null)
                return null;

            AdjacencyGraph<Point2D, Edge<Point2D>> result = new AdjacencyGraph<Point2D, Edge<Point2D>>();

            HashSet<Point2D> point2Ds = new HashSet<Point2D>();
            foreach(ISegmentable2D segmentable2D in segmentable2Ds)
            {
                List<Segment2D> segment2Ds = segmentable2D?.GetSegments();
                if (segment2Ds == null)
                    continue;

                foreach(Segment2D segment2D in segment2Ds)
                {
                    Point2D point2D_1 = segment2D[0];
                    point2D_1.Round(tolerance);
                    if(!point2Ds.Contains(point2D_1))
                    {
                        point2Ds.Add(point2D_1);
                        result.AddVertex(point2D_1);
                    }

                    Point2D point2D_2 = segment2D[1];
                    point2D_2.Round(tolerance);
                    if (!point2Ds.Contains(point2D_2))
                    {
                        point2Ds.Add(point2D_2);
                        result.AddVertex(point2D_2);
                    }

                    result.AddEdge(new Edge<Point2D>(point2D_1, point2D_2));
                    result.AddEdge(new Edge<Point2D>(point2D_2, point2D_1));
                }
            }

            return result;
        }
    }
}