namespace SAM.Geometry.Planar
{
    public interface IBoundable2D : ISAMGeometry2D
    {
        BoundingBox2D GetBoundingBox(double offset = 0);
    }
}