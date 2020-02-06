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
        public static Spatial.Plane ToSAM(this Rhino.Geometry.Plane plane)
        {
            return new Spatial.Plane(plane.Origin.ToSAM(), plane.Normal.ToSAM());
        }

        public static Spatial.Plane ToSAM(this GH_Plane plane)
        {
            return plane.Value.ToSAM();
        }
    }
}
