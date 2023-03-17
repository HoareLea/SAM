using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Analytical
{
    public class PanelAzimuthFilter : NumberFilter
    {
        public PanelAzimuthFilter(NumberComparisonType numberComparisonType, double value)
            : base(numberComparisonType, value)
        {

        }

        public PanelAzimuthFilter(PanelAzimuthFilter panelAzimuthFilter)
            : base(panelAzimuthFilter)
        {

        }

        public PanelAzimuthFilter(JObject jObject)
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

            number = Query.Azimuth(panel);
            return !double.IsNaN(number);
        }
    }
}