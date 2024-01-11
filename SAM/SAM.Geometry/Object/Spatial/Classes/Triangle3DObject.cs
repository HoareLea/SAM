using Newtonsoft.Json.Linq;
using SAM.Core;
using SAM.Geometry.Spatial;

namespace SAM.Geometry.Object.Spatial
{
    public class Triangle3DObject : Triangle3D, ITriangle3DObject, ITaggable
    {
        public CurveAppearance CurveAppearance { get; set; }

        public Triangle3D Triangle3D
        {
            get
            {
                return new Triangle3D(this);
            }
        }

        public Tag Tag { get; set; }

        public Triangle3DObject(Triangle3D triangle3D)
            : base(triangle3D)
        {

        }

        public Triangle3DObject(JObject jObject)
            : base(jObject)
        {

        }

        public Triangle3DObject(Triangle3DObject triangle3DObject)
                : base(triangle3DObject)
        {
            if (triangle3DObject?.CurveAppearance != null)
            {
                CurveAppearance = new CurveAppearance(triangle3DObject?.CurveAppearance);
            }

            Tag = triangle3DObject?.Tag;
        }

        public Triangle3DObject(Triangle3D triangle3D, CurveAppearance curveAppearance)
            : base(triangle3D)
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
