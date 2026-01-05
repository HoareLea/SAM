// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Geometry.Spatial;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static ConstructionLayer FirstConstructionLayer(this Panel panel, Vector3D direction)
        {
            if (panel == null || direction == null)
                return null;

            Vector3D normal = panel.Normal;
            if (normal == null)
                return null;

            Construction construction = panel.Construction;
            if (construction == null)
                return null;

            if (direction.SameHalf(normal))
                return InternalConstructionLayer(construction);

            return ExternalConstructionLayer(construction);
        }
    }
}
