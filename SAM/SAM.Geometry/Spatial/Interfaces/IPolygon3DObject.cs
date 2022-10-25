namespace SAM.Geometry.Spatial
{
    public interface IPolygon3DObject : ISegmentable3DObject
    {
        Polygon3D Polygon3D { get; }
    }
}