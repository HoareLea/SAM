namespace SAM.Analytical
{
    public static partial class Query
    {
        public static bool External(this PartitionAnalyticalType panelType)
        {
            switch(panelType)
            {
                case Analytical.PartitionAnalyticalType.CurtainWall:
                case Analytical.PartitionAnalyticalType.ExternalFloor:
                case Analytical.PartitionAnalyticalType.Roof:
                case Analytical.PartitionAnalyticalType.Shade:
                case Analytical.PartitionAnalyticalType.OnGradeFloor:
                case Analytical.PartitionAnalyticalType.UndergroundWall:
                case Analytical.PartitionAnalyticalType.UndergroundCeiling:
                case Analytical.PartitionAnalyticalType.ExternalWall:
                    return true;
                default:
                    return false;
            }
        }
    }
}