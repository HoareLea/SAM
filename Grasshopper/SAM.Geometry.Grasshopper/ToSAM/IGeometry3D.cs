using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry.Grasshopper.ToSAM
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
            if (!curve.IsClosed || !curve.IsPlanar())
                return null;

            PolylineCurve polylineCurve = curve as PolylineCurve;
            if (polylineCurve != null)
            {
                List<Spatial.Point3D> point3Ds = new List<Spatial.Point3D>();
                int count = polylineCurve.PointCount;

                if (polylineCurve.IsClosed)
                    count--;

                for (int i = 0; i < count; i++)
                    point3Ds.Add(polylineCurve.Point(i).ToSAM());

                if (curve.IsClosed)
                    return new Spatial.Polygon3D(point3Ds);
                else
                    return new Spatial.Polyline3D(point3Ds);

            }

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

            return null;
        }
    }
}
