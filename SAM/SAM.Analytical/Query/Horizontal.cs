namespace SAM.Analytical
{
    public static partial class Query
    {
        public static bool Horizontal(this Panel panel, double tolerance = Core.Tolerance.Distance)
        {
            return Geometry.Spatial.Query.Horizontal(panel?.GetFace3D(), tolerance);
        }

        public static bool Horizontal(this Aperture aperture, double tolerance = Core.Tolerance.Distance)
        {
            return Geometry.Spatial.Query.Horizontal(aperture?.GetFace3D(), tolerance);
        }
    }
}