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
        public static Spatial.Polycurve3D ToSAM(this Rhino.Geometry.PolyCurve polyCurve)
        {
            return new Spatial.Polycurve3D(polyCurve.Explode().ToList().ConvertAll(x => x.ToSAM() as Spatial.ICurve3D));
        }
    }
}
