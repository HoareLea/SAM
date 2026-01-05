// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

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
