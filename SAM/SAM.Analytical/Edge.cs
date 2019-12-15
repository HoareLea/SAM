using SAM.Core;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Analytical
{
    public class Edge : SAMObject
    {
        private ICurve3D curve3D;

        public Edge(Segment3D segment3D)
            : base()
        {
            this.curve3D = segment3D;
        }

        public Edge(Edge edge)
        {
            curve3D = (ICurve3D)edge.curve3D.Clone();
        }

        public List<Segment3D> Segments
        {
            get
            {
                if (curve3D is ISegmentable3D)
                    return ((ISegmentable3D)curve3D).GetSegments();
                return null;
            }
        }

        public void Snap(IEnumerable<Point3D> point3Ds)
        {
            if (point3Ds == null)
                return;
            
            if(curve3D is Segment3D)
            {
                Segment3D segment3D = (Segment3D)curve3D;
                Point3D point3D_1 = Point3D.Closest(point3Ds, segment3D.Start);
                Point3D point3D_2 = Point3D.Closest(point3Ds, segment3D.End);
                curve3D = new Segment3D(point3D_1, point3D_2);
            }
        }

        public BoundingBox3D GetBoundingBox(double offset = 0)
        {
            if (curve3D is Segment3D)
                return ((Segment3D)curve3D).GetBoundingBox(offset);

            return null;
        }
    }
}
