using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Grasshopper.Kernel.Types;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static GH_Curve ToGrasshopper(this Spatial.Polygon3D polygon3D)
        {
            return new GH_Curve(polygon3D.ToRhino());
        }
    }
}
