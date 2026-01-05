// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static IMaterial InternalMaterial(this Construction construction, MaterialLibrary materialLibrary)
        {
            if (construction == null || materialLibrary == null)
                return null;

            ConstructionLayer constructionLayer = InternalConstructionLayer(construction);
            if (constructionLayer == null)
                return null;

            return Material(constructionLayer, materialLibrary);
        }

        public static IMaterial InternalMaterial(this Panel panel, MaterialLibrary materialLibrary)
        {
            return InternalMaterial(panel?.Construction, materialLibrary);
        }
    }
}
