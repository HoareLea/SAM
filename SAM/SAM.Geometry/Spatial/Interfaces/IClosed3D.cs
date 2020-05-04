namespace SAM.Geometry.Spatial
{
    public interface IClosed3D : IBoundable3D
    {
        IClosed3D GetExternalEdge();
    }
}