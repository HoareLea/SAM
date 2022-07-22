using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<string> MissingInternalConditionsNames(this AnalyticalModel analyticalModel)
        {
            AdjacencyCluster adjacencyCluster = analyticalModel?.AdjacencyCluster;
            if(adjacencyCluster == null)
            {
                return null;
            }

            List<InternalCondition> internalConditions = adjacencyCluster.GetInternalConditions(true, false)?.ToList();
            if(internalConditions == null || internalConditions.Count == 0)
            {
                return new List<string>();
            }

            HashSet<string> names_Templates = new HashSet<string>();
            List<InternalCondition> internalConditions_Templates = adjacencyCluster.GetInternalConditions(false, true)?.ToList();
            if(internalConditions_Templates != null && internalConditions_Templates.Count != 0)
            {
                foreach(InternalCondition internalCondition_Template in internalConditions_Templates)
                {
                    string name = internalCondition_Template?.Name;
                    if(string.IsNullOrWhiteSpace(name))
                    {
                        continue;
                    }

                    names_Templates.Add(name);
                }
            }

            HashSet<string> names = new HashSet<string>();
            foreach(InternalCondition internalCondition in internalConditions)
            {
                string name = internalCondition?.Name;
                if(string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                if(names_Templates.Contains(name))
                {
                    continue;
                }

                names.Add(name);
            }

            return names.ToList();
        }
    }
}