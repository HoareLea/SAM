using System;
using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static double MaxProjectedDistance(this LinkedFace3D linkedFace3D, Plane plane)
        {
            if (linkedFace3D == null || plane == null)
            {
                return double.NaN;
            }


            IClosedPlanar3D closedPlanar3D = linkedFace3D.Face3D.GetExternalEdge3D();
            if (closedPlanar3D == null)
            {
                return double.NaN;
            }

            ISegmentable3D segmentable3D = closedPlanar3D as ISegmentable3D;
            if (segmentable3D == null)
            {
                throw new NotImplementedException();
            }

            List<Point3D> point3Ds = segmentable3D.GetPoints();
            if (point3Ds == null || point3Ds.Count == 0)
            {
                return double.NaN;
            }

            double result = double.MinValue;
            foreach (Point3D point3D_Temp in point3Ds)
            {
                if (point3D_Temp == null)
                {
                    continue;
                }

                Point3D point3D_Project = plane.Project(point3D_Temp);

                double distance = point3D_Project.Distance(point3D_Temp);
                if (distance > result)
                {
                    result = distance;
                }
            }

            return result;
        }
    }
}
