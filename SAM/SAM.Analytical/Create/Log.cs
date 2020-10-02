using SAM.Core;
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

            List<Space> spaces = adjacencyCluster.GetSpaces();
            if(spaces == null || spaces.Count == 0)
            {
                result.Add("AdjacencyCluster has no spaces.", LogRecordType.Warning);
                return result;
            }

            foreach(Space space in spaces)
            {
                Shell shell = adjacencyCluster.Shell(space);
                if(shell == null || !shell.IsClosed())
                {
                    result.Add("Space {0} (Guid: {1}) is not enclosed.", LogRecordType.Warning, space.Name, space.Guid);
                    continue;
                }

                List<Space> spaces_InRange = spaces.FindAll(x => shell.InRange(x.Location));
                if(spaces_InRange == null || spaces_InRange.Count > 1)
                {
                    result.Add("There are more than one space enclosed in single shell: {0}", LogRecordType.Warning, string.Join(", ",spaces_InRange.ConvertAll(x => x.Name)));
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
                {
                    Log log_Construction = construction.Log();
                    if (log_Construction != null)
                        result.AddRange(log_Construction);
                }
            }

            List<ApertureConstruction> apertureConstructions = adjacencyCluster.GetApertureConstructions();
            if (apertureConstructions != null && apertureConstructions.Count > 0)
            {
                foreach (ApertureConstruction apertureConstruction in apertureConstructions)
                {
                    Log log_Construction = apertureConstruction.Log();
                    if (log_Construction != null)
                        result.AddRange(log_Construction);
                }
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
            {
                result.Add("AdjacencyCluster missing in AnalyticalModel", LogRecordType.Error);
            }
            else
            {
                Log log_AdjacencyCluster = adjacencyCluster.Log();
                if (log_AdjacencyCluster != null)
                    result.AddRange(log_AdjacencyCluster);
            }

            if(materialLibrary == null)
            {
                result.Add("MaterialLibrary missing in AnalyticalModel", LogRecordType.Error);
            }
            else
            {
                Log log_MaterialLibrary = materialLibrary.Log();
                if (log_MaterialLibrary != null)
                    result.AddRange(log_MaterialLibrary);
            }

            if(adjacencyCluster != null)
            {
                List<Construction> constructions = adjacencyCluster.GetConstructions();
                if(constructions != null && materialLibrary != null)
                {
                    foreach(Construction construction in constructions)
                    {
                        Log log_Construction = construction.Log(materialLibrary);
                        if (log_Construction != null)
                            result.AddRange(log_Construction);
                    }
                }

                List<ApertureConstruction> apertureConstructions = adjacencyCluster.ApertureConstructions();
                if (apertureConstructions != null && materialLibrary != null)
                {
                    foreach (ApertureConstruction apertureConstruction in apertureConstructions)
                    {
                        Log log_Construction = apertureConstruction.Log(materialLibrary);
                        if (log_Construction != null)
                            result.AddRange(log_Construction);
                    }
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
            {
                Log log_Material = material.Log();
                if (log_Material != null)
                    result.AddRange(log_Material);
            }

            return result;
        }

        public static Log Log(this Construction construction)
        {
            if (construction == null)
                return null;

            Log result = new Log();

            List<ConstructionLayer> constructionLayers = construction?.ConstructionLayers;
            if (constructionLayers != null && constructionLayers.Count > 0)
            {
                Log log_ConstructionLayers = constructionLayers.Log(construction.Name, construction.Guid);
                if (log_ConstructionLayers != null)
                    result.AddRange(log_ConstructionLayers);
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
                Log log_ConstructionLayers = constructionLayers.Log(materialLibrary, construction.Name, construction.Guid);
                if (log_ConstructionLayers != null)
                    result.AddRange(log_ConstructionLayers);
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
            {
                Log log_ConstructionLayers = constructionLayers.Log(apertureConstruction.Name, apertureConstruction.Guid);
                if (log_ConstructionLayers != null)
                    result.AddRange(log_ConstructionLayers);
            }

            constructionLayers = apertureConstruction?.FrameConstructionLayers;
            if (constructionLayers != null && constructionLayers.Count > 0)
            {
                Log log_ConstructionLayers = constructionLayers.Log(apertureConstruction.Name, apertureConstruction.Guid);
                if (log_ConstructionLayers != null)
                    result.AddRange(log_ConstructionLayers);
            }

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
                Log log_ConstructionLayers = constructionLayers.Log(materialLibrary, apertureConstruction.Name, apertureConstruction.Guid);
                if (log_ConstructionLayers != null)
                    result.AddRange(log_ConstructionLayers);
            }

            constructionLayers = apertureConstruction?.FrameConstructionLayers;
            if (constructionLayers != null && constructionLayers.Count > 0)
            {
                Log log_ConstructionLayers = constructionLayers.Log(materialLibrary, apertureConstruction.Name, apertureConstruction.Guid);
                if (log_ConstructionLayers != null)
                    result.AddRange(log_ConstructionLayers);
            }

            return result;
        }

        public static Log Log(this IMaterial material)
        {
            if (material == null)
                return null;
            
            //TODO: Implement Checks for Material

            Log result = new Log();

            string name = material.Name;
            if(string.IsNullOrWhiteSpace(name))
                result.Add(string.Format("Material (Guid: {0}) has no name assigned", material.Guid), LogRecordType.Warning);

            if (material is GasMaterial)
            {
                GasMaterial gasMaterial = (GasMaterial)material;
            }
            else if (material is TransparentMaterial)
            {
                TransparentMaterial transparentMaterial = (TransparentMaterial)material;
            }
            else if (material is OpaqueMaterial)
            {
                OpaqueMaterial opaqueMaterial = (OpaqueMaterial)material;
            }

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

            foreach (ConstructionLayer constructionLayer in constructionLayers)
            {
                IMaterial material = constructionLayer.Material(materialLibrary);
                if (material == null)
                    result.Add(string.Format("Material Library does not contain Material {0} for {1} (Guid: {2})", constructionLayer.Name, name_Temp, guid), LogRecordType.Error);
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