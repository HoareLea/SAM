//using GH_IO.Types;
using System.Linq;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static Spatial.Polyline3D ToSAM(this Rhino.Geometry.Polyline polyline)
        {
            return new Spatial.Polyline3D(polyline.ToList().ConvertAll(x => x.ToSAM()));
        }
    }
}