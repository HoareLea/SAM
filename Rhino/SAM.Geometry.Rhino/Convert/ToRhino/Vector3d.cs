namespace SAM.Geometry.Rhino
{
    public static partial class Convert
    {
        public static global::Rhino.Geometry.Vector3d ToRhino(this Spatial.Vector3D vector3D)
        {
            return new global::Rhino.Geometry.Vector3d(vector3D.X, vector3D.Y, vector3D.Z);
        }

        public static global::Rhino.Geometry.Vector3d ToRhino(this Planar.Vector2D vector2D)
        {
            return new global::Rhino.Geometry.Vector3d(vector2D.X, vector2D.Y, 0);
        }
    }
}