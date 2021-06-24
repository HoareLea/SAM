namespace SAM.Geometry.Spatial
{
    public interface ISAMGeometry3D : ISAMGeometry
    {
        ISAMGeometry3D GetMoved(Vector3D vector3D);

        ISAMGeometry3D GetTransformed(Transform3D transform3D);
    }
}