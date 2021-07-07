namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        /// <summary>
        /// Angle between normal taken from given geometry and XY Plane mesured in degrees 
        /// </summary>
        /// <param name="closedPlanar3D">Closed Planar 3D Geometry</param>
        /// <returns>Tilt in degrees</returns>
        public static double Tilt(this IClosedPlanar3D closedPlanar3D)
        {
            if (closedPlanar3D == null)
                return double.NaN;

            Vector3D normal = closedPlanar3D.GetPlane()?.Normal;
            if (normal == null)
                return double.NaN;

            return Tilt(normal);
        }

        /// <summary>
        /// Angle between normal and XY Plane mesured in degrees 
        /// </summary>
        /// <param name="normal">Normal Vector 3D</param>
        /// <returns>Tilt in degrees</returns>
        public static double Tilt(this Vector3D normal)
        {
            if (normal == null)
                return double.NaN;
            
            return normal.Angle(Plane.WorldXY.Normal) * (180 / System.Math.PI);
        }
    }
}