namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static bool Collinear(this Point2D point2D_1, Point2D point2D_2, Point2D point2D_3, double tolerance = Core.Tolerance.Distance)
        {
            if (point2D_1 == null || point2D_2 == null || point2D_3 == null)
                return false;

            return (point2D_1.X * point2D_2.Y) + (point2D_2.X * point2D_3.Y) + (point2D_3.X * point2D_1.Y) - (point2D_1.X * point2D_3.Y) - (point2D_2.X * point2D_1.Y) - (point2D_3.X * point2D_2.Y) <= tolerance;
        }

        public static bool Collinear(this Segment2D segment2D, Point2D point2D, double tolerance = Core.Tolerance.Distance)
        {
            if (segment2D == null || point2D == null)
                return false;

            return Collinear(segment2D[0], segment2D[1], point2D, tolerance);
        }

        public static bool Collinear(this Segment2D segment2D_1, Segment2D segment2D_2, double tolerance = Core.Tolerance.Distance)
        {
            if (segment2D_1 == segment2D_2)
                return true;

            if (segment2D_1 == null || segment2D_2 == null)
                return false;

            return Collinear(segment2D_1, segment2D_2[0], tolerance) && Collinear(segment2D_1, segment2D_2[1], tolerance);
        }
    }
}