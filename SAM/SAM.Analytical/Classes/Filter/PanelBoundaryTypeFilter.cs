using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Analytical
{
    public class PanelBoundaryTypeFilter : EnumFilter<BoundaryType>, IAdjacencyClusterFilter
    {
        public AdjacencyCluster AdjacencyCluster { get; set; }

        public PanelBoundaryTypeFilter(BoundaryType boundaryType)
            :base()
        {
            Enum = boundaryType;
        }

        public PanelBoundaryTypeFilter(JObject jObject)
            :base(jObject)
        {

        }

        public PanelBoundaryTypeFilter(PanelBoundaryTypeFilter panelBoundaryTypeFilter)
            :base(panelBoundaryTypeFilter)
        {

        }


        public override bool TryGetEnum(IJSAMObject jSAMObject, out BoundaryType boundaryType)
        {
            boundaryType = BoundaryType.Undefined;

            if(AdjacencyCluster == null || jSAMObject == null)
            {
                return false;
            }

            Panel panel = jSAMObject as Panel;
            if(panel == null)
            {
                return false;
            }

            boundaryType = AdjacencyCluster.BoundaryType(panel);
            return true;
        }
    }
}