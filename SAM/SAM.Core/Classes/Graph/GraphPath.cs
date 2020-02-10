using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core
{
    public class GraphPath<Node, Edge> : IEnumerable<IGraphEdge>
    {
        private List<IGraphEdge> graphEdges;

        public GraphPath()
        {
            graphEdges = new List<IGraphEdge>();
        }

        public GraphPath(GraphPath<Node, Edge> graphPath)
        {
            graphEdges = new List<IGraphEdge>(graphPath.graphEdges);
        }

        public double GetWeight()
        {
            double weight = 0;

            if (graphEdges == null || graphEdges.Count == 0)
                return weight;

            graphEdges.ForEach(x => weight+= x.Weight);
            return weight;
        }

        public bool Add(IGraphEdge graphEdge)
        {
            if (graphEdges == null)
                graphEdges = new List<IGraphEdge>();
            
            if(graphEdges.Count == 0)
            {
                graphEdges.Add(graphEdge);
                return true;
            }

            IGraphEdge graphEdge_Last = graphEdges.Last();

            if (graphEdge is IGraphNode)
            {
                if (graphEdge_Last is IGraphNode)
                    return false;

                IGraphNode graphNode = (IGraphNode)(object)graphEdge;
                if(graphNode.Contains(graphEdge_Last))
                {
                    graphEdges.Add(graphNode);
                    return true;
                }

                return false;
            }

            if (graphEdge_Last is GraphNode<Node, Edge>)
            {
                GraphNode<Node, Edge> graphNode = (GraphNode<Node, Edge>)(object)graphEdge_Last;
                if (graphNode.Contains(graphEdge))
                {
                    graphEdges.Add(graphNode);
                    return true;
                }
            }

            return false;
        }

        public IEnumerator<IGraphEdge> GetEnumerator()
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

        public int IndexOf(IGraphEdge graphEdge)
        {
            if (graphEdge == null)
                return -1;

            if (graphEdges == null)
                return -1;

            return graphEdges.IndexOf(graphEdge);
        }
    }
}
