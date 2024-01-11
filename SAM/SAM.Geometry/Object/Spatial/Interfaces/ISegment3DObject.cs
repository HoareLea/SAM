using SAM.Geometry.Spatial;

namespace SAM.Geometry.Object.Spatial
{
    public interface ISegment3DObject : ISAMGeometry3DObject
    {
        Segment3D Segment3D { get; }
    }
}