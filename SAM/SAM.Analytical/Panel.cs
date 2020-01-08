using System;
using System.Collections.Generic;

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

        public Panel(Guid guid, Construction construction, IEnumerable<Edge> edges)
            : base(guid, construction)
        {
            this.edges = new List<Edge>(edges);
            this.panelType = PanelType.Undefined;
        }

        public Panel(string name, Construction construction, IClosed3D profile)
            : base(name, construction)
        {
            edges = new List<Edge>();

            if (profile is ISegmentable3D)
            {
                foreach (Segment3D segment3D in ((ISegmentable3D)profile).GetSegments())
                    edges.Add(new Edge(segment3D));
            }
            else if(profile is PolycurveLoop3D)
            {
                foreach (ICurve3D curve3D in ((PolycurveLoop3D)profile).Curves)
                    edges.Add(new Edge(curve3D));
            }
            this.panelType = PanelType.Undefined;
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
    }
}
