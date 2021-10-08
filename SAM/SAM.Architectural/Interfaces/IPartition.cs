using SAM.Geometry.Spatial;

namespace SAM.Architectural
{
    public interface IPartition : IArchitecturalObject
    {
        System.Guid Guid { get; }
        
        Face3D Face3D { get; }
    }
}
