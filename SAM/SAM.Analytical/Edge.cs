using System.Collections.Generic;

using SAM.Core;
using SAM.Geometry.Spatial;

namespace SAM.Analytical
{
    public class Edge : SAMObject
    {
        private ICurve3D curve3D;

        public Edge(System.Guid guid, string name, ICurve3D curve3D)
            : base(guid, name)
        {
            this.curve3D = curve3D;
        }
        
        public Edge(ICurve3D curve)
            : base()
        {
            this.curve3D = curve;
        }

        public Edge(Edge edge)
            : base(edge)
        {
            curve3D = (ICurve3D)edge.curve3D.Clone();
        }

        public List<Segment3D> ToSegments()
        {
            if (curve3D is ISegmentable3D)
                return ((ISegmentable3D)curve3D).GetSegments();
            return null;
        }

        public void Snap(IEnumerable<Point3D> point3Ds, double maxDistance = double.NaN)
        {
            if (point3Ds == null)
                return;

            if (curve3D is Segment3D)
            {
                Segment3D segment3D = (Segment3D)curve3D;
                Point3D point3D_1 = Point3D.Closest(point3Ds, segment3D[0]);
                Point3D point3D_2 = Point3D.Closest(point3Ds, segment3D[1]);

                if(!double.IsNaN(maxDistance))
                {
                    if (point3D_1.Distance(segment3D[0]) > maxDistance)
                        point3D_1 = new Point3D(segment3D[0]);

                    if(point3D_2.Distance(segment3D[1]) > maxDistance)
                        point3D_2 = new Point3D(segment3D[1]);
                }

                curve3D = new Segment3D(point3D_1, point3D_2);
            }
        }

        public BoundingBox3D GetBoundingBox(double offset = 0)
        {
            if (curve3D is Segment3D)
                return ((Segment3D)curve3D).GetBoundingBox(offset);

            return null;
        }

        public ICurve3D GetCurve3D()
        {
                return curve3D.Clone() as ICurve3D;
        }

        public static IEnumerable<Edge> FromGeometry(IGeometry3D geometry3D)
        {
            IGeometry3D geometry3D_Temp = geometry3D;

            if (geometry3D is IClosed3D)
                geometry3D_Temp = ((IClosed3D)geometry3D).GetBoundary();

            if (geometry3D_Temp is ICurvable3D)
                return ((ICurvable3D)geometry3D_Temp).GetCurves().ConvertAll(x => new Edge(x));

            return null;
        }
    }
}
