using SAM.Geometry.Spatial;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static Architectural.MaterialLayer FirstMaterialLayer(this IHostPartition hostPartition, Vector3D direction)
        {
            if (hostPartition == null || direction == null)
                return null;

            Vector3D normal = hostPartition.Face3D?.GetPlane()?.Normal;
            if (normal == null)
                return null;

            HostPartitionType hostPartitionType = hostPartition.Type();
            if (hostPartitionType == null)
                return null;

            if (direction.SameHalf(normal))
                return InternalMaterialLayer(hostPartitionType);

            return ExternalMaterialLayer(hostPartitionType);
        }
    }
}