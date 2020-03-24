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
        public static GH_Vector ToGrasshopper(this Spatial.Vector3D vector3D)
        {
            return new GH_Vector(vector3D.ToRhino());
        }

        public static GH_Vector ToGrasshopper(this Planar.Vector2D vector2D)
        {
            return new GH_Vector(vector2D.ToRhino());
        }
    }
}
