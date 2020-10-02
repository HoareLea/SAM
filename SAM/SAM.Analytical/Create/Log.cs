using SAM.Core;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

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
                    return result;
                }

                List<Space> spaces_InRange = spaces.FindAll(x => shell.InRange(x.Location));
                if(spaces_InRange == null || spaces_InRange.Count > 1)
                {
                    result.Add("There are more than one space enclosed in single shell: {0}", LogRecordType.Warning, string.Join(", ",spaces_InRange.ConvertAll(x => x.Name)));
                    return result;
                }
                
                List<Panel> panels_Space = adjacencyCluster.GetPanels(space);
                if(panels_Space == null || panels_Space.Count == 0)
                {
                    result.Add("Space {0} (Guid: {1}) is not enclosed.", LogRecordType.Warning, space.Name, space.Guid);
                    return result;
                }

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

            List<ConstructionLayer> constructionLayers = construction.ConstructionLayers;
            if(constructionLayers == null || constructionLayers.Count == 0)
            {
                result.Add(string.Format("{0} Construction (Guid: {1}) has no construction layers", construction.Name, construction.Guid), LogRecordType.Warning);
                return result;
            }

            for(int i=0; i < constructionLayers.Count; i++)
            {
                ConstructionLayer constructionLayer = constructionLayers[i];

                if (string.IsNullOrWhiteSpace(constructionLayer.Name))
                    result.Add(string.Format("{0} Construction (Guid: {1}) has layer with no name (Construction Layer Index: {2})", construction.Name, construction.Guid, i), LogRecordType.Error);

                if (constructionLayer.Thickness <= 0)
                    result.Add(string.Format("{0} Construction (Guid: {1}) has layer with thickness equal or less than 0 (Construction Layer Index: {2})", construction.Name, construction.Guid, i), LogRecordType.Error);
            }

            return result;
        }

        public static Log Log(this Construction construction, MaterialLibrary materialLibrary)
        {
            if (construction == null || materialLibrary == null)
                return null;

            Log result = new Log();

            List<ConstructionLayer> constructionLayers = construction?.ConstructionLayers;
            if (constructionLayers == null || constructionLayers.Count == 0)
                return result;

            foreach (ConstructionLayer constructionLayer in constructionLayers)
            {
                IMaterial material = constructionLayer.Material(materialLibrary);
                if (material == null)
                    result.Add(string.Format("Material Library does not contain Material {0} for {1} Construction (Guid: {2})", constructionLayer.Name, construction.Name, construction.Guid), LogRecordType.Error);
            }

            return result;
        }

        public static Log Log(this ApertureConstruction apertureConstruction)
        {
            if (apertureConstruction == null)
                return null;

            Log result = new Log();

            return result;
        }

        public static Log Log(this IMaterial material)
        {
            if (material == null)
                return null;
            
            //TODO: Implement Checks for Material

            Log result = new Log();

            if (material is GasMaterial)
            {

            }
            else if (material is TransparentMaterial)
            {

            }
            else if (material is OpaqueMaterial)
            {

            }

            return result;
        }
    }
}