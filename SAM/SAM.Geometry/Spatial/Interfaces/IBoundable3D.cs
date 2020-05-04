namespace SAM.Geometry.Spatial
{
    public interface IBoundable3D : ISAMGeometry3D
    {
        BoundingBox3D GetBoundingBox(double offset = 0);
    }
}