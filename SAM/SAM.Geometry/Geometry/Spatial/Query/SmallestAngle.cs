namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static double SmallestAngle(this Point3D point3D_Previous, Point3D point3D, Point3D point3D_Next)
        {
            if (point3D_Previous == null || point3D == null || point3D_Next == null)
                return double.NaN;

            return (new Vector3D(point3D, point3D_Previous).SmallestAngle(new Vector3D(point3D, point3D_Next)));
        }
    }
}