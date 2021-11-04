namespace SAM.Geometry.Rhino
{
    public static partial class Convert
    {
        public static Spatial.Circle3D ToSAM(this global::Rhino.Geometry.Circle circle)
        {
            return new Spatial.Circle3D(circle.Plane.ToSAM(), circle.Radius);
        }
    }
}