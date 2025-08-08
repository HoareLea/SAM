using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static IEnumerable<Aperture> SetDefaultApertureConstructionLayers(this AdjacencyCluster adjacencyCluster, IEnumerable<Guid> guids = null)
        {
            List<Panel> panels = adjacencyCluster?.GetPanels();
            if(panels == null || panels.Count == 0)
            {
                return null;
            }

            List<Tuple<ApertureConstruction, List<Tuple<Aperture, Guid, PanelType>>>> tuples = new List<Tuple<ApertureConstruction, List<Tuple<Aperture, Guid, PanelType>>>>();

            foreach(Panel panel in panels)
            {
                List<Aperture> apertures_Panel = panel?.Apertures;
                if(apertures_Panel == null || apertures_Panel.Count == 0)
                {
                    continue;
                }

                PanelType panelType = panel.PanelType;

                List<Aperture> apertures = new List<Aperture>();
                foreach(Aperture aperture_Panel in apertures_Panel)
                {
                    if(aperture_Panel == null)
                    {
                        continue;
                    }

                    if (guids != null && guids.Count() != 0 && !guids.Contains(aperture_Panel.Guid))
                    {
                        continue;
                    }

                    ApertureConstruction apertureConstruction_Default = Query.DefaultApertureConstruction(panel, aperture_Panel.ApertureType);
                    if(apertureConstruction_Default == null)
                    {
                        continue;
                    }

                    if(aperture_Panel.Name != null && apertureConstruction_Default.Name == aperture_Panel.Name)
                    {
                        continue;
                    }

                    apertures.Add(aperture_Panel);
                }

                if(apertures == null || apertures.Count == 0)
                {
                    continue;
                }

                foreach(Aperture aperture in apertures)
                {
                    ApertureConstruction apertureConstruction = aperture.ApertureConstruction;
                    int index = tuples.FindIndex(x => apertureConstruction == null ? x == null : apertureConstruction.Guid == x.Item1?.Guid);

                    Tuple<ApertureConstruction, List<Tuple<Aperture, Guid, PanelType>>> tuple = null;

                    if (index != -1) 
                    { 
                        tuple = tuples[index];
                    }
                    else
                    {
                        tuples.Add(new Tuple<ApertureConstruction, List<Tuple<Aperture, Guid, PanelType>>>(apertureConstruction, new List<Tuple<Aperture, Guid, PanelType>>()));
                        tuple = tuples.Last();
                    }

                    tuple.Item2.Add(new Tuple<Aperture, Guid, PanelType>(aperture, panel.Guid, panel.PanelType));
                }
            }

            List<Aperture> result = new List<Aperture>();
            foreach(Tuple<ApertureConstruction, List<Tuple<Aperture, Guid, PanelType>>> tuple in tuples)
            {
                ApertureConstruction apertureConstruction = tuple.Item1;

                foreach(ApertureType apertureType in Enum.GetValues(typeof(ApertureType)))
                {
                    List<Tuple<Aperture, Guid, PanelType>> tuples_ApertureType = tuple.Item2.FindAll(x => x.Item1.ApertureType == apertureType);
                    if(tuples_ApertureType == null || tuples_ApertureType.Count == 0)
                    {
                        continue;
                    }

                    foreach(PanelType panelType in Enum.GetValues(typeof(PanelType)))
                    {
                        List<Tuple<Aperture, Guid, PanelType>> tuples_PanelType = tuple.Item2.FindAll(x => x.Item3 == panelType);
                        if (tuples_PanelType == null || tuples_PanelType.Count == 0)
                        {
                            continue;
                        }

                        ApertureConstruction apertureConstruction_Default = Query.DefaultApertureConstruction(panelType, apertureType);
                        if (apertureConstruction_Default == null)
                        {
                            continue;
                        }

                        ApertureConstruction apertureConstruction_New = new ApertureConstruction(Guid.NewGuid(), apertureConstruction, apertureConstruction?.Name);
                        apertureConstruction_New = new ApertureConstruction(apertureConstruction_New, apertureConstruction_Default.PaneConstructionLayers, apertureConstruction_Default.FrameConstructionLayers);

                        foreach (Tuple<Aperture, Guid, PanelType> tuple_PanelType in tuples_PanelType)
                        {
                            Aperture aperture = tuple_PanelType.Item1;
                            Panel panel = adjacencyCluster.GetObject<Panel>(tuple_PanelType.Item2);

                            panel.RemoveAperture(aperture.Guid);

                            aperture = new Aperture(aperture, apertureConstruction_New);

                            panel.AddAperture(aperture);

                            adjacencyCluster.AddObject(panel);

                            result.Add(aperture);
                        }
                    }
                }
            }

            return result;
        }
    }
}