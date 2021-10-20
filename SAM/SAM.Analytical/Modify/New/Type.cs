namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static bool Type(this IHostPartition hostPartition, HostPartitionType hostPartitionType)
        {
            if(hostPartition == null || hostPartitionType == null)
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

        public static bool Type(this IOpening opening, OpeningType openingType)
        {
            if (opening == null || openingType == null)
            {
                return false;
            }

            if (opening is Window && openingType is WindowType)
            {
                ((Window)opening).Type = (WindowType)openingType;
            }
            else if (opening is Door && openingType is DoorType)
            {
                ((Door)opening).Type = (DoorType)openingType;
            }
            else
            {
                return false;
            }

            return true;
        }
    }
}