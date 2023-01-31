namespace SAM.Geometry.Spatial
{
    public interface IPolyline3DObject : ISAMGeometry3DObject
    {
        Polyline3D Polyline3D { get; }
    }
}