namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        /// <summary>
        /// Thiness Ratio of closed Shape see: <see cref="https://tereshenkov.wordpress.com/2014/04/08/fighting-sliver-polygons-in-arcgis-thinness-ratio/)"/>. 
        /// Thinness is typically used to define the regularity of an object
        /// </summary>
        /// <param name="closedPlanar3D"> closed planar 3D shape</param>
        /// <returns>ThinnessRatio</returns>
        public static double ThinnessRatio(this IClosedPlanar3D closedPlanar3D)
        {
            if (closedPlanar3D == null)
                return double.NaN;

            Plane plane = closedPlanar3D.GetPlane();
            if (plane == null)
                return double.NaN;

            if (closedPlanar3D is Face)
                return Planar.Query.ThinnessRatio(((Face)closedPlanar3D).ExternalEdge);

            return Planar.Query.ThinnessRatio(plane.Convert(closedPlanar3D));
        }
    }
}