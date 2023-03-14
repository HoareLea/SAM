using Newtonsoft.Json.Linq;
using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public class PanelApertureCountFilter : NumberFilter
    {
        public PanelApertureCountFilter(NumberComparisonType numberComparisonType, double value)
            : base(numberComparisonType, value)
        {

        }

        public PanelApertureCountFilter(PanelApertureCountFilter panelApertureCountFilter)
            : base(panelApertureCountFilter)
        {

        }

        public PanelApertureCountFilter(JObject jObject)
            : base(jObject)
        {

        }

        public override bool IsValid(IJSAMObject jSAMObject)
        {
            Panel panel = jSAMObject as Panel;
            if(panel == null)
            {
                return false;
            }

            List<Aperture> apertures = panel.Apertures;

            double count = apertures == null ? 0 : apertures.Count;

            return Core.Query.Compare(count, Value, NumberComparisonType);
        }
    }
}