namespace SAM.Geometry.Planar
{
    public interface IClosed2D : ISAMGeometry2D, IBoundable2D
    {
        //closed2D inside this
        bool Inside(IClosed2D closed2D, double tolerance = Core.Tolerance.Distance);

        bool Inside(Point2D point2D, double tolerance = Core.Tolerance.Distance);

        bool On(Point2D point2D, double tolerance = Core.Tolerance.Distance);

        Point2D GetInternalPoint2D(double tolerance = Core.Tolerance.Distance);

        Point2D GetCentroid();

        double GetArea();
    }
}