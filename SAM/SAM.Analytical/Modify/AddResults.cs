using SAM.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static void AddResults(this AdjacencyCluster adjacencyCluster, IEnumerable<IResult> results, bool simplify = true)
        {
            if (results == null || adjacencyCluster == null)
            {
                return;
            }

            List<Space> spaces = null;
            List<Panel> panels = null;
            List<Zone> zones = null;
            foreach (IResult result in results)
            {
                IResult result_Temp = result.Clone();
                if (result_Temp == null)
                {
                    result_Temp = result;
                }

                adjacencyCluster.AddObject(result_Temp);

                if(simplify)
                {
                    if(result_Temp is TM52ExtendedResult)
                    {
                        result_Temp = ((TM52ExtendedResult)result_Temp).Simplify();
                    }
                }
                
                if (result_Temp is SpaceSimulationResult)
                {
                    SpaceSimulationResult spaceSimulationResult = (SpaceSimulationResult)result_Temp;

                    Space space = null;
                    if(Core.Query.TryConvert(spaceSimulationResult.Reference, out Guid guid))
                    {
                        if (spaces == null)
                        {
                            spaces = adjacencyCluster.GetSpaces();
                        }

                        space = spaces?.Find(x => x.Guid == guid);
                    }

                    if(space == null)
                    {
                        ObjectReference objectReference = Core.Convert.ComplexReference<ObjectReference>(spaceSimulationResult.Reference);
                        if(objectReference != null)
                        {
                            space = adjacencyCluster?.GetObjects<Space>(objectReference)?.FirstOrDefault();
                        }
                    }

                    if(space == null)
                    {
                        if (spaces == null)
                        {
                            spaces = adjacencyCluster.GetSpaces();
                        }

                        space = spaces?.Find(x => x?.Name == spaceSimulationResult.Reference);

                        if (space == null)
                        {
                            space = spaces?.Find(x => x?.Name == spaceSimulationResult.Name);
                        }
                    }

                    if(space != null)
                    {
                        adjacencyCluster.AddRelation(space, spaceSimulationResult);
                    }
                }
                else if(result_Temp is SurfaceSimulationResult)
                {
                    SurfaceSimulationResult surfaceSimulationResult = (SurfaceSimulationResult)result_Temp;

                    Panel panel = null;
                    if (Core.Query.TryConvert(surfaceSimulationResult.Reference, out Guid guid))
                    {
                        if (panels == null)
                        {
                            panels = adjacencyCluster.GetPanels();
                        }

                        panel = panels?.Find(x => x.Guid == guid);
                    }

                    if (panel == null)
                    {
                        ObjectReference objectReference = Core.Convert.ComplexReference<ObjectReference>(surfaceSimulationResult.Reference);
                        if (objectReference != null)
                        {
                            panel = adjacencyCluster?.GetObjects<Panel>(objectReference)?.FirstOrDefault();
                        }
                    }

                    if (panel == null)
                    {
                        if (panels == null)
                        {
                            panels = adjacencyCluster.GetPanels();
                        }

                        panel = panels?.Find(x => x?.Name == surfaceSimulationResult.Reference);

                        if (panel == null)
                        {
                            panel = panels?.Find(x => x?.Name == panel.Name);
                        }
                    }

                    if (panel != null)
                    {
                        adjacencyCluster.AddRelation(panel, surfaceSimulationResult);
                    }
                }
                else if (result_Temp is ZoneSimulationResult)
                {
                    ZoneSimulationResult zoneSimulationResult = (ZoneSimulationResult)result_Temp;

                    Zone zone = null;
                    if (Core.Query.TryConvert(zoneSimulationResult.Reference, out Guid guid))
                    {
                        if (zones == null)
                        {
                            zones = adjacencyCluster.GetZones();
                        }

                        zone = zones?.Find(x => x.Guid == guid);
                    }

                    if (zone == null)
                    {
                        ObjectReference objectReference = Core.Convert.ComplexReference<ObjectReference>(zoneSimulationResult.Reference);
                        if (objectReference != null)
                        {
                            zone = adjacencyCluster?.GetObjects<Zone>(objectReference)?.FirstOrDefault();
                        }
                    }

                    if (zone == null)
                    {
                        if (zones == null)
                        {
                            zones = adjacencyCluster.GetZones();
                        }

                        zone = zones?.Find(x => x?.Name == zoneSimulationResult.Reference);

                        if (zone == null)
                        {
                            zone = zones?.Find(x => x?.Name == zone.Name);
                        }
                    }

                    if (zone != null)
                    {
                        adjacencyCluster.AddRelation(zone, zoneSimulationResult);
                    }
                }
                else if (result_Temp is TMResult)
                {
                    TMResult tMSpaceResult = (TMResult)result_Temp;

                    Space space = null;
                    if (Core.Query.TryConvert(tMSpaceResult.Reference, out Guid guid))
                    {
                        if (spaces == null)
                        {
                            spaces = adjacencyCluster.GetSpaces();
                        }

                        space = spaces?.Find(x => x.Guid == guid);
                    }

                    if (space == null)
                    {
                        ObjectReference objectReference = Core.Convert.ComplexReference<ObjectReference>(tMSpaceResult.Reference);
                        if (objectReference != null)
                        {
                            space = adjacencyCluster?.GetObjects<Space>(objectReference)?.FirstOrDefault();
                        }
                    }

                    if (space == null)
                    {
                        if (spaces == null)
                        {
                            spaces = adjacencyCluster.GetSpaces();
                        }

                        space = spaces?.Find(x => x?.Name == tMSpaceResult.Reference);

                        if (space == null)
                        {
                            space = spaces?.Find(x => x?.Name == tMSpaceResult.Name);
                        }
                    }

                    if (space != null)
                    {
                        adjacencyCluster.AddRelation(space, tMSpaceResult);
                    }
                }
                else if (result_Temp is TMExtendedResult)
                {
                    TMExtendedResult tMSpaceExtendedResult = (TMExtendedResult)result_Temp;

                    Space space = null;
                    if (Core.Query.TryConvert(tMSpaceExtendedResult.Reference, out Guid guid))
                    {
                        if (spaces == null)
                        {
                            spaces = adjacencyCluster.GetSpaces();
                        }

                        space = spaces?.Find(x => x.Guid == guid);
                    }

                    if (space == null)
                    {
                        ObjectReference objectReference = Core.Convert.ComplexReference<ObjectReference>(tMSpaceExtendedResult.Reference);
                        if (objectReference != null)
                        {
                            space = adjacencyCluster?.GetObjects<Space>(objectReference)?.FirstOrDefault();
                        }
                    }

                    if (space == null)
                    {
                        if (spaces == null)
                        {
                            spaces = adjacencyCluster.GetSpaces();
                        }

                        space = spaces?.Find(x => x?.Name == tMSpaceExtendedResult.Reference);

                        if (space == null)
                        {
                            space = spaces?.Find(x => x?.Name == tMSpaceExtendedResult.Name);
                        }
                    }

                    if (space != null)
                    {
                        adjacencyCluster.AddRelation(space, tMSpaceExtendedResult);
                    }
                }
            }
        }
    }
}