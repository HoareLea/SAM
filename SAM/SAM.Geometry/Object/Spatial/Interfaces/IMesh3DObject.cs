using SAM.Geometry.Spatial;

namespace SAM.Geometry.Object.Spatial
{
    public interface IMesh3DObject : ISAMGeometry3DObject
    {
        Mesh3D Mesh3D { get; }
    }
}