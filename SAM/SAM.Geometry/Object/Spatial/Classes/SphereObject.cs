using Newtonsoft.Json.Linq;
using SAM.Core;
using SAM.Geometry.Spatial;

namespace SAM.Geometry.Object.Spatial
{
    public class SphereObject : Sphere, ISphereObject, ITaggable
    {
        public SurfaceAppearance SurfaceAppearance { get; set; }

        public Sphere Sphere
        {
            get
            {
                return new Sphere(this);
            }
        }

        public Tag Tag { get; set; }

        public SphereObject(Sphere sphere)
            : base(sphere)
        {

        }

        public SphereObject(JObject jObject)
            : base(jObject)
        {

        }

        public SphereObject(SphereObject sphereObject)
            : base(sphereObject)
        {
            if (sphereObject?.SurfaceAppearance != null)
            {
                SurfaceAppearance = new SurfaceAppearance(sphereObject?.SurfaceAppearance);
            }

            Tag = sphereObject?.Tag;
        }

        public SphereObject(Sphere sphere, SurfaceAppearance surfaceAppearance)
            : base(sphere)
        {
            if(surfaceAppearance != null)
            {
                SurfaceAppearance = new SurfaceAppearance(surfaceAppearance);
            }
        }

        public SphereObject(Sphere sphere, System.Drawing.Color surfaceColor, System.Drawing.Color curveColor, double curveThickness)
            : base(sphere)
        {
            SurfaceAppearance = new SurfaceAppearance(surfaceColor, curveColor, curveThickness);
        }

        public override bool FromJObject(JObject jObject)
        {
            if(!base.FromJObject(jObject))
            {
                return false;
            }

            if(jObject.ContainsKey("SurfaceAppearance"))
            {
                SurfaceAppearance = new SurfaceAppearance(jObject.Value<JObject>("SurfaceAppearance"));
            }
            Tag = Core.Query.Tag(jObject);

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if(jObject == null)
            {
                return null;
            }

            if(SurfaceAppearance != null)
            {
                jObject.Add("SurfaceAppearance", SurfaceAppearance.ToJObject());
            }

            Core.Modify.Add(jObject, Tag);

            return jObject;
        }
    }
}
