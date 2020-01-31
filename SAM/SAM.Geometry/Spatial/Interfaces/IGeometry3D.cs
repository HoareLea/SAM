namespace SAM.Geometry.Spatial
{
    public interface IGeometry3D : IGeometry
    {
        IGeometry3D GetMoved(Vector3D vector3D);
    }
}
