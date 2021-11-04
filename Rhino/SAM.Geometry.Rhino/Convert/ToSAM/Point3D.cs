namespace SAM.Geometry.Rhino
{
    public static partial class Convert
    {
        public static Spatial.Point3D ToSAM(this global::Rhino.Geometry.Point3d point3d)
        {
            return new Spatial.Point3D(point3d.X, point3d.Y, point3d.Z);
        }

        public static Spatial.Point3D ToSAM(this global::Rhino.Geometry.Point3f point3f)
        {
            return new Spatial.Point3D(point3f.X, point3f.Y, point3f.Z);
        }
        public static Spatial.Point3D ToSAM(this global::Rhino.Geometry.Point point)
        {
            return ToSAM(point.Location);
        }
    }
}