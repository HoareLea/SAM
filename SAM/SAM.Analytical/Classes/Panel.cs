using System;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

using SAM.Core;
using SAM.Geometry.Spatial;

namespace SAM.Analytical
{
    public class Panel : SAMInstance
    {
        private PanelType panelType;
        private PlanarBoundary3D planarBoundary3D;
        private List<Aperture> apertures;

        public Panel(Panel panel)
            : base(panel)
        {
            planarBoundary3D = new PlanarBoundary3D(panel.planarBoundary3D);
            panelType = panel.panelType;

            if (panel.apertures != null)
                apertures = new List<Aperture>(panel.apertures.ConvertAll(x => new Aperture(x)));
        }

        public Panel(Panel panel, Construction construction)
            : base(panel, construction)
        {
            planarBoundary3D = new PlanarBoundary3D(panel.planarBoundary3D);
            panelType = panel.panelType;

            if (panel.apertures != null)
                apertures = new List<Aperture>(panel.apertures.ConvertAll(x => new Aperture(x)));
        }

        public Panel(Panel panel, PanelType panelType)
            : base(panel)
        {
            planarBoundary3D = new PlanarBoundary3D(panel.planarBoundary3D);
            this.panelType = panelType;

            if (panel.apertures != null)
                apertures = new List<Aperture>(panel.apertures.ConvertAll(x => new Aperture(x)));
        }

        public Panel(Construction construction, PanelType panelType, Face3D face)
            : base(construction == null ? null : construction.Name, construction)
        {
            this.panelType = panelType;
            planarBoundary3D = new PlanarBoundary3D(face);
        }

        public Panel(Construction construction, PanelType panelType, PlanarBoundary3D planarBoundary3D)
            : base(construction == null ? null : construction.Name, construction)
        {
            this.panelType = panelType;
            this.planarBoundary3D = planarBoundary3D;
        }

        public Panel(Guid guid, Panel panel, Face3D face, double maxDistance = Geometry.Tolerance.MacroDistance)
            : base(guid, panel)
        {
            panelType = panel.panelType;
            planarBoundary3D = new PlanarBoundary3D(face);

            if (panel.apertures != null)
            {
                foreach(Aperture aperture in panel.apertures)
                    Modify.AddAperture(this, aperture.ApertureConstruction.Name, aperture.ApertureConstruction.ApertureType, aperture.GetFace3D().GetExternalEdge(), maxDistance);
            }
        }

        public Panel(Guid guid, string name, IEnumerable<ParameterSet> parameterSets, Construction construction, PanelType panelType, PlanarBoundary3D planarBoundary3D)
            : base(guid, name, parameterSets, construction)
        {
            this.panelType = panelType;
            this.planarBoundary3D = new PlanarBoundary3D(planarBoundary3D);
        }

        public Panel(JObject jObject)
             : base(jObject)
        {

        }

        public Face3D GetFace3D()
        {
            return planarBoundary3D.GetFace3D();
        }

        public void Snap(IEnumerable<Point3D> point3Ds, double maxDistance = double.NaN)
        {
            planarBoundary3D.Snap(point3Ds, maxDistance);
        }

        public BoundingBox3D GetBoundingBox(double offset = 0)
        {
            return GetFace3D().GetBoundingBox(offset);
        }

        public Construction Construction
        {
            get
            {
                return SAMType as Construction;
            }
        }

        public PanelType PanelType
        {
            get
            {
                return panelType;
            }
        }

        public PlanarBoundary3D PlanarBoundary3D
        {
            get
            {
                return new PlanarBoundary3D(planarBoundary3D);
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            Enum.TryParse(jObject.Value<string>("PanelType"), out this.panelType);

            planarBoundary3D = new PlanarBoundary3D(jObject.Value<JObject>("PlanarBoundary3D"));

            if (jObject.ContainsKey("Apertures"))
                apertures = Core.Create.IJSAMObjects<Aperture>(jObject.Value<JArray>("Apertures"));

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return jObject;

            jObject.Add("PanelType", panelType.ToString());
            jObject.Add("PlanarBoundary3D", planarBoundary3D.ToJObject());

            if (apertures != null)
                jObject.Add("Apertures", Core.Create.JArray(apertures));
            
            return jObject;
        }

        public Aperture AddAperture(ApertureConstruction apertureConstruction, Point3D location)
        {
            if (apertureConstruction == null || location == null)
                return null;

            Plane plane = planarBoundary3D.Plane;

            if (apertures == null)
                apertures = new List<Aperture>();

            Aperture aperture = new Aperture(apertureConstruction, plane, location);

            apertures.Add(aperture);
            return aperture;
        }

        public List<Aperture> Apertures
        {
            get
            {
                if (apertures == null)
                    return null;
                return apertures.ConvertAll(x => x.Clone());
            }
        }
    }
}
