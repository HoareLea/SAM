using SAM.Geometry.Spatial;

namespace SAM.Architectural
{
    public static partial class Query
    {
        public static HostPartitionCategory HostPartitionCategory(Vector3D normal, double tolerance = Core.Tolerance.Angle)
        {
            if (normal == null)
                return Architectural.HostPartitionCategory.Undefined;

            double value = normal.Unit.DotProduct(Vector3D.WorldZ);
            if (System.Math.Abs(value) <= tolerance)
                return Architectural.HostPartitionCategory.Wall;

            if (value < 0)
                return Architectural.HostPartitionCategory.Floor;

            return Architectural.HostPartitionCategory.Roof;
        }
    }
}