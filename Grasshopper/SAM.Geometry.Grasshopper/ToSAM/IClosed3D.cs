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
        public static Spatial.IClosed3D ToSAM(this BrepLoop brepLoop, bool simplify = true)
        {
            List<Spatial.ICurve3D> curve3Ds = new List<Spatial.ICurve3D>();
            foreach (BrepTrim brepTrim in brepLoop.Trims)
                curve3Ds.Add((Spatial.ICurve3D)ToSAM(brepTrim.TrimCurve, simplify));

            if (brepLoop.Face.IsPlanar(Tolerance.MicroDistance))
                return new Spatial.Face(new Spatial.Polygon3D(curve3Ds.ConvertAll(x => x.GetStart())));

            return new Spatial.Surface(new Spatial.PolycurveLoop3D(curve3Ds));
        }
    }
}
