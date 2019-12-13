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
        public static Rhino.Geometry.Polyline ToRhino(this Spatial.Polygon3D polygon3D)
        {
            return new Rhino.Geometry.Polyline(polygon3D.Points.ConvertAll(x => x.ToRhino()));
        }
        public static Rhino.Geometry.Polyline ToRhino_Polyline(this Spatial.Polyline3D polyline3D)
        {
            return new Rhino.Geometry.Polyline(polyline3D.Points.ConvertAll(x => x.ToRhino()));
        }
    }
}
