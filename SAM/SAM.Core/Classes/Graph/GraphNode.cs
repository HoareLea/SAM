using System.Collections;
using System.Collections.Generic;

namespace SAM.Core
{
    public class GraphNode<Node, Edge> : GraphEdge<Node>, IEnumerable<GraphEdge<Edge>>, IGraphNode
    {
        private List<GraphEdge<Edge>> graphEdges;

        public GraphNode(Node @object, double weight)
            : base(@object, weight)
        {

        }

        public GraphNode(Node @object, double weight, IEnumerable<GraphEdge<Edge>> graphEdges)
            : base(@object, weight)
        {
            if (graphEdges != null)
                this.graphEdges = new List<GraphEdge<Edge>>(graphEdges);
        }

        public GraphNode(GraphNode<Node, Edge> graphNode)
            : base(graphNode)
        {
            if (graphNode.graphEdges != null)
                graphEdges = new List<GraphEdge<Edge>>(graphNode.graphEdges);
        }

        public bool Contains(Edge @object)
        {
            if (graphEdges == null)
                return false;

            if (@object is GraphEdge<Edge>)
                return graphEdges.Contains((GraphEdge<Edge>)(object)@object);

            foreach (GraphEdge<Edge> graphEdge in graphEdges)
                if (ReferenceEquals(graphEdge.Object, @object))
                    return true;

            return false;
        }

        public bool Remove(object @object)
        {
            if (@object is GraphEdge<Edge>)
                return graphEdges.Remove((GraphEdge<Edge>)@object);

            return graphEdges.RemoveAll(x => ReferenceEquals(x.Object,@object)) > 0;
        }

        public IEnumerator<GraphEdge<Edge>> GetEnumerator()
        {
            return graphEdges.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return graphEdges.GetEnumerator();
        }

        public bool Contains(IGraphEdge graphEdge)
        {
            if (graphEdges == null)
                return false;

            if (graphEdge is GraphEdge<Edge>)
                return graphEdges.Contains((GraphEdge<Edge>)graphEdge);

            foreach (GraphEdge<Edge> graphEdge_Temp in graphEdges)
                if (ReferenceEquals(graphEdge_Temp.Object, graphEdge.GetObject()))
                    return true;

            return false;
        }
    }
}
