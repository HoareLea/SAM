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

        public override bool IsValid(IJSAMObject jSAMObject)
        {
            Aperture aperture = jSAMObject as Aperture;
            if(aperture == null)
            {
                return false;
            }

            double area = aperture.GetFrameArea();
            if(double.IsNaN(area))
            {
                return false;
            }

            return Core.Query.Compare(area, Value, NumberComparisonType);
            
        }
    }
}