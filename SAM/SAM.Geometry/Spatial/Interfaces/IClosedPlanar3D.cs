namespace SAM.Geometry.Spatial
{
    public interface IClosedPlanar3D : IClosed3D, IPlanar3D, IBoundable3D
    {
        double GetArea();
    }
}