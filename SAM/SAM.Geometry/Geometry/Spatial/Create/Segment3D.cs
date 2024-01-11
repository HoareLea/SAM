namespace SAM.Geometry.Spatial
{
    public static partial class Create
    {
        public static Segment3D Segment3D(double x_1, double y_1, double z_1, double x_2, double y_2, double z_2)
        {
            return new Segment3D(new Point3D(x_1, y_1, z_1), new Point3D(x_2, y_2, z_2));
        }
    }
}