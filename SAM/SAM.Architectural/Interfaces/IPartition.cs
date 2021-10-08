using SAM.Core;
using SAM.Geometry.Spatial;

namespace SAM.Architectural
{
    public interface IPartition : IArchitecturalObject, ISAMObject
    {
        
        Face3D Face3D { get; }
    }
}
