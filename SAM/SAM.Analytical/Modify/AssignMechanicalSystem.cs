// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;
namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static void AssignMechanicalSystem(this AdjacencyCluster adjacencyCluster, MechanicalSystem mechanicalSystem, IEnumerable<Space> spaces, bool allowMultipleSystems = false)
        {
            if (adjacencyCluster == null || mechanicalSystem == null)
            {
                return;
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
                    //space_Filtered, not spaces_Filtered. The list is constructed just above and is never
                    //null, so this guard never fired and a space that is not in the cluster was added to
                    //the filtered list as null instead of being skipped.
                    if (space_Filtered == null)
                    {
                        continue;
                    }

                    spaces_Filtered.Add(space_Filtered);
                }

                spaces_Temp = spaces_Filtered;
            }

            adjacencyCluster.AddObject(mechanicalSystem);

            MechanicalSystemType mechanicalSystemType = mechanicalSystem.Type;

            foreach (Space space in spaces_Temp)
            {
                if (space == null)
                {
                    continue;
                }

                if (!allowMultipleSystems)
                {
                    List<MechanicalSystem> mechanicalSystems_Space = adjacencyCluster.GetRelatedObjects<MechanicalSystem>(space);
                    if (mechanicalSystems_Space != null)
                    {
                        foreach (MechanicalSystem mechanicalSystem_Space in mechanicalSystems_Space)
                        {
                            if (mechanicalSystem_Space.MechanicalSystemCategory() == mechanicalSystemType.MechanicalSystemCategory())
                            {
                                adjacencyCluster.RemoveRelation(space, mechanicalSystem_Space);
                            }
                        }
                    }
                }

                InternalCondition internalCondition = space.InternalCondition;
                if (internalCondition != null)
                {
                    InternalConditionParameter? internalConditionParameter = Query.SystemTypeInternalConditionParameter(mechanicalSystem.MechanicalSystemCategory());
                    if (internalConditionParameter != null && internalConditionParameter.HasValue)
                    {
                        internalCondition.SetValue(internalConditionParameter.Value, mechanicalSystem.Name);
                        space.InternalCondition = internalCondition;
                        adjacencyCluster.AddObject(space);
                    }
                }

                adjacencyCluster.AddRelation(mechanicalSystem, space);
            }
        }
    }
}
