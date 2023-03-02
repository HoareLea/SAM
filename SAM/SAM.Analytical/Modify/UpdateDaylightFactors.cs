using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static void UpdateDaylightFactors(this AdjacencyCluster adjacencyCluster, IEnumerable<Space> spaces = null)
        {
            if (adjacencyCluster == null)
            {
                return;
            }

            List<Space> spaces_Temp = adjacencyCluster.GetSpaces();
            if(spaces_Temp == null || spaces_Temp.Count == 0)
            {
                return;
            }

            if(spaces != null)
            {
                List<Space> spaces_Temp_Temp = new List<Space>(spaces);
                spaces_Temp.RemoveAll(x => spaces_Temp_Temp.Find(y => x.Guid == y.Guid) == null);
            }

            foreach(Space space in spaces_Temp)
            {
                double daylightFactor = adjacencyCluster.DaylightFactor(space);
                if(double.IsNaN(daylightFactor) || daylightFactor == 0)
                {
                    space.RemoveValue(SpaceParameter.DaylightFactor);
                }
                else
                {
                    space.SetValue(SpaceParameter.DaylightFactor, daylightFactor);
                }

                adjacencyCluster.AddObject(space);
            }

        }
    }
}