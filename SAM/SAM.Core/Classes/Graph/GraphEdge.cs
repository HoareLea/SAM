namespace SAM.Core
{
    public class GraphEdge<Edge> : IGraphEdge
    {
        private Edge @object;
        private double weight = 1;

        public GraphEdge(Edge @object, double weight)
        {
            this.@object = @object;
            this.weight = weight;
        }

        public GraphEdge()
        {

        }

        public GraphEdge(Edge @object)
        {
            this.@object = @object;
        }

        public GraphEdge(IGraphEdge graphEdge)
        {
            object @object = graphEdge.GetObject();
            if(@object != null)
            {
                if(typeof(Edge).IsAssignableFrom(@object.GetType()))
                    this.@object = (Edge)(object)graphEdge.GetObject();
            }

            this.weight = graphEdge.Weight;
        }

        public Z GetObject<Z>()
        {
            if (@object == null)
                return default;

            if (typeof(Z).IsAssignableFrom(@object.GetType()))
                return (Z)(object)@object;

            return default;
        }

        public object GetObject()
        {
            return @object;
        }

        public double Weight => weight;

        public Edge Object => @object;


    }
}
