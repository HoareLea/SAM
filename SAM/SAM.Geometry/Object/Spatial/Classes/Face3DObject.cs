using Newtonsoft.Json.Linq;
using SAM.Core;
using SAM.Geometry.Spatial;

namespace SAM.Geometry.Object.Spatial
{
    public class Face3DObject : Face3D, IFace3DObject, ITaggable
    {
        public SurfaceAppearance SurfaceAppearance { get; set; }

        public Face3D Face3D
        {
            get
            {
                return new Face3D(this);
            }
        }

        public Tag Tag { get; set; }

        public Face3DObject(Face3D face3D)
            : base(face3D)
        {

        }

        public Face3DObject(JObject jObject)
            : base(jObject)
        {

        }

        public Face3DObject(Face3DObject face3DObject)
            : base(face3DObject)
        {
            if (face3DObject?.SurfaceAppearance != null)
            {
                SurfaceAppearance = new SurfaceAppearance(face3DObject?.SurfaceAppearance);
            }

            Tag = face3DObject?.Tag;
        }

        public Face3DObject(Face3D face3D, SurfaceAppearance surfaceAppearance)
            : base(face3D)
        {
            if(surfaceAppearance != null)
            {
                SurfaceAppearance = new SurfaceAppearance(surfaceAppearance);
            }
        }

        public Face3DObject(Face3D face3D, System.Drawing.Color surfaceColor, System.Drawing.Color curveColor, double curveThickness)
            : base(face3D)
        {
            SurfaceAppearance = new SurfaceAppearance(surfaceColor, curveColor, curveThickness);
        }

        public override bool FromJObject(JObject jObject)
        {
            if(!base.FromJObject(jObject))
            {
                return false;
            }

            if(jObject.ContainsKey("SurfaceAppearance"))
            {
                SurfaceAppearance = new SurfaceAppearance(jObject.Value<JObject>("SurfaceAppearance"));
            }
            Tag = Core.Query.Tag(jObject);

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if(jObject == null)
            {
                return null;
            }

            if(SurfaceAppearance != null)
            {
                jObject.Add("SurfaceAppearance", SurfaceAppearance.ToJObject());
            }

            Core.Modify.Add(jObject, Tag);

            return jObject;
        }
    }
}
