using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static bool UpdateConstruction(this AdjacencyCluster adjacencyCluster, ConstructionLibrary constructionLibrary, string constructionName_Source, string constructionName_Template, string constructionName_Destination)
        {
            if (adjacencyCluster == null || constructionLibrary == null || string.IsNullOrWhiteSpace(constructionName_Source) || string.IsNullOrWhiteSpace(constructionName_Destination) || string.IsNullOrWhiteSpace(constructionName_Template))
                return false;

            List<Construction> constructions_Source = adjacencyCluster.GetConstructions()?.FindAll(x => !string.IsNullOrWhiteSpace(x.Name) &&x.Name.Equals(constructionName_Source));
            if (constructions_Source == null || constructions_Source.Count == 0)
                return false;

            foreach(Construction construction_Source in constructions_Source)
            {
                Construction construction_Destination = Create.Construction(constructionLibrary, constructionName_Template, constructionName_Destination);
                if (construction_Destination == null)
                    continue;

                List<Panel> panels = adjacencyCluster.Panels(construction_Source);
                if (panels == null || panels.Count == 0)
                    continue;

                foreach(Panel panel in panels)
                {
                    Panel panel_Destination = new Panel(panel, construction_Destination);

                    adjacencyCluster.AddObject(panel_Destination);
                }
            }

            return true;
        }
    }
}