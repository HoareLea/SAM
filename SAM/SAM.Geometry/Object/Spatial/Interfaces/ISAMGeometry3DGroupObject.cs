using SAM.Geometry.Spatial;

namespace SAM.Geometry.Object.Spatial
{
    public interface ISAMGeometry3DGroupObject : ISAMGeometry3DObject
    {
        SAMGeometry3DGroup SAMGeometry3DGroup { get; }
    }
}