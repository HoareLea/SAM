// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static Vector3D AxisY(this Vector3D normal)
        {
            if (normal == null)
                return null;

            return AxisY(normal, AxisX(normal));

            //if (normal.X == 0 && normal.Y == 0)
            //    return new Vector3D(0, 1, 0);

            //return new Vector3D(-normal.Y, normal.X, 0).Unit;
        }

        public static Vector3D AxisY(this Vector3D normal, Vector3D axisX)
        {
            if (normal == null || axisX == null)
                return null;

            return normal.CrossProduct(axisX).Unit;
        }
    }
}
