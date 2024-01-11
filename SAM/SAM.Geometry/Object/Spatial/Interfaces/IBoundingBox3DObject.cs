using SAM.Geometry.Spatial;

namespace SAM.Geometry.Object.Spatial
{
    public interface IBoundingBox3DObject : ISAMGeometry3DObject
    {
        BoundingBox3D BoundingBox3D { get; }
    }
}