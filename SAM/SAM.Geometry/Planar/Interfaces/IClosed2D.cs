namespace SAM.Geometry.Planar
{
    public interface IClosed2D : ISAMGeometry2D, IBoundable2D
    {
        /// <summary>
        /// Check if closed2D inside this geometry
        /// </summary>
        /// <param name="closed2D">Geometry to be checked</param>
        /// <param name="tolerance">Tolerance</param>
        /// <returns></returns>
        bool Inside(IClosed2D closed2D, double tolerance = Core.Tolerance.Distance);

        bool Inside(Point2D point2D, double tolerance = Core.Tolerance.Distance);

        bool On(Point2D point2D, double tolerance = Core.Tolerance.Distance);

        Point2D GetInternalPoint2D(double tolerance = Core.Tolerance.Distance);

        Point2D GetCentroid();

        double GetArea();
    }
}