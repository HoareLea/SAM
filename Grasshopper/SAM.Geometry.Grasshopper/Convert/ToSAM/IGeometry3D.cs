using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static Spatial.ISAMGeometry3D ToSAM(this GH_Curve curve, bool simplify = true)
        {
            if (curve.Value is LineCurve)
                return Rhino.Convert.ToSAM(((LineCurve)curve.Value).Line);
            else
                return Rhino.Convert.ToSAM( curve.Value, simplify);
        }
    }
}