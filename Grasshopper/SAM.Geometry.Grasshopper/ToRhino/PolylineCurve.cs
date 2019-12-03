using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Grasshopper.Kernel.Types;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static Rhino.Geometry.PolylineCurve ToRhino(this Spatial.Polygon3D polygon3D)
        {
            return ToRhino_PolylineCurve(polygon3D);
        }

        public static Rhino.Geometry.PolylineCurve ToRhino_PolylineCurve(this Spatial.Polygon3D polygon3D)
        {
            List<Rhino.Geometry.Point3d> points = polygon3D.Points.ConvertAll(x => x.ToRhino());
            points.Add(polygon3D.Points.First().ToRhino());

            return new Rhino.Geometry.PolylineCurve(points);
        }
    }
}
