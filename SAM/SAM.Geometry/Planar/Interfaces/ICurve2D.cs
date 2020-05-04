namespace SAM.Geometry.Planar
{
    public interface ICurve2D : ISAMGeometry2D, IBoundable2D
    {
        Point2D GetStart();

        Point2D GetEnd();

        double GetLength();

        void Reverse();
    }
}