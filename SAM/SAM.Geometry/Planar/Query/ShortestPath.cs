using QuickGraph;
using QuickGraph.Algorithms.Observers;
using QuickGraph.Algorithms.ShortestPath;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static List<Point2D> ShortestPath(this IEnumerable<ISegmentable2D> segmentable2Ds, Point2D point2D_Start, Point2D point2D_End, double tolerance = Core.Tolerance.Distance)
        {
            if (segmentable2Ds == null)
            {
                return null;
            }

            HashSet<Point2D> point2Ds_Unique = new HashSet<Point2D>();
            List<Segment2D> segment2Ds = new List<Segment2D>();
            foreach (ISegmentable2D segmentable2D in segmentable2Ds)
            {
                List<Segment2D> segment2Ds_Temp = segmentable2D?.GetSegments();
                if (segment2Ds_Temp == null || segment2Ds_Temp.Count == 0)
                {
                    continue;
                }

                for (int i = 0; i < segment2Ds_Temp.Count; i++)
                {
                    segment2Ds_Temp[i].Round(tolerance);
                    segment2Ds.Add(segment2Ds_Temp[i]);
                }

                segmentable2D.GetPoints()?.ForEach(x => point2Ds_Unique.Add(x));
            }

            segment2Ds = segment2Ds?.Split(tolerance);
            if (segment2Ds == null)
            {
                return null;
            }

            Point2D point2D_Start_Temp = point2D_Start;
            if(point2D_Start_Temp != null)
            {
                List<Segment2D> segment2Ds_Connected = segment2Ds.Connect(point2D_Start_Temp, PointConnectMethod.Ends, tolerance);
                if(segment2Ds_Connected != null)
                {
                    Point2D point2D = segment2Ds_Connected.Find(x => x[0].AlmostEquals(point2D_Start_Temp, tolerance))?[0];
                    if(point2D == null)
                        point2D = segment2Ds_Connected.Find(x => x[1].AlmostEquals(point2D_Start_Temp, tolerance))?[1];

                    if (point2D == null)
                        return null;

                    point2D_Start_Temp = point2D;
                }
            }


            Point2D point2D_End_Temp = point2D_End;
            if (point2D_End_Temp != null)
            {
                List<Segment2D> segment2Ds_Connected = segment2Ds.Connect(point2D_End_Temp, PointConnectMethod.Ends, tolerance);
                if (segment2Ds_Connected != null)
                {
                    Point2D point2D = segment2Ds_Connected.Find(x => x[0].AlmostEquals(point2D_End_Temp, tolerance))?[0];
                    if (point2D == null)
                        point2D = segment2Ds_Connected.Find(x => x[1].AlmostEquals(point2D_End_Temp, tolerance))?[1];

                    if (point2D == null)
                        return null;

                    point2D_End_Temp = point2D;
                }
            }

            if (point2D_Start_Temp == null || point2D_End_Temp == null)
                return null;

            AdjacencyGraph<Point2D, Edge<Point2D>> adjacencyGraph = Create.AdjacencyGraph(segment2Ds);

            AStarShortestPathAlgorithm<Point2D, Edge<Point2D>> aStarShortestPathAlgorithm = new AStarShortestPathAlgorithm<Point2D, Edge<Point2D>>(adjacencyGraph, edge => edge.Source.Distance(edge.Target), point2D => point2D.Distance(point2D_Start_Temp));

            VertexPredecessorRecorderObserver<Point2D, Edge<Point2D>> vertexPredecessorRecorderObserver = new VertexPredecessorRecorderObserver<Point2D, Edge<Point2D>>();
            vertexPredecessorRecorderObserver.Attach(aStarShortestPathAlgorithm);

            aStarShortestPathAlgorithm.Compute(point2D_Start_Temp);

            List<Point2D> result = new List<Point2D>();
            if (!vertexPredecessorRecorderObserver.TryGetPath(point2D_End_Temp, out IEnumerable<Edge<Point2D>> edges))
                return result;

            if (edges.Count() == 0)
            {
                return result;
            }

            foreach (Edge<Point2D> edge_Temp in edges)
            {
                Point2D point2D = edge_Temp.Source;

                foreach (Point2D point2D_Unique in point2Ds_Unique)
                {
                    if (point2D_Unique.AlmostEquals(point2D))
                    {
                        point2D = point2D_Unique;
                        break;
                    }
                }

                result.Add(point2D);
            }

            foreach (Point2D point2D_Unique in point2Ds_Unique)
            {
                if (point2D_Unique.AlmostEquals(point2D_End))
                {
                    point2D_End = point2D_Unique;
                    break;
                }
            }

            result.Add(point2D_End);

            return result;
        }
    }
}