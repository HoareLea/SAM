using System.Collections;
using System.Collections.Generic;

namespace SAM.Core
{
    public class GraphNode : GraphEdge, IEnumerable<GraphEdge>
    {
        private List<GraphEdge> graphEdges;

        public GraphNode(object @object, double weight)
            : base(@object, weight)
        {
            graphEdges = new List<GraphEdge>();
        }

        public GraphNode(object @object)
            : base(@object)
        {
            graphEdges = new List<GraphEdge>();
        }

        public GraphNode(object @object, double weight, IEnumerable<GraphEdge> graphEdges)
            : base(@object, weight)
        {
            if (graphEdges != null)
                this.graphEdges = new List<GraphEdge>(graphEdges);
        }

        public bool Add(GraphEdge graphEdge)
        {
            graphEdges.Add(graphEdge);
            return true;
        }

        public GraphNode(GraphNode graphNode)
            : base(graphNode)
        {
            if (graphNode.graphEdges != null)
                graphEdges = new List<GraphEdge>(graphNode.graphEdges);
        }

        public bool Contains(object @object)
        {
            if (graphEdges == null)
                return false;

            if (@object is GraphEdge)
                return graphEdges.Contains((GraphEdge)@object);

            foreach (GraphEdge graphEdge in graphEdges)
                if (graphEdge.Object == @object)
                    return true;

            return false;
        }

        public bool Remove(object @object)
        {
            if (@object is GraphEdge)
                return graphEdges.Remove((GraphEdge)@object);

            return graphEdges.RemoveAll(x => x.Object == @object) > 0;
        }

        public IEnumerator<GraphEdge> GetEnumerator()
        {
            return graphEdges.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return graphEdges.GetEnumerator();
        }
    }
}