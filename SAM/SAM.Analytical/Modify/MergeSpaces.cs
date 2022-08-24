using SAM.Geometry.Planar;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static Space MergeSpaces(this AdjacencyCluster adjacencyCluster, Guid spaceGuid_1, Guid spaceGuid_2, IEnumerable<Guid> panelGuids = null)
        {
            if (adjacencyCluster == null)
            {
                return null;
            }

            Space space_1 = adjacencyCluster.GetObject<Space>(spaceGuid_1);
            if(space_1 == null)
            {
                return null;
            }

            Space space_2 = adjacencyCluster.GetObject<Space>(spaceGuid_2);
            if (space_2 == null)
            {
                return null;
            }

            List<Panel> panels_Adjacent = adjacencyCluster.GetPanels(Core.LogicalOperator.And, space_1, space_2);
            if(panels_Adjacent == null || panels_Adjacent.Count == 0)
            {
                return null;
            }

            List<Guid> panelGuids_ToBeRemoved = panels_Adjacent.ConvertAll(x => x.Guid).FindAll(x => panelGuids.Contains(x));
            if (panelGuids_ToBeRemoved == null || panelGuids_ToBeRemoved.Count == 0)
            {
                return null;
            }

            List<Panel> panels_2 = adjacencyCluster.GetPanels(space_2).FindAll(x => !panelGuids_ToBeRemoved.Contains(x.Guid));

            adjacencyCluster.RemoveObject<Space>(spaceGuid_2);
            panelGuids_ToBeRemoved.ForEach(x => adjacencyCluster.RemoveObject<Panel>(x));

            panels_Adjacent.RemoveAll(x => panelGuids_ToBeRemoved.Contains(x.Guid));
            if(panels_Adjacent.Count != 0)
            {
                panels_Adjacent = panels_Adjacent.ConvertAll(x => Create.Panel(x, PanelType.Shade));

                foreach (Panel panel in panels_Adjacent)
                {
                    adjacencyCluster.AddObject(panels_Adjacent);
                }

            }

            foreach(Panel panel in panels_2)
            {
                adjacencyCluster.AddRelation(space_1, panel);
            }

            if(space_1.TryGetValue(SpaceParameter.Volume, out double volume))
            {
                space_1.SetValue(SpaceParameter.Volume, space_1.Volume(adjacencyCluster));
            }

            if (space_1.TryGetValue(SpaceParameter.Area, out double area))
            {
                space_1.SetValue(SpaceParameter.Area, space_1.CalculatedArea(adjacencyCluster));
            }

            adjacencyCluster.AddObject(space_1);

            return space_1;
        }

        public static List<Space> MergeSpaces(this AdjacencyCluster adjacencyCluster, IEnumerable<Guid> spaceGuids = null)
        {
            if(adjacencyCluster == null)
            {
                return null;
            }

            if(spaceGuids == null)
            {
                spaceGuids = adjacencyCluster.GetSpaces()?.ConvertAll(x => x.Guid);
            }

            if(spaceGuids == null)
            {
                return null;
            }


            HashSet<Guid> guids = new HashSet<Guid>();
            foreach (Guid guid in spaceGuids)
            {
                Space space = adjacencyCluster.GetObject<Space>(guid);
                if (space == null)
                {
                    continue;
                }

                List<Space> spaces_Adjacent = Analytical.Query.AdjacenSpaces(adjacencyCluster, space);
                if (spaces_Adjacent == null || spaces_Adjacent.Count == 0)
                {
                    continue;
                }

                spaces_Adjacent.RemoveAll(x => !spaceGuids.Contains(x.Guid));

                foreach (Space space_Adjacent in spaces_Adjacent)
                {
                    List<Panel> panels = adjacencyCluster.GetPanels(Core.LogicalOperator.And, space, space_Adjacent);
                    panels.RemoveAll(x => x.PanelType != PanelType.Air);
                    if (panels == null || panels.Count == 0)
                    {
                        continue;
                    }

                    Space space_1 = space;
                    Space space_2 = space_Adjacent;
                    if (space_1.Volume(adjacencyCluster) < space_2.Volume(adjacencyCluster))
                    {
                        space_2 = space;
                        space_1 = space_Adjacent;
                    }

                    Space space_New = adjacencyCluster.MergeSpaces(space_1.Guid, space_2.Guid, panels.ConvertAll(x => x.Guid));
                    if (space_New != null)
                    {
                        guids.Add(space_New.Guid);
                    }
                }
            }

            return guids.ToList().ConvertAll(x => adjacencyCluster.GetObject<Space>(x)).FindAll(x => x != null);
        }
    }
}