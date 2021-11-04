namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static Rhino.Geometry.LineCurve ToRhino_LineCurve(this Spatial.Segment3D segment3D)
        {
            return new Rhino.Geometry.LineCurve(segment3D[0].ToRhino(), segment3D[1].ToRhino());
        }
    }
}