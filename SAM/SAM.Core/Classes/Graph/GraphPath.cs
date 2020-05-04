using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core
{
    public class GraphPath : IEnumerable<GraphEdge>
    {
        private List<GraphEdge> graphEdges;

        public GraphPath()
        {
            graphEdges = new List<GraphEdge>();
        }

        public GraphPath(GraphPath graphPath)
        {
            graphEdges = new List<GraphEdge>(graphPath.graphEdges);
        }

        public double GetWeight()
        {
            double weight = 0;

            if (graphEdges == null || graphEdges.Count == 0)
                return weight;

            graphEdges.ForEach(x => weight += x.Weight);
            return weight;
        }

        public bool Add(GraphEdge graphEdge)
        {
            if (graphEdges == null)
                graphEdges = new List<GraphEdge>();

            if (graphEdges.Count == 0)
            {
                graphEdges.Add(graphEdge);
                return true;
            }

            GraphEdge graphEdge_Last = graphEdges.Last();

            if (graphEdge is GraphNode)
            {
                if (graphEdge_Last is GraphNode)
                    return false;

                GraphNode graphNode = (GraphNode)graphEdge;
                if (graphNode.Contains(graphEdge_Last))
                {
                    graphEdges.Add(graphNode);
                    return true;
                }

                return false;
            }

            if (graphEdge_Last is GraphNode)
            {
                GraphNode graphNode = (GraphNode)graphEdge_Last;
                if (graphNode.Contains(graphEdge))
                {
                    graphEdges.Add(graphNode);
                    return true;
                }
            }

            return false;
        }

        public IEnumerator<GraphEdge> GetEnumerator()
        {
            if (graphEdges == null)
                return null;

            return graphEdges.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (graphEdges == null)
                return null;

            return graphEdges.GetEnumerator();
        }

        public int IndexOf(GraphEdge graphEdge)
        {
            if (graphEdge == null)
                return -1;

            if (graphEdges == null)
                return -1;

            return graphEdges.IndexOf(graphEdge);
        }
    }
}