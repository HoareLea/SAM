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
        private List<Edge> edges;

        public Panel(Panel panel)
            : base(Guid.NewGuid(), panel.SAMType)
        {
            edges = panel.edges.ConvertAll(x => new Edge(x));
            this.panelType = panel.panelType;
        }

        public Panel(string name, Construction construction, IClosed3D profile)
            : base(name, construction)
        {
            panelType = PanelType.Undefined;

            IEnumerable<Edge> edges_Temp = Edge.FromGeometry(profile);
            if (edges_Temp != null)
                edges = edges_Temp.ToList();
            else
                edges = new List<Edge>();
        }

        public Panel(Construction construction, IClosed3D profile)
            : base(construction == null ? null : construction.Name, construction)
        {
            panelType = PanelType.Undefined;

            IEnumerable<Edge> edges_Temp = Edge.FromGeometry(profile);
            if (edges_Temp != null)
                edges = edges_Temp.ToList();
            else
                edges = new List<Edge>();
        }

        public PolycurveLoop3D ToPolycurveLoop()
        {
            List<ICurve3D> curve3Ds = new List<ICurve3D>();
            foreach (Edge edge in edges)
                curve3Ds.Add(edge.GetCurve3D());

            return new PolycurveLoop3D(curve3Ds);
        }

        public Surface ToSurface()
        {
            return new Surface(ToPolycurveLoop());
        }

        public void Snap(IEnumerable<Point3D> point3Ds, double maxDistance = double.NaN)
        {
            foreach (Edge edge in edges)
                edge.Snap(point3Ds, maxDistance);
        }

        public BoundingBox3D GetBoundingBox(double offset = 0)
        {
            List<BoundingBox3D> boundingBox3ds = new List<BoundingBox3D>();
            foreach (Edge edge in edges)
                boundingBox3ds.Add(edge.GetBoundingBox(offset));

            return new BoundingBox3D(boundingBox3ds);
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

        public List<Edge> GetEdges()
        {
            if (edges == null)
                return null;

            return new List<Edge>(edges);
        }
    }
}
