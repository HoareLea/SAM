namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        /// <summary>
        /// Checks if point is inside or on closed shape in 2D (excluding holes)
        /// </summary>
        /// <param name="closed2D">CLosed2D Shape</param>
        /// <param name="point2D">Point2D</param>
        /// <returns>True if poin2D is inside or on given closed shape</returns>
        public static bool InRange(this IClosed2D closed2D, Point2D point2D, double tolerance = Core.Tolerance.Distance)
        {
            if (closed2D == null || point2D == null)
                return false;

            IClosed2D closed2D_Temp = closed2D;
            if (closed2D_Temp is Face2D)
                closed2D_Temp = ((Face2D)closed2D_Temp).ExternalEdge;

            return closed2D_Temp.On(point2D, tolerance) || closed2D_Temp.Inside(point2D);
        }
    }
}