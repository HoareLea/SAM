namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static Rhino.Geometry.Plane ToRhino(this Spatial.Plane plane)
        {
            return new Rhino.Geometry.Plane(ToRhino(plane.Origin), ToRhino(plane.AxisX), ToRhino(plane.AxisY));
        }
    }
}