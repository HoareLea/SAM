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
        public static GH_Point ToGrasshopper(this Spatial.Point3D point3D)
        {
            return new GH_Point(point3D.ToRhino());
        }
    }
}
