using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Analytical
{
    public class ApertureFrameAreaFilter : NumberFilter
    {
        public ApertureFrameAreaFilter(NumberComparisonType numberComparisonType, double value)
            : base(numberComparisonType, value)
        {

        }

        public ApertureFrameAreaFilter(ApertureFrameAreaFilter apertureFrameAreaFilter)
            : base(apertureFrameAreaFilter)
        {

        }

        public ApertureFrameAreaFilter(JObject jObject)
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

            number = aperture.GetFrameArea();
            return !double.IsNaN(number);
        }
    }
}