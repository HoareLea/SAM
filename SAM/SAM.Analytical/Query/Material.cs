// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static IMaterial Material(this ConstructionLayer constructionLayer, MaterialLibrary materialLibrary)
        {
            if (constructionLayer == null || materialLibrary == null)
                return null;

            return materialLibrary.GetObject<IMaterial>(constructionLayer.Name);
        }
    }
}
