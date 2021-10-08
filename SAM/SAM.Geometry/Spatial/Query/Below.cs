using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static bool Below(this Plane plane, Point3D point3D, double tolerance = 0)
        {
            if (point3D == null || plane == null)
                return false;

            return !Above(plane, point3D, tolerance) && !plane.On(point3D, tolerance);
        }

        public static bool Below(this Face3D face3D, Plane plane, double tolerance = 0)
        {
            if (face3D == null || plane == null)
            {
                return false;
            }

            ISegmentable3D segmentable3D = face3D.GetExternalEdge3D() as ISegmentable3D;
            if (segmentable3D == null)
            {
                throw new System.NotImplementedException();
            }


            List<Point3D> point3Ds = segmentable3D.GetPoints();
            if (point3Ds == null || point3Ds.Count == 0)
            {
                return false;
            }

            return point3Ds.TrueForAll(x => plane.On(x, tolerance) || !plane.Above(x, tolerance));
        }

        public static bool Below(this IFace3DObject face3DObject, Plane plane, double tolerance = 0)
        {
            return Below(face3DObject?.Face3D, plane, tolerance);
        }
    }
}