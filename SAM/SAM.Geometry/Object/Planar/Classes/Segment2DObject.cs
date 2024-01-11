using Newtonsoft.Json.Linq;
using SAM.Core;
using SAM.Geometry.Planar;

namespace SAM.Geometry.Object.Planar
{
    public class Segment2DObject : Segment2D, ISegment2DObject, ITaggable
    {
        public CurveAppearance CurveAppearance { get; set; }

        public Segment2D Segment2D
        {
            get
            {
                return new Segment2D(this);
            }
        }

        public Tag Tag { get; set; }

        public Segment2DObject(Segment2D segment2D)
            : base(segment2D)
        {

        }

        public Segment2DObject(Segment2D segment2D, CurveAppearance curveAppearance)
            : base(segment2D)
        {
            if (curveAppearance != null)
            {
                CurveAppearance = new CurveAppearance(curveAppearance);
            }
        }

        public Segment2DObject(JObject jObject)
            :base(jObject)
        {

        }

        public Segment2DObject(Segment2DObject segment2DObject)
            : base(segment2DObject)
        {
            if (segment2DObject.CurveAppearance != null)
            {
                CurveAppearance = new CurveAppearance(segment2DObject.CurveAppearance);
            }

            Tag = segment2DObject?.Tag;
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
