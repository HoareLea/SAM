using Newtonsoft.Json.Linq;
using SAM.Core;
using SAM.Geometry.Planar;

namespace SAM.Geometry.Object.Planar
{
    public class Polygon2DObject : Polygon2D, IPolygon2DObject, ITaggable
    {
        public CurveAppearance CurveAppearance { get; set; }

        public Polygon2D Polygon2D
        {
            get
            {
                return new Polygon2D(this);
            }
        }

        public Tag Tag { get; set; }

        public Polygon2DObject(Polygon2D polygon2D)
            : base(polygon2D)
        {

        }

        public Polygon2DObject(JObject jObject)
            : base(jObject)
        {

        }

        public Polygon2DObject(Polygon2DObject polygon2DObject)
                : base(polygon2DObject)
        {
            if (polygon2DObject?.CurveAppearance != null)
            {
                CurveAppearance = new CurveAppearance(polygon2DObject?.CurveAppearance);
            }

            Tag = polygon2DObject?.Tag;
        }

        public Polygon2DObject(Polygon2D polygon2D, CurveAppearance curveAppearance)
            : base(polygon2D)
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
