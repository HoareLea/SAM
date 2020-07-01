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

            //IClosedPlanar3D closedPlanar3D_Temp = closedPlanar3D;
            //if (closedPlanar3D is Face3D)
            //    closedPlanar3D_Temp = ((Face3D)closedPlanar3D).GetExternalEdge();

            //if (!Spatial.Query.Clockwise(closedPlanar3D_Temp))
            //    normal.Negate();
        }

        public static double Tilt(this Vector3D normal)
        {
            if (normal == null)
                return double.NaN;
            
            return normal.Angle(Plane.WorldXY().Normal) * (180 / System.Math.PI);
        }
    }
}