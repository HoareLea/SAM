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

        public List<Segment3D> ToSegments()
        {
            //TODO: Convert to ICurve3Ds

            List<Segment3D> result = new List<Segment3D>();
            foreach (Edge edge in edges)
            {
                if (edge == null)
                    continue;

                List<Segment3D> segment3Ds = edge.ToSegments();
                if (segment3Ds != null)
                    result.AddRange(segment3Ds);
            }
            return result;
        }

        public Polygon3D ToPolygon()
        {
            //TODO: Sort Segment3Ds to get valid order for points

            List<Point3D> result = new List<Point3D>();

            foreach (Segment3D segment3D in ToSegments())
                result.Add(segment3D[0]);

            return new Polygon3D(result);
        }

        public Face ToFace()
        {
            return new Face(ToPolygon());
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
    }
}
