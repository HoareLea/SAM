using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static ConstructionLayer InternalConstructionLayer(this Construction construction)
        {
            List<ConstructionLayer> constructionLayers = construction?.ConstructionLayers;
            if (constructionLayers == null || constructionLayers.Count == 0)
                return null;

            return constructionLayers.First();
        }

        public static ConstructionLayer InternalConstructionLayer(this Panel panel)
        {
            return InternalConstructionLayer(panel?.Construction);


        }
    }
}