namespace SAM.Geometry.Rhino
{
    public static partial class Convert
    {
        public static Spatial.Segment3D ToSAM(this global::Rhino.Geometry.Line line)
        {
            return new Spatial.Segment3D(line.From.ToSAM(), line.To.ToSAM());
        }

        public static Spatial.Segment3D ToSAM(this global::Rhino.Geometry.LineCurve lineCurve)
        {
            return new Spatial.Segment3D(lineCurve.PointAtStart.ToSAM(), lineCurve.PointAtEnd.ToSAM());
        }
    }
}