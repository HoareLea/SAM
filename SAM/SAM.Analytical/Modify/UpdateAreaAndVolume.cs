using System.Collections.Generic;

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

        public static void UpdateAreaAndVolume(this AdjacencyCluster adjacencyCluster)
        {
            List<Space> spaces = adjacencyCluster?.GetSpaces();
            if(spaces == null || spaces.Count == 0)
            {
                return;
            }

            foreach(Space space in spaces)
            {
                Space space_New = UpdateAreaAndVolume(adjacencyCluster, space);
                if(space_New == null)
                {
                    continue;
                }

                adjacencyCluster.AddObject(space_New);
            }
        }
    }
}