using SAM.Geometry.Planar;

namespace SAM.Geometry.Object.Planar
{
    public interface IFace2DObject : IBoundable2DObject
    {
        Face2D Face2D { get; }
    }
}