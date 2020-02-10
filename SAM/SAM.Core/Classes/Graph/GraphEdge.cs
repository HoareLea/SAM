using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Core
{
    public class GraphEdge
    {
        private object @object = null;
        private double weight = 1;

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

        public GraphEdge(GraphEdge graphEdge)
        {
            this.@object = graphEdge.@object;
            this.weight = graphEdge.weight;
        }

        public T GetObject<T>()
        {
            if (@object == null)
                return default;

            if (typeof(T).IsAssignableFrom(@object.GetType()))
                return (T)(object)@object;

            return default;
        }

        public double Weight => weight;

        public object Object => @object;
    }
}
