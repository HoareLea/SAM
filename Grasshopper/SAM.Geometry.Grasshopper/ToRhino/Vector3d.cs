namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static Rhino.Geometry.Vector3d ToRhino(this Spatial.Vector3D vector3D)
        {
            return new Rhino.Geometry.Vector3d(vector3D.X, vector3D.Y, vector3D.Z);
        }
    }
}
