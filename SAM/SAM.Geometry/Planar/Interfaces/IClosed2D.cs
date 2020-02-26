namespace SAM.Geometry.Planar
{
    public interface IClosed2D : ISAMGeometry2D, IBoundable2D
    {
        //closed2D inside this
        bool Inside(IClosed2D closed2D);

        bool Inside(Point2D point2D);

        double GetArea();
    }
}
