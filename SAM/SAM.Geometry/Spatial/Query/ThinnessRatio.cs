namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        /// <summary>
        /// Thiness Ratio of closed Shape. See: <see href="https://tereshenkov.wordpress.com/2014/04/08/fighting-sliver-polygons-in-arcgis-thinness-ratio/">Fighting sliver polygons in ArcGIS – thinness ratio</see>.
        /// Thinness is typically used to define the regularity of an object.
        /// </summary>
        /// <param name="closedPlanar3D"> closed planar 3D shape</param>
        /// <returns>ThinnessRatio</returns>
        public static double ThinnessRatio(this IClosedPlanar3D closedPlanar3D)
        {
            // Return NaN if the shape is null
            if (closedPlanar3D == null)
                return double.NaN;

            // Get the plane of the shape
            Plane plane = closedPlanar3D.GetPlane();

            // Return NaN if the plane is null
            if (plane == null)
                return double.NaN;

            // If the shape is a Face, calculate the Thinness Ratio using the external edge
            if (closedPlanar3D is Face)
                return Planar.Query.ThinnessRatio(((Face)closedPlanar3D).ExternalEdge2D);

            // Otherwise, convert the shape to a planar representation and calculate the Thinness Ratio
            return Planar.Query.ThinnessRatio(plane.Convert(closedPlanar3D));
        }
    }
}