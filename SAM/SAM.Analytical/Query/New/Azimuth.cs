using SAM.Geometry.Spatial;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double Azimuth(this IPartition partition)
        {
            return Geometry.Object.Spatial.Query.Azimuth(partition, Vector3D.WorldY);
        }
    }
}