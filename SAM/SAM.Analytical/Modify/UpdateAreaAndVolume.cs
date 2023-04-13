using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static Space UpdateAreaAndVolume(this AdjacencyCluster adjacencyCluster, Space space, bool forceUpdate = true)
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

            bool update;

            update = true;
            if(!forceUpdate && result.TryGetValue(SpaceParameter.Volume, out double volume) && volume > 0)
            {
                update = false;
            }

            if(update)
            {
                result.SetValue(SpaceParameter.Volume, result.Volume(adjacencyCluster));
            }

            update = true;
            if (!forceUpdate && result.TryGetValue(SpaceParameter.Area, out double area) && area > 0)
            {
                update = false;
            }

            if (update)
            {
                result.SetValue(SpaceParameter.Area, result.CalculatedArea(adjacencyCluster));
            }

            return result;
        }

        public static void UpdateAreaAndVolume(this AdjacencyCluster adjacencyCluster, bool forceUpdate = true)
        {
            List<Space> spaces = adjacencyCluster?.GetSpaces();
            if(spaces == null || spaces.Count == 0)
            {
                return;
            }

            foreach(Space space in spaces)
            {
                Space space_New = UpdateAreaAndVolume(adjacencyCluster, space, forceUpdate);
                if(space_New == null)
                {
                    continue;
                }

                adjacencyCluster.AddObject(space_New);
            }
        }
    }
}