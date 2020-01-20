using System.Collections.Generic;

using Grasshopper.Kernel.Types;
using Rhino.Geometry;


namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static Spatial.Surface ToSAM(this GH_Brep brep, bool simplify = true)
        {
            List<Spatial.ICurve3D> curve3Ds = new List<Spatial.ICurve3D>();
            foreach (Curve curve in brep.Value.Curves3D)
                curve3Ds.Add(curve.ToSAM(simplify) as Spatial.ICurve3D);

            return new Spatial.Surface(new Spatial.PolycurveLoop3D(curve3Ds));
        }
    }
}
