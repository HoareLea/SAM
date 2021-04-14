using QuickGraph;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {

        /// <summary>
        /// Groups segmentable2Ds if there are connection between them
        /// </summary>
        /// <param name="segmentable2Ds">Segmentable2Ds</param>
        /// <param name="split">Split Segmentable2Ds before connectivity check</param>
        /// <param name="tolerance">Tolerance</param>
        /// <returns>List of connected Segmentable2Ds</returns>
        public static List<List<T>> Connected<T>(this IEnumerable<T> segmentable2Ds, bool split = true, double tolerance = Core.Tolerance.Distance) where T : ISegmentable2D
        {
            if (segmentable2Ds == null)
            {
                return null;
            }

            List<Segment2D> segment2Ds = new List<Segment2D>();
            List<Tuple<BoundingBox2D, List<Point2D>, T>> tuples = new List<Tuple<BoundingBox2D, List<Point2D>, T>>();
            foreach (T segmentable2D in segmentable2Ds)
            {
                List<Segment2D> segment2Ds_Temp = segmentable2D?.GetSegments();
                if(segment2Ds_Temp == null)
                {
                    continue;
                }

                segment2Ds.AddRange(segment2Ds_Temp);
                
                tuples.Add(new Tuple<BoundingBox2D, List<Point2D>, T>(segmentable2D.GetBoundingBox(), segmentable2D.GetPoints(), segmentable2D));
            }

            if(split)
            {
                segment2Ds = Query.Split(segment2Ds, tolerance);
            }
            

            UndirectedGraph<Point2D, Edge<Point2D>> undirectedGraph = segment2Ds.UndirectedGraph();
            if (undirectedGraph == null)
            {
                return null;
            }

            QuickGraph.Algorithms.ConnectedComponents.ConnectedComponentsAlgorithm<Point2D, Edge<Point2D>> connectedComponentsAlgorithm = new QuickGraph.Algorithms.ConnectedComponents.ConnectedComponentsAlgorithm<Point2D, Edge<Point2D>>(undirectedGraph);

            connectedComponentsAlgorithm.Compute();

            IDictionary<Point2D, int> components = connectedComponentsAlgorithm.Components;
            if(components == null || components.Count == 0)
            {
                return null;
            }

            List<List<T>> result = Enumerable.Repeat<List<T>>(null, connectedComponentsAlgorithm.ComponentCount).ToList();

            foreach (KeyValuePair<Point2D, int> keyValuePair in components)
            {
                List<Tuple<BoundingBox2D, List<Point2D>, T>> tuples_Temp = tuples.FindAll(x => x.Item1.InRange(keyValuePair.Key, tolerance)).FindAll(x => x.Item2.Find(y => y.AlmostEquals(keyValuePair.Key, tolerance)) != null);

                List<T> segmentable2Ds_Temp = result[keyValuePair.Value];
                if(segmentable2Ds_Temp == null)
                {
                    segmentable2Ds_Temp = new List<T>();
                    result[keyValuePair.Value] = segmentable2Ds_Temp;
                }
                
                foreach(T segmentable2D in tuples_Temp.ConvertAll(x => x.Item3))
                {
                    if(!segmentable2Ds_Temp.Contains(segmentable2D))
                    {
                        segmentable2Ds_Temp.Add(segmentable2D);
                    }
                }
            }

            return result;
        }
    }
}