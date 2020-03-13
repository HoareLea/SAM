namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static bool Colinear(this Point3D point2D_1, Point3D point2D_2, Point3D point2D_3, double tolerance = Core.Tolerance.Angle)
        {
            if (point2D_1 == null || point2D_2 == null || point2D_3 == null)
                return false;

            return new Vector3D(point2D_2, point2D_1).SmallestAngle(new Vector3D(point2D_2, point2D_3)) < tolerance;
        }
    }
}