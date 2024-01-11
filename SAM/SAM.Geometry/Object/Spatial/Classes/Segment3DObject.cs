using Newtonsoft.Json.Linq;
using SAM.Core;
using SAM.Geometry.Spatial;

namespace SAM.Geometry.Object.Spatial
{
    public class Segment3DObject : Segment3D, ISegment3DObject, ITaggable
    {
        public CurveAppearance CurveAppearance { get; set; }

        public Segment3D Segment3D
        {
            get
            {
                return new Segment3D(this);
            }
        }

        public Tag Tag { get; set; }

        public Segment3DObject(Segment3D segment3D)
            : base(segment3D)
        {

        }

        public Segment3DObject(Segment3D segment3D, CurveAppearance curveAppearance)
            : base(segment3D)
        {
            if (curveAppearance != null)
            {
                CurveAppearance = new CurveAppearance(curveAppearance);
            }
        }

        public Segment3DObject(JObject jObject)
            :base(jObject)
        {

        }

        public Segment3DObject(Segment3DObject segment3DObject)
            : base(segment3DObject)
        {
            if (segment3DObject.CurveAppearance != null)
            {
                CurveAppearance = new CurveAppearance(segment3DObject.CurveAppearance);
            }

            Tag = segment3DObject?.Tag;
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
