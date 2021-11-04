using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static Rhino.Geometry.Polyline ToRhino(this Spatial.Polygon3D polygon3D)
        {
            List<Spatial.Point3D> point3Ds = polygon3D.GetPoints();
            if (point3Ds == null || point3Ds.Count < 2)
                return null;

            point3Ds.Add(point3Ds.Last());

            return new Rhino.Geometry.Polyline(point3Ds.ConvertAll(x => x.ToRhino()));
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