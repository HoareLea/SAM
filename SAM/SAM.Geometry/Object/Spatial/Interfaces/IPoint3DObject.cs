using SAM.Geometry.Spatial;

namespace SAM.Geometry.Object.Spatial
{
    public interface IPoint3DObject : ISAMGeometry3DObject
    {
        Point3D Point3D { get; }
    }
}