using SAM.Geometry.Spatial;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static IHostPartition HostPartition(this Face3D face3D, HostPartitionType hostPartitionType = null, double tolerance = Core.Tolerance.Angle)
        {
            if(face3D == null || !face3D.IsValid())
            {
                return null;
            }

            if(hostPartitionType == null)
            {
                hostPartitionType = Query.DefaultHostPartitionType(face3D, tolerance);
            }

            if (hostPartitionType is WallType)
            {
                return new Wall((WallType)hostPartitionType, face3D);
            }

            if(hostPartitionType is RoofType)
            {
                return new Roof((RoofType)hostPartitionType, face3D);
            }

            if(hostPartitionType is FloorType)
            {
                return new Floor((FloorType)hostPartitionType, face3D);
            }

            return null;
        }

        public static IHostPartition HostPartition(System.Guid guid, Face3D face3D, HostPartitionType hostPartitionType = null, double tolerance = Core.Tolerance.Angle)
        {
            if (face3D == null || !face3D.IsValid())
            {
                return null;
            }

            if (hostPartitionType == null)
            {
                hostPartitionType = Query.DefaultHostPartitionType(face3D, tolerance);
            }

            if (hostPartitionType is WallType)
            {
                return new Wall(guid, (WallType)hostPartitionType, face3D);
            }

            if (hostPartitionType is RoofType)
            {
                return new Roof(guid, (RoofType)hostPartitionType, face3D);
            }

            if (hostPartitionType is FloorType)
            {
                return new Floor(guid, (FloorType)hostPartitionType, face3D);
            }

            return null;
        }

        public static IHostPartition HostPartition(System.Guid guid, Face3D face3D, IHostPartition hostPartition, double tolerance = Core.Tolerance.Distance)
        {
            if (face3D == null || !face3D.IsValid() || hostPartition == null)
            {
                return null;
            }

            if (hostPartition is Wall)
            {
                return new Wall(guid, (Wall)hostPartition, face3D, tolerance);
            }

            if (hostPartition is Roof)
            {
                return new Roof(guid, (Roof)hostPartition, face3D, tolerance);
            }

            if (hostPartition is Floor)
            {
                return new Floor(guid, (Floor)hostPartition, face3D, tolerance);
            }

            return null;
        }
    }
}
