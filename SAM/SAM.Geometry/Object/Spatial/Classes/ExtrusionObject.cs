using Newtonsoft.Json.Linq;
using SAM.Core;
using SAM.Geometry.Spatial;

namespace SAM.Geometry.Object.Spatial
{
    public class ExtrusionObject : Extrusion, IExtrusionObject, ITaggable
    {
        public SurfaceAppearance SurfaceAppearance { get; set; }

        public Extrusion Extrusion
        {
            get
            {
                return new Extrusion(this);
            }
        }

        public Tag Tag { get; set; }

        public ExtrusionObject(Extrusion extrusion)
            : base(extrusion)
        {

        }

        public ExtrusionObject(JObject jObject)
            : base(jObject)
        {

        }

        public ExtrusionObject(ExtrusionObject extrusionObject)
                : base(extrusionObject)
        {
            if (extrusionObject?.SurfaceAppearance != null)
            {
                SurfaceAppearance = new SurfaceAppearance(extrusionObject?.SurfaceAppearance);
            }

            Tag = extrusionObject?.Tag;
        }

        public ExtrusionObject(Extrusion extrusion, SurfaceAppearance surfaceAppearance)
            : base(extrusion)
        {
            if (surfaceAppearance != null)
            {
                SurfaceAppearance = new SurfaceAppearance(surfaceAppearance);
            }
        }

        public ExtrusionObject(Extrusion extrusion, System.Drawing.Color surfaceColor, System.Drawing.Color curveColor, double curveThickness)
            : base(extrusion)
        {
            SurfaceAppearance = new SurfaceAppearance(surfaceColor, curveColor, curveThickness);
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
            {
                return false;
            }

            if (jObject.ContainsKey("SurfaceAppearance"))
            {
                SurfaceAppearance = new SurfaceAppearance(jObject.Value<JObject>("SurfaceAppearance"));
            }
            Tag = Core.Query.Tag(jObject);

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
            {
                return null;
            }

            if (SurfaceAppearance != null)
            {
                jObject.Add("SurfaceAppearance", SurfaceAppearance.ToJObject());
            }

            Core.Modify.Add(jObject, Tag);

            return jObject;
        }
    }
}
