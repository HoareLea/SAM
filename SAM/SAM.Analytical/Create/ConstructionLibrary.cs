using SAM.Core;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static ConstructionLibrary ConstructionLibrary(this ConstructionLibrary constructionLibrary, DelimitedFileTable delimitedFileTable, string columnName_Source, string columnName_Template, string columnName_Destination = null)
        {
            if (constructionLibrary == null || delimitedFileTable == null || string.IsNullOrWhiteSpace(columnName_Source) || string.IsNullOrWhiteSpace(columnName_Template))
                return null;

            int index_Source = delimitedFileTable.GetIndex(columnName_Source);
            if (index_Source == -1)
                return null;

            int index_Template = delimitedFileTable.GetIndex(columnName_Template);
            if (index_Template == -1)
                return null;

            int index_Destination = delimitedFileTable.GetIndex(columnName_Destination);

            ConstructionLibrary result = new ConstructionLibrary(constructionLibrary.Name);
            for(int i =0; i < delimitedFileTable.Count; i++)
            {
                string name_Source = delimitedFileTable.ToString(i, index_Source);
                if (string.IsNullOrWhiteSpace(name_Source))
                    continue;

                string name_Template = delimitedFileTable.ToString(i, index_Template);
                if (string.IsNullOrWhiteSpace(name_Template))
                    continue;

                string name_Destination = name_Template;
                if(index_Destination != -1)
                    name_Destination = delimitedFileTable.ToString(i, index_Destination);

                if (string.IsNullOrWhiteSpace(name_Destination))
                    name_Destination = name_Template;

                Construction construction = constructionLibrary.GetObjects<Construction>(name_Template, TextComparisonType.Equals, true)?.FirstOrDefault();
                if (construction == null)
                    continue;

                if (!name_Template.Equals(name_Destination))
                    construction = new Construction(construction, name_Destination);

                result.Add(construction);
            }

            return result;
        }
    }
}