using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Analytical
{
    /// <summary>
    /// A filter that operates on a collection of ApertureConstruction objects related to an Aperture object.
    /// </summary>
    public class ApertureApertureConstructionFilter : RelationFilter<ApertureConstruction>
    {
        /// <summary>
        /// Initializes a new instance of the ApertureApertureConstructionFilter class with the specified filter.
        /// </summary>
        /// <param name="filter">The filter to use.</param>
        public ApertureApertureConstructionFilter(IFilter filter)
            : base(filter)
        {

        }

        /// <summary>
        /// Initializes a new instance of the ApertureApertureConstructionFilter class with the specified ApertureApertureConstructionFilter.
        /// </summary>
        /// <param name="apertureApertureConstructionFilter">The ApertureApertureConstructionFilter to use.</param>
        public ApertureApertureConstructionFilter(ApertureApertureConstructionFilter apertureApertureConstructionFilter)
            : base(apertureApertureConstructionFilter)
        {

        }

        /// <summary>
        /// Initializes a new instance of the ApertureApertureConstructionFilter class with the specified JSON object.
        /// </summary>
        /// <param name="jObject">The JSON object to use.</param>
        public ApertureApertureConstructionFilter(JObject jObject)
            : base(jObject)
        {

        }

        /// <summary>
        /// Gets the ApertureConstruction object related to the specified IJSAMObject object.
        /// </summary>
        /// <param name="jSAMObject">The IJSAMObject object to get the ApertureConstruction from.</param>
        /// <returns>The ApertureConstruction object related to the IJSAMObject object.</returns>
        public override ApertureConstruction GetRelative(IJSAMObject jSAMObject)
        {
            Aperture aperture = jSAMObject as Aperture;
            if (aperture == null) // if the object is not an Aperture, return null
            {
                return null;
            }

            return aperture.ApertureConstruction; // return the ApertureConstruction related to the Aperture object
        }
    }
}
