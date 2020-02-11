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
        public static GH_Curve ToGrasshopper(this Spatial.Polygon3D polygon3D)
        {
            return new GH_Curve(polygon3D.ToRhino_PolylineCurve());
        }

        public static GH_Curve ToGrasshopper(this Planar.Polygon2D polygon2D)
        {
            return new GH_Curve(polygon2D.ToRhino_PolylineCurve());
        }

        public static GH_Curve ToGrasshopper(this Spatial.Polyline3D polyline3D)
        {
            return new GH_Curve(polyline3D.ToRhino_PolylineCurve());
        }

        public static GH_Curve ToGrasshopper(this Spatial.Polycurve3D polycurve3D)
        {
            return new GH_Curve(Rhino.Geometry.PolyCurve.JoinCurves((polycurve3D).GetCurves().ConvertAll(x => x.ToRhino()), Tolerance.MicroDistance, false)[0]);
        }
    }
}
