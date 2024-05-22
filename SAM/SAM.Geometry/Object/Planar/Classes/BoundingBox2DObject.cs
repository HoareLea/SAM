using Newtonsoft.Json.Linq;
using SAM.Core;
using SAM.Geometry.Planar;

namespace SAM.Geometry.Object.Planar
{
    public class BoundingBox2DObject : BoundingBox2D, IBoundingBox2DObject, ITaggable
    {
        public CurveAppearance CurveAppearance { get; set; }

        public BoundingBox2D BoundingBox2D
        {
            get
            {
                return new BoundingBox2D(this);
            }
        }

        public Tag Tag { get; set; }

        public BoundingBox2DObject(BoundingBox2D boundingBox2D)
            : base(boundingBox2D)
        {

        }

        public BoundingBox2DObject(JObject jObject)
            : base(jObject)
        {

        }

        public BoundingBox2DObject(BoundingBox2DObject boundingBox2DObject)
                : base(boundingBox2DObject)
        {
            if (boundingBox2DObject?.CurveAppearance != null)
            {
                CurveAppearance = new CurveAppearance(boundingBox2DObject?.CurveAppearance);
            }

            Tag = boundingBox2DObject?.Tag;
        }

        public BoundingBox2DObject(BoundingBox2D boundingBox2D, CurveAppearance curveAppearance)
            : base(boundingBox2D)
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
