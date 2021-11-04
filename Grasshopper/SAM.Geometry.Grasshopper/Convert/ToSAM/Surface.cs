using Grasshopper.Kernel.Types;
using System.Collections.Generic;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static List<Spatial.ISAMGeometry3D> ToSAM(this GH_Brep brep, bool simplify = true)
        {
            return brep.Value.ToSAM(simplify);
        }
    }
}