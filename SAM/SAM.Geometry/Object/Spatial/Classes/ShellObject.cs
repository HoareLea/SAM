using Newtonsoft.Json.Linq;
using SAM.Core;
using SAM.Geometry.Spatial;

namespace SAM.Geometry.Object.Spatial
{
    public class ShellObject : Shell, IShellObject, ITaggable
    {
        public SurfaceAppearance SurfaceAppearance { get; set; }

        public Shell Shell
        {
            get
            {
                return new Shell(this);
            }
        }

        public Tag Tag { get; set; }

        public ShellObject(Shell shell)
            : base(shell)
        {

        }

        public ShellObject(JObject jObject)
            : base(jObject)
        {

        }

        public ShellObject(ShellObject shellObject)
            : base(shellObject)
        {
            if (shellObject?.SurfaceAppearance != null)
            {
                SurfaceAppearance = new SurfaceAppearance(shellObject?.SurfaceAppearance);
            }

            Tag = shellObject?.Tag;
        }

        public ShellObject(Shell shell, SurfaceAppearance surfaceAppearance)
            : base(shell)
        {
            if(surfaceAppearance != null)
            {
                SurfaceAppearance = new SurfaceAppearance(surfaceAppearance);
            }
        }

        public ShellObject(Shell shell, System.Drawing.Color surfaceColor, System.Drawing.Color curveColor, double curveThickness)
            : base(shell)
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
