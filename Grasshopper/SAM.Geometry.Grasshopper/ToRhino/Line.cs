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
        public static Rhino.Geometry.Line ToRhino(this Spatial.Segment3D segment3D)
        {
            return ToRhino_Line(segment3D);
        }

        public static Rhino.Geometry.Line ToRhino_Line(this Spatial.Segment3D segment3D)
        {
            return new Rhino.Geometry.Line(segment3D[0].ToRhino(), segment3D[1].ToRhino());
        }
    }
}
