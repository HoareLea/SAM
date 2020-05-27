using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static Rhino.Geometry.Curve ToRhino(this Spatial.ICurve3D curve3D)
        {
            //TODO: Add handling of Polyline3D!!
            if (curve3D is Spatial.Segment3D)
                return ToRhino_LineCurve((Spatial.Segment3D)curve3D);

            if (curve3D is Spatial.ICurvable3D)
            {
                List<Spatial.ICurve3D> curve3Ds = ((Spatial.ICurvable3D)curve3D).GetCurves();
                return ToRhino_PolylineCurve(curve3Ds);
            }

            return null;
        }
    }
}