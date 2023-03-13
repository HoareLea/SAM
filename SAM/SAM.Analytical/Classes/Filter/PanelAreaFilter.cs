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

        public override bool IsValid(IJSAMObject jSAMObject)
        {
            Panel panel = jSAMObject as Panel;
            if(panel == null)
            {
                return false;
            }

            double area = panel.GetAreaNet();
            if(double.IsNaN(area))
            {
                return false;
            }

            return Core.Query.Compare(area, Value, NumberComparisonType);
            
        }
    }
}