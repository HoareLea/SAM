using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Grasshopper.Kernel.Types;
using Rhino.Geometry;


namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static Spatial.Polygon3D ToSAM(this Curve curve)
        {
            if (!curve.IsClosed || !curve.IsPlanar())
                return null;

            PolylineCurve polylineCurve = curve as PolylineCurve;
            if(polylineCurve != null)
            {
                List<Spatial.Point3D> point3Ds = new List<Spatial.Point3D>();
                int count = polylineCurve.PointCount;
                for(int i =0; i < count; i++)
                    point3Ds.Add(polylineCurve.Point(i).ToSAM());

                return new Spatial.Polygon3D(point3Ds);

            }

            PolyCurve polyCurve = curve as PolyCurve;
            if (polyCurve != null)
            {
                List<Spatial.Point3D> point3Ds = new List<Spatial.Point3D>();
                Curve[] curves = polyCurve.Explode();
                foreach (Curve curve_Temp in curves)
                    point3Ds.Add(curve_Temp.PointAtEnd.ToSAM());

                return new Spatial.Polygon3D(point3Ds);
            }

            return null;
        }

        public static Spatial.Polygon3D ToSAM(this GH_Curve curve)
        {
            return ToSAM(curve.Value);
        }
    }
}
