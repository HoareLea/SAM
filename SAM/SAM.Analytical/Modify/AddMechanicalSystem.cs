using System.Collections.Generic;
namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static MechanicalSystem AddMechanicalSystem(this AdjacencyCluster adjacencyCluster, MechanicalSystemType mechanicalSystemType, int index = -1, IEnumerable<Space> spaces = null)
        {
            if (adjacencyCluster == null || mechanicalSystemType == null)
                return null;

            List<Space> spaces_Temp = adjacencyCluster.GetSpaces();
            if (spaces != null)
            {
                List<Space> spaces_Filtered = new List<Space>();
                foreach (Space space in spaces)
                {
                    if (space == null)
                        continue;

                    Space space_Filtered = spaces_Temp.Find(x => x.Guid == space.Guid);
                    if (spaces_Filtered == null)
                        continue;

                    spaces_Filtered.Add(space_Filtered);
                }

                spaces_Temp = spaces_Filtered;
            }

            MechanicalSystem mechanicalSystem = Create.MechanicalSystem(mechanicalSystemType, index);
            if (mechanicalSystem == null)
                return null;

            adjacencyCluster.AddObject(mechanicalSystem);

            foreach(Space space in spaces_Temp)
                adjacencyCluster.AddRelation(mechanicalSystem, space);

            return mechanicalSystem;
        }
    }
}