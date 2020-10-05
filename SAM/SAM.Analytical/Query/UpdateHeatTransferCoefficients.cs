using SAM.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static AnalyticalModel UpdateHeatTransferCoefficients(this AnalyticalModel analyticalModel, out List<Construction> constructions)
        {
            constructions = null;

            AdjacencyCluster adjacencyCluster = analyticalModel?.AdjacencyCluster;
            if (adjacencyCluster == null)
                return null;

            MaterialLibrary materialLibrary = analyticalModel.MaterialLibrary;
            if (materialLibrary == null)
                return null;


            if (adjacencyCluster == null || materialLibrary == null)
                return null;

            List<Panel> panels = adjacencyCluster.GetPanels();

            Dictionary<Guid, Tuple<Construction, List<Panel>>> dictionary = new Dictionary<Guid, Tuple<Construction, List<Panel>>>();
            foreach (Panel panel in panels)
            {
                Guid guid = panel.SAMTypeGuid;

                Tuple<Construction, List<Panel>> tuple = null;
                if (!dictionary.TryGetValue(guid, out tuple))
                {
                    Construction construction = panel.Construction;
                    if (construction == null)
                        continue;

                    IEnumerable<GasMaterial> gasMaterials = tuple.Item1.Materials<GasMaterial>(materialLibrary);
                    if (gasMaterials == null || gasMaterials.Count() == 0)
                        continue;

                    tuple = new Tuple<Construction, List<Panel>>(construction, new List<Panel>());
                    dictionary[guid] = tuple;
                }

                if (tuple == null)
                    continue;

                tuple.Item2.Add(panel);
            }


            constructions = new List<Construction>();
            foreach (Tuple<Construction, List<Panel>> tuple in dictionary.Values)
            {
                double tilt = 0;
                double area = 0;

                foreach (Panel panel in panels)
                {
                    double area_Panel = panel.GetArea();

                    tilt += panel.Tilt() * System.Math.PI / 180 * area_Panel;
                    area += area_Panel;
                }

                if (area == 0)
                    continue;

                tilt = tilt / area;

                double tilt_degree = System.Math.Round(tilt * 180 / System.Math.PI, 0);

                Construction construction = tuple.Item1;
                List<ConstructionLayer> constructionLayers = new List<ConstructionLayer>();
                foreach (ConstructionLayer constructionLayer in tuple.Item1.ConstructionLayers)
                {
                    GasMaterial gasMaterial = constructionLayer.Material(materialLibrary) as GasMaterial;
                    if (gasMaterial == null)
                    {
                        constructionLayers.Add(constructionLayer.Clone());
                        continue;
                    }


                    double thickness = constructionLayer.Thickness;
                    double heatTransferCoefficient = Query.HeatTransferCoefficient(gasMaterial, 10, thickness, 283.15, tilt);
                    string name = gasMaterial.Name + "Tilt: " + tilt_degree.ToString();

                    GasMaterial gasMaterial_New = materialLibrary.GetObject<GasMaterial>(name);
                    if (gasMaterial_New == null)
                    {
                        gasMaterial_New = Create.GasMaterial(gasMaterial, name, name, name, thickness, heatTransferCoefficient);
                        materialLibrary.Add(gasMaterial_New);
                    }

                    constructionLayers.Add(new ConstructionLayer(gasMaterial_New.Name, thickness));
                }

                construction = new Construction(construction, constructionLayers);
                foreach (Panel panel in tuple.Item2)
                {
                    Panel panel_New = new Panel(panel, construction);
                    adjacencyCluster.AddObject(panel_New);
                }
                constructions.Add(construction);
            }

            AnalyticalModel result = new AnalyticalModel(analyticalModel, adjacencyCluster, materialLibrary);

            return result;
        }
    }
}