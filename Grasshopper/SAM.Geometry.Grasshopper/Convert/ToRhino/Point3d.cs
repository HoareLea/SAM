namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static Rhino.Geometry.Point3d ToRhino(this Spatial.Point3D point3D)
        {
            return new Rhino.Geometry.Point3d(point3D.X, point3D.Y, point3D.Z);
        }

        public static Rhino.Geometry.Point3d ToRhino(this Planar.Point2D point2D)
        {
            return new Rhino.Geometry.Point3d(point2D.X, point2D.Y, 0);
        }
    }
}