namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static Point2D Mid(this Point2D point2D_1, Point2D point2D_2)
        {
            if (point2D_1 == null || point2D_2 == null)
                return null;

            return new Point2D((point2D_1.X + point2D_2.X) / 2, (point2D_1.Y + point2D_2.Y) / 2);
        }
    }
}