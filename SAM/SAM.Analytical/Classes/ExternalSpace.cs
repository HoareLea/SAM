using Newtonsoft.Json.Linq;
using SAM.Core;
using SAM.Geometry.Spatial;
using System;

namespace SAM.Analytical
{
    public class ExternalSpace : SAMObject, ISpace
    {
        private Point3D location;

        public Point3D Location
        {
            get
            {
                return location == null ? null : new Point3D(location);
            }
        }

        public ExternalSpace(string name, Point3D location)
            : base(name)
        {
            this.location = location == null ? null : new Point3D(Location);
        }

        public ExternalSpace(Guid guid, string name)
            : base(guid, name)
        {

        }

        public ExternalSpace(ExternalSpace externalSpace, string name)
            : base(name, externalSpace)
        {
            location = externalSpace?.Location?.Clone<Point3D>();
        }

        public ExternalSpace(ExternalSpace externalSpace)
            : base(externalSpace)
        {
            location = externalSpace?.Location?.Clone<Point3D>();
        }

        public ExternalSpace(JObject jObject)
            : base(jObject)
        {
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
            {
                return false;
            }

            if(jObject.ContainsKey("Location"))
            {
                location = new Point3D(jObject.Value<JObject>("Location"));
            }

            return true;
        }

        public override JObject ToJObject()
        {
           JObject jObject = base.ToJObject();
            if (jObject == null)
            {
                return null;
            }

            if(location != null)
            {
                jObject.Add("Location", location.ToJObject());
            }

            return jObject;
        }
    }
}