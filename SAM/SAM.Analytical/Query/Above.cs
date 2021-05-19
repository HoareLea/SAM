using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static bool Above(this Panel panel, Plane plane, double tolerance = 0)
        {
            if (panel == null || plane == null)
            {
                return false;
            }

            Face3D face3D = panel.GetFace3D();
            if (face3D == null)
            {
                return false;
            }

            ISegmentable3D segmentable3D = face3D.GetExternalEdge3D() as ISegmentable3D;
            if (segmentable3D == null)
            {
                throw new System.NotImplementedException();
            }


            List<Point3D> point3Ds = segmentable3D.GetPoints();
            if(point3Ds == null || point3Ds.Count == 0)
            {
                return false;
            }

            return point3Ds.TrueForAll(x => plane.Above(x, tolerance));
        }
    }
}