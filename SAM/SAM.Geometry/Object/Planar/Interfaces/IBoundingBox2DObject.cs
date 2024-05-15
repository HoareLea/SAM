using SAM.Geometry.Planar;

namespace SAM.Geometry.Object.Planar
{
    public interface IBoundingBox2DObject : ISAMGeometry2DObject, IBoundable2DObject
    {
        BoundingBox2D BoundingBox2D { get; }
    }
}