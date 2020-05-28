using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System.Collections.Generic;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static Spatial.ISAMGeometry3D ToSAM(this GH_Curve curve, bool simplify = true)
        {
            if (curve.Value is LineCurve)
                return ((LineCurve)curve.Value).Line.ToSAM();
            else
                return ToSAM(curve.Value, simplify);
        }

        public static Spatial.ISAMGeometry3D ToSAM(this Curve curve, bool simplify = true)
        {
            if (curve is PolylineCurve)
                return ((PolylineCurve)curve).ToSAM();

            if (curve is PolyCurve)
                return ((PolyCurve)curve).ToSAM();

            if (curve is LineCurve)
                return ((LineCurve)curve).ToSAM();

            if (simplify)
            {
                PolylineCurve polylineCurve_Temp = curve.ToPolyline(Core.Tolerance.Distance, Core.Tolerance.Angle, 0.4, 1);
                return polylineCurve_Temp.ToSAM();
            }
            else
            {
                PolylineCurve polylineCurve = ToRhino_PolylineCurve(curve);
                return polylineCurve.ToSAM();
            }
        }

        public static Spatial.ISAMGeometry3D ToSAM(this PolylineCurve polylineCurve, bool simplify = true, double tolerance = Core.Tolerance.Distance)
        {
            int count = polylineCurve.PointCount;
            if (count == 2)
                return new Spatial.Segment3D(polylineCurve.Point(0).ToSAM(), polylineCurve.Point(1).ToSAM());

            List<Spatial.Point3D> point3Ds = new List<Spatial.Point3D>();
            if (polylineCurve.IsClosed)
                count--;

            for (int i = 0; i < count; i++)
                point3Ds.Add(polylineCurve.Point(i).ToSAM());

            if (simplify)
                point3Ds = Spatial.Point3D.SimplifyByAngle(point3Ds, polylineCurve.IsClosed, Core.Tolerance.Angle);

            if (polylineCurve.IsClosed && polylineCurve.IsPlanar(tolerance))
                return Spatial.Create.Polygon3D(point3Ds, tolerance);

            return new Spatial.Polyline3D(point3Ds, polylineCurve.IsClosed);
        }
    }
}