using System.Collections.Generic;
namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static MechanicalSystem AddMechanicalSystem(this AdjacencyCluster adjacencyCluster, MechanicalSystemType mechanicalSystemType, int index = -1, IEnumerable<Space> spaces = null, string supplyUnitName = null, string exhaustUnitName = null)
        {
            if (adjacencyCluster == null || mechanicalSystemType == null)
                return null;

            List<Space> spaces_Temp = adjacencyCluster.GetSpaces();
            if (spaces != null)
            {
                foreach (Space space in spaces)
                    if (space != null)
                        spaces_Temp.RemoveAll(x => x.Guid == space.Guid);
            }

            MechanicalSystem mechanicalSystem = Create.MechanicalSystem(mechanicalSystemType, index, supplyUnitName, exhaustUnitName);
            if (mechanicalSystem == null)
                return null;

            adjacencyCluster.AddObject(mechanicalSystem);

            foreach(Space space in spaces_Temp)
                adjacencyCluster.AddRelation(mechanicalSystem, space);

            return mechanicalSystem;
        }
    }
}