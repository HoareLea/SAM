using SAM.Geometry.Spatial;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static HostPartitionCategory HostPartitionCategory(Vector3D normal, double tolerance = Core.Tolerance.Angle)
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

        public static HostPartitionCategory HostPartitionCategory(IPartition partition, double tolerance = Core.Tolerance.Angle)
        {
            return HostPartitionCategory(partition?.Face3D?.GetPlane()?.Normal, tolerance);
        }
    }
}