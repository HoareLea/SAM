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

        public override bool IsValid(IJSAMObject jSAMObject)
        {
            Panel panel = jSAMObject as Panel;
            if(panel == null)
            {
                return false;
            }

            double azimuth = Query.Azimuth(panel);
            if(double.IsNaN(azimuth))
            {
                return false;
            }

            return Core.Query.Compare(Value, azimuth, NumberComparisonType);
            
        }
    }
}