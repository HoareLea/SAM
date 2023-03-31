using System.Linq;
using System.Collections.Generic;
using System;
using SAM.Core;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static void SplitMaterialsByThickness(this AnalyticalModel analyticalModel, bool includeConstructions = true, bool includeApertureConstructions = true)
        {
            AdjacencyCluster adjacencyCluster = analyticalModel?.AdjacencyCluster;
            if(adjacencyCluster == null)
            {
                return;
            }

            if(!includeConstructions && !includeApertureConstructions)
            {
                return;
            }

            List<ConstructionLayer> constructionLayers = new List<ConstructionLayer>();

            List<Construction> constructions = null;
            List<ApertureConstruction> apertureConstructions = null;

            if (includeConstructions)
            {
                constructions = adjacencyCluster.GetConstructions();
                if (constructions != null)
                {
                    foreach (Construction construction in constructions)
                    {
                        List<ConstructionLayer> constructionLayers_Construction = construction?.ConstructionLayers;
                        if (constructionLayers_Construction == null || constructionLayers_Construction.Count == 0)
                        {
                            continue;
                        }

                        constructionLayers.AddRange(constructionLayers_Construction);
                    }
                }
            }

            if(includeApertureConstructions)
            {
                apertureConstructions = adjacencyCluster.GetApertureConstructions();
                if (apertureConstructions != null)
                {
                    foreach (ApertureConstruction apertureConstruction in apertureConstructions)
                    {
                        List<ConstructionLayer> constructionLayers_ApertureConstruction = null;

                        constructionLayers_ApertureConstruction = apertureConstruction?.PaneConstructionLayers;
                        if (constructionLayers_ApertureConstruction != null)
                        {
                            constructionLayers.AddRange(constructionLayers_ApertureConstruction);
                        }

                        constructionLayers_ApertureConstruction = apertureConstruction?.FrameConstructionLayers;
                        if (constructionLayers_ApertureConstruction != null)
                        {
                            constructionLayers.AddRange(constructionLayers_ApertureConstruction);
                        }
                    }
                }
            }

            if(constructionLayers == null || constructionLayers.Count == 0)
            {
                return;
            }

            List<Tuple<string, ConstructionLayer>> tuples = new List<Tuple<string, ConstructionLayer>>();
            while(constructionLayers.Count != 0)
            {
                ConstructionLayer constructionLayer = constructionLayers[0];
                if(constructionLayer == null)
                {
                    constructionLayers.RemoveAt(0);
                    continue;
                }

                string name = constructionLayer.Name;

                List<ConstructionLayer> constructionLayers_Name = constructionLayers.FindAll(x => x.Name == name);
                if(constructionLayers_Name == null || constructionLayers.Count < 2)
                {
                    constructionLayers.RemoveAt(0);
                    continue;
                }

                IEnumerable<double> thicknesses = constructionLayers_Name.ConvertAll(x => x.Thickness).Distinct();
                if(thicknesses == null || thicknesses.Count() < 2)
                {
                    constructionLayers.RemoveAt(0);
                    continue;
                }

                foreach(double thickness in thicknesses)
                {
                    ConstructionLayer constructionLayer_New = new ConstructionLayer(string.Format("{0}_{1}", name, thicknesses), thickness);
                    tuples.Add(new Tuple<string, ConstructionLayer>(name, constructionLayer_New));
                }
            }

            if(tuples == null || tuples.Count == 0)
            {
                return;
            }

            if (includeConstructions && constructions != null)
            {
                foreach (Construction construction in constructions)
                {
                    List<ConstructionLayer> constructionLayers_Construction = construction?.ConstructionLayers;
                    if (constructionLayers_Construction == null)
                    {
                        continue;
                    }

                    bool updated = false;
                    for (int i = 0; i < constructionLayers_Construction.Count; i++)
                    {
                        ConstructionLayer constructionLayer = constructionLayers_Construction[i];
                        if (constructionLayer == null)
                        {
                            continue;
                        }

                        Tuple<string, ConstructionLayer> tuple = tuples.Find(x => x.Item1 == constructionLayer.Name && x.Item2.Thickness == x.Item2.Thickness);
                        if (tuple == null)
                        {
                            continue;
                        }

                        updated = true;

                        constructionLayers_Construction[i] = tuple.Item2;
                    }

                    if (!updated)
                    {
                        continue;
                    }

                    Construction construction_New = new Construction(construction, constructionLayers);

                    List<Panel> panels = analyticalModel.GetPanels(x => x?.Construction != null && x.Construction.Guid == construction_New.Guid);
                    if (panels == null || panels.Count == 0)
                    {
                        continue;
                    }

                    analyticalModel.ReplaceConstruction(panels.ConvertAll(x => x.Guid), construction_New);
                }
            }

            if (includeApertureConstructions && apertureConstructions != null)
            {
                foreach (ApertureConstruction apertureConstruction in apertureConstructions)
                {
                    bool updated = false;

                    List<ConstructionLayer> constructionLayers_Pane = apertureConstruction?.PaneConstructionLayers;
                    if (constructionLayers_Pane != null)
                    {
                        for (int i = 0; i < constructionLayers_Pane.Count; i++)
                        {
                            ConstructionLayer constructionLayer = constructionLayers_Pane[i];
                            if (constructionLayer == null)
                            {
                                continue;
                            }

                            Tuple<string, ConstructionLayer> tuple = tuples.Find(x => x.Item1 == constructionLayer.Name && x.Item2.Thickness == x.Item2.Thickness);
                            if (tuple == null)
                            {
                                continue;
                            }

                            updated = true;

                            constructionLayers_Pane[i] = tuple.Item2;
                        }
                    }

                    List<ConstructionLayer> constructionLayers_Frame = apertureConstruction?.FrameConstructionLayers;
                    if (constructionLayers_Frame != null)
                    {
                        for (int i = 0; i < constructionLayers_Frame.Count; i++)
                        {
                            ConstructionLayer constructionLayer = constructionLayers_Frame[i];
                            if (constructionLayer == null)
                            {
                                continue;
                            }

                            Tuple<string, ConstructionLayer> tuple = tuples.Find(x => x.Item1 == constructionLayer.Name && x.Item2.Thickness == x.Item2.Thickness);
                            if (tuple == null)
                            {
                                continue;
                            }

                            updated = true;

                            constructionLayers_Frame[i] = tuple.Item2;
                        }
                    }

                    if (!updated)
                    {
                        continue;
                    }

                    ApertureConstruction apertureConstruction_New = new ApertureConstruction(apertureConstruction, constructionLayers_Pane, constructionLayers_Frame);

                    List<Aperture> apertures = analyticalModel.GetApertures(x => x?.ApertureConstruction != null && x.ApertureConstruction.Guid == apertureConstruction_New.Guid);
                    if (apertures == null || apertures.Count == 0)
                    {
                        continue;
                    }

                    analyticalModel.ReplaceApertureConstruction(apertures.ConvertAll(x => x.Guid), apertureConstruction_New);
                }
            }

            MaterialLibrary materialLibrary = analyticalModel.MaterialLibrary;
            if(materialLibrary != null)
            {
                foreach(Tuple<string, ConstructionLayer> tuple in tuples)
                {
                    Material material = materialLibrary.GetMaterial(tuple.Item1) as Material;
                    if(material == null)
                    {
                        continue;
                    }

                    material = Core.Create.Material(material, tuple.Item1);
                    if (material == null)
                    {
                        continue;
                    }


                    material.SetValue(Core.MaterialParameter.DefaultThickness, tuple.Item2.Thickness);
                    analyticalModel.AddMaterial(material);
                }

                
            }
        }
    }
}