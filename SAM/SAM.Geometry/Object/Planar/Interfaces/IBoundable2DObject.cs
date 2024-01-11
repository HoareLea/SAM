using SAM.Geometry.Planar;

namespace SAM.Geometry.Object.Planar
{
    public interface IBoundable2DObject : ISAMGeometry2DObject
    {
        BoundingBox2D GetBoundingBox(double offset = 0);
    }
}