﻿using SAM.Core;
using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static Log Log(this AdjacencyCluster adjacencyCluster)
        {
            if (adjacencyCluster == null)
                return null;

            Log result = new Log();

            List<Panel> panels = adjacencyCluster.GetPanels();
            if (panels == null || panels.Count == 0)
            {
                result.Add("AdjacencyCluster has no panels.", LogRecordType.Warning);
                return result;
            }
            else
            {
                foreach(Panel panel in panels)
                {
                    Core.Modify.AddRange(result, panel?.Log());

                    List<Aperture> apertures = panel.Apertures;
                    if(apertures != null && apertures.Count != 0)
                    {
                        PanelGroup panelGroup_Panel = panel.PanelType.PanelGroup();
                        if(panelGroup_Panel != PanelGroup.Undefined)
                        {
                            foreach (Aperture aperture in apertures)
                            {
                                ApertureConstruction apertureConstruction = aperture.ApertureConstruction;
                                if (apertureConstruction == null)
                                    continue;

                                PanelGroup panelGroup_ApertureConstruction = apertureConstruction.PanelType().PanelGroup();
                                if(panelGroup_ApertureConstruction != PanelGroup.Undefined)
                                {
                                    string apertureName = aperture.Name;
                                    if(string.IsNullOrEmpty(apertureName))
                                        apertureName = "???";

                                    string apertureConstructionName= apertureConstruction.Name;
                                    if (string.IsNullOrEmpty(apertureConstructionName))
                                        apertureConstructionName = "???";

                                    if (panelGroup_ApertureConstruction != panelGroup_Panel)
                                        result.Add(string.Format("PanelType of {0} Panel (Guid: {1}) does not match with assigned {2} ApertureConstruction (Guid: {3}) for {4} Aperture (Guid: {5}).", panel.Name, panel.Guid, apertureConstructionName, apertureConstruction.Guid, apertureName, aperture.Guid), LogRecordType.Warning);
                                }
                            }
                        }
                        

                    }
                }
                    
            }

            List<Space> spaces = adjacencyCluster.GetSpaces();
            if(spaces == null || spaces.Count == 0)
            {
                result.Add("AdjacencyCluster has no spaces.", LogRecordType.Warning);
                return result;
            }

            foreach(Space space in spaces)
            {
                Core.Modify.AddRange(result, space?.Log());

                Shell shell = adjacencyCluster.Shell(space);
                if(shell == null || !shell.IsClosed())
                {
                    result.Add("Space {0} (Guid: {1}) is not enclosed.", LogRecordType.Warning, space.Name, space.Guid);
                    continue;
                }

                List<Space> spaces_InRange = spaces.FindAll(x => shell.InRange(x.Location));
                if (spaces_InRange == null || spaces_InRange.Count > 1)
                {
                    result.Add("There are more than one space enclosed in single shell: {0}", LogRecordType.Warning, string.Join(", ", spaces_InRange.ConvertAll(x => x.Name)));
                    continue;
                }
                
                List<Panel> panels_Space = adjacencyCluster.GetPanels(space);
                if(panels_Space == null || panels_Space.Count == 0)
                {
                    result.Add("Space {0} (Guid: {1}) is not enclosed.", LogRecordType.Warning, space.Name, space.Guid);
                    continue;
                }

                if(panels_Space.Count < 4)
                    result.Add("Space {0} (Guid: {1}) has less than 4 panels.", LogRecordType.Warning, space.Name, space.Guid);

                Panel panel_Floor = panels_Space.Find(x => Query.PanelGroup(x.PanelType) == PanelGroup.Floor);
                if(panel_Floor == null)
                    result.Add("Space {0} (Guid: {1}) has no floor panels.", LogRecordType.Warning, space.Name, space.Guid);

                foreach(Panel panel in panels_Space)
                {
                    if (panel == null)
                        continue;

                    if(panel.PanelType == PanelType.Shade || panel.PanelType == PanelType.SolarPanel || panel.PanelType == PanelType.Undefined)
                    {
                        result.Add("Panel {0} (Guid: {1}) has invalid PanelType assigned.", LogRecordType.Warning, panel.Name, panel.Guid);
                        return result;
                    }
                }
            }

            List<Construction> constructions = adjacencyCluster.GetConstructions();
            if(constructions == null || constructions.Count == 0)
            {
                result.Add("Panels in AdjacencyCluster has no constructions assigned.", LogRecordType.Error);
            }
            else
            {
                foreach(Construction construction in constructions)
                    Core.Modify.AddRange(result, construction?.Log());
            }

            List<ApertureConstruction> apertureConstructions = adjacencyCluster.GetApertureConstructions();
            if (apertureConstructions != null && apertureConstructions.Count > 0)
            {
                foreach (ApertureConstruction apertureConstruction in apertureConstructions)
                    Core.Modify.AddRange(result, apertureConstruction?.Log());
            }

            return result;
        }

        public static Log Log(this AnalyticalModel analyticalModel)
        {
            if (analyticalModel == null)
                return null;

            Log result = new Log();

            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;
            MaterialLibrary materialLibrary = analyticalModel.MaterialLibrary;


            if(adjacencyCluster == null)
                result.Add("AdjacencyCluster missing in AnalyticalModel", LogRecordType.Error);
            else
                Core.Modify.AddRange(result, adjacencyCluster?.Log());

            if(materialLibrary == null)
                result.Add("MaterialLibrary missing in AnalyticalModel", LogRecordType.Error);
            else
                Core.Modify.AddRange(result, materialLibrary?.Log());


            if(adjacencyCluster != null)
            {
                List<Construction> constructions = adjacencyCluster.GetConstructions();
                if(constructions != null && materialLibrary != null)
                {
                    foreach(Construction construction in constructions)
                        Core.Modify.AddRange(result, construction?.Log(materialLibrary));
                }

                List<ApertureConstruction> apertureConstructions = adjacencyCluster.ApertureConstructions();
                if (apertureConstructions != null && materialLibrary != null)
                {
                    foreach (ApertureConstruction apertureConstruction in apertureConstructions)
                        Core.Modify.AddRange(result, apertureConstruction?.Log(materialLibrary));
                }

                List<Panel> panels = adjacencyCluster.GetPanels();
                if(panels != null && panels.Count > 0)
                {
                    foreach (Panel panel in panels)
                        Core.Modify.AddRange(result, panel?.Log(materialLibrary));
                }

            }

            return result;
        }

        public static Log Log(this MaterialLibrary materialLibrary)
        {
            if (materialLibrary == null)
                return null;

            Log result = new Log();

            List<IMaterial> materials = materialLibrary.GetMaterials();

            if(materials == null || materials.Count == 0)
            {
                result.Add("Material Library has no Materials.", LogRecordType.Message);
                return result;
            }
            
            foreach(IMaterial material in materials)
                Core.Modify.AddRange(result, material?.Log());

            return result;
        }

        public static Log Log(this Construction construction)
        {
            if (construction == null)
                return null;

            Log result = new Log();

            List<ConstructionLayer> constructionLayers = construction?.ConstructionLayers;
            if (constructionLayers != null && constructionLayers.Count > 0)
                Core.Modify.AddRange(result, constructionLayers?.Log(construction.Name, construction.Guid));

            return result;
        }

        public static Log Log(this Construction construction, MaterialLibrary materialLibrary)
        {
            if (construction == null || materialLibrary == null)
                return null;

            Log result = new Log();

            List<ConstructionLayer> constructionLayers = construction?.ConstructionLayers;
            if (constructionLayers != null && constructionLayers.Count > 0)
                Core.Modify.AddRange(result, constructionLayers?.Log(materialLibrary, construction.Name, construction.Guid));

            double thickness;
            if(construction.TryGetValue(ConstructionParameter.DefaultThickness, out thickness))
            {
                double thickness_ConstructionLayers = construction.GetThickness();
                if(!double.IsNaN(thickness_ConstructionLayers))
                {
                    if(System.Math.Abs(thickness - thickness_ConstructionLayers) > Tolerance.MacroDistance)
                        result.Add(string.Format("Parameter {0} in {1} Construction (Guid: {2}) has different value ({3}) than thickness of its ConstructionLayers ({4})", ConstructionParameter.DefaultThickness.Name(), construction.Name, construction.Guid, thickness, thickness_ConstructionLayers), LogRecordType.Warning);
                }
            }

            return result;
        }

        public static Log Log(this ApertureConstruction apertureConstruction)
        {
            if (apertureConstruction == null)
                return null;

            Log result = new Log();

            List<ConstructionLayer> constructionLayers = null;

            constructionLayers = apertureConstruction?.PaneConstructionLayers;
            if (constructionLayers != null && constructionLayers.Count > 0)
                Core.Modify.AddRange(result, constructionLayers?.Log(apertureConstruction.Name, apertureConstruction.Guid));

            constructionLayers = apertureConstruction?.FrameConstructionLayers;
            if (constructionLayers != null && constructionLayers.Count > 0)
                Core.Modify.AddRange(result, constructionLayers?.Log(apertureConstruction.Name, apertureConstruction.Guid));

            return result;
        }

        public static Log Log(this ApertureConstruction apertureConstruction, MaterialLibrary materialLibrary)
        {
            if (apertureConstruction == null || materialLibrary == null)
                return null;

            Log result = new Log();

            List<ConstructionLayer> constructionLayers = null;

            constructionLayers = apertureConstruction?.PaneConstructionLayers;
            if (constructionLayers != null && constructionLayers.Count > 0)
                Core.Modify.AddRange(result, constructionLayers?.Log(materialLibrary, apertureConstruction.Name, apertureConstruction.Guid));

            constructionLayers = apertureConstruction?.FrameConstructionLayers;
            if (constructionLayers != null && constructionLayers.Count > 0)
                Core.Modify.AddRange(result, constructionLayers?.Log(materialLibrary, apertureConstruction.Name, apertureConstruction.Guid));

            return result;
        }

        public static Log Log(this IMaterial material)
        {
            if (material == null)
                return null;

            Log result = new Log();

            string name = material.Name;
            if(string.IsNullOrWhiteSpace(name))
            {
                result.Add(string.Format("Material (Guid: {0}) has no name assigned", material.Guid), LogRecordType.Warning);
                name = "???";
            }
                

            if (material is GasMaterial)
            {
                GasMaterial gasMaterial = (GasMaterial)material;

                if (double.IsNaN(gasMaterial.GetValue<double>(MaterialParameter.DefaultThickness)))
                    result.Add(string.Format("Default Thickness for {0} Material (Guid: {1}) has invalid value", name, material.Guid), LogRecordType.Warning);

                if (double.IsNaN(gasMaterial.GetValue<double>(MaterialParameter.VapourDiffusionFactor)))
                    result.Add(string.Format("Vapur Diffusion Factor for {0} Material (Guid: {1}) has invalid value", name, material.Guid), LogRecordType.Error);

                if (double.IsNaN(gasMaterial.GetValue<double>(GasMaterialParameter.HeatTransferCoefficient)))
                    result.Add(string.Format("Heat Transfer Coefficient for {0} Material (Guid: {1}) has invalid value", name, material.Guid), LogRecordType.Error);
            }
            else if (material is TransparentMaterial)
            {
                TransparentMaterial transparentMaterial = (TransparentMaterial)material;

                if (double.IsNaN(transparentMaterial.GetValue<double>(MaterialParameter.DefaultThickness)))
                    result.Add(string.Format("Default Thickness for {0} Material (Guid: {1}) has invalid value", name, material.Guid), LogRecordType.Warning);

                if (double.IsNaN(transparentMaterial.ThermalConductivity))
                    result.Add(string.Format("Thermal Conductivity for {0} Material (Guid: {1}) has invalid value", name, material.Guid), LogRecordType.Error);

                if (double.IsNaN(transparentMaterial.GetValue<double>(MaterialParameter.VapourDiffusionFactor)))
                    result.Add(string.Format("Vapur Diffusion Factor for {0} Material (Guid: {1}) has invalid value", name, material.Guid), LogRecordType.Error);

                if (double.IsNaN(transparentMaterial.GetValue<double>(TransparentMaterialParameter.SolarTransmittance)))
                    result.Add(string.Format("Solar Transmittance for {0} Material (Guid: {1}) has invalid value", name, material.Guid), LogRecordType.Error);
                
                if (double.IsNaN(transparentMaterial.GetValue<double>(TransparentMaterialParameter.LightTransmittance)))
                    result.Add(string.Format("Light Transmittance for {0} Material (Guid: {1}) has invalid value", name, material.Guid), LogRecordType.Error);

                if (double.IsNaN(transparentMaterial.GetValue<double>(TransparentMaterialParameter.ExternalSolarReflectance)))
                    result.Add(string.Format("External Solar Reflectance for {0} Material (Guid: {1}) has invalid value", name, material.Guid), LogRecordType.Error);

                if (double.IsNaN(transparentMaterial.GetValue<double>(TransparentMaterialParameter.InternalSolarReflectance)))
                    result.Add(string.Format("Internal Solar Reflectance for {0} Material (Guid: {1}) has invalid value", name, material.Guid), LogRecordType.Error);

                if (double.IsNaN(transparentMaterial.GetValue<double>(TransparentMaterialParameter.ExternalLightReflectance)))
                    result.Add(string.Format("External Light Reflectance for {0} Material (Guid: {1}) has invalid value", name, material.Guid), LogRecordType.Error);

                if (double.IsNaN(transparentMaterial.GetValue<double>(TransparentMaterialParameter.InternalLightReflectance)))
                    result.Add(string.Format("Internal Light Reflectance for {0} Material (Guid: {1}) has invalid value", name, material.Guid), LogRecordType.Error);

                if (double.IsNaN(transparentMaterial.GetValue<double>(TransparentMaterialParameter.ExternalEmissivity)))
                    result.Add(string.Format("External Emissivity for {0} Material (Guid: {1}) has invalid value", name, material.Guid), LogRecordType.Error);

                if (double.IsNaN(transparentMaterial.GetValue<double>(TransparentMaterialParameter.InternalEmissivity)))
                    result.Add(string.Format("Internal Emissivity for {0} Material (Guid: {1}) has invalid value", name, material.Guid), LogRecordType.Error);
            }
            else if (material is OpaqueMaterial)
            {
                OpaqueMaterial opaqueMaterial = (OpaqueMaterial)material;

                if (double.IsNaN(opaqueMaterial.GetValue<double>(MaterialParameter.DefaultThickness)))
                    result.Add(string.Format("Default Thickness for {0} Material (Guid: {1}) has invalid value", name, material.Guid), LogRecordType.Warning);

                if (double.IsNaN(opaqueMaterial.ThermalConductivity))
                    result.Add(string.Format("Thermal Conductivity for {0} Material (Guid: {1}) has invalid value", name, material.Guid), LogRecordType.Error);

                if (double.IsNaN(opaqueMaterial.SpecificHeatCapacity))
                    result.Add(string.Format("Specific Heat Capacity for {0} Material (Guid: {1}) has invalid value", name, material.Guid), LogRecordType.Error);

                if (double.IsNaN(opaqueMaterial.Density))
                    result.Add(string.Format("Density for {0} Material (Guid: {1}) has invalid value", name, material.Guid), LogRecordType.Error);

                if (double.IsNaN(opaqueMaterial.GetValue<double>(MaterialParameter.VapourDiffusionFactor)))
                    result.Add(string.Format("Vapur Diffusion Factor for {0} Material (Guid: {1}) has invalid value", name, material.Guid), LogRecordType.Error);

                if (double.IsNaN(opaqueMaterial.GetValue<double>(OpaqueMaterialParameter.ExternalSolarReflectance)))
                    result.Add(string.Format("External Solar Reflectance for {0} Material (Guid: {1}) has invalid value", name, material.Guid), LogRecordType.Error);
                 
                if (double.IsNaN(opaqueMaterial.GetValue<double>(OpaqueMaterialParameter.InternalSolarReflectance)))
                    result.Add(string.Format("Internal Solar Reflectance for {0} Material (Guid: {1}) has invalid value", name, material.Guid), LogRecordType.Error);

                if (double.IsNaN(opaqueMaterial.GetValue<double>(OpaqueMaterialParameter.ExternalLightReflectance)))
                    result.Add(string.Format("External Light Reflectance for {0} Material (Guid: {1}) has invalid value", name, material.Guid), LogRecordType.Error);

                if (double.IsNaN(opaqueMaterial.GetValue<double>(OpaqueMaterialParameter.InternalLightReflectance)))
                    result.Add(string.Format("Internal Light Reflectance for {0} Material (Guid: {1}) has invalid value", name, material.Guid), LogRecordType.Error);

                if (double.IsNaN(opaqueMaterial.GetValue<double>(OpaqueMaterialParameter.ExternalEmissivity)))
                    result.Add(string.Format("External Emissivity for {0} Material (Guid: {1}) has invalid value", name, material.Guid), LogRecordType.Error);

                if (double.IsNaN(opaqueMaterial.GetValue<double>(OpaqueMaterialParameter.InternalEmissivity)))
                    result.Add(string.Format("Internal Emissivity for {0} Material (Guid: {1}) has invalid value", name, material.Guid), LogRecordType.Error);
            }

            return result;
        }

        public static Log Log(this Panel panel)
        {
            if (panel == null)
                return null;

            Log result = new Log();

            string name = panel.Name;
            if(string.IsNullOrEmpty(name))
            {
                if (panel.PanelType != PanelType.Air)
                {
                    result.Add(string.Format("Panel (Guid: {1}) has no name.", name, panel.Guid), LogRecordType.Warning);
                    name = "???";
                } 
                else
                {
                    name = "Air";
                }
            }

            PanelType panelType = panel.PanelType;
            if(panelType == PanelType.Undefined)
                result.Add(string.Format("Panel Type for {0} Panel (Guid: {1}) is not assigned.", name, panel.Guid), LogRecordType.Error);

            double area = double.NaN;
            
            PlanarBoundary3D planarBoundary3D = panel.PlanarBoundary3D;
            if(planarBoundary3D == null)
            {
                result.Add(string.Format("{0} Panel (Guid: {1}) has no geometry assigned.", name, panel.Guid), LogRecordType.Error);
            }
            else
            {
                area = panel.GetArea();
                if(double.IsNaN(area) || area < Tolerance.MacroDistance)
                    result.Add(string.Format("{0} Panel (Guid: {1}) area is less than {2}.", name, panel.Guid, Tolerance.MacroDistance), LogRecordType.Warning);

            }

            Construction construction = panel.Construction;
            if(construction == null)
            {
                if (panelType != PanelType.Air)
                    result.Add(string.Format("{0} Panel (Guid: {1}) has no construction assigned.", name, panel.Guid), LogRecordType.Error);
            }
            else
            {
                PanelGroup panelGroup_Construction = construction.PanelType().PanelGroup();
                if(panelGroup_Construction != PanelGroup.Undefined)
                {
                    PanelGroup panelGroup_Panel = panelType.PanelGroup();
                    if (panelGroup_Panel != PanelGroup.Undefined)
                    {
                        string name_Construction = construction.Name;
                        if (string.IsNullOrWhiteSpace(name_Construction))
                            name_Construction = "???";
                        
                        if(panelGroup_Construction != panelGroup_Panel)
                            result.Add(string.Format("PanelType of {0} Panel (Guid: {1}) does not match with assigned {2} Construction (Guid: {3}).", name, panel.Guid, name_Construction, construction.Guid), LogRecordType.Warning);
                    }
                }
            }

            List<Aperture> apertures = panel.Apertures;
            if(apertures != null && apertures.Count > 0)
            {
                if(panelType == PanelType.Air)
                    result.Add(string.Format("{0} Panel (Guid: {1}) with PanelType Air hosts Apertures", name, panel.Guid), LogRecordType.Error);
                
                double area_Apertures = 0;
                foreach(Aperture aperture in apertures)
                {
                    string name_Aperture = aperture.Name;
                    if (string.IsNullOrWhiteSpace(name_Aperture))
                        name_Aperture = "???";
                    
                    Core.Modify.AddRange(result, aperture?.Log());

                    double area_Aperture = aperture.GetArea();
                    if (!double.IsNaN(area_Aperture))
                    {
                        if (!double.IsNaN(area) && area < area_Aperture)
                            result.Add(string.Format("{0} aperture (Guid: {1}) is greater than {2} panel (Guid: {3}) area", name_Aperture, aperture.Guid, name, panel.Guid), LogRecordType.Error);

                        area_Apertures += area_Aperture;
                    }

                    if (!Query.IsValid(panel, aperture))
                        result.Add(string.Format("Geometry of {0} aperture (Guid: {1}) is invalid for {2} host panel (Guid: {3})", name_Aperture, aperture.Guid, name, panel.Guid), LogRecordType.Error);

                    ApertureConstruction apertureConstruction = aperture.ApertureConstruction;
                    if(apertureConstruction == null)
                    {
                        result.Add(string.Format("{0} aperture (Guid: {1}) in {2} host panel (Guid: {3}) has no ApertureConstruction", name_Aperture, aperture.Guid, name, panel.Guid), LogRecordType.Error);
                    }
                    else
                    {
                        string text;
                        if (apertureConstruction.TryGetValue(ApertureConstructionParameter.DefaultPanelType, out text) && !string.IsNullOrWhiteSpace(text))
                        {
                            PanelType panelType_ApertureConstruction = Query.PanelType(text, false);
                            if(panelType_ApertureConstruction != PanelType.Undefined && panelType_ApertureConstruction.PanelGroup() != panelType.PanelGroup())
                                result.Add(string.Format("ApertureConstruction for {0} aperture (Guid: {1}) has diiferent Default Panel Type than its {2} host panel (Guid: {3}) has ", name_Aperture, aperture.Guid, name, panel.Guid), LogRecordType.Warning);
                        }
                    }
                }

                if(!double.IsNaN(area) && area < area_Apertures)
                    result.Add(string.Format("Overall area of apertures is greater than {0} panel (Guid: {1}) area", name, panel.Guid), LogRecordType.Error);
            }

            return result;
        }

        public static Log Log(this Panel panel, MaterialLibrary materialLibrary)
        {
            if (panel == null || materialLibrary == null)
                return null;

            string name = panel.Name;
            if (string.IsNullOrEmpty(name))
                name = "???";

            Log result = new Log();

            Construction construction = panel.Construction;
            if(construction != null)
            {
                string name_Construction = panel.Construction.Name;
                if (string.IsNullOrWhiteSpace(name_Construction))
                    name_Construction = "???";

                MaterialType materialType = Query.MaterialType(construction.ConstructionLayers, materialLibrary);
                if (materialType != MaterialType.Undefined)
                {
                    bool transparent;
                    if(panel.TryGetValue(PanelParameter.Transparent, out transparent))
                    {
                        if ((transparent && materialType != MaterialType.Transparent) || (!transparent && materialType == MaterialType.Transparent))
                            result.Add(string.Format("{0} parameter value for {1} panel (Guid: {2}) does not match witch assigned {3} construction (Guid: {4})", PanelParameter.Transparent.Name(), name, panel.Guid, name_Construction, construction.Guid), LogRecordType.Warning);
                    }

                    PanelType panelType = panel.PanelType;
                    if(panelType == PanelType.CurtainWall && materialType != MaterialType.Transparent)
                        result.Add(string.Format("Assigned {3} construction (Guid: {4}) to {1} Courtain Wall panel (Guid: {2}) is not Transparent", PanelParameter.Transparent.Name(), name, panel.Guid, name_Construction, construction.Guid), LogRecordType.Warning);

                }
            }

            return result;
        }

        public static Log Log(this Aperture aperture)
        {
            if (aperture == null)
                return null;

            Log result = new Log();

            string name = aperture.Name;
            if (string.IsNullOrEmpty(name))
            {
                result.Add(string.Format("Aperture (Guid: {1}) has no name.", name, aperture.Guid), LogRecordType.Warning);
                name = "???";
            }

            ApertureType apertureType = aperture.ApertureType;
            if (apertureType == ApertureType.Undefined)
                result.Add(string.Format("Aperture Type for {0} Panel (Guid: {1}) is not assigned.", name, aperture.Guid), LogRecordType.Error);

            PlanarBoundary3D planarBoundary3D = aperture.PlanarBoundary3D;
            if (planarBoundary3D == null)
            {
                result.Add(string.Format("{0} Aperture (Guid: {1}) has no geometry assigned.", name, aperture.Guid), LogRecordType.Error);
            }
            else
            {
                double area = aperture.GetArea();
                if (double.IsNaN(area) || area < Tolerance.MacroDistance)
                    result.Add(string.Format("{0} Aperture (Guid: {1}) area is less than {2}.", name, aperture.Guid, Tolerance.MacroDistance), LogRecordType.Warning);
            }


            ApertureConstruction apertureConstruction = aperture.ApertureConstruction;
            if(apertureConstruction == null)
                result.Add(string.Format("{0} Aperture (Guid: {1}) has no ApertureConstruction assigned.", name, aperture.Guid), LogRecordType.Error);

            return result;
        }

        public static Log Log(this Space space)
        {
            if (space == null)
                return null;

            Log result = new Log();

            string name = space.Name;
            if (string.IsNullOrEmpty(name))
            {
                result.Add(string.Format("Space (Guid: {1}) has no name.", name, space.Guid), LogRecordType.Warning);
                name = "???";
            }

            Point3D location = space.Location;
            if(location == null)
                result.Add(string.Format("{0} Space (Guid: {1}) has no location assigned.", name, space.Guid), LogRecordType.Warning);


            return result;
        }


        private static Log Log(this IEnumerable<ConstructionLayer> constructionLayers, MaterialLibrary materialLibrary, string name, System.Guid guid)
        {
            if (constructionLayers == null)
                return null;

            string name_Temp = name;
            if (string.IsNullOrEmpty(name))
                name_Temp = "???";

            Log result = new Log();

            MaterialType materialType = constructionLayers.MaterialType(materialLibrary);

            int index = 0;
            foreach (ConstructionLayer constructionLayer in constructionLayers)
            {
                IMaterial material = constructionLayer.Material(materialLibrary);
                if (material == null)
                    result.Add(string.Format("Material Library does not contain Material {0} for {1} (Guid: {2}) (Construction Layer Index: {3})", constructionLayer.Name, name_Temp, guid, index), LogRecordType.Error);

                if(material is GasMaterial)
                {
                    GasMaterial gasMaterial = (GasMaterial)material;
                    DefaultGasType defaultGasType = Query.DefaultGasType(gasMaterial);
                    if(defaultGasType == DefaultGasType.Undefined)
                        result.Add(string.Format("{0} gas material is not recogionzed in {1} (Guid: {2}) (Construction Layer Index: {3}). Heat Transfer Coefficient may not be calculated properly.", constructionLayer.Name, name_Temp, guid, index), LogRecordType.Warning);
                    else if(materialType == MaterialType.Opaque && defaultGasType != DefaultGasType.Air)
                        result.Add(string.Format("{0} Construction Layer for Opaque {1} (Guid: {2}) (Construction Layer Index: {3}) in not recognized as air type. Heat Transfer Coefficient may not be calculated properly.", constructionLayer.Name, name_Temp, guid, index), LogRecordType.Warning);

                    if (defaultGasType != DefaultGasType.Undefined)
                        result.Add(string.Format("Gas Material {0} for {1} (Guid: {2}) recognized as {3} (Construction Layer Index: {4})", constructionLayer.Name, name_Temp, guid, Core.Query.Description(defaultGasType), index), LogRecordType.Message);
                }
                index++;
            }

            return result;
        }

        private static Log Log(this IEnumerable<ConstructionLayer> constructionLayers, string name, System.Guid guid)
        {
            string name_Temp = name;
            if (string.IsNullOrEmpty(name))
                name_Temp = "???";

            Log result = new Log();
            if (constructionLayers == null || constructionLayers.Count() == 0)
            {
                result.Add(string.Format("{0} (Guid: {1}) has no construction layers", name_Temp, guid), LogRecordType.Warning);
                return result;
            }

            for (int i = 0; i < constructionLayers.Count(); i++)
            {
                ConstructionLayer constructionLayer = constructionLayers.ElementAt(i);

                if (string.IsNullOrWhiteSpace(constructionLayer.Name))
                    result.Add(string.Format("{0} (Guid: {1}) has layer with no name (Construction Layer Index: {2})", name_Temp, guid, i), LogRecordType.Error);

                if (constructionLayer.Thickness <= 0)
                    result.Add(string.Format("{0} (Guid: {1}) has layer with thickness equal or less than 0 (Construction Layer Index: {2})", name_Temp, guid, i), LogRecordType.Error);
            }

            return result;
        }
    }
}