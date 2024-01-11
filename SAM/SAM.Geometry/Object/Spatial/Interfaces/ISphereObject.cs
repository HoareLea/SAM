using SAM.Geometry.Spatial;

namespace SAM.Geometry.Object.Spatial
{
    public interface ISphereObject : ISAMGeometry3DObject
    {
        Sphere Sphere { get; }
    }
}