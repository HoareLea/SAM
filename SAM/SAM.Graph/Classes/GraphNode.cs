using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Graph
{
    public class GraphNode : GraphEdge, IEnumerable<GraphEdge>
    {
        private List<GraphEdge> graphEdges;

        public GraphNode(object @object, double weight)
            : base(@object, weight)
        {

        }

        public GraphNode(object @object, double weight, IEnumerable<GraphEdge> graphEdges)
            : base(@object, weight)
        {
            if (graphEdges != null)
                this.graphEdges = new List<GraphEdge>(graphEdges);
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
