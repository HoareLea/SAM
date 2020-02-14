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
            plane = new Plane(aperture.plane);
        }

        public Aperture(JObject jObject)
            : base(jObject)
        {

        }

        public Aperture(ApertureType apertureType, Plane plane)
            : base(System.Guid.NewGuid(), apertureType)
        {
            this.plane = new Plane(plane);
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

        public Face3D GetFace3D()
        {
            if (plane == null)
                return null;

            ApertureType apertureType = ApertureType;
            if (apertureType == null)
                return null;

            return ApertureType.Boundary2D.GetFace3D(plane);
        }

        public Aperture Clone()
        {
            return new Aperture(this);
        }

        public ApertureType ApertureType
        {
            get
            {
                return SAMType as ApertureType;
            }
        }

        public BoundingBox3D GetBoundingBox(double offset = 0)
        {
            return GetFace3D().GetBoundingBox(offset);
        }

        public PlanarBoundary3D GetPlanarBoundary3D()
        {
            if (plane == null)
                return null;

            ApertureType apertureType = ApertureType;
            if (apertureType == null)
                return null;

            return apertureType.GetPlanarBoundary3D(plane);
        }
    }
}
