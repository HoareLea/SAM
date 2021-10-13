using SAM.Geometry.Spatial;

namespace SAM.Architectural
{
    public static partial class Query
    {
        public static double Azimuth(this IPartition partition)
        {
            return Geometry.Spatial.Query.Azimuth(partition, Vector3D.WorldY);
        }
    }
}