using System.Collections.Generic;

using Grasshopper.Kernel.Types;
using Rhino.Geometry;


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
