namespace SAM.Geometry.Rhino
{
    public static partial class Convert
    {
        public static Spatial.Vector3D ToSAM(this global::Rhino.Geometry.Vector3d vector3d)
        {
            return new Spatial.Vector3D(vector3d.X, vector3d.Y, vector3d.Z);
        }
    }
}