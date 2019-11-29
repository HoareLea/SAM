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

        public Panel(Guid guid, PanelType panelType, Polygon3D polygon3D)
            : base(guid, panelType)
        {
            edges = new List<Edge>();

            foreach (Segment3D segment3D in polygon3D.GetSegments())
                edges.Add(new Edge(segment3D));
        }
    }
}
