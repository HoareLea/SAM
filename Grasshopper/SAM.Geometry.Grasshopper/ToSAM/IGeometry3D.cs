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
        public static Spatial.IGeometry3D ToSAM(this GH_Curve curve)
        {
            if (curve.Value is LineCurve)
                return ((LineCurve)curve.Value).Line.ToSAM();
            else
                return ToSAM(curve.Value);
        }

        public static Spatial.IGeometry3D ToSAM(this Curve curve)
        {
            if (!curve.IsPlanar())
                return null;

            PolylineCurve polylineCurve = curve as PolylineCurve;
            if (polylineCurve != null)
                return polylineCurve.ToSAM();

            PolyCurve polyCurve = curve as PolyCurve;
            if (polyCurve != null)
            {
                List<Spatial.Point3D> point3Ds = new List<Spatial.Point3D>();
                Curve[] curves = polyCurve.Explode();
                foreach (Curve curve_Temp in curves)
                    point3Ds.Add(curve_Temp.PointAtEnd.ToSAM());

                if (curve.IsClosed)
                    return new Spatial.Polygon3D(point3Ds);
                else
                    return new Spatial.Polyline3D(point3Ds);
            }

            polylineCurve = ToRhino_PolylineCurve(curve);
            if (polylineCurve != null)
                return polylineCurve.ToSAM();

            return null;
        }

        public static Spatial.IGeometry3D ToSAM(this PolylineCurve polylineCurve)
        {
            int count = polylineCurve.PointCount;
            if (count == 2)
                return new Spatial.Segment3D(polylineCurve.Point(0).ToSAM(), polylineCurve.Point(1).ToSAM());

            List<Spatial.Point3D> point3Ds = new List<Spatial.Point3D>();
            if (polylineCurve.IsClosed)
                count--;

            for (int i = 0; i < count; i++)
                point3Ds.Add(polylineCurve.Point(i).ToSAM());

            if (polylineCurve.IsClosed)
                return new Spatial.Polygon3D(point3Ds);
            else
                return new Spatial.Polyline3D(point3Ds);
        }

        public static Spatial.IGeometry3D ToSAM(this IGH_GeometricGoo geometricGoo)
        {
            if (geometricGoo is GH_Curve)
                return ((GH_Curve)geometricGoo).ToSAM();

            if (geometricGoo is GH_Point)
                return ((GH_Point)geometricGoo).ToSAM();

            return (geometricGoo as dynamic).ToSAM();
        }

    }
}
