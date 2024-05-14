using SAM.Core;
using SAM.Geometry.Spatial;
using System;
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

            List<Space> spaces = adjacencyCluster.GetSpaces();
            if (spaces == null || spaces.Count == 0)
            {
                result.Add("AdjacencyCluster has no spaces.", LogRecordType.Warning);
                return result;
            }

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

                    if (spaces != null && spaces.Count != 0)
                    {
                        List<Space> spaces_Panel = adjacencyCluster.GetRelatedObjects<Space>(panel);
                        if(spaces_Panel != null && spaces_Panel.Count != 0)
                        {
                            PanelType panelType = panel.PanelType;
                            switch (panelType)
                            {
                                case PanelType.Air:
                                case PanelType.Ceiling:
                                case PanelType.FloorInternal:
                                case PanelType.FloorRaised:
                                case PanelType.UndergroundCeiling:
                                case PanelType.WallInternal:
                                    if (spaces_Panel == null || spaces_Panel.Count == 0)
                                    {
                                        result.Add("{0} Panel {1} (Guid: {2}) has no adjacent spaces.", LogRecordType.Warning, panelType.Text(), panel.Name, panel.Guid);
                                    }
                                    else if (spaces_Panel.Count < 2 && !Query.Adiabatic(panel))
                                    {
                                        result.Add("{0} Panel {1} (Guid: {2}) has not enough adjacent spaces.", LogRecordType.Warning, panelType.Text(), panel.Name, panel.Guid);
                                    }
                                    break;

                                case PanelType.FloorExposed:
                                case PanelType.Roof:
                                case PanelType.SlabOnGrade:
                                case PanelType.UndergroundSlab:
                                case PanelType.UndergroundWall:
                                case PanelType.WallExternal:
                                    if (spaces_Panel.Count > 1)
                                        result.Add("{0} Panel {1} (Guid: {2}) has more than one adjacent spaces.", LogRecordType.Warning, panelType.Text(), panel.Name, panel.Guid);
                                    break;

                                case PanelType.Shade:
                                case PanelType.SolarPanel:
                                    result.Add("{0} Panel {1} (Guid: {2}) has some adjacent spaces.", LogRecordType.Warning, panelType.Text(), panel.Name, panel.Guid);
                                    break;
                            }
                        }
                    }
                }
            }

            HashSet<string> spaceNames = new HashSet<string>();
            foreach(Space space in spaces)
            {
                Core.Modify.AddRange(result, space?.Log());

                Shell shell = adjacencyCluster.Shell(space);
                if(shell == null || !shell.IsClosed())
                {
                    result.Add("Space {0} (Guid: {1}) is not enclosed (with 1e-6 tolerance).", LogRecordType.Warning, space.Name, space.Guid);
                    continue;
                }

                if(space.Location == null)
                {
                    result.Add("Space {0} (Guid: {1}) has no location.", LogRecordType.Warning, space.Name, space.Guid);
                    continue;
                }

                if(space.Name != null)
                {
                    if (spaceNames.Contains(space.Name))
                    {
                        result.Add("Space {0} (Guid: {1}) name is duplicated", LogRecordType.Warning, space.Name, space.Guid);
                        continue;
                    }

                    spaceNames.Add(space.Name);
                }

                List<Panel> panels_Space = adjacencyCluster.GetPanels(space);
                if(panels_Space == null || panels_Space.Count == 0)
                {
                    result.Add("Space {0} (Guid: {1}) is not enclosed.", LogRecordType.Warning, space.Name, space.Guid);
                    continue;
                }

                if(panels_Space.Count < 4)
                    result.Add("Space {0} (Guid: {1}) has less than 4 panels.", LogRecordType.Message, space.Name, space.Guid);

                Panel panel_Floor = panels_Space.Find(x => Query.PanelGroup(x.PanelType) == PanelGroup.Floor);
                if(panel_Floor == null)
                {
                    Panel panel_Air = panels_Space.Find(x => x.PanelType == PanelType.Air);
                    if (panel_Air != null)
                        result.Add("Space {0} (Guid: {1}) has no floor panels but has air panels.", LogRecordType.Message, space.Name, space.Guid);
                    else
                        result.Add("Space {0} (Guid: {1}) has no floor panels and air panels.", LogRecordType.Message, space.Name, space.Guid);
                }

                foreach(Panel panel in panels_Space)
                {
                    if (panel == null)
                        continue;

                    if(panel.PanelType == PanelType.Shade || panel.PanelType == PanelType.SolarPanel || panel.PanelType == PanelType.Undefined)
                    {
                        result.Add("Panel {0} (Guid: {1}) has assigned {2} PanelType and it also encloses {3} space (Guid: {4}).", LogRecordType.Warning, panel.Name, panel.Guid, panel.PanelType, space.Name, space.Guid);
                        return result;
                    }
                }
            }

            Dictionary<Shell, List<Space>> dictionary = Query.DuplicatedSpacesDictionary(adjacencyCluster);
            if(dictionary != null && dictionary.Count > 0)
            {
                foreach(List<Space> spaces_Duplicated in dictionary.Values)
                {
                    List<string> names = spaces_Duplicated.ConvertAll(x => x?.Name);
                    for (int i = 0; i < names.Count; i++)
                        if (string.IsNullOrWhiteSpace(names[i]))
                            names[i] = "???";

                    List<string> guids = spaces_Duplicated.ConvertAll(x => x.Guid.ToString());

                    result.Add("Spaces {0} (Guids: {1}) are enclosed in single shell.", LogRecordType.Message, string.Join(", ", names), string.Join(", ", guids));
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
            ProfileLibrary profileLibrary = analyticalModel.ProfileLibrary;


            if(adjacencyCluster == null)
                result.Add("AdjacencyCluster missing in AnalyticalModel", LogRecordType.Error);
            else
                Core.Modify.AddRange(result, adjacencyCluster?.Log());

            if(materialLibrary == null)
                result.Add("MaterialLibrary missing in AnalyticalModel", LogRecordType.Error);
            else
                Core.Modify.AddRange(result, materialLibrary.Log());

            if (profileLibrary == null)
                result.Add("ProfileLibrary missing in AnalyticalModel", LogRecordType.Error);
            else
                Core.Modify.AddRange(result, profileLibrary.Log());


            if (adjacencyCluster != null)
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
                if(panels != null && panels.Count != 0)
                {
                    foreach (Panel panel in panels)
                        Core.Modify.AddRange(result, panel?.Log(materialLibrary));
                }

                List<Space> spaces = adjacencyCluster.GetSpaces();
                if(spaces != null && spaces.Count != 0)
                {
                    foreach (Space space in spaces)
                        Core.Modify.AddRange(result, space?.Log(profileLibrary));
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

        public static Log Log(this ProfileLibrary profileLibrary)
        {
            if (profileLibrary == null)
                return null;

            Log result = new Log();

            List<Profile> profiles = profileLibrary.GetProfiles();

            if (profiles == null || profiles.Count == 0)
            {
                result.Add("Profile Library has no Materials.", LogRecordType.Message);
                return result;
            }

            foreach (Profile profile in profiles)
                Core.Modify.AddRange(result, profile?.Log());

            return result;
        }

        public static Log Log(this Construction construction)
        {
            if (construction == null)
                return null;

            Log result = new Log();

            string name = construction.Name;
            if (string.IsNullOrEmpty(name))
            {
                result.Add(string.Format("apertureConstruction (Guid: {1}) has no name.", name, construction.Guid), LogRecordType.Warning);
                name = "???";
            }

            PanelType panelType = PanelType.Undefined;
            string text;
            if (construction.TryGetValue(ConstructionParameter.DefaultPanelType, out text) && !string.IsNullOrWhiteSpace(text))
                panelType = Query.PanelType(text, false);

            if(panelType != PanelType.Air)
            {
                List<ConstructionLayer> constructionLayers = construction?.ConstructionLayers;
                if (constructionLayers != null && constructionLayers.Count > 0)
                    Core.Modify.AddRange(result, constructionLayers?.Log(construction.Name, construction.Guid));
                else
                    result.Add(string.Format("{0} Construction (Guid: {1}) has no ConstructionLayers.", name, construction.Guid), LogRecordType.Warning);
            }

            return result;
        }

        public static Log Log(this Construction construction, MaterialLibrary materialLibrary)
        {
            if (construction == null || materialLibrary == null)
                return null;

            Log result = new Log();

            List<ConstructionLayer> constructionLayers = construction?.ConstructionLayers;
            if (constructionLayers != null && constructionLayers.Count > 0)
            {
                Core.Modify.AddRange(result, constructionLayers?.Log(materialLibrary, construction.Name, construction.Guid));

                IMaterial material = null;

                material = materialLibrary.GetMaterial(constructionLayers.First()?.Name);
                if(material is GasMaterial)
                {
                    result.Add(string.Format("First construction layer (Name: {1}) for Construction (Name: {2} Guid: {3}) shall not be gas type", material.Name, construction.Name, construction.Guid), LogRecordType.Warning);
                }

                material = materialLibrary.GetMaterial(constructionLayers.Last()?.Name);
                if (material is GasMaterial)
                {
                    result.Add(string.Format("Last construction layer (Name: {1}) for Construction (Name: {2} Guid: {3}) shall not be gas type", material.Name, construction.Name, construction.Guid), LogRecordType.Warning);
                }
            }

            double thickness;
            if(construction.TryGetValue(ConstructionParameter.DefaultThickness, out thickness))
            {
                double thickness_ConstructionLayers = construction.GetThickness();
                if(!double.IsNaN(thickness_ConstructionLayers))
                {
                    if(System.Math.Abs(thickness - thickness_ConstructionLayers) > Tolerance.MacroDistance)
                    {
                        result.Add(string.Format("Parameter {0} in {1} Construction (Guid: {2}) has different value ({3}) than thickness of its ConstructionLayers ({4})", ConstructionParameter.DefaultThickness.Name(), construction.Name, construction.Guid, thickness, thickness_ConstructionLayers), LogRecordType.Message);
                    }
                }
            }

            return result;
        }

        public static Log Log(this ApertureConstruction apertureConstruction)
        {
            if (apertureConstruction == null)
                return null;

            Log result = new Log();

            string name = apertureConstruction.Name;
            if (string.IsNullOrEmpty(name))
            {
                result.Add(string.Format("apertureConstruction (Guid: {1}) has no name.", name, apertureConstruction.Guid), LogRecordType.Warning);
                name = "???";
            }

            List<ConstructionLayer> constructionLayers = null;

            constructionLayers = apertureConstruction?.PaneConstructionLayers;
            if (constructionLayers != null && constructionLayers.Count > 0)
                Core.Modify.AddRange(result, constructionLayers?.Log(apertureConstruction.Name, apertureConstruction.Guid));
            else
                result.Add(string.Format("{0} ApertureConstruction (Guid: {1}) has no Pane ConstructionLayers.", name, apertureConstruction.Guid), LogRecordType.Warning);

            constructionLayers = apertureConstruction?.FrameConstructionLayers;
            if (constructionLayers != null && constructionLayers.Count > 0)
                Core.Modify.AddRange(result, constructionLayers?.Log(apertureConstruction.Name, apertureConstruction.Guid));
            else
                result.Add(string.Format("{0} ApertureConstruction (Guid: {1}) has no Frame ConstructionLayers.", name, apertureConstruction.Guid), LogRecordType.Warning);

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
            {
                Core.Modify.AddRange(result, constructionLayers?.Log(materialLibrary, apertureConstruction.Name, apertureConstruction.Guid));

                IMaterial material = null;

                material = materialLibrary.GetMaterial(constructionLayers.First()?.Name);
                if (material is GasMaterial)
                {
                    result.Add(string.Format("First aperture construction pane layer (Name: {1}) for ApertureConstruction (Name: {2} Guid: {3}) shall not be gas type", material.Name, apertureConstruction.Name, apertureConstruction.Guid), LogRecordType.Warning);
                }

                material = materialLibrary.GetMaterial(constructionLayers.Last()?.Name);
                if (material is GasMaterial)
                {
                    result.Add(string.Format("Last aperture construction pane layer (Name: {1}) for ApertureConstruction (Name: {2} Guid: {3}) shall not be gas type", material.Name, apertureConstruction.Name, apertureConstruction.Guid), LogRecordType.Warning);
                }
            }

            constructionLayers = apertureConstruction?.FrameConstructionLayers;
            if (constructionLayers != null && constructionLayers.Count > 0)
            {
                Core.Modify.AddRange(result, constructionLayers?.Log(materialLibrary, apertureConstruction.Name, apertureConstruction.Guid));

                IMaterial material = null;

                material = materialLibrary.GetMaterial(constructionLayers.First()?.Name);
                if (material is GasMaterial)
                {
                    result.Add(string.Format("First aperture construction frame layer (Name: {1}) for ApertureConstruction (Name: {2} Guid: {3}) shall not be gas type", material.Name, apertureConstruction.Name, apertureConstruction.Guid), LogRecordType.Warning);
                }

                material = materialLibrary.GetMaterial(constructionLayers.Last()?.Name);
                if (material is GasMaterial)
                {
                    result.Add(string.Format("Last aperture construction frame layer (Name: {1}) for ApertureConstruction (Name: {2} Guid: {3}) shall not be gas type", material.Name, apertureConstruction.Name, apertureConstruction.Guid), LogRecordType.Warning);
                }
            }

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

                if (double.IsNaN(gasMaterial.GetValue<double>(Core.MaterialParameter.DefaultThickness)))
                    result.Add(string.Format("Default Thickness for {0} Material (Guid: {1}) has invalid value", name, material.Guid), LogRecordType.Warning);

                if (double.IsNaN(gasMaterial.GetValue<double>(MaterialParameter.VapourDiffusionFactor)))
                    result.Add(string.Format("Vapur Diffusion Factor for {0} Material (Guid: {1}) has invalid value", name, material.Guid), LogRecordType.Warning);

                if (double.IsNaN(gasMaterial.GetValue<double>(GasMaterialParameter.HeatTransferCoefficient)))
                    result.Add(string.Format("Heat Transfer Coefficient for {0} Material (Guid: {1}) has invalid value", name, material.Guid), LogRecordType.Warning);
            }
            else if (material is TransparentMaterial)
            {
                TransparentMaterial transparentMaterial = (TransparentMaterial)material;

                if (double.IsNaN(transparentMaterial.GetValue<double>(Core.MaterialParameter.DefaultThickness)))
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

                if (double.IsNaN(opaqueMaterial.GetValue<double>(Core.MaterialParameter.DefaultThickness)))
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

            bool adiabatic = panel.Adiabatic();

            Construction construction = panel.Construction;
            if(construction == null)
            {
                if (panelType != PanelType.Air)
                    result.Add(string.Format("{0} Panel (Guid: {1}) has no construction assigned.", name, panel.Guid), LogRecordType.Error);
            }
            else if(panelType != PanelType.Shade && !adiabatic)
            {
                PanelGroup panelGroup_Construction = construction.PanelType().PanelGroup();
                if (panelGroup_Construction != PanelGroup.Undefined)
                {
                    PanelGroup panelGroup_Panel = panelType.PanelGroup();
                    if (panelGroup_Panel != PanelGroup.Undefined)
                    {
                        string name_Construction = construction.Name;
                        if (string.IsNullOrWhiteSpace(name_Construction))
                            name_Construction = "???";

                        if (panelGroup_Construction != panelGroup_Panel)
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
                    else if(!adiabatic)
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

        public static Log Log(this Profile profile)
        {
            if (profile == null)
                return null;

            Log result = new Log();

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

            if(!space.TryGetValue(SpaceParameter.Area, out double area) || double.IsNaN(area))
            {
                result.Add(string.Format("Space (Guid: {1}) has no area assigned.", name, space.Guid), LogRecordType.Error);
            }

            InternalCondition internalCondition = space.InternalCondition;
            if(internalCondition == null)
                result.Add(string.Format("{0} Space (Guid: {1}) has no InternalCondition assigned.", name, space.Guid), LogRecordType.Warning);

            return result;
        }

        public static Log Log(this Space space, ProfileLibrary profileLibrary)
        {
            if (space == null || profileLibrary == null)
                return null;

            Log result = new Log();

            InternalCondition internalCondition = space.InternalCondition;

            Core.Modify.AddRange(result, internalCondition?.Log(profileLibrary));

            Dictionary<ProfileType, string> dictionary = internalCondition?.GetProfileTypeDictionary();
            if (dictionary != null)
            {
                foreach (ProfileType profileType in Enum.GetValues(typeof(ProfileType)))
                {
                    if (profileType == ProfileType.Undefined || profileType == ProfileType.Other)
                    {
                        continue;
                    }

                    switch(profileType)
                    {
                        case ProfileType.Ventilation:
                            if (internalCondition.TryGetValue(InternalConditionParameter.VentilationSystemTypeName, out string ventilationSystemTypeName))
                            {
                                if (ventilationSystemTypeName == "EOC" || ventilationSystemTypeName == "EOL")
                                {
                                    double supplyAirFlow = Query.CalculatedSupplyAirFlow(space);
                                    if (supplyAirFlow > 0)
                                    {
                                        result.Add("{0} Space (Guid: {1}) Your Ventilation System is {2} but supply air flow is {3}", LogRecordType.Warning, space.Name, space.Guid, ventilationSystemTypeName, supplyAirFlow);
                                    }
                                }

                                if (ventilationSystemTypeName == "NV" || ventilationSystemTypeName == "UV")
                                {
                                    double supplyAirFlow = Query.CalculatedSupplyAirFlow(space);
                                    double exhaustAirFlow = Query.CalculatedExhaustAirFlow(space);
                                    if (supplyAirFlow > 0 || exhaustAirFlow > 0)
                                    {
                                        result.Add("{0} Space (Guid: {1}) Your Ventilation System is {2} but supply air flow is {3} and exhaust air flow is {4}", LogRecordType.Warning, space.Name, space.Guid, ventilationSystemTypeName, supplyAirFlow, exhaustAirFlow);
                                    }
                                }
                            }
                            break;
                    }
                }
            }

            return result;
        }

        public static Log Log(this InternalCondition internalCondition, ProfileLibrary profileLibrary)
        {
            if (internalCondition == null || profileLibrary == null)
                return null;

            Dictionary<ProfileType, string> dictionary = internalCondition.GetProfileTypeDictionary();
            if (dictionary == null)
                return null;

            string name = internalCondition.Name;
            if (string.IsNullOrEmpty(name))
                name = "???";

            Log result = new Log();

            foreach(ProfileType profileType in Enum.GetValues(typeof(ProfileType)))
            {
                if (profileType == ProfileType.Undefined || profileType == ProfileType.Other)
                    continue;

                string profileName = null;
                if (dictionary == null || !dictionary.TryGetValue(profileType, out profileName))
                    profileName = null;

                Profile profile = null;
                if (!string.IsNullOrEmpty(profileName))
                {
                    profile = internalCondition.GetProfile(profileType, profileLibrary);
                    if (profile == null)
                    {
                        result.Add(string.Format("Cannot find valid {0} profile for {1} InternalCondition (Guid: {2})", profileType.Text(), name, internalCondition.Guid));
                        continue;
                    }
                }

                if (string.IsNullOrEmpty(profileName))
                    profileName = "???";

                double value_1;
                double value_2;

                switch (profileType)
                {
                    case ProfileType.Cooling:
                        if (internalCondition.TryGetValue(InternalConditionParameter.CoolingSystemTypeName, out string coolingSystemTypeName))
                        {
                            if(coolingSystemTypeName == "UC")
                            {
                                double coolingDesignTemperature = Query.CoolingDesignTemperature(internalCondition, profileLibrary);
                                if (coolingDesignTemperature <= 50)
                                {
                                    result.Add("{0} InternalCondition (Guid: {1}) Your Cooling System is {2} but setpoint is {3}", LogRecordType.Warning, name, internalCondition.Guid, coolingSystemTypeName, coolingDesignTemperature);
                                }
                            }
                        }
                        break;

                    case ProfileType.Heating:
                        if (internalCondition.TryGetValue(InternalConditionParameter.HeatingSystemTypeName, out string heatingSystemTypeName))
                        {
                            if (heatingSystemTypeName == "UH")
                            {
                                double heatingDesignTemperature = Query.HeatingDesignTemperature(internalCondition, profileLibrary);
                                if (heatingDesignTemperature > 0)
                                {
                                    result.Add("{0} InternalCondition (Guid: {1}) Your Heating System is {2} but setpoint is {3}", LogRecordType.Warning, name, internalCondition.Guid, heatingSystemTypeName, heatingDesignTemperature);
                                }
                            }
                        }
                        break;

                    case ProfileType.EquipmentLatent:

                        if (!internalCondition.TryGetValue(InternalConditionParameter.EquipmentLatentGain, out value_1))
                            value_1 = double.NaN;

                        if (!internalCondition.TryGetValue(InternalConditionParameter.EquipmentLatentGainPerArea, out value_2))
                            value_2 = double.NaN;

                        if (double.IsNaN(value_1) && double.IsNaN(value_2) && profile != null && !profile.IsOff())
                            result.Add("{0} InternalCondition (Guid: {1}) has {2} {3} (Guid: {4}) assigned but Equipment Latent Gain or Equipment Latent Gain Per Area have not been provided.", LogRecordType.Warning, name, internalCondition.Guid, profileName, profileType.Text(), profile.Guid);
                        else if((!double.IsNaN(value_1) || !double.IsNaN(value_2)) && profile == null)
                            result.Add("{0} InternalCondition (Guid: {1}) has no {2} assigned but Equipment Latent Gain or Equipment Latent Gain Per Area has been provided.", LogRecordType.Warning, name, internalCondition.Guid, profileType.Text());
                        break;


                    case ProfileType.EquipmentSensible:

                        if (!internalCondition.TryGetValue(InternalConditionParameter.EquipmentSensibleGain, out value_1))
                            value_1 = double.NaN;

                        if (!internalCondition.TryGetValue(InternalConditionParameter.EquipmentSensibleGainPerArea, out value_2))
                            value_2 = double.NaN;

                        if (double.IsNaN(value_1) && double.IsNaN(value_2) && profile != null && !profile.IsOff())
                            result.Add("{0} InternalCondition (Guid: {1}) has {2} {3} (Guid: {4}) assigned but Equipment Sensible Gain or Equipment Sensible Gain Per Area have not been provided.", LogRecordType.Warning, name, internalCondition.Guid, profileName, profileType.Text(), profile.Guid);
                        else if ((!double.IsNaN(value_1) || !double.IsNaN(value_2)) && profile == null)
                            result.Add("{0} InternalCondition (Guid: {1}) has no {2} assigned but Equipment Sensible Gain or Equipment Sensible Gain Per Area has been provided.", LogRecordType.Warning, name, internalCondition.Guid, profileType.Text());
                        break;


                    case ProfileType.Infiltration:

                        if (!internalCondition.TryGetValue(InternalConditionParameter.InfiltrationAirChangesPerHour, out value_1))
                            value_1 = double.NaN;

                        if(double.IsNaN(value_1) && profile != null && !profile.IsOff())
                            result.Add("{0} InternalCondition (Guid: {1}) has {2} {3} (Guid: {4}) assigned but Infiltration Air Changes Per Hour has not been provided.", LogRecordType.Warning, name, internalCondition.Guid, profileName, profileType.Text(), profile.Guid);
                        else if(!double.IsNaN(value_1) && profile == null)
                            result.Add("{0} InternalCondition (Guid: {1}) has no {2} assigned but Infiltration Air Changes Per Hour has been provided.", LogRecordType.Warning, name, internalCondition.Guid, profileType.Text());
                        break;


                    case ProfileType.Lighting:

                        if (!internalCondition.TryGetValue(InternalConditionParameter.LightingGain, out value_1))
                            value_1 = double.NaN;

                        if (!internalCondition.TryGetValue(InternalConditionParameter.LightingGainPerArea, out value_2))
                            value_2 = double.NaN;

                        if (double.IsNaN(value_1) && double.IsNaN(value_2) && profile != null && !profile.IsOff())
                            result.Add("{0} InternalCondition (Guid: {1}) has {2} {3} (Guid: {4}) assigned but Lighting Gain or Lighting Gain Per Area have not been provided.", LogRecordType.Warning, name, internalCondition.Guid, profileName, profileType.Text(), profile.Guid);
                        else if ((!double.IsNaN(value_1) || !double.IsNaN(value_2)) && profile == null)
                            result.Add("{0} InternalCondition (Guid: {1}) has no {2} assigned but Lighting Gain or Lighting Gain Per Area has been provided.", LogRecordType.Warning, name, internalCondition.Guid, profileType.Text());
                        break;


                    case ProfileType.Occupancy:

                        if (!internalCondition.TryGetValue(InternalConditionParameter.OccupancyLatentGainPerPerson, out value_1))
                            value_1 = double.NaN;

                        if (!internalCondition.TryGetValue(InternalConditionParameter.OccupancySensibleGainPerPerson, out value_2))
                            value_2 = double.NaN;

                        if (double.IsNaN(value_1) && double.IsNaN(value_2) && profile != null && !profile.IsOff())
                            result.Add("{0} InternalCondition (Guid: {1}) has {2} {3} (Guid: {4}) assigned but Occupancy Latent Gain Per Person or Occupancy Sensible Gain Per Person have not been provided.", LogRecordType.Warning, name, internalCondition.Guid, profileName, profileType.Text(), profile.Guid);
                        else if ((!double.IsNaN(value_1) || !double.IsNaN(value_2)) && profile == null)
                            result.Add("{0} InternalCondition (Guid: {1}) has no {2} assigned but Occupancy Latent Gain Per Person or Occupancy Sensible Gain Per Person has been provided.", LogRecordType.Warning, name, internalCondition.Guid, profileType.Text());
                        break;


                    case ProfileType.Pollutant:

                        if (!internalCondition.TryGetValue(InternalConditionParameter.PollutantGenerationPerArea, out value_1))
                            value_1 = double.NaN;

                        if (!internalCondition.TryGetValue(InternalConditionParameter.PollutantGenerationPerPerson, out value_2))
                            value_2 = double.NaN;

                        if (double.IsNaN(value_1) && double.IsNaN(value_2) && profile != null && !profile.IsOff())
                            result.Add("{0} InternalCondition (Guid: {1}) has {2} {3} (Guid: {4}) assigned but Pollutant Generation Per Area or Pollutant Generation Per Person have not been provided.", LogRecordType.Warning, name, internalCondition.Guid, profileName, profileType.Text(), profile.Guid);
                        else if ((!double.IsNaN(value_1) || !double.IsNaN(value_2)) && profile == null)
                            result.Add("{0} InternalCondition (Guid: {1}) has no {2} assigned but Pollutant Generation Per Area or Pollutant Generation Per Person has been provided.", LogRecordType.Warning, name, internalCondition.Guid, profileType.Text());
                        break;

                }
            }

            return result;
        }


        private static Log Log(this IEnumerable<ConstructionLayer> constructionLayers, MaterialLibrary materialLibrary, string name, Guid guid)
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

        private static Log Log(this IEnumerable<ConstructionLayer> constructionLayers, string name, Guid guid)
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