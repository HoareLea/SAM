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
        public static Rhino.Geometry.Point3d ToRhino(this Spatial.Point3D point3D)
        {
            return new Rhino.Geometry.Point3d(point3D.X, point3D.Y, point3D.Z);
        }
    }
}
