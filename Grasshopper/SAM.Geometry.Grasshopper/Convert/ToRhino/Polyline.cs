using System.Collections.Generic;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static Rhino.Geometry.Polyline ToRhino(this Spatial.Polygon3D polygon3D)
        {
            return new Rhino.Geometry.Polyline(polygon3D.GetPoints().ConvertAll(x => x.ToRhino()));
        }
        public static Rhino.Geometry.Polyline ToRhino_Polyline(this Spatial.Polyline3D polyline3D)
        {
            return new Rhino.Geometry.Polyline(polyline3D.Points.ConvertAll(x => x.ToRhino()));
        }

        public static Rhino.Geometry.Polyline ToRhino_Polyline(List<Spatial.ICurve3D> curve3Ds)
        {
            return new Rhino.Geometry.Polyline(curve3Ds.ConvertAll(x => x.GetStart().ToRhino()));
        }
    }
}
