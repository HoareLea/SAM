using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static Spatial.IClosed3D ToSAM(this BrepLoop brepLoop, bool simplify = true)
        {
            if (brepLoop.Face.IsPlanar(Tolerance.MicroDistance))
                return new Spatial.Face(brepLoop.To3dCurve().ToSAM(simplify) as Spatial.IClosedPlanar3D);

            return new Spatial.Surface(brepLoop.To3dCurve().ToSAM(simplify) as Spatial.IClosed3D);
        }
    }
}
