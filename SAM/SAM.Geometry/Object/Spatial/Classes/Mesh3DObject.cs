using Newtonsoft.Json.Linq;
using SAM.Core;
using SAM.Geometry.Spatial;

namespace SAM.Geometry.Object.Spatial
{
    public class Mesh3DObject : Mesh3D, IMesh3DObject, ITaggable
    {
        public SurfaceAppearance SurfaceAppearance { get; set; }

        public Mesh3D Mesh3D
        {
            get
            {
                return new Mesh3D(this);
            }
        }

        public Tag Tag { get; set; }

        public Mesh3DObject(Mesh3D mesh3D)
            : base(mesh3D)
        {

        }

        public Mesh3DObject(JObject jObject)
            : base(jObject)
        {

        }

        public Mesh3DObject(Mesh3DObject mesh3DObject)
            : base(mesh3DObject)
        {
            if (mesh3DObject?.SurfaceAppearance != null)
            {
                SurfaceAppearance = new SurfaceAppearance(mesh3DObject?.SurfaceAppearance);
            }

            Tag = mesh3DObject?.Tag;
        }

        public Mesh3DObject(Mesh3D mesh3D, SurfaceAppearance surfaceAppearance)
            : base(mesh3D)
        {
            if(surfaceAppearance != null)
            {
                SurfaceAppearance = new SurfaceAppearance(surfaceAppearance);
            }
        }

        public Mesh3DObject(Mesh3D mesh3D, System.Drawing.Color surfaceColor, System.Drawing.Color curveColor, double curveThickness)
            : base(mesh3D)
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
