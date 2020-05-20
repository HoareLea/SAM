using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core
{
    public class Graph : IEnumerable<GraphNode>
    {
        private List<GraphNode> graphNodes;

        public Graph()
        {
            graphNodes = new List<GraphNode>();
        }

        public Graph(IEnumerable<GraphNode> graphNodes)
        {
            if (graphNodes != null)
                this.graphNodes = new List<GraphNode>(graphNodes);
        }

        public HashSet<GraphNode> GetGraphNodes(object @object)
        {
            HashSet<GraphNode> result = new HashSet<GraphNode>();
            if (@object is GraphNode)
            {
                GraphNode graphNode = graphNodes.Find(x => x == (GraphNode)@object);
                if (graphNode == null)
                    return result;

                result.Add(graphNode);

                return result;
            }

            if (@object is GraphEdge)
            {
                foreach (GraphNode graphNode in this)
                    if (graphNode.Contains((GraphEdge)@object))
                        result.Add(graphNode);

                return result;
            }

            foreach (GraphNode graphNode in this)
                if (graphNode.Contains(@object))
                    result.Add(graphNode);

            return result;
        }

        public HashSet<GraphEdge> GetGraphEdges(object @object)
        {
            HashSet<GraphEdge> result = new HashSet<GraphEdge>();
            if (@object is GraphNode)
            {
                GraphNode graphNode = graphNodes.Find(x => x == (GraphNode)@object);
                if (graphNode == null)
                    return result;

                foreach (GraphEdge graphEdge in graphNode)
                    result.Add(graphEdge);

                return result;
            }

            if (@object is GraphEdge)
            {
                foreach (GraphNode graphNode in this)
                    if (graphNode.Contains((GraphEdge)@object))
                    {
                        result.Add((GraphEdge)@object);
                        return result;
                    }

                return result;
            }

            foreach (GraphNode graphNode in this)
            {
                foreach (GraphEdge graphEdge in graphNode)
                {
                    if (graphEdge.Object == @object && !result.Contains(graphEdge))
                        result.Add(graphEdge);
                }
            }

            return result;
        }

        public bool Connect(object object_1, object object_2)
        {
            GraphNode graphNode_1 = null;

            if (object_1 is GraphNode)
            {
            }
            else if (object_1 is GraphEdge)
            {
            }
            else
            {
                graphNode_1 = graphNodes.Find(x => ReferenceEquals(x.Object, object_1));
                if (graphNode_1 == null)
                {
                    graphNode_1 = new GraphNode(object_1);
                    graphNodes.Add(graphNode_1);
                }
            }

            if (graphNode_1 == null)
                return false;

            GraphNode graphNode_2 = null;

            if (object_2 is GraphNode)
            {
            }
            else if (object_2 is GraphEdge)
            {
            }
            else
            {
                graphNode_2 = graphNodes.Find(x => ReferenceEquals(x.Object, object_2));
                if (graphNode_2 == null)
                {
                    graphNode_2 = new GraphNode(object_2);
                    graphNodes.Add(graphNode_2);
                }
            }

            if (graphNode_2 == null)
                return false;

            GraphEdge graphEdge = new GraphEdge();
            graphNode_1.Add(graphEdge);
            graphNode_2.Add(graphEdge);
            return true;
        }

        public bool Next(ref List<GraphPath> graphPaths)
        {
            bool result = false;

            List<GraphPath> graphPaths_New = new List<GraphPath>();
            foreach (GraphPath graphPath in graphPaths)
            {
                if (graphPath.Count() > 0 && graphPath.First() == graphPath.Last())
                    continue;

                GraphEdge graphEdge = graphPath.Last();
                if (graphEdge is GraphNode)
                {
                    HashSet<GraphEdge> graphEdges = GetGraphEdges(graphEdge);
                    if (graphEdges != null && graphEdges.Count > 0)
                    {
                        graphEdges.Remove(graphEdge);
                        foreach (GraphEdge graphEdge_New in graphEdges)
                        {
                            GraphPath graphPath_New = new GraphPath(graphPath);
                            if (graphPath_New.Add(graphEdge_New))
                            {
                                graphPaths_New.Add(graphPath_New);
                                result = true;
                            }
                        }
                    }
                }
                else
                {
                    HashSet<GraphNode> graphNodes = GetGraphNodes(graphEdge);
                    if (graphNodes != null && graphNodes.Count > 0)
                    {
                        foreach (GraphNode graphNode in graphNodes)
                        {
                            int index = graphPath.IndexOf(graphNode);
                            if (index > 0)
                                continue;

                            if (graphPath.Add(graphNode))
                                result = true;
                        }
                    }
                }
            }

            if (graphPaths_New != null)
                graphPaths.AddRange(graphPaths_New);

            return result;
        }

        public Graph GetGraph(object @object)
        {
            HashSet<GraphEdge> graphEdges = new HashSet<GraphEdge>();
            foreach (GraphEdge graphEdge in GetGraphEdges(@object))
                AppendGraphEdges(graphEdge, ref graphEdges);

            HashSet<GraphNode> graphNodes = new HashSet<GraphNode>();
            foreach (GraphEdge graphEdge in graphEdges)
            {
                HashSet<GraphNode> graphNodes_Temp = GetGraphNodes(graphEdge);
                if (graphNodes_Temp == null || graphNodes_Temp.Count == 0)
                    continue;

                foreach (GraphNode graphNode in graphNodes_Temp)
                    graphNodes.Add(graphNode);
            }

            return new Graph(graphNodes);
        }

        public List<Graph> GetGraphs()
        {
            List<Graph> result = new List<Graph>();
            if (graphNodes == null || graphNodes.Count == 0)
                return result;

            foreach (GraphNode graphNode in graphNodes)
            {
                bool exists = false;
                foreach (Graph graph_Temp in result)
                    if (graph_Temp.Contains(graphNode))
                    {
                        exists = true;
                        break;
                    }

                if (!exists)
                    result.Add(new Graph(GetGraph(graphNode)));
            }

            return result;
        }

        public bool Remove(object @object)
        {
            if (@object == null)
                return false;

            if (@object is GraphNode)
                return graphNodes.Remove((GraphNode)@object);

            HashSet<GraphNode> graphNodes_Temp = GetGraphNodes(@object);
            if (graphNodes_Temp != null && graphNodes_Temp.Count > 0)
            {
                foreach (GraphNode graphNode in graphNodes_Temp)
                    graphNode.Remove(@object);
                return true;
            }

            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return graphNodes.GetEnumerator();
        }

        public IEnumerator<GraphNode> GetEnumerator()
        {
            return graphNodes.GetEnumerator();
        }

        public RelationCluster GetRelationCluster()
        {
            return GetRelationCluster<object, object>();
        }

        public RelationCluster GetRelationCluster<X, Z>()
        {
            RelationCluster result = new RelationCluster();
            foreach (GraphNode graphNode in graphNodes)
            {
                if (!(graphNode.Object is X))
                    continue;

                foreach (GraphEdge graphEdge in graphNode)
                {
                    HashSet<GraphNode> graphNodes_Temp = GetGraphNodes(graphEdge);
                    foreach (GraphNode graphNode_Temp in graphNodes_Temp)
                    {
                        if (!(graphNode.Object is Z))
                            continue;

                        if (graphNode_Temp != graphNode)
                            result.Add(graphNode.Object, graphNode_Temp.Object);
                    }
                }
            }

            return result;
        }

        private void AppendGraphEdges(GraphEdge graphEdge, ref HashSet<GraphEdge> graphEdges)
        {
            if (graphEdge == null)
                return;

            graphEdges.Add(graphEdge);

            HashSet<GraphNode> graphNodes = GetGraphNodes(graphEdge);
            if (graphNodes == null || graphNodes.Count == 0)
                return;

            foreach (GraphNode graphNode in graphNodes)
            {
                HashSet<GraphEdge> graphEdges_Temp = GetGraphEdges(graphNode);
                if (graphEdges_Temp == null || graphEdges_Temp.Count == 0)
                    continue;

                foreach (GraphEdge graphEdge_Temp in graphEdges_Temp)
                    if (graphEdges.Add(graphEdge_Temp))
                        AppendGraphEdges(graphEdge_Temp, ref graphEdges);
            }
        }
    }
}