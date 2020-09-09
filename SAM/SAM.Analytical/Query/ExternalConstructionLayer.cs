using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static ConstructionLayer ExternalConstructionLayer(this Construction construction)
        {
            List<ConstructionLayer> constructionLayers = construction?.ConstructionLayers;
            if (constructionLayers == null || constructionLayers.Count == 0)
                return null;

            return constructionLayers.Last();
        }

        public static ConstructionLayer ExternalConstructionLayer(this Panel panel)
        {
            return ExternalConstructionLayer(panel?.Construction);
        }
    }
}