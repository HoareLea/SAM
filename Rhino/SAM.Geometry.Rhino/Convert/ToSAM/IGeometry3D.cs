using Rhino.Geometry;
using System.Collections.Generic;

namespace SAM.Geometry.Rhino
{
    public static partial class Convert
    {
        public static Spatial.ISAMGeometry3D ToSAM(this Curve curve, bool simplify = true)
        {
            if (curve is PolylineCurve)
                return ((PolylineCurve)curve).ToSAM();

            if (curve is PolyCurve)
                return ((PolyCurve)curve).ToSAM();

            if (curve is LineCurve)
                return ((LineCurve)curve).ToSAM();

            if(curve.IsCircle())
            {
                if(curve.TryGetCircle(out Circle circle))
                {
                    Spatial.Circle3D circle3D = new Spatial.Circle3D(circle.Plane.ToSAM(), circle.Radius);
                    if(!simplify)
                    {
                        return circle3D;
                    }

                    return Spatial.Query.Simplify(circle3D, 10);
                }
            }


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
                point3Ds = Spatial.Query.SimplifyByAngle(point3Ds, polylineCurve.IsClosed, Core.Tolerance.Angle);

            if (polylineCurve.IsClosed && polylineCurve.IsPlanar(tolerance))
                return Spatial.Create.Polygon3D(point3Ds, tolerance);

            return new Spatial.Polyline3D(point3Ds, polylineCurve.IsClosed);
        }
    }
}