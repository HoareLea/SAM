namespace SAM.Geometry.Planar
{
    public interface IBoundable2DObject : ISAMGeometry2DObject
    {
        BoundingBox2D GetBoundingBox(double offset = 0);
    }
}