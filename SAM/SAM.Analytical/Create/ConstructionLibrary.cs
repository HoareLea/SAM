using SAM.Core;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static ConstructionLibrary ConstructionLibrary(this ConstructionLibrary constructionLibrary, DelimitedFileTable delimitedFileTable, string columnName_Source, string columnName_Destination = null)
        {
            if (constructionLibrary == null || delimitedFileTable == null || string.IsNullOrWhiteSpace(columnName_Source))
                return null;

            int index_Source = delimitedFileTable.GetColumnIndex(columnName_Source);
            if (index_Source == -1)
                return null;

            int index_Destination = delimitedFileTable.GetColumnIndex(columnName_Destination);

            ConstructionLibrary result = new ConstructionLibrary(constructionLibrary.Name);
            for(int i =0; i < delimitedFileTable.RowCount; i++)
            {
                string name_Template = delimitedFileTable.ToString(i, index_Source);
                if (string.IsNullOrWhiteSpace(name_Template))
                    continue;

                string name_Destination = name_Template;
                if(index_Destination != -1)
                    name_Destination = delimitedFileTable.ToString(i, index_Destination);

                if (string.IsNullOrWhiteSpace(name_Destination))
                    name_Destination = name_Template;

                Construction construction = constructionLibrary.GetConstructions(name_Template, TextComparisonType.Equals, true)?.FirstOrDefault();
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