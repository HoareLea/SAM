// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors


namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double MinElevation(this BuildingModel buildingModel, Space space)
        {
            if (buildingModel == null || space == null)
            {
                return double.NaN;
            }

            Geometry.Spatial.BoundingBox3D boundingBox3D = buildingModel.GetShell(space)?.GetBoundingBox();
            if (boundingBox3D == null)
            {
                return double.NaN;
            }

            return boundingBox3D.Min.Z;
        }
    }
}
