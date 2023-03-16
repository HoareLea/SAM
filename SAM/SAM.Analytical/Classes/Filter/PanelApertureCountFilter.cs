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

        public override bool TryGetNumber(IJSAMObject jSAMObject, out double number)
        {
            number = double.NaN;
            Panel panel = jSAMObject as Panel;
            if (panel == null)
            {
                return false;
            }

            List<Aperture> apertures = panel.Apertures;

            number = apertures == null ? 0 : apertures.Count;
            return true;
        }
    }
}