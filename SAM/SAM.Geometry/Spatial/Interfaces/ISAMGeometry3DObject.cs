namespace SAM.Geometry.Spatial
{
    public interface ISAMGeometry3DObject : ISAMGeometryObject
    {
        void Move(Vector3D vector3D);

        void Transform(Transform3D transform3D);
    }
}