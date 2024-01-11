using SAM.Geometry.Planar;

namespace SAM.Geometry.Object.Planar
{
    public interface IPolyline2DObject : ISAMGeometry2DObject
    {
        Polyline2D Polyline2D { get; }
    }
}