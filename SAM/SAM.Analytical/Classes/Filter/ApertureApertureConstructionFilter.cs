using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Analytical
{
    public class ApertureApertureConstructionFilter : RelationFilter<ApertureConstruction>
    {
        public ApertureApertureConstructionFilter(IFilter filter)
            : base(filter)
        {

        }

        public ApertureApertureConstructionFilter(ApertureApertureConstructionFilter apertureApertureConstructionFilter)
            : base(apertureApertureConstructionFilter)
        {

        }

        public ApertureApertureConstructionFilter(JObject jObject)
            : base(jObject)
        {

        }

        public override ApertureConstruction GetRelative(IJSAMObject jSAMObject)
        {
            Aperture aperture = jSAMObject as Aperture;
            if(aperture == null)
            {
                return null;
            }

            return aperture.ApertureConstruction;
        }
    }
}