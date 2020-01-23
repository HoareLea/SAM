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
        public static GH_Surface ToGrasshopper(this Panel Panel)
        {
            return SAM.Geometry.Grasshopper.Convert.ToGrasshopper(Panel.GetFace());
        }
    }
}
