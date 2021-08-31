using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static AdjacencyCluster MergeCoplanarPanelsBySpace(this AdjacencyCluster adjacencyCluster, double offset, ref List<Panel> redundantPanels, bool validateConstruction = true, bool validatePanelGroup = true, double minArea = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if(adjacencyCluster == null)
            {
                return null;
            }

            AdjacencyCluster result = new AdjacencyCluster(adjacencyCluster);

            List<Space> spaces = adjacencyCluster.GetSpaces();
            if(spaces == null || spaces.Count == 0)
            {
                return result;
            }

            foreach(Space space in spaces)
            {
                List<Panel> panels = adjacencyCluster?.GetPanels(space);
                if (panels == null || panels.Count == 0)
                {
                    continue;
                }

                List<Tuple<List<Guid>, List<Panel>>> tuples = new List<Tuple<List<Guid>, List<Panel>>>();
                foreach(Panel panel in panels)
                {
                    List<Space> spaces_Panel = adjacencyCluster.GetSpaces(panel);

                    List<Guid> guids = spaces_Panel?.ConvertAll(x => x.Guid);
                    if(guids == null)
                    {
                        guids = new List<Guid>();
                    }

                    Tuple<List<Guid>, List<Panel>> tuple = tuples.Find(x => x.Item1.Count == guids.Count && x.Item1.TrueForAll(y => guids.Contains(y)));
                    if(tuple == null)
                    {
                        tuple = new Tuple<List<Guid>, List<Panel>>(guids, new List<Panel>());
                        tuples.Add(tuple);
                    }

                    int index = tuple.Item2.FindIndex(x => x.Guid == panel.Guid);
                    if(index == -1)
                    {
                        tuple.Item2.Add(panel);
                    }
                }

                if(tuples == null || tuples.Count == 0)
                {
                    continue;
                }

                foreach(Tuple<List<Guid>, List<Panel>> tuple in tuples)
                {
                    if(tuple.Item2 == null || tuple.Item2.Count < 2)
                    {
                        continue;
                    }
                    
                    List<Panel> mergedPanels = null;

                    if (validatePanelGroup)
                    {
                        mergedPanels = MergeCoplanarPanels((IEnumerable<Panel>)tuple.Item2, offset, ref redundantPanels, validateConstruction, minArea, tolerance);
                    }
                    else
                    {
                        mergedPanels = MergeCoplanarPanels(tuple.Item2, offset, ref redundantPanels, validateConstruction, minArea, tolerance);
                    }

                    if (redundantPanels != null && redundantPanels.Count != 0)
                    {
                        result.Remove(redundantPanels);
                    }

                    if (mergedPanels != null && mergedPanels.Count != 0)
                    {
                        mergedPanels.ForEach(x => result.AddObject(x));
                    }
                }
            }

            return result;
        }

        public static AnalyticalModel MergeCoplanarPanelsBySpace(this AnalyticalModel analyticalModel, double offset, ref List<Panel> redundantPanels, bool validateConstruction = true, bool validatePanelGroup = true, double minArea = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            AdjacencyCluster adjacencyCluster = analyticalModel?.AdjacencyCluster;
            if (adjacencyCluster == null)
                return null;

            adjacencyCluster = MergeCoplanarPanelsBySpace(adjacencyCluster, offset, ref redundantPanels, validateConstruction, validatePanelGroup, minArea, tolerance);

            return new AnalyticalModel(analyticalModel, adjacencyCluster);
        }
    }
}