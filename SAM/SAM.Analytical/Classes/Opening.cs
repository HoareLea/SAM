using System.Collections.Generic;

using Newtonsoft.Json.Linq;

using SAM.Core;
using SAM.Geometry.Spatial;


namespace SAM.Analytical
{
    public class Opening : SAMObject
    {
        private Plane plane;
        private Edge2DLoop edge2DLoop;

        public Opening(Opening opening)
            : base(opening)
        {
            this.plane = opening.plane;
            this.edge2DLoop = opening.edge2DLoop;
        }

        public Opening(JObject jObject)
            : base(jObject)
        {

        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            plane = new Plane(jObject.Value<JObject>("Plane"));
            edge2DLoop = new Edge2DLoop(jObject.Value<JObject>("Edge2DLoop"));
            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return jObject;

            jObject.Add("Plane", plane.ToJObject());
            jObject.Add("Edge2DLoop", edge2DLoop.ToJObject());

            return jObject;
        }
    }
}
