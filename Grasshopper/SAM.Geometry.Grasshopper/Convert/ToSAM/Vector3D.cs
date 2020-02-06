using GH_IO.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static Spatial.Vector3D ToSAM(this Rhino.Geometry.Vector3d vector3d)
        {
            return new Spatial.Vector3D(vector3d.X, vector3d.Y, vector3d.Z);
        }
    }
}
