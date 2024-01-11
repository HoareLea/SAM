using Newtonsoft.Json.Linq;
using SAM.Core;
using SAM.Geometry.Spatial;

namespace SAM.Geometry.Object.Spatial
{
    public class SAMGeometry3DGroupObject : SAMGeometry3DGroup, ISAMGeometry3DGroupObject, ITaggable
    {
        public CurveAppearance CurveAppearance { get; set; }
        public SurfaceAppearance SurfaceAppearance { get; set; }
        public PointAppearance PointAppearance { get; set; }

        public SAMGeometry3DGroup SAMGeometry3DGroup
        {
            get
            {
                return new SAMGeometry3DGroup(this);
            }
        }

        public Tag Tag { get; set; }

        public SAMGeometry3DGroupObject(SAMGeometry3DGroup sAMGeometry3DGroup)
            : base(sAMGeometry3DGroup)
        {

        }

        public SAMGeometry3DGroupObject(JObject jObject)
            : base(jObject)
        {

        }

        public SAMGeometry3DGroupObject(SAMGeometry3DGroupObject sAMGeometry3DGroupObject)
                : base(sAMGeometry3DGroupObject)
        {
            if (sAMGeometry3DGroupObject?.CurveAppearance != null)
            {
                CurveAppearance = new CurveAppearance(sAMGeometry3DGroupObject?.CurveAppearance);
            }

            if (sAMGeometry3DGroupObject?.SurfaceAppearance != null)
            {
                SurfaceAppearance = new SurfaceAppearance(sAMGeometry3DGroupObject?.SurfaceAppearance);
            }

            if (sAMGeometry3DGroupObject?.PointAppearance != null)
            {
                PointAppearance = new PointAppearance(sAMGeometry3DGroupObject?.PointAppearance);
            }

            Tag = sAMGeometry3DGroupObject?.Tag;
        }

        public SAMGeometry3DGroupObject(SAMGeometry3DGroup sAMGeometry3DGroup, PointAppearance pointAppearance, CurveAppearance curveAppearance, SurfaceAppearance surfaceAppearance)
            : base(sAMGeometry3DGroup)
        {
            if (pointAppearance != null)
            {
                PointAppearance = new PointAppearance(pointAppearance);
            }

            if (curveAppearance != null)
            {
                CurveAppearance = new CurveAppearance(curveAppearance);
            }

            if (surfaceAppearance != null)
            {
                SurfaceAppearance = new SurfaceAppearance(surfaceAppearance);
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
            {
                return false;
            }

            if (jObject.ContainsKey("CurveAppearance"))
            {
                CurveAppearance = new CurveAppearance(jObject.Value<JObject>("CurveAppearance"));
            }

            if (jObject.ContainsKey("PointAppearance"))
            {
                PointAppearance = new PointAppearance(jObject.Value<JObject>("PointAppearance"));
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

            if (CurveAppearance != null)
            {
                jObject.Add("CurveAppearance", CurveAppearance.ToJObject());
            }

            if (SurfaceAppearance != null)
            {
                jObject.Add("SurfaceAppearance", SurfaceAppearance.ToJObject());
            }

            if (PointAppearance != null)
            {
                jObject.Add("PointAppearance", PointAppearance.ToJObject());
            }

            Core.Modify.Add(jObject, Tag);

            return jObject;
        }
    }
}
