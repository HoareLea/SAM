using SAM.Geometry.Spatial;

namespace SAM.Geometry.Object.Spatial
{
    public interface IPolygon3DObject : ISAMGeometry3DObject
    {
        Polygon3D Polygon3D { get; }
    }
}