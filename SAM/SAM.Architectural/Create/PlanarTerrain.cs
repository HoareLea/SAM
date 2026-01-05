// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Geometry.Spatial;

namespace SAM.Architectural
{
    public static partial class Create
    {
        public static PlanarTerrain PlanarTerrain(double elevation)
        {
            Plane plane = Geometry.Spatial.Create.Plane(elevation);
            if (plane == null)
            {
                return null;
            }

            return new PlanarTerrain(plane);
        }
    }
}
