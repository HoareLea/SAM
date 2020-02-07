using Newtonsoft.Json.Linq;

using SAM.Core;
using SAM.Geometry.Spatial;


namespace SAM.Analytical
{
    public class Aperture : SAMInstance
    {
        private Point3D location;

        public Aperture(Aperture aperture)
            : base(aperture)
        {
            this.location = aperture.location;
        }

        public Aperture(JObject jObject)
            : base(jObject)
        {

        }

        public Aperture(ApertureType apertureType, Point3D location)
            : base(System.Guid.NewGuid(), apertureType)
        {
            this.location = location;
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            location = new Point3D(jObject.Value<JObject>("Location"));
            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return jObject;

            jObject.Add("Location", location.ToJObject());

            return jObject;
        }
    }
}
