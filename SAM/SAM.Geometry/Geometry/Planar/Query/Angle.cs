namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static double Angle(this Point2D point2D_Previous, Point2D point2D, Point2D point2D_Next)
        {
            if (point2D_Previous == null || point2D == null || point2D_Next == null)
                return double.NaN;

            return (new Vector2D(point2D, point2D_Previous).Angle(new Vector2D(point2D, point2D_Next)));
        }
    }
}