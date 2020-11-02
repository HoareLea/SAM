using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static List<Construction> UpdateConstructionsByName(this AdjacencyCluster adjacencyCluster, ConstructionLibrary constructionLibrary, TextComparisonType textComparisonType = TextComparisonType.Equals, bool caseSensitive = true)
        {
            if (adjacencyCluster == null || constructionLibrary == null)
                return null;

            List<Construction> constructions_Source = adjacencyCluster.GetConstructions();
            if (constructions_Source == null || constructions_Source.Count == 0)
                return null;

            List<Construction> result = new List<Construction>();
            foreach(Construction construction_Source in constructions_Source)
            {
                string name_Source = construction_Source.Name;
                if (string.IsNullOrWhiteSpace(name_Source))
                    continue;

                List<Construction> constructions = constructionLibrary.GetObjects<Construction>(name_Source, textComparisonType, caseSensitive);
                if (constructions == null || constructions.Count == 0)
                    continue;

                Construction construction_Destination = constructions.First();
                if (construction_Destination == null)
                    continue;

                result.Add(construction_Destination);

                List<Panel> panels = adjacencyCluster.Panels(construction_Source);
                if (panels == null || panels.Count == 0)
                    continue;

                foreach(Panel panel in panels)
                {
                    Panel panel_Destination = new Panel(panel, construction_Destination);

                    adjacencyCluster.AddObject(panel_Destination);
                }
            }

            return result;
        }

        public static List<Construction> UpdateConstructionsByName(this List<Panel> panels, ConstructionLibrary constructionLibrary, TextComparisonType textComparisonType = TextComparisonType.Equals, bool caseSensitive = true)
        {
            if (panels == null || constructionLibrary == null)
                return null;

            Dictionary<System.Guid, Construction> dictionary = new Dictionary<System.Guid, Construction>();
            for(int i=0; i <= panels.Count; i++)
            {
                Construction construction_Source = panels[i].Construction;
                if (construction_Source == null)
                    continue;

                string name_Source = construction_Source.Name;
                if (string.IsNullOrWhiteSpace(name_Source))
                    continue;

                List<Construction> constructions = constructionLibrary.GetObjects<Construction>(name_Source, textComparisonType, caseSensitive);
                if (constructions == null || constructions.Count == 0)
                    continue;

                Construction construction_Destination = constructions.First();
                if (construction_Destination == null)
                    continue;

                dictionary[construction_Destination.Guid] = construction_Destination;

                panels[i] = new Panel(panels[i], construction_Destination);
            }

            return dictionary.Values.ToList();
        }
    }
}