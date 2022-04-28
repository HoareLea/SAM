using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static string RiserName(this MechanicalSystemCategory mechanicalSystemCategory, AdjacencyCluster adjacencyCluster = null)
        {
            if (mechanicalSystemCategory == MechanicalSystemCategory.Undefined)
            {
                return null;
            }

            HashSet<string> riserNames = new HashSet<string>();

            SpaceParameter? spaceParameter = Query.RiserNameSpaceParameter(mechanicalSystemCategory);
            if (spaceParameter != null && spaceParameter.HasValue)
            {
                List<Space> spaces = adjacencyCluster.GetSpaces();
                if (spaces != null)
                {
                    foreach (Space space in spaces)
                    {
                        if (!space.TryGetValue(spaceParameter.Value, out string riserName) || string.IsNullOrWhiteSpace(riserName))
                        {
                            continue;
                        }

                        riserNames.Add(riserName);
                    }
                }
            }

            int index = 1;

            string result = Query.RiserName(mechanicalSystemCategory, index);
            while (riserNames.Contains(result))
            {
                index++;
                result = Query.RiserName(mechanicalSystemCategory, index);
            }

            return result;
        }
    }
}