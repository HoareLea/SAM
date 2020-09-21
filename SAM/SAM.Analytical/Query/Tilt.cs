namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double Tilt(this Panel panel)
        {
            if (panel == null)
                return double.NaN;

            return Geometry.Spatial.Query.Tilt(panel.GetFace3D());
        }

        public static double Tilt(this Aperture aperture)
        {
            if (aperture == null)
                return double.NaN;

            return Geometry.Spatial.Query.Tilt(aperture.GetFace3D());
        }
    }
}