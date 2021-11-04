using Grasshopper.Kernel.Types;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static GH_Curve ToGrasshopper(this Spatial.Polygon3D polygon3D)
        {
            return new GH_Curve(Rhino.Convert.ToRhino_PolylineCurve(polygon3D));
        }

        public static GH_Curve ToGrasshopper(this Planar.Polygon2D polygon2D)
        {
            return new GH_Curve(Rhino.Convert.ToRhino_PolylineCurve(polygon2D));
        }

        public static GH_Curve ToGrasshopper(this Spatial.Polyline3D polyline3D, bool close)
        {
            return new GH_Curve(Rhino.Convert.ToRhino_PolylineCurve(polyline3D, close));
        }

        public static GH_Curve ToGrasshopper(this Spatial.Polycurve3D polycurve3D)
        {
            return new GH_Curve(global::Rhino.Geometry.Curve.JoinCurves(polycurve3D.GetCurves().ConvertAll(x => Rhino.Convert.ToRhino(x)), Core.Tolerance.Distance, false)[0]);
        }
    }
}