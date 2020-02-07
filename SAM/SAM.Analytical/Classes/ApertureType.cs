using System;

using Newtonsoft.Json.Linq;

using SAM.Core;

namespace SAM.Analytical
{
    public class ApertureType : SAMType
    {
        private PlanarBoundary3D planarBoundary3D;
        private Construction frameConstruction;
        private Construction paneConstruction;

        public ApertureType(string name) 
            : base(name)
        {
        }

        public ApertureType(Guid guid, string name) 
            : base(guid, name)
        {
        }

        public ApertureType(ApertureType apertureType)
            : base(apertureType)
        {
        }

        public ApertureType(JObject jObject)
            : base(jObject)
        {

        }

        public PlanarBoundary3D PlanarBoundary3D
        {
            get
            {
                return new PlanarBoundary3D(planarBoundary3D);
            }
        }

        public Construction FrameConstruction
        {
            get
            {
                return new Construction(frameConstruction);
            }
        }

        public Construction PaneConstruction
        {
            get
            {
                return new Construction(paneConstruction);
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            planarBoundary3D = new PlanarBoundary3D(jObject.Value<JObject>("PlanarBoundary3D"));
            frameConstruction = new Construction(jObject.Value<JObject>("FrameConstruction"));
            paneConstruction = new Construction(jObject.Value<JObject>("PaneConstruction"));
            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return jObject;

            jObject.Add("PlanarBoundary3D", planarBoundary3D.ToJObject());
            jObject.Add("FrameConstruction", frameConstruction.ToJObject());
            jObject.Add("PaneConstruction", paneConstruction.ToJObject());
            return jObject;
        }
    }
}
