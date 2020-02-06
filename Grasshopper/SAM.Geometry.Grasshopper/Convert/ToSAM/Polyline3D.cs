//using GH_IO.Types;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static Spatial.Polyline3D ToSAM(this Rhino.Geometry.Polyline polyline)
        {
            return new Spatial.Polyline3D(polyline.ToList().ConvertAll(x => x.ToSAM()));
        }
    }
}
