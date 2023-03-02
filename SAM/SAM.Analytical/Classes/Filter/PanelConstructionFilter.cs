using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Analytical
{
    public class PanelConstructionFilter : RelationFilter<Construction>
    {
        public PanelConstructionFilter(IFilter filter)
            :base(filter)
        {

        }

        public PanelConstructionFilter(PanelConstructionFilter panelConstructionFilter)
            : base(panelConstructionFilter)
        {

        }

        public PanelConstructionFilter(JObject jObject)
            : base(jObject)
        {

        }

        public override Construction GetRelative(IJSAMObject jSAMObject)
        {
            Panel panel = jSAMObject as Panel;
            if(panel == null)
            {
                return null;
            }

            return panel.Construction;
        }
    }
}