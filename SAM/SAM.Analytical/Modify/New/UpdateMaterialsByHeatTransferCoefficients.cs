using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static void UpdateMaterialsByHeatTransferCoefficients(this BuildingModel buildingModel, bool duplicateHostPartitionTypes, bool duplicateOpeningTypes, out List<HostPartitionType> hostPartitionTypes, out List<OpeningType> openingTypes)
        {
            UpdateMaterialsByHeatTransferCoefficients_HostPartitionTypes(buildingModel, duplicateHostPartitionTypes, out hostPartitionTypes);
            UpdateMaterialsByHeatTransferCoefficients_OpeningTypes(buildingModel, duplicateOpeningTypes, out openingTypes);
        }

        public static void UpdateMaterialsByHeatTransferCoefficients(this BuildingModel buildingModel, bool duplicateHostPartitionTypes, bool duplicateOpeningTypes, double tolerance = Tolerance.MacroDistance)
        {
            if (buildingModel == null)
            {
                return;
            }

            UpdateMaterialsByHeatTransferCoefficients(buildingModel, duplicateHostPartitionTypes, duplicateOpeningTypes, out List <HostPartitionType> hostPartitionTypes, out List<OpeningType> openingTypes);
        }

        private static void UpdateMaterialsByHeatTransferCoefficients_HostPartitionTypes(this BuildingModel buildingModel, bool duplicateHostPartitionTypes, out List<HostPartitionType> hostPartitionTypes)
        {
            hostPartitionTypes = null;

            List<HostPartitionType> hostPartitionTypes_All = buildingModel?.GetHostPartitionTypes();
            if (hostPartitionTypes_All == null)
                return;

            hostPartitionTypes = new List<HostPartitionType>();

            foreach (HostPartitionType hostPartitionType in hostPartitionTypes_All)
            {
                if (hostPartitionType == null)
                    continue;

                if (!buildingModel.HasMaterial(hostPartitionType, MaterialType.Gas))
                    continue;

                List<IHostPartition> hostPartitions = buildingModel.GetHostPartitions(hostPartitionType);
                if (hostPartitions == null || hostPartitions.Count == 0)
                    continue;

                if (duplicateHostPartitionTypes)
                {
                    Dictionary<double, List<IHostPartition>> dictionary = new Dictionary<double, List<IHostPartition>>();
                    foreach (IHostPartition hostPartition in hostPartitions)
                    {
                        if (hostPartition == null)
                            continue;

                        double tilt = System.Math.Round(Geometry.Spatial.Query.Tilt(hostPartition), 0) * System.Math.PI / 180;

                        List<IHostPartition> hostPartitions_Tilt = null;
                        if (!dictionary.TryGetValue(tilt, out hostPartitions_Tilt))
                        {
                            hostPartitions_Tilt = new List<IHostPartition>();
                            dictionary[tilt] = hostPartitions_Tilt;
                        }

                        hostPartitions_Tilt.Add(hostPartition);
                    }

                    List<Architectural.MaterialLayer> materialLayers_In = hostPartitionType.MaterialLayers;
                    List<Architectural.MaterialLayer> materialLayers_Out = null;
                    List<GasMaterial> gasMaterials = null;

                    if (dictionary.Count == 1)
                    {
                        gasMaterials = null;
                        materialLayers_Out = null;

                        if (!UpdateMaterialsByHeatTransferCoefficients(materialLayers_In, dictionary.Keys.First(), buildingModel, out gasMaterials, out materialLayers_Out))
                            continue;

                        gasMaterials?.ForEach(x => buildingModel.Add(x));

                        HostPartitionType hostPartitionType_New = Create.HostPartitionType(Query.HostPartitionCategory(hostPartitionType), hostPartitionType.Name, materialLayers_Out);
                        foreach (IHostPartition hostPartition in hostPartitions)
                        {
                            hostPartition.Type(hostPartitionType_New);
                            buildingModel.Add(hostPartition);
                        }

                        hostPartitionTypes.Add(hostPartitionType_New);
                    }
                    else
                    {
                        foreach (KeyValuePair<double, List<IHostPartition>> keyValuePair in dictionary)
                        {
                            string name = GetSAMTypeName(hostPartitionType, keyValuePair.Key);
                            if (string.IsNullOrWhiteSpace(name))
                                continue;

                            gasMaterials = null;
                            materialLayers_Out = null;

                            if (!UpdateMaterialsByHeatTransferCoefficients(materialLayers_In, keyValuePair.Key, buildingModel, out gasMaterials, out materialLayers_Out))
                                continue;

                            gasMaterials?.ForEach(x => buildingModel.Add(x));

                            HostPartitionType hostPartitionType_New = Create.HostPartitionType(Query.HostPartitionCategory(hostPartitionType), name, materialLayers_Out);

                            foreach (IHostPartition hostPartition in keyValuePair.Value)
                            {
                                hostPartition.Type(hostPartitionType_New);
                                buildingModel.Add(hostPartition);
                            }

                            hostPartitionTypes.Add(hostPartitionType_New);
                        }
                    }
                }
                else
                {
                    double tilt = 0;
                    double area = 0;

                    foreach (IHostPartition hostPartition in hostPartitions)
                    {
                        double area_Panel = hostPartition.Face3D.GetArea();

                        tilt += Geometry.Spatial.Query.Tilt(hostPartition) * System.Math.PI / 180 * area_Panel;
                        area += area_Panel;
                    }

                    if (area == 0)
                        continue;

                    tilt = tilt / area;

                    List<Architectural.MaterialLayer> materialLayers_Out = null;

                    List<Architectural.MaterialLayer> materialLayers_In = hostPartitionType.MaterialLayers;
                    List<GasMaterial> gasMaterials = null;

                    if (!UpdateMaterialsByHeatTransferCoefficients(materialLayers_In, tilt, buildingModel, out gasMaterials, out materialLayers_Out))
                        continue;

                    gasMaterials?.ForEach(x => buildingModel.Add(x));

                    HostPartitionType hostPartitionType_New = Create.HostPartitionType(Query.HostPartitionCategory(hostPartitionType), hostPartitionType.Name, materialLayers_Out);
                    foreach (IHostPartition hostPartition in hostPartitions)
                    {
                        hostPartition.Type(hostPartitionType_New);
                        buildingModel.Add(hostPartition);
                    }

                    hostPartitionTypes.Add(hostPartitionType_New);
                }
            }
        }

        private static void UpdateMaterialsByHeatTransferCoefficients_OpeningTypes(this BuildingModel buildingModel, bool duplicateOpeningTypes, out List<OpeningType> openingTypes)
        {
            openingTypes = null;

            if (buildingModel == null)
                return;

            List<OpeningType> openingTypes_All = buildingModel.GetOpeningTypes();
            if (openingTypes_All == null)
                return;

            openingTypes = new List<OpeningType>();

            foreach (OpeningType openingType in openingTypes_All)
            {
                if (openingType == null)
                    continue;

                if (!buildingModel.HasMaterial(openingType, MaterialType.Gas))
                    continue;

                List<IOpening> openings = buildingModel.GetOpenings(openingType);
                if (openings == null || openings.Count == 0)
                    continue;

                if (duplicateOpeningTypes)
                {
                    Dictionary<double, List<IOpening>> dictionary = new Dictionary<double, List<IOpening>>();
                    foreach (IOpening opening in openings)
                    {
                        if (opening == null)
                            continue;

                        double tilt = System.Math.Round(Geometry.Spatial.Query.Tilt(opening), 0) * System.Math.PI / 180;

                        List<IOpening> opening_Tilt = null;
                        if (!dictionary.TryGetValue(tilt, out opening_Tilt))
                        {
                            opening_Tilt = new List<IOpening>();
                            dictionary[tilt] = opening_Tilt;
                        }

                        opening_Tilt.Add(opening);
                    }

                    bool paneUpdate = false;
                    bool frameUpdate = false;
                    List<Architectural.MaterialLayer> paneMaterialLayers = null;
                    List<Architectural.MaterialLayer> frameMaterialLayers = null;
                    List<GasMaterial> gasMaterials = null;

                    if (dictionary.Count == 1)
                    {
                        paneMaterialLayers = null;

                        gasMaterials = null;
                        paneUpdate = UpdateMaterialsByHeatTransferCoefficients(openingType.PaneMaterialLayers, dictionary.Keys.First(), buildingModel, out gasMaterials, out paneMaterialLayers);
                        if (paneUpdate)
                            gasMaterials?.ForEach(x => buildingModel.Add(x));

                        frameMaterialLayers = null;

                        gasMaterials = null;
                        frameUpdate = UpdateMaterialsByHeatTransferCoefficients(openingType.FrameMaterialLayers, dictionary.Keys.First(), buildingModel, out gasMaterials, out frameMaterialLayers);
                        if (frameUpdate)
                            gasMaterials?.ForEach(x => buildingModel.Add(x));

                        if (paneUpdate || frameUpdate)
                        {
                            OpeningType openingType_New = null;
                            if(openingType is WindowType)
                            {
                                openingType_New = new WindowType(openingType.Name, paneMaterialLayers, frameMaterialLayers);
                            }
                            else
                            {
                                openingType_New = new DoorType(openingType.Name, paneMaterialLayers, frameMaterialLayers);
                            }

                            foreach(IOpening opening in openings)
                            {
                                opening.Type(openingType_New);
                                buildingModel.Add(opening);
                            }

                            openingTypes.Add(openingType_New);
                        }
                    }
                    else
                    {
                        foreach (KeyValuePair<double, List<IOpening>> keyValuePair in dictionary)
                        {
                            string name = GetSAMTypeName(openingType, keyValuePair.Key);
                            if (string.IsNullOrWhiteSpace(name))
                                continue;

                            paneMaterialLayers = null;

                            gasMaterials = null;
                            paneUpdate = UpdateMaterialsByHeatTransferCoefficients(openingType.PaneMaterialLayers, keyValuePair.Key, buildingModel, out gasMaterials, out paneMaterialLayers);
                            if (paneUpdate)
                                gasMaterials?.ForEach(x => buildingModel.Add(x));

                            frameMaterialLayers = null;

                            gasMaterials = null;
                            frameUpdate = UpdateMaterialsByHeatTransferCoefficients(openingType.FrameMaterialLayers, keyValuePair.Key, buildingModel, out gasMaterials, out frameMaterialLayers);
                            if (frameUpdate)
                                gasMaterials?.ForEach(x => buildingModel.Add(x));

                            if (paneUpdate || frameUpdate)
                            {
                                OpeningType openingType_New = null;
                                if (openingType is WindowType)
                                {
                                    openingType_New = new WindowType(name, paneMaterialLayers, frameMaterialLayers);
                                }
                                else
                                {
                                    openingType_New = new DoorType(name, paneMaterialLayers, frameMaterialLayers);
                                }

                                foreach (IOpening opening in openings)
                                {
                                    opening.Type(openingType_New);
                                    buildingModel.Add(opening);
                                }

                                openingTypes.Add(openingType_New);
                            }

                        }
                    }
                }
                else
                {
                    double tilt = 0;
                    double area = 0;

                    foreach (Aperture aperture in openings)
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
                    List<Architectural.MaterialLayer> paneMaterialLayers = null;
                    List<Architectural.MaterialLayer> frameMaterialLayers = null;
                    List<GasMaterial> gasMaterials = null;

                    gasMaterials = null;
                    paneUpdate = UpdateMaterialsByHeatTransferCoefficients(openingType.PaneMaterialLayers, tilt, buildingModel, out gasMaterials, out paneMaterialLayers);
                    if (paneUpdate)
                        gasMaterials?.ForEach(x => buildingModel.Add(x));

                    gasMaterials = null;
                    frameUpdate = UpdateMaterialsByHeatTransferCoefficients(openingType.FrameMaterialLayers, tilt, buildingModel, out gasMaterials, out frameMaterialLayers);
                    if (frameUpdate)
                        gasMaterials?.ForEach(x => buildingModel.Add(x));

                    if (paneUpdate || frameUpdate)
                    {
                        OpeningType openingType_New = null;
                        if (openingType is WindowType)
                        {
                            openingType_New = new WindowType(openingType.Name, paneMaterialLayers, frameMaterialLayers);
                        }
                        else
                        {
                            openingType_New = new DoorType(openingType.Name, paneMaterialLayers, frameMaterialLayers);
                        }

                        foreach (IOpening opening in openings)
                        {
                            opening.Type(openingType_New);
                            buildingModel.Add(opening);
                        }

                        openingTypes.Add(openingType_New);
                    }
                }
            }
        }

        private static bool UpdateMaterialsByHeatTransferCoefficients(IEnumerable<Architectural.MaterialLayer> materialLayers_In, double tilt, BuildingModel buildingModel, out List<GasMaterial> gasMaterials, out List<Architectural.MaterialLayer> materialLayers_Out)
        {
            gasMaterials = null;
            materialLayers_Out = null;

            if (materialLayers_In == null || buildingModel == null)
                return false;

            MaterialType materialType = buildingModel.GetMaterialType(materialLayers_In);
            if (materialType == MaterialType.Undefined)
                return false;

            materialLayers_Out = new List<Architectural.MaterialLayer>();
            gasMaterials = new List<GasMaterial>();

            bool result = false;
            foreach (Architectural.MaterialLayer materialLayer in materialLayers_In)
            {
                if (materialLayer == null)
                {
                    materialLayers_Out.Add(new Architectural.MaterialLayer(materialLayer));
                    continue;
                }

                GasMaterial gasMaterial = buildingModel.GetMaterial<GasMaterial>(materialLayer);
                if (gasMaterial == null)
                {
                    materialLayers_Out.Add(new Architectural.MaterialLayer(materialLayer));
                    continue;
                }

                DefaultGasType defaultGasType = Query.DefaultGasType(gasMaterial);
                if (defaultGasType == DefaultGasType.Undefined)
                {
                    materialLayers_Out.Add(new Architectural.MaterialLayer(materialLayer));
                    continue;
                }

                GasMaterial gasMaterial_Default = Query.DefaultGasMaterial(defaultGasType);
                if (gasMaterial_Default == null)
                {
                    materialLayers_Out.Add(new Architectural.MaterialLayer(materialLayer));
                    continue;
                }

                double thickness = materialLayer.Thickness;

                double heatTransferCoefficient = double.NaN;
                if (materialType == MaterialType.Opaque && defaultGasType == DefaultGasType.Air)
                    heatTransferCoefficient = Query.AirspaceConvectiveHeatTransferCoefficient(tilt, thickness);
                else
                    heatTransferCoefficient = Query.HeatTransferCoefficient(gasMaterial_Default, thickness, tilt);

                heatTransferCoefficient = System.Math.Round(heatTransferCoefficient, 3);

                string name = GetMaterialName(defaultGasType, thickness, heatTransferCoefficient, tilt);// string.Format("{0}_{1}mm_{2}W/m2K_{3}deg", Core.Query.Description(defaultGasType), thickness * 1000, heatTransferCoefficient, tilt_degree); //gasMaterial_Default.Name + "_"+ "Tilt: " + tilt_degree.ToString();
                if (string.IsNullOrEmpty(name))
                {
                    materialLayers_Out.Add(new Architectural.MaterialLayer(materialLayer));
                    continue;
                }

                GasMaterial gasMaterial_New = buildingModel.GetMaterial<GasMaterial>(name);
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
                materialLayers_Out.Add(new ConstructionLayer(gasMaterial_New.Name, thickness));
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

        private static string GetSAMTypeName(SAMType sAMType, double tilt)
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