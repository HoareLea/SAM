using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static Orientation Orientation(this IClosedPlanar3D closedPlanar3D, Vector3D normal = null, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if(closedPlanar3D == null)
            {
                return Geometry.Orientation.Undefined;
            }

            bool? clockwise = Clockwise(closedPlanar3D, normal, tolerance_Angle, tolerance_Distance);
            if(clockwise == null || !clockwise.HasValue)
            {
                return Geometry.Orientation.Undefined;
            }

            return clockwise.Value ? Geometry.Orientation.Clockwise : Geometry.Orientation.CounterClockwise;
        }

        public static Orientation Orientation(this IEnumerable<Point3D> point3Ds, Vector3D normal = null, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (point3Ds == null)
            {
                return Geometry.Orientation.Undefined;
            }

            bool? clockwise = Clockwise(point3Ds, normal, tolerance_Angle, tolerance_Distance);
            if (clockwise == null || !clockwise.HasValue)
            {
                return Geometry.Orientation.Undefined;
            }

            return clockwise.Value ? Geometry.Orientation.Clockwise : Geometry.Orientation.CounterClockwise;
        }
    }
}