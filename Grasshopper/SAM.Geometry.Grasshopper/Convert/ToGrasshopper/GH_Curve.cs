using Grasshopper.Kernel.Types;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static GH_Curve ToGrasshopper(this Spatial.Polygon3D polygon3D)
        {
            return new GH_Curve(polygon3D.ToRhino_PolylineCurve());
        }

        public static GH_Curve ToGrasshopper(this Planar.Polygon2D polygon2D)
        {
            return new GH_Curve(polygon2D.ToRhino_PolylineCurve());
        }

        public static GH_Curve ToGrasshopper(this Spatial.Polyline3D polyline3D, bool close)
        {
            return new GH_Curve(polyline3D.ToRhino_PolylineCurve(close));
        }

        public static GH_Curve ToGrasshopper(this Spatial.Polycurve3D polycurve3D)
        {
            return new GH_Curve(Rhino.Geometry.Curve.JoinCurves(polycurve3D.GetCurves().ConvertAll(x => x.ToRhino()), Core.Tolerance.Distance, false)[0]);
        }
    }
}