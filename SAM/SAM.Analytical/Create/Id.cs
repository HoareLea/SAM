using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static string Id(this AdjacencyCluster adjacencyCluster, MechanicalSystemType mechanicalSystemType)
        {
            if(adjacencyCluster == null || mechanicalSystemType == null)
            {
                return null;
            }

            int index = 0;

            List<MechanicalSystem> mechanicalSystems = adjacencyCluster.GetMechanicalSystems<MechanicalSystem>();

            string fullName = null;
            do
            {
                index++;
                fullName = string.Format("{0} {1}", mechanicalSystemType.Name == null ? string.Empty : mechanicalSystemType.Name, index.ToString());
                fullName = fullName.Trim();
            }
            while (mechanicalSystems != null && mechanicalSystems.Find(x => x.FullName == fullName) != null);


            return index.ToString();
        }
    }
}