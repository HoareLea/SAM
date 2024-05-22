using Newtonsoft.Json.Linq;
using SAM.Core;
using SAM.Geometry.Planar;

namespace SAM.Geometry.Object.Planar
{
    public class Polyline2DObject : Polyline2D, IPolyline2DObject, ITaggable, IBoundable2DObject
    {
        public CurveAppearance CurveAppearance { get; set; }

        public Polyline2D Polyline2D
        {
            get
            {
                return new Polyline2D(this);
            }
        }

        public Tag Tag { get; set; }

        public Polyline2DObject(Polyline2D polyline2D)
            : base(polyline2D)
        {

        }

        public Polyline2DObject(JObject jObject)
            : base(jObject)
        {

        }

        public Polyline2DObject(Polyline2DObject polyline2DObject)
                : base(polyline2DObject)
        {
            if (polyline2DObject?.CurveAppearance != null)
            {
                CurveAppearance = new CurveAppearance(polyline2DObject?.CurveAppearance);
            }

            Tag = polyline2DObject?.Tag;
        }

        public Polyline2DObject(Polyline2D polyline2D, CurveAppearance curveAppearance)
            : base(polyline2D)
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
