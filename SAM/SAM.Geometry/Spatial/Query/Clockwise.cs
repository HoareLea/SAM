using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static bool Clockwise(this IClosedPlanar3D closedPlanar3D, double tolerance = Core.Tolerance.Angle)
        {
            Vector3D normal = closedPlanar3D?.GetPlane()?.Normal;
            if (normal == null)
                return false;

            ISegmentable3D segmentable3D = closedPlanar3D as ISegmentable3D;
            if (segmentable3D == null)
                throw new System.NotImplementedException();

            List<Point3D> point3Ds = segmentable3D.GetPoints();
            if (point3Ds == null || point3Ds.Count < 3)
                return false;

            Vector3D direction = (point3Ds.First() - point3Ds.Last()).Unit;

            double angle_Total = 0;

            for (int i = 1; i < point3Ds.Count; i++)
            {
                Vector3D dirtection_Temp = (point3Ds[i] - point3Ds[i - 1]).Unit;
                double angle = direction.SignedAngle(dirtection_Temp, normal);
                direction = dirtection_Temp;

                if (System.Math.PI - System.Math.Abs(angle) <= tolerance)
                    direction *= -1;
                else
                    angle_Total += angle;
            }

            return angle_Total > 0;
        }
    }
}