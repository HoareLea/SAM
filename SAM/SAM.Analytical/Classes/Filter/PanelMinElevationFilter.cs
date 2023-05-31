using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Analytical
{
    public class PanelMinElevationFilter : NumberFilter
    {
        public PanelMinElevationFilter(NumberComparisonType numberComparisonType, double value)
            : base(numberComparisonType, value)
        {

        }

        public PanelMinElevationFilter(PanelMinElevationFilter panelMinElevationFilter)
            : base(panelMinElevationFilter)
        {

        }

        public PanelMinElevationFilter(JObject jObject)
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