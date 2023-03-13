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

        public override bool IsValid(IJSAMObject jSAMObject)
        {
            Panel panel = jSAMObject as Panel;
            if(panel == null)
            {
                return false;
            }

            double tilt = Query.Tilt(panel);
            if(double.IsNaN(tilt))
            {
                return false;
            }

            return Core.Query.Compare(tilt, Value, NumberComparisonType);
            
        }
    }
}