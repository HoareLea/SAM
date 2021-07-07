using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static AnalyticalModel UpdateHeatTransferCoefficients(this AnalyticalModel analyticalModel, bool duplicateConstructions, bool duplicateApertureConstructions, out List<Construction> constructions, out List<ApertureConstruction> apertureConstructions )
        {
            AnalyticalModel result = null;

            result = UpdateHeatTransferCoefficients_Constructions(analyticalModel, duplicateConstructions, out constructions);
            result = UpdateHeatTransferCoefficients_ApertureConstructions(result, duplicateApertureConstructions, out apertureConstructions);

            return result;
        }

        private static AnalyticalModel UpdateHeatTransferCoefficients_Constructions(this AnalyticalModel analyticalModel, bool duplicateConstructions, out List<Construction> constructions)
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

            List<Construction> constructions_All = adjacencyCluster.GetConstructions();
            if (constructions_All == null)
                return null;

            constructions = new List<Construction>();

            foreach (Construction construction in constructions_All)
            {
                if (construction == null)
                    continue;

                if (!construction.HasMaterial(materialLibrary, Core.MaterialType.Gas))
                    continue;

                List<Panel> panels = adjacencyCluster.GetPanels(construction);
                if (panels == null || panels.Count == 0)
                    continue;

                if(duplicateConstructions)
                {
                    Dictionary<double, List<Panel>> dictionary = new Dictionary<double, List<Panel>>();
                    foreach(Panel panel in panels)
                    {
                        if (panel == null)
                            continue;

                        double tilt = System.Math.Round(panel.Tilt(), 0) * System.Math.PI / 180;

                        List<Panel> panels_Tilt = null;
                        if(!dictionary.TryGetValue(tilt, out panels_Tilt))
                        {
                            panels_Tilt = new List<Panel>();
                            dictionary[tilt] = panels_Tilt;
                        }

                        panels_Tilt.Add(panel);
                    }

                    List<ConstructionLayer> constructionLayers_In = construction.ConstructionLayers;
                    List<ConstructionLayer> constructionLayers_Out = null;
                    List<GasMaterial> gasMaterials = null;

                    if (dictionary.Count == 1)
                    {
                        gasMaterials = null;
                        constructionLayers_Out = null;

                        if (!UpdateHeatTransferCoefficients(constructionLayers_In, dictionary.Keys.First(), materialLibrary, out gasMaterials, out constructionLayers_Out))
                            continue;

                        gasMaterials?.ForEach(x => materialLibrary.Add(x));

                        Construction construction_New = new Construction(construction, constructionLayers_Out);
                        foreach (Panel panel in panels)
                        {
                            Panel panel_New = new Panel(panel, construction_New);
                            adjacencyCluster.AddObject(panel_New);
                        }

                        constructions.Add(construction_New);
                    }
                    else
                    {
                        foreach(KeyValuePair<double, List<Panel>> keyValuePair in dictionary)
                        {
                            string name = GetSAMTypenName(construction, keyValuePair.Key);
                            if (string.IsNullOrWhiteSpace(name))
                                continue;

                            gasMaterials = null;
                            constructionLayers_Out = null;

                            if (!UpdateHeatTransferCoefficients(constructionLayers_In, keyValuePair.Key, materialLibrary, out gasMaterials, out constructionLayers_Out))
                                continue;

                            gasMaterials?.ForEach(x => materialLibrary.Add(x));

                            Construction construction_New = new Construction(construction, name);
                            construction_New = new Construction(construction_New, constructionLayers_Out);

                            foreach (Panel panel in keyValuePair.Value)
                            {
                                Panel panel_New = new Panel(panel, construction_New);
                                adjacencyCluster.AddObject(panel_New);
                            }

                            constructions.Add(construction_New);
                        }
                    }
                }
                else
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

                    List<ConstructionLayer> constructionLayers_Out = null;

                    List<ConstructionLayer> constructionLayers_In = construction.ConstructionLayers;
                    List<GasMaterial> gasMaterials = null;

                    if (!UpdateHeatTransferCoefficients(constructionLayers_In, tilt, materialLibrary, out gasMaterials, out constructionLayers_Out))
                        continue;

                    gasMaterials?.ForEach(x => materialLibrary.Add(x));

                    Construction construction_New = new Construction(construction, constructionLayers_Out);
                    foreach (Panel panel in panels)
                    {
                        Panel panel_New = new Panel(panel, construction_New);
                        adjacencyCluster.AddObject(panel_New);
                    }

                    constructions.Add(construction_New);
                }
            }

            AnalyticalModel result = new AnalyticalModel(analyticalModel, adjacencyCluster, materialLibrary, analyticalModel.ProfileLibrary);

            return result;
        }

        private static AnalyticalModel UpdateHeatTransferCoefficients_ApertureConstructions(this AnalyticalModel analyticalModel, bool duplicateApertureConstructions, out List<ApertureConstruction> apertureConstructions)
        {
            apertureConstructions = null;

            AdjacencyCluster adjacencyCluster = analyticalModel?.AdjacencyCluster;
            if (adjacencyCluster == null)
                return null;

            MaterialLibrary materialLibrary = analyticalModel.MaterialLibrary;
            if (materialLibrary == null)
                return null;

            if (adjacencyCluster == null || materialLibrary == null)
                return null;

            List<ApertureConstruction> apertureConstructions_All = adjacencyCluster.GetApertureConstructions();
            if (apertureConstructions_All == null)
                return null;

            apertureConstructions = new List<ApertureConstruction>();

            foreach (ApertureConstruction apertureConstruction in apertureConstructions_All)
            {
                if (apertureConstruction == null)
                    continue;

                if (!apertureConstruction.HasMaterial(materialLibrary, Core.MaterialType.Gas))
                    continue;

                List<Aperture> apertures = adjacencyCluster.GetApertures(apertureConstruction);
                if (apertures == null || apertures.Count == 0)
                    continue;

                if(duplicateApertureConstructions)
                {
                    Dictionary<double, List<Aperture>> dictionary = new Dictionary<double, List<Aperture>>();
                    foreach (Aperture aperture in apertures)
                    {
                        if (aperture == null)
                            continue;

                        double tilt = System.Math.Round(aperture.Tilt(), 0) * System.Math.PI / 180;

                        List<Aperture> apertures_Tilt = null;
                        if (!dictionary.TryGetValue(tilt, out apertures_Tilt))
                        {
                            apertures_Tilt = new List<Aperture>();
                            dictionary[tilt] = apertures_Tilt;
                        }

                        apertures_Tilt.Add(aperture);
                    }

                    bool paneUpdate = false;
                    bool frameUpdate = false;
                    List<ConstructionLayer> paneConstructionLayers = null;
                    List<ConstructionLayer> frameConstructionLayers = null;
                    List<GasMaterial> gasMaterials = null;

                    if (dictionary.Count == 1)
                    {
                        paneConstructionLayers = null;

                        gasMaterials = null;
                        paneUpdate = UpdateHeatTransferCoefficients(apertureConstruction.PaneConstructionLayers, dictionary.Keys.First(), materialLibrary, out gasMaterials, out paneConstructionLayers);
                        if (paneUpdate)
                            gasMaterials?.ForEach(x => materialLibrary.Add(x));

                        frameConstructionLayers = null;

                        gasMaterials = null;
                        frameUpdate = UpdateHeatTransferCoefficients(apertureConstruction.FrameConstructionLayers, dictionary.Keys.First(), materialLibrary, out gasMaterials, out frameConstructionLayers);
                        if (frameUpdate)
                            gasMaterials?.ForEach(x => materialLibrary.Add(x));

                        if (paneUpdate || frameUpdate)
                        {
                            ApertureConstruction apertureConstruction_New = new ApertureConstruction(apertureConstruction, paneConstructionLayers, frameConstructionLayers);
                            adjacencyCluster.UpdateApertures(apertures.ConvertAll(x => new Aperture(x, apertureConstruction_New)));
                            apertureConstructions.Add(apertureConstruction_New);
                        }
                    }
                    else
                    {
                        foreach (KeyValuePair<double, List<Aperture>> keyValuePair in dictionary)
                        {
                            string name = GetSAMTypenName(apertureConstruction, keyValuePair.Key);
                            if (string.IsNullOrWhiteSpace(name))
                                continue;

                            paneConstructionLayers = null;

                            gasMaterials = null;
                            paneUpdate = UpdateHeatTransferCoefficients(apertureConstruction.PaneConstructionLayers, keyValuePair.Key, materialLibrary, out gasMaterials, out paneConstructionLayers);
                            if (paneUpdate)
                                gasMaterials?.ForEach(x => materialLibrary.Add(x));

                            frameConstructionLayers = null;

                            gasMaterials = null;
                            frameUpdate = UpdateHeatTransferCoefficients(apertureConstruction.FrameConstructionLayers, keyValuePair.Key, materialLibrary, out gasMaterials, out frameConstructionLayers);
                            if (frameUpdate)
                                gasMaterials?.ForEach(x => materialLibrary.Add(x));

                            if (paneUpdate || frameUpdate)
                            {
                                ApertureConstruction apertureConstruction_New = new ApertureConstruction(apertureConstruction, name);
                                apertureConstruction_New = new ApertureConstruction(apertureConstruction_New, paneConstructionLayers, frameConstructionLayers);
                                adjacencyCluster.UpdateApertures(keyValuePair.Value.ConvertAll(x => new Aperture(x, apertureConstruction_New)));
                                apertureConstructions.Add(apertureConstruction_New);
                            }

                        }
                    }
                }
                else
                {
                    double tilt = 0;
                    double area = 0;

                    foreach (Aperture aperture in apertures)
                    {
                        double area_Aperture = aperture.GetArea();

                        tilt += aperture.Tilt() * System.Math.PI / 180 * area_Aperture;
                        area += area_Aperture;
                    }

                    if (area == 0)
                        continue;

                    tilt = tilt / area;

                    bool paneUpdate = false;
                    bool frameUpdate = false;
                    List<ConstructionLayer> paneConstructionLayers = null;
                    List<ConstructionLayer> frameConstructionLayers = null;
                    List<GasMaterial> gasMaterials = null;

                    gasMaterials = null;
                    paneUpdate = UpdateHeatTransferCoefficients(apertureConstruction.PaneConstructionLayers, tilt, materialLibrary, out gasMaterials, out paneConstructionLayers);
                    if (paneUpdate)
                        gasMaterials?.ForEach(x => materialLibrary.Add(x));

                    gasMaterials = null;
                    frameUpdate = UpdateHeatTransferCoefficients(apertureConstruction.FrameConstructionLayers, tilt, materialLibrary, out gasMaterials, out frameConstructionLayers);
                    if (frameUpdate)
                        gasMaterials?.ForEach(x => materialLibrary.Add(x));

                    if (paneUpdate || frameUpdate)
                    {
                        ApertureConstruction apertureConstruction_New = new ApertureConstruction(apertureConstruction, paneConstructionLayers, frameConstructionLayers);
                        adjacencyCluster.UpdateApertures(apertures.ConvertAll(x => new Aperture(x, apertureConstruction_New)));
                        apertureConstructions.Add(apertureConstruction_New);
                    }
                }
            }

            AnalyticalModel result = new AnalyticalModel(analyticalModel, adjacencyCluster, materialLibrary, analyticalModel.ProfileLibrary);

            return result;
        }

        private static bool UpdateHeatTransferCoefficients(IEnumerable<ConstructionLayer> constructionLayers_In, double tilt, MaterialLibrary materialLibrary, out List<GasMaterial> gasMaterials, out List<ConstructionLayer> constructionLayers_Out)
        {
            gasMaterials = null;
            constructionLayers_Out = null;

            if (constructionLayers_In == null || materialLibrary == null)
                return false;

            MaterialType materialType = constructionLayers_In.MaterialType(materialLibrary);
            if (materialType == Core.MaterialType.Undefined)
                return false;

            constructionLayers_Out = new List<ConstructionLayer>();
            gasMaterials = new List<GasMaterial>();

            bool result = false;
            foreach (ConstructionLayer constructionLayer in constructionLayers_In)
            {
                if(constructionLayer == null)
                {
                    constructionLayers_Out.Add(new ConstructionLayer(constructionLayer));
                    continue;
                }

                GasMaterial gasMaterial = constructionLayer.Material(materialLibrary) as GasMaterial;
                if (gasMaterial == null)
                {
                    constructionLayers_Out.Add(new ConstructionLayer(constructionLayer));
                    continue;
                }

                DefaultGasType defaultGasType = Query.DefaultGasType(gasMaterial);
                if (defaultGasType == Analytical.DefaultGasType.Undefined)
                {
                    constructionLayers_Out.Add(new ConstructionLayer(constructionLayer));
                    continue;
                }

                GasMaterial gasMaterial_Default = DefaultGasMaterial(defaultGasType);
                if (gasMaterial_Default == null)
                {
                    constructionLayers_Out.Add(new ConstructionLayer(constructionLayer));
                    continue;
                }

                double thickness = constructionLayer.Thickness;

                double heatTransferCoefficient = double.NaN;
                if (materialType == Core.MaterialType.Opaque && defaultGasType == Analytical.DefaultGasType.Air)
                    heatTransferCoefficient = AirspaceConvectiveHeatTransferCoefficient(tilt, thickness);
                else
                    heatTransferCoefficient = HeatTransferCoefficient(gasMaterial_Default, 15, thickness, 283, tilt);

                heatTransferCoefficient = System.Math.Round(heatTransferCoefficient, 3);

                string name = GetMaterialName(defaultGasType, thickness, heatTransferCoefficient, tilt);// string.Format("{0}_{1}mm_{2}W/m2K_{3}deg", Core.Query.Description(defaultGasType), thickness * 1000, heatTransferCoefficient, tilt_degree); //gasMaterial_Default.Name + "_"+ "Tilt: " + tilt_degree.ToString();
                if (string.IsNullOrEmpty(name))
                {
                    constructionLayers_Out.Add(new ConstructionLayer(constructionLayer));
                    continue;
                }

                GasMaterial gasMaterial_New = materialLibrary.GetObject<GasMaterial>(name);
                if (gasMaterial_New == null)
                {
                    gasMaterial_New = gasMaterials.Find(x => x.Name.Equals(name));
                    if (gasMaterial_New == null)
                    {
                        gasMaterial_New = Create.GasMaterial(gasMaterial_Default, name, name, name, thickness, heatTransferCoefficient);

                        if (gasMaterial_New == null)
                            continue;

                        gasMaterials.Add(gasMaterial_New);
                    }
                }

                result = true;
                constructionLayers_Out.Add(new ConstructionLayer(gasMaterial_New.Name, thickness));
            }

            return result;
        }
    
        private static string GetMaterialName(DefaultGasType defaultGasType, double thickness, double heatTransferCoefficient, double tilt)
        {
            if (double.IsNaN(thickness) || double.IsNaN(heatTransferCoefficient) || double.IsNaN(tilt))
                return null;
            
            double tilt_Degree = System.Math.Round(tilt * 180 / System.Math.PI, 0);
            double thickness_Millimetres = System.Math.Round(thickness * 1000, 0);
            double heatTransferCoefficient_Rounded = System.Math.Round(heatTransferCoefficient, 3);

            return string.Format("{0}_{1}mm_{2}W/m2K_{3}deg", Core.Query.Description(defaultGasType), thickness_Millimetres, heatTransferCoefficient_Rounded, tilt_Degree);
        }

        private static string GetSAMTypenName(SAMType sAMType, double tilt)
        {
            if (sAMType == null)
                return null;

            string name = sAMType.Name;
            if (string.IsNullOrEmpty(name))
                return null;

            double tilt_Degree = System.Math.Round(tilt * 180 / System.Math.PI, 0);

            return string.Format("{0}_{1}deg", name, tilt_Degree);
        }
    }
}