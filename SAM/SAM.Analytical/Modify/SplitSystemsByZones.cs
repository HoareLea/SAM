// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using System;
using System.Collections.Generic;
using static SAM.Analytical.Name;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static List<MechanicalSystem> SplitSystemsByZones<TMechanicalSystem>(this AdjacencyCluster adjacencyCluster, string zoneCategoryName, bool removeEmptySystems = true) where TMechanicalSystem : MechanicalSystem
        {
            if (adjacencyCluster == null || zoneCategoryName == null)
            {
                return null;
            }

            List<Zone> zones = adjacencyCluster.GetZones().FindAll(x => x.GetValue<string>(ZoneParameter.ZoneCategory) == zoneCategoryName);
            if(zones is null || zones.Count == 0)
            {
                return null;
            }

            Dictionary<Guid, TMechanicalSystem> dictionary = [];

            List<Tuple<MechanicalSystemType, List<Space>>> tuples = []; 
            foreach (Zone zone in zones)
            {
                List<Space> spaces = adjacencyCluster.GetRelatedObjects<Space>(zone);
                if(spaces is null || spaces.Count == 0)
                {
                    continue;
                }

                MechanicalSystemType mechanicalSystemType = null;
                foreach(Space space in spaces)
                {
                    List<TMechanicalSystem> mechanicalSystems_Space = adjacencyCluster.GetRelatedObjects<TMechanicalSystem>(space);
                    if(mechanicalSystems_Space is null || mechanicalSystems_Space.Count == 0)
                    {
                        continue;
                    }

                    mechanicalSystemType = mechanicalSystems_Space.Find(x => x.Type != null) as MechanicalSystemType;
                    if(mechanicalSystemType is not null)
                    {
                        foreach(TMechanicalSystem mechanicalSystem in mechanicalSystems_Space)
                        {
                            if (!dictionary.ContainsKey(mechanicalSystem.Guid))
                            {
                                dictionary[mechanicalSystem.Guid] = mechanicalSystem;
                            }
                        }


                        break;
                    }
                }

                if(mechanicalSystemType is null)
                {
                    continue;
                }

                tuples.Add(new Tuple<MechanicalSystemType, List<Space>>(mechanicalSystemType, spaces));
            }

            List<MechanicalSystem> result = [];
            foreach (Tuple<MechanicalSystemType, List<Space>> tuple in tuples)
            {
                MechanicalSystem mechanicalSystem = adjacencyCluster.AddMechanicalSystem(tuple.Item1, tuple.Item2, false);
                if (mechanicalSystem is null)
                {
                    continue;
                }

                result.Add(mechanicalSystem);
            }

            if(removeEmptySystems)
            {
                foreach (TMechanicalSystem mechanicalSystem in dictionary.Values)
                {
                    List<Space> spaces = adjacencyCluster.GetRelatedObjects<Space>(mechanicalSystem);
                    if(spaces is null || spaces.Count == 0)
                    {
                        adjacencyCluster.RemoveObject(mechanicalSystem);
                    }
                }
            }

            return result;
        }
    }
}
