using Newtonsoft.Json.Linq;
using SAM.Core;
using SAM.Geometry.Spatial;

namespace SAM.Geometry.Object.Spatial
{
    public class Rectangle3DObject : Rectangle3D, IRectangle3DObject, ITaggable
    {
        public CurveAppearance CurveAppearance { get; set; }

        public Rectangle3D Rectangle3D
        {
            get
            {
                return new Rectangle3D(this);
            }
        }

        public Tag Tag { get; set; }

        public Rectangle3DObject(Rectangle3D rectangle3D)
            : base(rectangle3D)
        {

        }

        public Rectangle3DObject(JObject jObject)
            : base(jObject)
        {

        }

        public Rectangle3DObject(Rectangle3DObject rectangle3DObject)
                : base(rectangle3DObject)
        {
            if (rectangle3DObject?.CurveAppearance != null)
            {
                CurveAppearance = new CurveAppearance(rectangle3DObject?.CurveAppearance);
            }

            Tag = rectangle3DObject?.Tag;
        }

        public Rectangle3DObject(Rectangle3D rectangle3D, CurveAppearance curveAppearance)
            : base(rectangle3D)
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
