using QuickGraph;
using QuickGraph.Algorithms.Observers;
using QuickGraph.Algorithms.ShortestPath;
using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static List<Point2D> ShortestPath(this IEnumerable<ISegmentable2D> segmentable2Ds, Point2D point2D_Start, Point2D point2D_End, double tolerance = Core.Tolerance.Distance)
        {
            if (segmentable2Ds == null)
                return null;

            List<ISegmentable2D> segmentable2Ds_Temp = new List<ISegmentable2D>(segmentable2Ds);
            
            Point2D point2D_Start_Temp = null;
            Point2D point2D_End_Temp = null;
            foreach (ISegmentable2D segmentable2D in segmentable2Ds_Temp)
            {
                List<Point2D> point2Ds = segmentable2D.GetPoints();
                if (point2Ds == null)
                    continue;

                if(point2D_Start_Temp == null)
                {
                    point2D_Start_Temp = point2Ds.Find(x => x.AlmostEquals(point2D_Start, tolerance));
                }

                if (point2D_End_Temp == null)
                {
                    point2D_End_Temp = point2Ds.Find(x => x.AlmostEquals(point2D_End, tolerance));
                }

                if (point2D_Start_Temp != null && point2D_End_Temp != null)
                    break;
            }

            if(point2D_Start_Temp == null || point2D_End_Temp == null)
            {
                double distance_Min_Start = double.MaxValue;
                ISegmentable2D segmentable2D_Start = null;

                double distance_Min_End = double.MaxValue;
                ISegmentable2D segmentable2D_End = null;

                foreach (ISegmentable2D segmentable2D in segmentable2Ds_Temp)
                {
                    if (point2D_Start_Temp == null)
                    {
                        double distance = segmentable2D.Distance(point2D_Start);
                        if(distance < distance_Min_Start)
                        {
                            distance_Min_Start = distance;
                            segmentable2D_Start = segmentable2D;
                        }
                    }

                    if (point2D_End_Temp == null)
                    {
                        double distance = segmentable2D.Distance(point2D_End);
                        if (distance < distance_Min_End)
                        {
                            distance_Min_End = distance;
                            segmentable2D_End = segmentable2D;
                        }
                    }

                    if ((point2D_Start_Temp != null || segmentable2D_Start != null) && (point2D_End_Temp != null || segmentable2D_End != null))
                        break;
                }

                if(point2D_Start_Temp == null && segmentable2D_Start != null)
                {
                    if(segmentable2D_Start.On(point2D_Start, tolerance))
                    {
                        int index = segmentable2D_Start.IndexOfClosestPoint2D(point2D_Start);
                        if(index!= -1)
                        {
                            Point2D point2D = segmentable2D_Start.GetPoints()[index];
                            segmentable2Ds_Temp.Add(new Segment2D(point2D_Start, point2D));
                            point2D_Start_Temp = point2D_Start;
                        }
                    }
                    else
                    {
                        Point2D point2D = segmentable2D_Start.Closest(point2D_Start);
                        if(point2D != null)
                        {
                            segmentable2Ds_Temp.Add(new Segment2D(point2D_Start, point2D));
                            point2D_Start_Temp = point2D_Start;
                        }
                        
                    }
                }

                if (point2D_End_Temp == null && segmentable2D_End != null)
                {
                    if (segmentable2D_End.On(point2D_End, tolerance))
                    {
                        int index = segmentable2D_End.IndexOfClosestPoint2D(point2D_End);
                        if (index != -1)
                        {
                            Point2D point2D = segmentable2D_End.GetPoints()[index];
                            segmentable2Ds_Temp.Add(new Segment2D(point2D_End, point2D));
                            point2D_End_Temp = point2D_End;
                        }
                    }
                    else
                    {
                        Point2D point2D = segmentable2D_End.Closest(point2D_End);
                        if (point2D != null)
                        {
                            segmentable2Ds_Temp.Add(new Segment2D(point2D_End, point2D));
                            point2D_End_Temp = point2D_End;
                        }

                    }
                }
            }

            if (point2D_Start_Temp == null || point2D_End_Temp == null)
                return null;

            List<Segment2D> segment2Ds = segmentable2Ds_Temp.Split(tolerance);
            if (segment2Ds == null)
                return null;

            AdjacencyGraph<Point2D, Edge<Point2D>> adjacencyGraph = Geometry.Create.AdjacencyGraph(segment2Ds);

            AStarShortestPathAlgorithm<Point2D, Edge<Point2D>> aStarShortestPathAlgorithm = new AStarShortestPathAlgorithm<Point2D, Edge<Point2D>>(adjacencyGraph, edge => edge.Source.Distance(edge.Target), point2D => point2D.Distance(point2D_Start_Temp));

            VertexPredecessorRecorderObserver<Point2D, Edge<Point2D>> vertexPredecessorRecorderObserver = new VertexPredecessorRecorderObserver<Point2D, Edge<Point2D>>();
            vertexPredecessorRecorderObserver.Attach(aStarShortestPathAlgorithm);

            aStarShortestPathAlgorithm.Compute(point2D_Start_Temp);

            List<Point2D> result = new List<Point2D>();
            if (!vertexPredecessorRecorderObserver.TryGetPath(point2D_End_Temp, out IEnumerable<Edge<Point2D>> edges))
                return result;

            foreach(Edge<Point2D> edge in edges)
            {
                result.Add(edge.Source);
            }

            result.Add(point2D_End_Temp);

            return result;
        }
    }
}