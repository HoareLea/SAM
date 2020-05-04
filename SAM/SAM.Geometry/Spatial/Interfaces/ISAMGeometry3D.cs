namespace SAM.Geometry.Spatial
{
    public interface ISAMGeometry3D : ISAMGeometry
    {
        ISAMGeometry3D GetMoved(Vector3D vector3D);
    }
}