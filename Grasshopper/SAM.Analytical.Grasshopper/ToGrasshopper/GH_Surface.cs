using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Grasshopper.Kernel.Types;

namespace SAM.Analytical.Grasshopper
{
    public static partial class Convert
    {
        public static GH_Surface ToGrasshopper(this Panel panel)
        {
            return panel.Boundary3D.ToGrasshopper();
        }

        public static GH_Surface ToGrasshopper(this Boundary3D boundary3D)
        {
            return new GH_Surface(boundary3D.ToRhino());
        }
    }
}
