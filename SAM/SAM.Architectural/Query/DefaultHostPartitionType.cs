using SAM.Geometry.Spatial;

namespace SAM.Architectural
{
    public static partial class Query
    {
        public static HostPartitionType DefaultHostPartitionType(Vector3D normal, double tolerance = Core.Tolerance.Angle)
        {
            if (normal == null)
                return null;
            HostPartitionCategory hostPartitionCategory = HostPartitionCategory(normal, tolerance);
            switch (hostPartitionCategory)
            {
                case Architectural.HostPartitionCategory.Floor:
                    return new FloorType(string.Empty);

                case Architectural.HostPartitionCategory.Roof:
                    return new RoofType(string.Empty);

                case Architectural.HostPartitionCategory.Wall:
                    return new WallType(string.Empty);

                default:
                    return null;
            }
        }

        public static HostPartitionType DefaultHostPartitionType(Face3D face3D, double tolerance = Core.Tolerance.Angle)
        {
            return DefaultHostPartitionType(face3D?.GetPlane()?.Normal, tolerance);
        }
    }
}