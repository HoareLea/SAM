namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static Space UpdateAreaAndVolume(this AdjacencyCluster adjacencyCluster, Space space)
        {
            if (adjacencyCluster == null || space == null)
            {
                return null;
            }

            Space result = adjacencyCluster.GetObject<Space>(space.Guid);
            if(result == null)
            {
                return null;
            }

            result.SetValue(SpaceParameter.Volume, result.Volume(adjacencyCluster));

            result.SetValue(SpaceParameter.Area, result.CalculatedArea(adjacencyCluster));

            return result;
        }
    }
}