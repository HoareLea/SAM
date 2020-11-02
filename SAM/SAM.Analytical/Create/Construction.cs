using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static Construction Construction(ConstructionLibrary constructionLibrary, string sourceName, string destinationName)
        {
            if (constructionLibrary == null || string.IsNullOrWhiteSpace(sourceName) || string.IsNullOrWhiteSpace(destinationName))
                return null;

            List<Construction> constructions = constructionLibrary.GetConstructions(sourceName, TextComparisonType.Equals, true);
            if (constructions == null || constructions.Count == 0)
                return null;

            return new Construction(constructions.First(), destinationName);
        }
    }
}