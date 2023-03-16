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

        public override bool TryGetNumber(IJSAMObject jSAMObject, out double number)
        {
            number = double.NaN;
            Aperture aperture = jSAMObject as Aperture;
            if (aperture == null)
            {
                return false;
            }

            number = aperture.GetPaneArea();
            return !double.IsNaN(number);
        }
    }
}