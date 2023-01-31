namespace SAM.Geometry.Spatial
{
    public interface IBoundingBox3DObject : ISAMGeometry3DObject
    {
        BoundingBox3D BoundingBox3D { get; }
    }
}