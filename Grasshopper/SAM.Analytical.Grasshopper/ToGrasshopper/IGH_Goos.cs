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
        public static List<IGH_Goo> ToGrasshopper(this Space space)
        {
            if (space == null)
                return null;

            IGH_Goo iGH_Goo = Geometry.Grasshopper.Convert.ToGrasshopper(space.Location);
            if (iGH_Goo == null)
                return null;

            return new List<IGH_Goo>() { iGH_Goo };
        }
    }
}
