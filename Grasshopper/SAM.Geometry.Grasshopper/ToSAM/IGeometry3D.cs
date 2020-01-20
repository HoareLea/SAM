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
        public static Spatial.IGeometry3D ToSAM(this GH_Curve curve, bool simplify = true)
        {
            if (curve.Value is LineCurve)
                return ((LineCurve)curve.Value).Line.ToSAM();
            else
                return ToSAM(curve.Value, simplify);
        }

        public static Spatial.IGeometry3D ToSAM(this Curve curve, bool simplify = true)
        {
            //if (!curve.IsPlanar())
            //    return null;

            PolylineCurve polylineCurve = curve as PolylineCurve;
            if (polylineCurve != null)
                return polylineCurve.ToSAM(simplify);

            polylineCurve = ToRhino_PolylineCurve(curve);
            if (polylineCurve != null)
                return polylineCurve.ToSAM(simplify);

            return null;
        }

        public static Spatial.IGeometry3D ToSAM(this PolylineCurve polylineCurve, bool simplify = true)
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
                point3Ds = Spatial.Point3D.SimplifyByAngle(point3Ds, polylineCurve.IsClosed, 0.01);

            if (polylineCurve.IsClosed)
                return new Spatial.Polygon3D(point3Ds);
            else
                return new Spatial.Polyline3D(point3Ds);
        }

        public static Spatial.IGeometry3D ToSAM(this IGH_GeometricGoo geometricGoo, bool simplify = true)
        {
            if (geometricGoo is GH_Curve)
                return ((GH_Curve)geometricGoo).ToSAM(simplify);

            if (geometricGoo is GH_Surface)
                return ((GH_Surface)geometricGoo).ToSAM(simplify);

            if (geometricGoo is GH_Point)
                return ((GH_Point)geometricGoo).ToSAM();

            if(geometricGoo is GH_Brep)
                return ((GH_Brep)geometricGoo).ToSAM(simplify);

            return (geometricGoo as dynamic).ToSAM();
        }

    }
}
