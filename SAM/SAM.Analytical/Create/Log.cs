// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

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

            //Before the space and panel checks, because both of those give up and return early on a model
            //that has none, and an air handling unit's air movement is neither: it is related to the unit,
            //not to a space, and a plant object that would refuse in TAS still has to be reported.
            List<AirHandlingUnitAirMovement> airHandlingUnitAirMovements = adjacencyCluster.GetObjects<AirHandlingUnitAirMovement>();
            if (airHandlingUnitAirMovements != null)
            {
                foreach (AirHandlingUnitAirMovement airHandlingUnitAirMovement in airHandlingUnitAirMovements)
                    Core.Modify.AddRange(result, airHandlingUnitAirMovement?.Log(adjacencyCluster));
            }

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
                foreach (Panel panel in panels)
                {
                    Core.Modify.AddRange(result, panel?.Log());

                    List<Aperture> apertures = panel.Apertures;
                    if (apertures != null && apertures.Count != 0)
                    {
                        PanelGroup panelGroup_Panel = panel.PanelType.PanelGroup();
                        if (panelGroup_Panel != PanelGroup.Undefined)
                        {
                            foreach (Aperture aperture in apertures)
                            {
                                ApertureConstruction apertureConstruction = aperture.ApertureConstruction;
                                if (apertureConstruction == null)
                                    continue;

                                PanelGroup panelGroup_ApertureConstruction = apertureConstruction.PanelType().PanelGroup();
                                if (panelGroup_ApertureConstruction != PanelGroup.Undefined)
                                {
                                    string apertureName = aperture.Name;
                                    if (string.IsNullOrEmpty(apertureName))
                                        apertureName = "???";

                                    string apertureConstructionName = apertureConstruction.Name;
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
                        if (spaces_Panel != null && spaces_Panel.Count != 0)
                        {
                            PanelType panelType = panel.PanelType;
                            switch (panelType)
                            {
                                case PanelType.Air:
                                case PanelType.Ceiling:
                                case PanelType.FloorInternal:
                                case PanelType.FloorRaised:
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
                                case PanelType.UndergroundCeiling:
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
            foreach (Space space in spaces)
            {
                Core.Modify.AddRange(result, space?.Log());

                Shell shell = adjacencyCluster.Shell(space);
                if (shell == null || !shell.IsClosed())
                {
                    result.Add("Space {0} (Guid: {1}) is not enclosed (with 1e-6 tolerance).", LogRecordType.Warning, space.Name, space.Guid);
                    continue;
                }

                if (space.Location == null)
                {
                    result.Add("Space {0} (Guid: {1}) has no location.", LogRecordType.Warning, space.Name, space.Guid);
                    continue;
                }

                if (space.Name != null)
                {
                    if (spaceNames.Contains(space.Name))
                    {
                        result.Add("Space {0} (Guid: {1}) name is duplicated", LogRecordType.Warning, space.Name, space.Guid);
                        continue;
                    }

                    spaceNames.Add(space.Name);
                }

                List<Panel> panels_Space = adjacencyCluster.GetPanels(space);
                if (panels_Space == null || panels_Space.Count == 0)
                {
                    result.Add("Space {0} (Guid: {1}) is not enclosed.", LogRecordType.Warning, space.Name, space.Guid);
                    continue;
                }

                if (panels_Space.Count < 4)
                    result.Add("Space {0} (Guid: {1}) has less than 4 panels.", LogRecordType.Message, space.Name, space.Guid);

                if (panels_Space.TrueForAll(x => x.Adiabatic()))
                {
                    result.Add("Space {0} (Guid: {1}) all panels are adiabatic.", LogRecordType.Error, space.Name, space.Guid);
                }

                Panel panel_Floor = panels_Space.Find(x => Query.PanelGroup(x.PanelType) == PanelGroup.Floor);
                if (panel_Floor == null)
                {
                    Panel panel_Air = panels_Space.Find(x => x.PanelType == PanelType.Air);
                    if (panel_Air != null)
                        result.Add("Space {0} (Guid: {1}) has no floor panels but has air panels.", LogRecordType.Message, space.Name, space.Guid);
                    else
                        result.Add("Space {0} (Guid: {1}) has no floor panels and air panels.", LogRecordType.Message, space.Name, space.Guid);
                }

                foreach (Panel panel in panels_Space)
                {
                    if (panel == null)
                        continue;

                    if (panel.PanelType == PanelType.Shade || panel.PanelType == PanelType.SolarPanel || panel.PanelType == PanelType.Undefined)
                    {
                        result.Add("Panel {0} (Guid: {1}) has assigned {2} PanelType and it also encloses {3} space (Guid: {4}).", LogRecordType.Warning, panel.Name, panel.Guid, panel.PanelType, space.Name, space.Guid);
                        return result;
                    }
                }
            }

            Dictionary<Shell, List<Space>> dictionary = Query.DuplicatedSpacesDictionary(adjacencyCluster);
            if (dictionary != null && dictionary.Count > 0)
            {
                foreach (List<Space> spaces_Duplicated in dictionary.Values)
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
            if (constructions == null || constructions.Count == 0)
            {
                result.Add("Panels in AdjacencyCluster has no constructions assigned.", LogRecordType.Error);
            }
            else
            {
                foreach (Construction construction in constructions)
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


            if (adjacencyCluster == null)
                result.Add("AdjacencyCluster missing in AnalyticalModel", LogRecordType.Error);
            else
                Core.Modify.AddRange(result, adjacencyCluster?.Log());

            if (materialLibrary == null)
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
                if (constructions != null && materialLibrary != null)
                {
                    foreach (Construction construction in constructions)
                        Core.Modify.AddRange(result, construction?.Log(materialLibrary));
                }

                List<ApertureConstruction> apertureConstructions = adjacencyCluster.ApertureConstructions();
                if (apertureConstructions != null && materialLibrary != null)
                {
                    foreach (ApertureConstruction apertureConstruction in apertureConstructions)
                        Core.Modify.AddRange(result, apertureConstruction?.Log(materialLibrary));
                }

                List<Panel> panels = adjacencyCluster.GetPanels();
                if (panels != null && panels.Count != 0)
                {
                    foreach (Panel panel in panels)
                        Core.Modify.AddRange(result, panel?.Log(materialLibrary));
                }

                List<Space> spaces = adjacencyCluster.GetSpaces();
                if (spaces != null && spaces.Count != 0)
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

            if (materials == null || materials.Count == 0)
            {
                result.Add("Material Library has no Materials.", LogRecordType.Message);
                return result;
            }

            foreach (IMaterial material in materials)
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

            if (panelType != PanelType.Air && panelType != PanelType.Shade)
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
                if (material is GasMaterial)
                {
                    result.Add(string.Format("First construction layer (Name: {0}) for Construction (Name: {1} Guid: {2}) shall not be gas type", material.Name, construction.Name, construction.Guid), LogRecordType.Error);
                }

                material = materialLibrary.GetMaterial(constructionLayers.Last()?.Name);
                if (material is GasMaterial)
                {
                    result.Add(string.Format("Last construction layer (Name: {0}) for Construction (Name: {1} Guid: {2}) shall not be gas type", material.Name, construction.Name, construction.Guid), LogRecordType.Error);
                }
            }

            double thickness;
            if (construction.TryGetValue(ConstructionParameter.DefaultThickness, out thickness))
            {
                double thickness_ConstructionLayers = construction.GetThickness();
                if (!double.IsNaN(thickness_ConstructionLayers))
                {
                    if (System.Math.Abs(thickness - thickness_ConstructionLayers) > Tolerance.MacroDistance)
                    {
                        result.Add(string.Format("Parameter {0} in {1} Construction (Guid: {2}) has different value ({3}) than thickness of its ConstructionLayers ({4})", ConstructionParameter.DefaultThickness.Name(), construction.Name, construction.Guid, Core.Query.Round(thickness, Tolerance.MacroDistance), Core.Query.Round(thickness_ConstructionLayers, Tolerance.MacroDistance)), LogRecordType.Message);
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
                    result.Add(string.Format("First aperture construction pane layer (Name: {0}) for ApertureConstruction (Name: {1} Guid: {2}) shall not be gas type", material.Name, apertureConstruction.Name, apertureConstruction.Guid), LogRecordType.Error);
                }

                material = materialLibrary.GetMaterial(constructionLayers.Last()?.Name);
                if (material is GasMaterial)
                {
                    result.Add(string.Format("Last aperture construction pane layer (Name: {0}) for ApertureConstruction (Name: {1} Guid: {2}) shall not be gas type", material.Name, apertureConstruction.Name, apertureConstruction.Guid), LogRecordType.Error);
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
                    result.Add(string.Format("First aperture construction frame layer (Name: {0}) for ApertureConstruction (Name: {1} Guid: {2}) shall not be gas type", material.Name, apertureConstruction.Name, apertureConstruction.Guid), LogRecordType.Error);
                }

                material = materialLibrary.GetMaterial(constructionLayers.Last()?.Name);
                if (material is GasMaterial)
                {
                    result.Add(string.Format("Last aperture construction frame layer (Name: {0}) for ApertureConstruction (Name: {1} Guid: {2}) shall not be gas type", material.Name, apertureConstruction.Name, apertureConstruction.Guid), LogRecordType.Error);
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
            if (string.IsNullOrWhiteSpace(name))
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

                //if (double.IsNaN(gasMaterial.Density))
                //    result.Add(string.Format("Density for {0} Material (Guid: {1}) has invalid value", name, material.Guid), LogRecordType.Error);
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
            if (string.IsNullOrEmpty(name))
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
            if (panelType == PanelType.Undefined)
                result.Add(string.Format("Panel Type for {0} Panel (Guid: {1}) is not assigned.", name, panel.Guid), LogRecordType.Error);

            double area = double.NaN;

            PlanarBoundary3D planarBoundary3D = panel.PlanarBoundary3D;
            if (planarBoundary3D == null)
            {
                result.Add(string.Format("{0} Panel (Guid: {1}) has no geometry assigned.", name, panel.Guid), LogRecordType.Error);
            }
            else
            {
                area = panel.GetArea();
                if (double.IsNaN(area) || area < Tolerance.MacroDistance)
                    result.Add(string.Format("{0} Panel (Guid: {1}) area is less than {2}.", name, panel.Guid, Tolerance.MacroDistance), LogRecordType.Warning);

            }

            bool adiabatic = panel.Adiabatic();

            Construction construction = panel.Construction;
            if (construction == null)
            {
                if (panelType != PanelType.Air)
                    result.Add(string.Format("{0} Panel (Guid: {1}) has no construction assigned.", name, panel.Guid), LogRecordType.Error);
            }
            else if (panelType != PanelType.Shade && !adiabatic)
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
            if (apertures != null && apertures.Count > 0)
            {
                if (panelType == PanelType.Air)
                    result.Add(string.Format("{0} Panel (Guid: {1}) with PanelType Air hosts Apertures", name, panel.Guid), LogRecordType.Error);

                double area_Apertures = 0;
                foreach (Aperture aperture in apertures)
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
                    if (apertureConstruction == null)
                    {
                        result.Add(string.Format("{0} aperture (Guid: {1}) in {2} host panel (Guid: {3}) has no ApertureConstruction", name_Aperture, aperture.Guid, name, panel.Guid), LogRecordType.Error);
                    }
                    else if (!adiabatic)
                    {
                        string text;
                        if (apertureConstruction.TryGetValue(ApertureConstructionParameter.DefaultPanelType, out text) && !string.IsNullOrWhiteSpace(text))
                        {
                            PanelType panelType_ApertureConstruction = Query.PanelType(text, false);
                            if (panelType_ApertureConstruction != PanelType.Undefined && panelType_ApertureConstruction.PanelGroup() != panelType.PanelGroup())
                                result.Add(string.Format("ApertureConstruction for {0} aperture (Guid: {1}) has diiferent Default Panel Type than its {2} host panel (Guid: {3}) has ", name_Aperture, aperture.Guid, name, panel.Guid), LogRecordType.Warning);
                        }
                    }
                }

                if (!double.IsNaN(area) && area < area_Apertures)
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
            if (construction != null)
            {
                string name_Construction = panel.Construction.Name;
                if (string.IsNullOrWhiteSpace(name_Construction))
                    name_Construction = "???";

                MaterialType materialType = Query.MaterialType(construction.ConstructionLayers, materialLibrary);
                if (materialType != MaterialType.Undefined)
                {
                    bool transparent;
                    if (panel.TryGetValue(PanelParameter.Transparent, out transparent))
                    {
                        if ((transparent && materialType != MaterialType.Transparent) || (!transparent && materialType == MaterialType.Transparent))
                            result.Add(string.Format("{0} parameter value for {1} panel (Guid: {2}) does not match witch assigned {3} construction (Guid: {4})", PanelParameter.Transparent.Name(), name, panel.Guid, name_Construction, construction.Guid), LogRecordType.Warning);
                    }

                    PanelType panelType = panel.PanelType;
                    if (panelType == PanelType.CurtainWall && materialType != MaterialType.Transparent)
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
            if (apertureConstruction == null)
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

            if (!space.TryGetValue(SpaceParameter.Area, out double area) || double.IsNaN(area))
            {
                result.Add(string.Format("Space (Guid: {1}) has no area assigned.", name, space.Guid), LogRecordType.Error);
            }

            if (Core.Query.AlmostEqual(area, 0, Tolerance.MacroDistance))
            {
                result.Add(string.Format("Space (Guid: {1}) has assigned area almost equal to 0.", name, space.Guid), LogRecordType.Error);
            }

            InternalCondition internalCondition = space.InternalCondition;
            if (internalCondition == null)
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

                    switch (profileType)
                    {
                        case ProfileType.Ventilation:
                            if (internalCondition.TryGetValue(InternalConditionParameter.VentilationSystemTypeName, out string ventilationSystemTypeName))
                            {
                                if (ventilationSystemTypeName == "EOC" || ventilationSystemTypeName == "EOL")
                                {
                                    double supplyAirFlow = Query.CalculatedSupplyAirFlow(space);
                                    if (supplyAirFlow > 0)
                                    {
                                        result.Add("{0} Space (Guid: {1}) Your Ventilation System is {2} but supply air flow is {3}", LogRecordType.Warning, space.Name, space.Guid, ventilationSystemTypeName, Core.Query.Round(supplyAirFlow, Tolerance.MacroDistance));
                                    }
                                }

                                if (ventilationSystemTypeName == "NV" || ventilationSystemTypeName == "UV")
                                {
                                    double supplyAirFlow = Query.CalculatedSupplyAirFlow(space);
                                    double exhaustAirFlow = Query.CalculatedExhaustAirFlow(space);
                                    if (supplyAirFlow > 0 || exhaustAirFlow > 0)
                                    {
                                        result.Add("{0} Space (Guid: {1}) Your Ventilation System is {2} but supply air flow is {3} and exhaust air flow is {4}", LogRecordType.Warning, space.Name, space.Guid, ventilationSystemTypeName, Core.Query.Round(supplyAirFlow, Tolerance.MacroDistance), Core.Query.Round(exhaustAirFlow, Tolerance.MacroDistance));
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

            foreach (ProfileType profileType in Enum.GetValues(typeof(ProfileType)))
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
                            if (coolingSystemTypeName == "UC")
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
                        else if ((!double.IsNaN(value_1) || !double.IsNaN(value_2)) && profile == null)
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

                        if (double.IsNaN(value_1) && profile != null && !profile.IsOff())
                            result.Add("{0} InternalCondition (Guid: {1}) has {2} {3} (Guid: {4}) assigned but Infiltration Air Changes Per Hour has not been provided.", LogRecordType.Warning, name, internalCondition.Guid, profileName, profileType.Text(), profile.Guid);
                        else if (!double.IsNaN(value_1) && profile == null)
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

            //The two humidity limits read against EACH OTHER, which no per-profile-type case above can do.
            //
            //A humidistat is a pair - Humidification is the lower limit, Dehumidification the upper - and
            //either one alone is valid at any value. Only the pair can be wrong, and it is wrong in exactly
            //one way: a lower limit above the upper limit asks for air that is simultaneously wetter than
            //X% and drier than a smaller Y%. TAS detects this in its own pre-simulation check and refuses
            //the model outright ("Internal Condition '...' humidistat has overlapping limits"), so it is an
            //Error here - the model cannot be simulated as it stands.
            if (TryGetOverlappingHumidityLimits(
                internalCondition.GetProfile(ProfileType.Humidification, profileLibrary),
                internalCondition.GetProfile(ProfileType.Dehumidification, profileLibrary),
                out int index_HumidityLimits,
                out double lowerLimit_Humidity,
                out double upperLimit_Humidity))
            {
                result.Add(
                    "{0} InternalCondition (Guid: {1}) has overlapping humidistat limits: the {2} (humidity LOWER) limit is {3} and the {4} (humidity UPPER) limit is {5} at hour {6}. The lower limit cannot be above the upper limit - TAS refuses to simulate a model whose humidistat limits overlap.",
                    LogRecordType.Error,
                    name,
                    internalCondition.Guid,
                    ProfileType.Humidification.Text(),
                    lowerLimit_Humidity,
                    ProfileType.Dehumidification.Text(),
                    upperLimit_Humidity,
                    index_HumidityLimits);
            }

            return result;
        }

        /// <summary>
        /// An air handling unit's air movement, checked as the thing it becomes.
        /// <para>
        /// This object is not a schedule on a room. <c>SAM.Analytical.Tas.Modify.UpdateIZAMs</c> builds one
        /// small TAS zone per air handling unit that carries one of these - the unit's own plant zone, named
        /// after the unit ("MVHR-01" and so on) - and writes THESE FOUR PROFILES onto that zone's internal
        /// condition: Heating to the temperature lower limit, Cooling to the upper, Humidification to the
        /// humidity lower limit and Dehumidification to the humidity upper. So a limit pair that is invalid
        /// here is an internal condition that is invalid in the file, on a zone no space in the model
        /// names, which is why nothing that walks the spaces can find it.
        /// </para>
        /// </summary>
        public static Log Log(this AirHandlingUnitAirMovement airHandlingUnitAirMovement, AdjacencyCluster adjacencyCluster = null)
        {
            if (airHandlingUnitAirMovement == null)
                return null;

            Log result = new Log();

            string name = airHandlingUnitAirMovement.Name;
            if (string.IsNullOrWhiteSpace(name))
                name = "???";

            //The unit's generated TAS zone has to balance, and one specific way of breaking it is visible
            //here.
            //
            //UpdateIZAMs writes the plant zone one air movement per room the unit SUPPLIES, and one
            //"IZAM <unit> FROM OUTSIDE" that brings in what it therefore has to draw - sized by
            //Query.AirFlow, which reads the deliveries RELATED to the unit. Where that answers nothing the
            //intake is simply not written; if the unit delivers anyway, its zone loses the dwelling's whole
            //supply and gains nothing, and TAS refuses to simulate a zone whose air movements do not
            //balance - saying only "Simulation Failed" when it does.
            //
            //So the deterministic fault is a DISAGREEMENT, not an absence: the model holds a movement that
            //names this unit as its source and names a destination, and that movement is not related to the
            //unit. Whether it delivers is asked of the WHOLE cluster, because the missing relation is
            //exactly what is wrong; whether an intake will be sized is asked of Query.AirFlow, which can
            //only see the relations.
            //
            //Asking it that way round is what keeps a legitimate EXTRACT-ONLY unit valid. Such a unit
            //delivers to no room at all: its zone gains each room's extract and loses it again through the
            //unit's own exhaust, so it balances with no outside intake, and Query.AirFlow correctly answers
            //nothing. "No intake" is only a fault where something is being delivered.
            if (adjacencyCluster != null)
            {
                AirHandlingUnit airHandlingUnit = adjacencyCluster.GetRelatedObjects<AirHandlingUnit>(airHandlingUnitAirMovement)?.Find(x => x != null);
                if (airHandlingUnit == null)
                {
                    //A Warning, not an Error. Nothing pairs this movement with a unit, so UpdateIZAMs
                    //generates no plant zone from it and it is inert - the unit's supply condition is
                    //simply never applied. Worth saying; not a reason a model cannot be simulated.
                    result.Add(
                        "{0} AirHandlingUnitAirMovement (Guid: {1}) is related to no AirHandlingUnit, so nothing states which unit's supply condition it carries and no TAS plant zone will be generated from it.",
                        LogRecordType.Warning,
                        name,
                        airHandlingUnitAirMovement.Guid);
                }
                else
                {
                    SpaceAirMovement spaceAirMovement_Delivered = Query.SpaceAirMovement_Delivered(adjacencyCluster, airHandlingUnit);

                    if (spaceAirMovement_Delivered != null
                        && (double.IsNaN(Query.AirFlow(adjacencyCluster, airHandlingUnitAirMovement, out Profile profile_Intake)) || profile_Intake == null))
                    {
                        result.Add(
                            "{0} AirHandlingUnitAirMovement (Guid: {1}) resolves no intake air flow, although air handling unit '{2}' (Guid: {3}) supplies air movement '{4}' (Guid: {5}) - that movement is not RELATED to the unit, so the intake cannot be sized from it. The generated TAS plant zone would deliver supply air and take none in, and TAS refuses to simulate a zone whose air movements do not balance.",
                            LogRecordType.Error,
                            name,
                            airHandlingUnitAirMovement.Guid,
                            string.IsNullOrWhiteSpace(airHandlingUnit.Name) ? "???" : airHandlingUnit.Name,
                            airHandlingUnit.Guid,
                            string.IsNullOrWhiteSpace(spaceAirMovement_Delivered.Name) ? "???" : spaceAirMovement_Delivered.Name,
                            spaceAirMovement_Delivered.Guid);
                    }
                }
            }

            if (TryGetOverlappingHumidityLimits(
                airHandlingUnitAirMovement.Humidification,
                airHandlingUnitAirMovement.Dehumidification,
                out int index,
                out double lowerLimit,
                out double upperLimit))
            {
                result.Add(
                    "{0} AirHandlingUnitAirMovement (Guid: {1}) has overlapping humidity limits: the {2} (humidity LOWER) limit is {3} and the {4} (humidity UPPER) limit is {5} at hour {6}. These become the humidistat on the unit's generated TAS plant zone, so the lower limit cannot be above the upper limit - TAS refuses to simulate a model whose humidistat limits overlap.",
                    LogRecordType.Error,
                    name,
                    airHandlingUnitAirMovement.Guid,
                    ProfileType.Humidification.Text(),
                    lowerLimit,
                    ProfileType.Dehumidification.Text(),
                    upperLimit,
                    index);
            }

            return result;
        }

        /// <summary>
        /// Whether a humidistat's lower limit is above its upper limit at any index the two profiles share,
        /// and where.
        /// <para>
        /// Read index by index rather than by comparing one profile's maximum against the other's minimum,
        /// because two SCHEDULES that each move over the day can both be higher than the other at different
        /// hours without ever overlapping, and a check that reported those as errors would be reporting
        /// valid models. Every index in the union of the two ranges is read through the profile indexer,
        /// which repeats a shorter profile over a longer one exactly as a schedule does - so a single value
        /// against a 24-hour profile is compared against all 24 hours, which is the common case here.
        /// </para>
        /// <para>
        /// An absent profile is not an overlap: a humidistat with no lower limit stated is not
        /// deterministically invalid, and saying so would make "no humidity control" an error.
        /// </para>
        /// </summary>
        private static bool TryGetOverlappingHumidityLimits(Profile profile_LowerLimit, Profile profile_UpperLimit, out int index, out double value_LowerLimit, out double value_UpperLimit)
        {
            index = -1;
            value_LowerLimit = double.NaN;
            value_UpperLimit = double.NaN;

            if (profile_LowerLimit == null || profile_UpperLimit == null)
                return false;

            if (profile_LowerLimit.Count <= 0 || profile_UpperLimit.Count <= 0)
                return false;

            int min = System.Math.Min(profile_LowerLimit.Min, profile_UpperLimit.Min);
            int max = System.Math.Max(profile_LowerLimit.Max, profile_UpperLimit.Max);

            if (min == int.MinValue || max == int.MaxValue || max < min)
                return false;

            //The hours in a leap year. A yearly profile is read in full; a range beyond one is malformed,
            //and this check does not become an unbounded loop over it.
            if (max - min > 8783)
                max = min + 8783;

            for (int i = min; i <= max; i++)
            {
                double value_Lower = profile_LowerLimit[i];
                double value_Upper = profile_UpperLimit[i];

                //Not read as an overlap. A NaN is an unstated hour, not a limit above another limit, and
                //this method answers one question only.
                if (double.IsNaN(value_Lower) || double.IsNaN(value_Upper))
                    continue;

                if (value_Lower > value_Upper)
                {
                    index = i;
                    value_LowerLimit = value_Lower;
                    value_UpperLimit = value_Upper;

                    return true;
                }
            }

            return false;
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

                if (material is GasMaterial)
                {
                    GasMaterial gasMaterial = (GasMaterial)material;
                    DefaultGasType defaultGasType = Query.DefaultGasType(gasMaterial);
                    if (defaultGasType == DefaultGasType.Undefined)
                        result.Add(string.Format("{0} gas material is not recogionzed in {1} (Guid: {2}) (Construction Layer Index: {3}). Heat Transfer Coefficient may not be calculated properly.", constructionLayer.Name, name_Temp, guid, index), LogRecordType.Warning);
                    else if (materialType == MaterialType.Opaque && defaultGasType != DefaultGasType.Air)
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
