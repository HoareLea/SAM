// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;

namespace SAM.Architectural
{
    public static partial class Query
    {
        public static IMaterial Material(this MaterialLayer materialLayer, MaterialLibrary materialLibrary)
        {
            if (materialLayer == null || materialLibrary == null)
                return null;

            return materialLibrary.GetObject<IMaterial>(materialLayer.Name);
        }
    }
}
