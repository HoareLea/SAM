namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static double Azimuth(this IClosedPlanar3D closedPlanar3D, Vector3D referenceDirection)
        {
            if (closedPlanar3D == null || referenceDirection == null)
                return double.NaN;

            Vector3D normal = closedPlanar3D.GetPlane()?.Normal;
            if (normal == null)
                return double.NaN;

            //IClosedPlanar3D closedPlanar3D_Temp = closedPlanar3D;
            //if (closedPlanar3D is Face3D)
            //    closedPlanar3D_Temp = ((Face3D)closedPlanar3D).GetExternalEdge();

            //if (!Spatial.Query.Clockwise(closedPlanar3D_Temp))
            //    normal.Negate();

            if (normal.Z == 1)
                return 0;

            if (normal.Z == -1)
                return 180;

            Vector3D vector3D_Project_Normal = Plane.WorldXY.Project(normal);
            Vector3D vector3D_Project_ReferenceDirection = Plane.WorldXY.Project(referenceDirection);

            double azimuth = Spatial.Query.SignedAngle(vector3D_Project_Normal, vector3D_Project_ReferenceDirection, Vector3D.WorldZ) * (180 / System.Math.PI);
            if (azimuth < 0)
                azimuth = 360 + azimuth;

            return azimuth;
        }
    }
}