using SAM.Geometry.Planar;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static List<Space> MergeSpaces(this AdjacencyCluster adjacencyCluster, List<Guid> spaceGuids, out List<Panel> panels, IEnumerable<PanelType> panelTypes, IEnumerable<Guid> panelGuids = null)
        {
            panels = null;
            if (adjacencyCluster == null || spaceGuids == null)
            {
                return null;
            }

            panels = new List<Panel>();

            HashSet<Guid> guids = new HashSet<Guid>();

            foreach (Guid guid in spaceGuids)
            {
                Space space = adjacencyCluster.GetObject<Space>(guid);
                if (space == null)
                {
                    continue;
                }

                List<Space> spaces_Adjacent = Query.AdjacentSpaces(adjacencyCluster, space);
                if (spaces_Adjacent == null || spaces_Adjacent.Count == 0)
                {
                    continue;
                }

                spaces_Adjacent.RemoveAll(x => !spaceGuids.Contains(x.Guid));


                foreach (Space space_Adjacent in spaces_Adjacent)
                {
                    List<Panel> panels_Temp = adjacencyCluster.GetPanels(Core.LogicalOperator.And, space, space_Adjacent);
                    if (panels_Temp == null || panels_Temp.Count == 0)
                    {
                        continue;
                    }

                    if (panelGuids != null)
                    {
                        panels_Temp.RemoveAll(x => !panelGuids.Contains(x.Guid));
                    }

                    if (panels_Temp == null || panels_Temp.Count == 0)
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

                    panels.AddRange(panels_Temp);

                    Space space_New = adjacencyCluster.MergeSpaces(space_1.Guid, space_2.Guid, panelTypes, panels_Temp.ConvertAll(x => x.Guid));
                    if (space_New != null)
                    {
                        guids.Add(space_New.Guid);
                    }
                }
            }


            return guids.ToList().ConvertAll(x => adjacencyCluster.GetObject<Space>(x)).FindAll(x => x != null);
        }

        public static Space MergeSpaces(this AdjacencyCluster adjacencyCluster, Guid spaceGuid_1, Guid spaceGuid_2, IEnumerable<PanelType> panelTypes, IEnumerable<Guid> panelGuids = null, string nameSeparator = "_")
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

            if (!string.IsNullOrEmpty(nameSeparator))
            {
                string name_1 = space_1.Name;
                string name_2 = space_2.Name;
                if (!string.IsNullOrWhiteSpace(name_1) && !string.IsNullOrWhiteSpace(name_2))
                {
                    string prefix_1 = name_1;
                    string prefix_2 = name_2;

                    int index_1 = name_1.LastIndexOf(nameSeparator);
                    if (index_1 > 0)
                    {
                        prefix_1 = name_1.Substring(0, index_1);
                    }

                    int index_2 = name_2.LastIndexOf(nameSeparator);
                    if (index_2 > 0)
                    {
                        prefix_2 = name_2.Substring(0, index_2);
                    }

                    if(prefix_2.StartsWith(prefix_1))
                    {
                        space_1 = new Space(space_1, prefix_1, space_1.Location);
                    }
                }
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
                //panels_Adjacent = panels_Adjacent.ConvertAll(x => Create.Panel(x, PanelType.Shade));

                for (int i = panels_Adjacent.Count - 1; i >= 0; i--)
                {
                    Panel panel = panels_Adjacent[i];
                    if (panelTypes == null || panelTypes.Contains(panel.PanelType))
                    {
                        panels_Adjacent.RemoveAt(i);
                        adjacencyCluster.RemoveObject<Panel>(panel.Guid);
                        continue;
                    }

                    panel = Create.Panel(panel, PanelType.Shade);
                    adjacencyCluster.AddObject(panel);
                    adjacencyCluster.RemoveRelation(panel, space_1);
                    adjacencyCluster.RemoveRelation(panel, space_2);
                    panels_Adjacent[i] = panel;
                }
            }

            foreach (Panel panel in panels_2)
            {
                Panel panel_Temp = panels_Adjacent.Find(x => x.Guid == panel.Guid);
                if(panel_Temp != null)
                {
                    continue;
                }

                adjacencyCluster.AddRelation(space_1, panel);
            }

            adjacencyCluster.UpdateAreaAndVolume(space_1);

            adjacencyCluster.AddObject(space_1);

            return space_1;
        }

        public static List<Space> MergeSpaces(this AdjacencyCluster adjacencyCluster, out List<Panel> panels, IEnumerable<PanelType> panelTypes, IEnumerable<Guid> spaceGuids = null)
        {
            panels = null;

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

            panels = new List<Panel>();

            HashSet<Guid> guids = new HashSet<Guid>();
            foreach (Guid guid in spaceGuids)
            {
                Space space = adjacencyCluster.GetObject<Space>(guid);
                if (space == null)
                {
                    continue;
                }

                List<Space> spaces_Adjacent = Query.AdjacentSpaces(adjacencyCluster, space);
                if (spaces_Adjacent == null || spaces_Adjacent.Count == 0)
                {
                    continue;
                }

                spaces_Adjacent.RemoveAll(x => !spaceGuids.Contains(x.Guid));

                foreach (Space space_Adjacent in spaces_Adjacent)
                {
                    List<Panel> panels_Temp = adjacencyCluster.GetPanels(Core.LogicalOperator.And, space, space_Adjacent);
                    if(panelTypes != null)
                    {
                        panels_Temp.RemoveAll(x => !panelTypes.Contains(x.PanelType));
                    }

                    if (panels_Temp == null || panels_Temp.Count == 0)
                    {
                        continue;
                    }

                    Space space_1 = space;
                    Space space_2 = space_Adjacent;
                    if (space_1.Volume(adjacencyCluster) > space_2.Volume(adjacencyCluster))
                    {
                        space_2 = space;
                        space_1 = space_Adjacent;
                    }

                    panels.AddRange(panels_Temp);

                    Space space_New = adjacencyCluster.MergeSpaces(space_1.Guid, space_2.Guid, panelTypes, panels_Temp.ConvertAll(x => x.Guid));
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