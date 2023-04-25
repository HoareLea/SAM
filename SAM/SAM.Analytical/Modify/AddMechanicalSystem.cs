using System.Collections.Generic;
namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static MechanicalSystem AddMechanicalSystem(this AdjacencyCluster adjacencyCluster, MechanicalSystemType mechanicalSystemType, IEnumerable<Space> spaces = null, bool allowMultipleSystems = false)
        {
            if (adjacencyCluster == null || mechanicalSystemType == null)
            {
                return null;
            }

            List<Space> spaces_Temp = adjacencyCluster.GetSpaces();
            if (spaces != null)
            {
                List<Space> spaces_Filtered = new List<Space>();
                foreach (Space space in spaces)
                {
                    if (space == null)
                    {
                        continue;
                    }

                    Space space_Filtered = spaces_Temp.Find(x => x.Guid == space.Guid);
                    if (spaces_Filtered == null)
                    {
                        continue;
                    }

                    spaces_Filtered.Add(space_Filtered);
                }

                spaces_Temp = spaces_Filtered;
            }

            string id = Query.NextId(adjacencyCluster, mechanicalSystemType);

            MechanicalSystem mechanicalSystem = Create.MechanicalSystem(mechanicalSystemType, id);
            if (mechanicalSystem == null)
            {
                return null;
            }

            adjacencyCluster.AddObject(mechanicalSystem);

            foreach(Space space in spaces_Temp)
            {
                if(space == null)
                {
                    continue;
                }

                if(!allowMultipleSystems)
                {
                    List<MechanicalSystem> mechanicalSystems_Space = adjacencyCluster.GetRelatedObjects<MechanicalSystem>(space);
                    if(mechanicalSystems_Space != null)
                    {
                        foreach(MechanicalSystem mechanicalSystem_Space in mechanicalSystems_Space)
                        {
                            if(mechanicalSystem_Space.MechanicalSystemCategory() == mechanicalSystemType.MechanicalSystemCategory())
                            {
                                adjacencyCluster.RemoveRelation(space, mechanicalSystem_Space);
                            }
                        }
                    }
                }

                InternalCondition internalCondition = space.InternalCondition;
                if(internalCondition != null)
                {
                    InternalConditionParameter? internalConditionParameter = Query.SystemTypeInternalConditionParameter(mechanicalSystem.MechanicalSystemCategory());
                    if(internalConditionParameter != null && internalConditionParameter.HasValue)
                    {
                        internalCondition.SetValue(internalConditionParameter.Value, mechanicalSystem.Name);
                        space.InternalCondition = internalCondition;
                        adjacencyCluster.AddObject(space);
                    }
                }

                adjacencyCluster.AddRelation(mechanicalSystem, space);
            }


            return mechanicalSystem;
        }
    }
}