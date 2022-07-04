using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static string NextId(this AdjacencyCluster adjacencyCluster, MechanicalSystemType mechanicalSystemType)
        {
            if(adjacencyCluster == null || mechanicalSystemType == null)
            {
                return null;
            }

            int index = 1;

            List<MechanicalSystem> mechanicalSystems = adjacencyCluster.GetMechanicalSystems<MechanicalSystem>();
            if(mechanicalSystems != null && mechanicalSystems.Count != 0)
            {
                List<int> indexes = new List<int>();
                foreach(MechanicalSystem mechanicalSystem in mechanicalSystems)
                {
                    if(mechanicalSystem.Type.Guid == mechanicalSystemType.Guid)
                    {
                        string id = mechanicalSystem.Id;
                        if(Core.Query.TryConvert(id, out int index_Temp))
                        {
                            indexes.Add(index_Temp);
                        }
                    }
                }

                while(indexes.Contains(index))
                {
                    index++;
                }
            }

            return index.ToString();
        }
    }
}