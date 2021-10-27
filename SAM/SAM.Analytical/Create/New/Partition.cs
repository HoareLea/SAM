using SAM.Geometry.Spatial;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static IPartition Partition(this IPartition partition, System.Guid guid, Face3D face3D, double tolerance = Core.Tolerance.Distance)
        {
            return Partition<IPartition>(partition, guid, face3D, tolerance);
        }

        public static T Partition<T>(this T partition, System.Guid guid, Face3D face3D, double tolerance = Core.Tolerance.Distance) where T : IPartition
        {
            if (partition == null || face3D == null)
            {
                return default;
            }

            if (partition is IHostPartition)
            {
                IHostPartition hostPartition = HostPartition(guid, face3D, (IHostPartition)partition, tolerance);
                if(hostPartition is T)
                {
                    return (T)hostPartition;
                }
            }

            AirPartition airPartition = new AirPartition(guid, partition as AirPartition, face3D);
            if(airPartition is T)
            {
                return (T)(object)airPartition;
            }

            return default;
        }
    }
}
