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

            double min = dimensionIndex == 0 ? boundingBox3D.MinX : (dimensionIndex == 1 ? boundingBox3D.MinY : boundingBox3D.MinZ);
            double max = dimensionIndex == 0 ? boundingBox3D.MaxX : (dimensionIndex == 1 ? boundingBox3D.MaxY : boundingBox3D.MaxZ);
            return new Range<double>(min, max);
        }
    }
}
