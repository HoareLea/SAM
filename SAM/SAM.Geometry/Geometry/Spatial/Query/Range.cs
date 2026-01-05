// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static Range<double> Range(this BoundingBox3D boundingBox3D, int dimensionIndex)
        {
            if (boundingBox3D == null)
            {
                return null;
            }

            return new Range<double>(boundingBox3D.Min[dimensionIndex], boundingBox3D.Max[dimensionIndex]);
        }
    }
}
