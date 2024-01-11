using QuickGraph;
using QuickGraph.Algorithms.Observers;
using QuickGraph.Algorithms.ShortestPath;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Create
    {
        public static List<Polyline2D> Polyline2Ds(this IEnumerable<ISegmentable2D> segmentable2Ds, Point2D point2D_Start = null, bool split = true, double tolerance = Core.Tolerance.Distance)
        {
            if (segmentable2Ds == null || segmentable2Ds.Count() == 0)
            {
                return null;
            }

            if (segmentable2Ds.Count() == 1)
            {
                ISegmentable2D segmentable2D = segmentable2Ds.ElementAt(0);
                return new List<Polyline2D>() { new Polyline2D(segmentable2D.GetPoints(), segmentable2D is IClosed2D) };
            }

            List<Segment2D> segment2Ds = null;
            if(split)
            {
                segment2Ds = segmentable2Ds.Split(tolerance);
            }
            else
            {
                segment2Ds = new List<Segment2D>();
                foreach(ISegmentable2D segmentable2D in segmentable2Ds)
                {
                    List<Segment2D> segment2Ds_Temp = segmentable2D.GetSegments();
                    if (segment2Ds_Temp == null)
                    {
                        continue;
                    }

                    segment2Ds.AddRange(segment2Ds_Temp);
                }
            }

            AdjacencyGraph<Point2D, Edge<Point2D>> adjacencyGraph = segment2Ds?.AdjacencyGraph(tolerance);
            if (adjacencyGraph == null || adjacencyGraph.Vertices == null || adjacencyGraph.Vertices.Count() == 0)
            {
                return null;
            }

            if (point2D_Start == null)
            {
                IEnumerable<Edge<Point2D>> edges_Temp;

                List<Point2D> point2Ds_Temp = new List<Point2D>();
                foreach (Point2D point2D in adjacencyGraph.Vertices)
                {
                    if (!adjacencyGraph.TryGetOutEdges(point2D, out edges_Temp) || edges_Temp == null || edges_Temp.Count() != 1)
                    {
                        continue;
                    }

                    point2Ds_Temp.Add(point2D);
                }

                if (point2Ds_Temp != null || point2Ds_Temp.Count != 0)
                {
                    double maxDistance_Vertex = Query.MaxDistance(point2Ds_Temp, out Point2D point2D_1, out Point2D point2D_2);
                    point2D_Start = point2D_1;
                }
            }
            else
            {
                double distance = double.MaxValue;
                Point2D point2D = null;
                foreach(Point2D point_Temp in adjacencyGraph.Vertices)
                {
                    double distance_Temp = point_Temp.Distance(point2D_Start);
                    if (distance_Temp < distance)
                    {
                        distance = distance_Temp;
                        point2D = point_Temp;
                    }
                }

                if (point2D != null)
                    point2D_Start = point2D;
            }

            if (point2D_Start == null)
            {
                double maxDistance_Vertex = Query.MaxDistance(adjacencyGraph.Vertices, out Point2D point2D_1, out Point2D point2D_2);
                point2D_Start = point2D_1;
            }

            if (point2D_Start == null)
            {
                point2D_Start = adjacencyGraph.Vertices.First();
            }

            if(point2D_Start == null)
            {
                return null;
            }

            AStarShortestPathAlgorithm<Point2D, Edge<Point2D>> aStarShortestPathAlgorithm = new AStarShortestPathAlgorithm<Point2D, Edge<Point2D>>(adjacencyGraph, edge => edge.Source.Distance(edge.Target), point2D => point2D.Distance(point2D_Start));

            VertexPredecessorRecorderObserver<Point2D, Edge<Point2D>> vertexPredecessorRecorderObserver = new VertexPredecessorRecorderObserver<Point2D, Edge<Point2D>>();
            vertexPredecessorRecorderObserver.Attach(aStarShortestPathAlgorithm);

            VertexDistanceRecorderObserver<Point2D, Edge<Point2D>> vertexDistanceRecorderObserver = new VertexDistanceRecorderObserver<Point2D, Edge<Point2D>>(edge => edge.Source.Distance(edge.Target));
            vertexDistanceRecorderObserver.Attach(aStarShortestPathAlgorithm);

            aStarShortestPathAlgorithm.Compute(point2D_Start);

            Point2D point2D_End = null;
            double maxDistance = double.MinValue;
            foreach (KeyValuePair<Point2D, double> keyValuePair in vertexDistanceRecorderObserver.Distances)
            {
                if (keyValuePair.Value > maxDistance)
                {
                    point2D_End = keyValuePair.Key;
                    maxDistance = keyValuePair.Value;
                }
            }

            vertexPredecessorRecorderObserver.TryGetPath(point2D_End, out IEnumerable<Edge<Point2D>> edges);

            if (edges == null)
            {
                return null;
            }

            List<Point2D> point2Ds = new List<Point2D>();
            foreach (Edge<Point2D> edge in edges)
            {
                point2Ds.Add(edge.Source);
                int index = segment2Ds.FindIndex(x => (x[0].AlmostEquals(edge.Source, tolerance) && x[1].AlmostEquals(edge.Target, tolerance)) || (x[1].AlmostEquals(edge.Source, tolerance) && x[0].AlmostEquals(edge.Target, tolerance)));
                if (index != -1)
                {
                    segment2Ds.RemoveAt(index);
                }
            }

            point2Ds.Add(edges.Last().Target);

            List<Polyline2D> result = new List<Polyline2D>();
            result.Add(new Polyline2D(point2Ds));

            if (segment2Ds.Count > 0)
            {
                List<Polyline2D> polyline2Ds = Polyline2Ds(segment2Ds, null, false, tolerance);
                if(polyline2Ds != null && polyline2Ds.Count != 0)
                {
                    result.AddRange(polyline2Ds);
                }
            }

            return result;
        }
    }
}