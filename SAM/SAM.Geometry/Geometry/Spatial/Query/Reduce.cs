// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static Face3D Reduce(this Face3D face3D, double minDistance)
        {
            if (face3D == null)
                return null;

            Plane plane = face3D.GetPlane();
            if (plane == null)
            {
                return new Face3D(face3D);
            }

            Planar.Face2D face2D = Planar.Query.Reduce(plane.Convert(face3D), minDistance);
            if (face2D == null)
            {
                return new Face3D(face3D);
            }

            return plane.Convert(face2D);
        }
    }
}
