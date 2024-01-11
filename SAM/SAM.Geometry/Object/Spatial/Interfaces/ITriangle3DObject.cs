using SAM.Geometry.Spatial;

namespace SAM.Geometry.Object.Spatial
{
    public interface ITriangle3DObject : ISAMGeometry3DObject
    {
        Triangle3D Triangle3D { get; }
    }
}