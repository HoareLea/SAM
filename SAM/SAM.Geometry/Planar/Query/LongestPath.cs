using QuickGraph;
using QuickGraph.Algorithms.Observers;
using QuickGraph.Algorithms.ShortestPath;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        /// <summary>
        /// Returns longest path in given segmentable2Ds with exclusion of loops. If loops exists method will find shortest path through the loop
        /// </summary>
        /// <param name="segmentable2Ds">Segmentable2Ds</param>
        /// <param name="tolerance">Tolerance</param>
        /// <returns>List of Point2Ds representing the longest path</returns>
        public static List<Point2D> LongestPath(this IEnumerable<ISegmentable2D> segmentable2Ds, double tolerance = Core.Tolerance.Distance)
        {
            List<Segment2D> segment2Ds = segmentable2Ds?.Split(tolerance);
            if (segment2Ds == null)
            {
                return null;
            }

            AdjacencyGraph<Point2D, Edge<Point2D>> adjacencyGraph = Geometry.Create.AdjacencyGraph(segment2Ds);

            IEnumerable<Point2D> point2Ds_All = adjacencyGraph.Vertices;
            if(point2Ds_All == null || point2Ds_All.Count() == 0)
            {
                return null;
            }

            IEnumerable<Edge<Point2D>> edges;

            List<Point2D> point2Ds = new List<Point2D>();
            foreach(Point2D point2D in point2Ds_All)
            {
                if(!adjacencyGraph.TryGetOutEdges(point2D, out edges) || edges == null || edges.Count() != 1)
                {
                    continue;
                }

                point2Ds.Add(point2D);
            }

            if(point2Ds.Count == 0)
            {
                point2Ds.Add(point2Ds_All.ElementAt(0));
            }

            double distance = double.MinValue;
            Point2D point2D_Start = null;
            Point2D point2D_End = null;

            foreach(Point2D point2D in point2Ds)
            {
                AStarShortestPathAlgorithm<Point2D, Edge<Point2D>> aStarShortestPathAlgorithm_Temp = new AStarShortestPathAlgorithm<Point2D, Edge<Point2D>>(adjacencyGraph, edge => edge.Source.Distance(edge.Target), point2D_Temp => point2D.Distance(point2D_Temp));

                VertexDistanceRecorderObserver<Point2D, Edge<Point2D>> vertexDistanceRecorderObserver = new VertexDistanceRecorderObserver<Point2D, Edge<Point2D>>(edge => edge.Source.Distance(edge.Target));
                vertexDistanceRecorderObserver.Attach(aStarShortestPathAlgorithm_Temp);

                aStarShortestPathAlgorithm_Temp.Compute(point2D);

                List<string> values = new List<string>();
                foreach (KeyValuePair<Point2D, double> keyValuePair in vertexDistanceRecorderObserver.Distances)
                {
                    if(keyValuePair.Value > distance)
                    {
                        distance = keyValuePair.Value;
                        point2D_Start = point2D;
                        point2D_End = keyValuePair.Key;
                    }
                }
            }

            if(point2D_Start == null || point2D_End == null)
            {
                return null;
            }

            AStarShortestPathAlgorithm<Point2D, Edge<Point2D>> aStarShortestPathAlgorithm = new AStarShortestPathAlgorithm<Point2D, Edge<Point2D>>(adjacencyGraph, edge => edge.Source.Distance(edge.Target), point2D_Temp => point2D_Start.Distance(point2D_Temp));

            VertexPredecessorRecorderObserver<Point2D, Edge<Point2D>> vertexPredecessorRecorderObserver = new VertexPredecessorRecorderObserver<Point2D, Edge<Point2D>>();
            vertexPredecessorRecorderObserver.Attach(aStarShortestPathAlgorithm);

            aStarShortestPathAlgorithm.Compute(point2D_Start);

            if (!vertexPredecessorRecorderObserver.TryGetPath(point2D_End, out edges) || edges == null)
            {
                return null;
            }

            List<Point2D> result = new List<Point2D>();
            if(edges.Count() == 0)
            {
                return result;
            }

            foreach(Edge<Point2D> edge_Temp in edges)
            {
                result.Add(edge_Temp.Source);
            }

            result.Add(point2D_End);

            return result;
        }
    }
}