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

        public static List<Spatial.IGeometry3D> ToSAM(this GH_Surface surface, bool simplify = true)
        {
            return ToSAM(surface.Value);
        }

        public static Spatial.IGeometry3D ToSAM(this Surface surface, bool simplify = true)
        {
            List<Spatial.ICurve3D> curve3Ds = new List<Spatial.ICurve3D>();
            foreach (Curve curve in surface.ToBrep().Curves3D)
                curve3Ds.Add(curve.ToSAM(simplify) as Spatial.ICurve3D);

            if (surface.IsPlanar())
                return new Spatial.Face(new Spatial.Polygon3D(curve3Ds.ConvertAll(x => x.GetStart())));

            return new Spatial.Surface(new Spatial.PolycurveLoop3D(curve3Ds));
        }

    }
}
