using Newtonsoft.Json.Linq;

using SAM.Geometry.Spatial;

namespace SAM.Architectural
{
    public class PlanarTerrain : Terrain
    {
        private Plane plane;

        public PlanarTerrain(PlanarTerrain planarTerrain)
            : base(planarTerrain)
        {
            plane = planarTerrain?.plane == null ? null : new Plane(planarTerrain.plane);
        }

        public PlanarTerrain(JObject jObject)
            : base(jObject)
        {
        }

        public PlanarTerrain(Plane plane)
            : base()
        {
            this.plane = new Plane(plane);
        }

        public override bool Below(Face3D face3D, double tolerance = Core.Tolerance.Distance)
        {
            if (face3D == null)
            {
                return false;
            }

            return face3D.Below(plane, tolerance);
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
            {
                return false;
            }

            if (plane != null)
            {
                jObject.Add("Plane", plane.ToJObject());
            }

            return true;
        }

        public override bool On(Face3D face3D, double tolerance = Core.Tolerance.Distance)
        {
            return plane.On(face3D, tolerance);
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();

            if (jObject == null)
            {
                return jObject;
            }

            if (jObject.ContainsKey("Plane"))
            {
                plane = new Plane(jObject.Value<JObject>("Plane"));
            }

            return jObject;
        }
    }
}