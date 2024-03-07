using SAM.Geometry.Planar;

namespace SAM.Geometry.Object.Planar
{
    public interface IPolygon2DObject : ISAMGeometry2DObject
    {
        Polygon2D Polygon2D { get; }
    }
}