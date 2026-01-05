// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Analytical
{
    public static partial class Convert
    {
        public static ConstructionLayer ToSAM(this Architectural.MaterialLayer materialLayer)
        {
            if (materialLayer == null)
            {
                return null;
            }

            return new ConstructionLayer(materialLayer.Name, materialLayer.Thickness);
        }
    }
}
