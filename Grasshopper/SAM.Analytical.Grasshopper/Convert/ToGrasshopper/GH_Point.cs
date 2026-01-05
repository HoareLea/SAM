// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel.Types;

namespace SAM.Analytical.Grasshopper
{
    public static partial class Convert
    {
        public static GH_Point ToGrasshopper(this Space space)
        {
            if (space == null)
                return null;

            return Geometry.Grasshopper.Convert.ToGrasshopper(space.Location);
        }
    }
}
