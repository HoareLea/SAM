using Newtonsoft.Json.Linq;
using SAM.Core;
using SAM.Geometry.Spatial;

namespace SAM.Analytical
{
    public class ExternalPanel : SAMInstance<Construction>, IPanel
    {
        private Face3D face3D;

        public ExternalPanel(Face3D face3D, Construction construction)
            : base(construction)
        {
            this.face3D = face3D == null ? null :new Face3D(face3D);
        }

        public ExternalPanel(Face3D face3D)
            : base(null as Construction)
        {
            this.face3D = face3D == null ? null : new Face3D(face3D);
        }

        public ExternalPanel(ExternalPanel externalPanel)
            : base(externalPanel)
        {

        }

        public ExternalPanel(JObject jObject)
            : base(jObject)
        {
        }

        public Face3D Face3D
        {
            get
            {
                return face3D == null ? null : new Face3D(face3D);
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
            {
                return false;
            }

            if(jObject.ContainsKey("Face3D"))
            {
                face3D = new Face3D(jObject.Value<JObject>());
            }

            return true;
        }

        public override JObject ToJObject()
        {
           JObject jObject = base.ToJObject();
            if (jObject == null)
            {
                return null;
            }

            if(face3D != null)
            {
                jObject.Add("Face3D", face3D.ToJObject());
            }

            return jObject;
        }
    }
}