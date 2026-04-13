// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using System;
using System.Collections.Generic;

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
                List<Space> spaces_All = adjacencyCluster.GetRelatedObjects<Space>(zone);
                if(spaces_All is null || spaces_All.Count == 0)
                {
                    continue;
                }

                while (spaces_All.Count > 0)
                {
                    Space space = spaces_All[0];

                    bool added = false;

                    List<TMechanicalSystem> mechanicalSystems_Space = adjacencyCluster.GetRelatedObjects<TMechanicalSystem>(space);
                    if (mechanicalSystems_Space != null && mechanicalSystems_Space.Count > 0)
                    {
                        foreach (TMechanicalSystem mechanicalSystem in mechanicalSystems_Space)
                        {
                            MechanicalSystemType mechanicalSystemType = mechanicalSystem?.Type;
                            if (mechanicalSystemType is null)
                            {
                                continue;
                            }

                            List<Space> spaces_MechanicalSystem = adjacencyCluster.GetRelatedObjects<Space>(mechanicalSystem);
                            if (spaces_MechanicalSystem is null || spaces_MechanicalSystem.Count == 0)
                            {
                                continue;
                            }

                            foreach (Space space_MechanicalSystem in spaces_MechanicalSystem)
                            {
                                if (spaces_All.FindIndex(x => x.Guid == space_MechanicalSystem.Guid) is int index && index == -1)
                                {
                                    continue;
                                }

                                Tuple<MechanicalSystemType, List<Space>> tuple = tuples.Find(x => x.Item1?.Name == mechanicalSystemType.Name);
                                if (tuple is null)
                                {
                                    tuple = new Tuple<MechanicalSystemType, List<Space>>(mechanicalSystemType, []);
                                    tuples.Add(tuple);

                                }

                                tuple.Item2.Add(space_MechanicalSystem);

                                if (space_MechanicalSystem.Guid == space.Guid)
                                {
                                    added = true;
                                }
                            }
                        }
                    }

                    if(!added)
                    {
                        Tuple<MechanicalSystemType, List<Space>> tuple = tuples.Find(x => x.Item1 == null);
                        if (tuple is null)
                        {
                            tuple = new Tuple<MechanicalSystemType, List<Space>>(null, []);
                            tuples.Add(tuple);

                        }

                        tuple.Item2.Add(space);
                        spaces_All.RemoveAt(0);
                    }
                }

                
                //TODO:
                //Return list of MechanicalSystemType instead of one MechanicalSystemType.

                //MechanicalSystemType mechanicalSystemType = null;
                //foreach(Space space in spaces_All)
                //{
                //    List<TMechanicalSystem> mechanicalSystems_Space = adjacencyCluster.GetRelatedObjects<TMechanicalSystem>(space);
                //    if(mechanicalSystems_Space is null || mechanicalSystems_Space.Count == 0)
                //    {
                //        continue;
                //    }

                //    mechanicalSystemType = mechanicalSystems_Space.Find(x => x.Type != null) as MechanicalSystemType;
                //    if(mechanicalSystemType is not null)
                //    {
                //        foreach(TMechanicalSystem mechanicalSystem in mechanicalSystems_Space)
                //        {
                //            if (!dictionary.ContainsKey(mechanicalSystem.Guid))
                //            {
                //                dictionary[mechanicalSystem.Guid] = mechanicalSystem;
                //            }
                //        }


                //        break;
                //    }
                //}

                //if(mechanicalSystemType is null)
                //{
                //    continue;
                //}

                //tuples.Add(new Tuple<MechanicalSystemType, List<Space>>(mechanicalSystemType, spaces_All));
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
