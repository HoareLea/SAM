namespace SAM.Geometry.Rhino
{
    public static partial class Convert
    {
        public static global::Rhino.Geometry.Plane ToRhino(this Spatial.Plane plane)
        {
            return new global::Rhino.Geometry.Plane(ToRhino(plane.Origin), ToRhino(plane.AxisX), ToRhino(plane.AxisY));
        }
    }
}