using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static Spatial.Polycurve3D ToSAM(this Rhino.Geometry.PolyCurve polyCurve)
        {
            List<Rhino.Geometry.Curve> curves = polyCurve.Explode().ToList();
            List<Spatial.ICurve3D> curve3Ds = Spatial.Query.Explode(curves.ConvertAll(x => x.ToSAM() as Spatial.ICurve3D));
            return new Spatial.Polycurve3D(curve3Ds);
        }
    }
}
