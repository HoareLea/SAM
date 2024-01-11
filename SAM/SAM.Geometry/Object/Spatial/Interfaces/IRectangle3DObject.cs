using SAM.Geometry.Spatial;

namespace SAM.Geometry.Object.Spatial
{
    public interface IRectangle3DObject : ISAMGeometry3DObject
    {
        Rectangle3D Rectangle3D { get; }
    }
}