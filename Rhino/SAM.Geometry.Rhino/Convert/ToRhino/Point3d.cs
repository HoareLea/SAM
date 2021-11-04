namespace SAM.Geometry.Rhino
{
    public static partial class Convert
    {
        public static global::Rhino.Geometry.Point3d ToRhino(this Spatial.Point3D point3D)
        {
            if (point3D == null)
                return global::Rhino.Geometry.Point3d.Unset;
            
            return new global::Rhino.Geometry.Point3d(point3D.X, point3D.Y, point3D.Z);
        }

        public static global::Rhino.Geometry.Point3d ToRhino(this Planar.Point2D point2D)
        {
            if (point2D == null)
                return global::Rhino.Geometry.Point3d.Unset;

            return new global::Rhino.Geometry.Point3d(point2D.X, point2D.Y, 0);
        }
    }
}