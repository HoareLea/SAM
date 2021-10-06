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

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
            {
                return false;
            }

            if(plane != null)
            {
                jObject.Add("Plane", plane.ToJObject());
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

            if(jObject.ContainsKey("Plane"))
            {
                plane = new Plane(jObject.Value<JObject>("Plane"));
            }

            return jObject;
        }

    }
}
