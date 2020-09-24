namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// Tilit of the Panel mesured in degrees
        /// </summary>
        /// <param name="panel">SAM Analytical Panel</param>
        /// <returns>Tilt in degrees</returns>
        public static double Tilt(this Panel panel)
        {
            if (panel == null)
                return double.NaN;

            return Geometry.Spatial.Query.Tilt(panel.GetFace3D());
        }

        /// <summary>
        /// Tilit of the Aperture mesured in degrees
        /// </summary>
        /// <param name="aperture">SAM Analytical Aperture</param>
        /// <returns>Tilt in degrees</returns>
        public static double Tilt(this Aperture aperture)
        {
            if (aperture == null)
                return double.NaN;

            return Geometry.Spatial.Query.Tilt(aperture.GetFace3D());
        }
    }
}