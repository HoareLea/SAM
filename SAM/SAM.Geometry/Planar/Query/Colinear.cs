namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static bool Colinear(this Point2D point2D_1, Point2D point2D_2, Point2D point2D_3, double tolerance = Tolerance.MicroDistance)
        {
            if (point2D_1 == null || point2D_2 == null || point2D_3 == null)
                return false;

            return (point2D_1.X * point2D_2.Y) + (point2D_2.X * point2D_3.Y) + (point2D_3.X * point2D_1.Y) - (point2D_1.X * point2D_3.Y) - (point2D_2.X * point2D_1.Y) - (point2D_3.X * point2D_2.Y) <= tolerance;
        }
    }
}
