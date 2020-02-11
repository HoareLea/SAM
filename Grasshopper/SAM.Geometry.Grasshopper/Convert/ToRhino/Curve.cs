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
        public static Rhino.Geometry.Curve ToRhino(this Spatial.ICurve3D curve3D)
        {
            //TODO: Add handling of Polyline3D!!
            if (curve3D is Spatial.Segment3D)
                return Convert.ToRhino_LineCurve((Spatial.Segment3D)curve3D);

            return null;
        }

    }
}
