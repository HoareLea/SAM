using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Graph
{
    public class GraphEdge
    {
        private object @object = null;
        private double weight = 1;

        public double Weight => weight;

        public object Object => @object;

        public GraphEdge(object @object, double weight)
        {
            this.@object = @object;
            this.weight = weight;
        }

        public GraphEdge()
        {

        }

        public GraphEdge(object @object)
        {
            this.@object = @object;
        }
    }
}
