using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Analytical
{
    public class AperturePaneAreaFilter : NumberFilter
    {
        public AperturePaneAreaFilter(NumberComparisonType numberComparisonType, double value)
            : base(numberComparisonType, value)
        {

        }

        public AperturePaneAreaFilter(AperturePaneAreaFilter aperturePaneAreaFilter)
            : base(aperturePaneAreaFilter)
        {

        }

        public AperturePaneAreaFilter(JObject jObject)
            : base(jObject)
        {

        }

        public override bool IsValid(IJSAMObject jSAMObject)
        {
            Aperture aperture = jSAMObject as Aperture;
            if(aperture == null)
            {
                return false;
            }

            double area = aperture.GetPaneArea();
            if(double.IsNaN(area))
            {
                return false;
            }

            return Core.Query.Compare(area, Value, NumberComparisonType);
            
        }
    }
}