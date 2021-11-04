namespace SAM.Geometry.Rhino
{
    public static partial class Convert
    {
        public static global::Rhino.Geometry.Line ToRhino(this Spatial.Segment3D segment3D)
        {
            return ToRhino_Line(segment3D);
        }

        public static global::Rhino.Geometry.Line ToRhino(this Planar.Segment2D segment2D)
        {
            return ToRhino_Line(segment2D);
        }

        public static global::Rhino.Geometry.Line ToRhino_Line(this Spatial.Segment3D segment3D)
        {
            return new global::Rhino.Geometry.Line(segment3D[0].ToRhino(), segment3D[1].ToRhino());
        }

        public static global::Rhino.Geometry.Line ToRhino_Line(this Planar.Segment2D segment2D)
        {
            return new global::Rhino.Geometry.Line(segment2D[0].ToRhino(), segment2D[1].ToRhino());
        }
    }
}