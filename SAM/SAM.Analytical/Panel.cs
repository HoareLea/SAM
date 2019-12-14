using System;
using System.Collections.Generic;

using SAM.Core;
using SAM.Geometry.Spatial;

namespace SAM.Analytical
{
    public class Panel : SAMInstance
    {
        private List<Edge> edges;

        public Panel(Guid guid, PanelType panelType, IEnumerable<Edge> edges)
            : base(guid, panelType)
        {
            this.edges = new List<Edge>(edges);
        }

        public Panel(Panel panel)
            : base(Guid.NewGuid(), panel.SAMType)
        {
            edges = panel.edges.ConvertAll(x => new Edge(x));
        }

        public Panel(Guid guid, PanelType panelType, Polygon3D polygon3D)
            : base(guid, panelType)
        {
            edges = new List<Edge>();

            foreach (Segment3D segment3D in polygon3D.GetSegments())
                edges.Add(new Edge(segment3D));
        }

        public void Snap(IEnumerable<Point3D> point3Ds)
        {
            foreach (Edge edge in edges)
                edge.Snap(point3Ds);
        }

        public BoundingBox3D GetBoundingBox(double offset = 0)
        {
            List<BoundingBox3D> boundingBox3ds = new List<BoundingBox3D>();
            foreach (Edge edge in edges)
                boundingBox3ds.Add(edge.GetBoundingBox(offset));

            return new BoundingBox3D(boundingBox3ds);
        }
    }
}
