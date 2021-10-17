namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static bool Type(this IHostPartition hostPartition, HostPartitionType hostPartitionType)
        {
            if(hostPartition == null)
            {
                return false;
            }

            if(hostPartition is Wall && hostPartitionType is WallType)
            {
                ((Wall)hostPartition).Type = (WallType)hostPartitionType;
            }
            else if(hostPartition is Roof && hostPartitionType is RoofType)
            {
                ((Roof)hostPartition).Type = (RoofType)hostPartitionType;
            }
            else if (hostPartition is Floor && hostPartitionType is FloorType)
            {
                ((Floor)hostPartition).Type = (FloorType)hostPartitionType;
            }
            else
            {
                return false;
            }

            return true;
        }
    }
}