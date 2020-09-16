using SAM.Geometry.Spatial;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static double Tilt(this IClosedPlanar3D closedPlanar3D)
        {
            if (closedPlanar3D == null)
                return double.NaN;

            Vector3D normal = closedPlanar3D.GetPlane()?.Normal;
            if (normal == null)
                return double.NaN;

            return Tilt(normal);
        }

        public static double Tilt(this Vector3D normal)
        {
            if (normal == null)
                return double.NaN;
            
            return normal.Angle(Plane.WorldXY.Normal) * (180 / System.Math.PI);
        }
    }
}