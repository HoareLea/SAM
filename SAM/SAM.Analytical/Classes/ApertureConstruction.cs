using System;

using Newtonsoft.Json.Linq;

using SAM.Core;

namespace SAM.Analytical
{
    public class ApertureConstruction : SAMType
    {
        private ApertureType apertureType;
        private Boundary2D boundary2D;
        private Construction frameConstruction;
        private Construction paneConstruction;

        public ApertureConstruction(string name, ApertureType apertureType)
            : base(name)
        {
            this.apertureType = apertureType;
        }

        public ApertureConstruction(string name, Geometry.Planar.IClosed2D edge, ApertureType apertureType)
            : base(name)
        {
            boundary2D = new Boundary2D(edge);
            this.apertureType = apertureType;
        }

        public ApertureConstruction(Guid guid, string name, ApertureType apertureType)
            : base(guid, name)
        {
            this.apertureType = apertureType;
        }

        public ApertureConstruction(ApertureConstruction apertureConstruction)
            : base(apertureConstruction)
        {
            apertureType = apertureConstruction.apertureType;
            boundary2D = new Boundary2D(apertureConstruction.boundary2D);
            frameConstruction = apertureConstruction.frameConstruction;
            paneConstruction = apertureConstruction.paneConstruction;
        }

        public ApertureConstruction(JObject jObject)
            : base(jObject)
        {
            FromJObject(jObject);
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
                if(frameConstruction == null)
                    return null;
                
                return new Construction(frameConstruction);
            }
        }

        public Construction PaneConstruction
        {
            get
            {
                if (paneConstruction == null)
                    return null;
                
                return new Construction(paneConstruction);
            }
        }

        public ApertureType ApertureType
        {
            get
            {
                return apertureType;
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            if (jObject.ContainsKey("Boundary2D"))
                boundary2D = new Boundary2D(jObject.Value<JObject>("Boundary2D"));

            if (jObject.ContainsKey("FrameConstruction"))
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

            if (boundary2D != null)
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
