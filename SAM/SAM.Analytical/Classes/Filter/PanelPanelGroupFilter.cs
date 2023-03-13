using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Analytical
{
    public class PanelPanelGroupFilter : EnumFilter<PanelGroup>
    {

        public PanelPanelGroupFilter(PanelGroup panelGroup)
            :base()
        {
            Value = panelGroup;
        }

        public PanelPanelGroupFilter(PanelPanelGroupFilter panelPanelGroupFilter)
            :base(panelPanelGroupFilter)
        {

        }

        public PanelPanelGroupFilter(JObject jObject)
            :base(jObject)
        {

        }

        public override bool TryGetEnum(IJSAMObject jSAMObject, out PanelGroup panelGroup)
        {
            panelGroup = PanelGroup.Undefined;

            Panel panel = jSAMObject as Panel;
            if(panel == null)
            {
                return false;
            }

            panelGroup = panel.PanelGroup;
            return true;
        }
    }
}