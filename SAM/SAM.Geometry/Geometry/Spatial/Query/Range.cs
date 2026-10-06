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

            switch (dimensionIndex)
            {
                case 0:
                    return new Range<double>(boundingBox3D.MinX, boundingBox3D.MaxX);
                case 1:
                    return new Range<double>(boundingBox3D.MinY, boundingBox3D.MaxY);
                case 2:
                    return new Range<double>(boundingBox3D.MinZ, boundingBox3D.MaxZ);
                default:
                    throw new System.IndexOutOfRangeException($"Index was outside the bounds of the 3D bounding box (index: {dimensionIndex}).");
            }
        }
    }
}
