namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static bool Similar(this Segment2D segment2D_1, Segment2D segment2D_2)
        {
            if (segment2D_1 == segment2D_2)
                return true;

            if (segment2D_1 == null || segment2D_2 == null)
                return false;

            return (segment2D_1[0].Equals(segment2D_2[0]) && segment2D_1[1].Equals(segment2D_2[1])) || (segment2D_1[0].Equals(segment2D_2[1]) && segment2D_1[1].Equals(segment2D_2[0]));
        }
    }
}