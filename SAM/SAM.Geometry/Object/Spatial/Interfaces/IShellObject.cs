using SAM.Geometry.Spatial;

namespace SAM.Geometry.Object.Spatial
{
    public interface IShellObject : ISAMGeometry3DObject
    {
        Shell Shell { get; }
    }
}