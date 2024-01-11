using Newtonsoft.Json.Linq;
using SAM.Core;
using SAM.Geometry.Spatial;

namespace SAM.Geometry.Object.Spatial
{
    public class Polygon3DObject : Polygon3D, IPolygon3DObject, ITaggable
    {
        public CurveAppearance CurveAppearance { get; set; }

        public Polygon3D Polygon3D
        {
            get
            {
                return new Polygon3D(this);
            }
        }

        public Tag Tag { get; set; }

        public Polygon3DObject(Polygon3D polygon3D)
            : base(polygon3D)
        {

        }

        public Polygon3DObject(JObject jObject)
            : base(jObject)
        {

        }

        public Polygon3DObject(Polygon3DObject polygon3DObject)
                : base(polygon3DObject)
        {
            if (polygon3DObject?.CurveAppearance != null)
            {
                CurveAppearance = new CurveAppearance(polygon3DObject?.CurveAppearance);
            }

            Tag = polygon3DObject?.Tag;
        }

        public Polygon3DObject(Polygon3D polygon3D, CurveAppearance curveAppearance)
            : base(polygon3D)
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
