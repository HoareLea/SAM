using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static ConstructionLibrary UpdateConstructionsByName(this List<Panel> panels, ConstructionLibrary constructionLibrary)
        {
            if (panels == null || constructionLibrary == null)
                return null;

            ConstructionLibrary result = new ConstructionLibrary(constructionLibrary.Name);
            for (int i = 0; i < panels.Count; i++)
            {
                string name = panels[i].Construction?.Name;
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                List<Construction> constructions = result.GetConstructions(name);
                if(constructions == null || constructions.Count == 0)
                {
                    constructions = constructionLibrary.GetConstructions(name);
                }

                if(constructions == null || constructions.Count == 0)
                {
                    continue;
                }

                Construction construction = null;
                foreach(Construction construction_Temp in constructions)
                {
                    if(construction_Temp.PanelType() == panels[i].PanelType)
                    {
                        construction = construction_Temp;
                        break;
                    }
                }

                if(construction == null)
                {
                    construction = constructions[0];
                }

                if(construction == null)
                {
                    continue;
                }

                if (!result.Contains(construction))
                {
                    result.Add(construction);
                }

                panels[i] = new Panel(panels[i], construction);
            }
            return result;
        }

        public static ConstructionLibrary UpdateConstructionsByName(
        this List<Panel> panels,
        ConstructionLibrary constructionLibrary,
        DelimitedFileTable delimitedFileTable,
        string columnName_Source,
        string columnName_Template,
        string columnName_Destination = null)
        {
            if (panels == null || constructionLibrary == null || delimitedFileTable == null)
                return null;

            int index_Template = delimitedFileTable.GetColumnIndex(columnName_Template);
            if (index_Template == -1)
                return null;

            int index_Source = delimitedFileTable.GetColumnIndex(columnName_Source);
            if (index_Source == -1)
                return null;

            int index_Destination = delimitedFileTable.GetColumnIndex(columnName_Destination);

            ConstructionLibrary result = new ConstructionLibrary(constructionLibrary.Name);
            for (int i = 0; i < panels.Count; i++)
            {
                string name = panels[i].Construction?.Name;
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                List<int> indexes = delimitedFileTable.GetRowIndexes(index_Source, name, TextComparisonType.Equals, true);
                if (indexes == null || indexes.Count == 0)
                    continue;

                int index = indexes[0];

                string name_Template = delimitedFileTable.ToString(index, index_Template);
                if (string.IsNullOrWhiteSpace(name_Template))
                    continue;

                string name_Destination = name_Template;
                if (index_Destination != -1)
                {
                    name_Destination = delimitedFileTable.ToString(index, index_Destination);
                    if (string.IsNullOrWhiteSpace(name_Destination))
                        name_Destination = name_Template;
                }

                Construction construction = result.GetConstructions(name_Destination)?.FirstOrDefault();
                if (construction == null)
                {
                    Construction construction_Temp = constructionLibrary.GetConstructions(name_Template, TextComparisonType.Equals, true)?.FirstOrDefault();
                    if (construction_Temp == null)
                        continue;

                    if (name_Destination.Equals(name_Template))
                        construction = construction_Temp;
                    else
                        construction = new Construction(construction_Temp, name_Destination);

                    result.Add(construction);
                }

                panels[i] = new Panel(panels[i], construction);
            }
            return result;
        }

        public static ConstructionLibrary UpdateConstructionsByName(
        this List<Construction> constructions,
        ConstructionLibrary constructionLibrary,
        DelimitedFileTable delimitedFileTable,
        string columnName_Source,
        string columnName_Template,
        string columnName_Destination = null)
        {
            if (constructions == null || constructionLibrary == null || delimitedFileTable == null)
                return null;

            int index_Template = delimitedFileTable.GetColumnIndex(columnName_Template);
            if (index_Template == -1)
                return null;

            int index_Source = delimitedFileTable.GetColumnIndex(columnName_Source);
            if (index_Source == -1)
                return null;

            int index_Destination = delimitedFileTable.GetColumnIndex(columnName_Destination);

            ConstructionLibrary result = new ConstructionLibrary(constructionLibrary.Name);
            for (int i = 0; i < constructions.Count; i++)
            {
                string name = constructions[i].Name;
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                List<int> indexes = delimitedFileTable.GetRowIndexes(index_Source, name, TextComparisonType.Equals, true);
                if (indexes == null || indexes.Count == 0)
                    continue;

                int index = indexes[0];

                string name_Template = delimitedFileTable.ToString(index, index_Template);
                if (string.IsNullOrWhiteSpace(name_Template))
                    continue;

                string name_Destination = name_Template;
                if (index_Destination != -1)
                {
                    name_Destination = delimitedFileTable.ToString(index, index_Destination);
                    if (string.IsNullOrWhiteSpace(name_Destination))
                        name_Destination = name_Template;
                }

                Construction construction = result.GetConstructions(name_Destination)?.FirstOrDefault();
                if (construction == null)
                {
                    Construction construction_Temp = constructionLibrary.GetConstructions(name_Template, TextComparisonType.Equals, true)?.FirstOrDefault();
                    if (construction_Temp == null)
                        continue;

                    if (name_Destination.Equals(name_Template))
                        construction = construction_Temp;
                    else
                        construction = new Construction(construction_Temp, name_Destination);

                    result.Add(construction);
                }

                constructions[i] = construction;
            }
            return result;
        }
    }
}