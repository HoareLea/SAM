namespace SAM.Geometry.Rhino
{
    public static partial class Convert
    {
        public static global::Rhino.Geometry.LineCurve ToRhino_LineCurve(this Spatial.Segment3D segment3D)
        {
            return new global::Rhino.Geometry.LineCurve(segment3D[0].ToRhino(), segment3D[1].ToRhino());
        }
    }
}