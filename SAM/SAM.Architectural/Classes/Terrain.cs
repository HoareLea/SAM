using Newtonsoft.Json.Linq;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Architectural
{
    public abstract class Terrain : Core.SAMObject, ITerrain
    {
        private List<MaterialLayer> materialLayers;

        public Terrain()
            : base()
        {

        }

        public Terrain(Terrain terrain)
            : base(terrain)
        {

        }

        public Terrain(JObject jObject)
            : base(jObject)
        {

        }

        public abstract bool Below(Face3D face3D, double tolerance = Core.Tolerance.Distance);

        public bool Below(IPartition partition, double tolerance = Core.Tolerance.Distance)
        {
            return Below(partition?.Face3D, tolerance);
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
            {
                return false;
            }

            if (jObject.ContainsKey("MaterialLayers"))
            {
                materialLayers = Core.Create.IJSAMObjects<MaterialLayer>(jObject.Value<JArray>("MaterialLayers"));
            }

            return true;
        }

        public abstract bool On(Face3D face3D, double tolerance = Core.Tolerance.Distance);

        public bool On(IPartition partition, double tolerance = Core.Tolerance.Distance)
        {
            return On(partition?.Face3D, tolerance);
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();

            if (jObject == null)
            {
                return jObject;
            }

            if (materialLayers != null)
            {
                jObject.Add("MaterialLayers", Core.Create.JArray(materialLayers));
            }

            return jObject;
        }

    }
}
