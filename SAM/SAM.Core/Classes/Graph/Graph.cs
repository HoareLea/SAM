using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Core
{
    public class Graph<Node, Edge>: IEnumerable<GraphNode<Node, Edge>>
    {
        private List<GraphNode<Node, Edge>> graphNodes;

        public Graph(IEnumerable<GraphNode<Node, Edge>> graphNodes)
        {
            if (graphNodes != null)
                this.graphNodes = new List<GraphNode<Node, Edge>>(graphNodes); 
        }

        public HashSet<GraphNode<Node, Edge>> GetGraphNodes(object @object)
        {            
            HashSet<GraphNode<Node, Edge>> result = new HashSet<GraphNode<Node, Edge>>();
            if (@object is GraphNode<Node, Edge>)
            {
                GraphNode<Node, Edge> graphNode = graphNodes.Find(x => x == (GraphNode<Node, Edge>)@object);
                if (graphNode == null)
                    return result;
                
                result.Add(graphNode);

                return result;
            }

            if(@object is GraphEdge<Edge>)
            {
                foreach (GraphNode<Node, Edge> graphNode in this)
                    if (graphNode.Contains((GraphEdge<Edge>)@object))
                        result.Add(graphNode);

                return result;
            }


            foreach (GraphNode<Node, Edge> graphNode in this)
                if (graphNode.Contains(@object))
                    result.Add(graphNode);

            return result;
        }

        public HashSet<IGraphEdge> GetGraphEdges(object @object)
        {
            HashSet<IGraphEdge> result = new HashSet<IGraphEdge>();
            if (@object is GraphNode<Node, Edge>)
            {
                GraphNode<Node, Edge> graphNode = graphNodes.Find(x => x == (GraphNode<Node, Edge>)@object);
                if (graphNode == null)
                    return result;

                foreach (GraphEdge<Edge> graphEdge in graphNode)
                    result.Add(graphEdge);

                return result;
            }

            if (@object is GraphEdge<Edge>)
            {
                foreach (GraphNode<Node, Edge> graphNode in this)
                    if (graphNode.Contains((GraphEdge<Edge>)@object))
                    {
                        result.Add((GraphEdge<Edge>)@object);
                        return result;
                    }

                return result;
            }


            foreach (GraphNode<Node, Edge> graphNode in this)
            {
                foreach(GraphEdge<Edge> graphEdge in graphNode)
                {
                    if (ReferenceEquals(graphEdge.Object, @object) && !result.Contains(graphEdge))
                        result.Add(graphEdge);
                }
            }

            return result;
        }

        public HashSet<IGraphEdge> GetGraphEdges(object object_From, object object_To)
        {
            if (object_From == null || object_To == null)
                return null;

            HashSet<IGraphEdge> graphEdges_From = GetGraphEdges(object_From);
            if (graphEdges_From == null || graphEdges_From.Count == 0)
                return null;

            HashSet<IGraphEdge> graphEdges_To = GetGraphEdges(object_To);
            if (graphEdges_To == null || graphEdges_To.Count == 0)
                return null;

            throw new NotImplementedException();
        }

        public bool Next(ref List<GraphPath<Node, Edge>> graphPaths)
        {
            bool result = false;
            
            List<GraphPath<Node, Edge>> graphPaths_New = new List<GraphPath<Node, Edge>>();
            foreach (GraphPath<Node, Edge> graphPath in graphPaths)
            {
                if (graphPath.Count() > 0 && graphPath.First() == graphPath.Last())
                    continue;
                
                IGraphEdge graphEdge = graphPath.Last();
                if(graphEdge is GraphNode<Node, Edge>)
                {
                    HashSet<IGraphEdge> graphEdges = GetGraphEdges(graphEdge);
                    if(graphEdges != null && graphEdges.Count > 0)
                    {
                        graphEdges.Remove(graphEdge);
                        foreach(GraphEdge<Edge> graphEdge_New in graphEdges)
                        {
                            GraphPath<Node, Edge> graphPath_New = new GraphPath<Node, Edge>(graphPath);
                            if(graphPath_New.Add(graphEdge_New))
                            {
                                graphPaths_New.Add(graphPath_New);
                                result = true;
                            }
                        }
                    }
                }
                else
                {
                    HashSet<GraphNode<Node, Edge>> graphNodes = GetGraphNodes(graphEdge);
                    if (graphNodes != null && graphNodes.Count > 0)
                    {
                        foreach(GraphNode<Node, Edge> graphNode in graphNodes)
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

        public Graph<Node, Edge> GetGraph(object @object)
        {
            HashSet<GraphEdge<Edge>> graphEdges = new HashSet<GraphEdge<Edge>>();
            foreach (GraphEdge<Edge> graphEdge in GetGraphEdges(@object))
                AppendGraphEdges(graphEdge, ref graphEdges);

            HashSet<GraphNode<Node, Edge>> graphNodes = new HashSet<GraphNode<Node, Edge>>();
            foreach(GraphEdge<Edge> graphEdge in graphEdges)
            {
                HashSet<GraphNode<Node, Edge>> graphNodes_Temp = GetGraphNodes(graphEdge);
                if (graphNodes_Temp == null || graphNodes_Temp.Count == 0)
                    continue;

                foreach (GraphNode<Node, Edge> graphNode in graphNodes_Temp)
                    graphNodes.Add(graphNode);
            }

            return new Graph<Node, Edge>(graphNodes);
        }

        public List<Graph<Node, Edge>> GetGraphs()
        {
            List<Graph<Node, Edge>> result = new List<Graph<Node, Edge>>();
            if (graphNodes == null || graphNodes.Count == 0)
                return result;

            foreach (GraphNode<Node, Edge> graphNode in graphNodes)
            {
                bool exists = false;
                foreach (Graph<Node, Edge> graph_Temp in result)
                    if (graph_Temp.Contains(graphNode))
                    {
                        exists = true;
                        break;
                    }

                if (!exists)
                    result.Add(new Graph<Node, Edge>(GetGraph(graphNode)));
            }

            return result;
        }

        public bool Remove(object @object)
        {
            if (@object == null)
                return false;

            if(@object is GraphNode<Node, Edge>)
                return graphNodes.Remove((GraphNode<Node, Edge>)@object);

            HashSet<GraphNode<Node, Edge>> graphNodes_Temp = GetGraphNodes(@object);
            if (graphNodes_Temp != null && graphNodes_Temp.Count > 0)
            {
                foreach (GraphNode<Node, Edge> graphNode in graphNodes_Temp)
                    graphNode.Remove(@object);
                return true;
            }

            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return graphNodes.GetEnumerator();
        }

        public IEnumerator<GraphNode<Node, Edge>> GetEnumerator()
        {
            return graphNodes.GetEnumerator();
        }


        private void AppendGraphEdges(GraphEdge<Edge> graphEdge, ref HashSet<GraphEdge<Edge>> graphEdges)
        {
            if (graphEdge == null)
                return;

            graphEdges.Add(graphEdge);

            HashSet<GraphNode<Node, Edge>> graphNodes = GetGraphNodes(graphEdge);
            if (graphNodes == null || graphNodes.Count == 0)
                return;

            foreach (GraphNode<Node, Edge> graphNode in graphNodes)
            {
                HashSet<IGraphEdge> graphEdges_Temp = GetGraphEdges(graphNode);
                if (graphEdges_Temp == null || graphEdges_Temp.Count == 0)
                    continue;

                foreach (GraphEdge<Edge> graphEdge_Temp in graphEdges_Temp)
                    if (graphEdges.Add(graphEdge_Temp))
                        AppendGraphEdges(graphEdge_Temp, ref graphEdges);
            }
        }
    }
}
