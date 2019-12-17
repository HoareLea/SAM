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
        public static GH_Surface ToGrasshopper(this Spatial.Face face)
        {
            return new GH_Surface(face.ToRhino());
        }
    }
}
