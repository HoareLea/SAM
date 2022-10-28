namespace SAM.Geometry.Spatial
{
    public interface IPolygon3DObject : ISAMGeometry3DObject
    {
        Polygon3D Polygon3D { get; }
    }
}