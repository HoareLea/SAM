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

            return Clockwise(point3Ds, normal, tolerance);

        }

        public static bool Clockwise(this IEnumerable<Point3D> point3Ds, Vector3D normal = null, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (point3Ds == null || point3Ds.Count() < 3)
                return false;

            Vector3D vector3D_Normal = normal;
            if (vector3D_Normal == null)
                vector3D_Normal = point3Ds.Normal(tolerance_Distance);

            if (vector3D_Normal == null)
                return false;

            Vector3D direction = (point3Ds.First() - point3Ds.Last()).Unit;

            double angle_Total = 0;

            for (int i = 1; i < point3Ds.Count(); i++)
            {
                Vector3D dirtection_Temp = (point3Ds.ElementAt(i) - point3Ds.ElementAt(i - 1)).Unit;
                double angle = direction.SignedAngle(dirtection_Temp, vector3D_Normal);
                direction = dirtection_Temp;

                if (System.Math.PI - System.Math.Abs(angle) <= tolerance_Angle)
                    direction *= -1;
                else
                    angle_Total += angle;
            }

            return angle_Total > 0;
        }
    }
}