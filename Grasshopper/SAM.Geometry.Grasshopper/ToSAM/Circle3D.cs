using GH_IO.Types;
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
        public static Spatial.Circle3D ToSAM(this Rhino.Geometry.Circle circle)
        {
            return new Spatial.Circle3D(circle.Plane.ToSAM(), circle.Radius);
        }

        public static Spatial.Circle3D ToSAM(this GH_Circle circle)
        {
            return circle.Value.ToSAM();
        }
    }
}
