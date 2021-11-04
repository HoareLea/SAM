using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static Spatial.Polycurve3D ToSAM(this Rhino.Geometry.PolyCurve polyCurve)
        {
            List<Rhino.Geometry.Curve> curves = polyCurve.Explode().ToList();

            List<Spatial.ICurve3D> curve3Ds = new List<Spatial.ICurve3D>();
            foreach (Rhino.Geometry.Curve curve in curves)
            {
                if (!curve.IsLinear())
                    curve3Ds.AddRange(Spatial.Query.Explode(curve.ToSAM() as Spatial.ICurve3D));
                else
                    curve3Ds.Add(new Spatial.Segment3D(curve.PointAtStart.ToSAM(), curve.PointAtEnd.ToSAM()));
            }

            return new Spatial.Polycurve3D(curve3Ds);
        }
    }
}