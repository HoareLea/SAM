using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        //public static void UpdateHeatTransferCoefficients(this ArchitecturalModel architecturalModel, bool duplicateHostPartitionTypes, bool duplicateOpeningTypes, out List<HostPartitionType> hostPartitionTypes, out List<OpeningType> openingTypes)
        //{
        //    UpdateHeatTransferCoefficients_Constructions(architecturalModel, duplicateHostPartitionTypes, out hostPartitionTypes);
        //    UpdateHeatTransferCoefficients_ApertureConstructions(architecturalModel, duplicateOpeningTypes, out openingTypes);
        //}

        //private static void UpdateHeatTransferCoefficients_Constructions(this ArchitecturalModel architecturalModel, bool duplicateHostPartitionTypes, out List<HostPartitionType> hostPartitionTypes)
        //{
        //    hostPartitionTypes = null;

        //    List<HostPartitionType> hostPartitionTypes_All = architecturalModel?.GetHostPartitionTypes();
        //    if (hostPartitionTypes_All == null)
        //        return;

        //    hostPartitionTypes = new List<HostPartitionType>();

        //    foreach (HostPartitionType hostPartitionType in hostPartitionTypes_All)
        //    {
        //        if (hostPartitionType == null)
        //            continue;

        //        if (!architecturalModel.HasMaterial(hostPartitionType, Core.MaterialType.Gas))
        //            continue;

        //        List<IHostPartition> hostPartitions = architecturalModel.GetHostPartitions(hostPartitionType);
        //        if (hostPartitions == null || hostPartitions.Count == 0)
        //            continue;

        //        if(duplicateHostPartitionTypes)
        //        {
        //            Dictionary<double, List<IHostPartition>> dictionary = new Dictionary<double, List<IHostPartition>>();
        //            foreach(IHostPartition hostPartition in hostPartitions)
        //            {
        //                if (hostPartition == null)
        //                    continue;

        //                double tilt = System.Math.Round(Geometry.Spatial.Query.Tilt(hostPartition), 0) * System.Math.PI / 180;

        //                List<IHostPartition> hostPartitions_Tilt = null;
        //                if(!dictionary.TryGetValue(tilt, out hostPartitions_Tilt))
        //                {
        //                    hostPartitions_Tilt = new List<IHostPartition>();
        //                    dictionary[tilt] = hostPartitions_Tilt;
        //                }

        //                hostPartitions_Tilt.Add(hostPartition);
        //            }

        //            List <Architectural.MaterialLayer> materialLayers_In = hostPartitionType.MaterialLayers;
        //            List<Architectural.MaterialLayer> materialLayers_Out = null;
        //            List<GasMaterial> gasMaterials = null;

        //            if (dictionary.Count == 1)
        //            {
        //                gasMaterials = null;
        //                materialLayers_Out = null;

        //                if (!UpdateHeatTransferCoefficients(materialLayers_In, dictionary.Keys.First(), architecturalModel, out gasMaterials, out materialLayers_Out))
        //                    continue;

        //                gasMaterials?.ForEach(x => architecturalModel.Add(x));

        //                Construction construction_New = new Construction(hostPartitionType, materialLayers_Out);
        //                foreach (Panel panel in panels)
        //                {
        //                    Panel panel_New = new Panel(panel, construction_New);
        //                    adjacencyCluster.AddObject(panel_New);
        //                }

        //                hostPartitionTypes.Add(construction_New);
        //            }
        //            else
        //            {
        //                foreach(KeyValuePair<double, List<Panel>> keyValuePair in dictionary)
        //                {
        //                    string name = GetSAMTypenName(hostPartitionType, keyValuePair.Key);
        //                    if (string.IsNullOrWhiteSpace(name))
        //                        continue;

        //                    gasMaterials = null;
        //                    materialLayers_Out = null;

        //                    if (!UpdateHeatTransferCoefficients(materialLayers_In, keyValuePair.Key, materialLibrary, out gasMaterials, out materialLayers_Out))
        //                        continue;

        //                    gasMaterials?.ForEach(x => materialLibrary.Add(x));

        //                    Construction construction_New = new Construction(hostPartitionType, name);
        //                    construction_New = new Construction(construction_New, materialLayers_Out);

        //                    foreach (Panel panel in keyValuePair.Value)
        //                    {
        //                        Panel panel_New = new Panel(panel, construction_New);
        //                        adjacencyCluster.AddObject(panel_New);
        //                    }

        //                    hostPartitionTypes.Add(construction_New);
        //                }
        //            }
        //        }
        //        else
        //        {
        //            double tilt = 0;
        //            double area = 0;

        //            foreach (Panel panel in panels)
        //            {
        //                double area_Panel = panel.GetArea();

        //                tilt += panel.Tilt() * System.Math.PI / 180 * area_Panel;
        //                area += area_Panel;
        //            }

        //            if (area == 0)
        //                continue;

        //            tilt = tilt / area;

        //            List<ConstructionLayer> constructionLayers_Out = null;

        //            List<ConstructionLayer> constructionLayers_In = hostPartitionType.ConstructionLayers;
        //            List<GasMaterial> gasMaterials = null;

        //            if (!UpdateHeatTransferCoefficients(constructionLayers_In, tilt, materialLibrary, out gasMaterials, out constructionLayers_Out))
        //                continue;

        //            gasMaterials?.ForEach(x => materialLibrary.Add(x));

        //            Construction construction_New = new Construction(hostPartitionType, constructionLayers_Out);
        //            foreach (Panel panel in panels)
        //            {
        //                Panel panel_New = new Panel(panel, construction_New);
        //                adjacencyCluster.AddObject(panel_New);
        //            }

        //            hostPartitionTypes.Add(construction_New);
        //        }
        //    }

        //    AnalyticalModel result = new AnalyticalModel(architecturalModel, adjacencyCluster, materialLibrary, architecturalModel.ProfileLibrary);

        //    return result;
        //}

        //private static AnalyticalModel UpdateHeatTransferCoefficients_ApertureConstructions(this AnalyticalModel analyticalModel, bool duplicateApertureConstructions, out List<ApertureConstruction> apertureConstructions)
        //{
        //    apertureConstructions = null;

        //    AdjacencyCluster adjacencyCluster = analyticalModel?.AdjacencyCluster;
        //    if (adjacencyCluster == null)
        //        return null;

        //    MaterialLibrary materialLibrary = analyticalModel.MaterialLibrary;
        //    if (materialLibrary == null)
        //        return null;

        //    if (adjacencyCluster == null || materialLibrary == null)
        //        return null;

        //    List<ApertureConstruction> apertureConstructions_All = adjacencyCluster.GetApertureConstructions();
        //    if (apertureConstructions_All == null)
        //        return null;

        //    apertureConstructions = new List<ApertureConstruction>();

        //    foreach (ApertureConstruction apertureConstruction in apertureConstructions_All)
        //    {
        //        if (apertureConstruction == null)
        //            continue;

        //        if (!apertureConstruction.HasMaterial(materialLibrary, Core.MaterialType.Gas))
        //            continue;

        //        List<Aperture> apertures = adjacencyCluster.GetApertures(apertureConstruction);
        //        if (apertures == null || apertures.Count == 0)
        //            continue;

        //        if(duplicateApertureConstructions)
        //        {
        //            Dictionary<double, List<Aperture>> dictionary = new Dictionary<double, List<Aperture>>();
        //            foreach (Aperture aperture in apertures)
        //            {
        //                if (aperture == null)
        //                    continue;

        //                double tilt = System.Math.Round(aperture.Tilt(), 0) * System.Math.PI / 180;

        //                List<Aperture> apertures_Tilt = null;
        //                if (!dictionary.TryGetValue(tilt, out apertures_Tilt))
        //                {
        //                    apertures_Tilt = new List<Aperture>();
        //                    dictionary[tilt] = apertures_Tilt;
        //                }

        //                apertures_Tilt.Add(aperture);
        //            }

        //            bool paneUpdate = false;
        //            bool frameUpdate = false;
        //            List<ConstructionLayer> paneConstructionLayers = null;
        //            List<ConstructionLayer> frameConstructionLayers = null;
        //            List<GasMaterial> gasMaterials = null;

        //            if (dictionary.Count == 1)
        //            {
        //                paneConstructionLayers = null;

        //                gasMaterials = null;
        //                paneUpdate = UpdateHeatTransferCoefficients(apertureConstruction.PaneConstructionLayers, dictionary.Keys.First(), materialLibrary, out gasMaterials, out paneConstructionLayers);
        //                if (paneUpdate)
        //                    gasMaterials?.ForEach(x => materialLibrary.Add(x));

        //                frameConstructionLayers = null;

        //                gasMaterials = null;
        //                frameUpdate = UpdateHeatTransferCoefficients(apertureConstruction.FrameConstructionLayers, dictionary.Keys.First(), materialLibrary, out gasMaterials, out frameConstructionLayers);
        //                if (frameUpdate)
        //                    gasMaterials?.ForEach(x => materialLibrary.Add(x));

        //                if (paneUpdate || frameUpdate)
        //                {
        //                    ApertureConstruction apertureConstruction_New = new ApertureConstruction(apertureConstruction, paneConstructionLayers, frameConstructionLayers);
        //                    adjacencyCluster.UpdateApertures(apertures.ConvertAll(x => new Aperture(x, apertureConstruction_New)));
        //                    apertureConstructions.Add(apertureConstruction_New);
        //                }
        //            }
        //            else
        //            {
        //                foreach (KeyValuePair<double, List<Aperture>> keyValuePair in dictionary)
        //                {
        //                    string name = GetSAMTypenName(apertureConstruction, keyValuePair.Key);
        //                    if (string.IsNullOrWhiteSpace(name))
        //                        continue;

        //                    paneConstructionLayers = null;

        //                    gasMaterials = null;
        //                    paneUpdate = UpdateHeatTransferCoefficients(apertureConstruction.PaneConstructionLayers, keyValuePair.Key, materialLibrary, out gasMaterials, out paneConstructionLayers);
        //                    if (paneUpdate)
        //                        gasMaterials?.ForEach(x => materialLibrary.Add(x));

        //                    frameConstructionLayers = null;

        //                    gasMaterials = null;
        //                    frameUpdate = UpdateHeatTransferCoefficients(apertureConstruction.FrameConstructionLayers, keyValuePair.Key, materialLibrary, out gasMaterials, out frameConstructionLayers);
        //                    if (frameUpdate)
        //                        gasMaterials?.ForEach(x => materialLibrary.Add(x));

        //                    if (paneUpdate || frameUpdate)
        //                    {
        //                        ApertureConstruction apertureConstruction_New = new ApertureConstruction(apertureConstruction, name);
        //                        apertureConstruction_New = new ApertureConstruction(apertureConstruction_New, paneConstructionLayers, frameConstructionLayers);
        //                        adjacencyCluster.UpdateApertures(keyValuePair.Value.ConvertAll(x => new Aperture(x, apertureConstruction_New)));
        //                        apertureConstructions.Add(apertureConstruction_New);
        //                    }

        //                }
        //            }
        //        }
        //        else
        //        {
        //            double tilt = 0;
        //            double area = 0;

        //            foreach (Aperture aperture in apertures)
        //            {
        //                double area_Aperture = aperture.GetArea();

        //                tilt += aperture.Tilt() * System.Math.PI / 180 * area_Aperture;
        //                area += area_Aperture;
        //            }

        //            if (area == 0)
        //                continue;

        //            tilt = tilt / area;

        //            bool paneUpdate = false;
        //            bool frameUpdate = false;
        //            List<ConstructionLayer> paneConstructionLayers = null;
        //            List<ConstructionLayer> frameConstructionLayers = null;
        //            List<GasMaterial> gasMaterials = null;

        //            gasMaterials = null;
        //            paneUpdate = UpdateHeatTransferCoefficients(apertureConstruction.PaneConstructionLayers, tilt, materialLibrary, out gasMaterials, out paneConstructionLayers);
        //            if (paneUpdate)
        //                gasMaterials?.ForEach(x => materialLibrary.Add(x));

        //            gasMaterials = null;
        //            frameUpdate = UpdateHeatTransferCoefficients(apertureConstruction.FrameConstructionLayers, tilt, materialLibrary, out gasMaterials, out frameConstructionLayers);
        //            if (frameUpdate)
        //                gasMaterials?.ForEach(x => materialLibrary.Add(x));

        //            if (paneUpdate || frameUpdate)
        //            {
        //                ApertureConstruction apertureConstruction_New = new ApertureConstruction(apertureConstruction, paneConstructionLayers, frameConstructionLayers);
        //                adjacencyCluster.UpdateApertures(apertures.ConvertAll(x => new Aperture(x, apertureConstruction_New)));
        //                apertureConstructions.Add(apertureConstruction_New);
        //            }
        //        }
        //    }

        //    AnalyticalModel result = new AnalyticalModel(analyticalModel, adjacencyCluster, materialLibrary, analyticalModel.ProfileLibrary);

        //    return result;
        //}

        //private static bool UpdateHeatTransferCoefficients(IEnumerable<Architectural.MaterialLayer> materialLayers_In, double tilt, ArchitecturalModel architecturalModel, out List<GasMaterial> gasMaterials, out List<Architectural.MaterialLayer> materialLayers_Out)
        //{
        //    gasMaterials = null;
        //    materialLayers_Out = null;

        //    if (materialLayers_In == null || architecturalModel == null)
        //        return false;

        //    MaterialType materialType = architecturalModel.GetMaterialType(materialLayers_In);
        //    if (materialType == Core.MaterialType.Undefined)
        //        return false;

        //    materialLayers_Out = new List<Architectural.MaterialLayer>();
        //    gasMaterials = new List<GasMaterial>();

        //    bool result = false;
        //    foreach (Architectural.MaterialLayer materialLayer in materialLayers_In)
        //    {
        //        if(materialLayer == null)
        //        {
        //            materialLayers_Out.Add(new Architectural.MaterialLayer(materialLayer));
        //            continue;
        //        }

        //        GasMaterial gasMaterial = architecturalModel.GetMaterial<GasMaterial>(materialLayer);
        //        if (gasMaterial == null)
        //        {
        //            materialLayers_Out.Add(new Architectural.MaterialLayer(materialLayer));
        //            continue;
        //        }

        //        DefaultGasType defaultGasType = DefaultGasType(gasMaterial);
        //        if (defaultGasType == Analytical.DefaultGasType.Undefined)
        //        {
        //            materialLayers_Out.Add(new Architectural.MaterialLayer(materialLayer));
        //            continue;
        //        }

        //        GasMaterial gasMaterial_Default = DefaultGasMaterial(defaultGasType);
        //        if (gasMaterial_Default == null)
        //        {
        //            materialLayers_Out.Add(new Architectural.MaterialLayer(materialLayer));
        //            continue;
        //        }

        //        double thickness = materialLayer.Thickness;

        //        double heatTransferCoefficient = double.NaN;
        //        if (materialType == Core.MaterialType.Opaque && defaultGasType == Analytical.DefaultGasType.Air)
        //            heatTransferCoefficient = AirspaceConvectiveHeatTransferCoefficient(tilt, thickness);
        //        else
        //            heatTransferCoefficient = HeatTransferCoefficient(gasMaterial_Default, thickness, tilt);

        //        heatTransferCoefficient = System.Math.Round(heatTransferCoefficient, 3);

        //        string name = GetMaterialName(defaultGasType, thickness, heatTransferCoefficient, tilt);// string.Format("{0}_{1}mm_{2}W/m2K_{3}deg", Core.Query.Description(defaultGasType), thickness * 1000, heatTransferCoefficient, tilt_degree); //gasMaterial_Default.Name + "_"+ "Tilt: " + tilt_degree.ToString();
        //        if (string.IsNullOrEmpty(name))
        //        {
        //            materialLayers_Out.Add(new Architectural.MaterialLayer(materialLayer));
        //            continue;
        //        }

        //        GasMaterial gasMaterial_New = architecturalModel.GetMaterial<GasMaterial>(name);
        //        if (gasMaterial_New == null)
        //        {
        //            gasMaterial_New = gasMaterials.Find(x => x.Name.Equals(name));
        //            if (gasMaterial_New == null)
        //            {
        //                gasMaterial_New = Create.GasMaterial(gasMaterial_Default, name, name, name, thickness, heatTransferCoefficient);

        //                if (gasMaterial_New == null)
        //                    continue;

        //                gasMaterials.Add(gasMaterial_New);
        //            }
        //        }

        //        result = true;
        //        materialLayers_Out.Add(new ConstructionLayer(gasMaterial_New.Name, thickness));
        //    }

        //    return result;
        //}
    }
}