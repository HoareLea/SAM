using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Rhino
{
    public static partial class Convert
    {
        public static Spatial.ISAMGeometry3D ToSAM(this global::Rhino.Geometry.PolyCurve polyCurve)
        {
            List<global::Rhino.Geometry.Curve> curves = polyCurve.Explode().ToList();

            List<Spatial.ICurve3D> curve3Ds = new List<Spatial.ICurve3D>();
            foreach (global::Rhino.Geometry.Curve curve in curves)
            {
                if (!curve.IsLinear())
                    curve3Ds.AddRange(Spatial.Query.Explode(curve.ToSAM() as Spatial.ICurve3D));
                else
                    curve3Ds.Add(new Spatial.Segment3D(curve.PointAtStart.ToSAM(), curve.PointAtEnd.ToSAM()));
            }

            if(curve3Ds.TrueForAll(x => x is Segment3D))
            {
                List<Point3D> point3Ds = new List<Point3D>();
                for(int i =0; i < curve3Ds.Count; i++)
                {
                    point3Ds.Add(((Segment3D)curve3Ds[i]).GetStart());
                }

                point3Ds.Add(((Segment3D)curve3Ds.Last()).GetEnd());

                return new Polyline3D(point3Ds);
            }

            return new Polycurve3D(curve3Ds);
        }
    }
}