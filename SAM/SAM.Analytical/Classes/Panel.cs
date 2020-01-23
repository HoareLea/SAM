using System;
using System.Collections.Generic;
using System.Linq;
using SAM.Core;
using SAM.Geometry.Spatial;

namespace SAM.Analytical
{
    public class Panel : SAMInstance
    {
        private PanelType panelType;
        private Boundary3D boundary3D;

        public Panel(Panel panel)
            : base(panel)
        {
            boundary3D = new Boundary3D(panel.boundary3D);
            panelType = panel.panelType;
        }

        public Panel(Panel panel, Construction construction)
            : base(panel, construction)
        {
            boundary3D = new Boundary3D(panel.boundary3D);
            panelType = panel.panelType;
        }

        public Panel(Panel panel, PanelType panelType)
            : base(panel)
        {
            boundary3D = new Boundary3D(panel.boundary3D);
            this.panelType = panelType;
        }

        public Panel(Construction construction, PanelType panelType, Face face)
            : base(construction == null ? null : construction.Name, construction)
        {
            this.panelType = panelType;
            boundary3D = new Boundary3D(face);
        }

        public Panel(Construction construction, PanelType panelType, Boundary3D boundary3D)
            : base(construction == null ? null : construction.Name, construction)
        {
            this.panelType = panelType;
            this.boundary3D = boundary3D;
        }

        public Panel(Guid guid, Panel panel, Face face)
            : base(guid, panel)
        {
            panelType = panel.panelType;
            boundary3D = new Boundary3D(face);
        }

        public Panel(Guid guid, string name, IEnumerable<ParameterSet> parameterSets, Construction construction, PanelType panelType, Boundary3D boundary3D)
            : base(guid, name, parameterSets, construction)
        {
            this.panelType = panelType;
            this.boundary3D = new Boundary3D(boundary3D);
        }
        
        public Face GetFace()
        {
            return boundary3D.GetFace();
        }

        public void Snap(IEnumerable<Point3D> point3Ds, double maxDistance = double.NaN)
        {
            boundary3D.Snap(point3Ds, maxDistance);
        }

        public BoundingBox3D GetBoundingBox(double offset = 0)
        {
            return GetFace().GetBoundingBox(offset);

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

        public List<Edge3D> GetEdge3Ds()
        {
            return boundary3D.GetEdge3DLoop().Edge3Ds;
        }

        public IClosedPlanar3D GetClosedPlanar3D()
        {
            return GetFace().ToClosedPlanar3D();
        }

        public Boundary3D Boundary3D
        {
            get
            {
                return new Boundary3D(boundary3D);
            }
        }
    }
}
