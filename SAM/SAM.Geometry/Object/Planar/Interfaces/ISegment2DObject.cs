using SAM.Geometry.Planar;

namespace SAM.Geometry.Object.Planar
{
    public interface ISegment2DObject : IBoundable2DObject
    {
        Segment2D Segment2D { get; }
    }
}