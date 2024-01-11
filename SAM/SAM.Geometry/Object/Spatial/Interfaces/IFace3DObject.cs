using SAM.Geometry.Spatial;

namespace SAM.Geometry.Object.Spatial
{
    public interface IFace3DObject : ISAMGeometry3DObject
    {
        Face3D Face3D { get; }
    }
}