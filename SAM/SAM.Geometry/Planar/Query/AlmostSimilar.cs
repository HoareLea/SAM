namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static bool AlmostSimilar(this Segment2D segment2D_1, Segment2D segment2D_2, double tolerance = Core.Tolerance.Distance)
        {
            if (segment2D_1 == segment2D_2)
                return true;

            if (segment2D_1 == null || segment2D_2 == null)
                return false;

            return (segment2D_1[0].AlmostEquals(segment2D_2[0], tolerance) && segment2D_1[1].AlmostEquals(segment2D_2[1], tolerance)) || (segment2D_1[0].AlmostEquals(segment2D_2[1], tolerance) && segment2D_1[1].AlmostEquals(segment2D_2[0], tolerance));
        }
    }
}
