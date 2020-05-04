namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static bool IsValid(this Segment2D segment2D)
        {
            if (segment2D == null)
                return false;

            if (!IsValid(segment2D[0]))
                return false;

            if (!IsValid(segment2D[1]))
                return false;

            return true;
        }

        public static bool IsValid(this Point2D point2D)
        {
            if (point2D == null)
                return false;

            if (point2D.IsNaN())
                return false;

            return true;
        }
    }
}