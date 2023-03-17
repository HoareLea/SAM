using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Analytical
{
    public class PanelTiltFilter : NumberFilter
    {
        public PanelTiltFilter(NumberComparisonType numberComparisonType, double value)
            : base(numberComparisonType, value)
        {

        }

        public PanelTiltFilter(PanelTiltFilter panelTiltFilter)
            : base(panelTiltFilter)
        {

        }

        public PanelTiltFilter(JObject jObject)
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

            number = Query.Tilt(panel);
            return !double.IsNaN(number);
        }
    }
}