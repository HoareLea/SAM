using Newtonsoft.Json.Linq;

using SAM.Geometry.Spatial;

namespace SAM.Analytical
{
    public class Opening<T> : BuildingElement<T>, IOpening  where T : OpeningType
    {
        public Opening(Opening<T> opening)
            : base(opening)
        {

        }

        public Opening(JObject jObject)
            : base(jObject)
        {

        }

        public Opening(T openingType, Face3D face3D)
            : base(openingType, face3D)
        {

        }

        public Opening(System.Guid guid, T openingType, Face3D face3D)
            : base(guid, openingType, face3D)
        {

        }

        public Opening(System.Guid guid, Opening<T> opening, Face3D face3D)
            : base(guid, opening, face3D)
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
