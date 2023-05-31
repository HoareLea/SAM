using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Analytical
{
    public class PanelMaxElevationFilter : NumberFilter
    {
        public PanelMaxElevationFilter(NumberComparisonType numberComparisonType, double value)
            : base(numberComparisonType, value)
        {

        }

        public PanelMaxElevationFilter(PanelMaxElevationFilter panelMaxElevationFilter)
            : base(panelMaxElevationFilter)
        {

        }

        public PanelMaxElevationFilter(JObject jObject)
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

            number = panel.GetBoundingBox().Min.Z;
            return !double.IsNaN(number);
        }
    }
}