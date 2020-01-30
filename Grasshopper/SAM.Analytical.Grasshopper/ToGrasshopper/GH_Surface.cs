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
            return panel.PlanarBoundary3D.ToGrasshopper();
        }

        public static GH_Surface ToGrasshopper(this PlanarBoundary3D planarBoundary3D)
        {
            return new GH_Surface(planarBoundary3D.ToRhino());
        }
    }
}
