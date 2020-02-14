using System;

using Newtonsoft.Json.Linq;

using SAM.Core;

namespace SAM.Analytical
{
    public class ApertureType : SAMType
    {
        private Boundary2D boundary2D;
        private Construction frameConstruction;
        private Construction paneConstruction;

        public ApertureType(string name) 
            : base(name)
        {
        }

        public ApertureType(string name, Geometry.Planar.IClosed2D edge)
            : base(name)
        {
            boundary2D = new Boundary2D(edge);
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

        public Boundary2D Boundary2D
        {
            get
            {
                return new Boundary2D(boundary2D);
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

            boundary2D = new Boundary2D(jObject.Value<JObject>("Boundary2D"));

            if(jObject.ContainsKey("FrameConstruction"))
            frameConstruction = new Construction(jObject.Value<JObject>("FrameConstruction"));

            if (jObject.ContainsKey("PaneConstruction"))
                paneConstruction = new Construction(jObject.Value<JObject>("PaneConstruction"));

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return jObject;

            jObject.Add("Boundary2D", boundary2D.ToJObject());
            if (frameConstruction != null)

                jObject.Add("FrameConstruction", frameConstruction.ToJObject());
            if (paneConstruction != null)
                jObject.Add("PaneConstruction", paneConstruction.ToJObject());

            return jObject;
        }

        public PlanarBoundary3D GetPlanarBoundary3D(Geometry.Spatial.Plane plane)
        {
            if (boundary2D == null)
                return null;

            return boundary2D.GetPlanarBoundary3D(plane);
        }
    }
}
