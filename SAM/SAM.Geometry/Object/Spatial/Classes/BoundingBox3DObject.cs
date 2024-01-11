using Newtonsoft.Json.Linq;
using SAM.Core;
using SAM.Geometry.Spatial;

namespace SAM.Geometry.Object.Spatial
{
    public class BoundingBox3DObject : BoundingBox3D, IBoundingBox3DObject, ITaggable
    {
        public CurveAppearance CurveAppearance { get; set; }

        public BoundingBox3D BoundingBox3D
        {
            get
            {
                return new BoundingBox3D(this);
            }
        }

        public Tag Tag { get; set; }

        public BoundingBox3DObject(BoundingBox3D boundingBox3D)
            : base(boundingBox3D)
        {

        }

        public BoundingBox3DObject(JObject jObject)
            : base(jObject)
        {

        }

        public BoundingBox3DObject(BoundingBox3DObject boundingBox3DObject)
                : base(boundingBox3DObject)
        {
            if (boundingBox3DObject?.CurveAppearance != null)
            {
                CurveAppearance = new CurveAppearance(boundingBox3DObject?.CurveAppearance);
            }

            Tag = boundingBox3DObject?.Tag;
        }

        public BoundingBox3DObject(BoundingBox3D boundingBox3D, CurveAppearance curveAppearance)
            : base(boundingBox3D)
        {
            if (curveAppearance != null)
            {
                CurveAppearance = new CurveAppearance(curveAppearance);
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

            Core.Modify.Add(jObject, Tag);

            return jObject;
        }
    }
}
