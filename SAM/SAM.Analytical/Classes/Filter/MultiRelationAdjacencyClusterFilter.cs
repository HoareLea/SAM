using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Analytical
{
    public abstract class MultiRelationAdjacencyClusterFilter<T> : MultiRelationFilter<T>, IAdjacencyClusterFilter where T: IJSAMObject
    {
        public AdjacencyCluster AdjacencyCluster { get; set; }

        public MultiRelationAdjacencyClusterFilter(IFilter filter)
        {
            Filter = filter;
        }

        public MultiRelationAdjacencyClusterFilter(JObject jObject)
            :base(jObject)
        {

        }

        public MultiRelationAdjacencyClusterFilter(MultiRelationAdjacencyClusterFilter<T> multiRelationAdjacencyClusterFilter)
            :base(multiRelationAdjacencyClusterFilter)
        {
            AdjacencyCluster = multiRelationAdjacencyClusterFilter?.AdjacencyCluster;
        }
    }
}