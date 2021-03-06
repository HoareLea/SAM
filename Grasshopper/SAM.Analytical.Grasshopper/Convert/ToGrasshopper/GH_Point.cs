﻿using Grasshopper.Kernel.Types;

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