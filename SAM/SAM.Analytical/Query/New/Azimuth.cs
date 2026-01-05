// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Geometry.Spatial;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double Azimuth(this IPartition partition)
        {
            return Geometry.Object.Spatial.Query.Azimuth(partition, Vector3D.WorldY);
        }
    }
}
