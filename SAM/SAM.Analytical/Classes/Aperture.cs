using Newtonsoft.Json.Linq;

using SAM.Core;
using SAM.Geometry.Spatial;


namespace SAM.Analytical
{
    public class Aperture : SAMInstance
    {
        private Plane plane;

        public Aperture(Aperture aperture)
            : base(aperture)
        {
            this.plane = aperture.plane;
        }

        public Aperture(JObject jObject)
            : base(jObject)
        {

        }

        public Aperture(ApertureType apertureType, Plane plane)
            : base(System.Guid.NewGuid(), apertureType)
        {
            this.plane = plane;
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            plane = new Plane(jObject.Value<JObject>("Plane"));
            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return jObject;

            jObject.Add("Plane", plane.ToJObject());

            return jObject;
        }

        public Aperture Clone()
        {
            return new Aperture(this);
        }
    }
}
