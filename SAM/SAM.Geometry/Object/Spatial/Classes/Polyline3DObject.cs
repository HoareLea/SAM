using Newtonsoft.Json.Linq;
using SAM.Core;
using SAM.Geometry.Spatial;

namespace SAM.Geometry.Object.Spatial
{
    public class Polyline3DObject : Polyline3D, IPolyline3DObject, ITaggable
    {
        public CurveAppearance CurveAppearance { get; set; }

        public Polyline3D Polyline3D
        {
            get
            {
                return new Polyline3D(this);
            }
        }

        public Tag Tag { get; set; }

        public Polyline3DObject(Polyline3D polyline3D)
            : base(polyline3D)
        {

        }

        public Polyline3DObject(JObject jObject)
            : base(jObject)
        {

        }

        public Polyline3DObject(Polyline3DObject polyline3DObject)
                : base(polyline3DObject)
        {
            if (polyline3DObject?.CurveAppearance != null)
            {
                CurveAppearance = new CurveAppearance(polyline3DObject?.CurveAppearance);
            }

            Tag = polyline3DObject?.Tag;
        }

        public Polyline3DObject(Polyline3D polyline3D, CurveAppearance curveAppearance)
            : base(polyline3D)
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
