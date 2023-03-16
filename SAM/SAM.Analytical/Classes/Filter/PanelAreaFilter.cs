using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Analytical
{
    public class PanelAreaFilter : NumberFilter
    {
        public PanelAreaFilter(NumberComparisonType numberComparisonType, double value)
            : base(numberComparisonType, value)
        {

        }

        public PanelAreaFilter(PanelAreaFilter panelAreaFilter)
            : base(panelAreaFilter)
        {

        }

        public PanelAreaFilter(JObject jObject)
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

            number = panel.GetAreaNet();
            return !double.IsNaN(number);
        }
    }
}