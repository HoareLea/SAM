namespace SAM.Geometry.Spatial
{
    public interface IBoundable3DObject : ISAMGeometry3DObject
    {
        BoundingBox3D GetBoundingBox(double offset = 0);
    }
}