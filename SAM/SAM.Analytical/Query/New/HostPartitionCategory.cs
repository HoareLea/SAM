using SAM.Geometry.Spatial;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static HostPartitionCategory HostPartitionCategory(this Vector3D normal, double tolerance = Core.Tolerance.Angle)
        {
            if (normal == null)
                return Analytical.HostPartitionCategory.Undefined;

            double value = normal.Unit.DotProduct(Vector3D.WorldZ);
            if (System.Math.Abs(value) <= tolerance)
                return Analytical.HostPartitionCategory.Wall;

            if (value < 0)
                return Analytical.HostPartitionCategory.Floor;

            return Analytical.HostPartitionCategory.Roof;
        }

        public static HostPartitionCategory HostPartitionCategory(this IPartition partition, double tolerance = Core.Tolerance.Angle)
        {
            return HostPartitionCategory(partition?.Face3D?.GetPlane()?.Normal, tolerance);
        }

        public static HostPartitionCategory HostPartitionCategory(this HostPartitionType hostPartitionType)
        {
            if(hostPartitionType == null)
            {
                return Analytical.HostPartitionCategory.Undefined;
            }

            if(hostPartitionType is WallType)
            {
                return Analytical.HostPartitionCategory.Wall;
            }

            if (hostPartitionType is FloorType)
            {
                return Analytical.HostPartitionCategory.Floor;
            }

            if (hostPartitionType is RoofType)
            {
                return Analytical.HostPartitionCategory.Roof;
            }

            return Analytical.HostPartitionCategory.Undefined;
        }
    }
}