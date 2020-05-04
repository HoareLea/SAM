using Newtonsoft.Json.Linq;
using SAM.Core;
using System;

namespace SAM.Analytical
{
    public class Space : SAMObject
    {
        private Geometry.Spatial.Point3D location;

        public Space(Space space)
            : base(space)
        {
            this.location = space.Location;
        }

        public Space(Guid guid, Space space)
        : base(guid, space)
        {
            this.location = space.Location;
        }

        public Space(Guid guid, string name, Geometry.Spatial.Point3D location)
            : base(guid, name)
        {
            this.location = location;
        }

        public Space(string name)
            : base(name)
        {
        }

        public Space(string name, Geometry.Spatial.Point3D location)
            : base(name)
        {
            this.location = location;
        }

        public Space(JObject jObject)
            : base(jObject)
        {
        }

        public Geometry.Spatial.Point3D Location
        {
            get
            {
                return location;
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            location = new Geometry.Spatial.Point3D(jObject.Value<JObject>("Location"));
            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return jObject;

            if (location != null)
                jObject.Add("Location", location.ToJObject());

            return jObject;
        }
    }
}