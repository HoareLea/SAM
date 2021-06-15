using Newtonsoft.Json.Linq;

using SAM.Geometry.Spatial;

namespace SAM.Architectural
{
    public abstract class Opening : BuildingElement
    {
        public Opening(Opening opening)
            : base(opening)
        {

        }

        public Opening(JObject jObject)
            : base(jObject)
        {

        }

        public Opening(OpeningType openingType, Face3D face3D)
            : base(openingType, face3D)
        {

        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
            {
                return false;
            }

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();

            if (jObject == null)
            {
                return jObject;
            }

            return jObject;
        }

    }
}
