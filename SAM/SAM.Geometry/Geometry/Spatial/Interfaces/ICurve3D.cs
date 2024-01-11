namespace SAM.Geometry.Spatial
{
    public interface ICurve3D : IBoundable3D
    {
        Point3D GetStart();

        Point3D GetEnd();

        double GetLength();

        void Reverse();
    }
}